using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// The dialog which will appear when we press escape - has options for quitting, resuming and options.
    /// </summary>
    public class InGameEscapeDialog : ListControl
    {
        #region Properties and Fields

        /// <summary>
        /// A reference to the options dialog that will appear when we press the Options button
        /// </summary>
        private InGameOptionsDialog OptionsDialog { get; set; }

        #endregion

        public InGameEscapeDialog(string textureAsset = AssetManager.DefaultMenuTextureAsset) :
            base(ScreenManager.Instance.ScreenDimensions * 0.5f, ScreenManager.Instance.ScreenCentre, textureAsset)
        {

        }

        #region Virtual Functions

        /// <summary>
        /// Populate our menu with the buttons to resume, quit or go to options.
        /// </summary>
        public override void LoadContent()
        {
            CheckShouldLoad();

            Button resumeButton = AddChild(new Button("Resume", Vector2.Zero, AssetManager.DefaultNarrowButtonTextureAsset, AssetManager.DefaultNarrowButtonHighlightedTextureAsset));
            resumeButton.ClickableModule.OnLeftClicked += ResumeGame;

            Button optionsButton = AddChild(new Button("Options", Vector2.Zero, AssetManager.DefaultNarrowButtonTextureAsset, AssetManager.DefaultNarrowButtonHighlightedTextureAsset));
            optionsButton.ClickableModule.OnLeftClicked += AddOptionsDialog;

            Button quitToLobby = AddChild(new Button("Quit To Lobby", Vector2.Zero, AssetManager.DefaultNarrowButtonTextureAsset, AssetManager.DefaultNarrowButtonHighlightedTextureAsset));
            quitToLobby.ClickableModule.OnLeftClicked += QuitToLobby;

            Button quitToDesktop = AddChild(new Button("Quit To Desktop", Vector2.Zero, AssetManager.DefaultNarrowButtonTextureAsset, AssetManager.DefaultNarrowButtonHighlightedTextureAsset));
            quitToDesktop.ClickableModule.OnLeftClicked += QuitToDesktop;

            base.LoadContent();
        }

        /// <summary>
        /// Create our options menu once we have added the gameplay screen
        /// </summary>
        public override void Begin()
        {
            base.Begin();

            Debug.Assert(ScreenManager.Instance.CurrentScreen is GameplayScreen);
            OptionsDialog = ScreenManager.Instance.CurrentScreen.AddScreenUIObject(new InGameOptionsDialog(), true, true);
            OptionsDialog.Hide();
            OptionsDialog.Begin();
        }

        #endregion

        #region Callbacks

        /// <summary>
        /// If we click the resume button, we hide this dialog and the options dialog and resume the game.
        /// </summary>
        /// <param name="clickedObject"></param>
        private void ResumeGame(BaseObject clickedObject)
        {
            Hide();
            OptionsDialog.Hide();
        }

        /// <summary>
        /// When we click the options button, we add some dialog for the options and hide this dialog.
        /// </summary>
        /// <param name="clickedObject"></param>
        protected void AddOptionsDialog(BaseObject clickedObject)
        {
            Hide();
            OptionsDialog.Show();
        }

        /// <summary>
        /// When we click the quit to lobby button, we want to stop the current game and go back to the lobby screen.
        /// </summary>
        /// <param name="clickedObject"></param>
        protected virtual void QuitToLobby(BaseObject clickedObject) { }

        /// <summary>
        /// When we click the quit to desktop button, we want to stop the current game and quit to desktop.
        /// </summary>
        /// <param name="clickedObject"></param>
        protected void QuitToDesktop(BaseObject clickedObject)
        {
            ScreenManager.Instance.EndGame();
        }

        #endregion
    }
}
