using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// This class is a base class for any UI objects in the game.  
    /// It is marked abstract because we do not want to create an instance of this class - it is too generic
    /// </summary>
    public class UIObject : BaseObject
    {
        #region Properties and Fields

        /// <summary>
        /// An object which can be used to store values.
        /// Useful for buttons etc.
        /// </summary>
        public object StoredObject { get; set; }

        /// <summary>
        /// A class specific SpriteFont which can be used to draw text.
        /// </summary>
        private SpriteFont spriteFont;
        protected SpriteFont SpriteFont
        {
            get
            {
                if (spriteFont == null)
                {
                    spriteFont = AssetManager.GetSpriteFont(AssetManager.DefaultSpriteFontAsset);
                }

                return spriteFont;
            }
            set { spriteFont = value; }
        }

        /// <summary>
        /// The rectangle we will use to cut off non-visible sections of our Children
        /// </summary>
        protected virtual Rectangle ScissorRectangle
        {
            get
            {
                return new Rectangle(
                    (int)(WorldPosition.X - Size.X * 0.5f),
                    (int)(WorldPosition.Y - Size.Y * 0.5f),
                    (int)Size.X,
                    (int)Size.Y);
            }
        }

        /// <summary>
        /// A flag to indicate whether we will be scissoring off our sub objects.
        /// By default set to false.
        /// </summary>
        public bool UseScissorRectangle { private get; set; }

        /// <summary>
        /// The rasterizer state for this object
        /// </summary>
        private RasterizerState RasterizerState { get; set; }

        #endregion

        public UIObject(Vector2 localPosition, string textureAsset) :
            this(Vector2.Zero, localPosition, textureAsset)
        {
        }

        public UIObject(Vector2 size, Vector2 localPosition, string textureAsset) :
            base(size, localPosition, textureAsset)
        {

        }

        public UIObject(Anchor anchor, int depth, string textureAsset) :
            this(Vector2.Zero, anchor, depth, textureAsset)
        {
        }

        public UIObject(Vector2 size, Anchor anchor, int depth, string textureAsset) :
            base(size, anchor, depth, textureAsset)
        {

        }

        #region Virtual Functions

        /// <summary>
        /// Initialises the RasterizerState in case we are using scissoring
        /// </summary>
        public override void Initialise()
        {
            CheckShouldInitialise();

            RasterizerState = new RasterizerState() { ScissorTestEnable = true };

            base.Initialise();
        }

        /// <summary>
        /// Draws using our scissor rectangle if we have it enabled.
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            // Otherwise, draw the base, but turn don't draw the children if we are using the scissor rectangle
            // This is so we don't scissor off this object's texture - only the children
            Children.ShouldDraw = !UseScissorRectangle;
            base.Draw(spriteBatch);

            // If we are using the scissor rectangle then use the rasterizer state and scissor rectangle to limit what is drawn
            if (UseScissorRectangle)
            {
                Rectangle previousScissorRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;

                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, RasterizerState);

                spriteBatch.GraphicsDevice.ScissorRectangle = ScissorRectangle;

                // Now set the children to be drawn in our scissor section
                Children.ShouldDraw = true;
                Children.Draw(spriteBatch);

                spriteBatch.End();

                spriteBatch.Begin();

                spriteBatch.GraphicsDevice.ScissorRectangle = previousScissorRectangle;
            }
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// A function which expands this object to the size which contains all the objects inside of it plus an inputted border padding
        /// </summary>
        /// <param name="padding"></param>
        public void CalculateWrappingSize(Vector2 border)
        {
            // Should not call this function before we have loaded and initialised all our objects
            Debug.Assert(ShouldLoad == false);
            Debug.Assert(ShouldInitialise == false);

            Vector2 size = Size;

            // Because we sometimes cannot set the position before we set the size, all objects MUST be parented to this container if we are calling this function
            foreach (UIObject uiObject in Children)
            {
                Debug.Assert(uiObject.Parent == this);
                size.X = MathHelper.Max(size.X, 2 * (Math.Abs(uiObject.LocalPosition.X) + border.X) + uiObject.Size.X);
                size.Y = MathHelper.Max(size.Y, 2 * (Math.Abs(uiObject.LocalPosition.Y) + border.Y) + uiObject.Size.Y);
            }

            Size = size;
        }

        #endregion
    }
}
