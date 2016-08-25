namespace _2DEngine
{
    /// <summary>
    /// An interface used for objects which wants to vary their opacity over a period of time
    /// </summary>
    public interface IFlashing
    {
        /// <summary>
        /// A utility bool used to determine whether we should increase or decrease our opacity
        /// </summary>
        bool FlashingOut { get; }

        /// <summary>
        /// A function which changes the opacity based on the elapsed seconds and whether we are flashing in or out
        /// </summary>
        /// <param name="elapsedSeconds"></param>
        void FlashFunction(float elapsedGameTime);
    }
}
