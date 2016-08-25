using _2DEngineData;
using Microsoft.Xna.Framework;

namespace _2DEngine
{
    /// <summary>
    /// A class used in the ParticleEmitter which handles size and colour changing
    /// </summary>
    public class Particle : AnimatedGameObject
    {
        #region Properties and Fields

        private Vector2 StartSize { get; set; }
        public Vector2 EndSize { private get; set; }
        private Color StartColour { get; set; }
        public Color EndColour { private get; set; }
        public Vector2 Velocity { private get; set; }

        private float LerpAmount { get; set; }

        /// <summary>
        /// A reference to the animation data we will use to drive this particle
        /// </summary>
        private AnimationData AnimationData { get; set; }

        #endregion

        public Particle(Vector2 startSize, Vector2 localPosition, AnimationData animationData, float lifeTime) :
            base(localPosition, "")
        {
            UsesCollider = false;
            LerpAmount = 0;
            StartColour = Colour;
            StartSize = startSize;

            DebugUtils.AssertNotNull(animationData);
            AnimationData = animationData;
        }

        #region Virtual Functions
        
        /// <summary>
        /// Updates the size, colour and position of our particle
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public override void Update(float elapsedGameTime)
        {
            base.Update(elapsedGameTime);

            LerpAmount = MathHelper.Clamp(LerpAmount + elapsedGameTime, 0, 1);

            Colour = Color.Lerp(StartColour, EndColour, LerpAmount);
            Size = Vector2.Lerp(StartSize, EndSize, LerpAmount);

            LocalPosition += Velocity * elapsedGameTime;
        }

        #endregion
    }
}
