namespace _2DEngine
{
    public interface IDamageable
    {
        /// <summary>
        /// The health of the object.
        /// Should only be settable in the class - change it outside by calling Damage.
        /// </summary>
        float Health { get; }

        /// <summary>
        /// A flag to indicate whether we are dead.
        /// Can be used to mark the object as dead without having to change the IsAlive property.
        /// This is useful when we wish to keep it alive temporarily.
        /// </summary>
        bool Dead { get; }

        /// <summary>
        /// A function used to damage the object
        /// </summary>
        /// <param name="damage"></param>
        void Damage(float damage, BaseObject objectDoingTheDamage);
    }
}
