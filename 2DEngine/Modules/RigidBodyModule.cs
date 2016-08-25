using Microsoft.Xna.Framework;

namespace _2DEngine
{
    /// <summary>
    /// A module which applies some basic newtonian laws of physics to an object.
    /// Can optionally take a linear velocity and an angular momentum to adjust the rotation and position of an object using suvat.
    /// </summary>
    public class RigidBodyModule : BaseObjectModule
    {
        #region Properties and Fields

        /// <summary>
        /// The amount that we will alter the rotation of an object per frame.
        /// This value is signed - +ve = clockwise, -ve = anti-clockwise; in the local space of the attached object.
        /// Uses the formula LocalRotation += AngularVelocity * elapsedSeconds.
        /// </summary>
        public float AngularVelocity { get; set; }

        /// <summary>
        /// The amount that we will alter the position of an object per frame.
        /// This vector is signed and in the local space of the attached object.
        /// Therefore, a velocity of (0, 1) will move the object forward by 1 in the direction it is facing, not in the world y coordinate
        /// Uses the formula LocalPosition += LinearVelocity * elapsedSeconds.
        /// </summary>
        public Vector2 LinearVelocity { get; set; }

        #endregion

        #region Constructors

        public RigidBodyModule() :
            this(Vector2.Zero, 0)
        {

        }

        public RigidBodyModule(float angularVelocity) :
            this(Vector2.Zero, angularVelocity)
        {

        }

        public RigidBodyModule(Vector2 linearVelocity) :
            this(linearVelocity, 0)
        {

        }

        public RigidBodyModule(Vector2 linearVelocity, float angularVelocity) :
            base()
        {
            // We do this because the y axis points downwards in world space, but upwards in local space.  Doing this means that a velocity of (0, 1) moves the object in the direction it's facing
            LinearVelocity = linearVelocity * new Vector2(1, -1);
            AngularVelocity = angularVelocity;
        }

        #endregion

        #region Virtual Functions

        /// <summary>
        /// Updates the rotation of our object using the angular velocity.
        /// Then updates the position of our object using the linear velocity.
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public override void Update(float elapsedGameTime)
        {
            base.Update(elapsedGameTime);

            if (AngularVelocity > 0)
            {
                AttachedBaseObject.LocalRotation += AngularVelocity * elapsedGameTime;
            }

            if (LinearVelocity != Vector2.Zero)
            {
                AttachedBaseObject.LocalPosition += Vector2.Transform(LinearVelocity, Matrix.CreateRotationZ(AttachedBaseObject.WorldRotation)) * elapsedGameTime;
            }
        }

        #endregion
    }
}
