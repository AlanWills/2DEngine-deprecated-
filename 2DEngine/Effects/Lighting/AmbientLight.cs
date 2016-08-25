using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2DEngine
{
    /// <summary>
    /// A class which corresponds to a light which will affect our entire game scene
    /// </summary>
    public class AmbientLight : Light
    {
        public AmbientLight(Color colour, float intensity = 1, float lifeTime = float.MaxValue) :
            base(ScreenManager.Instance.ScreenCentre, colour, AssetManager.DefaultEmptyPanelTextureAsset, intensity, lifeTime)
        {

        }

        #region Virtual Functions

        /// <summary>
        /// Ambient lights perform no drawing so this function is empty.
        /// They merely affect the colour that we pass into the Light Shader
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            // Ambient lights perform no drawing
        }

        #endregion
    }
}
