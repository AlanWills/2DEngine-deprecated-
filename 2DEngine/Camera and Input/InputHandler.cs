using Microsoft.Xna.Framework.Input;

namespace _2DEngine
{
    /// <summary>
    /// A static class which uses our input map and our current platform to query current input status
    /// </summary>
    public static class InputMap
    {
        #region Defaults
        
        // Camera
        public static Keys DefaultZoomIn = Keys.Add;
        public static Keys DefaultZoomOut = Keys.Subtract;
        public static Keys DefaultPanLeft = Keys.Left;
        public static Keys DefaultPanRight = Keys.Right;
        public static Keys DefaultPanUp = Keys.Up;
        public static Keys DefaultPanDown = Keys.Down;

        // Menu Navigation
        public static Keys DefaultBackToPreviousScreen = Keys.Escape;

        #endregion

        #region Properties and Fields

        // Movement
        public static Keys MoveLeft = Keys.A;
        public static Keys MoveRight = Keys.D;
        public static Keys Run = Keys.LeftShift;

        // Camera
        public static Keys ZoomIn = Keys.Add;
        public static Keys ZoomOut = Keys.Subtract;
        public static Keys PanLeft = Keys.Left;
        public static Keys PanRight = Keys.Right;
        public static Keys PanUp = Keys.Up;
        public static Keys PanDown = Keys.Down;

        // Menu Navigation
        public static Keys BackToPreviousScreen = Keys.Escape;

        #endregion
    }
}
