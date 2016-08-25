using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// A screen which is responsible for presenting the options for the game to the user to edit.
    /// For now, it holds generic options so can be overridden for custom game options.
    /// </summary>
    public abstract class OptionsScreen : MenuScreen
    {
        #region Properties and Fields

        /// <summary>
        /// A reference to our label - we will just use this for fixing up UI
        /// </summary>
        private Label FullScreenLabel { get; set; }

        /// <summary>
        /// The slider UI to adjust the music volume
        /// </summary>
        private Slider MusicVolumeSlider { get; set; }

        /// <summary>
        /// The slider UI to adjust the sfx volume
        /// </summary>
        private Slider SFXVolumeSlider { get; set; }

        #endregion

        public OptionsScreen(string screenDataAsset = "Screens\\OptionsScreen.xml") :
            base(screenDataAsset)
        {
            
        }

        #region Virtual Functions

        /// <summary>
        /// Adds UI to alter the UI.
        /// </summary>
        protected override void AddInitialUI()
        {
            base.AddInitialUI();

            float padding = ScreenDimensions.Y * 0.1f;

            Label titleLabel = AddScreenUIObject(new Label("Options", new Vector2(ScreenCentre.X, ScreenDimensions.Y * 0.1f)));

            MusicVolumeSlider = titleLabel.AddChild(new Slider(OptionsManager.MusicVolume, "Music Volume", new Vector2(0, padding)));
            MusicVolumeSlider.OnValueChanged += SyncMusicVolume;

            SFXVolumeSlider = MusicVolumeSlider.AddChild(new Slider(OptionsManager.SFXVolume, "SFX Volume", new Vector2(0, padding)));

            Button fullScreenButton = SFXVolumeSlider.AddChild(new Button(OptionsManager.IsFullScreen.ToString(), 
                                                                          new Vector2(0, padding), 
                                                                          AssetManager.DefaultNarrowButtonTextureAsset, 
                                                                          AssetManager.DefaultNarrowButtonHighlightedTextureAsset));
            fullScreenButton.ClickableModule.OnLeftClicked += SyncOptionsIsFullScreen;

            FullScreenLabel = fullScreenButton.AddChild(new Label("Fullscreen", Vector2.Zero));
        }

        /// <summary>
        /// Fixup UI positions
        /// </summary>
        public override void Begin()
        {
            base.Begin();

            DebugUtils.AssertNotNull(FullScreenLabel);
            DebugUtils.AssertNotNull(FullScreenLabel.Parent);

            float padding = 5;
            FullScreenLabel.LocalPosition = new Vector2(-(FullScreenLabel.Parent.Size.X + FullScreenLabel.Size.X) * 0.5f - padding, 0);
        }

        /// <summary>
        /// Update the music volume and sfx volume using the slider values
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        /// <param name="mousePosition"></param>
        public override void HandleInput(float elapsedGameTime, Vector2 mousePosition)
        {
            base.HandleInput(elapsedGameTime, mousePosition);

            OptionsManager.SFXVolume = SFXVolumeSlider.CurrentValue;
            OptionsManager.MusicVolume = MusicVolumeSlider.CurrentValue;
        }

        #endregion

        #region Callbacks

        /// <summary>
        /// Set's the MusicManager's volume when we change the slider for the music volume
        /// </summary>
        /// <param name="clickable"></param>
        private void SyncMusicVolume(Slider slider)
        {
            MediaPlayer.Volume = slider.CurrentValue;
        }

        /// <summary>
        /// Syncs our button's value with our options is full screen option
        /// </summary>
        /// <param name="baseObject"></param>
        private void SyncOptionsIsFullScreen(BaseObject baseObject)
        {
            Debug.Assert(baseObject is Button);
            Button button = baseObject as Button;
            DebugUtils.AssertNotNull(button);

            OptionsManager.IsFullScreen = !OptionsManager.IsFullScreen;
            button.Label.Text = OptionsManager.IsFullScreen.ToString();
        }

        /// <summary>
        /// Write the options to disc before we exit this screen
        /// </summary>
        public override void Die()
        {
            base.Die();

            OptionsManager.SaveAssets();
        }

        #endregion
    }
}