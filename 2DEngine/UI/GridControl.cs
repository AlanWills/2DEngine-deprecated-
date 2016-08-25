using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// Holds an unlimited number of elements in a grid format rather than a list
    /// </summary>
    public class GridControl : ListControl
    {
        #region Properties and Fields

        // Private Properties just to help for UI calculations
        /// <summary>
        /// The size of each element calculated once using our size and the number of columns.
        /// Don'se use this in calculations, but rather to set the size of objects we are adding who will then resize themselves accordingly (e.g. images)
        /// </summary>
        private Vector2 ElementSize { get; set; }

        /// <summary>
        /// The number of columns our control has
        /// </summary>
        private int Columns { get; set; }

        #endregion

        /// <summary>
        /// A size must be inputted, because we need to know how big to make any element we add to the control.  We cannot wait for a size from the texture to do this as items might be added before it's Initialise step is complete.
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="size"></param>
        /// <param name="localPosition"></param>
        /// <param name="textureAsset"></param>
        public GridControl(int columns, Vector2 size, Vector2 localPosition, string textureAsset = AssetManager.DefaultEmptyPanelTextureAsset) :
            base(size, localPosition, textureAsset)
        {
            Columns = columns;
            UsesCollider = false;
            BorderPadding = Size * 0.1f;

            // Use the x dimension for both, because we do not know how many rows we are going to have
            // Assume we have square elements and let the image code deal with the rest
            ElementSize = new Vector2((Size.X - 2 * BorderPadding.X) / Columns);
        }

        #region Virtual Functions

        public override T AddChild<T>(T uiObjectToAdd, bool load = false, bool initialise = false)
        {
            if (initialise || uiObjectToAdd.Size == Vector2.Zero)
            {
                // If we have not called initialise yet or the object has no size, set the ElementSize and let the object use that to scale itself
                uiObjectToAdd.Size = ElementSize;
            }
            else
            {
                // If the object has already been initialised, we need to just scale the object by the ratio between our ElementSize.X and it's Size.X
                uiObjectToAdd.Scale(ElementSize.X / uiObjectToAdd.Size.X);
            }

            return base.AddChild(uiObjectToAdd, load, initialise);
        }

        /// <summary>
        /// Loops through all the alive elements in this container and updates their positions based on their order.
        /// Gets more costly the more elements we have so should not be done lightly.
        /// </summary>
        protected override void RebuildList()
        {
            Debug.Assert(NeedsRebuild);

            UIObject previous = null;

            int counter = 0;
            foreach (UIObject uiObject in Children)
            {
                // If our object is dead it will be cleared up so should have it's position recalculated
                if (!uiObject.IsAlive) { continue; }

                if (previous == null)
                {
                    uiObject.LocalPosition = new Vector2((-Size.X + uiObject.Size.X) * 0.5f + BorderPadding.X, (-Size.Y + uiObject.Size.Y) * 0.5f + verticalPaddingBetweenElements + BorderPadding.Y);
                }
                else
                {
                    // Add relative to the position of the last card
                    int column = counter % Columns;
                    if (column == 0)
                    {
                        // We add to new row in first column
                        uiObject.LocalPosition = new Vector2((-Size.X + uiObject.Size.X) * 0.5f + BorderPadding.X, previous.LocalPosition.Y + uiObject.Size.Y + verticalPaddingBetweenElements);
                    }
                    else
                    {
                        // Add to next column along on same row
                        uiObject.LocalPosition = previous.LocalPosition + new Vector2(uiObject.Size.X, 0);
                    }

                    // Check we are indeed creating in a different place
                    Debug.Assert(uiObject.LocalPosition != previous.LocalPosition);
                }

                previous = uiObject;
                counter++;
            }
        }

        #endregion
    }
}
