using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace _2DEngine
{
    public static class SFXManager
    {
        #region Const File Paths

        private const string SFXFilePath = "\\SFX";

        #endregion

        #region Properties and Fields

        /// <summary>
        /// A dictionary of sound effect names to loaded sound effects
        /// </summary>
        private static Dictionary<string, SoundEffect> SoundEffects { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads all the sound effects from our SFX directory and stores them in a dictionary for further querying
        /// </summary>
        /// <param name="content"></param>
        public static void LoadAssets(ContentManager content)
        {
            SoundEffects = new Dictionary<string, SoundEffect>();

            try
            {
                if (Directory.Exists(content.RootDirectory + SFXFilePath))
                {
                    string[] sfxFiles = Directory.GetFiles(content.RootDirectory + SFXFilePath, ".", SearchOption.AllDirectories);
                    for (int i = 0; i < sfxFiles.Length; i++)
                    {
                        // Remove the Content\\ from the start
                        sfxFiles[i] = sfxFiles[i].Remove(0, 8);

                        // Remove the .xnb at the end
                        sfxFiles[i] = sfxFiles[i].Split('.')[0];

                        // Remove the SFX\\ from the start
                        string key = sfxFiles[i].Remove(0, 4);

                        if (!SoundEffects.ContainsKey(key))
                        {
                            SoundEffects.Add(key, content.Load<SoundEffect>(sfxFiles[i]));
                        }
                    }
                }
            }
            catch { Debug.Fail("Serious failure in SFXManager loading SFX."); }
        }
        
        /// <summary>
        /// Returns the stored sound effect with the inputted key
        /// </summary>
        /// <param name="sfxName"></param>
        /// <returns></returns>
        public static SoundEffect GetSoundEffect(string sfxName)
        {
            Debug.Assert(SoundEffects.ContainsKey(sfxName));
            return SoundEffects[sfxName];
        }

        /// <summary>
        /// Returns an instance of the sound effect with the inputted key
        /// </summary>
        /// <param name="sfxName"></param>
        /// <returns></returns>
        public static SoundEffectInstance CreateInstance(string sfxName)
        {
            return GetSoundEffect(sfxName).CreateInstance();
        }

        #endregion
    }
}
