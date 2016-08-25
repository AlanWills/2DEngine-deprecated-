using _2DEngineData;
using Microsoft.Xna.Framework;

namespace _2DEngine
{
    /// <summary>
    /// A simple game object class which implements the AnimationModule
    /// </summary>
    public class AnimatedGameObject : GameObject
    {
        #region Properties and Fields

        /// <summary>
        /// The animation module for our animation
        /// </summary>
        public AnimationModule AnimationModule { get; private set; }

        #endregion

        public AnimatedGameObject(Vector2 localPosition, string animationDataAsset) :
            base(localPosition, animationDataAsset)
        {
            UsesCollider = false;
            AnimationModule = AddModule(new AnimationModule());
        }

        // If we want to implement adding size, then we need to work out a nice way of scaling the input size by the number of frames
        /*public AnimatedGameObject(Vector2 size, Vector2 localPosition, string animationDataAsset) :
            base(localPosition, animationDataAsset)
        {
            Size = size;
            
        }*/

        #region Virtual Functions

        /// <summary>
        /// Begins playing our animation
        /// </summary>
        public override void Begin()
        {
            base.Begin();

            AnimationModule.IsPlaying = true;
        }

        #endregion
    }
}
