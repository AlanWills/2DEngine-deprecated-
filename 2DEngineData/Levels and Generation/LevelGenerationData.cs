using System.Collections.Generic;
using System.Xml.Serialization;

namespace _2DEngineData
{
    public class LevelGenerationData : BaseData
    {
        /// <summary>
        /// The width of our level in tiles
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of our level in tiles
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The probability we will generate a walkable layer tile
        /// </summary>
        public float WalkableLayerProbability { get; set; }

        /// <summary>
        /// The probability we will change the height of our walkable layer
        /// </summary>
        public float HeightChangeProbability { get; set; }

        /// <summary>
        /// The maximum value our level can change in tiles
        /// </summary>
        public int MaximumHeightChange { get; set; }

        /// <summary>
        /// The texture asset corresponding to a left tile of our walkable layer
        /// </summary>
        public string WalkableLayerLeftTextureAsset { get; set; }

        /// <summary>
        /// The texture asset corresponding to a middle tile of our walkable layer
        /// </summary>
        public string WalkableLayerMiddleTextureAsset { get; set; }

        /// <summary>
        /// The texture asset corresponding to a right tile of our walkable layer
        /// </summary>
        public string WalkableLayerRightTextureAsset { get; set; }

        /// <summary>
        /// A list of all the hazard texture assets in our level
        /// </summary>
        [XmlArrayItem(ElementName = "Item")]
        public List<string> HazardTextureAssets { get; set; }

        /// <summary>
        /// The texture asset we will use for the terrain underneath our walkable layer (positive y direction in game space)
        /// </summary>
        public string BelowWalkableLayerTextureAsset { get; set; }

        /// <summary>
        /// The texture asset we will use for the terrain above our walkable layer (negative y)
        /// </summary>
        public string AboveWalkableLayerTextureAsset { get; set; }
    }
}
