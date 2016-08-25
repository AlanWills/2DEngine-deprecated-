namespace _2DEngine
{
    /// <summary>
    /// A command for displaying a text entry dialog box
    /// </summary>
    public class TextEntryCommand : Command
    {
        #region Properties and Fields

        /// <summary>
        /// A reference to the created text control
        /// </summary>
        public TextEntryBox TextEntryDialogBox { get; private set; }

        #endregion

        public TextEntryCommand(TextEntryBox textEntryDialogBox) :
            base()
        {
            TextEntryDialogBox = textEntryDialogBox;
        }

        #region Virtual Functions

        /// <summary>
        /// Updates our text control
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public override void Update(float elapsedGameTime)
        {
            base.Update(elapsedGameTime);

            // Kill this command when we kill the dialog box - not sure if this is the right way to do this at the moment but whatever
            if (!TextEntryDialogBox.IsAlive)
            {
                Die();
            }
        }

        #endregion
    }
}
