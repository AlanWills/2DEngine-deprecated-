using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2DEngine
{
    /// <summary>
    /// A class to manager our game's lights and lighting effects.
    /// Use ShouldDraw to determine whether we should use lights or not.
    /// </summary>
    public class LightManager : ObjectManager<Light>
    {
        #region Properties and Fields

        /// <summary>
        /// The effect we will use to draw our lighting.
        /// </summary>
        public Effect LightEffect { get; private set; }

        /// <summary>
        /// The ambient light in our scene that will affect all the objects in our game world.
        /// Only one of these exists.
        /// </summary>
        public AmbientLight AmbientLightReference { get; private set; }

        /// <summary>
        /// The colour we will use to reset our LightRenderTarget to create ambient light.
        /// </summary>
        public Color AmbientLight
        {
            get
            {
                return AmbientLightReference.Colour * AmbientLightReference.Opacity;
            }
        }

        private const string defaultLightEffect = "LightEffect";

        #endregion

        public LightManager() :
            base()
        {
            AddChild(new AmbientLight(Color.Black, 1));
        }

        #region Virtual Functions

        /// <summary>
        /// Loads the effect for the lighting.
        /// </summary>
        public override void LoadContent()
        {
            CheckShouldLoad();

            LightEffect = AssetManager.GetEffect(defaultLightEffect);
            DebugUtils.AssertNotNull(LightEffect);

            base.LoadContent();
        }

        /// <summary>
        /// Adds a light, but if it is an ambient light then we replace our current ambient light with a new one.
        /// </summary>
        /// <param name="objectToAdd">The light to add</param>
        /// <param name="load">Whether we should call load content</param>
        /// <param name="initialise">Whether we should initialise</param>
        public override T AddChild<T>(T objectToAdd, bool load = false, bool initialise = false)
        {
            if (objectToAdd is AmbientLight)
            {
                // Remove our old ambient light
                if (AmbientLightReference != null)
                {
                    RemoveChild(AmbientLightReference);
                }

                // Set our new ambient light
                AmbientLightReference = objectToAdd as AmbientLight;
            }

            return base.AddChild(objectToAdd, load, initialise);
        }

        #endregion
    }
}
