using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace _2DEngine
{
    /// <summary>
    /// Adds a text dialog box to the screen.
    /// </summary>
    public class TextDialogBoxCommand : Command
    {
        #region Properties and Fields

        /// <summary>
        /// The dialog box we create for this command
        /// </summary>
        private TextDialogBox TextBox { get; set; }

        #endregion

        public TextDialogBoxCommand(string text, bool shouldPauseGame = true, float lifeTime = float.MaxValue) : 
            this(new string[1] { text }, shouldPauseGame, lifeTime)
        {

        }

        public TextDialogBoxCommand(List<string> strings, bool shouldPauseGame = true, float lifeTime = float.MaxValue) :
            this(strings.ToArray(), shouldPauseGame, lifeTime)
        {
            
        }

        public TextDialogBoxCommand(string[] strings, bool shouldPauseGame = true, float lifeTime = float.MaxValue) :
            base(shouldPauseGame, lifeTime)
        {
            Debug.Assert(strings.Length > 0);

            TextBox = ParentScreen.AddScreenUIObject(new TextDialogBox(strings, "", ScreenManager.Instance.ScreenCentre), true, true);
            TextBox.Hide();
        }

        #region Virtual Functions

        /// <summary>
        /// When we begin running this command, we show our text box
        /// </summary>
        public override void Begin()
        {
            base.Begin();

            TextBox.Show();
        }

        /// <summary>
        /// If our game is paused, manually handle input for our text box
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        /// <param name="mousePosition"></param>
        public override void HandleInput(float elapsedGameTime, Vector2 mousePosition)
        {
            base.HandleInput(elapsedGameTime, mousePosition);

            if (!GameHandleInput)
            {
                TextBox.HandleInput(elapsedGameTime, mousePosition);
            }
        }

        /// <summary>
        /// If our game is paused, manually update our text box
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public override void Update(float elapsedGameTime)
        {
            base.Update(elapsedGameTime);

            if (!GameUpdate)
            {
                TextBox.Update(elapsedGameTime);
            }

            // When the text box dies, kill this command
            if (!TextBox.IsAlive)
            {
                Die();
            }
        }

        #endregion
    }
}
