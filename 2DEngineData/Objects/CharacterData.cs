using System.Collections.Generic;
using System.Xml.Serialization;

namespace _2DEngineData
{
    /// <summary>
    /// A class to hold the animation data assets for a character.
    /// </summary>
    public class CharacterData : GameObjectData
    {
        /// <summary>
        /// The path to the folder for these animations.
        /// </summary>
        public string FolderPath { get; set; }

        /// <summary>
        /// The list of animation data assets that will be used by this character.
        /// Should be the full path - folder path of the xml file describing the animation.
        /// </summary>
        public List<string> AnimationInfo { get; set; }

        /// <summary>
        /// The speed at which the player will walk
        /// </summary>
        public float WalkSpeed { get; set; }

        /// <summary>
        /// The speed at which the player will run
        /// </summary>
        public float RunSpeed { get; set; }

        /// <summary>
        /// The height at which the player can jump
        /// </summary>
        public float JumpHeight { get; set; }
    }
}
