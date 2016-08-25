using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace _2DEngine
{
    /// <summary>
    /// A dialog box for text entry
    /// </summary>
    public class TextEntryBox : UIObjectBox
    {
        #region Properties and Fields

        /// <summary>
        /// A reference to our main UIObject stored as a reference to a TextEntryControl - makes life easier
        /// </summary>
        private TextEntryControl TextEntryControl { get; set; }

        /// <summary>
        /// The button to confirm our text entry and close the box
        /// </summary>
        public Button ConfirmButton { get; private set; }

        /// <summary>
        /// The button which discards our changes and closes the box
        /// </summary>
        public Button CancelButton { get; private set; }

        /// <summary>
        /// The text from our text entry control
        /// </summary>
        public string Text { get { return TextEntryControl.Text; } }

        private static Vector2 padding = new Vector2(5, 5);

        #endregion

        public TextEntryBox(string startingString, string title, Vector2 localPosition, string textureAsset = AssetManager.DefaultMenuTextureAsset) :
            base(new TextEntryControl(startingString, Vector2.Zero), title, localPosition, textureAsset)
        {
            TextEntryControl = UIObject as TextEntryControl;

            ConfirmButton = AddChild(new Button("Confirm", Vector2.Zero));
            ConfirmButton.ClickableModule.OnLeftClicked += CloseDialogBox;
            ConfirmButton.ClickableModule.LeftClickAccelerator = Keys.Enter;

            CancelButton = AddChild(new Button("Cancel", Vector2.Zero));
            CancelButton.ClickableModule.OnLeftClicked += CloseDialogBox;
        }

        #region Virtual Functions

        /// <summary>
        /// Performs some UI fixup
        /// </summary>
        public override void Initialise()
        {
            CheckShouldInitialise();

            // It is possible that size is set manually in another class
            // If not, we just wrap the dialog box around the various controls
            bool sizeNotSet = Size == Vector2.Zero;

            base.Initialise();

            if (sizeNotSet)
            {
                Vector2 size = new Vector2();

                // This looks a bit bonkers, but all it's doing is finding the maximum possible dimensions based on the separate controls
                size.X = Math.Max(Math.Max(ConfirmButton.Size.X + CancelButton.Size.X, Title.Size.X), TextEntryControl.Size.X) + 2 * padding.X;
                size.Y = 2 * Math.Max(ConfirmButton.Size.Y, Title.Size.Y) + TextEntryControl.Size.Y + Title.Size.Y + 4 * padding.Y;

                Size = size;
            }

            TextEntryControl.LocalPosition = new Vector2(0, -Size.Y * 0.25f);
            ConfirmButton.LocalPosition = new Vector2(-ConfirmButton.Size.X * 0.5f, Size.Y * 0.5f - ConfirmButton.Size.Y - padding.Y);
            CancelButton.LocalPosition = ConfirmButton.LocalPosition + new Vector2((ConfirmButton.Size.X + CancelButton.Size.X) * 0.5f, 0);
            Title.LocalPosition = TextEntryControl.LocalPosition - new Vector2(0, padding.Y + (Title.Size.Y + TextEntryControl.Size.Y) * 0.5f);
        }

        #endregion

        #region Properties and Fields

        /// <summary>
        /// Kills this dialog box
        /// </summary>
        /// <param name="baseObject"></param>
        private void CloseDialogBox(BaseObject baseObject)
        {
            Die();
        }

        #endregion
    }
}
