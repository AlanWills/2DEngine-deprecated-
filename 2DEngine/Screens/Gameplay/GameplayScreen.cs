using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace _2DEngine
{
    /// <summary>
    /// Really a base class for more custom gameplay screens
    /// </summary>
    public abstract class GameplayScreen : BaseScreen
    {
        #region Properties and Fields

        /// <summary>
        /// The objects we will check collisions with our against GameObjects and Backgrounds
        /// </summary>
        private List<ICollisionObject> CollisionObjects { get; set; }

        /// <summary>
        /// The dialog that will appear when we press escape.
        /// </summary>
        public InGameEscapeDialog InGameDialog { get; protected set; }

        #endregion

        public GameplayScreen(string screenDataAsset) :
            base(screenDataAsset)
        {
            Lights.ShouldDraw = true;
            CollisionObjects = new List<ICollisionObject>();
        }

        #region Virtual Functions

        /// <summary>
        /// Add, but hide our InGameEscapeDialog for showing options, quitting etc.
        /// </summary>
        public override void LoadContent()
        {
            CheckShouldLoad();

            InGameDialog = AddScreenUIObject(AddInGameEscapeDialog());
            InGameDialog.Hide();

            base.LoadContent();
        }

        /// <summary>
        /// A virtual function for adding a custom in game escape dialog
        /// </summary>
        /// <returns></returns>
        protected abstract InGameEscapeDialog AddInGameEscapeDialog();

        /// <summary>
        /// Updates the Camera to be free moving
        /// </summary>
        public override void Initialise()
        {
            base.Initialise();

            Camera.SetFree(Vector2.Zero);
        }

        /// <summary>
        /// Hides or shows our EscapeDialog if escape is pressed
        /// Handles collisions of CollisionObjects
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        /// <param name="mousePosition"></param>
        public override void HandleInput(float elapsedGameTime, Vector2 mousePosition)
        {
            base.HandleInput(elapsedGameTime, mousePosition);

            if (GameKeyboard.Instance.IsKeyPressed(Keys.Escape))
            {
                if (InGameDialog.ShouldHandleInput)
                {
                    InGameDialog.Hide();
                }
                else
                {
                    // We don't want to show children, because otherwise the options dialog will appear too
                    InGameDialog.Show();
                }
            }

            CollisionObjects.RemoveAll(x => (x is BaseObject) && !(x as BaseObject).IsAlive);

            foreach (ICollisionObject collisionObject in CollisionObjects)
            {
                foreach (UIObject backgroundObject in EnvironmentObjects)
                {
                    if (backgroundObject.UsesCollider && collisionObject.Collider.CheckCollisionWith(backgroundObject.Collider))
                    {
                        DebugUtils.AssertNotNull(collisionObject as ICollisionObject);
                        collisionObject.OnCollisionWithEnvironment(backgroundObject);
                    }
                }

                foreach (ICollisionObject otherCollisionObject in CollisionObjects)
                {
                    DebugUtils.AssertNotNull(otherCollisionObject.Collider);
                    if (collisionObject != otherCollisionObject && collisionObject.Collider.CheckCollisionWith(otherCollisionObject.Collider))
                    {
                        DebugUtils.AssertNotNull(collisionObject as ICollisionObject);
                        collisionObject.OnCollisionWithCollisionObject(otherCollisionObject);
                    }
                }
            }
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// A function used to indicate that a GameObject should check collisions with the BackgroundObjects and other GameObjects
        /// </summary>
        /// <param name="collisionObject"></param>
        protected void AddCollisionObject(ICollisionObject collisionObject)
        {
            //Debug.Assert(collisionObject.UsesCollider);
            //DebugUtils.AssertNotNull(collisionObject.PhysicsBody);

            CollisionObjects.Add(collisionObject);
        }

        #endregion
    }
}
