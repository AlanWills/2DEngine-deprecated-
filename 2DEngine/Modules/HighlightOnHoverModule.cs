using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// An enum to indicate how we are going to blend our two colours in the HighlightOnHoverModule
    /// </summary>
    public enum BlendMode
    {
        kLerp,      // Linearly blend with factor over time
        kBinary,    // Step function based on whether the mouse is over the object or not
    }

    /// <summary>
    /// A module which implements basic colour change highlighting when the mouse is over the attached base object
    /// </summary>
    public class HighlightOnHoverModule : BaseObjectModule
    {

        #region Properties and Fields

        /// <summary>
        /// The colour we will lerp to when our mouse is not over our object it
        /// </summary>
        public Color DefaultColour { get; set; }

        /// <summary>
        /// The colour we will set our object to when our mouse is not over it
        /// </summary>
        public Color HighlightedColour { get; set; }

        /// <summary>
        /// The current mode of blending this module is using
        /// </summary>
        public BlendMode BlendMode { get; set; }

        /// <summary>
        /// The speed at which we will be lerping
        /// </summary>
        public float LerpSpeed { get; set; }

        /// <summary>
        /// The current lerp amount between our two colour modes
        /// </summary>
        private float lerpAmount;
        private float LerpAmount
        {
            get
            {
                if (BlendMode == BlendMode.kBinary)
                {
                    // If our BlendMode is binary then we return 1 if our mouse is over it, or 0 if not
                    return AttachedBaseObject.Collider.IsMouseOver ? 1 : 0;
                }
                else
                {
                    return lerpAmount;
                }
            }
            set
            {
                // Clamp our lerp amount between 0 and 1
                lerpAmount = MathHelper.Clamp(value, 0, 1);
            }
        }

        #endregion

        public HighlightOnHoverModule(Color defaultColour, Color highlightedColour, BlendMode blendMode = BlendMode.kLerp, float lerpSpeed = 2.5f)
            : base()
        {
            DefaultColour = defaultColour;
            HighlightedColour = highlightedColour;

            BlendMode = blendMode;
            LerpAmount = lerpAmount;
            LerpSpeed = lerpSpeed;
        }

        #region Virtual Functions

        /// <summary>
        /// Checks our attached object's collider is not null, otherwise we are a bit boned
        /// </summary>
        public override void Begin()
        {
            base.Begin();

            // If we should handle input, we set the colour of the object
            // We only do this if we should handle input, because if this is initially switched off we may not want to override any default colour we have set
            DebugUtils.AssertNotNull(AttachedBaseObject.Collider);
            if (ShouldHandleInput)
            {
                AttachedBaseObject.Colour = DefaultColour;
            }
        }

        /// <summary>
        /// Uses whether the mouse is over our object to change the lerp amount and colour
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        /// <param name="mousePosition"></param>
        public override void HandleInput(float elapsedGameTime, Vector2 mousePosition)
        {
            base.HandleInput(elapsedGameTime, mousePosition);

            if (AttachedBaseObject.Collider.IsMouseOver)
            {
                // If our mouse is over the object then we set the lerp amount to 1 - want to use all of the HighlightedColour
                LerpAmount = 1;
            }
            else
            {
                // Lerp down to 0 again
                LerpAmount -= elapsedGameTime * LerpSpeed;
            }

            // Perform the colour lerping
            AttachedBaseObject.Colour = Color.Lerp(DefaultColour, HighlightedColour, LerpAmount);
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// A function which resets the attached objects colour to DefaultColour and resets the LerpAmount to 0
        /// </summary>
        public void Reset()
        {
            LerpAmount = 0;
            AttachedBaseObject.Colour = DefaultColour;
        }

        #endregion
    }
}
