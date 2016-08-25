using Microsoft.Xna.Framework;

namespace _2DEngine
{
    /// <summary>
    /// A class used a lot in description UI.
    /// Represents an image with a description label on it's RHS by default.
    /// Useful in menus or gameplay for holding stats and thumbnails.
    /// The anchor of this object will be in the centre of the combined images.
    /// </summary>
    public class ImageAndLabel : Image
    {
        #region Properties and Fields

        /// <summary>
        /// The label in this pair
        /// </summary>
        public Label Label { get; set; }

        /// <summary>
        /// When we set the colour of this, we also set the colour of the label
        /// </summary>
        public override Color Colour
        {
            get
            {
                return base.Colour;
            }

            set
            {
                base.Colour = value;
                Label.Colour = value;
            }
        }

        /// <summary>
        /// The centre of the Image and Label together
        /// </summary>
        public Vector2 CentreAnchor { get; private set; }

        #endregion

        public ImageAndLabel(string labelText, Vector2 localPosition, string imageTextureAsset) :
            this(labelText, Vector2.Zero, localPosition, imageTextureAsset)
        {

        }

        public ImageAndLabel(string labelText, Vector2 size, Vector2 localPosition, string imageTextureAsset) :
            base(size, localPosition, imageTextureAsset)
        {
            // Fixup the position later
            Label = AddChild(new Label(labelText, Vector2.Zero));
        }

        #region Virtual Functions

        /// <summary>
        /// Do some UI fixup - find the centre of image and label and shift them accordingly so the anchor of this object is now in the centre.
        /// </summary>
        public override void Initialise()
        {
            CheckShouldInitialise();

            base.Initialise();

            float padding = 10;
            CentreAnchor = new Vector2(Size.X + padding + Label.Size.X, 0) * 0.5f;

            LocalPosition -= (CentreAnchor - new Vector2(Size.X * 0.5f, 0));
            Label.LocalPosition = CentreAnchor;
        }

        #endregion
    }
}