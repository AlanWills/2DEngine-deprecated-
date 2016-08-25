using Microsoft.Xna.Framework;

namespace _2DEngine
{
    /// <summary>
    /// A simple class which represents a UIObject and a title
    /// </summary>
    public class UIObjectBox : UIObject
    {
        #region Properties and Fields

        /// <summary>
        /// The title of the dialog box
        /// </summary>
        public Label Title { get; set; }

        /// <summary>
        /// The UIObject we wish to display
        /// </summary>
        private UIObject uiObject;
        public UIObject UIObject
        {
            get { return uiObject; }
            protected set
            {
                if (value != null)
                {
                    // Don't want to reset this object
                    DebugUtils.AssertNull(UIObject);

                    // Only do this if our value is not null otherwise we're boned
                    AddChild(value);
                    uiObject = value;
                }
            }
        }

        /// <summary>
        /// The padding on each side
        /// </summary>
        protected float xPadding = 5;
        protected float yPadding = 5;

        #endregion

        public UIObjectBox(string title, Vector2 localPosition, string textureAsset = AssetManager.DefaultTextBoxTextureAsset) :
            this(null, title, localPosition, textureAsset)
        {

        }

        public UIObjectBox(UIObject mainUIObject, string title, Vector2 localPosition, string textureAsset = AssetManager.DefaultTextBoxTextureAsset) :
            base(localPosition, textureAsset)
        {
            UIObject = mainUIObject;
            UsesCollider = false;

            Title = AddChild(new Label(title, Anchor.kTop, 1));
            Title.Colour = Color.White;
        }
    }
}
