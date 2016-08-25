using Microsoft.Xna.Framework.Content;
using System.Threading;
using _2DEngineData;

namespace _2DEngine
{
    /// <summary>
    /// A delegate we can use to add load behaviour
    /// </summary>
    /// <param name="content"></param>
    public delegate void AssetLoadingEvent(ContentManager content);

    /// <summary>
    /// A delegate we can use to add save behaviour
    /// </summary>
    public delegate void AssetSavingEvent();

    /// <summary>
    /// A screen to be displayed on start up whilst we load content.
    /// Very simple - just displays a logo and possibly a background.
    /// </summary>
    public class StartupLogoScreen : MenuScreen
    {
        #region Properties and Fields

        /// <summary>
        /// An event used to load assets - can add extra events to call custom load events
        /// </summary>
        public static event AssetLoadingEvent LoadAssets;

        /// <summary>
        /// The screen we wish to transition to after we have finished loading
        /// </summary>
        private BaseScreen ScreenAfterLoading { get; set; }

        #endregion

        public StartupLogoScreen(BaseScreen screenAfterLoading, string screenDataAsset = "Screens\\StartupLogoScreen.xml") :
            base(screenDataAsset)
        {
            ScreenAfterLoading = screenAfterLoading;

            LoadAssets += AssetManager.LoadAssets;
            LoadAssets += MusicManager.LoadAssets;
            LoadAssets += SFXManager.LoadAssets;
        }

        #region Virtual Functions

        /// <summary>
        /// Adds the startup logo.
        /// </summary>
        protected override void AddInitialUI()
        {
            base.AddInitialUI();

            AddScreenUIObject(new Logo());
        }

        /// <summary>
        /// Make sure we create the data file if it does not exist so our game does not crash when running from a third party like a build server
        /// </summary>
        /// <returns></returns>
        protected override BaseScreenData LoadScreenData()
        {
            return AssetManager.GetData<BaseScreenData>("Screens\\StartupLogoScreen.xml", true);
        }

        /// <summary>
        /// Creates a thread to load the content.
        /// </summary>
        public override void Begin()
        {
            base.Begin();

            ThreadManager.CreateThread(LoadAllAssetsCallback, TransitionCallback);
        }

        #endregion

        /// <summary>
        /// A callback for our loading thread to load all our game's assets.
        /// </summary>
        private void LoadAllAssetsCallback()
        {
            ContentManager content = ScreenManager.Instance.Content;

            DebugUtils.AssertNotNull(content);
            DebugUtils.AssertNotNull(LoadAssets);   // LoadAssets should REALLY PROBABLY not be null, unless we decide to not load ANYTHING
            LoadAssets(content);

            ScreenAfterLoading.LoadContent();
            ScreenAfterLoading.Initialise();
        }

        /// <summary>
        /// A callback for our loading thread to complete when finished loading.
        /// It will transition to the next screen.
        /// </summary>
        private void TransitionCallback()
        {
            Transition(ScreenAfterLoading, false, false);
        }
    }
}