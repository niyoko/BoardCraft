namespace BoardCraft.Drawing
{
    using System;

    /// <summary>
    ///     Representing 3x3 transformation matrix.
    ///     <code>
    /// [ M11  M12  Dx ] <para />
    /// [ M21  M22  Dy ] <para />
    /// [  0    0    1 ] <para />
    /// </code>
    /// </summary>
    public struct Matrix
    {
        public static readonly Matrix Identity = new Matrix(1, 0, 0, 1, 0, 0);
        public static readonly Matrix Zero = new Matrix();

        public Matrix(double m11, double m12, double m21, double m22, double dx, double dy)
        {
            M11 = m11;
            M12 = m12;
            M21 = m21;
            M22 = m22;
            Dx = dx;
            Dy = dy;
        }

        /// <summary>
        ///     First-row first-column element.
        /// </summary>
        public double M11 { get; }

        /// <summary>
        ///     First-row second-column element.
        /// </summary>
        public double M12 { get; }

        /// <summary>
        ///     Second-row first-column element.
        /// </summary>
        public double M21 { get; }

        /// <summary>
        ///     Second row-second column element.
        /// </summary>
        public double M22 { get; } // second row second column

        public double Dx { get; } // first row third column?

        public double Dy { get; } // second row third column?

        public double Determinant => (M11 * M22) - (M21 * M12);

        public bool HasInverse => Math.Abs(Determinant) > 1E-14;

        public static Matrix operator *(Matrix left, Matrix right)
        {
            var m11 = (left.M11 * right.M11) + (left.M12 * right.M21);
            var m12 = (left.M11 * right.M12) + (left.M12 * right.M22);
            var dx = (left.M11 * right.Dx) + (left.M12 * right.Dy) + left.Dx;
            var m21 = (left.M21 * right.M11) + (left.M22 * right.M21);
            var m22 = (left.M21 * right.M12) + (left.M22 * right.M22);
            var dy = (left.M21 * right.Dx) + (left.M22 * right.Dy) + left.Dy;

            return new Matrix(m11, m12, m21, m22, dx, dy);
        }

        public static Matrix operator +(Matrix left, Matrix right)
        {
            var m11 = left.M11 + right.M11;
            var m12 = left.M12 + right.M12;
            var dx = left.Dx + right.Dx;
            var m21 = left.M21 + right.M21;
            var m22 = left.M22 + right.M22;
            var dy = left.Dy + right.Dy;

            return new Matrix(m11, m12, m21, m22, dx, dy);
        }

        public static Matrix operator -(Matrix left, Matrix right)
        {
            var m11 = left.M11 - right.M11;
            var m12 = left.M12 - right.M12;
            var dx = left.Dx - right.Dx;
            var m21 = left.M21 - right.M21;
            var m22 = left.M22 - right.M22;
            var dy = left.Dy - right.Dy;

            return new Matrix(m11, m12, m21, m22, dx, dy);
        }

        public static Matrix operator -(Matrix matrix)
        {
            return Zero - matrix;
        }
    }
}