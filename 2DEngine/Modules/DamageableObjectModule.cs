using Microsoft.Xna.Framework;
using System;

namespace _2DEngine
{
    public delegate float CalculateDamageHandler(BaseObject objectDoingTheDamage, float inputDamage);
    public delegate void OnDamageHandler(DamageableObjectModule damageableObjectModule, float damageDealt);

    /// <summary>
    /// A module which implements the IDamageable interface.
    /// </summary>
    public class DamageableObjectModule : BaseObjectModule, IDamageable
    {
        #region Properties and Fields

        /// <summary>
        /// A bool to indicate whether this object has health less than zero.
        /// Readonly.
        /// </summary>
        public bool Dead
        {
            get { return Health <= 0; }
        }

        /// <summary>
        /// The current health of the object
        /// </summary>
        private float health;
        public float Health
        {
            get { return health; }
            private set { health = MathHelper.Clamp(value, 0, MaxHealth); }
        }

        /// <summary>
        /// The max health of the object
        /// </summary>
        private float MaxHealth { get; set; }

        /// <summary>
        /// An event called before we inflict damage, which can be used to alter how much we take.
        /// </summary>
        public event CalculateDamageHandler CalculateDamage;

        /// <summary>
        /// An event called after we have been damaged.
        /// </summary>
        public event OnDamageHandler OnDamage;

        #endregion

        public DamageableObjectModule(float health) :
            base()
        {
            MaxHealth = health;
            Health = health;
        }

        #region IDamageable Interface functions

        /// <summary>
        /// Alters the health of this object.
        /// </summary>
        /// <param name="damage"></param>
        public void Damage(float damage, BaseObject objectDoingTheDamage)
        {
            if (CalculateDamage != null)
            {
                // Override the damage we are inputting
                damage = CalculateDamage(objectDoingTheDamage, damage);
            }

            float damageDealt = Math.Min(Health, damage);       // If our health is less than the damage we are dealing we only want to pass the amount of damage we are actually doing
            Health -= damage;

            OnDamage?.Invoke(this, damageDealt);
        }

        #endregion
    }
}