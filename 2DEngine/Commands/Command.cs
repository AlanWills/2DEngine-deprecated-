using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// A delegate use for a function which will be called to test whether the current command can run
    /// </summary>
    public delegate void CanCommandRun();

    /// <summary>
    /// A delegate used for a function which will be called when the command dies
    /// </summary>
    public delegate void OnCommandDeath(Command command);

    /// <summary>
    /// A command class to run a set of actions within a screen.
    /// Commands should not draw, update or handle input for items, but instead add them to their appropriate screen's manager.
    /// This can always be done by caching the screen as a property.
    /// Commands are really designed to perform custom input.
    /// Call Die for the Command to be completed and cleared up by the CommandManager.
    /// </summary>
    public abstract class Command : Component
    {
        #region Properties and Fields

        /// <summary>
        /// A reference to the parent screen - this will be useful for adding objects etc.
        /// </summary>
        public BaseScreen ParentScreen { get; private set; }

        /// <summary>
        /// An optional parameter we can set to indicate that this command should not be able to run unless the previous command has finished.
        /// </summary>
        public Command PreviousCommand { get; set; }

        /// <summary>
        /// An event handler for specifying a function to determine whether this script can run.
        /// The event must specify itself whether the command ShouldHandleInput and ShouldUpdate.
        /// </summary>
        public event CanCommandRun CanRunEvent;

        /// <summary>
        /// An event handler used to manage a function which will be called when the command is completed.
        /// Useful for resetting the game state for example.
        /// </summary>
        public event OnCommandDeath OnDeathCallback;

        /// <summary>
        /// Flags which can be set to pause screen updating and input handling (drawing will always happen).
        /// </summary>
        public bool GameHandleInput { get; set; }
        public bool GameUpdate { get; set; }

        /// <summary>
        /// A lifetime we can use to forcibly kill commands after a certain amount of time has passed.
        /// </summary>
        protected float LifeTime { get; private set; }

        private float currentLifeTime = 0;

        #endregion

        public Command(bool shouldPauseGame = false, float lifeTime = float.MaxValue)
        {
            LifeTime = lifeTime;
            ParentScreen = ScreenManager.Instance.CurrentScreen;

            GameHandleInput = !shouldPauseGame;
            GameUpdate = !shouldPauseGame;
        }

        #region Virtual Functions

        /// <summary>
        /// Pauses our game updating and input handling if we flagged to do so
        /// </summary>
        public override void Begin()
        {
            base.Begin();

            if (!GameHandleInput)
            {
                ParentScreen.ShouldHandleInput = false;
            }

            if (!GameUpdate)
            {
                ParentScreen.ShouldUpdate = false;
            }
        }

        /// <summary>
        /// Checks to see if the command can run and handle input.
        /// Will not call begin until the command can actually start running
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public override void Update(float elapsedGameTime)
        {
            currentLifeTime += elapsedGameTime;
            if (currentLifeTime > LifeTime)
            {
                Die();
            }

            base.Update(elapsedGameTime);
        }

        /// <summary>
        /// No drawing in commands
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            // No drawing for commands
            Debug.Fail("No drawing in commands");
        }

        /// <summary>
        /// Utility Function to set ShouldHandleInput and ShouldUpdate based on certain conditions.
        /// Tests if the PreviousCommand is completely, or based on a custom event, otherwise true.
        /// </summary>
        /// <returns></returns>
        public virtual void CheckCanRun()
        {
            if (ScreenManager.Instance.CurrentScreen != ParentScreen)
            {
                ShouldHandleInput = false;
                ShouldUpdate = false;
            }
            else if (PreviousCommand != null)
            {
                ShouldHandleInput = !PreviousCommand.IsAlive;
                ShouldUpdate = !PreviousCommand.IsAlive;
            }
            else if (CanRunEvent != null)
            {
                CanRunEvent();
            }
            else
            {
                ShouldHandleInput = true;
                ShouldUpdate = true;
            }
        }

        /// <summary>
        /// Indicates that the command has finished and calls the OnDeathCallback event.
        /// Also unpauses our game and resumes input handling if necessary.
        /// </summary>
        public override void Die()
        {
            base.Die();

            // Clear our previous command reference so we don't have a memory leak with references not being released
            // This is really important if we have a string of commands otherwise they will not be GB until the final one is completed
            if (PreviousCommand != null)
            {
                PreviousCommand = null;
            }

            if (!GameHandleInput)
            {
                ParentScreen.ShouldHandleInput = true;
            }

            if (!GameUpdate)
            {
                ParentScreen.ShouldUpdate = true;
            }

            OnDeathCallback?.Invoke(this);
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Wrapper function to allow a builder-like command framework.
        /// Sets the inputted command's previous command to this and adds it to the CommandManager.
        /// Then returns that command
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public T NextCommand<T>(T command, bool load = true, bool initialise = true) where T : Command
        {
            command.PreviousCommand = this;
            CommandManager.Instance.AddChild(command, load, initialise);

            return command;
        }

        #endregion
    }
}
