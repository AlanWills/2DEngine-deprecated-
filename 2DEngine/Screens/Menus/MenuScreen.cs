using Microsoft.Xna.Framework;
using System;

namespace _2DEngine
{
    /// <summary>
    /// A fairly basic class that fixes the camera and sets up transitioning between menus.
    /// </summary>
    public abstract class MenuScreen : BaseScreen
    {
        public MenuScreen(string screenDataAsset) :
            base(screenDataAsset)
        {
            Lights.ShouldDraw = false;
        }

        #region Virtual Functions

        /// <summary>
        /// Set up the camera to be fixed and reset it to it's default position of zero
        /// </summary>
        public override void Initialise()
        {
            base.Initialise();

            Camera.SetFixed(Vector2.Zero);
        }

        /// <summary>
        /// Handle input and check whether we are transitioning to the previous screen if it exists (press escape)
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        /// <param name="mousePosition"></param>
        public override void HandleInput(float elapsedGameTime, Vector2 mousePosition)
        {
            base.HandleInput(elapsedGameTime, mousePosition);

            if (GameKeyboard.Instance.IsKeyPressed(InputMap.BackToPreviousScreen))
            {
                GoToPreviousScreen();
            }
        }

        /// <summary>
        /// An overridable function used to traverse between menu screens.
        /// </summary>
        /// <returns>The type of the previous screen we wish to go back to</returns>
        protected virtual void GoToPreviousScreen() { }

        #endregion
    }
}
