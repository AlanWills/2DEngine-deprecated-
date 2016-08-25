using _2DEngineData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// A base abstract class for all screens in our game.
    /// Contains three managers, for the GameObjects, In Game UIObjects and Screen UIObjects.
    /// Is responsible for drawing the mouse.
    /// An instance of this class cannot be created because it is too general
    /// </summary>
    public abstract class BaseScreen : Component, IModuleCompatible
    {
        #region Properties and Fields

        /// <summary>
        /// The render target we will use to draw our lighting.
        /// </summary>
        private RenderTarget2D LightRenderTarget { get; set; }

        /// <summary>
        /// The render target we will use to draw our game world and ultimately our final scene.
        /// </summary>
        private RenderTarget2D GameWorldRenderTarget { get; set; }

        /// <summary>
        /// The render target we will use to draw our In Game UI.
        /// </summary>
        private RenderTarget2D InGameUIRenderTarget { get; set; }

        /// <summary>
        /// The render target we will use to draw our Screen UI
        /// </summary>
        private RenderTarget2D ScreenUIRenderTarget { get; set; }

        /// <summary>
        /// A Manager for all the lights in the game.
        /// </summary>
        protected LightManager Lights { get; private set; }

        /// <summary>
        /// A Manager for all the In Game background UI Objects (that will appear behind the Game Objects) which constitute the level environment
        /// </summary>
        protected ObjectManager<BaseObject> EnvironmentObjects { get; private set; }

        /// <summary>
        /// A Manager for the GameObjects in our screen
        /// </summary>
        protected ObjectManager<BaseObject> GameObjects { get; private set; }

        /// <summary>
        /// A Manager for the In Game (camera dependent) UI Objects in our screen (that will appear in front of the Game Objects)
        /// </summary>
        protected ObjectManager<BaseObject> InGameUIObjects { get; private set; }

        /// <summary>
        /// A Manager for the Screen (camera independent) UI Objects in our screen
        /// </summary>
        protected ObjectManager<BaseObject> ScreenUIObjects { get; private set; }

        /// <summary>
        /// An object manager for the modules of this object
        /// </summary>
        private ObjectManager<Module> Modules { get; set; }

        /// <summary>
        /// A string for the xml data file for this screen.
        /// </summary>
        protected string ScreenDataAsset { get; private set; }

        /// <summary>
        /// A property for the data for this screen.  In screens that inherit from BaseScreen, this could be a custom data class.
        /// </summary>
        private BaseScreenData screenData;
        protected BaseScreenData ScreenData
        {
            get
            {
                if (screenData == null)
                {
                    screenData = LoadScreenData();
                }

                return screenData;
            }
        }

        /// <summary>
        /// The screen background
        /// </summary>
        private Image Background { get; set; }

        /// <summary>
        /// Returns the dimensions of the game window
        /// </summary>
        protected Vector2 ScreenDimensions
        {
            get { return ScreenManager.Instance.ScreenDimensions; }
        }

        /// <summary>
        /// Returns the centre of the game window
        /// </summary>
        protected Vector2 ScreenCentre
        {
            get { return ScreenManager.Instance.ScreenCentre; }
        }

        /// <summary>
        /// A variable used to determine whether this screen should queue it's screen music or not.
        /// Set to 'WaitForCurrent' to queue songs after the music already playing (DEFAULT).
        /// Set to 'PlayImmediately to clear current queued songs and play this screen's music.
        /// </summary>
        protected QueueType MusicQueueType { private get; set; }

        #endregion

        public BaseScreen(string screenDataAsset) : 
            base()
        {
            ScreenDataAsset = screenDataAsset;
            Name = GetType().Name;        // By default we set the name of this screen to be it's class name - useful for finding specific screens if we need them

            GraphicsDevice graphicsDevice = ScreenManager.Instance.GraphicsDeviceManager.GraphicsDevice;
            var pp = graphicsDevice.PresentationParameters;

            LightRenderTarget = new RenderTarget2D(graphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);
            GameWorldRenderTarget = new RenderTarget2D(graphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);
            InGameUIRenderTarget = new RenderTarget2D(graphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);
            ScreenUIRenderTarget = new RenderTarget2D(graphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);

            Lights = new LightManager();
            EnvironmentObjects = new ObjectManager<BaseObject>();
            GameObjects = new ObjectManager<BaseObject>();
            InGameUIObjects = new ObjectManager<BaseObject>();
            ScreenUIObjects = new ObjectManager<BaseObject>();
            Modules = new ObjectManager<Module>();

            MusicQueueType = QueueType.WaitForCurrent;
        }

        // Do three drawing steps here rather than in screen manager
        // Draw mouse

        #region Virtual Functions

        /// <summary>
        /// Called in the LoadContent loop, before we load the Manager classes.
        /// Use this function to add any initial UI which will then get loaded and initialised.
        /// Sets up the background if the BackgroundDataAsset has been set.
        /// </summary>
        protected virtual void AddInitialUI()
        {
            DebugUtils.AssertNotNull(ScreenData);

            if (!string.IsNullOrEmpty(ScreenData.BackgroundTextureAsset))
            {
                // Don't add this for updating etc. just draw it
                Background = new Image(ScreenDimensions, ScreenCentre, ScreenData.BackgroundTextureAsset);
                Background.PreservesAspectRatio = false;    // Want to make sure the background takes up the whole screen dimension
                Background.LoadContent();
                Background.Initialise();
            }
        }

        /// <summary>
        /// Adds initial game objects to our screen.
        /// </summary>
        protected virtual void AddInitialGameObjects() { }

        /// <summary>
        /// Adds initial lights to our screen.
        /// </summary>
        protected virtual void AddInitialLights() { }

        /// <summary>
        /// A function which loads the screen data of a certain type.
        /// Can be overridden to load screen data of a type different to BaseScreenData.
        /// </summary>
        /// <returns></returns>
        protected virtual BaseScreenData LoadScreenData()
        {
            return AssetManager.GetData<BaseScreenData>(ScreenDataAsset);
        }

        /// <summary>
        /// Creates GameObjects, Lights and then UI and then calls LoadContent on the screen Managers.
        /// </summary>
        public override void LoadContent()
        {
            // Check if we should load
            CheckShouldLoad();

            // Create the objects then lights first as our UI will probably depend on their values
            AddInitialGameObjects();
            GameObjects.LoadContent();

            AddInitialLights();

            AddInitialUI();
            // Should move this to before this - currently screwing up because we add the lights from the generation engine in AddInitialUI - probably should not do this
            Lights.LoadContent();
            EnvironmentObjects.LoadContent();
            InGameUIObjects.LoadContent();
            ScreenUIObjects.LoadContent();

            Modules.LoadContent();

            base.LoadContent();
        }

        /// <summary>
        /// Calls Initialise on the screen Managers and ScriptManager.
        /// Adds Initial Scripts to the ScriptManager
        /// </summary>
        public override void Initialise()
        {
            CheckShouldInitialise();

            Lights.Initialise();
            EnvironmentObjects.Initialise();
            GameObjects.Initialise();
            InGameUIObjects.Initialise();
            ScreenUIObjects.Initialise();

            Modules.Initialise();

            base.Initialise();
        }

        /// <summary>
        /// Adds initial commands during the Begin function for this screen.
        /// </summary>
        protected virtual void AddInitialCommands() { }

        /// <summary>
        /// Queues up any music for this screen
        /// </summary>
        public override void Begin()
        {
            base.Begin();

            Lights.Begin();
            EnvironmentObjects.Begin();
            GameObjects.Begin();
            InGameUIObjects.Begin();
            ScreenUIObjects.Begin();

            Modules.Begin();

            AddMusic();
            AddInitialCommands();
        }

        /// <summary>
        /// Adds music to the screen.
        /// Can be overridden to clear current music queue etc.
        /// </summary>
        protected virtual void AddMusic()
        {
            MusicManager.SetPlaylist(ScreenData.Music, MusicQueueType);
        }

        /// <summary>
        /// Call HandleInput on the screen managers.
        /// </summary>
        /// <param name="elapsedGameTime">The time in seconds since the last frame</param>
        /// <param name="mousePosition">The current screen space position of the mouse</param>
        public override void HandleInput(float elapsedGameTime, Vector2 mousePosition)
        {
            base.HandleInput(elapsedGameTime, mousePosition);

            Vector2 gameMouseCoords = Camera.ScreenToGameCoords(mousePosition);

            if (ScreenUIObjects.ShouldHandleInput) { ScreenUIObjects.HandleInput(elapsedGameTime, mousePosition); }
            if (InGameUIObjects.ShouldHandleInput) { InGameUIObjects.HandleInput(elapsedGameTime, gameMouseCoords); }
            if (GameObjects.ShouldHandleInput) { GameObjects.HandleInput(elapsedGameTime, gameMouseCoords); }
            if (EnvironmentObjects.ShouldHandleInput) { EnvironmentObjects.HandleInput(elapsedGameTime, gameMouseCoords); }

            if (Modules.ShouldHandleInput) { Modules.HandleInput(elapsedGameTime, mousePosition); }
        }

        /// <summary>
        /// Call Update on the screen managers and check their visibility against the camera viewport.
        /// </summary>
        /// <param name="elapsedGameTime">The time in seconds since the last frame</param>
        public override void Update(float elapsedGameTime)
        {
            base.Update(elapsedGameTime);

            if (Lights.ShouldUpdate) { Lights.Update(elapsedGameTime); }
            if (EnvironmentObjects.ShouldUpdate) { EnvironmentObjects.Update(elapsedGameTime); }
            if (GameObjects.ShouldUpdate) { GameObjects.Update(elapsedGameTime); }
            if (InGameUIObjects.ShouldUpdate) { InGameUIObjects.Update(elapsedGameTime); }
            if (ScreenUIObjects.ShouldUpdate) { ScreenUIObjects.Update(elapsedGameTime); }

            if (Modules.ShouldUpdate) { Modules.Update(elapsedGameTime); }

            /*Camera.CheckVisibility(Lights, false);
            Camera.CheckVisibility(EnvironmentObjects, false);
            Camera.CheckVisibility(GameObjects, false);
            Camera.CheckVisibility(InGameUIObjects, false);
            Camera.CheckVisibility(ScreenUIObjects, true);*/
        }

        /// <summary>
        /// Draws the background first.
        /// Calls draw on the screen objects in the order: Lights, Background, GameObjects, InGameUIObjects, ScreenUIObjects.
        /// Draws the mouse at the after everything else.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch we should use for drawing sprites</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            GraphicsDevice graphicsDevice = ScreenManager.Instance.GraphicsDeviceManager.GraphicsDevice;

            // Draw our lighting
            {
                graphicsDevice.SetRenderTarget(LightRenderTarget);

                if (Lights.ShouldDraw)
                {
                    graphicsDevice.Clear(Color.Black);

                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null, Camera.TransformationMatrix);
                    Lights.Draw(spriteBatch);
                    spriteBatch.End();
                }
                else
                {
                    graphicsDevice.Clear(Color.White);
                }
            }

            // Draw our game world
            {
                graphicsDevice.SetRenderTarget(GameWorldRenderTarget);
                graphicsDevice.Clear(Color.White);

                if (Background != null)
                {
                    spriteBatch.Begin();
                    {
                        if (Background.ShouldDraw) { Background.Draw(spriteBatch); }
                    }

                    spriteBatch.End();
                }

                // Draw the camera dependent objects using the camera transformation matrix
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, Camera.TransformationMatrix);
                {
                    if (EnvironmentObjects.ShouldDraw) { EnvironmentObjects.Draw(spriteBatch); }
                    if (GameObjects.ShouldDraw) { GameObjects.Draw(spriteBatch); }
                }

                spriteBatch.End();
            }

            // Draw our InGameUI
            {
                graphicsDevice.SetRenderTarget(InGameUIRenderTarget);
                graphicsDevice.Clear(Color.Transparent);

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, Camera.TransformationMatrix);
                {
                    if (InGameUIObjects.ShouldDraw) { InGameUIObjects.Draw(spriteBatch); }
                }

                spriteBatch.End();
            }

            // Draw our Screen UI
            {
                graphicsDevice.SetRenderTarget(ScreenUIRenderTarget);
                graphicsDevice.Clear(Color.Transparent);

                // Draw the camera independent objects and the mouse last
                spriteBatch.Begin();
                {
                    if (ScreenUIObjects.ShouldDraw) { ScreenUIObjects.Draw(spriteBatch); }
                    if (GameMouse.Instance.ShouldDraw) { GameMouse.Instance.Draw(spriteBatch); }
                }

                spriteBatch.End();
            }

            // Combine the separate render targets together using the appropriate effects
            {
                graphicsDevice.SetRenderTarget(null);
                graphicsDevice.Clear(Color.Black);

                // Combine the Light and Game World render targets
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                Lights.LightEffect.Parameters["lightMask"].SetValue(LightRenderTarget);
                Lights.LightEffect.Parameters["ambientLight"].SetValue(Lights.AmbientLight.ToVector4());
                Lights.LightEffect.CurrentTechnique.Passes[0].Apply();
                spriteBatch.Draw(GameWorldRenderTarget, Vector2.Zero, Color.White);
                spriteBatch.End();

                // Draw our UI render targets
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                spriteBatch.Draw(InGameUIRenderTarget, Vector2.Zero, Color.White);
                spriteBatch.Draw(ScreenUIRenderTarget, Vector2.Zero, Color.White);
                spriteBatch.End();
            }
        }

        /// <summary>
        /// Call die on our ObjectManagers
        /// </summary>
        public override void Die()
        {
            base.Die();

            EnvironmentObjects.Die();
            GameObjects.Die();
            InGameUIObjects.Die();
            ScreenUIObjects.Die();

            Modules.Die();
        }

        #endregion

        #region IModuleCompatible Functions

        /// <summary>
        /// Adds a module to this screen and sets up the AttachedComponent on the module.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="module"></param>
        /// <param name="load"></param>
        /// <param name="initialise"></param>
        /// <returns></returns>
        public T AddModule<T>(T module, bool load = false, bool initialise = false) where T : Module
        {
            // Set up reference to attached component
            module.AttachedComponent = this;

            // Checks to make sure we are not adding another copy of the same type of module - should only have one
            Debug.Assert(!Modules.Exists(x => x.GetType() == typeof(T)));
            return Modules.AddChild(module, load, initialise);
        }

        /// <summary>
        /// A function for finding a module registered with this screen.
        /// We are guaranteed to only have one module of each type registered.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FindModule<T>() where T : Module
        {
            Debug.Assert(Modules.Exists(x => x.GetType() == typeof(T)));
            return Modules.FindChild<T>(x => x.GetType() == typeof(T));
        }

        #endregion

        #region Functions for Managing Objects

        /// <summary>
        /// Adds a Light to this screen's Lights manager
        /// </summary>
        /// <param name="lightToAdd">The light to add</param>
        /// <param name="load">A flag to indicate whether LoadContent should be called on this light when adding</param>
        /// <param name="initialise">A flag to indicate whether Initialise should be called on this light when adding</param>
        public T AddLight<T>(T lightToAdd, bool load = false, bool initialise = false) where T : Light
        {
            return Lights.AddChild(lightToAdd, load, initialise);
        }

        /// <summary>
        /// Removes a Light from this screen's Lights manager
        /// </summary>
        /// <param name="lightToRemove">The light to remove</param>
        public void RemoveLight(Light lightToRemove)
        {
            Lights.RemoveChild(lightToRemove);
        }

        /// <summary>
        /// Finds a Light in this screen's Lights manager
        /// </summary>
        /// <typeparam name="K">The type that we wish to return the found light as</typeparam>
        /// <param name="backgroundObjectName">The predicate we will use to find our light</param>
        /// <returns>Returns the found light casted to type K, or null</returns>
        public K FindLight<K>(Predicate<Light> predicate) where K : Light
        {
            return Lights.FindChild<K>(predicate);
        }

        /// <summary>
        /// Adds a UIObject to this screen's EnvironmentObjects manager
        /// </summary>
        /// <param name="environmentObjectToAdd">The object to add</param>
        /// <param name="load">A flag to indicate whether LoadContent should be called on this object when adding</param>
        /// <param name="initialise">A flag to indicate whether Initialise should be called on this object when adding</param>
        public T AddEnvironmentObject<T>(T environmentObjectToAdd, bool load = false, bool initialise = false) where T : BaseObject
        {
            return EnvironmentObjects.AddChild(environmentObjectToAdd, load, initialise);
        }

        /// <summary>
        /// Removes a UIObject from this screen's EnvironmentObjects manager
        /// </summary>
        /// <param name="environmentObjectToRemove">The environment object to remove</param>
        public void RemoveEnvironmentObject(BaseObject environmentObjectToRemove)
        {
            EnvironmentObjects.RemoveChild(environmentObjectToRemove);
        }

        /// <summary>
        /// Finds a UIObject in this screen's EnvironmentObjects manager
        /// </summary>
        /// <typeparam name="K">The type that we wish to return the found object as</typeparam>
        /// <param name="envrionmentObjectToRemove">The predicate we will use to find our UIObject</param>
        /// <returns>Returns the found object casted to type K, or null</returns>
        public K FindEnvironmentObject<K>(Predicate<BaseObject> predicate) where K : BaseObject
        {
            return EnvironmentObjects.FindChild<K>(predicate);
        }

        /// <summary>
        /// Adds a GameObject to this screen's GameObjects manager
        /// </summary>
        /// <param name="gameObjectToAdd">The object to add</param>
        /// <param name="load">A flag to indicate whether LoadContent should be called on this object when adding</param>
        /// <param name="initialise">A flag to indicate whether Initialise should be called on this object when adding</param>
        public T AddGameObject<T>(T gameObjectToAdd, bool load = false, bool initialise = false) where T : BaseObject
        {
            return GameObjects.AddChild(gameObjectToAdd, load, initialise);
        }

        /// <summary>
        /// Removes a GameObject from this screen's GameObjects manager
        /// </summary>
        /// <param name="gameObjectToRemove">The game object to remove</param>
        public void RemoveGameObject(BaseObject gameObjectToRemove)
        {
            GameObjects.RemoveChild(gameObjectToRemove);
        }

        /// <summary>
        /// Finds a GameObject in this screen's GameObjects manager
        /// </summary>
        /// <typeparam name="K">The type that we wish to return the found object as</typeparam>
        /// <param name="predicate">The predicate we will use to find our object</param>
        /// <returns>Returns the found object casted to type K, or null</returns>
        protected K FindGameObject<K>(Predicate<BaseObject> predicate) where K : BaseObject
        {
            return GameObjects.FindChild<K>(predicate);
        }

        /// <summary>
        /// Adds an InGameUIObject to this screen's InGameUIObjects manager
        /// </summary>
        /// <param name="uiObjectToAdd">The object to add</param>
        /// <param name="load">A flag to indicate whether LoadContent should be called on this object when adding</param>
        /// <param name="initialise">A flag to indicate whether Initialise should be called on this object when adding</param>
        public T AddInGameUIObject<T>(T uiObjectToAdd, bool load = false, bool initialise = false) where T : BaseObject
        {
            return InGameUIObjects.AddChild(uiObjectToAdd, load, initialise);
        }

        /// <summary>
        /// Removes an InGameUIObject from this screen's InGameUIObjects manager
        /// </summary>
        /// <param name="uiObjectToRemove"></param>
        public void RemoveInGameUIObject(BaseObject uiObjectToRemove)
        {
            InGameUIObjects.RemoveChild(uiObjectToRemove);
        }

        /// <summary>
        /// Finds an InGameUIObject in this screen's InGameUIObjects manager
        /// </summary>
        /// <typeparam name="K">The type we wish to return the found object as</typeparam>
        /// <param name="predicate">The predicate we will use to find our UIObject</param>
        /// <returns>Returns the found object casted to type K, or null</returns>
        public K FindInGameUIObject<K>(Predicate<BaseObject> predicate) where K : BaseObject
        {
            return InGameUIObjects.FindChild<K>(predicate);
        }

        /// <summary>
        /// Extracts an InGameUIObject in this screen's InGameUIObjects manager without killing it
        /// </summary>
        /// <typeparam name="K">The type we wish to return the found object as</typeparam>
        /// <param name="uiObjectName">The predicate we will use to find our UIObject</param>
        /// <returns>Returns he found object casted to type K, or null</returns>
        public K ExtractInGameUIObject<K>(K childToExtract) where K : BaseObject
        {
            return InGameUIObjects.ExtractChild(childToExtract);
        }

        /// <summary>
        /// Adds a UIObject to this screen's ScreenUIObjects manager
        /// </summary>
        /// <param name="uiObjectToAdd">The object to add</param>
        /// <param name="load">A flag to indicate whether LoadContent should be called on this object when adding</param>
        /// <param name="initialise">A flag to indicate whether Initialise should be called on this object when adding</param>
        public T AddScreenUIObject<T>(T uiObjectToAdd, bool load = false, bool initialise = false) where T : BaseObject
        {
            return ScreenUIObjects.AddChild(uiObjectToAdd, load, initialise);
        }

        /// <summary>
        /// Removes a ScreenUIObject from this screen's ScreenUIObjects manager
        /// </summary>
        /// <param name="uiObjectToRemove"></param>
        public void RemoveScreenUIObject(BaseObject uiObjectToRemove)
        {
            ScreenUIObjects.RemoveChild(uiObjectToRemove);
        }

        /// <summary>
        /// Finds a ScreenUIObject in this screen's ScreenObjects manager
        /// </summary>
        /// <typeparam name="K">The type we wish to return the found object as</typeparam>
        /// <param name="uiObjectName">The predicate we will use to find our UIObject</param>
        /// <returns>Returns he found object casted to type K, or null</returns>
        public K FindScreenUIObject<K>(Predicate<BaseObject> predicate) where K : BaseObject
        {
            return ScreenUIObjects.FindChild<K>(predicate);
        }

        /// <summary>
        /// Extracts a ScreenUIObject in this screen's ScreenObjects manager without killing it
        /// </summary>
        /// <typeparam name="K">The type we wish to return the found object as</typeparam>
        /// <param name="uiObjectName">The predicate we will use to find our UIObject</param>
        /// <returns>Returns he found object casted to type K, or null</returns>
        public K ExtractScreenUIObject<K>(K childToExtract) where K : BaseObject
        {
            return ScreenUIObjects.ExtractChild(childToExtract);
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Calls the ScreenManager Transition function.  Moves from the current screen to the inputted screen.
        /// </summary>
        /// <param name="screenToTransitionTo">The screen to transition to</param>
        /// <param name="load">Whether we should call LoadContent on the screen</param>
        /// <param name="initialise">Whether we should call Initialise on the screen</param>
        /// <returns>The new screen we are adding</returns>
        public BaseScreen Transition(BaseScreen screenToTransitionTo, bool load = true, bool initialise = true)
        {
            return ScreenManager.Instance.Transition(this, screenToTransitionTo, load, initialise);
        }

        /// <summary>
        /// Adds a command to the CommandManager
        /// </summary>
        /// <param name="script"></param>
        /// <param name="load">Calls Load on the command</param>
        /// <param name="initialise">Calls Initialise on the command</param>
        protected T AddCommand<T>(T script, Command previousCommand = null, bool load = true, bool initialise = true) where T : Command
        {
            script.PreviousCommand = previousCommand;
            //script.ParentScreen = this;
            return CommandManager.Instance.AddChild(script, load, initialise);
        }

        #endregion
    }
}