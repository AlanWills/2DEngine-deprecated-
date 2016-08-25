using System.Threading;

namespace _2DEngine
{
    /// <summary>
    /// An intermediary class used for moving to a gameplay screen.
    /// Is added automatically when we call Transition from the ScreenManager
    /// </summary>
    public class LoadingScreen : BaseScreen
    {
        #region Properties and Fields

        /// <summary>
        /// The screen we wish to transition to after this loading screen is completed
        /// </summary>
        private GameplayScreen ScreenToTransitionTo { get; set; }

        #endregion

        public LoadingScreen(GameplayScreen screenToTransitionTo, string screenDataAsset = "Screens\\LoadingScreen.xml") :
            base(screenDataAsset)
        {
            ScreenToTransitionTo = screenToTransitionTo;
            MusicQueueType = QueueType.PlayImmediately;
        }

        #region Virtual Functions

        /// <summary>
        /// Transition to the gameplay screen.
        /// Because the screenmanager will call load during Transition,
        /// this screen hides the pause whilst we load the assets.
        /// </summary>
        public override void Begin()
        {
            base.Begin();

            ThreadManager.CreateThread(LoadScreenCallback, TransitionCallback);
        }

        #endregion

        /// <summary>
        /// A callback for our loading thread to load and initialise our next screen.
        /// </summary>
        private void LoadScreenCallback()
        {
            ScreenToTransitionTo.LoadContent();
            ScreenToTransitionTo.Initialise();
        }

        /// <summary>
        /// A callback for our loading thread to transition to the next screen when completed loading.
        /// </summary>
        private void TransitionCallback()
        {
            Transition(ScreenToTransitionTo, false, false);
        }
    }
}
