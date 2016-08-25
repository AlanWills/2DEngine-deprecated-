namespace _2DEngine
{
    /// <summary>
    /// A class which is basically an Image, but loads it's texture through the ContentManager rather than through the AssetManager.
    /// ONLY to be used in the start up logo screen.
    /// </summary>
    public class Logo : Image
    {
        public Logo() :
            base(ScreenManager.Instance.ScreenCentre, AssetManager.StartupLogoTextureAsset)
        {
        }

        #region Virtual Functions

        /// <summary>
        /// Load the logo from the ContentManager directly rather than from the AssetManager.
        /// This is because it will be created before the Assets are loaded.
        /// </summary>
        public override void LoadContent()
        {
            CheckShouldLoad();

            Texture = AssetManager.GetSprite(TextureAsset);

            base.LoadContent();
        }

        #endregion
    }
}