using System.Collections.Generic;
using System.Threading;

namespace _2DEngine
{
    public static class ThreadManager
    {
        /// <summary>
        /// A list of our current threads in use.
        /// </summary>
        private static List<CustomThread> ActiveThreads { get; set; }

        /// <summary>
        /// The main thread for our game.
        /// </summary>
        public static Thread MainThread { get; private set; }

        /// <summary>
        /// Initialises any static properties for the ThreadManager.
        /// </summary>
        public static void Initialise()
        {
            ActiveThreads = new List<CustomThread>();
            MainThread = Thread.CurrentThread;
        }

        /// <summary>
        /// Removes all our threads which have been 
        /// </summary>
        public static void Update()
        {
            foreach (CustomThread thread in ActiveThreads)
            {
                thread.Update();
            }

            ActiveThreads.RemoveAll(x => !x.IsAlive);
        }

        /// <summary>
        /// Creates and starts a thread for our thread manager to keep track of.
        /// </summary>
        /// <param name="functionToRun">The function the thread should run.</param>
        /// <returns>The created thread</returns>
        public static CustomThread CreateThread(ThreadStart functionToRun)
        {
            CustomThread thread = new CustomThread(functionToRun);
            ActiveThreads.Add(thread);

            return thread;
        }

        /// <summary>
        /// Creates and starts a thread for our thread manager with a callback to run when completed.
        /// </summary>
        /// <param name="functionToRun">The function the thread should run.</param>
        /// <param name="functionToRunOnComplete">The function the thread should run when it completes it's main task.</param>
        /// <returns>The created thread</returns>
        public static CustomThread CreateThread(ThreadStart functionToRun, OnThreadTaskComplete functionToRunOnComplete)
        {
            CustomThread thread = CreateThread(functionToRun);
            thread.OnThreadTaskComplete += functionToRunOnComplete;

            return thread;
        }
    }
}
