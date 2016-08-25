using System;
using System.Collections;

namespace _2DEngine
{
    /// <summary>
    /// A class which iterates down through our hierarchy over every node of type T doing breadth first search.
    /// Currently cannot deal with uneven trees very well - need to improve the algorithm.
    /// </summary>
    public class BaseObjectIterator<T> : IEnumerator where T : BaseObject
    {
        #region Properties and Fields

        /// <summary>
        /// Represents the current level we are searching in the tree
        /// </summary>
        private int CurrentLevel { get; set; }

        /// <summary>
        /// A reference to our root component
        /// </summary>
        private BaseObject Root { get; set; }

        /// <summary>
        /// The current object in our iterator
        /// </summary>
        private BaseObject current;
        public object Current { get { return current; } }

        /// <summary>
        /// The current object in our iterator casted as T
        /// </summary>
        public T CurrentAsT { get { return current as T; } }

        /// <summary>
        /// A function which gets the next object in our hierarchy
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            return BreadthFirstImpl();
        }

        /// <summary>
        /// Resets our iterator to the top of our hierarchy
        /// </summary>
        public void Reset()
        {
            current = Root;
            CurrentLevel = 0;
        }

        // Cache this predicate rather than constantly recreating it
        private Predicate<BaseObject> isTPredicate = new Predicate<BaseObject>(x => x is T);

        #endregion

        public BaseObjectIterator(BaseObject root)
        {
            Root = root;
            current = root;
            CurrentLevel = 0;
        }

        #region Iterator Implementation

        /// <summary>
        /// The actual implementation of MoveNext stored in a separate function for convenience and recursive calling
        /// </summary>
        /// <returns></returns>
        private bool BreadthFirstImpl()
        {
            // Check to see whether we are at the root - we need to do custom behaviour here
            if (current == Root)
            {
                // If we have no children, we are done
                if (current.ChildrenCount == 0)
                {
                    return false;
                }

                // Get the first child under the root
                current = current.FirstChild();
                CurrentLevel++;

                return EitherCorrectTypeOrPerformImpl();
            }

            // We are not at the root, but have found a child not of type T

            // Run through our siblings until we have found a sibling of type T
            while (current.HasNextSibling)
            {
                current = current.NextSibling;
                return EitherCorrectTypeOrPerformImpl();
            }

            // If our parent has another sibling who's children we need to iterate through, we go to that sibling and get it's first child
            BaseObject currentParent = current.Parent;
            if (currentParent.HasNextSibling)
            {
                while (currentParent.HasNextSibling)
                {
                    if (currentParent.NextSibling.ChildrenCount > 0)
                    {
                        current = currentParent.NextSibling.FirstChild();
                        return EitherCorrectTypeOrPerformImpl();
                    }

                    // Move to the next sibling
                    currentParent = currentParent.NextSibling;
                }
            }

            // None of the siblings on this level are of type T

            // Later on instead of moving up one, we need to move down from the root the number of levels we are in - 1

            // Find the first sibling of our current which has children - we do this by going to the root and iterating down to the level we were at
            // Only go through the nodes with children
            current = Root;
            for (int i = 0; i < CurrentLevel; i++)
            {
                current = current.FindChild<BaseObject>(x => x.ChildrenCount > 0);
            }

            if (current == null)
            {
                // If no siblings exist with children, we are done
                return false;
            }

            // Get the first child under the first sibling with children
            current = current.FirstChild();
            CurrentLevel++;

            return EitherCorrectTypeOrPerformImpl();
        }

        /// <summary>
        /// Simple utility wrapper for checking whether our current is of type T, or otherwise performing the search impl again
        /// </summary>
        /// <returns></returns>
        private bool EitherCorrectTypeOrPerformImpl()
        {
            if (current is T)
            {
                // If it is of type T, we are done
                return true;
            }
            else
            {
                // Otherwise perform the same logic but for our new current
                return BreadthFirstImpl();
            }
        }

        #endregion

        #region Operator Overloads

        #endregion
    }
}
