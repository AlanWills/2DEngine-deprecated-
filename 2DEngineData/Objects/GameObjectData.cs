namespace _2DEngineData
{
    /// <summary>
    /// A class used as a base for all game objects.
    /// Contains information about their texture asset, since most game obejcts will load data rather than textures.
    /// </summary>
    public class GameObjectData : BaseData
    {
        /// <summary>
        /// The texture asset for this object.
        /// </summary>
        public string TextureAsset { get; set; }
    }
}
