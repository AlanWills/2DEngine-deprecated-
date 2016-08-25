using _2DEngineData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace _2DEngine
{
    public enum CharacterBehaviours
    {
        kIdle,
        kDeath,

        kNumBehaviours
    }

    public delegate void BehaviourChangeHandler(uint newState);

    public class Character : GameObject, IDamageable
    {
        #region Properties and Fields

        /// <summary>
        /// An event handler used with the StateMachine.
        /// Call EmitBehaviourChanged() when a character changes behaviour to update the State Machine for that character.
        /// </summary>
        public event BehaviourChangeHandler BehaviourChanged;

        /// <summary>
        /// A cached cast of the loaded GameObject data to stop us from having to do lots of casts
        /// </summary>
        protected CharacterData CharacterData { get; set; }

        /// <summary>
        /// A property to indicate the current behaviour that this character is in.
        /// Handles behaviour changing event after the value is set.
        /// </summary>
        private uint currentBehaviour;
        protected uint CurrentBehaviour
        {
            get { return currentBehaviour; }
            set
            {
                currentBehaviour = value;

                // Behaviour Changed should not be null.
                // If it is, it means we have not set up our State Machine to hook into this event.
                DebugUtils.AssertNotNull(BehaviourChanged);
                BehaviourChanged(currentBehaviour);
            }
        }

        /// <summary>
        /// The animations that this character has access to.  Defined in it's xml CharacterData.
        /// </summary>
        protected Dictionary<string, AnimationModule> Animations { get; private set; }

        /// <summary>
        /// This should be set from the Behaviour Enum from any derived class to be the total number of behaviours declared.
        /// Therefore, if we have declared two more in a derived class, this should be set to 3 to count the behaviour we declare in this class.
        /// </summary>
        protected uint NumBehaviours { private get; set; }

        /// <summary>
        /// The state machine for this character.
        /// </summary>
        protected StateMachine StateMachine { get; set; }

        /// <summary>
        /// The health of our character
        /// </summary>
        private float health = 1;
        public float Health { get { return health; } }

        /// <summary>
        /// A flag to indicate whether we are dead without having to change the IsAlive property
        /// </summary>
        private bool dead;
        public bool Dead
        {
            get { return dead; }
            set { dead = value; }
        }

        #endregion

        public Character(Vector2 localPosition, string dataAsset) :
            base(localPosition, dataAsset)
        {
            Animations = new Dictionary<string, AnimationModule>();
            NumBehaviours = (uint)CharacterBehaviours.kNumBehaviours;

            AddPhysicsBody();
            PhysicsBody.OnDirectionChange += OnDirectionChange;
        }

        #region Virtual Functions

        /// <summary>
        /// Sets up all the states and transitions.
        /// </summary>
        protected virtual void SetUpAnimations()
        {
            DebugUtils.AssertNotNull(Data);
            CharacterData data = Data.As<CharacterData>();
            DebugUtils.AssertNotNull(data);

            // Checks that we have declared at most the same number of enum behaviours as we have animations.
            // If NumAnimations is larger than data.AnimationInfo.Count, it means we have not loaded enough animations for all our behaviours.
            Debug.Assert(data.AnimationInfo.Count >= NumBehaviours);

            StateMachine = new StateMachine(this, NumBehaviours);

            foreach (string animationDataAsset in data.AnimationInfo)
            {
                AnimationModule animation = new AnimationModule();
                animation.LoadContent();

                Debug.Assert(!Animations.ContainsKey(animationDataAsset));
                Animations.Add(animationDataAsset, animation);
            }

            CreateState("Idle", (uint)CharacterBehaviours.kIdle);
            CreateState("Death", (uint)CharacterBehaviours.kDeath);

            StateMachine.StartingState = (uint)CharacterBehaviours.kIdle;
        }

        /// <summary>
        /// Loads all the states.
        /// </summary>
        public override void LoadContent()
        {
            CheckShouldLoad();

            // Do not need to rely on loading character data - this will happen when needed.
            SetUpAnimations();
            StateMachine.LoadContent();

            base.LoadContent();

            DebugUtils.AssertNotNull(CharacterData);
        }

        /// <summary>
        /// Starts the state machine playing the ActiveState animation
        /// </summary>
        public override void Initialise()
        {
            CheckShouldInitialise();

            StateMachine.ActiveState.Animation.IsPlaying = true;

            base.Initialise();
        }

        /// <summary>
        /// Because this object uses an animation, we have to alter the position of the collider based on the size of a single frame
        /// </summary>
        /// <param name="position"></param>
        /// <param name="size"></param>
        public override void UpdateCollider(ref Vector2 position, ref Vector2 size)
        {
            position = WorldPosition + StateMachine.ActiveState.Animation.ColliderCentreOffset;
            size = StateMachine.ActiveState.Animation.ColliderDimensions;
        }

        /// <summary>
        /// Checks the current behaviour to see if it can move to a new behaviour.
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        /// <param name="mousePosition"></param>
        public override void HandleInput(float elapsedGameTime, Vector2 mousePosition)
        {
            base.HandleInput(elapsedGameTime, mousePosition);

            UpdateBehaviour();
        }

        /// <summary>
        /// Runs through all the behaviours in our Behaviour enum and checks their appropriate function for whether the behaviour state can change.
        /// Can be overridden to check inherited class' new behaviours.
        /// </summary>
        protected virtual void UpdateBehaviour()
        {
            switch (CurrentBehaviour)
            {
                case (uint)CharacterBehaviours.kIdle:
                    IdleState();
                    break;

                case (uint)CharacterBehaviours.kDeath:
                    DeathState();
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Updates the state machine.
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public override void Update(float elapsedGameTime)
        {
            base.Update(elapsedGameTime);

            StateMachine.Update(elapsedGameTime);
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// A utility function which performs some validity checks before it creates a state for our state machine.
        /// It then adds it to the state machine.  This must be called in the same order as the enum IDs are declared.
        /// </summary>
        /// <param name="name">The name of the asset in our Animations dictionary - will be the name of the XML, e.g. "Walk".</param>
        /// <param name="id">The desired ID for this state.</param>
        protected void CreateState(string name, uint id)
        {
            DebugUtils.AssertNotNull(Animations[name]);

            StateMachine.CreateState(id, Animations[name]);
        }

        /// <summary>
        /// Obtains a state from the state machine.  Checks for valid ID and null state.
        /// </summary>
        /// <param name="id">The ID of the state we wish to obtain.</param>
        /// <returns>The state we requested from our state machine.</returns>
        public State GetState(uint id)
        {
            Debug.Assert(id < NumBehaviours);
            return StateMachine.GetState(id);
        }

        /// <summary>
        /// A function for when our direction changes.  Used for flipping sprites and for re-fixing animations.
        /// </summary>
        /// <param name="newDirection"></param>
        public void OnDirectionChange(int oldDirection, int newDirection)
        {
            DebugUtils.AssertNotNull(StateMachine);
            DebugUtils.AssertNotNull(PhysicsBody);

            SpriteEffect = newDirection == PhysicsConstants.LeftDirection ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            LocalPosition += new Vector2(2 * StateMachine.ActiveState.Animation.AnimationFixup.X * newDirection, 0);
        }

        /// <summary>
        /// Damages the character
        /// </summary>
        /// <param name="damage"></param>
        public void Damage(float damage, BaseObject objectDoingTheDamage)
        {
            // Add transition to hurt animation and possibly death animation here
            health -= damage;
        }

        /// <summary>
        /// Checks our character's current health
        /// </summary>
        public void HandleCurrentHealth()
        {
            // Die when animation done
        }

        #endregion

        #region Character Behaviour Changing Functions

        /// <summary>
        /// The function that will be called in the kIdle state to see if we can transition to a new behaviour state.
        /// </summary>
        protected virtual void IdleState()
        {
            // Don't check is alive as this is still true so that we can play the death animation.
            if (Health <= 0)
            {
                CurrentBehaviour = (uint)CharacterBehaviours.kDeath;
            }
        }

        /// <summary>
        /// The function that will be called in the kDeath state to see if we can transition to a new behaviour state.
        /// </summary>
        protected virtual void DeathState()
        {
            
        }

        #endregion
    }
}