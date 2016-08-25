using _2DEngineData;
using Microsoft.Xna.Framework;

namespace _2DEngine
{
    /// <summary>
    /// A particle emitter
    /// </summary>
    public class ParticleEmitter : UIObject
    {
        // Start and end size
        // Start and end colour
        // Size variation
        // Life span
        // Life span variation
        // Velocity
        // Emit rate

        #region Properties and Fields

        /// <summary>
        /// The beginning size for an emitted particle
        /// </summary>
        public Vector2 StartSize { get; set; }

        /// <summary>
        /// The end size for an emitted particle
        /// </summary>
        public Vector2 EndSize { get; set; }

        /// <summary>
        /// The variation of our end size
        /// </summary>
        public Vector2 SizeVariation { get; set; }

        /// <summary>
        /// The velocity of our particle
        /// </summary>
        public Vector2 Velocity { get; set; }

        /// <summary>
        /// The variation in the velocity of our particle
        /// </summary>
        public Vector2 VelocityVariation { get; set; }

        /// <summary>
        /// The start colour of our particle
        /// </summary>
        public Color StartColour { get; set; }

        /// <summary>
        /// The end colour of our particle
        /// </summary>
        public Color EndColour { get; set; }

        /// <summary>
        /// The life time of a particle
        /// </summary>
        public float ParticleLifeTime { get; set; }

        /// <summary>
        /// The variation in the life time of a particle
        /// </summary>
        public float ParticleLifeTimeVariation { get; set; }

        /// <summary>
        /// The time delay between emitting particles
        /// </summary>
        public float EmitTimer { get; set; }

        /// <summary>
        /// The data asset for an emitted particle
        /// </summary>
        private string ParticleDataAsset { get; set; }

        /// <summary>
        /// The cached animation data for this object
        /// </summary>
        private AnimationData AnimationData { get; set; }

        private float currentEmitTimer = 0;

        #endregion

        public ParticleEmitter(
            Vector2 startSize, 
            Vector2 endSize, 
            Vector2 sizeVariation,
            Vector2 velocity,
            Vector2 velocityVariation,
            Color startColour,
            Color endColour,
            float particleLifeTime,
            float particleLifeTimeVariation,
            float emitTimer,
            Vector2 localPosition, 
            string particleDataAsset) :
            base(localPosition, AssetManager.DefaultEmptyPanelTextureAsset)
        {
            StartSize = startSize;
            EndSize = endSize;
            SizeVariation = sizeVariation;
            Velocity = velocity;
            VelocityVariation = velocityVariation;
            StartColour = startColour;
            EndColour = endColour;
            ParticleLifeTime = particleLifeTime;
            ParticleLifeTimeVariation = particleLifeTimeVariation;
            EmitTimer = emitTimer;
            ParticleDataAsset = particleDataAsset;

            currentEmitTimer = EmitTimer;
        }

        #region Virtual Functions

        /// <summary>
        /// Cache the animation data for our particles
        /// </summary>
        public override void LoadContent()
        {
            CheckShouldLoad();

            AnimationData = AssetManager.GetData<AnimationData>(ParticleDataAsset);

            base.LoadContent();
        }

        /// <summary>
        /// Emits particles when our timer has reached a certain amount
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public override void Update(float elapsedGameTime)
        {
            base.Update(elapsedGameTime);

            currentEmitTimer += elapsedGameTime;
            while (currentEmitTimer >= EmitTimer)
            {
                EmitParticle();
                currentEmitTimer -= EmitTimer;
            }
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Creates a new particle
        /// </summary>
        private void EmitParticle()
        {
            DebugUtils.AssertNotNull(AnimationData);

            float extraLifeTime = MathUtils.GenerateFloat(0, ParticleLifeTimeVariation);
            Vector2 frames = new Vector2(AnimationData.TextureFramesX, AnimationData.TextureFramesY);

            Particle particle = AddChild(new Particle(StartSize * frames, Vector2.Zero, AnimationData, ParticleLifeTime + extraLifeTime), true, true);
            particle.EndSize = (EndSize + new Vector2(MathUtils.GenerateFloat(0, SizeVariation.X), MathUtils.GenerateFloat(0, SizeVariation.X))) * frames;
            particle.Colour = StartColour;
            particle.EndColour = EndColour;
            particle.Velocity = Velocity + new Vector2(MathUtils.GenerateFloat(-VelocityVariation.X, VelocityVariation.X), MathUtils.GenerateFloat(0, VelocityVariation.Y));
        }

        #endregion
    }
}