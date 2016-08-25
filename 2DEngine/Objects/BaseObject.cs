using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace _2DEngine
{
    public enum Anchor
    {
        kTop = 1 << 0,
        kBottom = 1 << 1,
        kLeft = 1 << 2,
        kRight = 1 << 3,
        kCentre = 1 << 4,
        kTopLeft = kTop | kLeft,
        kTopCentre = kTop | kCentre,
        kTopRight = kTop | kRight,
        kCentreLeft = kCentre | kLeft,
        kCentreRight = kCentre | kRight,
        kBottomLeft = kBottom | kLeft,
        kBottomCentre = kBottom | kCentre,
        kBottomRight = kBottom | kRight,
    }

    /// <summary>
    /// The base class for any UI or game objects in our game.
    /// Marked as abstract, because we should not be able to create an instance of this class.
    /// Instead we should create a UIObject or GameObject
    /// </summary>
    public abstract class BaseObject : Component, IContainer<BaseObject>, IModuleCompatible, IEnumerable
    {
        #region Properties and Fields

        /// <summary>
        /// A string to store the texture asset for this object
        /// </summary>
        public string TextureAsset { get; set; }

        /// <summary>
        /// The texture for this object - override if we have different textures that need to be drawn at different times
        /// </summary>
        private Texture2D texture;
        protected virtual Texture2D Texture
        {
            get
            {
                if (texture == null)
                {
                    Debug.Assert(!string.IsNullOrEmpty(TextureAsset));
                    texture = AssetManager.GetSprite(TextureAsset);
                }

                return texture;
            }
            set { texture = value; }
        }

        /// <summary>
        /// This is a cached vector that will only be set once.  Used in the draw method to indicate the dimensions of the texture.
        /// Will be set when the texture is loaded ONLY.
        /// </summary>
        private Vector2 TextureDimensions { get; set; }

        /// <summary>
        /// This is a cached vector that will only be set once.  Used in the draw method to indicate the centre of the texture.
        /// Will be set when the texture is loaded ONLY.
        /// </summary>
        public virtual Vector2 TextureCentre { get; set; }

        /// <summary>
        /// A source rectangle used to specify a sub section of the Texture2D to draw.
        /// Useful for animations and bars and by default set to (0, 0, texture width, texture height).
        /// </summary>
        public Rectangle SourceRectangle { get; set; }

        /// <summary>
        /// An object which we can parent this object off of.  Positions and rotations are then relative to this object.
        /// However, no extraction is done
        /// </summary>
        private BaseObject parent;
        public BaseObject Parent
        {
            get { return parent; }

            // Shouldn't be able to set outside of this class - either use AddChild or Reparent to keep Children inline
            private set
            {
                // Set the actual base object rather than the property - this will not change
                parent = value;
                calculateWorldPosition = true;

                // Set up the index in our parent's children
                if (parent != null)
                {
                    IndexInParent = parent.ChildrenCount;
                }
                else
                {
                    IndexInParent = -1;
                }
            }
        }

        /// <summary>
        /// An object manager for the children of this object
        /// </summary>
        protected ObjectManager<BaseObject> Children { get; private set; }

        /// <summary>
        /// A flag to work out whether we have a previous sibling so we do not call PreviousSibling badly
        /// </summary>
        public bool HasPreviousSibling
        {
            get
            {
                return Parent != null && IndexInParent > 0 && IndexInParent <= Parent.Children.ActiveObjectsCount - 1;
            }
        }

        /// <summary>
        /// A flag to work out whether we have a next sibling so we do not call PreviousSibling badly
        /// </summary>
        public bool HasNextSibling
        {
            get
            {
                return Parent != null && IndexInParent >= 0 && IndexInParent < Parent.Children.ActiveObjectsCount - 1;
            }
        }

        /// <summary>
        /// Gets the next sibling of this base object in it's parent's Children
        /// </summary>
        public BaseObject NextSibling
        {
            get
            {
                DebugUtils.AssertNotNull(Parent);
                Debug.Assert(HasNextSibling);
                return Parent.Children.ChildAtIndex(IndexInParent + 1);
            }
        }

        /// <summary>
        /// Gets the previous sibling of this base object it it's parent's Children
        /// </summary>
        public BaseObject PreviousSibling
        {
            get
            {
                DebugUtils.AssertNotNull(Parent);
                Debug.Assert(HasPreviousSibling);
                return Parent.Children.ChildAtIndex(IndexInParent - 1);
            }
        }

        /// <summary>
        /// A function to indicate the index we hold in our Parent's Children - used for getting siblings
        /// </summary>
        private int IndexInParent { get; set; }

        /// <summary>
        /// Returns the number of children this object has (active and to add)
        /// </summary>
        public int ChildrenCount { get { return Children.ActiveObjectsCount + Children.ObjectsToAddCount; } }

        /// <summary>
        /// The relative anchor to the parent.
        /// This anchor will determine where the centre of this object is placed in relation to the edge of the rectangle created by the size of the parent.
        /// </summary>
        private Anchor Anchor { get; set; }

        /// <summary>
        /// Determines where the centre of this object is placed in terms of it's anchor
        /// </summary>
        private int Depth { get; set; }

        /// <summary>
        /// The local offset from the parent.
        /// Cannot do LocalPosition.X = x, because Vector2 is a struct.
        /// Instead, do LocalPosition = new Vector2(x, LocalPosition.Y).
        /// </summary>
        private Vector2 localPosition;
        public Vector2 LocalPosition
        {
            get { return localPosition; }
            set
            {
                // If we flagged we need to recalculate world position, or our local position has changed, we should calculate our world position again
                calculateWorldPosition = calculateWorldPosition || localPosition != value;
                localPosition = value;
            }
        }

        /// <summary>
        /// The local rotation from the parent's rotation - this value is bound between -PI and PI
        /// </summary>
        private float localRotation;
        public float LocalRotation
        {
            get { return localRotation; }
            set
            {
                // Wrap the angle between -PI and PI
                float tempLocalRotation = MathHelper.WrapAngle(value);

                // If we flagged we need to recalculate world position, or our local rotation has changed, we should calculate our world position again
                calculateWorldPosition = calculateWorldPosition || tempLocalRotation != localRotation;

                localRotation = tempLocalRotation;
            }
        }

        /// <summary>
        /// The world space position of the object
        /// </summary>
        private bool calculateWorldPosition;
        private Vector2 worldPosition;
        private Vector2 parentWorldPosition;
        public Vector2 WorldPosition
        {
            get
            {
                // If we have flagged we need recalculating, or our parent world position has changed, we recalculate the world position
                if (calculateWorldPosition || (Parent != null && parentWorldPosition != Parent.WorldPosition))
                {
                    CalculateWorldPosition();
                }

                return worldPosition;
            }
        }

        /// <summary>
        /// The world space rotation, calculated recursively using the parent's WorldRotation.
        /// This value will be between -PI and PI
        /// </summary>
        public float WorldRotation
        {
            get
            {
                // If we have no parent, return the local rotation
                if (Parent == null)
                {
                    return LocalRotation;
                }

                // Wrap the angle between -PI and PI
                return MathHelper.WrapAngle(Parent.WorldRotation + localRotation);
            }
        }

        /// <summary>
        /// The size of this object.  By default this will be the size of the Texture.
        /// </summary>
        public Vector2 Size { get; set; }

        /// <summary>
        /// The colour of the object - by default this is set to white, so that white in the png will appear transparent
        /// </summary>
        public virtual Color Colour { get; set; }

        /// <summary>
        /// The opacity of the object - between 0 and 1.  A value of 0 makes the texture completely transparent, and 1 completely opaque
        /// </summary>
        public float Opacity { get; set; }

        /// <summary>
        /// A property that can be used to reverse an image - useful for animations or sprites that are facing just one way.
        /// By default, this is SpriteEffects.None.
        /// </summary>
        protected SpriteEffects SpriteEffect { get; set; }

        /// <summary>
        /// A bool to indicate whether we should add a collider during initialisation.
        /// Some objects (like text) do not need a collider - this is an optimisation step.
        /// </summary>
        public bool UsesCollider { get; set; }

        /// <summary>
        /// The collider associated with this object.  Also, is responsible for mouse interactions.
        /// Done like this for interface reasons.  Any object which implements ICollisionObject will need a read only Collider property
        /// </summary>
        private Collider collider;
        public Collider Collider { get { return collider; } }

        /// <summary>
        /// An object manager for the modules of this object
        /// </summary>
        private ObjectManager<Module> Modules { get; set; }

        #endregion

        public BaseObject(Vector2 localPosition, string textureAsset) :
            base()
        {
            Anchor = Anchor.kCentre;
            Depth = 0;
            LocalPosition = localPosition;
            TextureAsset = textureAsset;
            Opacity = 1;
            UsesCollider = true;
            SpriteEffect = SpriteEffects.None;
            Colour = Color.White;

            Children = new ObjectManager<BaseObject>();
            Modules = new ObjectManager<Module>();
        }

        public BaseObject(Vector2 size, Vector2 localPosition, string textureAsset) :
            this(localPosition, textureAsset)
        {
            Size = size;
        }

        public BaseObject(Anchor anchor, int depth, string textureAsset) :
            base()
        {
            Anchor = anchor;
            Depth = depth;
            TextureAsset = textureAsset;
            Opacity = 1;
            UsesCollider = true;
            SpriteEffect = SpriteEffects.None;
            Colour = Color.White;

            Children = new ObjectManager<BaseObject>();
            Modules = new ObjectManager<Module>();
        }

        public BaseObject(Vector2 size, Anchor anchor, int depth, string textureAsset) :
            this(anchor, depth, textureAsset)
        {
            Size = size;
        }

        #region Virtual Functions

        /// <summary>
        /// Check that the texture has been loaded by doing a get call
        /// </summary>
        public override void LoadContent()
        {
            // Check to see whether we should load
            CheckShouldLoad();

            DebugUtils.AssertNotNull(Texture);
            Children.LoadContent();
            Modules.LoadContent();

            base.LoadContent();
        }

        /// <summary>
        /// Set up the size if it has not been set already.
        /// Updates the local position using the anchor and depth.
        /// Adds the collider if it should.
        /// </summary>
        public override void Initialise()
        {
            CheckShouldInitialise();

            if (Texture != null)
            {
                TextureDimensions = new Vector2(Texture.Bounds.Width, Texture.Bounds.Height);
                TextureCentre = new Vector2(Texture.Bounds.Center.X, Texture.Bounds.Center.Y);

                // Set the source rectangle to the default size of the texture
                SourceRectangle = new Rectangle(
                     0, 0,
                     (int)TextureDimensions.X,
                     (int)TextureDimensions.Y);
            }

            // If our size is zero (i.e. uninitialised) we use the texture's size (if it is not null)
            if (Size == Vector2.Zero && Texture != null)
            {
                Size = new Vector2(Texture.Bounds.Width, Texture.Bounds.Height);
            }

            // Check to see whether we have a non-trivial case for our positioning
            if (Anchor != Anchor.kCentre || Depth != 0)
            {
                // If we are using relative anchors the Parent cannot be null
                Debug.Assert(Parent != null);
                if (Anchor.HasFlag(Anchor.kCentre))
                {
                    if (Anchor.HasFlag(Anchor.kLeft) || Anchor.HasFlag(Anchor.kRight))
                    {
                        float xMultiplier = Anchor.HasFlag(Anchor.kLeft) ? -1 : 1;
                        LocalPosition = new Vector2(0.5f * xMultiplier * (Parent.Size.X + Depth * Size.X), 0);
                    }
                    else
                    {
                        float yMultiplier = Anchor.HasFlag(Anchor.kTop) ? -1 : 1;
                        LocalPosition = new Vector2(0, 0.5f * yMultiplier * (Parent.Size.X + Depth * Size.Y));
                    }
                }
                else
                {
                    float yMultiplier = Anchor.HasFlag(Anchor.kTop) ? -1 : 1;
                    LocalPosition = new Vector2(0, 0.5f * yMultiplier * (Parent.Size.Y + Depth * Size.Y));

                    if (Anchor.HasFlag(Anchor.kLeft))
                    {
                        LocalPosition -= new Vector2(0.5f * (Parent.Size.X + Depth * Size.X));
                    }
                    else if (Anchor.HasFlag(Anchor.kRight))
                    {
                        LocalPosition += new Vector2(0.5f * (Parent.Size.X + Depth * Size.Y));
                    }
                }
            }

            Children.Initialise();
            Modules.Initialise();

            base.Initialise();
        }

        /// <summary>
        /// By default returns a RectangleCollider for this object if it's bool UsesCollider is set to true.
        /// Can be overridden to return custom colliders instead.
        /// </summary>
        /// <returns>The collider we wish this object to have</returns>
        protected virtual Collider AddCollider()
        {
            return new RectangleCollider(this);
        }

        /// <summary>
        /// A function which updates the collider per frame.
        /// Can be overridden to provide custom behaviour - i.e. for objects which use an animation.
        /// </summary>
        /// <param name="position">The position we wish the collider to be centred at</param>
        /// <param name="size">The dimensions of the collider</param>
        public virtual void UpdateCollider(ref Vector2 position, ref Vector2 size)
        {
            position = WorldPosition;
            size = Size;
        }

        /// <summary>
        /// Calls begin on our children and adds a collider
        /// </summary>
        public override void Begin()
        {
            base.Begin();

            Children.Begin();
            Modules.Begin();

            if (UsesCollider)
            {
                // Adds the collider if the flag is true
                // Do this here so thathe size is properly set up etc.
                collider = AddCollider();

                DebugUtils.AssertNotNull(Collider);
            }
        }

        /// <summary>
        /// Update the collider's mouse state variables
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        /// <param name="mousePosition"></param>
        public override void HandleInput(float elapsedGameTime, Vector2 mousePosition)
        {
            base.HandleInput(elapsedGameTime, mousePosition);

            if (UsesCollider)
            {
                DebugUtils.AssertNotNull(Collider);
                Collider.HandleInput(mousePosition);
            }

            if (Children.ShouldHandleInput)
            {
                Children.HandleInput(elapsedGameTime, mousePosition);
            }

            if (Modules.ShouldHandleInput)
            {
                Modules.HandleInput(elapsedGameTime, mousePosition);
            }
        }

        /// <summary>
        /// Updates the object's collider if it has one
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public override void Update(float elapsedGameTime)
        {
            base.Update(elapsedGameTime);

            if (UsesCollider)
            {
                // Update the collider position and state variables
                DebugUtils.AssertNotNull(Collider);
                Collider.Update();
            }

            if (Children.ShouldUpdate)
            {
                Children.Update(elapsedGameTime);
            }

            if (Modules.ShouldUpdate)
            {
                Modules.Update(elapsedGameTime);
            }
        }

        /// <summary>
        /// Draws the object's texture.
        /// If we wish to create an object, but not draw it, change it's ShouldDraw property
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            // If we are drawing this object, it should have a valid texture
            // If we wish to create an object but not draw it, simply change it's ShouldDraw property
            DebugUtils.AssertNotNull(Texture);
            spriteBatch.Draw(
                Texture,
                WorldPosition,
                null,
                SourceRectangle,
                TextureCentre,
                WorldRotation,
                Vector2.Divide(Size, TextureDimensions),
                Colour * Opacity,
                SpriteEffect,
                0);

            if (Children.ShouldDraw)
            {
                Children.Draw(spriteBatch);
            }
        }

        /// <summary>
        /// We must make sure that we show the children if necessary
        /// </summary>
        /// <param name="showChildren">A flag to indicate whether the children should be shown</param>
        public override void Show(bool showChildren = true)
        {
            base.Show();

            Children.Show(showChildren);
            Modules.Show(showChildren);
        }

        /// <summary>
        /// We must make sure that we hide the children if necessary
        /// </summary>
        /// <param name="hideChildren">A flag to indicate whether the children should be hidden</param>
        public override void Hide(bool hideChildren = true)
        {
            base.Hide();

            Children.Hide(hideChildren);
            Modules.Hide(hideChildren);
        }

        /// <summary>
        /// We must make sure that we explicitly call Die on each child.
        /// The IsAlive attributes are connected up, but this will not result in a call to Die.
        /// </summary>
        public override void Die()
        {
            base.Die();

            Children.Die();
            Modules.Die();
        }

        #endregion

        #region Extra non-virtual IContainer functions

        /// <summary>
        /// A function which will be used to add a child and sets it's parent to this
        /// </summary>
        /// <typeparam name="K">The type of the child</typeparam>
        /// <param name="childToAdd">The child itself</param>
        /// <param name="load">A flag to indicate whether we wish to call LoadContent on the child</param>
        /// <param name="initialise">A flag to indicate whether we wish to call Initialise on the child</param>
        /// <returns></returns>
        public virtual K AddChild<K>(K childToAdd, bool load = false, bool initialise = false) where K : BaseObject
        {
            DebugUtils.AssertNotNull(childToAdd);
            DebugUtils.AssertNull(childToAdd.Parent);

            // Set the parent to be this
            childToAdd.Parent = this;

            return Children.AddChild(childToAdd, load, initialise);
        }

        /// <summary>
        /// A function to remove a child
        /// </summary>
        /// <param name="childToRemove">The child we wish to remove</param>
        public void RemoveChild(BaseObject childToRemove)
        {
            DebugUtils.AssertNotNull(childToRemove);

            // This function will set IsAlive to false so that the object gets cleaned up next Update loop
            Children.RemoveChild(childToRemove);
        }

        /// <summary>
        /// Extracts the inputted child from this container, but keeps it alive for insertion into another
        /// </summary>
        /// <param name="childToExtract">The child we wish to extract from this container</param>
        public T ExtractChild<T>(T childToExtract) where T : BaseObject
        {
            DebugUtils.AssertNotNull(childToExtract);
            childToExtract.Parent = null;

            return Children.ExtractChild(childToExtract);
        }

        /// <summary>
        /// Searches through the children and returns all that match the inputted type
        /// </summary>
        /// <typeparam name="T">The inputted type we will use to find objects</typeparam>
        /// <returns>All the objects we have found of the inputted type</returns>
        public List<T> GetChildrenOfType<T>(bool includeObjectsToAdd = false) where T : BaseObject
        {
            return Children.GetChildrenOfType<T>(includeObjectsToAdd);
        }

        /// <summary>
        /// Finds an object of the inputted name and casts to the inputted type K.
        /// First searches the ActiveObjects and then the ObjectsToAdd
        /// </summary>
        /// <typeparam name="K">The type we wish to return the found object as</typeparam>
        /// <param name="name">The name of the object we wish to find</param>
        /// <returns>Returns the object casted to K or null</returns>
        public K FindChild<K>() where K : BaseObject
        {
            return Children.FindChild<K>();
        }

        /// <summary>
        /// Finds an object of the inputted name and casts to the inputted type T.
        /// First searches the ActiveObjects and then the ObjectsToAdd
        /// </summary>
        /// <typeparam name="T">The type we wish to return the found object as</typeparam>
        /// <param name="predicate">The predicate we will use to find our object</param>
        /// <returns>Returns the object casted to T or null</returns>
        public T FindChild<T>(Predicate<BaseObject> predicate) where T : BaseObject
        {
            return Children.FindChild<T>(predicate);
        }

        /// <summary>
        /// Returns whether an object satisfying the inputted predicate exists within our children.
        /// </summary>
        /// <param name="predicate">The predicate the child must satisfy</param>
        /// <returns>True if such a child exists and false if not</returns>
        public bool Exists(Predicate<BaseObject> predicate)
        {
            return Children.Exists(predicate);
        }

        /// <summary>
        /// Returns the first child.
        /// Shouldn't really be called unless we have children
        /// </summary>
        /// <returns>The first child we added</returns>
        public BaseObject FirstChild()
        {
            return Children.FirstChild();
        }

        /// <summary>
        /// Returns the most recent child we added which is castable to the inputted type.
        /// Shouldn't really be called unless we have children
        /// </summary>
        /// <returns>The most recent child we added</returns>
        public T LastChild<T>() where T : BaseObject
        {
            return Children.LastChild<T>();
        }

        /// <summary>
        /// Returns the most recent child we added which is castable to the inputted type and satisfies the inputted condition.
        /// Shouldn't really be called unless we have children
        /// </summary>
        /// <returns>The most recent child we added</returns>
        public T LastChild<T>(Predicate<T> condition) where T : BaseObject
        {
            return Children.LastChild<T>(condition);
        }

        #endregion

        #region IModuleCompatible Functions

        /// <summary>
        /// Adds a module to this base object and sets up the AttachedComponent on the module.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="module"></param>
        /// <param name="load"></param>
        /// <param name="initialise"></param>
        /// <returns></returns>
        public T AddModule<T>(T module, bool load = false, bool initialise = false) where T : Module
        {
            // Set up reference to attached component
            module.AttachedComponent = this;

            // Checks to make sure we are not adding another copy of the same type of module - should only have one
            Debug.Assert(!Modules.Exists(x => x.GetType() == typeof(T)));
            return Modules.AddChild(module, load, initialise);
        }

        /// <summary>
        /// A function for finding a module registered with this BaseObject.
        /// We are guaranteed to only have one module of each type registered.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FindModule<T>() where T : Module
        {
            Debug.Assert(Modules.Exists(x => x.GetType() == typeof(T)));
            return Modules.FindChild<T>(x => x.GetType() == typeof(T));
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Resizes this object and it's children by the inputted scale.
        /// Also multiplies the local position of the children so that the relative positions are the same.
        /// </summary>
        /// <param name="scale"></param>
        public void Scale(Vector2 scale)
        {
            Size *= scale;

            foreach (BaseObject child in Children)
            {
                child.LocalPosition *= scale;
                child.Scale(scale);
            }
        }

        /// <summary>
        /// Resizes this object and it's children by the inputted scale.
        /// Also multiplies the local position of the children so that the relative positions are the same.
        /// </summary>
        /// <param name="scale"></param>
        public void Scale(float scale)
        {
            Scale(new Vector2(scale));
        }

        /// <summary>
        /// A utility function which wraps the extraction of this from it's current owner and the insertion into the new one.
        /// Should only be called if it's parent is already set - otherwise use AddChild.
        /// The second parameter is a flag we can pass to prevent us from calling newParent.AddChild - this is in case we do not want to call custom implementation in an AddChild function.
        /// This does mean that if the object is not reinserted manually elsewhere, it will be left hanging around, but not being updated.
        /// </summary>
        /// <param name="newParent"></param>
        public void ReparentTo(BaseObject newParent, bool moveToNewParentsChildren = true)
        {
            // Extract ourselves from our parent
            DebugUtils.AssertNotNull(Parent);

            // We are reparenting to ourselves, so no need to go through this whole process of extraction and addition again
            if (Parent == newParent)
            {
                return;
            }

            Parent.ExtractChild(this);

            // Assume this has been loaded and initialised and insert it into the new parent's Children (if not null)
            if (newParent != null)
            {
                if (moveToNewParentsChildren)
                {
                    newParent.AddChild(this);
                }
                else
                {
                    // We do not want to call add child, but rather do a shallow parent set
                    Parent = newParent;
                }
            }
        }

        /// <summary>
        /// Calculate the world position for this object using it's local rotation and position from it's parent
        /// </summary>
        private void CalculateWorldPosition()
        {
            if (Parent == null)
            {
                worldPosition = LocalPosition;
            }
            else
            {
                // This syntax is for optimisation
                worldPosition = Vector2.Add(Parent.WorldPosition, Vector2.Transform(LocalPosition, Matrix.CreateRotationZ(WorldRotation)));
                parentWorldPosition = Parent.WorldPosition;
            }

            calculateWorldPosition = false;
        }

        /// <summary>
        /// An enumerator for the Children of this BaseObject
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return Children.GetEnumerator();
        }

        #endregion
    }
}