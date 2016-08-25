namespace _2DEngine
{
    /// <summary>
    /// A singleton class for managing commands in for our screens.
    /// </summary>
    public class CommandManager : ObjectManager<Command>
    {
        #region Properties

        /// <summary>
        /// Our CommandManager singleton.
        /// </summary>
        private static CommandManager instance;
        public static CommandManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CommandManager();
                }

                return instance;
            }
        }

        #endregion

        /// <summary>
        /// Private constructor to enforce the singleton.
        /// </summary>
        private CommandManager() :
            base()
        {
            
        }

        #region Virtual Functions

        /// <summary>
        /// Perform the CanRun on each command
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public override void Update(float elapsedGameTime)
        {
            base.Update(elapsedGameTime);

            foreach (Command command in this)
            {
                command.CheckCanRun();
            }
        }

        /// <summary>
        /// Adds a command and sets it's ParentScreen property to the current screen if not currently set.
        /// </summary>
        /// <param name="commandToAdd"></param>
        /// <param name="load"></param>
        /// <param name="initialise"></param>
        /// <returns></returns>
        public override T AddChild<T>(T commandToAdd, bool load = false, bool initialise = false)
        {
            if (commandToAdd.ParentScreen == null)
            {
                //commandToAdd.ParentScreen = ScreenManager.Instance.CurrentScreen;
            }

            // Run our can run validation otherwise if we wait a frame for this to happen in the Update loop, it will be too late and it will already have started running
            base.AddChild(commandToAdd, load, initialise);
            commandToAdd.CheckCanRun();

            return commandToAdd;
        }

        #endregion
    }
}
