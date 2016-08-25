using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Diagnostics;
using System;

namespace _2DEngine
{
    /// <summary>
    /// A class which is used to manage game components - it will load, initialise, update, draw and handle input
    /// Typical examples of what you would use this for include screens and objects in game
    /// </summary>
    /// <typeparam name="T">An object which extends Component</typeparam>
    public class ObjectManager<T> : Component, IContainer<T>, IEnumerable<T> where T : Component
    {
        #region Properties and Fields

        /// <summary>
        /// A list to temporarily hold objects we wish to add to ActiveObjects
        /// </summary>
        protected List<T> ObjectsToAdd { get; private set; }

        /// <summary>
        /// All the current objects which we will update etc.
        /// </summary>
        protected List<T> ActiveObjects { get; private set; }

        /// <summary>
        /// All the objects we are going to remove from our ActiveObjects
        /// </summary>
        protected List<T> ObjectsToRemove { get; private set; }

        /// <summary>
        /// The number of active objects this container has
        /// </summary>
        public int ActiveObjectsCount { get { return ActiveObjects.Count; } }

        /// <summary>
        /// The number of objects waiting to be added
        /// </summary>
        public int ObjectsToAddCount { get { return ObjectsToAdd.Count; } }

        #endregion

        // Constructor
        public ObjectManager()
        {
            // Create the lists here
            ObjectsToAdd = new List<T>();
            ActiveObjects = new List<T>();
            ObjectsToRemove = new List<T>();
        }

        #region Virtual Functions

        /// <summary>
        /// Because we add objects in the update loop, the current objects we need to load are in ObjectsToAdd
        /// Iterate through them and call LoadContent on each of them
        /// </summary>
        public override void LoadContent()
        {
            // Check to see whether we have already called LoadContent
            CheckShouldLoad();

            foreach (T obj in ObjectsToAdd.FindAll(x => x.ShouldLoad))
            {
                // Call load content on all objects which have not already been initialised
                obj.LoadContent();
            }

            base.LoadContent();
        }

        /// <summary>
        /// Because we add objects in the update loop, the current objects we need to load are in ObjectsToAdd.
        /// Iterate through them, call Initialise on each of them and add to our ActiveObjects
        /// </summary>
        public override void Initialise()
        {
            // Check to see whether we have already called Initialise
            CheckShouldInitialise();

            foreach (T obj in ObjectsToAdd.FindAll(x => x.ShouldInitialise))
            {
                // Call initialise on all objects which have not already been initialised
                obj.Initialise();
            }

            ActiveObjects.AddRange(ObjectsToAdd);
            ObjectsToAdd.Clear();

            base.Initialise();
        }

        /// <summary>
        /// Iterate through all the objects in ActiveObjects and call Begin
        /// </summary>
        public override void Begin()
        {
            base.Begin();

            // Do not need to check whether begun has already been called - this is guaranteed

            // Do another pass on our ObjectsToAdd to add any stragglers
            ActiveObjects.AddRange(ObjectsToAdd);
            ObjectsToAdd.Clear();

            foreach (T obj in ActiveObjects.FindAll(x => !x.IsBegun))
            {
                // Call begin on any object which has not already begun
                obj.Begin();
            }
        }

        /// <summary>
        /// Iterate through the ActiveObjects and call HandleInput on them
        /// </summary>
        /// <param name="elapsedGameTime">The seconds that have elapsed since the last update loop</param>
        /// <param name="mousePosition">The current position of the mouse in the space of the Component (screen or game)</param>
        public override void HandleInput(float elapsedGameTime, Vector2 mousePosition)
        {
            base.HandleInput(elapsedGameTime, mousePosition);

            // Loop through the active object
            foreach (T obj in ActiveObjects)
            {
                // Handle input for the object
                if (obj.ShouldHandleInput)
                {
                    obj.HandleInput(elapsedGameTime, mousePosition);
                }
            }
        }

        /// <summary>
        /// Add all the objects in ObjectsToAdd to ActiveObjects, then update ActiveObjects before deleting objects in ObjectsToRemove
        /// </summary>
        /// <param name="elapsedGameTime">The seconds that have elapsed since the last update loop</param>
        public override void Update(float elapsedGameTime)
        {
            // Always call the super class' function - it will deal with whether it should run it itself
            base.Update(elapsedGameTime);

            // Add the objects and then clear the list - it is only a temporary holder
            ActiveObjects.AddRange(ObjectsToAdd);
            ObjectsToAdd.Clear();

            // Loop through the active object
            foreach (T obj in ActiveObjects)
            {
                if (obj.ShouldUpdate)
                {
                    // Update the object
                    obj.Update(elapsedGameTime);
                }
            }

            // Remove all the objects that are no longer alive
            ActiveObjects.RemoveAll(x => x.IsAlive == false);

            // Remove all the objects we have marked to remove
            foreach (T obj in ObjectsToRemove)
            {
                ActiveObjects.Remove(obj);
            }
            ObjectsToRemove.Clear();    // Clear the list - it is only a temporary holder
        }

        /// <summary>
        /// Loop through all the objects and call Draw
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch we can use to draw any textures</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            foreach (T obj in ActiveObjects)
            {
                if (obj.ShouldDraw)
                {
                    // Draw the object
                    obj.Draw(spriteBatch);
                }
            }
        }

        /// <summary>
        /// We must make sure that we show the children if necessary
        /// </summary>
        /// <param name="showChildren">A flag to indicate whether the children should be shown</param>
        public override void Show(bool showChildren = true)
        {
            base.Show();

            if (!showChildren) { return; }

            foreach (T obj in ActiveObjects)
            {
                obj.Show();
            }

            foreach (T obj in ObjectsToAdd)
            {
                obj.Show();
            }
        }

        /// <summary>
        /// We must make sure that we hide the children if necessary
        /// </summary>
        /// <param name="hideChildren">A flag to indicate whether the children should be hidden</param>
        public override void Hide(bool hideChildren = true)
        {
            base.Hide();

            if (!hideChildren) { return; }

            foreach (T obj in ActiveObjects)
            {
                obj.Hide();
            }

            foreach (T obj in ObjectsToAdd)
            {
                obj.Hide();
            }
        }

        /// <summary>
        /// We must make sure that we explicitly call Die on each child.
        /// The IsAlive attributes are connected up, but this will not result in a call to Die.
        /// </summary>
        public override void Die()
        {
            base.Die();

            foreach (T obj in ActiveObjects)
            {
                if (obj.IsAlive)
                {
                    obj.Die();
                }
            }

            foreach (T obj in ObjectsToAdd)
            {
                if (obj.IsAlive)
                {
                    obj.Die();
                }
            }
        }

        #endregion

        #region Object Management Functions

        /// <summary>
        /// A function to add a child
        /// </summary>
        /// <typeparam name="K">The type of the child</typeparam>
        /// <param name="childToAdd">The child itself</param>
        /// <param name="load">A flag to indicate whether we wish to call LoadContent on the child</param>
        /// <param name="initialise">A flag to indicate whether we wish to call Initialise on the child</param>
        /// <returns></returns>
        public virtual K AddChild<K>(K childToAdd, bool load = false, bool initialise = false) where K : T
        {
            if (load)
            {
                childToAdd.LoadContent();
            }

            if (initialise)
            {
                childToAdd.Initialise();
            }

            ObjectsToAdd.Add(childToAdd);

            return childToAdd;
        }

        /// <summary>
        /// A function to remove and kill a child
        /// </summary>
        /// <param name="childToRemove">The child we wish to remove</param>
        public void RemoveChild(T childToRemove)
        {
            DebugUtils.AssertNotNull(childToRemove);
            Debug.Assert(Exists(x => x == childToRemove));

            // This function will set IsAlive to false so that the object gets cleaned up next Update loop
            childToRemove.Die();
        }

        /// <summary>
        /// Extracts the inputted child from this container, but keeps it alive for insertion into another
        /// </summary>
        /// <param name="childToExtract">The child we wish to extract from this container</param>
        public K ExtractChild<K>(K childToExtract) where K : T
        {
            DebugUtils.AssertNotNull(childToExtract);

            ObjectsToRemove.Add(childToExtract);
            return childToExtract;
        }

        /// <summary>
        /// Searches through the children and returns all that match the inputted type
        /// </summary>
        /// <typeparam name="K">The inputted type we will use to find objects</typeparam>
        /// <returns>All the objects we have found of the inputted type</returns>
        public List<K> GetChildrenOfType<K>(bool includeObjectsToAdd = false) where K : T
        {
            List<K> objects = new List<K>();
            foreach (T obj in ActiveObjects)
            {
                if (obj is K)
                {
                    objects.Add(obj as K);
                }
            }

            if (includeObjectsToAdd)
            {
                foreach (T obj in ObjectsToAdd)
                {
                    if (obj is K)
                    {
                        objects.Add(obj as K);
                    }
                }
            }

            return objects;
        }

        /// <summary>
        /// Finds an object of the inputted name and casts to the inputted type K.
        /// First searches the ActiveObjects and then the ObjectsToAdd
        /// </summary>
        /// <typeparam name="K">The type we wish to return the found object as</typeparam>
        /// <param name="name">The name of the object we wish to find</param>
        /// <returns>Returns the object casted to K or null</returns>
        public K FindChild<K>() where K : T
        {
            return FindChild<K>(x => x is K);
        }

        /// <summary>
        /// Finds an object of the inputted name and casts to the inputted type K.
        /// First searches the ActiveObjects and then the ObjectsToAdd
        /// </summary>
        /// <typeparam name="K">The type we wish to return the found object as</typeparam>
        /// <param name="predicate">The predicate we will use to apply a condition for which we wish to find the object</param>
        /// <returns>Returns the object casted to K or null</returns>
        public K FindChild<K>(Predicate<T> predicate) where K : T
        {
            K obj = null;
            obj = ActiveObjects.Find(predicate) as K;

            if (obj != null) { return obj; }

            obj = ObjectsToAdd.Find(predicate) as K;

            return obj;
        }

        /// <summary>
        /// Returns whether an object satisfying the inputted predicate exists within the container.
        /// First serches the ActiveObjects and then the ObjectsToAdd.
        /// </summary>
        /// <param name="predicate">The predicate the object must satisfy</param>
        /// <returns>True if such an object exists and false if not</returns>
        public bool Exists(Predicate<T> predicate)
        {
            // Check our active objects for a valid object
            if (ActiveObjects.Exists(predicate))
            {
                // Return true if one exists
                return true;
            }

            // Check our objects to add for a valid object
            if (ObjectsToAdd.Exists(predicate))
            {
                // Return true if one exists
                return true;
            }

            // Otherwise no such object exists so return false
            return false;
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Returns the first child.
        /// Shouldn't really be called unless we have children
        /// </summary>
        /// <returns>The first child we added</returns>
        public T FirstChild()
        {
            Debug.Assert(ActiveObjectsCount > 0);
            return ActiveObjects[0];
        }

        /// <summary>
        /// Returns the most recent child we added which is castable to the inputted type.
        /// Shouldn't really be called unless we have children
        /// </summary>
        /// <returns>The most recent child we added</returns>
        public K LastChild<K>() where K : T
        {
            K lastChildOfType = ActiveObjects.FindLast(x => x is K) as K;
            DebugUtils.AssertNotNull(lastChildOfType);

            return lastChildOfType;
        }

        /// <summary>
        /// Returns the most recent child we added which is castable to the inputted type and satisfies the inputted condition.
        /// Shouldn't really be called unless we have children
        /// </summary>
        /// <returns>The most recent child we added</returns>
        public K LastChild<K>(Predicate<K> condition) where K : T
        {
            K lastChildOfType = ActiveObjects.FindLast(x => x is K && condition(x as K)) as K;
            DebugUtils.AssertNotNull(lastChildOfType);

            return lastChildOfType;
        }

        /// <summary>
        /// Returns the child at the inputted index in our list.
        /// Shouldn't be called unless we have children.
        /// </summary>
        /// <param name="index">The 0 based index of the element we wish to extract</param>
        /// <returns></returns>
        public T ChildAtIndex(int index)
        {
            Debug.Assert(index < ActiveObjectsCount);
            return ActiveObjects[index];
        }

        #endregion

        #region Enumerators

        /// <summary>
        /// Iterator used so that we can use this class in a foreach loop and it will iterate through the active objects
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)ActiveObjects).GetEnumerator();
        }

        /// <summary>
        /// Iterator used so that we can use this class in a foreach loop and it will iterate through the active objects
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)ActiveObjects).GetEnumerator();
        }

        #endregion
    }
}