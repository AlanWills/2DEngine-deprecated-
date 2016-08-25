using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// A class which represents text in a dialog box.
    /// Auto wraps the inputted text using the dimensions and padding of the box.
    /// </summary>
    public class TextDialogBox : UIObjectBox
    {
        #region Properties and Fields

        /// <summary>
        /// The strings this dialog box will show.
        /// </summary>
        public string[] Strings { get; private set; }

        /// <summary>
        /// The button to move to the next string in our list which will be displayed.
        /// </summary>
        private Button NextStringButton { get; set; }

        /// <summary>
        /// The button to move to the previous string in our list which will be displayed.
        /// </summary>
        private Button PreviousStringButton { get; set; }

        /// <summary>
        /// The button to close the dialog box.
        /// </summary>
        private Button DoneButton { get; set; }

        /// <summary>
        /// The index in our array for the string we are currently displaying.
        /// When it is set, it rebuilds our UI and wraps the next string
        /// </summary>
        private int currentStringIndex;
        private int CurrentStringIndex
        {
            get { return currentStringIndex; }
            set
            {
                DebugUtils.AssertNotNull(Strings);
                Debug.Assert(value >= 0 && value < Strings.Length);
                currentStringIndex = value;
                Text.Text = Strings[currentStringIndex];

                ValidateUI();
                WrapString();
            }
        }

        /// <summary>
        /// The label for the current string
        /// </summary>
        private Label Text { get; set; }

        #endregion

        public TextDialogBox(string text, string title, Vector2 localPosition, string textureAsset = AssetManager.DefaultMenuTextureAsset) :
            this(new string[1] { text }, title, localPosition, textureAsset)
        {

        }

        public TextDialogBox(List<string> strings, string title, Vector2 localPosition, string textureAsset = AssetManager.DefaultMenuTextureAsset) :
            this(strings.ToArray(), title, localPosition, textureAsset)
        {

        }

        public TextDialogBox(string[] strings, string title, Vector2 localPosition, string textureAsset = AssetManager.DefaultMenuTextureAsset) :
            base(new Label(strings[0], Vector2.Zero), title, localPosition, textureAsset)
        {
            // Base class sets UsesCollider = false

            Strings = strings;
            Text = UIObject as Label;
            Text.Colour = Color.White;
            Size = new Vector2(400, 200);

            xPadding = Size.X * 0.15f;
            yPadding = Size.Y * 0.15f;
        }

        #region Virtual Functions

        /// <summary>
        /// Sets up our UI
        /// </summary>
        public override void LoadContent()
        {
            CheckShouldLoad();

            Vector2 buttonSize = new Vector2(90, 45);

            NextStringButton = AddChild(new Button("Next", buttonSize, Vector2.Zero, AssetManager.DefaultNarrowButtonTextureAsset, AssetManager.DefaultNarrowButtonHighlightedTextureAsset));
            NextStringButton.ClickableModule.OnLeftClicked += NextString;

            PreviousStringButton = AddChild(new Button("Previous", buttonSize, Vector2.Zero, AssetManager.DefaultNarrowButtonTextureAsset, AssetManager.DefaultNarrowButtonHighlightedTextureAsset));
            PreviousStringButton.ClickableModule.OnLeftClicked += PreviousString;

            DoneButton = AddChild(new Button("Done", buttonSize, Vector2.Zero, AssetManager.DefaultNarrowButtonTextureAsset, AssetManager.DefaultNarrowButtonHighlightedTextureAsset));
            DoneButton.ClickableModule.OnLeftClicked += CloseDialog;

            // This will trigger a validation
            CurrentStringIndex = 0;

            base.LoadContent();
        }

        /// <summary>
        /// Fixup the positions of some of the buttons
        /// </summary>
        public override void Begin()
        {
            base.Begin();

            PreviousStringButton.LocalPosition = new Vector2(-(Size.X + PreviousStringButton.Size.X) * 0.5f + xPadding, (Size.Y + PreviousStringButton.Size.Y) * 0.5f - yPadding);
            NextStringButton.LocalPosition = new Vector2(0, (Size.Y + NextStringButton.Size.Y) * 0.5f - yPadding);
            DoneButton.LocalPosition = new Vector2((Size.X + DoneButton.Size.X) * 0.5f - xPadding, (Size.Y + DoneButton.Size.Y) * 0.5f - yPadding);
        }

        /// <summary>
        /// Make sure that when we show this box we still validate the buttons.
        /// </summary>
        /// <param name="showChildren"></param>
        public override void Show(bool showChildren = true)
        {
            base.Show(showChildren);

            ValidateUI();
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Take our label and uses the size and padding of this box to ensure it's text fits by wrapping it over several lines.
        /// </summary>
        /// <param name="stringToWrap"></param>
        private void WrapString()
        {
            float margin = Size.X - 2 * xPadding;

            if (SpriteFont.MeasureString(Text.Text).X > margin)
            {
                string[] tokens = Text.Text.Split(' ');
                string newString = "";
                string currentLine = "";

                foreach (string token in tokens)
                {
                    if (SpriteFont.MeasureString(currentLine + token).X < margin)
                    {
                        if (currentLine != "")
                        {
                            currentLine += " " + token;
                        }
                        else
                        {
                            currentLine = token;
                        }
                    }
                    else
                    {
                        newString += currentLine;
                        currentLine = "\n" + token;
                    }
                }

                Text.Text = newString + currentLine;
            }

            // Store the new wrapped string so that if we recalculate it we will not need to do the wrapping again
            Strings[currentStringIndex] = Text.Text;
        }

        /// <summary>
        /// Our previous and next buttons need to be turned on and off based on whether we are at the start or end of our string array.
        /// </summary>
        private void ValidateUI()
        {
            if (CurrentStringIndex == 0)
            {
                PreviousStringButton.Hide();
            }
            else
            {
                PreviousStringButton.Show();
            }

            if (CurrentStringIndex == Strings.Length - 1)
            {
                NextStringButton.Hide();
            }
            else
            {
                NextStringButton.Show();
            }
        }

        #endregion

        #region Callbacks

        /// <summary>
        /// Move to the next string 
        /// </summary>
        /// <param name="clickedObject"></param>
        private void NextString(BaseObject clickedObject)
        {
            CurrentStringIndex++;
        }

        /// <summary>
        /// Move to the previous string
        /// </summary>
        /// <param name="clickedObject"></param>
        private void PreviousString(BaseObject clickedObject)
        {
            CurrentStringIndex--;
        }

        /// <summary>
        /// Closes the dialog.
        /// </summary>
        /// <param name="clickedObject"></param>
        private void CloseDialog(BaseObject clickedObject)
        {
            Die();
        }

        #endregion
    }
}
