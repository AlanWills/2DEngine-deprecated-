using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// A singleton class which is responsible for managing the screens in game
    /// </summary>
    public class ScreenManager : ObjectManager<BaseScreen>
    {
        #region Properties and Fields

        /// <summary>
        /// We will only have one instance of this class, so have a static Instance which can be accessed
        /// anywhere in the program by calling ScreenManager.Instance
        /// </summary>
        private static ScreenManager instance;
        public static ScreenManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ScreenManager();
                }
                return instance;
            }
        }

        /// <summary>
        /// A reference to our game.  Used really for exiting.
        /// </summary>
        private Game Game { get; set; }

        /// <summary>
        /// The SpriteBatch we will use for all our rendering.
        /// It will be used in three rendering loops:
        /// Game Objects - objects we want to be affected by our camera position
        /// In Game UI Objects - UI objects we want to be affected by our camera position
        /// Screen UI Objects - UI Objects we want to be independent of our camera position
        /// </summary>
        public SpriteBatch SpriteBatch { get; private set; }

        /// <summary>
        /// The device we can use to load content.
        /// Not really used as we use the AssetManager to obtain all of our Content instead.
        /// </summary>
        public ContentManager Content { get; private set; }

        /// <summary>
        /// The Graphics Device we can use to change display settings.
        /// Not really used except at startup and during Options changes.
        /// </summary>
        public GraphicsDeviceManager GraphicsDeviceManager { get; private set; }

        /// <summary>
        /// The Viewport for our game window - can be used to access screen dimensions
        /// </summary>
        public Viewport Viewport { get; private set; }

        /// <summary>
        /// A wrapper property to return the Viewport's width and height
        /// </summary>
        public Vector2 ScreenDimensions { get; private set; }

        /// <summary>
        /// A wrapper property to return the centre of the screen
        /// </summary>
        public Vector2 ScreenCentre { get; private set; }

        /// <summary>
        /// A reference to the last child in the manager.
        /// </summary>
        public BaseScreen CurrentScreen
        {
            get
            {
                return LastChild<BaseScreen>();
            }
        }

        /// <summary>
        /// An event handler we will call just before we close down to save any assets
        /// </summary>
        public event AssetSavingEvent SaveAssets;

        #endregion

        /// <summary>
        /// Constructor is private because this class will be accessed through the static 'Instance' property
        /// </summary>
        protected ScreenManager() :
            base()
        {
            SaveAssets += OptionsManager.SaveAssets;
        }

        #region Virtual Functions

        /// <summary>
        /// Loads the game mouse and any screens already added
        /// </summary>
        public override void LoadContent()
        {
            CheckShouldLoad();

            AssetManager.MigrateDataIfRequired(Content);

            DebugUtils.AssertNotNull(Content);
            OptionsManager.LoadAssets(Content);
            CommandManager.Instance.LoadContent();
            InputManager.Instance.LoadContent();

            base.LoadContent();
        }

        /// <summary>
        /// Initialises the camera and game mouse
        /// </summary>
        public override void Initialise()
        {
            CheckShouldInitialise();

            CommandManager.Instance.Initialise();
            ThreadManager.Initialise();
            Camera.Initialise();
            InputManager.Instance.Initialise();

            base.Initialise();
        }

        /// <summary>
        /// Updates the keyboard and mouse.
        /// Handles input for Camera and screens in ScreenManager.
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        /// <param name="mousePosition">The screen position of the mouse</param>
        public override void HandleInput(float elapsedGameTime, Vector2 mousePosition)
        {
            // Update the input manager first
            InputManager.Instance.HandleInput(elapsedGameTime, mousePosition);

            // Then handle camera input
            Camera.HandleInput(elapsedGameTime);

            // Handle input for all of the scripts in our script manager
            CommandManager.Instance.HandleInput(elapsedGameTime, mousePosition);

            // Then finally handle screen input
            base.HandleInput(elapsedGameTime, mousePosition);
        }

        /// <summary>
        /// Update the camera and then any screens
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public override void Update(float elapsedGameTime)
        {
            InputManager.Instance.Update(elapsedGameTime);

            // Updates Music manager
            MusicManager.Update();

            // Updates Thread manager
            ThreadManager.Update();

            // Update camera
            Camera.Update(elapsedGameTime);

            // Then update any screens
            base.Update(elapsedGameTime);

            // Update ScriptManager scripts
            CommandManager.Instance.Update(elapsedGameTime);

            // Deflush the mouse after all input handling and updating
            // If the draw logic depends on this, it is just wrong
            GameMouse.Instance.IsFlushed = false;
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Sets up class variables from the main Game1 class which will be useful for our game.
        /// Loads options from XML.
        /// MUST be called before LoadContent and Initialise.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch from our Game1 class</param>
        /// <param name="viewport">The Viewport corresponding to the window</param>
        public void Setup(Game game, SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            // Check that we have called this before loading and initialising
            CheckShouldLoad();
            CheckShouldInitialise();

            Game = game;
            SpriteBatch = spriteBatch;
            Content = game.Content;
            Viewport = game.GraphicsDevice.Viewport;
            GraphicsDeviceManager = graphics;

            ScreenDimensions = new Vector2(Viewport.Width, Viewport.Height);
            ScreenCentre = ScreenDimensions * 0.5f;

            // Set our game to update on a fixed time step
            Game.IsFixedTimeStep = true;
        }

        /// <summary>
        /// Remove the current screen and adds another.
        /// If we are transitioning to a gameplay screen, we add a loading screen first (unless we are going from a loading screen).
        /// </summary>
        /// <param name="transitionTo">The screen to add</param>
        /// <param name="load">Whether we should call LoadContent on the screen to add</param>
        /// <param name="initialise">Whether we should call Initialise on the screen to add</param>
        public BaseScreen Transition(BaseScreen transitionTo, bool load = true, bool initialise = true)
        {
            DebugUtils.AssertNotNull(CurrentScreen);

            return Transition(CurrentScreen, transitionTo, load, initialise);
        }

        /// <summary>
        /// Remove one screen and add another.
        /// If we are transitioning to a gameplay screen, we add a loading screen first (unless we are going from a loading screen).
        /// </summary>
        /// <param name="transitionFrom">The screen to remove</param>
        /// <param name="transitionTo">The screen to add</param>
        /// <param name="load">Whether we should call LoadContent on the screen to add</param>
        /// <param name="initialise">Whether we should call Initialise on the screen to add</param>
        /// <returns>The screen we are adding next</returns>
        public BaseScreen Transition(BaseScreen transitionFrom, BaseScreen transitionTo, bool load = true, bool initialise = true)
        {
            BaseScreen newScreen = null;
            
            if (transitionTo is GameplayScreen && !(transitionFrom is LoadingScreen))
            {
                newScreen = AddChild(new LoadingScreen(transitionTo as GameplayScreen), true, true);
            }
            else
            {
                newScreen = AddChild(transitionTo, load, initialise);
            }

            transitionFrom.Die();
            transitionFrom.ShouldDraw = true;     // Bit of a hack so that we get a continuous draw until the new screen takes over

            DebugUtils.AssertNotNull(newScreen);
            return newScreen;
        }

        /// <summary>
        /// Adds a startup logo screen and kicks off asset loading
        /// </summary>
        /// <param name="screenAfterLoading">The screen we wish to display after the StartupLogoScreen</param>
        public void StartGame(BaseScreen screenAfterLoading)
        {
            AddChild(new StartupLogoScreen(screenAfterLoading), true, true);
        }

        /// <summary>
        /// Closes the game and saves any necessary data
        /// </summary>
        public void EndGame()
        {
            SaveAssets?.Invoke();

            Game.Exit();
        }

        /// <summary>
        /// A wrapper for returning the current screen as the inputted type.
        /// Performs Debug validity checking.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetCurrentScreenAs<T>() where T : BaseScreen
        {
            Debug.Assert(CurrentScreen is T);
            return CurrentScreen as T;
        }

        #endregion
    }
}
