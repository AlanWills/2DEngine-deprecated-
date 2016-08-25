using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// A class for scaling textures but preserving their aspect ratio.
    /// With a UIObject, inputting a size scalesthe image exactly, destroying the aspect ratio.
    /// This class takes the inputted size, sets the largest dimension of the texture to that size
    /// and sets the other dimension to the appopriate size to preserve the aspect ratio of the texture.
    /// </summary>
    public class Image : UIObject
    {
        #region Properties and Fields

        /// <summary>
        /// A flag to indicate whether this object preserves the aspect ratio of the texture it represents.
        /// Default is true.
        /// </summary>
        public bool PreservesAspectRatio { get; set; }

        #endregion

        public Image(Vector2 localPosition, string textureAsset) :
            this(Vector2.Zero, localPosition, textureAsset)
        {
            
        }
        
        public Image(Vector2 size, Vector2 localPosition, string textureAsset) :
            base(size, localPosition, textureAsset)
        {
            UsesCollider = false;
            PreservesAspectRatio = true;
        }

        public Image(Anchor anchor, int depth, string textureAsset) :
            this(Vector2.Zero, anchor, depth, textureAsset)
        {
        }

        public Image(Vector2 size, Anchor anchor, int depth, string textureAsset) :
            base(size, anchor, depth, textureAsset)
        {
            UsesCollider = false;
            PreservesAspectRatio = true;
        }

        #region Virtual Functions

        /// <summary>
        /// Sets up the size to preserve the texture aspect ratio
        /// </summary>
        public override void Initialise()
        {
            // Check to see whether we should Initialise
            CheckShouldInitialise();

            // Texture cannot be null
            DebugUtils.AssertNotNull(Texture);

            // If we are preserving aspect ratio, recalculate the size
            if (PreservesAspectRatio)
            {
                float aspectRatio = Texture.Bounds.Height / (float)Texture.Bounds.Width;
                Debug.Assert(aspectRatio > 0);

                if (aspectRatio < 1)
                {
                    Size = new Vector2(Size.X, Size.X * aspectRatio);
                }
                else
                {
                    Size = new Vector2(Size.Y / aspectRatio, Size.Y);
                }
            }

            base.Initialise();
        }

        #endregion
    }
}
