using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace _2DEngine
{
    /// <summary>
    /// A wrapper class for the XNA SoundEffect which uses the SFXManager when playing.
    /// </summary>
    public class CustomSoundEffect
    {
        #region Properties and Fields

        /// <summary>
        /// A value between 0 and 1 representing the volume of the sound effect.
        /// </summary>
        private float volume;
        public float Volume
        {
            get { return volume; }
            set
            {
                volume = MathHelper.Clamp(value, 0, 1);
            }
        }

        /// <summary>
        /// A reference to our actual sound effect.
        /// </summary>
        private SoundEffect SoundEffect { get; set; }

        #endregion

        public CustomSoundEffect(string soundEffectAsset)
        {
            SoundEffect = SFXManager.GetSoundEffect(soundEffectAsset);
            Volume = 1;
        }

        #region Utility Functions

        /// <summary>
        /// Play the sound effect.
        /// </summary>
        public void Play()
        {
            SoundEffect.Play(Volume * OptionsManager.SFXVolume, 0, 0);
        }

        #endregion
    }
}