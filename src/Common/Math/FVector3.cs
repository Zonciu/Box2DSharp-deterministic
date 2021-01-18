using System;
using System.Runtime.CompilerServices;

namespace Box2DSharp.Common
{
    public struct FVector3 : IEquatable<FVector3>
    {
        private static FP ZeroEpsilonSq = FMath.Epsilon;

        internal static FVector3 InternalZero;

        internal static FVector3 Arbitrary;

        #region Static readonly variables

        /// <summary>
        /// A vector with components (0,0,0);
        /// </summary>
        public static readonly FVector3 Zero;

        /// <summary>
        /// A vector with components (-1,0,0);
        /// </summary>
        public static readonly FVector3 Left;

        /// <summary>
        /// A vector with components (1,0,0);
        /// </summary>
        public static readonly FVector3 Right;

        /// <summary>
        /// A vector with components (0,1,0);
        /// </summary>
        public static readonly FVector3 Up;

        /// <summary>
        /// A vector with components (0,-1,0);
        /// </summary>
        public static readonly FVector3 Down;

        /// <summary>
        /// A vector with components (0,0,-1);
        /// </summary>
        public static readonly FVector3 Back;

        /// <summary>
        /// A vector with components (0,0,1);
        /// </summary>
        public static readonly FVector3 Forward;

        /// <summary>
        /// A vector with components (1,1,1);
        /// </summary>
        public static readonly FVector3 One;

        /// <summary>
        /// A vector with components 
        /// (FP.MinValue,FP.MinValue,FP.MinValue);
        /// </summary>
        public static readonly FVector3 MinValue;

        /// <summary>
        /// A vector with components 
        /// (FP.MaxValue,FP.MaxValue,FP.MaxValue);
        /// </summary>
        public static readonly FVector3 MaxValue;

        #endregion

        #region Private static constructor

        static FVector3()
        {
            One = new FVector3(1, 1, 1);
            Zero = new FVector3(0, 0, 0);
            Left = new FVector3(-1, 0, 0);
            Right = new FVector3(1, 0, 0);
            Up = new FVector3(0, 1, 0);
            Down = new FVector3(0, -1, 0);
            Back = new FVector3(0, 0, -1);
            Forward = new FVector3(0, 0, 1);
            MinValue = new FVector3(FP.MinValue);
            MaxValue = new FVector3(FP.MaxValue);
            Arbitrary = new FVector3(1, 1, 1);
            InternalZero = Zero;
        }

        #endregion

        #region Public Fields

        public FP X;

        public FP Y;

        public FP Z;

        #endregion Public Fields

        #region Constructors

        /// <summary>
        /// Constructor foe standard 3D vector.
        /// </summary>
        public FVector3(FP x, FP y, FP z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Constructor for "square" vector.
        /// </summary>
        public FVector3(FP value)
        {
            X = value;
            Y = value;
            Z = value;
        }

        public void Set(FP x, FP y, FP z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        #endregion Constructors

        #region Public Methods

        /// <inheritdoc />
        public bool Equals(FVector3 other)
        {
            return X.Equals(other.X)
                && Y.Equals(other.Y)
                && Z.Equals(other.Z);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is FVector3 other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Returns the dot product of two vectors.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>The dot product.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FP Dot(FVector3 vector1, FVector3 vector2)
        {
            return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
        }

        /// <summary>
        /// Computes the cross product of two vectors.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>The cross product.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FVector3 Cross(FVector3 vector1, FVector3 vector2)
        {
            return new FVector3(
                vector1.Y * vector2.Z - vector1.Z * vector2.Y,
                vector1.Z * vector2.X - vector1.X * vector2.Z,
                vector1.X * vector2.Y - vector1.Y * vector2.X);
        }

        /// <summary>
        /// Returns a vector whose elements are the minimum of each of the pairs of elements in the two source vectors.
        /// </summary>
        /// <param name="value1">The first source vector.</param>
        /// <param name="value2">The second source vector.</param>
        /// <returns>The minimized vector.</returns>
        public static FVector3 Min(FVector3 value1, FVector3 value2)
        {
            return new FVector3(
                (value1.X < value2.X) ? value1.X : value2.X,
                (value1.Y < value2.Y) ? value1.Y : value2.Y,
                (value1.Z < value2.Z) ? value1.Z : value2.Z);
        }

        /// <summary>
        /// Returns a vector whose elements are the maximum of each of the pairs of elements in the two source vectors.
        /// </summary>
        /// <param name="value1">The first source vector.</param>
        /// <param name="value2">The second source vector.</param>
        /// <returns>The maximized vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FVector3 Max(FVector3 value1, FVector3 value2)
        {
            return new FVector3(
                (value1.X > value2.X) ? value1.X : value2.X,
                (value1.Y > value2.Y) ? value1.Y : value2.Y,
                (value1.Z > value2.Z) ? value1.Z : value2.Z);
        }

        /// <summary>
        /// Returns a vector whose elements are the absolute values of each of the source vector's elements.
        /// </summary>
        /// <param name="value">The source vector.</param>
        /// <returns>The absolute value vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FVector3 Abs(FVector3 value)
        {
            return new FVector3(FP.Abs(value.X), FP.Abs(value.Y), FP.Abs(value.Z));
        }

        /// <summary>
        /// Returns a vector whose elements are the square root of each of the source vector's elements.
        /// </summary>
        /// <param name="value">The source vector.</param>
        /// <returns>The square root vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FVector3 SquareRoot(FVector3 value)
        {
            return new FVector3(FP.Sqrt(value.X), FP.Sqrt(value.Y), FP.Sqrt(value.Z));
        }

        #endregion Public Static Methods

        #region Public Static Operators

        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The summed vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FVector3 operator +(FVector3 left, FVector3 right)
        {
            return new FVector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        /// <summary>
        /// Subtracts the second vector from the first.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The difference vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FVector3 operator -(FVector3 left, FVector3 right)
        {
            return new FVector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        /// <summary>
        /// Multiplies two vectors together.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The product vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FVector3 operator *(FVector3 left, FVector3 right)
        {
            return new FVector3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        }

        /// <summary>
        /// Multiplies a vector by the given scalar.
        /// </summary>
        /// <param name="left">The source vector.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The scaled vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FVector3 operator *(FVector3 left, FP right)
        {
            return left * new FVector3(right);
        }

        /// <summary>
        /// Multiplies a vector by the given scalar.
        /// </summary>
        /// <param name="left">The scalar value.</param>
        /// <param name="right">The source vector.</param>
        /// <returns>The scaled vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FVector3 operator *(FP left, FVector3 right)
        {
            return new FVector3(left) * right;
        }

        /// <summary>
        /// Divides the first vector by the second.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The vector resulting from the division.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FVector3 operator /(FVector3 left, FVector3 right)
        {
            return new FVector3(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
        }

        /// <summary>
        /// Divides the vector by the given scalar.
        /// </summary>
        /// <param name="value1">The source vector.</param>
        /// <param name="value2">The scalar value.</param>
        /// <returns>The result of the division.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FVector3 operator /(FVector3 value1, FP value2)
        {
            return value1 / new FVector3(value2);
        }

        /// <summary>
        /// Negates a given vector.
        /// </summary>
        /// <param name="value">The source vector.</param>
        /// <returns>The negated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FVector3 operator -(FVector3 value)
        {
            return Zero - value;
        }

        /// <summary>
        /// Returns a boolean indicating whether the two given vectors are equal.
        /// </summary>
        /// <param name="left">The first vector to compare.</param>
        /// <param name="right">The second vector to compare.</param>
        /// <returns>True if the vectors are equal; False otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(FVector3 left, FVector3 right)
        {
            return left.X == right.X && left.Y == right.Y && left.Z == right.Z;
        }

        /// <summary>
        /// Returns a boolean indicating whether the two given vectors are not equal.
        /// </summary>
        /// <param name="left">The first vector to compare.</param>
        /// <param name="right">The second vector to compare.</param>
        /// <returns>True if the vectors are not equal; False if they are equal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(FVector3 left, FVector3 right)
        {
            return !(left == right);
        }

        #endregion Public Static Operators
    }
}