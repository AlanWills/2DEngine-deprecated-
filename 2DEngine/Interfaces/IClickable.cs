using Microsoft.Xna.Framework.Input;

namespace _2DEngine
{
    /// <summary>
    /// An enum to describe the state of a clickable object
    /// </summary>
    public enum ClickState
    {
        kIdle,
        kHighlighted,
        kPressed,
        kDisabled,
    }

    /// <summary>
    /// An interface to describe objects which respond to left, middle and right clicks by the mouse
    /// </summary>
    public interface IClickable
    {
        /// <summary>
        /// An optional key which will also trigger the left click event
        /// </summary>
        Keys LeftClickAccelerator { get; set; }

        /// <summary>
        /// An optional key which will also trigger the middle click event
        /// </summary>
        Keys MiddleClickAccelerator { get; set; }

        /// <summary>
        /// An optional key which will also trigger the right click event
        /// </summary>
        Keys RightClickAccelerator { get; set; }

        /// <summary>
        /// These represent a function which we want to execute when this object is clicked by the appropriate mouse button.
        /// To do this, construct a function of the form:
        /// public void Function(Button button) { ... }
        /// 
        /// Then do OnClicked += Function;
        /// 
        /// In fact, more than one function can be set up in this way.
        /// </summary>
        event OnClicked OnLeftClicked;
        event OnClicked OnMiddleClicked;
        event OnClicked OnRightClicked;

        /// <summary>
        /// A variable to mark the current status of the clickable object
        /// </summary>
        ClickState ClickState { get; }
    }
}
