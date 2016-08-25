using System.Collections.Generic;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// A class that represents a certain animation and a list of transitions to other possible states.
    /// </summary>
    public class State
    {
        #region Properties and Fields

        /// <summary>
        /// The animation associated with this state.
        /// </summary>
        public AnimationModule Animation { get; set; }

        /// <summary>
        /// A list of transitions to other states
        /// </summary>
        public List<Transition> Transitions { get; set; }

        /// <summary>
        /// A bool property to indicate whether this state is global or not.
        /// If set, the state will be marked in the state machine as global and be always checked on behaviour changes.
        /// </summary>
        public bool IsGlobal { get; private set; }

        /// <summary>
        /// The state ID of this state.  Corresponds to an enum value in a class deriving from Character.
        /// </summary>
        public uint StateID { get; private set; }

        #endregion

        public State(uint stateID, AnimationModule animation)
        {
            StateID = stateID;
            Animation = animation;
            IsGlobal = animation.IsGlobal;

            Transitions = new List<Transition>();
        }

        #region State Update Functions

        public void Update(float elapsedGameTime)
        {
            Animation.Update(elapsedGameTime);
        }

        #endregion

        #region State Transition Functions

        /// <summary>
        /// Iterates through the transitions and checks each condition.
        /// </summary>
        /// <returns>Returns true if we have found a state to transition to, otherwise false.</returns>
        public bool CheckTransitions(uint newBehaviourState)
        {
            foreach (Transition transition in Transitions)
            {
                if (transition.DestinationState == newBehaviourState)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Transition Utility Functions

        /// <summary>
        /// Adds a transition between this and the state with the inputted state ID.
        /// </summary>
        /// <param name="destinationStateID">The ID of the state we will transition to.</param>
        public void AddTransition(uint destinationStateID)
        {
            Debug.Assert(destinationStateID != StateID);
            Debug.Assert(!Transitions.Exists(x => x.DestinationState == destinationStateID));

            Transitions.Add(new Transition(destinationStateID));
        }

        #endregion
    }
}