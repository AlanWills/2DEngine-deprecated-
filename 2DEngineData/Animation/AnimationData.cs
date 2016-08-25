namespace _2DEngineData
{
    /// <summary>
    /// A class to hold all the information necessary for an animation.
    /// </summary>
    public class AnimationData : GameObjectData
    {
        /// <summary>
        /// The number of frames in X.
        /// </summary>
        public int TextureFramesX { get; set; }

        /// <summary>
        /// The number of frames in Y.
        /// </summary>
        public int TextureFramesY { get; set; }

        /// <summary>
        /// A flag to indicate whether the animation should repeat or not.
        /// </summary>
        public bool Continual { get; set; }

        /// <summary>
        /// A flag to indicate whether this animation will be global in our state machine.
        /// Assign true if it should be reachable from every other state in our state machine.
        /// </summary>
        public bool IsGlobal { get; set; }

        /// <summary>
        /// An amount to offset the animation by for animations that are not central
        /// </summary>
        public float FixupX { get; set; }
        public float FixupY { get; set; }

        /// <summary>
        /// Specifies where the collider centre is as a proportion of the animation frame dimensions - Zero means the centre remains unchanged
        /// </summary>
        public float ColliderCentrePositionOffsetX { get; set; }

        public float ColliderCentrePositionOffsetY { get; set; }

        /// <summary>
        /// Specifies the width and heigh of the collider as a proportion of the animation frame
        /// </summary>
        public float ColliderWidthProportion { get; set; }

        public float ColliderHeightProportion { get; set; }
    }
}