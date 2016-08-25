using Microsoft.Xna.Framework;

namespace _2DEngine
{
    /// <summary>
    /// A command which is bound to an object and will kill the object as soon as we click any mouse button.
    /// </summary>
    public class ClickDismissCommand : Command
    {
        #region Properties and Fields

        /// <summary>
        /// A reference to the object that we will kill when we click the mouse
        /// </summary>
        private BaseObject BaseObject { get; set; }

        #endregion

        public ClickDismissCommand(BaseObject baseObject)
        {
            BaseObject = baseObject;
        }

        #region Virtual Functions

        /// <summary>
        /// If any mouse button has been clicked, we immediately kill the object we are bound to and end this command.
        /// We will both then get cleaned up by our manager.
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        /// <param name="mousePosition"></param>
        public override void HandleInput(float elapsedGameTime, Vector2 mousePosition)
        {
            base.HandleInput(elapsedGameTime, mousePosition);

            // Check to see if we have clicked a mouse button
            if (GameMouse.Instance.IsClicked(MouseButton.kLeftButton) ||
                GameMouse.Instance.IsClicked(MouseButton.kMiddleButton) ||
                GameMouse.Instance.IsClicked(MouseButton.kRightButton))
            {
                // Kill ourselves and the object we are bound to if we have clicked it
                BaseObject.Die();
                Die();
            }
        }

        #endregion
    }
}
