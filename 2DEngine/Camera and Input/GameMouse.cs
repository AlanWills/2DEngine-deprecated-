using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace _2DEngine
{
    /// <summary>
    /// An enum for the mouse buttons
    /// </summary>
    public enum MouseButton
    {
        kLeftButton,
        kMiddleButton,
        kRightButton,
    }

    /// <summary>
    /// A singleton class to handle the mouse state
    /// </summary>
    public class GameMouse : UIObject
    {
        #region Properties and Fields

        /// <summary>
        /// Uses the camera to calculate the in game world position of the mouse
        /// </summary>
        public Vector2 InGameWorldPosition
        {
            get
            {
                return Camera.ScreenToGameCoords(WorldPosition);
            }
        }

        /// <summary>
        /// We wish the origin of the texture to be the top left, so that the click position is the tip of the cursor
        /// </summary>
        public override Vector2 TextureCentre { get { return Vector2.Zero; } }

        /// <summary>
        /// A timer to prevent multiple clicks happening in a short space of time
        /// </summary>
        private float ClickDelayTimer { get; set; }

        /// <summary>
        /// The current state of the mouse - query it to obtain position and mouse button states
        /// </summary>
        private MouseState CurrentMouseState { get; set; }

        /// <summary>
        /// The previous state of the mouse - query this and the current state to obtain mouse button click states
        /// </summary>
        private MouseState PreviousMouseState { get; set; }

        /// <summary>
        /// A bool which can be used to disable all further checks this frame.  
        /// Any call to IsClicked, IsPressed, IsDragged or GetDragDelta will return false or Vector2.Zero until the next frame begins.
        /// </summary>
        public bool IsFlushed { get; set; }

        /// <summary>
        /// When we update colliders we wish to cast a ray into our scene.
        /// The first object we hit will have it's collider bools updated, the rest will not.
        /// </summary>
        public bool ReadyForRayIntersection { get; set; }

        /// <summary>
        /// The amount the mouse wheel has scrolled in the latest frame.
        /// Scrolling mouse wheel AWAY from you will result in positive delta, and towards you will result in negative delta.
        /// </summary>
        public int MouseWheelScrollDelta { get; private set; }

        /// <summary>
        /// Returns whether the mouse wheel has scrolled in the latest frame
        /// </summary>
        public bool HasMouseWheelScrolled { get { return MouseWheelScrollDelta != 0; } }

        /// <summary>
        /// A flag to set whether this mouse should snap to multiples of a position or be completely free moving.
        /// Can only be set via the function SetSnapping() since it requires an increment
        /// </summary>
        public bool Snapping { get; private set; }

        /// <summary>
        /// The increments we should snap to.
        /// </summary>
        private Vector2 Increment { get; set; }

        /// <summary>
        /// The single static instance of this class.
        /// </summary>
        private static GameMouse instance;
        public static GameMouse Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameMouse();
                }

                return instance;
            }
        }

        #endregion

        /// <summary>
        /// The constructor is private because we wish to have one single static instance of this class
        /// </summary>
        private GameMouse() :
            base(Vector2.Zero, AssetManager.MouseTextureAsset)
        {

        }

        #region Virtual Functions

        /// <summary>
        /// Loads the mouse texture directly from Content.
        /// </summary>
        public override void LoadContent()
        {
            CheckShouldLoad();

            Texture = AssetManager.GetSprite(AssetManager.MouseTextureAsset);

            base.LoadContent();
        }

        /// <summary>
        /// Updates the mouse position and the mouse wheel scroll delta
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public override void Update(float elapsedGameTime)
        {
            base.Update(elapsedGameTime);

            // Reset our ray collision bool
            ReadyForRayIntersection = true;

            PreviousMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();

            LocalPosition = new Vector2(CurrentMouseState.X, CurrentMouseState.Y);

            if (Snapping)
            {
                LocalPosition = Increment * new Vector2((int)(CurrentMouseState.X / Increment.X), (int)(CurrentMouseState.Y / Increment.Y));
            }

            MouseWheelScrollDelta = CurrentMouseState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue;
        }

        /// <summary>
        /// Always draw the mouse on top of it's children
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            Children.Draw(spriteBatch);

            DebugUtils.AssertNotNull(Texture);
            spriteBatch.Draw(
                Texture,
                WorldPosition,
                null,
                SourceRectangle,
                TextureCentre,
                WorldRotation,
                new Vector2(1, 1),
                Colour * Opacity,
                SpriteEffect,
                0);
        }

        #endregion

        #region Functions for Querying Mouse Button States

        /// <summary>
        /// Determines whether the inputted button was released this frame and pressed the previous frame.
        /// This gives the effect of clicking, except we return true when the button is released so that
        /// any effects that might take place of of this only do so when the mouse button is released rather than pressed.
        /// </summary>
        /// <param name="mouseButton">The mouse button we wish to query</param>
        /// <returns>Returns true if the mouse button was pressed in the previous frame and released this frame</returns>
        public bool IsClicked(MouseButton mouseButton)
        {
            // If we are flushed this frame return false
            if (IsFlushed) { return false; }

            switch (mouseButton)
            {
                case MouseButton.kLeftButton:
                    return CurrentMouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed;

                case MouseButton.kMiddleButton:
                    return CurrentMouseState.MiddleButton == ButtonState.Released && PreviousMouseState.MiddleButton == ButtonState.Pressed;

                case MouseButton.kRightButton:
                    return CurrentMouseState.RightButton == ButtonState.Released && PreviousMouseState.RightButton == ButtonState.Pressed;

                default:
                    return false;

            }
        }

        /// <summary>
        /// Determines whether the inputted mouse button is pressed down this frame
        /// </summary>
        /// <param name="mouseButton">The mouse button we wish to query</param>
        /// <returns>Returns true if the mouse button was pressed down this frame</returns>
        public bool IsDown(MouseButton mouseButton)
        {
            // If we are flushed this frame, return false
            if (IsFlushed) { return false; }

            switch (mouseButton)
            {
                case MouseButton.kLeftButton:
                    return CurrentMouseState.LeftButton == ButtonState.Pressed;

                case MouseButton.kMiddleButton:
                    return CurrentMouseState.MiddleButton == ButtonState.Pressed;

                case MouseButton.kRightButton:
                    return CurrentMouseState.RightButton == ButtonState.Pressed;

                default:
                    return false;

            }
        }

        /// <summary>
        /// Returns whether the mouse has been dragged with the inputted button down
        /// </summary>
        /// <param name="mouseButton">The mouse button we wish to query</param>
        /// <returns>Returns true if the inputted button is down and we have moved the mouse since last frame</returns>
        public bool IsDragged(MouseButton mouseButton)
        {
            // If we are flushed this frame, return false
            if (IsFlushed) { return false; }

            return IsDown(mouseButton) && GetDragDelta() != Vector2.Zero;
        }

        /// <summary>
        /// Returns the amount that we have moved the mouse since last frame
        /// </summary>
        /// <returns>The amount that we have moved the mouse since last frame</returns>
        public Vector2 GetDragDelta()
        {
            // If we are flushed this frame return Vector2.Zero
            if (IsFlushed) { return Vector2.Zero; }

            return new Vector2(CurrentMouseState.X - PreviousMouseState.X, CurrentMouseState.Y - PreviousMouseState.Y);
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Sets the mouse to be free or snapping to an increment based on the bool input.
        /// </summary>
        /// <param name="snapping">Set true to have mouse snapping.</param>
        /// <param name="increment">If snapping is true, the amount we should increment when snapping.</param>
        public void SetSnapping(bool snapping, Vector2 increment)
        {
            Snapping = snapping;
            Increment = increment;
        }

        #endregion
    }
}
