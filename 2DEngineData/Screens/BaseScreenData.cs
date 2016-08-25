using System.Collections.Generic;
using System.Xml.Serialization;

namespace _2DEngineData
{
    /// <summary>
    /// A class to handle basic data for all screens.
    /// Can be overidden to provide data for custom screens.
    /// </summary>
    public class BaseScreenData : BaseData
    {
        /// <summary>
        /// The path for the background image.  Leave blank if no image is included.
        /// </summary>
        public string BackgroundTextureAsset { get; set; }

        /// <summary>
        /// A list of the songs which will play in our screen.
        /// </summary>
        [XmlArrayItem(ElementName = "Item")]
        public List<string> Music { get; set; }
    }
}
