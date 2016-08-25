using Microsoft.Xna.Framework;

namespace _2DEngine
{
    /// <summary>
    /// A module which makes an object move towards a certain position with a certain speed
    /// </summary>
    public class MoveToDestinationModule : BaseObjectModule
    {
        #region Properties and Fields

        /// <summary>
        /// The position we are moving the attached object to
        /// </summary>
        private Vector2 Destination { get; set; }

        /// <summary>
        /// The number of game units we should be moving per second - if we are running at 60fps then each frame will be 16.6ms.
        /// Provides a frame invariant but usable speed.
        /// </summary>
        private float GameUnitsPerSecond { get; set; }

        private bool DestinationReached { get; set; }

        #endregion

        public MoveToDestinationModule(Vector2 destination, float gameUnitsPerSecond)
        {
            Destination = destination;
            GameUnitsPerSecond = gameUnitsPerSecond;
        }

        #region Virtual Functions

        public override void Update(float elapsedGameTime)
        {
            base.Update(elapsedGameTime);

            // 0 degrees is pointing up the screen which corresponds to the vector (0, -1)
            //Vector2 diff = Vector2.Transform(-Vector2.UnitY, Matrix.CreateRotationZ(AttachedBaseObject.WorldRotation));

            // TODO rotating to destination too
            Vector2 diff = Destination - AttachedBaseObject.WorldPosition;
            DestinationReached = diff.LengthSquared() < GameUnitsPerSecond * elapsedGameTime;

            if (DestinationReached)
            {
                Die();
                return;
            }

            diff.Normalize();

            AttachedBaseObject.LocalPosition += diff * elapsedGameTime * GameUnitsPerSecond;
        }

        #endregion
    }
}
