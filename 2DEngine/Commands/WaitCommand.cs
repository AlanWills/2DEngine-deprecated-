namespace _2DEngine
{
    /// <summary>
    /// A command used to pause the game for a certain amount of time
    /// </summary>
    public class WaitCommand : Command
    {
        #region Properties and Fields

        /// <summary>
        /// The total amount which we wish to pause the game for.
        /// </summary>
        private float Delay { get; set; }

        private float currentDelay = 0;

        #endregion

        public WaitCommand(float delay)
        {
            Delay = delay;
        }

        #region Virtual Functions

        /// <summary>
        /// Pauses the ParentScreen so that it cannot handle input or update
        /// </summary>
        public override void Begin()
        {
            base.Begin();

            ParentScreen.ShouldHandleInput = false;
            ParentScreen.ShouldUpdate = false;
        }

        /// <summary>
        /// Updates the current amount of time we have spent waiting
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public override void Update(float elapsedGameTime)
        {
            base.Update(elapsedGameTime);

            currentDelay += elapsedGameTime;

            if (currentDelay >= Delay)
            {
                Die();
            }
        }

        /// <summary>
        /// Restarts the game update and input handling
        /// </summary>
        public override void Die()
        {
            base.Die();

            ParentScreen.ShouldHandleInput = true;
            ParentScreen.ShouldUpdate = true;
        }

        #endregion
    }
}
