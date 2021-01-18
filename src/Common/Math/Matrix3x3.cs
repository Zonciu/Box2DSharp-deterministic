namespace Box2DSharp.Common
{
    public struct Matrix3x3
    {
        public FVector3 Ex;

        public FVector3 Ey;

        public FVector3 Ez;

        /// Construct this matrix using columns.
        public Matrix3x3(in FVector3 c1, in FVector3 c2, in FVector3 c3)
        {
            Ex = c1;
            Ey = c2;
            Ez = c3;
        }

        /// Set this matrix to all zeros.
        public void SetZero()
        {
            Ex.SetZero();
            Ey.SetZero();
            Ez.SetZero();
        }

        /// Solve A * x = b, where b is a column vector. This is more efficient
        /// than computing the inverse in one-shot cases.
        public FVector3 Solve33(in FVector3 b)
        {
            var det = FVector3.Dot(Ex, FVector3.Cross(Ey, Ez));
            if (!det.Equals(0.0f))
            {
                det = 1.0f / det;
            }

            FVector3 x;
            x.X = det * FVector3.Dot(b,  FVector3.Cross(Ey, Ez));
            x.Y = det * FVector3.Dot(Ex, FVector3.Cross(b, Ez));
            x.Z = det * FVector3.Dot(Ex, FVector3.Cross(Ey, b));
            return x;
        }

        /// Solve A * x = b, where b is a column vector. This is more efficient
        /// than computing the inverse in one-shot cases. Solve only the upper
        /// 2-by-2 matrix equation.
        public FVector2 Solve22(in FVector2 b)
        {
            var a11 = Ex.X;
            var a12 = Ey.X;
            var a21 = Ex.Y;
            var a22 = Ey.Y;

            var det = a11 * a22 - a12 * a21;
            if (!det.Equals(0.0f))
            {
                det = 1.0f / det;
            }

            FVector2 x;
            x.X = det * (a22 * b.X - a12 * b.Y);
            x.Y = det * (a11 * b.Y - a21 * b.X);
            return x;
        }

        /// Get the inverse of this matrix as a 2-by-2.
        /// Returns the zero matrix if singular.
        public void GetInverse22(ref Matrix3x3 matrix3X3)
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

            matrix3X3.Ex.X = det * d;
            matrix3X3.Ey.X = -det * b;
            matrix3X3.Ex.Z = 0.0f;
            matrix3X3.Ex.Y = -det * c;
            matrix3X3.Ey.Y = det * a;
            matrix3X3.Ey.Z = 0.0f;
            matrix3X3.Ez.X = 0.0f;
            matrix3X3.Ez.Y = 0.0f;
            matrix3X3.Ez.Z = 0.0f;
        }

        /// Get the symmetric inverse of this matrix as a 3-by-3.
        /// Returns the zero matrix if singular.
        public void GetSymInverse33(ref Matrix3x3 matrix3X3)
        {
            var det = FVector3.Dot(Ex, FVector3.Cross(Ey, Ez));
            if (!det.Equals(0.0f))
            {
                det = 1.0f / det;
            }

            var a11 = Ex.X;
            var a12 = Ey.X;
            var a13 = Ez.X;
            var a22 = Ey.Y;
            var a23 = Ez.Y;
            var a33 = Ez.Z;

            matrix3X3.Ex.X = det * (a22 * a33 - a23 * a23);
            matrix3X3.Ex.Y = det * (a13 * a23 - a12 * a33);
            matrix3X3.Ex.Z = det * (a12 * a23 - a13 * a22);

            matrix3X3.Ey.X = matrix3X3.Ex.Y;
            matrix3X3.Ey.Y = det * (a11 * a33 - a13 * a13);
            matrix3X3.Ey.Z = det * (a13 * a12 - a11 * a23);

            matrix3X3.Ez.X = matrix3X3.Ex.Z;
            matrix3X3.Ez.Y = matrix3X3.Ey.Z;
            matrix3X3.Ez.Z = det * (a11 * a22 - a12 * a12);
        }
    }
}