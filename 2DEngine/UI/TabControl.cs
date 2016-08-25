using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// A UI control for filtering different UI objects with a tab system.
    /// This UI merely refers to the actual tabs - the panels, menus etc. are added to it.
    /// Therefore it's position and size correspond to the position and size of the tabs and the added objects are positioned relative to the tabs.
    /// </summary>
    public class TabControl : UIObject
    {
        #region Properties and Fields

        /// <summary>
        /// A list of references to our tabs
        /// </summary>
        private List<ClickableImage> Tabs { get; set; }

        /// <summary>
        /// The list of the objects that are hidden/shown by the tabs
        /// </summary>
        public List<BaseObject> TabbedObjects
        {
            get
            {
                List<BaseObject> tabbedObjects = new List<BaseObject>();

                foreach (Button button in Tabs)
                {
                    Debug.Assert(button.StoredObject is BaseObject);
                    tabbedObjects.Add((button.StoredObject as BaseObject));
                }

                return tabbedObjects;
            }
        }

        /// <summary>
        /// Determines the dimensions of our tabs.
        /// By default set to (100, Size.Y);
        /// </summary>
        public Vector2 TabSize { get; set; }

        #endregion

        public TabControl(Vector2 sizeOfTabsSection, Vector2 localPosition, string textureAsset = AssetManager.DefaultEmptyPanelTextureAsset) :
            base(sizeOfTabsSection, localPosition, textureAsset)
        {
            Tabs = new List<ClickableImage>();
            TabSize = new Vector2(100, sizeOfTabsSection.Y);
            UsesCollider = false;
        }

        #region Virtual Functions

        /// <summary>
        /// Hide all the panels except the first one
        /// </summary>
        public override void Begin()
        {
            base.Begin();

            // If we have added objects to our tab control, we show the first tab object
            if (Tabs.Count > 0)
            {
                SwitchObject(Tabs[0]);
            }
        }

        /// <summary>
        /// Used to add a tab which controls the visibility of the added object.
        /// Uses the name of the inputted object for the tab text
        /// </summary>
        /// <param name="uiObjectToAdd"></param>
        /// <param name="load"></param>
        /// <param name="initialise"></param>
        /// <returns></returns>
        public override T AddChild<T>(T uiObjectToAdd, bool load = false, bool initialise = false)
        {
            // Use the name of the added object on the tab
            Debug.Assert(!string.IsNullOrEmpty(uiObjectToAdd.Name));

            // Add a button for our tab
            Button button = base.AddChild(new Button(uiObjectToAdd.Name, TabSize, Vector2.Zero));
            button.ClickableModule.OnLeftClicked += SwitchObject;
            button.StoredObject = uiObjectToAdd;

            Tabs.Add(button);
            CentreTabs();

            return base.AddChild(uiObjectToAdd, load, initialise);
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// When we add a new tab, we wish to shift all the tabs so that they are still positioned all around the LocalPosition as the centre.
        /// This alters the position of the tabs so that this is achieved.
        /// Assumed we have called this after the new tab has been added to Tabs
        /// </summary>
        /// <returns></returns>
        private void CentreTabs()
        {
            Debug.Assert(Tabs.Count > 0);

            float totalLength = TabSize.X * Tabs.Count;
            Vector2 firstPos = new Vector2((-totalLength + TabSize.X) * 0.5f, Tabs[0].LocalPosition.Y); // Start from the left and work right
            Tabs[0].LocalPosition = firstPos;

            if (Tabs.Count > 1)
            {
                for (int i = 1; i < Tabs.Count; i++)
                {
                    // Add on TabSize.X from the previous tab button
                    Tabs[i].LocalPosition = Tabs[i - 1].LocalPosition + new Vector2(TabSize.X, 0);
                }
            }
        }

        #endregion

        #region Callbacks

        /// <summary>
        /// Hides all objects except the one corresponding to the tab we clicked on.
        /// </summary>
        /// <param name="baseObject">The tab corresponding to our new object we wish to show</param>
        private void SwitchObject(BaseObject baseObject)
        {
            foreach (Button tab in Tabs)
            {
                DebugUtils.AssertNotNull(tab.StoredObject);
                Debug.Assert(tab.StoredObject is BaseObject);

                if (tab == baseObject)
                {
                    (tab.StoredObject as BaseObject).Show();
                }
                else
                {
                    

                    (tab.StoredObject as BaseObject).Hide();
                }
            }
        }

        #endregion
    }
}
