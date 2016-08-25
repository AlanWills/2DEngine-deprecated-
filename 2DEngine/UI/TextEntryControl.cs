using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace _2DEngine
{
    /// <summary>
    /// A class which holds a string value and responds to input from the Keyboard to update it
    /// </summary>
    public class TextEntryControl : ClickableImage
    {
        #region Properties and Fields

        /// <summary>
        /// A constant list of all the valid keys and their string representation
        /// </summary>
        public Dictionary<Keys, string> ValidKeys = new Dictionary<Keys, string>()
        {
            { Keys.A, "A" },
            { Keys.B, "B" },
            { Keys.C, "C" },
            { Keys.D, "D" },
            { Keys.E, "E" },
            { Keys.F, "F" },
            { Keys.G, "G" },
            { Keys.H, "H" },
            { Keys.I, "I" },
            { Keys.J, "J" },
            { Keys.K, "K" },
            { Keys.L, "L" },
            { Keys.M, "M" },
            { Keys.N, "N" },
            { Keys.O, "O" },
            { Keys.P, "P" },
            { Keys.Q, "Q" },
            { Keys.R, "R" },
            { Keys.S, "S" },
            { Keys.T, "T" },
            { Keys.U, "U" },
            { Keys.V, "V" },
            { Keys.W, "W" },
            { Keys.X, "X" },
            { Keys.Y, "Y" },
            { Keys.Z, "Z" },
            {Keys.Space, " " },
        };

        /// <summary>
        /// The label which will draw the current string.
        /// Do not want to expose this as public otherwise we will be able to change the string value from outside of this class.
        /// </summary>
        private Label Label { get; set; }

        /// <summary>
        /// The current value of the string.
        /// </summary>
        public string Text
        {
            get { return Label.Text; }
            private set { Label.Text = value; }
        }

        /// <summary>
        /// A flag to indicate whether we should change the string or not.
        /// Will be set to true when clicked on and set to false when enter is pressed or clicked off.
        /// </summary>
        private bool HasFocus { get; set; }

        private static Color FocusColour = Color.Red;
        private static Color NonFocusColour = Color.White;

        #endregion

        public TextEntryControl(string startingString, Vector2 localPosition, string textureAsset = AssetManager.DefaultTextEntryBoxTextureAsset) :
            this(startingString, Vector2.Zero, localPosition, textureAsset)
        {

        }

        public TextEntryControl(string startingString, Vector2 size, Vector2 localPosition, string textureAsset = AssetManager.DefaultTextEntryBoxTextureAsset) :
            base(size, localPosition, textureAsset)
        {
            // Position in the centre of the box
            Label = AddChild(new Label(startingString, Vector2.Zero));
            Label.Colour = Color.White;

            // Update our click callbacks
            ClickableModule.OnLeftClicked += OnLeftClicked_HasFocus;

            HasFocus = true;
            Colour = FocusColour;
        }

        #region Virtual Functions
        
        /// <summary>
        /// Update the label using the input from the keyboard.
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        /// <param name="mousePosition"></param>
        public override void HandleInput(float elapsedGameTime, Vector2 mousePosition)
        {
            base.HandleInput(elapsedGameTime, mousePosition);

            UpdateFocus();

            Colour = HasFocus ? FocusColour : NonFocusColour;

            if (HasFocus)
            {
                HandleKeyboardInput();
            }
        }
        
        #endregion

        #region Input Utility Functions

        /// <summary>
        /// Performs the logic for updating the focus of this based on the keyboard and mouse.
        /// </summary>
        private void UpdateFocus()
        {
            // See if the mouse has been clicked.
            bool leftMouseClicked = GameMouse.Instance.IsClicked(MouseButton.kLeftButton);

            if (!HasFocus)
            {
                // If we do not have focus and the left mouse has not been clicked, we cannot change focus so just quit
                if (!leftMouseClicked)
                {
                    return;
                }

                // Clicking on is dealt with by click callbacks
            }
            else
            {
                // If we have focus and enter is pressed, we no longer have focus and should return
                if (GameKeyboard.Instance.IsKeyPressed(Keys.Enter))
                {
                    HasFocus = false;
                    return;
                }
                else if (!Collider.IsClicked && leftMouseClicked)
                {
                    // The mouse has been clicked, but not on use so lose focus
                    HasFocus = false;
                    return;
                }
            }
        }

        /// <summary>
        /// Handles any input from the keyboard.
        /// </summary>
        private void HandleKeyboardInput()
        {
            // While we have a non-empty string and the backspace key is down, delete the text character by character
            if (GameKeyboard.Instance.IsKeyPressed(Keys.Back))
            {
                if (Text.Length > 0)
                {
                    // Remove the last character
                    Text = Text.Remove(Text.Length - 1);
                }
            }
            else
            {
                foreach (KeyValuePair<Keys, string> keyPair in ValidKeys)
                {
                    if (GameKeyboard.Instance.IsKeyPressed(keyPair.Key))
                    {
                        Text = Text + keyPair.Value;
                    }
                }
            }
        }

        #endregion

        #region Click Callbacks

        /// <summary>
        /// Sets this as having focus when we left click on it with the mouse.
        /// </summary>
        /// <param name="baseObject"></param>
        private void OnLeftClicked_HasFocus(BaseObject baseObject)
        {
            HasFocus = true;
        }

        #endregion
    }
}
