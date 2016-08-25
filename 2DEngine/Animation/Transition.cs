using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// A class representing a condition between two animation states.
    /// </summary>
    public class Transition
    {
        #region Properties and Fields

        /// <summary>
        /// The ID of the state that we wish to transition to.
        /// </summary>
        public uint DestinationState { get; private set; }

        #endregion

        public Transition(uint destinationState)
        {
            DestinationState = destinationState;
        }
    }
}
