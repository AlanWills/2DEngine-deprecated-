namespace _2DEngine
{
    public delegate void CallbackEventHandler();

    /// <summary>
    /// A command which allows registration of a callback.
    /// The callback is run as soon as the command enters it's Begin function and then the command dies
    /// </summary>
    public class CallbackCommand : Command
    {
        #region Properties and Fields

        public event CallbackEventHandler Callback;

        #endregion

        public CallbackCommand(CallbackEventHandler callback, bool shouldPauseGame = false, float lifeTime = float.MaxValue) :
            base(shouldPauseGame, lifeTime)
        {
            Callback += callback;
        }

        #region Virtual Functions

        public override void Begin()
        {
            base.Begin();

            Callback?.Invoke();
            Die();
        }

        #endregion
    }
}
