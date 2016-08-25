using _2DEngineData;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace _2DEngine
{
    /// <summary>
    /// A class to load and cache all the assets (including XML data).
    /// This will be done on startup in the Game1 LoadContent function
    /// </summary>
    public static class AssetManager
    {
        #region File Paths

        private const string SpriteFontsPath = "SpriteFonts\\";
        private const string SpritesPath = "Sprites\\";
        private const string EffectsPath = "Effects\\";
        private const string DataPath = "Data\\";
        public const string OptionsPath = "Options\\Options.xml";
        public const string PlayerGameDataPath = "Options\\PlayerGameData.xml";

        #endregion

        #region Default Assets

        // UI
        public const string MouseTextureAsset = "UI\\Cursor";
        public const string DefaultSpriteFontAsset = "DefaultSpriteFont";
        public const string StartupLogoTextureAsset = "UI\\Logo";

        // Buttons
        public const string DefaultButtonTextureAsset = "UI\\Button";
        public const string DefaultButtonHighlightedTextureAsset = "UI\\ButtonHighlighted";
        public const string DefaultNarrowButtonTextureAsset = "UI\\NarrowButton";
        public const string DefaultNarrowButtonHighlightedTextureAsset = "UI\\NarrowButtonHighlighted";

        // Text boxes and Dialog boxes
        public const string DefaultTextBoxTextureAsset = "UI\\Menu";

        // Sliders
        public const string DefaultSliderBarTextureAsset = "UI\\SliderBar";
        public const string DefaultSliderHandleTextureAsset = "UI\\BlueSliderDown";

        // Bars
        public const string DefaultBarForegroundTextureAsset = "UI\\BarBackground";
        public const string DefaultBarBackgroundTextureAsset = "UI\\BarBackground";

        // Text Entry box
        public const string DefaultTextEntryBoxTextureAsset = "UI\\TextEntryBox";

        // Panels and Menus
        public const string DefaultEmptyPanelTextureAsset = "UI\\EmptyPanelBackground";
        public const string DefaultMenuTextureAsset = "UI\\Menu";

        // Lights
        public const string DefaultPointLightTextureAsset = "PointLightMask";
        public const string AmbientLightTextureAsset = "AmbientLightMask";

        // Game Objects
        public const string EmptyGameObjectDataAsset = "GameObjects\\Empty.xml";

        #endregion

        #region Properties

        private static Dictionary<string, SpriteFont> SpriteFonts = new Dictionary<string, SpriteFont>();
        private static Dictionary<string, Texture2D> Sprites = new Dictionary<string, Texture2D>();
        private static Dictionary<string, Effect> Effects = new Dictionary<string, Effect>();
        private static Dictionary<string, BaseData> Data = new Dictionary<string, BaseData>();

        #endregion

        /// <summary>
        /// Loads all the assets from the default spritefont, sprites and data directories.
        /// Formats them into dictionaries so that they can be obtained with just the filename (minus the filetype)
        /// </summary>
        /// <param name="content"></param>
        public static void LoadAssets(ContentManager content)
        {
            SpriteFonts = Load<SpriteFont>(content, SpriteFontsPath);
            Sprites = Load<Texture2D>(content, SpritesPath);
            Effects = Load<Effect>(content, EffectsPath);

            LoadData(content);
        }

        /// <summary>
        /// Loads all the assets of an inputted type that exist in our Content folder
        /// </summary>
        /// <typeparam name="T">The type of asset to load</typeparam>
        /// <param name="content">The ContentManager we will use to load our content</param>
        /// <param name="path">The path of the assets we wish to load</param>
        /// <returns>Returns the dictionary of all loading content</returns>
        private static Dictionary<string, T> Load<T>(ContentManager content, string path)
        {
            Dictionary<string, T> objects = new Dictionary<string, T>();

            string directoryPath = content.RootDirectory + "\\" + path;

            if (Directory.Exists(directoryPath))
            {
                string[] files = Directory.GetFiles(directoryPath, "*.xnb*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    // Remove the directoryPath from the start of the string
                    files[i] = files[i].Remove(0, directoryPath.Length);

                    // Remove the .xnb at the end
                    files[i] = files[i].Split('.')[0];

                    try
                    {
                        objects.Add(files[i], LoadFromDisc<T>(path + files[i]));
                    }
                    catch { Debug.Fail("Problem loading asset: " + files[i]); }
                }
            }

            return objects;
        }

        /// <summary>
        /// Loads our data using the IntermediateSerializer.
        /// This is the best thing.
        /// Literally, like, ever.
        /// </summary>
        /// <param name="content"></param>
        private static void LoadData(ContentManager content)
        {
            Data = new Dictionary<string, BaseData>();

            string directoryPath = content.RootDirectory + "\\" + DataPath;

            if (Directory.Exists(directoryPath))
            {
                string[] files = Directory.GetFiles(directoryPath, "*.xml*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    // Remove the Content\\Data\\ from the start
                    files[i] = files[i].Remove(0, 13);

                    // Slow - could probably be improved by inlining the function
                    BaseData data = LoadDataFromDisc<BaseData>(files[i]);

                    try
                    {
                        Data.Add(files[i], data);
                    }
                    catch { Debug.Fail("Adding asset more than once."); }
                }
            }
        }

        /// <summary>
        /// A wrapper for loading content directly using the ContentManager.
        /// Should only be used as a last resort.
        /// </summary>
        /// <typeparam name="T">The type of content to load</typeparam>
        /// <param name="path">The full path of the object from the ContentManager directory e.g. Sprites\\UI\\Cursor</param>
        /// <param name="extension">The extension of the file we are trying to load - used for error checking</param>
        /// <returns>The loaded content</returns>
        private static T LoadFromDisc<T>(string path)
        {
            ContentManager content = ScreenManager.Instance.Content;
            T asset = default(T);

            // Because File.Exists relies on extensions and we do not use extensions, we cannot do a test here.
            // We use try catch here instead.
            // Ouch - I know, but there's no real nice workaround, unless we can check without extensions
            try
            {
                asset = content.Load<T>(path);
            }
            catch
            {
                asset = default(T);
            }
            
            DebugUtils.AssertNotNull(asset);
            return asset;
        }

        /// <summary>
        /// A wrapper for loading xml data directly using the Intermediate Deserialiser
        /// Should only be used as a last resort.
        /// </summary>
        /// <typeparam name="T">The type of data to load</typeparam>
        /// <param name="path">The path of the object e.g. Screens\\BattleScreen</param>
        /// <returns>The loaded content</returns>
        private static T LoadDataFromDisc<T>(string path, bool createIfDoesNotExist = false) where T : BaseData
        {
            ContentManager content = ScreenManager.Instance.Content;
            T data = null;

            if (!File.Exists(content.RootDirectory + "\\" + DataPath + path))
            {
                if (createIfDoesNotExist)
                {
                    // If the file does not exist but we have specified that we should create the data file, we call the constructor of the data file
                    data = (T)Activator.CreateInstance(typeof(T));
                }
                else
                {
                    Debug.Fail("Data file does not exist");
                }
            }
            else
            {
                data = XmlDataSerializer.Deserialize<T>(content.FullRootDirectory() + "\\" + DataPath + path);
            }

            DebugUtils.AssertNotNull(data);
            return data;
        }

        #region Utility Functions

        /// <summary>
        /// Get a loaded SpriteFont
        /// </summary>
        /// <param name="path">The full path of the SpriteFont, e.g. "DefaultSpriteFont"</param>
        /// <returns>Returns the sprite font</returns>
        public static SpriteFont GetSpriteFont(string path)
        {
            SpriteFont spriteFont;

            if (!SpriteFonts.TryGetValue(path, out spriteFont))
            {
                spriteFont = LoadFromDisc<SpriteFont>(SpriteFontsPath + path);
            }

            DebugUtils.AssertNotNull(spriteFont);
            return spriteFont;
        }

        /// <summary>
        /// Get a pre-loaded sprite
        /// </summary>
        /// <param name="path">The full path of the Sprite, e.g. "UI\\Cursor"</param>
        /// <returns>Returns the texture</returns>
        public static Texture2D GetSprite(string path)
        {
            Texture2D sprite;

            if (!Sprites.TryGetValue(path, out sprite))
            {
                sprite = LoadFromDisc<Texture2D>(SpritesPath + path);
            }

            DebugUtils.AssertNotNull(sprite);
            return sprite;
        }

        /// <summary>
        /// Get a pre-loaded effect
        /// </summary>
        /// <param name="path">The full path of the Effect, e.g. "LightEffect"</param>
        /// <returns>Returns the effect</returns>
        public static Effect GetEffect(string path)
        {
            Effect effect;

            if (!Effects.TryGetValue(path, out effect))
            {
                effect = LoadFromDisc<Effect>(EffectsPath + path);
            }

            DebugUtils.AssertNotNull(effect);
            return effect;
        }

        /// <summary>
        /// Get a pre-loaded data file.
        /// If it cannot be accessed, it loads it from disc.
        /// </summary>
        /// <typeparam name="T">The type we wish to case the data to</typeparam>
        /// <param name="name">The full path of the data file relative to the Data directory, e.g. Screens\\BaseScreenData.xml</param>
        /// <returns>Returns the data</returns>
        public static T GetData<T>(string name, bool createIfDoesNotExist = false) where T : BaseData
        {
            BaseData data = null;

            if (Data.TryGetValue(name, out data))
            {
                Debug.Assert(data is T);
                return data as T;
            }

            return LoadDataFromDisc<T>(name, createIfDoesNotExist);
        }

        /// <summary>
        /// Returns a list of all the key value pairs in our dictionary who's data is castable to type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> GetAllDataOfType<T>() where T : BaseData
        {
            List<T> data = new List<T>();

            foreach (KeyValuePair<string, BaseData> registeredPair in Data)
            {
                if (registeredPair.Value is T)
                {
                    data.Add(registeredPair.Value as T);
                }
            }

            return data;
        }

        /// <summary>
        /// Returns a list of all the key value pairs in our dictionary who's data is castable to type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<KeyValuePair<string, T>> GetAllDataPairsOfType<T>() where T : BaseData
        {
            List<KeyValuePair<string, T>> data = new List<KeyValuePair<string, T>>();

            foreach (KeyValuePair<string, BaseData> registeredPair in Data)
            {
                if (registeredPair.Value is T)
                {
                    data.Add(new KeyValuePair<string, T>(registeredPair.Key, registeredPair.Value as T));
                }
            }

            return data;
        }

        /// <summary>
        /// Saves data to an XML file on disc.
        /// </summary>
        /// <typeparam name="T">THe type of data we wish to save</typeparam>
        /// <param name="data">The data we are saving</param>
        /// <param name="path">The full path of where to save the data e.g. "Screens\\MainMenuScreen.xml"</param>
        public static void SaveData<T>(T data, string path) where T : BaseData
        {
            DebugUtils.AssertNotNull(data);
            XmlDataSerializer.Serialize(data, ScreenManager.Instance.Content.FullRootDirectory() + "\\" + DataPath + path);
        }

        #endregion

        #region Migration

        /// <summary>
        /// When publishing, the data files are saved elsewhere.
        /// We need to retrieve them and move them to the correct location.
        /// </summary>
        public static void MigrateDataIfRequired(ContentManager content)
        {
            // If our data folder already exists we don't migrate
            if (Directory.Exists(content.RootDirectory + "\\" + DataPath))
            {
                return;
            }

            // Find our content directory
            string contentFullPath = Path.GetFullPath(content.RootDirectory);
            DirectoryInfo contentDirectoryInfo = new DirectoryInfo(contentFullPath);

            string twoLevelsUp = contentDirectoryInfo.Parent.Parent.FullName;

            // Find any other data folders and move the data into our main content directory.
            DirectoryInfo info = new DirectoryInfo(twoLevelsUp);
            foreach (DirectoryInfo directory in info.GetDirectories("Data", SearchOption.AllDirectories))
            {
                if (directory.FullName.Contains(contentFullPath))
                {
                    continue;
                }

                directory.MoveTo(contentDirectoryInfo + "\\Data");
            }
        }

        #endregion
    }
}