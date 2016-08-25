namespace _2DEngineData
{
    /// <summary>
    /// A base class for all data - probably empty, but useful for clarity
    /// </summary>
    public class BaseData
    {
        public T As<T>() where T : BaseData
        {
            return this as T;
        }
    }
}