namespace _2DEngine
{
    public interface IModuleCompatible
    {
        /// <summary>
        /// Adds a module to this component and sets up the AttachedComponent on the module.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="module"></param>
        /// <param name="load"></param>
        /// <param name="initialise"></param>
        /// <returns></returns>
        T AddModule<T>(T module, bool load = false, bool initialise = false) where T : Module;

        /// <summary>
        /// A function for finding a module registered with a Component.
        /// We are guaranteed to only have one module of each type registered.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T FindModule<T>() where T : Module;
    }
}