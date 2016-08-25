using _2DEngineData;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace _2DEngine
{
    public static class OptionsManager
    {
        #region Properties and Fields

        /// <summary>
        /// A bool to indicate whether our game is full screen or not.
        /// </summary>
        private static bool isFullScreen = false;
        public static bool IsFullScreen
        {
            get { return isFullScreen; }
            set
            {
                isFullScreen = value;

                ScreenManager.Instance.GraphicsDeviceManager.IsFullScreen = IsFullScreen;
                ScreenManager.Instance.GraphicsDeviceManager.ApplyChanges();
            }
        }

        /// <summary>
        /// A property we can use to drive the music volume
        /// </summary>
        private static float musicVolume = 1;
        public static float MusicVolume
        {
            get { return musicVolume; }
            set
            {
                musicVolume = value;
                MediaPlayer.Volume = musicVolume;
            }
        }

        /// <summary>
        /// A float between 0 and 1 which determines the volume of our game SFX
        /// </summary>
        public static float SFXVolume { get; set; }

        #endregion

        #region Methods

        public static void LoadAssets(ContentManager content)
        {
            DebugUtils.AssertNotNull(AssetManager.OptionsPath);

            bool createIfDoesNotExist = true;
            OptionsData options = AssetManager.GetData<OptionsData>(AssetManager.OptionsPath, createIfDoesNotExist);
            DebugUtils.AssertNotNull(options);

            IsFullScreen = options.IsFullScreen;
            MusicVolume = options.MusicVolume;
            MediaPlayer.Volume = MusicVolume;
            SFXVolume = options.SFXVolume;
        }

        public static void SaveAssets()
        {
            DebugUtils.AssertNotNull(AssetManager.OptionsPath);

            OptionsData options = new OptionsData();
            options.IsFullScreen = IsFullScreen;
            options.MusicVolume = MusicVolume;
            options.SFXVolume = SFXVolume;

            AssetManager.SaveData(options, AssetManager.OptionsPath);
        }

        #endregion
    }
}
