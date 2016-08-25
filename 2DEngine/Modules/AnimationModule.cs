using _2DEngineData;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// A delegate used for events after animations have completed.
    /// </summary>
    public delegate void OnAnimationCompleteHandler();

    public class AnimationModule : GameObjectModule
    {
        #region Animation Properties

        /// <summary>
        /// The pixel dimensions of one frame of the animation
        /// </summary>
        private Point FrameDimensions { get; set; }

        /// <summary>
        /// The centre we should pass to the object that is using this animation.
        /// Set only once at loading.
        /// </summary>
        public Vector2 Centre { get; private set; }

        /// <summary>
        /// The number of frames in the sprite sheet
        /// </summary>
        private Point Frames { get; set; }

        /// <summary>
        /// The current frame of the animation
        /// </summary>
        private int CurrentFrame { get; set; }

        /// <summary>
        /// The time each frame is displayed before moving on
        /// </summary>
        public float TimePerFrame { get; set; }

        /// <summary>
        /// A flag to indicate whether the animation is playing
        /// </summary>
        public bool IsPlaying { get; set; }

        /// <summary>
        /// A flag to indicate whether this animation should keep playing or just play once
        /// </summary>
        public bool Continual { get; set; }

        /// <summary>
        /// A flag used for our state machine to indicate whether this animation should be reachable from any state.
        /// </summary>
        public bool IsGlobal { get; private set; }

        /// <summary>
        /// A flag to indicate whether the animation is completed
        /// </summary>
        public bool Finished { get; private set; }

        /// <summary>
        /// A Vector2 to apply to keep the animation centred.
        /// </summary>
        public Vector2 AnimationFixup { get; private set; }

        public Vector2 ColliderCentreOffset { get; private set; }
        public Vector2 ColliderDimensions { get; private set; }

        /// <summary>
        /// Can only be used by non continual animations.
        /// Used to perform a function after the animation has completed.
        /// </summary>
        public event OnAnimationCompleteHandler OnAnimationComplete;

        private float currentTimeOnFrame = 0;
        private const float defaultTimePerFrame = 0.05f;

        #endregion

        public AnimationModule() :
            base()
        {
            IsPlaying = false;
            CurrentFrame = 0;
        }

        #region Virtual Functions
        
        /// <summary>
        /// Finalises all the information the animation needs
        /// </summary>
        public override void LoadContent()
        {
            Debug.Assert(AttachedGameObject is GameObject);
            Debug.Assert(AttachedGameObject.Data is AnimationData);
            AnimationData data = AttachedGameObject.Data as AnimationData;

            Frames = new Point(data.TextureFramesX, data.TextureFramesY);
            Continual = data.Continual;
            IsGlobal = data.IsGlobal;
            AnimationFixup = new Vector2(data.FixupX, data.FixupY);

            TimePerFrame = defaultTimePerFrame;

            base.LoadContent();
        }

        /// <summary>
        /// Updates some of the properties on the GameObject
        /// </summary>
        public override void Initialise()
        {
            base.Initialise();

            Debug.Assert(AttachedGameObject is GameObject);
            Debug.Assert(AttachedGameObject.Data is AnimationData);
            AnimationData data = AttachedGameObject.Data as AnimationData;

            Debug.Assert(AttachedGameObject.TextureCentre != Vector2.Zero);
            Vector2 textureDimensions = AttachedGameObject.TextureCentre * 2;

            FrameDimensions = new Point((int)(textureDimensions.X / Frames.X), (int)(textureDimensions.Y / Frames.Y));
            ColliderCentreOffset = new Vector2(data.ColliderCentrePositionOffsetX * FrameDimensions.X, data.ColliderCentrePositionOffsetY * FrameDimensions.Y);
            ColliderDimensions = new Vector2(FrameDimensions.X * data.ColliderWidthProportion, FrameDimensions.Y * data.ColliderHeightProportion);

            AttachedGameObject.TextureCentre = new Vector2(FrameDimensions.X * 0.5f, FrameDimensions.Y * 0.5f);

            CalculateSourceRectangle();
        }

        /// <summary>
        /// Updates the time on the current animation frame and the animation frame if needs be
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public override void Update(float elapsedGameTime)
        {
            base.Update(elapsedGameTime);

            if (IsPlaying)
            {
                currentTimeOnFrame += elapsedGameTime;

                if (currentTimeOnFrame >= TimePerFrame)
                {
                    // We have changed frame so recalculate the source rectangle
                    CurrentFrame++;

                    // If the animation should only play once, we kill the object it was attached to once it reaches the last frame
                    if (!Continual)
                    {
                        if (CurrentFrame == Frames.X * Frames.Y - 1)
                        {
                            Finished = true;
                            IsPlaying = false;

                            if (OnAnimationComplete != null)
                            {
                                OnAnimationComplete();
                            }

                            AttachedBaseObject.Die();

                            return;
                        }
                    }
                    else
                    {
                        CurrentFrame %= Frames.X * Frames.Y;
                    }

                    CalculateSourceRectangle();

                    currentTimeOnFrame = 0;
                }
            }
            else
            {
                CurrentFrame = 0;
            }
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Calculates the source rectangle from the sprite sheet we should use based on the dimensions and current frame.
        /// </summary>
        private void CalculateSourceRectangle()
        {
            int currentRow = CurrentFrame / Frames.X;
            int currentColumn = CurrentFrame % Frames.X;

            Debug.Assert(currentColumn < Frames.X);
            Debug.Assert(currentRow < Frames.Y);
            DebugUtils.AssertNotNull(AttachedBaseObject);

            AttachedBaseObject.SourceRectangle = new Rectangle(currentColumn * FrameDimensions.X, currentRow * FrameDimensions.Y, FrameDimensions.X, FrameDimensions.Y);
        }

        /// <summary>
        /// Resets the animation to make it ready to play again.
        /// </summary>
        public void Reset()
        {
            currentTimeOnFrame = 0;
            IsPlaying = false;
            CurrentFrame = 0;
            Finished = false;

            CalculateSourceRectangle();
        }

        #endregion
    }
}
