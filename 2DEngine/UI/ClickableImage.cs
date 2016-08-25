using Microsoft.Xna.Framework;

namespace _2DEngine
{
    /// <summary>
    /// A class to represent an image which executes a function when clicked.
    /// </summary>
    public class ClickableImage : Image
    {
        #region Properties and Fields

        /// <summary>
        /// A reference to our clickable module.
        /// </summary>
        public ClickableObjectModule ClickableModule { get; private set; }

        #endregion

        public ClickableImage(Vector2 localPosition, string textureAsset) :
            this(Vector2.Zero, localPosition, textureAsset)
        {
        }

        public ClickableImage(Vector2 size, Vector2 localPosition, string textureAsset) :
            base(size, localPosition, textureAsset)
        {
            UsesCollider = true;

            ClickableModule = AddModule(new ClickableObjectModule());
            DebugUtils.AssertNotNull(ClickableModule);
        }

        public ClickableImage(Anchor anchor, int depth, string textureAsset) :
            this(Vector2.Zero, anchor, depth, textureAsset)
        {
        }

        public ClickableImage(Vector2 size, Anchor anchor, int depth, string textureAsset) :
            base(size, anchor, depth, textureAsset)
        {
            UsesCollider = true;

            ClickableModule = AddModule(new ClickableObjectModule());
            DebugUtils.AssertNotNull(ClickableModule);
        }
    }
}
