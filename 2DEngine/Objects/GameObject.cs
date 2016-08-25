using _2DEngineData;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace _2DEngine
{
    public class GameObject : BaseObject
    {
        #region Properties and Fields

        /// <summary>
        /// A string used to store the data asset for this game object.
        /// </summary>
        protected string DataAsset { get; private set; }

        /// <summary>
        /// The data associated with this game object.  Not all game objects will have data, but most should.
        /// Loads when called for the first time - we may want to get the data before we have reached this class' LoadContent.
        /// Setting this also sets the TextureAsset.
        /// </summary>
        private GameObjectData data;
        public GameObjectData Data
        {
            get
            {
                if (data == null)
                {
                    // Set the property rather than the private field so we also set the texture asset
                    Data = AssetManager.GetData<GameObjectData>(DataAsset);
                }

                return data;
            }
            protected set
            {
                data = value;

                if (data != null)
                {
                    TextureAsset = data.TextureAsset;
                }
            }
        }

        /// <summary>
        /// A potentially null physics body used to apply physics to this GameObject
        /// </summary>
        public PhysicsBody PhysicsBody { get; private set; }

        #endregion

        public GameObject(Vector2 localPosition, string dataAsset) :
            base(localPosition, "")
        {
            DataAsset = dataAsset;
        }

        #region Virtual Functions

        /// <summary>
        /// Performs JUST debug checks on the data and texture asset.
        /// Then makes sure our texture asset is set for loading
        /// </summary>
        public override void LoadContent()
        {
            // Check to see if we should load
            CheckShouldLoad();

            // Either we have a valid data asset and valid data or we have set a texture asset
            if (!string.IsNullOrEmpty(DataAsset))
            {
                // Make sure we always load and set the texture asset - even with no asserts
                if (Data == null)
                {
                    Debug.Fail("Data cannot be null");
                }
            }
            else
            {
                // If we have got in here, the data asset was not specified and so our texture asset was manually set.
                // Should check that this is true.
                Debug.Assert(Data != null || !string.IsNullOrEmpty(TextureAsset));
            }

            // This will handle the loading if not done so already.
            base.LoadContent();
        }

        /// <summary>
        /// Calls Die on the object if it has insufficient health.
        /// </summary>
        /// <param name="elapsedGameTime"></param>
        public override void Update(float elapsedGameTime)
        {
            base.Update(elapsedGameTime);

            if (PhysicsBody != null) { PhysicsBody.Update(elapsedGameTime); }
        }


        /// <summary>
        /// A virtual function which is explicitly called by another object when it collides with this game object.
        /// Override to perform custom behaviour on collision.
        /// </summary>
        /// <param name="collidedObject"></param>
        public virtual void OnCollisionWith(GameObject collidedObject) { }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Add a physics body to this object
        /// </summary>
        protected void AddPhysicsBody()
        {
            PhysicsBody = new PhysicsBody(this);
        }

        #endregion
    }
}
