using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace _2DEngine
{
    /// <summary>
    /// An interface which provides preset functions which any class acting as a container must have.
    /// Helps keep API consistent.
    /// </summary>
    public interface IContainer<T> where T : Component
    {
        #region Virtual Component Functions

        /// <summary>
        /// We must make sure that we load our children
        /// </summary>
        void LoadContent();

        /// <summary>
        /// We must make sure that we initialise our children
        /// </summary>
        void Initialise();

        /// <summary>
        /// We must make sure that we call begin on our children
        /// </summary>
        void Begin();

        /// <summary>
        /// We must make sure that we handle input for our children
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        /// <param name="mousePosition"></param>
        void HandleInput(float elapsedGameTime, Vector2 mousePosition);

        /// <summary>
        /// We must make sure that we update our children
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        void Update(float elapsedGameTime);

        /// <summary>
        /// We must make sure that we are drawing our children
        /// </summary>
        /// <param name="spriteBatch"></param>
        void Draw(SpriteBatch spriteBatch);

        /// <summary>
        /// We must make sure that we show the children if necessary
        /// </summary>
        /// <param name="showChildren">A flag to indicate whether the children should be shown</param>
        void Show(bool showChildren = true);

        /// <summary>
        /// We must make sure that we hide the children if necessary
        /// </summary>
        /// <param name="hideChildren">A flag to indicate whether the children should be hidden</param>
        void Hide(bool hideChildren = true);

        /// <summary>
        /// We must make sure that we explicitly call Die on each child.
        /// The IsAlive attributes are connected up, but this will not result in a call to Die.
        /// </summary>
        void Die();

        /// <summary>
        /// Returns the first child.
        /// Shouldn't really be called unless we have children
        /// </summary>
        /// <returns>The first child we added</returns>
        T FirstChild();

        /// <summary>
        /// Returns the most recent child we added which is castable to the inputted type.
        /// Shouldn't really be called unless we have children
        /// </summary>
        /// <returns>The most recent child we added</returns>
        K LastChild<K>() where K : T;


        /// <summary>
        /// Returns the most recent child we added which is castable to the inputted type and satisfies the inputted condition.
        /// Shouldn't really be called unless we have children
        /// </summary>
        /// <returns>The most recent child we added</returns>
        K LastChild<K>(Predicate<K> condition) where K : T;

        #endregion

        #region Object Management Utility Functions

        /// <summary>
        /// A function to add a child
        /// </summary>
        /// <typeparam name="K">The type of the child</typeparam>
        /// <param name="childToAdd">The child itself</param>
        /// <param name="load">A flag to indicate whether we wish to call LoadContent on the child</param>
        /// <param name="initialise">A flag to indicate whether we wish to call Initialise on the child</param>
        /// <returns></returns>
        K AddChild<K>(K childToAdd, bool load = false, bool initialise = false) where K : T;

        /// <summary>
        /// A function to remove and kill a child
        /// </summary>
        /// <param name="childToRemove">The child we wish to remove</param>
        void RemoveChild(T childToRemove);

        /// <summary>
        /// Extracts the inputted child from this container, but keeps it alive for insertion into another
        /// </summary>
        /// <param name="childToExtract">The child we wish to extract from this container</param>
        K ExtractChild<K>(K childToExtract) where K : T;

        /// <summary>
        /// Searches through the children and returns all that match the inputted type
        /// </summary>
        /// <typeparam name="K">The inputted type we will use to find objects</typeparam>
        /// <returns>All the objects we have found of the inputted type</returns>
        List<K> GetChildrenOfType<K>(bool includeObjectsToAdd = false) where K : T;

        /// <summary>
        /// Finds an object of the inputted name and casts to the inputted type K.
        /// First searches the ActiveObjects and then the ObjectsToAdd
        /// </summary>
        /// <typeparam name="K">The type we wish to return the found object as</typeparam>
        /// <param name="name">The name of the object we wish to find</param>
        /// <returns>Returns the object casted to K or null</returns>
        K FindChild<K>() where K : T;

        /// <summary>
        /// Finds an object of the inputted name, which matches the inputted predicate and casts to the inputted type K.
        /// First searches the ActiveObjects and then the ObjectsToAdd
        /// </summary>
        /// <typeparam name="K">The type we wish to return the found object as</typeparam>
        /// <param name="name">The name of the object we wish to find</param>
        /// <returns>Returns the object casted to K or null</returns>
        K FindChild<K>(Predicate<T> predicate) where K : T;

        /// <summary>
        /// Returns whether an object satisfying the inputted predicate exists within the container
        /// </summary>
        /// <param name="predicate">The predicate the object must satisfy</param>
        /// <returns>True if such an object exists and false if not</returns>
        bool Exists(Predicate<T> predicate);

        #endregion
    }
}