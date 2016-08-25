using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// A class that represents a connected set of animation states.
    /// Also contains global states which can be accessed to from anywhere (a good example of this, is Death).
    /// </summary>
    public class StateMachine
    {
        #region Properties and Fields

        /// <summary>
        /// A list of all the current states in our state machine.
        /// </summary>
        private State[] States { get; set; }

        /// <summary>
        /// A list of all the current global states in our state machine.  Global states can be accessed from any state, i.e. Death.
        /// These are represented as transitions, because they are really the thing we are interested in.
        /// </summary>
        private List<State> GlobalStates { get; set; }

        /// <summary>
        /// The current state we are in.  The transitions from the active state and global transitions will be checked every frame.
        /// This is for optimization, as there is no need to check all off the states if we cannot reach them.
        /// </summary>
        public State ActiveState { get; private set; }

        /// <summary>
        /// The starting state for the state machine obtained from the Character's data xml file.
        /// </summary>
        public uint StartingState { private get; set; }

        /// <summary>
        /// A flag to determine whether we should do a full load.
        /// </summary>
        private bool ShouldLoad { get; set; }

        /// <summary>
        /// A reference to the Character that this state machine is animating.
        /// </summary>
        private Character Character { get; set; }

        // A field just used for validating addition of states in the correct order.
        private uint currentAddedStates = 0;

        #endregion

        public StateMachine(Character character, uint numAnimations)
        {
            Character = character;
            Character.BehaviourChanged += HandleBehaviourChange;

            States = new State[numAnimations];
            GlobalStates = new List<State>();
            ShouldLoad = true;
        }

        #region State Machine Update Functions

        /// <summary>
        /// Calls LoadContent on every State in this state machine.
        /// </summary>
        public void LoadContent()
        {
            if (!ShouldLoad) { return; }

            CheckStateValid(StartingState);
            ActiveState = States[StartingState];
            
            ShouldLoad = false;
        }

        /// <summary>
        /// Checks the transitions from our ActiveState and sets the new state if there was a change.
        /// Then, checks the global transitions and if there was a successful transition, sets the new state to be this state and returns.
        /// I.e. normal states can overridden on successful transition, but global states cannot.
        /// </summary>
        public void Update(float elapsedGameTime)
        {
            DebugUtils.AssertNotNull(ActiveState);

            ActiveState.Update(elapsedGameTime);
        }

        #endregion

        #region State Utility Functions

        /// <summary>
        /// Adds a state to this state machine.  This function takes care of global states and ID checks.
        /// Should not really be used - instead use Character function 'CreateState' - it is much nicer and tidier.
        /// </summary>
        /// <param name="state"></param>
        public void CreateState(uint id, AnimationModule animation)
        {
            // This is a check to make sure that we are adding the states in the order they are declared in the enum.
            // If this doesn't occur, the states will be mixed up and all hell will break loose.
            Debug.Assert(id == currentAddedStates);

            State state = new State(id, animation);

            States[currentAddedStates] = state;
            currentAddedStates++;

            // If our state is global, add it to our list
            if (state.IsGlobal)
            {
                GlobalStates.Add(state);
            }
        }

        /// <summary>
        /// Gets a state from the state machine and performs checks for null and valid ID.
        /// </summary>
        /// <param name="stateID">The ID of the state we wish to obtain.</param>
        /// <returns>The state we requested.  Guaranteed to not be null in debug.</returns>
        public State GetState(uint stateID)
        {
            CheckStateValid(stateID);

            return States[stateID];
        }

        /// <summary>
        /// Checks the current ActiveState and returns true if it has finished playing.
        /// Can only be checked against non-continual animations.
        /// </summary>
        /// <returns></returns>
        public bool CurrentAnimationFinished()
        {
            DebugUtils.AssertNotNull(ActiveState);
            Debug.Assert(!ActiveState.Animation.Continual);

            return ActiveState.Animation.Finished;
        }

        /// <summary>
        /// Only available in Debug.
        /// Debug.Asserts state ID and whether the state is null. 
        /// </summary>
        /// <param name="stateID"></param>
        [Conditional("DEBUG")]
        private void CheckStateValid(uint stateID)
        {
            Debug.Assert(stateID < States.Length);
            DebugUtils.AssertNotNull(States[stateID]);
        }

        /// <summary>
        /// Only available in Debug.
        /// Debug.Asserts ActiveState stateID with inputed state
        /// </summary>
        /// <param name="stateID"></param>
        [Conditional("DEBUG")]
        public void CheckActiveStateHasID(uint stateID)
        {
            Debug.Assert(ActiveState.StateID == stateID);
        }

        #endregion

        #region Transition Utility Functions

        /// <summary>
        /// Adds a transition between two states.  Useful because it means you do not need the state itself.
        /// Checks for valid IDs and existing transitions.  If a transition exists, it will not create it.
        /// </summary>
        /// <param name="sourceStateID">The ID of the source state.</param>
        /// <param name="destStateID">The ID of the destination state.</param>
        /// <param name="reciprocal">A flag to indicate whether we should create a transition on the state with destinationStateID back to this one.  True by default.</param>
        public void AddTransition(uint sourceStateID, uint destStateID, bool reciprocal = true)
        {
            CheckStateValid(sourceStateID);
            CheckStateValid(destStateID);

            States[sourceStateID].AddTransition(destStateID);

            if (reciprocal)
            {
                States[destStateID].AddTransition(sourceStateID);
            }
        }

        #endregion

        #region Behaviour Change Handling

        /// <summary>
        /// Checks the current active state's transitions against the new behaviour state.
        /// Performs no checking if the new state and current ActiveState have the same ID.
        /// </summary>
        /// <param name="newBehaviourState"></param>
        private void HandleBehaviourChange(uint newBehaviourState)
        {
            DebugUtils.AssertNotNull(ActiveState);

            // If we have not changed state then just return.
            if (newBehaviourState == ActiveState.StateID) { return; }

            // Check the transitions of the current active state
            if (ActiveState.CheckTransitions(newBehaviourState))
            {
                SetNewActiveState(newBehaviourState);
            }

            // Check the global states
            foreach (State state in GlobalStates)
            {
                if (state != ActiveState && state.StateID == newBehaviourState)
                {
                    SetNewActiveState(newBehaviourState);

                    break;
                }
            }

            // Check that we have indeed transitioned to our new state
            Debug.Assert(newBehaviourState == ActiveState.StateID);

            // The new state we have moved to should not be playing already
            Debug.Assert(ActiveState.Animation.IsPlaying == false);

            ActiveState.Animation.IsPlaying = true;
        }

        /// <summary>
        /// A helper function which performs clean up and checks when transitioning to a new state.
        /// </summary>
        /// <param name="newBehaviourState"></param>
        private void SetNewActiveState(uint newBehaviourState)
        {
            // Check to make sure we have transitioned to a state that exists in our state machine.
            CheckStateValid(newBehaviourState);

            // Reset the old states's animation
            ActiveState.Animation.Reset();

            // Remove old animation fixup
            Character.LocalPosition -= new Vector2(ActiveState.Animation.AnimationFixup.X * Character.PhysicsBody.Direction, ActiveState.Animation.AnimationFixup.Y);

            // Set the new state
            ActiveState = States[newBehaviourState];

            // Apply new animation fixup
            Character.LocalPosition += new Vector2(ActiveState.Animation.AnimationFixup.X * Character.PhysicsBody.Direction, ActiveState.Animation.AnimationFixup.Y);
        }

        #endregion
    }
}
