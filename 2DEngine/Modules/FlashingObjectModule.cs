using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// A module which implements the IFlashing interface.
    /// Alters the AttachedGameObject's Opacity so that it flashes in and out.
    /// </summary>
    public class FlashingObjectModule : BaseObjectModule, IFlashing
    {
        #region Properties and Fields

        /// <summary>
        /// A flag to indicate whether we are flashing out.
        /// </summary>
        public bool FlashingOut { get; private set; }

        /// <summary>
        /// The minimum opacity we will lerp to
        /// </summary>
        public float MinOpacity { get; private set; }

        /// <summary>
        /// The maximum opacity we will lerp to
        /// </summary>
        public float MaxOpacity { get; private set; }

        /// <summary>
        /// The speed at which we flash
        /// </summary>
        public float LerpSpeed { get; set; }

        /// <summary>
        /// The lerp value for our flashing
        /// </summary>
        private float CurrentLerpValue { get; set; }

        #endregion

        public FlashingObjectModule(float minOpacity = 0.1f, float maxOpacity = 1f, float lerpSpeed = 0.5f) :
            base()
        {
            Debug.Assert(minOpacity >= 0);
            Debug.Assert(maxOpacity <= 1);

            FlashingOut = true;
            MinOpacity = minOpacity;
            MaxOpacity = maxOpacity;
            LerpSpeed = lerpSpeed;
            CurrentLerpValue = 1;
        }

        #region Virtual Functions

        /// <summary>
        /// Set the base object to the max opacity
        /// </summary>
        public override void Begin()
        {
            base.Begin();

            DebugUtils.AssertNotNull(AttachedBaseObject);
            AttachedBaseObject.Opacity = MaxOpacity;
        }

        /// <summary>
        /// Implement the flashing here - changing the opacity
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public override void Update(float elapsedGameTime)
        {
            base.Update(elapsedGameTime);

            FlashFunction(elapsedGameTime);
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// The function which alters the object's opacity
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public void FlashFunction(float elapsedGameTime)
        {
            if (FlashingOut)
            {
                // Subtract a linear increment from our lerp value
                CurrentLerpValue -= LerpSpeed * elapsedGameTime;

                // Check to see if we are less than or equal to the min and switch to FlashingIn
                if (AttachedBaseObject.Opacity <= MinOpacity)
                {
                    FlashingOut = false;
                }
            }
            else
            {
                // Add a linear increment to our lerp value
                CurrentLerpValue += LerpSpeed * elapsedGameTime;
                
                // Check to see if we are more than or equal to the max and switch to FlashingOut
                if (AttachedBaseObject.Opacity >= MaxOpacity)
                {
                    FlashingOut = true;
                }
            }

            // Update the opacity of our object
            AttachedBaseObject.Opacity = MathHelper.Lerp(MinOpacity, MaxOpacity, CurrentLerpValue);
        }

        /// <summary>
        /// Resets the flash to start at it's max value and begin FlashingOut
        /// </summary>
        public void Reset()
        {
            AttachedBaseObject.Opacity = MaxOpacity;
            FlashingOut = true;
            CurrentLerpValue = 1;
        }

        #endregion
    }
}