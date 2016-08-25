using Microsoft.Xna.Framework;

namespace _2DEngine
{
    /// <summary>
    /// A light that eminates from a position.
    /// Using the default light texture the size is 250, 250
    /// </summary>
    public class PointLight : Light
    {
        public PointLight(Vector2 localPosition, Color colour, string pointLightTextureAsset = AssetManager.DefaultPointLightTextureAsset, float intensity = 1, float lifeTime = float.MaxValue) :
            this(Vector2.Zero, localPosition, colour, pointLightTextureAsset, intensity, lifeTime)
        {

        }

        public PointLight(Vector2 size, Vector2 localPosition, Color colour, string pointLightTextureAsset = AssetManager.DefaultPointLightTextureAsset, float intensity = 1, float lifeTime = float.MaxValue) :
            base(size, localPosition, colour, pointLightTextureAsset, intensity, lifeTime)
        {
            
        }
    }
}