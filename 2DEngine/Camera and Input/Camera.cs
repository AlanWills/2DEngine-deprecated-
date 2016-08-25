using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// An enum to describe the different behaviours that our camera can perform
    /// </summary>
    public enum CameraMode
    {
        kFree,      // The camera will move anywhere based on certain input - mouse at edge of screen, or keyboard
        kFixed,     // The camera will not move at all - useful for menu screens
        kFollow,    // The camera will follow an object - this requires the object to follow to be passed in
    }

    /// <summary>
    /// A static class used as an in game camera.
    /// This will also allow us to not render objects if they are not contained within the camera viewport
    /// </summary>
    public static class Camera
    {
        #region Properties and Fields

        /// <summary>
        /// A private variable used to determine what behaviour the camera should perform.
        /// This should remain private and instead, to change the camera mode, the appropriate functions 'SetFree', 'SetFixed', 'SetFollow'
        /// should be called.  This is because extra parameters are required for each mode.
        /// </summary>
        private static CameraMode CameraMode { get; set; }

        /// <summary>
        /// The position of the Camera - corresponds to where the top left of the screen is.  Again we should not be able to set this outside of this class.
        /// Ways to change the camera position are determined by the CameraMode, and also by a parameter passed in when changing mode ONLY.
        /// </summary>
        public static Vector2 Position { get; private set; }

        /// <summary>
        /// A value to determine how fast the camera should move in Free mode.  This should be freely available for alteration.
        /// </summary>
        public static float PanSpeed { get; set; }

        /// <summary>
        /// A value to determine how much we increase or decrease the camera zoom by when pressing + or -.
        /// </summary>
        public static float ZoomIncrement { get; set; }

        /// <summary>
        /// A value only to be altered and seen by this class, used to determine the zoom of the camera.
        /// This value can be changed in Free or Follow mode by keyboard input ONLY.
        /// </summary>
        private static float Zoom { get; set; }

        /// <summary>
        /// This represents the current transformation of the camera calculated from the position and zoom.
        /// It is used as a parameter to SpriteBatch.Begin() to give the impression of a camera when drawing objects
        /// </summary>
        public static Matrix TransformationMatrix
        {
            get
            {
                Debug.Assert(IsInitialised);

                // This could be done with usual operators - *, + etc., but this is an optimisation
                return Matrix.Multiply(Matrix.CreateTranslation(Position.X, Position.Y, 0), Matrix.CreateScale(Zoom));
            }
        }

        /// <summary>
        /// Corresponds to the window of our game - will have coordinates (0, 0, screen width, screen height).
        /// This will be used in determining whether an object is visible or not.
        /// It will need to be set up once, or when our screen size changes.
        /// </summary>
        public static Rectangle ViewportRectangleScreenSpace { get; private set; }

        /// <summary>
        /// Corresponds to our viewport rectangle but in game space - means we can check objects in the game world to see whether we should draw them.
        /// </summary>
        public static Rectangle ViewportRectangleGameSpace { get; private set; }

        /// <summary>
        /// A bool just to track whether we have called Initialise.  This will be checked in Update and HandleInput
        /// </summary>
        private static bool IsInitialised { get; set; }

        #endregion

        /// <summary>
        /// Initialises the Camera properties
        /// </summary>
        public static void Initialise()
        {
            // Check to see whether we have already initialised the camera
            if (IsInitialised) { return; }

            CameraMode = CameraMode.kFixed;
            PanSpeed = 500;
            Zoom = 1;
            ViewportRectangleScreenSpace = new Rectangle(0, 0, ScreenManager.Instance.Viewport.Width, ScreenManager.Instance.Viewport.Height);

            // This can be improved upon by using zoom to affect the length
            ViewportRectangleGameSpace = new Rectangle((int)-Position.X, (int)-Position.Y, ViewportRectangleScreenSpace.Width, ViewportRectangleScreenSpace.Height);

            IsInitialised = true;
        }

        #region Camera Position and Zoom Update Functions

        /// <summary>
        /// Updates the camera position based on what mode we are in - should be called every frame
        /// </summary>
        /// <param name="elapsedSeconds"></param>
        public static void Update(float elapsedSeconds)
        {
            Debug.Assert(IsInitialised);

            // This can be improved upon by using zoom to affect the length
            ViewportRectangleGameSpace = new Rectangle((int)-Position.X, (int)-Position.Y, ViewportRectangleScreenSpace.Width, ViewportRectangleScreenSpace.Height);

            // Only the follow mode requires no user input to update
            if (CameraMode != CameraMode.kFollow) { return; }

            // TODO - Follow mode not supported yet
        }

        /// <summary>
        /// Updates the camera position based on what mode we are in and any appropriate user input - should be called every frame
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public static void HandleInput(float elapsedGameTime)
        {
            Debug.Assert(IsInitialised);

            // If we are in fixed camera mode, we should not update anything
            if (CameraMode == CameraMode.kFixed) { return; }

            // Handles zooming in or out
            if (GameKeyboard.Instance.IsKeyPressed(InputMap.ZoomIn))
            {
                Zoom += ZoomIncrement;
            }
            else if (GameKeyboard.Instance.IsKeyPressed(InputMap.ZoomOut))
            {
                Zoom -= ZoomIncrement;
                Zoom = MathHelper.Clamp(Zoom, ZoomIncrement, float.MaxValue);
            }

            // We will be updating the position using keyboard input from now on - this only applies to Free mode
            if (CameraMode == CameraMode.kFollow) { return; }

            Vector2 delta = Vector2.Zero;

            // Handles panning in all four directions
            if (GameKeyboard.Instance.IsKeyDown(InputMap.PanLeft))
            {
                delta += new Vector2(1, 0);
            }

            if (GameKeyboard.Instance.IsKeyDown(InputMap.PanRight))
            {
                delta += new Vector2(-1, 0);
            }

            if (GameKeyboard.Instance.IsKeyDown(InputMap.PanUp))
            {
                delta += new Vector2(0, 1);
            }

            if (GameKeyboard.Instance.IsKeyDown(InputMap.PanDown))
            {
                delta += new Vector2(0, -1);
            }

            if (delta != Vector2.Zero)
            {
                delta.Normalize();
                Position += delta * PanSpeed * elapsedGameTime;
            }
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// A function used to position the camera so that the inputted position is in the centre of the screen.
        /// </summary>
        /// <param name="focusPosition">The position that we wish to have in the centre of the screen</param>
        /// <param name="screenSpace">A flag to indicate whether the focus position is in screen space or game space.  The camera will transform the point if necessary</param>
        public static void FocusOnPosition(Vector2 focusPosition, bool screenSpace)
        {
            Debug.Assert(IsInitialised);

            if (!screenSpace)
            {
                // Transform into screen space if necessary
                focusPosition = GameToScreenCoords(focusPosition);
            }

            /// Since the camera position corresponds to the opposite of what we expect (just look at movement in update function) and the top left is the position, we subtract the focus position from the screen centre
            Position = new Vector2(ViewportRectangleScreenSpace.Width, ViewportRectangleScreenSpace.Height) * 0.5f - focusPosition;
        }

        /// <summary>
        /// A function for converting a position on the screen into game space, using the zoom and position of the camera
        /// </summary>
        /// <param name="screenPosition">The position on the screen between (0, 0) and (screen width, screen height)</param>
        /// <returns>The game space coordinates corresponding to the inputted screen position</returns>
        public static Vector2 ScreenToGameCoords(Vector2 screenPosition)
        {
            Debug.Assert(IsInitialised);

            // This could be done using ordinary mathematical operators - +, / etc. but this is an optimisation
            return Vector2.Divide(Vector2.Add(Position, screenPosition), Zoom);
        }

        /// <summary>
        /// A function for converting a position in game space into screen space, using the zoom and position of the camera
        /// </summary>
        /// <param name="gamePosition">The position in game space.</param>
        /// <returns>The screen space coordinates corresponding to the inputted game position</returns>
        public static Vector2 GameToScreenCoords(Vector2 gamePosition)
        {
            Debug.Assert(IsInitialised);

            // This could be done using ordinary mathematical operators - +, / etc. but this is an optimisation
            return Vector2.Subtract(Vector2.Multiply(gamePosition, Zoom), Position);
        }

        /// <summary>
        /// Checks each object in the inputted ObjectManager to determine whether it should be drawn this frame.
        /// </summary>
        /// <param name="objectManager"></param>
        /// <param name="screenSpace"></param>
        public static void CheckVisibility<T>(ObjectManager<T> objectManager, bool screenSpace) where T : BaseObject
        {
            Rectangle rectangleToUse = screenSpace ? ViewportRectangleScreenSpace : ViewportRectangleGameSpace;
            Vector2 centre = rectangleToUse.Center.ToVector2();

            foreach (BaseObject baseObject in objectManager)
            {
                // If our object's visibility is driven by another object we do not need to perform visibility checks
                if (baseObject.ShouldDraw)
                {
                    continue;
                }

                if (baseObject.UsesCollider)
                {
                    DebugUtils.AssertNotNull(baseObject.Collider);
                    baseObject.ShouldDraw = baseObject.Collider.CheckIntersects(rectangleToUse);
                }
                else
                {
                    // Crude circle check, can definitely be improved upon
                    float x = rectangleToUse.Width + baseObject.Size.X;
                    float y = rectangleToUse.Height + baseObject.Size.Y;
                    baseObject.ShouldDraw = (centre - baseObject.WorldPosition).LengthSquared() <= x * x + y * y;
                }
            }
        }

        #endregion

        #region Changing Camera Mode Functions

        /// <summary>
        /// Sets the CameraMode to free
        /// </summary>
        public static void SetFree()
        {
            CameraMode = CameraMode.kFree;
        }

        /// <summary>
        /// Sets the CameraMode to free and the camera's position to the inputted value.
        /// </summary>
        /// <param name="resetPosition">The new value of the camera's position</param>
        public static void SetFree(Vector2 resetPosition)
        {
            SetFree();
            Position = resetPosition;
        }

        /// <summary>
        /// Sets the CameraMode to fixed
        /// </summary>
        public static void SetFixed()
        {
            CameraMode = CameraMode.kFixed;
        }

        /// <summary>
        /// Sets the CameraMode to Fixed and the camera's position to the inputted value
        /// </summary>
        /// <param name="resetPosition">The new value of the camera's position</param>
        public static void SetFixed(Vector2 fixedPosition)
        {
            SetFixed();
            Position = fixedPosition;
        }

        #endregion
    }
}
