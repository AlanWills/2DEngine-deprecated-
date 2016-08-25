using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace _2DEngine
{
    public class Button : ClickableImage
    {
        #region Properties and Fields

        /// <summary>
        /// A label for any text we wish to draw on top of our button texture
        /// </summary>
        public Label Label { get; private set; }

        /// <summary>
        /// The texture that will be displayed when the mouse is over this object
        /// </summary>
        private Texture2D HighlightedTexture { get; set; }

        /// <summary>
        /// Saved reference to our normal texture so that we can retrieve it again if we change our drawing texture
        /// </summary>
        private Texture2D DefaultTexture { get; set; }

        /// <summary>
        /// A sound effect triggered when the mouse enters the button
        /// </summary>
        private CustomSoundEffect onEnterSFX;
        private CustomSoundEffect OnEnterSFX
        {
            get
            {
                if (onEnterSFX == null)
                {
                    onEnterSFX = new CustomSoundEffect(OnEnterSFXAsset);
                }

                return onEnterSFX;
            }
        }

        /// <summary>
        /// A sound effect triggered when the button is pressed
        /// </summary>
        private CustomSoundEffect onPressedSFX;
        private CustomSoundEffect OnPressedSFX
        {
            get
            {
                if (onPressedSFX == null)
                {
                    onPressedSFX = new CustomSoundEffect(OnPressedSFXAsset);
                }

                return onPressedSFX;
            }
        }

        /// <summary>
        /// A string to represent the sound effect we will load when we want to play the OnEnterSFX
        /// </summary>
        protected string OnEnterSFXAsset { get; set; }

        /// <summary>
        /// A string to represent the sound effect we will load when we want to play the OnPressedSFX
        /// </summary>
        protected string OnPressedSFXAsset { get; set; }

        private string _highlightedTextureAsset;

        #endregion

        private static Color DefaultColour = Color.White;
        private static Color DefaultLabelColour = Color.White;
        private static Color DisabledColour = Color.DarkGray;
        private static Color DisabledLabelColour = Color.DarkGray;

        public Button(
            string buttonText, 
            Vector2 localPosition, 
            string textureAsset = AssetManager.DefaultButtonTextureAsset, 
            string highlightedTextureAsset = AssetManager.DefaultButtonHighlightedTextureAsset) :
            this(buttonText, Vector2.Zero, localPosition, textureAsset, highlightedTextureAsset)
        {

        }

        public Button(string buttonText, Vector2 size, Vector2 localPosition, string textureAsset = AssetManager.DefaultButtonTextureAsset, string highlightedTextureAsset = AssetManager.DefaultButtonHighlightedTextureAsset) :
            base(size, localPosition, textureAsset)
        {
            // In general we want our buttons to be of any size really
            PreservesAspectRatio = false;

            // Create the label in the centre of the button
            Label = AddChild(new Label(buttonText, Vector2.Zero));
            Label.Colour = DefaultLabelColour;

            _highlightedTextureAsset = highlightedTextureAsset;

            OnEnterSFXAsset = "UI\\ButtonHover";
            OnPressedSFXAsset = "UI\\ButtonPressedSound";
        }

        public Button(
            string buttonText,
            Anchor anchor,
            int depth,
            string textureAsset = AssetManager.DefaultButtonTextureAsset,
            string highlightedTextureAsset = AssetManager.DefaultButtonHighlightedTextureAsset) :
            this(buttonText, Vector2.Zero, anchor, depth, textureAsset, highlightedTextureAsset)
        {

        }

        public Button(string buttonText, Vector2 size, Anchor anchor, int depth, string textureAsset = AssetManager.DefaultButtonTextureAsset, string highlightedTextureAsset = AssetManager.DefaultButtonHighlightedTextureAsset) :
            base(size, anchor, depth, textureAsset)
        {
            // In general we want our buttons to be of any size really
            PreservesAspectRatio = false;

            // Create the label in the centre of the button
            Label = AddChild(new Label(buttonText, Vector2.Zero));
            Label.Colour = DefaultLabelColour;

            _highlightedTextureAsset = highlightedTextureAsset;

            OnEnterSFXAsset = "UI\\ButtonHover";
            OnPressedSFXAsset = "UI\\ButtonPressedSound";
        }

        #region Virtual Functions

        /// <summary>
        /// Loads our button textures
        /// </summary>
        public override void LoadContent()
        {
            CheckShouldLoad();

            HighlightedTexture = AssetManager.GetSprite(_highlightedTextureAsset);
            DebugUtils.AssertNotNull(HighlightedTexture);

            base.LoadContent();

            DebugUtils.AssertNotNull(Texture);
            DefaultTexture = Texture;
        }

        /// <summary>
        /// Handles sound effect for our button - plays a sound effect if we have just entered the button.
        /// Set our texture to be the highlighted texture if our mouse is over, otherwise the normal texture
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        /// <param name="mousePosition"></param>
        public override void HandleInput(float elapsedGameTime, Vector2 mousePosition)
        {
            base.HandleInput(elapsedGameTime, mousePosition);

            // Play SFX if we have entered the button
            DebugUtils.AssertNotNull(Collider);
            if (Collider.IsEntered)
            {
                OnEnterSFX.Play();
            }

            if (Collider.IsClicked)
            {
                OnPressedSFX.Play();
            }

            DebugUtils.AssertNotNull(Collider);
            Texture = Collider.IsMouseOver ? HighlightedTexture : DefaultTexture;
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Disables the button from handling input or updating.
        /// Changes the button colour to be grey and the text colour to be black.
        /// </summary>
        public void Disable()
        {
            ShouldHandleInput = false;
            Texture = DefaultTexture;
            Colour = DisabledColour;
            Label.Colour = DisabledLabelColour;
        }

        /// <summary>
        /// Enables the button so it handles input and updates.
        /// Changes the button colour back to default and the text colour to white
        /// </summary>
        public void Enable()
        {
            ShouldHandleInput = true;
            Texture = Collider.IsMouseOver ? HighlightedTexture : DefaultTexture;
            Colour = DefaultColour;
            Label.Colour = DefaultLabelColour;
        }

        #endregion
    }
}