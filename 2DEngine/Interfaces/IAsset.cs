using Microsoft.Xna.Framework.Content;

namespace _2DEngine
{
    /// <summary>
    /// An interface for classes which will be loaded at startup and saved at close down.
    /// </summary>
    public interface IAsset
    {
        /// <summary>
        /// The function to call on startup during our Startup screen, during our load thread
        /// </summary>
        /// <param name="content"></param>
        void LoadAssets(ContentManager content);

        /// <summary>
        /// The function which will be called at close down (and possibly at other points too for backup)
        /// </summary>
        void SaveAssets();
    }
}
