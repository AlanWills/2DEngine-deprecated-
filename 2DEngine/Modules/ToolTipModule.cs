using Microsoft.Xna.Framework;

namespace _2DEngine
{
    /// <summary>
    /// A module which is responsible for showing and hiding a piece of UI when the attached base object is hovered over.
    /// To ensure our object is rendered over everything, we extract and add it as a screen object rather than just hiding and showing it.
    /// We will show it at the first place the mouse enters the object's collider.
    /// Loads and Initialises the UIObject if it is passed in.
    /// </summary>
    public class ToolTipModule : BaseObjectModule
    {
        #region Properties and Fields

        /// <summary>
        /// A reference to our our tool tip - will cache this and then insert/extract it from the current screen as appropriate
        /// </summary>
        public UIObject ToolTip { get; protected set; }

        /// <summary>
        /// The position the tool tip should appear at in Screen coordinates.
        /// </summary>
        protected virtual Vector2 ToolTipPosition
        {
            get
            {
                return GameMouse.Instance.WorldPosition + new Vector2(ToolTip.Size.X * 0.5f, -ToolTip.Size.Y * 0.5f);
            }
        }

        /// <summary>
        /// The amount of time needed for the mouse to be hovered over the attached object before we show the ToolTip.
        /// By default, this is 0.5f;
        /// </summary>
        protected float HoverTimeThreshold { private get; set; }

        private bool toolTipAdded;
        private float currentHoverTime;

        #endregion

        public ToolTipModule()
        {
            HoverTimeThreshold = 0.5f;
        }

        public ToolTipModule(string text)
        {
            HoverTimeThreshold = 0.5f;

            ToolTip = new Label(text, Vector2.Zero);
            ToolTip.Colour = Color.White;
        }

        public ToolTipModule(UIObject toolTip)
        {
            HoverTimeThreshold = 0.5f;
            ToolTip = toolTip;
        }

        #region Virtual Functions

        /// <summary>
        /// Loads the ToolTip object
        /// </summary>
        public override void LoadContent()
        {
            CheckShouldLoad();

            DebugUtils.AssertNotNull(ToolTip);
            ToolTip.LoadContent();

            base.LoadContent();
        }

        /// <summary>
        /// Initialises the ToolTip object
        /// </summary>
        public override void Initialise()
        {
            CheckShouldInitialise();

            ToolTip.Initialise();

            base.Initialise();
        }

        /// <summary>
        /// Adds or removes the ToolTip from the main screen as approprite
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        /// <param name="mousePosition"></param>
        public override void HandleInput(float elapsedGameTime, Vector2 mousePosition)
        {
            base.HandleInput(elapsedGameTime, mousePosition);

            // Check to see if the tool tip has been added
            if (!toolTipAdded)
            {
                // If it hasn't, check to see whether we have the right input to register hover time
                if (IsInputValidToShowToolTip())
                {
                    // If we do, increment the current time hovered
                    currentHoverTime += elapsedGameTime;

                    if (currentHoverTime >= HoverTimeThreshold)
                    {
                        // If we have hovered over it enough, we add the tool tip to the screen
                        ScreenManager.Instance.CurrentScreen.AddScreenUIObject(ToolTip);
                        ToolTip.LocalPosition = ToolTipPosition;
                        toolTipAdded = true;
                    }
                }
            }
            else
            {
                // It it has been added, check to see whether we should keep it visible
                if (!IsInputValidToShowToolTip())
                {
                    // If we have invalid input to keep the tool tup visible, we extract it from the screen, but do not kill it
                    // This means we can cache it rather than constantly recreating it
                    ScreenManager.Instance.CurrentScreen.ExtractScreenUIObject(ToolTip);
                    toolTipAdded = false;

                    currentHoverTime = 0;
                }
            }
        }

        /// <summary>
        /// A virtual function to allow modules inheriting off of this one to be able to specify custom behaviour for when the tool tip appears
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsInputValidToShowToolTip()
        {
            DebugUtils.AssertNotNull(AttachedBaseObject.Collider);

            return AttachedBaseObject.Collider.IsMouseOver;
        }

        /// <summary>
        /// Make sure we clean up the ToolTip
        /// </summary>
        public override void Die()
        {
            base.Die();

            ToolTip.Die();
        }

        #endregion
    }
}
