using Microsoft.Xna.Framework;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;

namespace _2DEngine
{
    /// <summary>
    /// A delegate used to update an external value when this bar changes it's value.
    /// Bar drives external value.
    /// </summary>
    /// <param name="bar"></param>
    public delegate void OnValueChanged(Bar bar);

    /// <summary>
    /// A delegate used to update the bar based on an external value.
    /// External value drives Bar.
    /// </summary>
    /// <returns></returns>
    public delegate float WatchValue();

    /// <summary>
    /// The class for a bar UI which holds a number between 0 and 1
    /// </summary>
    public class Bar : Image
    {
        #region Properties and Fields

        /// <summary>
        /// This image is the front of the bar
        /// </summary>
        public Image FrontImage { get; private set; }

        /// <summary>
        /// The minimum value for our bar
        /// </summary>
        private float MinValue { get; set; }

        /// <summary>
        /// The maximum value for our bar
        /// </summary>
        private float MaxValue { get; set; }

        /// <summary>
        /// The current value of our bar
        /// </summary>
        private float currentValue;
        public float CurrentValue
        {
            get { return currentValue; }
            private set
            {
                // We do asserts rather than actual clamping, because if these asserts are false the slider is behaving incorrectly
                Debug.Assert(value >= MinValue);
                Debug.Assert(value <= MaxValue);

                currentValue = value;
                UpdateUIFromValueChange();
            }
        }

        /// <summary>
        /// Optional info label for this bar
        /// </summary>
        private Label InfoLabel { get; set; }

        /// <summary>
        /// A label to display the current value of the bar
        /// </summary>
        private Label ValueLabel { get; set; }

        /// <summary>
        /// A callback for when this bar's value is changed
        /// </summary>
        public event OnValueChanged OnValueChanged;

        /// <summary>
        /// A callback to update this bar's value using an external value
        /// </summary>
        public event WatchValue WatchValue;

        #endregion

        public Bar(
            float currentValue,
            string text,
            Vector2 size,
            Vector2 localPosition,
            string foregroundTextureAsset = AssetManager.DefaultBarForegroundTextureAsset,
            string backgroundTextureAsset = AssetManager.DefaultBarBackgroundTextureAsset) :
            this(0, 1, currentValue, text, size, localPosition, foregroundTextureAsset, backgroundTextureAsset)
        {

        }

        public Bar(
            float minValue,
            float maxValue,
            float currentValue,
            string text,
            Vector2 size,
            Vector2 localPosition,
            string foregroundTextureAsset = AssetManager.DefaultBarForegroundTextureAsset,
            string backgroundTextureAsset = AssetManager.DefaultBarBackgroundTextureAsset) :
            base(size, localPosition, backgroundTextureAsset)
        {
            FrontImage = AddChild(new Image(Vector2.Zero, foregroundTextureAsset));
            FrontImage.Colour = Color.Blue;

            // Fix up our label position after all the textures are initialised
            // Parent it to the bar
            InfoLabel = AddChild(new Label(text, Vector2.Zero, AssetManager.DefaultSpriteFontAsset));

            // Fix up our label position after all the textures are initialised
            // Parent it to the bar
            ValueLabel = AddChild(new Label(currentValue.ToString(), Vector2.Zero, AssetManager.DefaultSpriteFontAsset));

            MinValue = minValue;
            MaxValue = maxValue;
            CurrentValue = currentValue;
        }

        #region Virtual Functions

        /// <summary>
        /// Check that we do not have events set up for both the WatchEvent and OnValueChanged otherwise we will get an infinite loop.
        /// Also perform some UI fixup on our labels etc.
        /// </summary>
        public override void Begin()
        {
            base.Begin();

            // Set up our UI sizes and positions correctly
            UpdateUIFromValueChange();

            float padding = 5f;
            InfoLabel.LocalPosition = new Vector2(-(Size.X * 0.5f + InfoLabel.Size.X * 0.5f + padding), 0);
            ValueLabel.LocalPosition = new Vector2(Size.X * 0.5f + InfoLabel.Size.X * 0.5f + padding, 0);

            // Check that at least one of the event handlers is null
            Debug.Assert(OnValueChanged == null || WatchValue == null);
        }

        /// <summary>
        /// Handles the behaviour we want from this UI - mouse clicking on the bar will set the value to where we clicked on the bar proportionally
        /// Then updates the value if needs be.
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        /// <param name="mousePosition"></param>
        public override void HandleInput(float elapsedGameTime, Vector2 mousePosition)
        {
            base.HandleInput(elapsedGameTime, mousePosition);

            if (Collider.IsPressed)
            {
                float barHalfWidth = Size.X * 0.5f;

                // Calculate our new value based on where we clicked on the bar
                float worldPosition = WorldPosition.X;
                float newX = MathHelper.Clamp(mousePosition.X, worldPosition - barHalfWidth, worldPosition + barHalfWidth);
                CurrentValue = MinValue + MaxValue * (newX - (worldPosition - barHalfWidth)) / Size.X;
            }
        }

        /// <summary>
        /// Updates our UI for the Bar
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public override void Update(float elapsedGameTime)
        {
            base.Update(elapsedGameTime);

            if (WatchValue != null)
            {
                CurrentValue = WatchValue();
            }
        }

        #endregion

        /// <summary>
        /// Updates FrontImage Size and LocalPosition and other Value Label text
        /// </summary>
        private void UpdateUIFromValueChange()
        {
            float valueMappedToZeroOne = (CurrentValue - MinValue) / (MaxValue - MinValue);
            float newX = Size.X * (valueMappedToZeroOne - 1) * 0.5f;

            FrontImage.LocalPosition = new Vector2(newX, 0);
            FrontImage.Size = new Vector2(valueMappedToZeroOne * Size.X, Size.Y);

            // Update our value label text
            ValueLabel.Text = CurrentValue.ToString();

            if (OnValueChanged != null)
            {
                OnValueChanged(this);
            }
        }
    }
}
