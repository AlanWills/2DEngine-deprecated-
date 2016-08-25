using _2DEngineData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

namespace _2DEngine
{
    /// <summary>
    /// A class which generates a level using a custom algorithm.
    /// Makes a walkable layer, then adds hazards.
    /// Finally it fills in the above and below areas and adds lights.
    /// </summary>
    public class GenerationEngine
    {
        private enum Position
        {
            kLeft,
            kMiddle,
            kRight,
        }

        private enum Type
        {
            kWalkableLayer,
            kHazard,
            kBelowWalkableLayer,
            kAboveWalkableLayer
        }

        /// <summary>
        /// A reference to the level we will add the generated objects too
        /// </summary>
        private GameplayScreen LevelScreen { get; set; }

        /// <summary>
        /// The xml data for the level we will use in our generation algorithm
        /// </summary>
        private LevelGenerationData GenerationData { get; set; }

        /// <summary>
        /// Whether the previous object in our array was used 
        /// </summary>
        private bool PreviousObjectExists { get; set; }

        /// <summary>
        /// Whether we will currently add an object for this slot
        /// </summary>
        private bool CurrentObjectExists { get; set; }

        /// <summary>
        /// Whether we will add an object for the next slot
        /// </summary>
        private bool NextObjectExists { get; set; }

        /// <summary>
        /// The dimensions of our tiles
        /// </summary>
        private Vector2 TileDimensions { get; set; }

        /// <summary>
        /// An array of our walkable and hazard objects
        /// </summary>
        private UIObject[,] LevelObjects { get; set; }

        /// <summary>
        /// A list marking all of our walkable layer points (including hazards) and what type they were
        /// </summary>
        private List<KeyValuePair<Point, Type>> walkingLayerPoints = new List<KeyValuePair<Point, Type>>();
        private int currentHeight = 7;        

        /*
        Improvements :
            when max GenerationData.Height change is greater than one, we need to add colliders for the appropriate 'underneath' textures
        */

        public GenerationEngine(GameplayScreen levelScreen, string generationDataAsset)
        {
            LevelScreen = levelScreen;

            DebugUtils.AssertNotNull(generationDataAsset);
            GenerationData = AssetManager.GetData<LevelGenerationData>(generationDataAsset);
            DebugUtils.AssertNotNull(GenerationData);

            Texture2D tex = AssetManager.GetSprite(GenerationData.WalkableLayerMiddleTextureAsset);
            TileDimensions = new Vector2(tex.Width, tex.Height);

            LevelObjects = new UIObject[GenerationData.Height, GenerationData.Width];
        }

        /// <summary>
        /// Generates our walkable layer, hazards and background above and below layers.
        /// </summary>
        public void GenerateLevel()
        {
            GenerateWalkableLayer();
            GenerateHazards();
            GenerateBackgroundBelowWalkableLayer();
            GenerateBackgroundAboveWalkableLayer();

            AddObjectsInFrontOfBackground();
        }

        /// <summary>
        /// Adds the hazards and walkable layers to our LevelScreen Environment manager - this should happen after we add our background objects so the draw order is correct
        /// </summary>
        private void AddObjectsInFrontOfBackground()
        {
            for (int y = 0; y < GenerationData.Height; y++)
            {
                for (int x = 0; x < GenerationData.Width; x++)
                {
                    if (LevelObjects[y, x] != null)
                    {
                        // Do we want to add environment objects for these guys?
                        // Or should it be game objects?
                        LevelScreen.AddEnvironmentObject(LevelObjects[y, x]);
                    }
                }
            }
        }

        #region Walkable Layer

        /// <summary>
        /// Generate our main walkable layer for this level
        /// </summary>
        private void GenerateWalkableLayer()
        {
            UIObject previousObject = Setup();

            for (int x = 2; x < GenerationData.Width - 1; x++)
            {
                ShiftExistenceFlags();
                previousObject = Create(x);
            }

            TakeDown();
        }

        /// <summary>
        /// We should always have the first two tiles as added, so do this here
        /// </summary>
        /// <returns></returns>
        private UIObject Setup()
        {
            Vector2 screenDimensions = ScreenManager.Instance.ScreenDimensions;

            PreviousObjectExists = false;
            CurrentObjectExists = true;
            NextObjectExists = true;

            Create(0);

            //Camera.FocusOnPosition(LevelObjects[currentHeight, 0].WorldPosition, false);

            PreviousObjectExists = true;
            CurrentObjectExists = true;
            NextObjectExists = MathUtils.GenerateFloat(0, 1) <= GenerationData.WalkableLayerProbability;

            Create(1);

            return LevelObjects[currentHeight, 1];
        }

        /// <summary>
        /// We should also always add the last walkable tile too
        /// </summary>
        private void TakeDown()
        {
            PreviousObjectExists = CurrentObjectExists;
            CurrentObjectExists = true;
            NextObjectExists = false;

            Create(GenerationData.Width - 1);
        }

        /// <summary>
        /// Determines whether we should add an object based on our CurrentObjectExists flag.
        /// If so, we generate a possible height change using our probability and add our tile to our LevelObjects array
        /// </summary>
        /// <param name="xIndex"></param>
        /// <returns></returns>
        private UIObject Create(int xIndex)
        {
            if (!CurrentObjectExists)
            {
                LevelObjects[currentHeight, xIndex] = null;
            }
            else
            {
                float yDelta = 0;

                if (MathUtils.GenerateFloat(0, 1) <= GenerationData.HeightChangeProbability)
                {
                    int amount = MathUtils.GenerateInt(-GenerationData.MaximumHeightChange, GenerationData.MaximumHeightChange);

                    currentHeight += amount;
                    currentHeight = MathHelper.Clamp(currentHeight, 0, GenerationData.Height - 1);

                    yDelta = amount * TileDimensions.Y;
                }

                LevelObjects[currentHeight, xIndex] = new Image(new Vector2(TileDimensions.X * xIndex, TileDimensions.Y * currentHeight), QueryTextureAsset());
                LevelObjects[currentHeight, xIndex].StoredObject = QueryPositionType();

                Point thisPoint = new Point(xIndex, currentHeight);
                Debug.Assert(!walkingLayerPoints.Exists(x => x.Key == thisPoint));
                walkingLayerPoints.Add(new KeyValuePair<Point, Type>(thisPoint, Type.kWalkableLayer));
            }

            return LevelObjects[currentHeight, xIndex];
        }

        /// <summary>
        /// Shifts our flags determining what tiles have been created.
        /// </summary>
        private void ShiftExistenceFlags()
        {
            PreviousObjectExists = CurrentObjectExists;
            CurrentObjectExists = NextObjectExists;
            NextObjectExists = MathUtils.GenerateFloat(0, 1) <= GenerationData.WalkableLayerProbability;
        }

        /// <summary>
        /// Determines what type of tile we should create using our existence flags
        /// </summary>
        /// <returns></returns>
        private Position QueryPositionType()
        {
            if (PreviousObjectExists && NextObjectExists)
            {
                return Position.kMiddle;
            }
            else if (PreviousObjectExists && !NextObjectExists)
            {
                return Position.kRight;
            }
            else if (!PreviousObjectExists && NextObjectExists)
            {
                return Position.kLeft;
            }
            else
            {
                return Position.kMiddle;
            }
        }

        /// <summary>
        /// Returns the appropriate texture based on what type of tile we are creating
        /// </summary>
        /// <returns></returns>
        private string QueryTextureAsset()
        {
            switch (QueryPositionType())
            {
                case Position.kLeft:
                    return GenerationData.WalkableLayerLeftTextureAsset;

                case Position.kMiddle:
                    return GenerationData.WalkableLayerMiddleTextureAsset;

                case Position.kRight:
                    return GenerationData.WalkableLayerRightTextureAsset;

                default:
                    Debug.Fail("No texture asset for this direction");
                    return "";
            }
        }

        #endregion

        #region Hazards

        /// <summary>
        /// Create randomly selected hazards in the gaps of our walkable layer
        /// </summary>
        private void GenerateHazards()
        {
            Debug.Assert(GenerationData.Width > 2);

            List<KeyValuePair<Point, Type>> tempListOfHazards = new List<KeyValuePair<Point, Type>>();

            for (int index = 0; index < walkingLayerPoints.Count - 1; index++)
            {
                Point currentPoint = walkingLayerPoints[index].Key;
                Point nextPoint = walkingLayerPoints[index + 1].Key;

                int diff = nextPoint.X - currentPoint.X - 1;
                // Do max because increasing y is downwards
                int y = MathHelper.Max(nextPoint.Y, currentPoint.Y);

                // Use the same hazard texture for all the hazards in this block
                int numberOfHazards = GenerationData.HazardTextureAssets.Count;
                string chosenTexture = GenerationData.HazardTextureAssets[MathUtils.GenerateInt(0, numberOfHazards - 1)];

                while (diff > 0)
                {
                    Point thisPoint = new Point(nextPoint.X - diff, y);

                    LevelObjects[y, thisPoint.X] = new Image(new Vector2(thisPoint.X * TileDimensions.X, thisPoint.Y * TileDimensions.Y), chosenTexture);
                    LevelObjects[y, thisPoint.X].StoredObject = Position.kMiddle;
                    
                    Debug.Assert(!walkingLayerPoints.Exists(x => x.Key == thisPoint));
                    Debug.Assert(!tempListOfHazards.Exists(x => x.Key == thisPoint));
                    tempListOfHazards.Add(new KeyValuePair<Point, Type>(thisPoint, Type.kHazard));
                    diff--;
                }
            }

            walkingLayerPoints.AddRange(tempListOfHazards);
        }

        #endregion

        #region Background

        /// <summary>
        /// Generate background below our walkable layer and add them to our LevelScreen
        /// </summary>
        private void GenerateBackgroundBelowWalkableLayer()
        {
            Dictionary<Position, string> belowWalkableLayer = new Dictionary<Position, string>()
            {
                //{ Position.kLeft, "Level\\Tiles\\Tile (4)" },
                { Position.kLeft, GenerationData.AboveWalkableLayerTextureAsset },
                { Position.kMiddle, GenerationData.AboveWalkableLayerTextureAsset },
                { Position.kRight, GenerationData.AboveWalkableLayerTextureAsset },
                //{ Position.kRight, "Level\\Tiles\\Tile (6)" },
            };

            for (int index = 0; index < walkingLayerPoints.Count; index++)
            {
                Point point = walkingLayerPoints[index].Key;
                Position positionType = (Position)LevelObjects[point.Y, point.X].StoredObject;

                for (int y = point.Y + 1; y < GenerationData.Height; y++)
                {
                    Image addedObject = LevelScreen.AddEnvironmentObject(new Image(new Vector2(point.X * TileDimensions.X, y * TileDimensions.Y), belowWalkableLayer[positionType]));
                    addedObject.UsesCollider = false;
                }
            }
        }

        /// <summary>
        /// Generate background above our walkable layer and add them to our LevelScreen
        /// </summary>
        private void GenerateBackgroundAboveWalkableLayer()
        {
            int backgroundHeight = 4;
            int minY = walkingLayerPoints[0].Key.Y;

            string background = "Level\\Tiles\\BGTile (3)";

            while (walkingLayerPoints.Exists(x => x.Key.Y < minY))
            {
                minY = walkingLayerPoints.Find(x => x.Key.Y < minY).Key.Y;
            }

            for (int index = 0; index < walkingLayerPoints.Count; index++)
            {
                KeyValuePair<Point, Type> info = walkingLayerPoints[index];

                // Want to create a background image behind our hazard so need to increment the currentPoint y by 1 so we start underneath our hazard
                Point currentPoint = info.Key;
                if (info.Value == Type.kHazard)
                {
                    currentPoint.Y++;
                }

                for (int y = currentPoint.Y - 1; y > minY - 1 - backgroundHeight; y--)
                {
                    Image addedObject = LevelScreen.AddEnvironmentObject(new Image(new Vector2(currentPoint.X, y) * TileDimensions, background));
                    addedObject.UsesCollider = false;
                }

                if (index % 3 == 0)
                {
                    LevelScreen.AddLight(new PointLight(new Vector2(750, 750), new Vector2(currentPoint.X * TileDimensions.X, (minY - 1) * TileDimensions.Y), Color.White));
                }
            }
        }

        #endregion
    }
}