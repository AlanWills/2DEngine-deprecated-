using Microsoft.Xna.Framework.Graphics;

namespace _2DEngine
{
    /// <summary>
    /// An abstract class we can use to implement custom behaviour for components in our game.
    /// Basically allows us to simulate multiple inheritance
    /// </summary>
    public abstract class Module : Component
    {
        #region Properties and Fields

        /// <summary>
        /// The object this module is attached to
        /// </summary>
        public Component AttachedComponent { get; set; }

        #endregion

        public Module() { }

        #region Virtual Functions

        /// <summary>
        /// Check our AttachedComponent has been set
        /// </summary>
        public override void Begin()
        {
            base.Begin();

            DebugUtils.AssertNotNull(AttachedComponent);
        }

        /// <summary>
        /// No drawing in modules
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override sealed void Draw(SpriteBatch spriteBatch) { }

        #endregion
    }
}
