namespace _2DEngine
{
    public delegate bool ConditionHandler();

    /// <summary>
    /// A command which will run until a condition is fulfilled.
    /// </summary>
    public class WaitForConditionCommand : Command
    {
        #region Properties and Fields

        /// <summary>
        /// The condition we will wait to be fulfilled during this command.
        /// Return true to mark the condition as fulfilled and kill this command.
        /// </summary>
        public event ConditionHandler Condition;

        #endregion

        public WaitForConditionCommand(ConditionHandler condition, bool shouldPauseGame = false, float lifeTime = float.MaxValue) :
            base(shouldPauseGame, lifeTime)
        {
            Condition += condition;
        }

        #region Virtual Functions

        /// <summary>
        /// Checks our condition and if it returns true 
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public override void Update(float elapsedGameTime)
        {
            base.Update(elapsedGameTime);

            if (Condition())
            {
                // If our condition is fulfilled we kill this command.
                Die();
            }
        }

        #endregion
    }
}
