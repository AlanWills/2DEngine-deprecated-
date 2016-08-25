using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// A delegate for specifying a custom sort function for this list.
    /// Returns the object which should appear nearest the top.
    /// Will be applied using one of the sorting algorithms.
    /// </summary>
    /// <param name="uiObject1">The first object to compare</param>
    /// <param name="uiObject2">The second object to compare</param>
    /// <returns>The object which should appear further up the list.</returns>
    public delegate UIObject SortFunction(UIObject uiObject1, UIObject uiObject2);

    /// <summary>
    /// An object for controlling elements in a list.
    /// Can add and remove objects and set a custom sort function (default is order of addition).
    /// </summary>
    public class ListControl : UIObject
    {
        #region Properties and Fields

        /// <summary>
        /// A bool to indicate whether we should rebuild this list control on the next update loop.
        /// Need this because adding objects takes a frame to occur.
        /// </summary>
        public bool NeedsRebuild { protected get; set; }

        /// <summary>
        /// Our function for sorting the elements in our list.
        /// If not specified, we will just sort by order added to this container.
        /// </summary>
        public event SortFunction SortFunction;

        /// <summary>
        /// A list of objects we need to sort.
        /// </summary>
        private List<BaseObject> ObjectsToSort { get; set; }

        /// <summary>
        /// The padding from our border
        /// </summary>
        public Vector2 BorderPadding { get; set; }

        /// <summary>
        /// A flag to indicate whether we should scroll the objects in this control
        /// </summary>
        protected bool ScrollingEnabled { private get; set; }

        /// <summary>
        /// The rectangle we will use to cut off non-visible sections of our objects
        /// </summary>
        protected override Rectangle ScissorRectangle
        {
            get
            {
                return new Rectangle(
                    (int)(WorldPosition.X - Size.X * 0.5f  + BorderPadding.X),
                    (int)(WorldPosition.Y - Size.Y * 0.5f + BorderPadding.Y),
                    (int)(Size.X - 2 * BorderPadding.X),
                    (int)(Size.Y - 2 * BorderPadding.Y));
            }
        }

        /// <summary>
        /// Since when we add objects we do not add them straight away, we use this flag to indicate we need to sort in our update loop.
        /// </summary>
        private bool NeedsSort { get; set; }

        private const float scroll = 0.75f;
        protected const float verticalPaddingBetweenElements = 15;

        #endregion

        public ListControl(Vector2 localPosition, string textureAsset = AssetManager.DefaultEmptyPanelTextureAsset) :
            this(Vector2.Zero, localPosition, textureAsset)
        { 

        }

        public ListControl(Vector2 size, Vector2 localPosition, string textureAsset = AssetManager.DefaultEmptyPanelTextureAsset) :
            base(size, localPosition, textureAsset)
        {
            ObjectsToSort = new List<BaseObject>();
            UseScissorRectangle = true;
            BorderPadding = new Vector2(float.NegativeInfinity, float.NegativeInfinity);        // Yeah this is probably a mistake
            UsesCollider = false;
            ScrollingEnabled = true;
        }

        #region Virtual Functions

        /// <summary>
        /// Set up our border padding and mark our list as needing a rebuild
        /// </summary>
        public override void Initialise()
        {
            CheckShouldInitialise();

            base.Initialise();

            // Want to only set the border padding if it is unset, but theoretically any value should be valid
            if (BorderPadding == new Vector2(float.NegativeInfinity, float.NegativeInfinity))
            {
                BorderPadding = Size * 0.1f;
            }
        }

        /// <summary>
        /// Scrolls the options if the mouse wheel has been scrolled.
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        /// <param name="mousePosition"></param>
        public override void HandleInput(float elapsedGameTime, Vector2 mousePosition)
        {
            base.HandleInput(elapsedGameTime, mousePosition);

            // Only scroll if the mouse has been scrolled and we actually HAVE objects in our list control
            if (ScrollingEnabled && GameMouse.Instance.HasMouseWheelScrolled && Children.ActiveObjectsCount > 0)
            {
                // Shift the objects
                int mouseWheelDelta = GameMouse.Instance.MouseWheelScrollDelta;
                Vector2 scrollAmount = new Vector2(0, mouseWheelDelta * scroll);

                // Since the objects are sorted at the moment, we can enforce scroll bounds using the first and last elements.
                if (mouseWheelDelta > 0)
                {
                    // We have scrolled away from us so menu items are going down - check first element is not going too far
                    BaseObject first = FirstChild();
                    float firstTopY = first.LocalPosition.Y - first.Size.Y * 0.5f;

                    float yDiff = -Size.Y * 0.5f + verticalPaddingBetweenElements + BorderPadding.Y - firstTopY;
                    scrollAmount.Y = MathHelper.Clamp(yDiff, 0, scrollAmount.Y);
                }
                else
                {
                    // We have scrolled towards us so menu items are going up - check last element is contained
                    BaseObject last = Children.LastChild<BaseObject>();
                    float lastBottomY = last.LocalPosition.Y + last.Size.Y * 0.5f;

                    // Find minimum difference between the bottom object's bottom and the bottom of our list control as a whole compared with the scroll amount.  
                    float yDiff = Size.Y * 0.5f - verticalPaddingBetweenElements - BorderPadding.Y - lastBottomY;
                    scrollAmount.Y = MathHelper.Clamp(yDiff, scrollAmount.Y, 0);
                }

                foreach (UIObject uiObject in Children)
                {
                    uiObject.LocalPosition += scrollAmount;
                }
            }
        }

        /// <summary>
        /// Sort our objects if we need to.
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public override void Update(float elapsedGameTime)
        {
            base.Update(elapsedGameTime);

            // Maybe rebuild when we sort, by swapping positions?
            /*if (NeedsSort)
            {
                foreach (UIObject uiObject in ObjectsToSort)
                {
                    Sort(uiObject);
                    NeedsSort = true;
                }

                NeedsSort = false;
                ObjectsToSort.Clear();
            }*/

            // Rebuild after we sort so the elements will be in the correct places
            if (NeedsRebuild)
            {
                RebuildList();
                NeedsRebuild = false;
            }
        }

        /// <summary>
        /// Adds a UIObject and updates our sort flag for our next update loop.
        /// </summary>
        /// <param name="uiObjectToAdd"></param>
        /// <param name="load"></param>
        /// <param name="initialise"></param>
        /// <returns></returns>
        public override T AddChild<T>(T uiObjectToAdd, bool load = false, bool initialise = false)
        {
            NeedsRebuild = true;
            ObjectsToSort.Add(uiObjectToAdd);

            return base.AddChild(uiObjectToAdd, load, initialise);
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Calls our sort function on the inputted element and all elements in the list
        /// </summary>
        /// <param name="addedObject"></param>
        private void Sort(UIObject addedObject)
        {
            Debug.Assert(NeedsSort);

            // Sort the addedObject into it's appropriate position in the list

            NeedsSort = false;
        }

        /// <summary>
        /// A virtual function for rebuilding our list - sorting and spacing
        /// </summary>
        protected virtual void RebuildList()
        {
            UIObject previous = null;
            float padding = 5;
            foreach (UIObject uiObject in Children)
            {
                if (previous == null)
                {
                    uiObject.LocalPosition = new Vector2(0, -Size.Y * 0.5f + uiObject.Size.Y + BorderPadding.Y + verticalPaddingBetweenElements);
                }
                else
                {
                    uiObject.LocalPosition = previous.LocalPosition + new Vector2(0, uiObject.Size.Y + padding + verticalPaddingBetweenElements);
                }

                previous = uiObject;
            }
        }

        #endregion
    }
}
