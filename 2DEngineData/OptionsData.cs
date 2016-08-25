namespace _2DEngineData
{
    public class OptionsData : BaseData
    {
        /// <summary>
        /// A bool to indicate whether the game is in full screen mode
        /// </summary>
        public bool IsFullScreen { get; set; }

        /// <summary>
        /// The volume of the music between 0 and 1
        /// </summary>
        public float MusicVolume { get; set; }

        /// <summary>
        /// The volumes of the SFX between 0 and 1
        /// </summary>
        public float SFXVolume { get; set; }
    }
}
