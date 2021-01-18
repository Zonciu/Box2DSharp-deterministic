using System.Runtime.CompilerServices;

namespace Box2DSharp.Common
{
    public struct Matrix2x2
    {
        public FVector2 Ex;

        public FVector2 Ey;

        /// The default constructor does nothing (for performance).
        /// Construct this matrix using columns.
        public Matrix2x2(in FVector2 c1, in FVector2 c2)
        {
            Ex = c1;
            Ey = c2;
        }

        /// Construct this matrix using scalars.
        public Matrix2x2(FP a11, FP a12, FP a21, FP a22)
        {
            Ex.X = a11;
            Ex.Y = a21;
            Ey.X = a12;
            Ey.Y = a22;
        }

        /// Initialize this matrix using columns.
        public void Set(in FVector2 c1, in FVector2 c2)
        {
            Ex = c1;
            Ey = c2;
        }

        /// Set this to the identity matrix.
        public void SetIdentity()
        {
            Ex.X = 1.0f;
            Ey.X = 0.0f;
            Ex.Y = 0.0f;
            Ey.Y = 1.0f;
        }

        /// Set this matrix to all zeros.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetZero()
        {
            Ex.X = 0.0f;
            Ey.X = 0.0f;
            Ex.Y = 0.0f;
            Ey.Y = 0.0f;
        }

        public Matrix2x2 GetInverse()
        {
            var a = Ex.X;
            var b = Ey.X;
            var c = Ex.Y;
            var d = Ey.Y;

            var det = a * d - b * c;
            if (!det.Equals(0.0f))
            {
                det = 1.0f / det;
            }

            var B = new Matrix2x2();
            B.Ex.X = det * d;
            B.Ey.X = -det * b;
            B.Ex.Y = -det * c;
            B.Ey.Y = det * a;
            return B;
        }

        /// Solve A * x = b, where b is a column vector. This is more efficient
        /// than computing the inverse in one-shot cases.
        public FVector2 Solve(in FVector2 b)
        {
            var a11 = Ex.X;
            var a12 = Ey.X;
            var a21 = Ex.Y;
            var a22 = Ey.Y;
            var det = a11 * a22 - a12 * a21;
            if (det != 0)
            {
                det = 1.0f / det;
            }

            var x = new FVector2 {X = det * (a22 * b.X - a12 * b.Y), Y = det * (a11 * b.Y - a21 * b.X)};
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix2x2 operator +(in Matrix2x2 A, in Matrix2x2 B)
        {
            return new Matrix2x2(A.Ex + B.Ex, A.Ey + B.Ey);
        }
    }
}