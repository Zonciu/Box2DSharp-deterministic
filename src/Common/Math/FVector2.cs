#region License

/*
MIT License
Copyright © 2006 The Mono.Xna Team

All rights reserved.

Authors
 * Alan McGovern

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

#endregion License

using System;
using System.Runtime.CompilerServices;

namespace Box2DSharp.Common
{
    [Serializable]
    public struct FVector2 : IEquatable<FVector2>
    {
        #region Private Fields

        private static FVector2 _zeroVector = new FVector2(0, 0);

        private static FVector2 _oneVector = new FVector2(1, 1);

        private static FVector2 _rightVector = new FVector2(1, 0);

        private static FVector2 _leftVector = new FVector2(-1, 0);

        private static FVector2 _upVector = new FVector2(0, 1);

        private static FVector2 _downVector = new FVector2(0, -1);

        #endregion Private Fields

        #region Public Fields

        public FP X;

        public FP Y;

        #endregion Public Fields

        #region Properties

        public static FVector2 Zero
        {
            get { return _zeroVector; }
        }

        public static FVector2 One
        {
            get { return _oneVector; }
        }

        public static FVector2 Right
        {
            get { return _rightVector; }
        }

        public static FVector2 Left
        {
            get { return _leftVector; }
        }

        public static FVector2 Up
        {
            get { return _upVector; }
        }

        public static FVector2 Down
        {
            get { return _downVector; }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructor foe standard 2D vector.
        /// </summary>
        /// <param name="x">
        /// A <see cref="System.Single"/>
        /// </param>
        /// <param name="y">
        /// A <see cref="System.Single"/>
        /// </param>
        public FVector2(FP x, FP y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Constructor for "square" vector.
        /// </summary>
        /// <param name="value">
        /// A <see cref="System.Single"/>
        /// </param>
        public FVector2(FP value)
        {
            X = value;
            Y = value;
        }

        public void Set(FP x, FP y)
        {
            this.X = x;
            this.Y = y;
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <value>The component at <paramref name="index"/>.</value>
        /// <remarks>
        /// The index is zero based: x = vector[0], y = vector[1].
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The <paramref name="index"/> is out of range.
        /// </exception>
        public FP this[int index]
        {
            get
            {
                switch (index)
                {
                case 0:
                    return X;
                case 1:
                    return Y;
                default:
                    throw new ArgumentOutOfRangeException("index", "The index is out of range. Allowed values are 0 or 1.");
                }
            }
            set
            {
                switch (index)
                {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("index", "The index is out of range. Allowed values are 0 or 1");
                }
            }
        }

        public static void Reflect(ref FVector2 vector, ref FVector2 normal, out FVector2 result)
        {
            FP dot = Dot(vector, normal);
            result.X = vector.X - ((2f * dot) * normal.X);
            result.Y = vector.Y - ((2f * dot) * normal.Y);
        }

        public static FVector2 Reflect(FVector2 vector, FVector2 normal)
        {
            FVector2 result;
            Reflect(ref vector, ref normal, out result);
            return result;
        }

        public static FVector2 Add(FVector2 value1, FVector2 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            return value1;
        }

        public static void Add(ref FVector2 value1, ref FVector2 value2, out FVector2 result)
        {
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
        }


        public static FVector2 Clamp(FVector2 value1, FVector2 min, FVector2 max)
        {
            return new FVector2(
                FMath.Clamp(value1.X, min.X, max.X),
                FMath.Clamp(value1.Y, min.Y, max.Y));
        }

        public static void Clamp(ref FVector2 value1, ref FVector2 min, ref FVector2 max, out FVector2 result)
        {
            result = new FVector2(
                FMath.Clamp(value1.X, min.X, max.X),
                FMath.Clamp(value1.Y, min.Y, max.Y));
        }

        /// <summary>
        /// Returns FP precison distanve between two vectors
        /// </summary>
        /// <param name="value1">
        /// A <see cref="FVector2"/>
        /// </param>
        /// <param name="value2">
        /// A <see cref="FVector2"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.Single"/>
        /// </returns>
        public static FP Distance(FVector2 value1, FVector2 value2)
        {
            FP result;
            DistanceSquared(ref value1, ref value2, out result);
            return (FP)FP.Sqrt(result);
        }

        public static void Distance(ref FVector2 value1, ref FVector2 value2, out FP result)
        {
            DistanceSquared(ref value1, ref value2, out result);
            result = (FP)FP.Sqrt(result);
        }

        public static FP DistanceSquared(FVector2 value1, FVector2 value2)
        {
            FP result;
            DistanceSquared(ref value1, ref value2, out result);
            return result;
        }

        public static void DistanceSquared(ref FVector2 value1, ref FVector2 value2, out FP result)
        {
            result = (value1.X - value2.X) * (value1.X - value2.X) + (value1.Y - value2.Y) * (value1.Y - value2.Y);
        }

        /// <summary>
        /// Devide first vector with the secund vector
        /// </summary>
        /// <param name="value1">
        /// A <see cref="FVector2"/>
        /// </param>
        /// <param name="value2">
        /// A <see cref="FVector2"/>
        /// </param>
        /// <returns>
        /// A <see cref="FVector2"/>
        /// </returns>
        public static FVector2 Divide(FVector2 value1, FVector2 value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            return value1;
        }

        public static void Divide(ref FVector2 value1, ref FVector2 value2, out FVector2 result)
        {
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
        }

        public static FVector2 Divide(FVector2 value1, FP divider)
        {
            FP factor = 1 / divider;
            value1.X *= factor;
            value1.Y *= factor;
            return value1;
        }

        public static void Divide(ref FVector2 value1, FP divider, out FVector2 result)
        {
            FP factor = 1 / divider;
            result.X = value1.X * factor;
            result.Y = value1.Y * factor;
        }

        public static FVector2 Abs(FVector2 value)
        {
            return new FVector2(FP.Abs(value.X), FP.Abs(value.Y));
        }

        public static FP Dot(FVector2 value1, FVector2 value2)
        {
            return value1.X * value2.X + value1.Y * value2.Y;
        }

        public static void Dot(ref FVector2 value1, ref FVector2 value2, out FP result)
        {
            result = value1.X * value2.X + value1.Y * value2.Y;
        }

        public override bool Equals(object obj)
        {
            return (obj is FVector2) ? this == ((FVector2)obj) : false;
        }

        public bool Equals(FVector2 other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return (int)(X + Y);
        }

        public static FVector2 Hermite(FVector2 value1, FVector2 tangent1, FVector2 value2, FVector2 tangent2, FP amount)
        {
            FVector2 result = new FVector2();
            Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount, out result);
            return result;
        }

        public static void Hermite(
            ref FVector2 value1,
            ref FVector2 tangent1,
            ref FVector2 value2,
            ref FVector2 tangent2,
            FP amount,
            out FVector2 result)
        {
            result.X = FMath.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount);
            result.Y = FMath.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount);
        }

        public FP magnitude
        {
            get
            {
                FP result;
                DistanceSquared(ref this, ref _zeroVector, out result);
                return FP.Sqrt(result);
            }
        }

        // Reference: http://forum.photonengine.com/discussion/10422/truesync-tsvector-clampmagnitude-is-inconsistent-with-unityengine-vector3-clampmagnitude
        public static FVector2 ClampMagnitude(FVector2 vector, FP maxLength)
        {
            FVector2 result;
            if (vector.LengthSquared() > maxLength * maxLength)
            {
                result = vector.normalized * maxLength;
            }
            else
            {
                result = vector;
            }

            return result;
        }

        /// <summary>
        /// Returns the length of the vector.
        /// </summary>
        /// <returns>The vector's length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FP Length()
        {
            var ls = Dot(this, this);
            return FP.Sqrt(ls);
        }

        public FP LengthSquared()
        {
            // Todo need to check
            FP result;
            DistanceSquared(ref this, ref _zeroVector, out result);
            return result;
        }

        public static FVector2 Lerp(FVector2 value1, FVector2 value2, FP amount)
        {
            amount = FMath.Clamp(amount, 0, 1);

            return new FVector2(
                FMath.Lerp(value1.X, value2.X, amount),
                FMath.Lerp(value1.Y, value2.Y, amount));
        }

        public static FVector2 LerpUnclamped(FVector2 value1, FVector2 value2, FP amount)
        {
            return new FVector2(
                FMath.Lerp(value1.X, value2.X, amount),
                FMath.Lerp(value1.Y, value2.Y, amount));
        }

        public static void LerpUnclamped(ref FVector2 value1, ref FVector2 value2, FP amount, out FVector2 result)
        {
            result = new FVector2(
                FMath.Lerp(value1.X, value2.X, amount),
                FMath.Lerp(value1.Y, value2.Y, amount));
        }

        public static FVector2 Max(FVector2 value1, FVector2 value2)
        {
            return new FVector2(
                FMath.Max(value1.X, value2.X),
                FMath.Max(value1.Y, value2.Y));
        }

        public static void Max(ref FVector2 value1, ref FVector2 value2, out FVector2 result)
        {
            result.X = FMath.Max(value1.X, value2.X);
            result.Y = FMath.Max(value1.Y, value2.Y);
        }

        public static FVector2 Min(FVector2 value1, FVector2 value2)
        {
            return new FVector2(
                FMath.Min(value1.X, value2.X),
                FMath.Min(value1.Y, value2.Y));
        }

        public static void Min(ref FVector2 value1, ref FVector2 value2, out FVector2 result)
        {
            result.X = FMath.Min(value1.X, value2.X);
            result.Y = FMath.Min(value1.Y, value2.Y);
        }

        public void Scale(FVector2 other)
        {
            this.X = X * other.X;
            this.Y = Y * other.Y;
        }

        public static FVector2 Scale(FVector2 value1, FVector2 value2)
        {
            FVector2 result;
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;

            return result;
        }

        public static FVector2 Multiply(FVector2 value1, FVector2 value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            return value1;
        }

        public static FVector2 Multiply(FVector2 value1, FP scaleFactor)
        {
            value1.X *= scaleFactor;
            value1.Y *= scaleFactor;
            return value1;
        }

        public static void Multiply(ref FVector2 value1, FP scaleFactor, out FVector2 result)
        {
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
        }

        public static void Multiply(ref FVector2 value1, ref FVector2 value2, out FVector2 result)
        {
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
        }

        public static FVector2 Negate(FVector2 value)
        {
            value.X = -value.X;
            value.Y = -value.Y;
            return value;
        }

        public static void Negate(ref FVector2 value, out FVector2 result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
        }

        // public void Normalize()
        // {
        //     Normalize(ref this, out this);
        // }

        public static FVector2 Normalize(FVector2 value)
        {
            Normalize(ref value, out value);
            return value;
        }

        public FVector2 normalized
        {
            get
            {
                FVector2 result;
                FVector2.Normalize(ref this, out result);

                return result;
            }
        }

        /// Convert this vector into a unit vector. Returns the length.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FP Normalize()
        {
            return Normalize(ref this, out this);
        }

        public static FP Normalize(ref FVector2 value, out FVector2 result)
        {
            FP factor;
            DistanceSquared(ref value, ref _zeroVector, out factor);
            var length = FP.Sqrt(factor);
            if (length < Settings.Epsilon)
            {
                result = value;
                return FP.Zero;
            }

            factor = 1f / length;
            result.X = value.X * factor;
            result.Y = value.Y * factor;
            return length;
        }

        public static FVector2 SmoothStep(FVector2 value1, FVector2 value2, FP amount)
        {
            return new FVector2(
                FMath.SmoothStep(value1.X, value2.X, amount),
                FMath.SmoothStep(value1.Y, value2.Y, amount));
        }

        public static void SmoothStep(ref FVector2 value1, ref FVector2 value2, FP amount, out FVector2 result)
        {
            result = new FVector2(
                FMath.SmoothStep(value1.X, value2.X, amount),
                FMath.SmoothStep(value1.Y, value2.Y, amount));
        }

        public static FVector2 Subtract(FVector2 value1, FVector2 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            return value1;
        }

        public static void Subtract(ref FVector2 value1, ref FVector2 value2, out FVector2 result)
        {
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
        }

        public static FP Angle(FVector2 a, FVector2 b)
        {
            return FP.Acos(a.normalized * b.normalized) * FP.Rad2Deg;
        }

        public FVector3 ToTSVector()
        {
            return new FVector3(this.X, this.Y, 0);
        }

        public override string ToString()
        {
            return $"({X.AsFloat}, {Y.AsFloat}";
        }

        #endregion Public Methods

        #region Operators

        public static FVector2 operator -(FVector2 value)
        {
            value.X = -value.X;
            value.Y = -value.Y;
            return value;
        }

        public static bool operator ==(FVector2 value1, FVector2 value2)
        {
            return value1.X == value2.X && value1.Y == value2.Y;
        }

        public static bool operator !=(FVector2 value1, FVector2 value2)
        {
            return value1.X != value2.X || value1.Y != value2.Y;
        }

        public static FVector2 operator +(FVector2 value1, FVector2 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            return value1;
        }

        public static FVector2 operator -(FVector2 value1, FVector2 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            return value1;
        }

        public static FP operator *(FVector2 value1, FVector2 value2)
        {
            return FVector2.Dot(value1, value2);
        }

        public static FVector2 operator *(FVector2 value, FP scaleFactor)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            return value;
        }

        public static FVector2 operator *(FP scaleFactor, FVector2 value)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            return value;
        }

        public static FVector2 operator /(FVector2 value1, FVector2 value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            return value1;
        }

        public static FVector2 operator /(FVector2 value1, FP divider)
        {
            FP factor = 1 / divider;
            value1.X *= factor;
            value1.Y *= factor;
            return value1;
        }

        #endregion Operators
    }
}