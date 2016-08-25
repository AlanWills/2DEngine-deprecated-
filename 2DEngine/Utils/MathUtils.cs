using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace _2DEngine
{
    public static class MathUtils
    {
        public static float PiOver4 = MathHelper.PiOver4;
        public static float ThreePiOver4 = MathHelper.Pi - MathHelper.PiOver4;
        public static float FivePiOver4 = MathHelper.Pi + MathHelper.PiOver4;
        public static float SevenPiOver4 = MathHelper.TwoPi - MathHelper.PiOver4;

        public static float ThreePiOver2 = 1.5f * MathHelper.Pi;

        private static Random Random = new Random();

        /// <summary>
        /// Generates a random float between the min and max values
        /// </summary>
        /// <param name="min">The minimum value the float can attain</param>
        /// <param name="max">The maximum value the float can attain</param>
        /// <returns>A random number between min and max inclusive</returns>
        public static float GenerateFloat(float min, float max)
        {
            Debug.Assert(min <= max);
            float randFloat = (float)Random.NextDouble();

            randFloat *= (max - min);
            randFloat += min;

            return randFloat;
        }

        /// <summary>
        /// Generates a random int between the min and max values
        /// </summary>
        /// <param name="inclusiveMin">The minimum value that can be attained (inclusive)</param>
        /// <param name="inclusiveMax">The maximum value that can be attained (inclusive)</param>
        /// <returns>A random int between the min and max values</returns>
        public static int GenerateInt(int inclusiveMin, int inclusiveMax)
        {
            Debug.Assert(inclusiveMin <= inclusiveMax);

            return Random.Next(inclusiveMin, inclusiveMax + 1);
        }

        /// <summary>
        /// Calculates the angle between two points by analysis the angle between their difference vector and the upwards vector(0, -1) centre at point2.
        /// Therefore, the result will be the clockwise angle from (0, -1) to the difference vector point2 - point1.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static float AngleBetweenPoints(Vector2 point1, Vector2 point2)
        {
            Vector2 diff = point2 - point1;
            diff.Normalize();
            bool lessThanPi = diff.X >= 0;
            Vector2 normal = Vector2.Negate(Vector2.UnitY);

            float result = (float)Math.Acos(Vector2.Dot(normal, diff));
            
            if (lessThanPi)
            {
                return result;
            }
            else
            {
                return MathHelper.TwoPi - result;
            }
        }

        /// <summary>
        /// Checks whether an inputted angle is in the correct range to have been contained within the top of a rectangle.
        /// </summary>
        /// <param name="collisionAngle"></param>
        /// <returns></returns>
        public static bool CheckCollisionFromAbove(float collisionAngle)
        {
            // || for first condition because angle wrapped over 2PI.
            return (collisionAngle <= PiOver4 || collisionAngle >= SevenPiOver4);
        }

        public static bool CheckCollisionFromBelow(float collisionAngle)
        {
            /// <summary>
            /// Checks whether an inputted angle is in the correct range to have been contained within the bottom of a rectangle.
            /// </summary>
            /// <param name="collisionAngle"></param>
            /// <returns></returns>
            return (collisionAngle >= ThreePiOver4 && collisionAngle <= FivePiOver4);
        }

        /// <summary>
        /// Checks whether an inputted angle is in the correct range to have been contained within the left of a rectangle.
        /// </summary>
        /// <param name="collisionAngle"></param>
        /// <returns></returns>
        public static bool CheckCollisionFromLeft(float collisionAngle)
        {
            return (collisionAngle >= FivePiOver4 && collisionAngle <= SevenPiOver4);
        }

        /// <summary>
        /// Checks whether an inputted angle is in the correct range to have been contained within the right of a rectangle.
        /// </summary>
        /// <param name="collisionAngle"></param>
        /// <returns></returns>
        public static bool CheckCollisionFromRight(float collisionAngle)
        {
            return (collisionAngle >= PiOver4 && collisionAngle <= ThreePiOver4);
            
        }
    }
}