namespace _2DEngine
{
    /// <summary>
    /// A module which will automatically kill an object if it has been alive for a certain amount of time
    /// </summary>
    public class LifeTimeModule : Module
    {
        #region Properties and Fields

        /// <summary>
        /// The total lifetime for the attached component
        /// </summary>
        private float LifeTime { get; set; }

        private float currentLifeTimer = 0;

        #endregion

        public LifeTimeModule(float lifeTime)
        {
            LifeTime = lifeTime;
        }

        #region Properties and Fields

        public override void Update(float elapsedGameTime)
        {
            base.Update(elapsedGameTime);

            // The object may have been killed already this update loop so only call die if it is still alive and used up it's lifetime
            currentLifeTimer += elapsedGameTime;
            if (currentLifeTimer > LifeTime && AttachedComponent.IsAlive)
            {
                AttachedComponent.Die();
            }
        }

        #endregion
    }
}
