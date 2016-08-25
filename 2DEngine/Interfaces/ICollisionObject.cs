namespace _2DEngine
{
    /// <summary>
    /// An interface for objects whose collisions will be handled by our gameplay screen.
    /// Enforces two behaviours be addressed - collision with environment and other collision object
    /// </summary>
    public interface ICollisionObject
    {
        /// <summary>
        /// The collider for this object - MUST have one
        /// </summary>
        Collider Collider { get; }

        /// <summary>
        /// A function which is called when colliding with an environment object
        /// </summary>
        /// <param name="collidedEnvironmentObject">The environment object we have collided with</param>
        void OnCollisionWithEnvironment(BaseObject collidedEnvironmentObject);

        /// <summary>
        /// A function which is called when colliding with another collision object
        /// </summary>
        /// <param name="collisionObject">The collision object we have collided with</param>
        void OnCollisionWithCollisionObject(ICollisionObject collisionObject);
    }
}