using Microsoft.Xna.Framework;

namespace _2DEngine
{
    /// <summary>
    /// A very simple extension to the Label class which contains a flashing module.
    /// </summary>
    public class FlashingLabel : Label
    {
        public FlashingLabel(string text, Vector2 localPosition, string spriteFontAsset = AssetManager.DefaultSpriteFontAsset) :
            base(text, localPosition, spriteFontAsset)
        {
            AddModule(new FlashingObjectModule());
        }
    }
}
