using Microsoft.Xna.Framework;

namespace _2DEngine
{
    /// <summary>
    /// A command which displays a flashing label for an inputted amount of time
    /// </summary>
    public class FlashingTextCommand : Command
    {
        #region Properties and Fields

        /// <summary>
        /// A reference to the label we will create
        /// </summary>
        private FlashingLabel Label { get; set; }

        #endregion

        public FlashingTextCommand(string text, Vector2 localPosition, Color labelColour, bool shouldPauseGame = false, float lifeTime = float.MaxValue) :
            base(shouldPauseGame, lifeTime)
        {
            // Create and add to our parent screen here
            Label = ParentScreen.AddScreenUIObject(new FlashingLabel(text, localPosition), true, true);
            Label.Colour = labelColour;
        }

        #region Virtual Functions
        
        /// <summary>
        /// Update our label if we have paused the game.
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public override void Update(float elapsedGameTime)
        {
            base.Update(elapsedGameTime);

            if (!GameUpdate)
            {
                Label.Update(elapsedGameTime);
            }
        }

        /// <summary>
        /// Kills our label when this command is done
        /// </summary>
        public override void Die()
        {
            base.Die();

            Label.Die();
        }

        #endregion
    }
}