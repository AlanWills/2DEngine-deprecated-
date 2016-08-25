using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// A module used on a BaseObject.
    /// Marked as abstract because it merely gives us an interface to our AttachedComponent as a BaseObject
    /// </summary>
    public abstract class BaseObjectModule : Module
    {
        #region Properties and Fields

        /// <summary>
        /// A reference to our Component as a BaseObject.
        /// Readonly.
        /// </summary>
        private BaseObject baseObject;
        public BaseObject AttachedBaseObject
        {
            get
            {
                if (baseObject == null)
                {
                    Debug.Assert(AttachedComponent is BaseObject);
                    baseObject = AttachedComponent as BaseObject;
                }

                return baseObject;
            }
        }

        #endregion
    }
}
