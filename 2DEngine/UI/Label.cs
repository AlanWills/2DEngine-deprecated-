using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2DEngine
{
    /// <summary>
    /// A class for drawing text.
    /// Has no collider by default.
    /// </summary>
    public class Label : UIObject
    {
        #region Properties and Fields

        /// <summary>
        /// The text which we will be rendering.
        /// Automatically changes the size so the new text will be in the same dimensions.
        /// </summary>
        private string text;
        public string Text
        {
            get { return text; }
            set
            {
                // Initially, our text will be null so cannot use the old dimensions to calculate the new ones, so use an indentity vector
                Vector2 oldScale = Vector2.One;

                // If our size is non zero, we wish to retain this information when we change string
                if (Size != Vector2.Zero)
                {
                    if (!string.IsNullOrEmpty(Text))
                    {
                        // If our old text is non empty we can calculate a scale and use that to scale our new text so the text appears in the same proportion
                        oldScale = Vector2.Divide(Size, TextDimensions);
                    }
                    else
                    {
                        // If our size is set, but our text is blank, we want the new text to be of the current size
                        oldScale = Vector2.Divide(Size, SpriteFont.MeasureString(value));
                    }
                }
                
                text = value;
                Size = Vector2.Multiply(oldScale, TextDimensions);
            }
        }

        /// <summary>
        /// Used in drawing the text.  Corresponds to the centre of the text string.
        /// </summary>
        private Vector2 TextCentre { get { return SpriteFont.MeasureString(Text) * 0.5f; } }

        /// <summary>
        /// Corresponds to the dimensions of the text using the SpriteFont we have set up.
        /// </summary>
        private Vector2 TextDimensions { get { return SpriteFont.MeasureString(Text); } }

        #endregion

        public Label(string text, Vector2 localPosition, string spriteFontAsset = AssetManager.DefaultSpriteFontAsset) :
            base(localPosition, AssetManager.DefaultEmptyPanelTextureAsset)
        {
            Text = text;

            // Labels do not need a collider
            UsesCollider = false;
            Colour = Color.Black;
        }

        public Label(string text, Anchor anchor, int depth, string spriteFontAsset = AssetManager.DefaultSpriteFontAsset) :
            base(anchor, depth, AssetManager.DefaultEmptyPanelTextureAsset)
        {
            Text = text;

            // Labels do not need a collider
            UsesCollider = false;
            Colour = Color.Black;
        }

        #region Virtual Functions

        /// <summary>
        /// Updates the size of the text if necessary
        /// </summary>
        public override void Initialise()
        {
            // Check to see if we should initialise
            CheckShouldInitialise();

            // Change the size to the text dimensions if unset
            if (Size == Vector2.Zero)
            {
                Size = TextDimensions;
            }

            base.Initialise();
        }

        /// <summary>
        /// Draws the text
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch used for rendering</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw the text
            DebugUtils.AssertNotNull(SpriteFont);
            spriteBatch.DrawString(
                SpriteFont, 
                Text, 
                WorldPosition, 
                Colour * Opacity, 
                WorldRotation, 
                TextCentre, 
                Vector2.Divide(Size, TextDimensions), 
                SpriteEffects.None, 
                0);

            Children.Draw(spriteBatch);
        }

        #endregion
    }
}
