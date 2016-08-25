using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// A dialog which we access from our InGameEscapeDialog for changing options in game
    /// </summary>
    public class InGameOptionsDialog : UIObject
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

        public InGameOptionsDialog(string textureAsset = AssetManager.DefaultMenuTextureAsset) :
            base(ScreenManager.Instance.ScreenDimensions * 0.5f, ScreenManager.Instance.ScreenCentre, textureAsset)
        {
            UsesCollider = false;
        }

        #region Virtual Functions

        /// <summary>
        /// Add the UI for our options.
        /// </summary>
        public override void LoadContent()
        {
            CheckShouldLoad();

            Vector2 screenDimensions = ScreenManager.Instance.ScreenDimensions;
            float padding = Size.Y * 0.2f;

            MusicVolumeSlider = AddChild(new Slider(OptionsManager.MusicVolume, "Music Volume", new Vector2(0, -Size.Y * 0.5f + padding)));
            MusicVolumeSlider.OnValueChanged += SyncMusicVolume;
            MusicVolumeSlider.InfoLabel.Colour = Color.White;
            MusicVolumeSlider.ValueLabel.Colour = Color.White;

            SFXVolumeSlider = MusicVolumeSlider.AddChild(new Slider(OptionsManager.SFXVolume, "SFX Volume", new Vector2(0, padding)));
            SFXVolumeSlider.InfoLabel.Colour = Color.White;
            SFXVolumeSlider.ValueLabel.Colour = Color.White;

            Button fullScreenButton = SFXVolumeSlider.AddChild(new Button(OptionsManager.IsFullScreen.ToString(),
                                                                          new Vector2(0, padding),
                                                                          AssetManager.DefaultNarrowButtonTextureAsset,
                                                                          AssetManager.DefaultNarrowButtonHighlightedTextureAsset));
            fullScreenButton.ClickableModule.OnLeftClicked += SyncOptionsIsFullScreen;

            FullScreenLabel = fullScreenButton.AddChild(new Label("Fullscreen", Vector2.Zero));
            FullScreenLabel.Colour = Color.White;

            base.LoadContent();
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
        /// If escape is pressed we kill this dialog and go back to the in game escape dialog.
        /// Update the Music and SFX volume based on the slider values
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        /// <param name="mousePosition"></param>
        public override void HandleInput(float elapsedGameTime, Vector2 mousePosition)
        {
            base.HandleInput(elapsedGameTime, mousePosition);

            if (GameKeyboard.Instance.IsKeyPressed(Keys.Escape))
            {
                Hide();

                // Don't need to show the In Game Escape Dialog - that will happen automatically from the gameplay screen
            }

            OptionsManager.SFXVolume = SFXVolumeSlider.CurrentValue;
            OptionsManager.MusicVolume = MusicVolumeSlider.CurrentValue;
        }

        /// <summary>
        /// Save the options to disc before we hide this dialog (corresponds to leaving the dialog)
        /// </summary>
        public override void Hide(bool hideChildren = true)
        {
            base.Hide(hideChildren);

            OptionsManager.SaveAssets();
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

        #endregion
    }
}