using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// A delegate used for events that are triggered when a slider has it's value changed.
    /// </summary>
    public delegate void OnSliderValueChanged(Slider slider);

    /// <summary>
    /// A UIObject representing a slider we can use to adjust a float between two limits.
    /// </summary>
    public class Slider : UIObject
    {
        /// <summary>
        /// The maximum value for our slider
        /// </summary>
        private float MaxValue { get; set; }

        /// <summary>
        /// The minimum value for our slider
        /// </summary>
        private float MinValue { get; set; }

        /// <summary>
        /// The current value of our slider
        /// </summary>
        public float CurrentValue { get; private set; }

        /// <summary>
        /// The handle for for our slider
        /// </summary>
        private Image SliderHandle { get; set; }

        /// <summary>
        /// Optional info label for this slider
        /// </summary>
        public Label InfoLabel { get; private set; }

        /// <summary>
        /// A label to display the current value of the slider
        /// </summary>
        public Label ValueLabel { get; private set; }

        /// <summary>
        /// An event handler used to call a function when the slider's value is changed.
        /// Useful for updating other UI, like a label etc.
        /// </summary>
        public event OnSliderValueChanged OnValueChanged;

        public Slider(
            float currentValue,
            string infoText, 
            Vector2 localPosition, 
            string sliderHandleTextureAsset = AssetManager.DefaultSliderHandleTextureAsset,
            string sliderBarTextureAsset = AssetManager.DefaultSliderBarTextureAsset) :
            this(0, 1, currentValue, infoText, localPosition, sliderHandleTextureAsset, sliderBarTextureAsset)
        {

        }

        public Slider(
            float minValue,
            float maxValue,
            float currentValue,
            string infoText,
            Vector2 localPosition,
            string sliderHandleTextureAsset = AssetManager.DefaultSliderHandleTextureAsset,
            string sliderBarTextureAsset = AssetManager.DefaultSliderBarTextureAsset) :
            base(localPosition, sliderBarTextureAsset)
        {
            // We do not want to use collisions ourself, but rather for our handle
            UsesCollider = false;

            SliderHandle = AddChild(new Image(Vector2.Zero, sliderHandleTextureAsset));
            SliderHandle.UsesCollider = true;

            // Fix up our label position after all the textures are initialised
            // Parent it to the bar
            InfoLabel = AddChild(new Label(infoText, Vector2.Zero, AssetManager.DefaultSpriteFontAsset));

            // Fix up our label position after all the textures are initialised
            // Parent it to the bar
            ValueLabel = AddChild(new Label(currentValue.ToString(), Vector2.Zero));

            MaxValue = maxValue;
            MinValue = minValue;
            CurrentValue = currentValue;
        }

        #region Virtual Functions

        /// <summary>
        /// Fix up UI positions now that we have all the sizes etc. for the slider bar texture.
        /// </summary>
        public override void Begin()
        {
            base.Begin();

            // Maps the current value between [0, 1]
            float currentValue = (CurrentValue - MinValue) / (MaxValue - MinValue);
            SliderHandle.LocalPosition = new Vector2((currentValue - 0.5f) * Size.X, 0);

            float padding = 5f;
            InfoLabel.LocalPosition = new Vector2(-(Size.X * 0.5f + InfoLabel.Size.X * 0.5f + SliderHandle.Size.X * 0.5f + padding), 0);
            ValueLabel.LocalPosition = new Vector2(Size.X * 0.5f + InfoLabel.Size.X * 0.5f + SliderHandle.Size.X * 0.5f + padding, 0);
        }

        /// <summary>
        /// Handles the behaviour we want from this UI - to move with the mouse when it is pressed over it, and stop when released.
        /// Then updates the value if needs be.
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        /// <param name="mousePosition"></param>
        public override void HandleInput(float elapsedGameTime, Vector2 mousePosition)
        {
            base.HandleInput(elapsedGameTime, mousePosition);

            DebugUtils.AssertNotNull(SliderHandle.Collider);
            if (SliderHandle.Collider.IsSelected)
            {
                float sliderBarHalfWidth = Size.X * 0.5f;
                float newX = MathHelper.Clamp(mousePosition.X - WorldPosition.X, -sliderBarHalfWidth, sliderBarHalfWidth);

                SliderHandle.LocalPosition = new Vector2(newX, 0);

                // Calculate our new value
                float multiplier = (newX + sliderBarHalfWidth) / (2 * sliderBarHalfWidth);
                Debug.Assert(multiplier >= 0 && multiplier <= 1);

                CurrentValue = (1 - multiplier) * MinValue + multiplier * MaxValue;

                // Update our value label text
                ValueLabel.Text = CurrentValue.ToString();

                // We do asserts rather than actual clamping, because if these asserts are false the slider is behaving incorrectly
                Debug.Assert(CurrentValue >= MinValue);
                Debug.Assert(CurrentValue <= MaxValue);

                if (OnValueChanged != null)
                {
                    OnValueChanged(this);
                }
            }
        }

        #endregion
    }
}