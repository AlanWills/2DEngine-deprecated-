using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// A module used on a GameObject.
    /// Marked as abstract because it merely gives us an interface to our AttachedComponent as a GameObject
    /// </summary>
    public abstract class GameObjectModule : BaseObjectModule
    {
        #region Properties and Fields

        /// <summary>
        /// A reference to our Component as a GameObject.
        /// Readonly.
        /// </summary>
        private GameObject gameObject;
        public GameObject AttachedGameObject
        {
            get
            {
                if (gameObject == null)
                {
                    Debug.Assert(AttachedComponent is GameObject);
                    gameObject = AttachedComponent as GameObject;
                }

                return gameObject;
            }
        }

        #endregion
    }
}
