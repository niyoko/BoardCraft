namespace BoardCraft.Drawing
{
    using System;
    using System.Collections.Generic;

    public sealed class Transform
    {
        private readonly Stack<Matrix> _matrixStack;

        /// <summary>
        ///     Create an instance of <see cref="Transform" />
        /// </summary>
        public Transform()
        {
            _matrixStack = new Stack<Matrix>(25);
            Reset();
        }

        public Matrix Matrix { get; set; }

        public void Translate(double dx, double dy)
        {
            var m = new Matrix(1, 0, 0, 1, dx, dy);
            Multiply(m);
        }

        public void Rotate(double alpha)
        {
            var sin = Math.Sin(alpha);
            var cos = Math.Cos(alpha);

            var m = new Matrix(cos, -sin, sin, cos, 0, 0);
            Multiply(m);
        }

        public void Scale(double scaleX, double scaleY)
        {
            var m = new Matrix(scaleX, 0, 0, scaleY, 0, 0);
            Multiply(m);
        }

        public void Scale(double scale)
        {
            Scale(scale, scale);
        }

        public void Multiply(Matrix matrix)
        {
            Matrix = Matrix * matrix;
        }

        /// <summary>
        ///     Reset transformation matrix and clear matrix stack
        /// </summary>
        public void Reset()
        {
            Matrix = Matrix.Identity;
            _matrixStack.Clear();
        }

        /// <summary>
        ///     Push current transformation matrix into matrix stack
        /// </summary>
        public void PushMatrix()
        {
            _matrixStack.Push(Matrix);
        }

        /// <summary>
        ///     Pop matrix from matrix stack and use it as transformation matrix
        /// </summary>
        public void PopMatrix()
        {
            try
            {
                Matrix = _matrixStack.Pop();
            }
            catch (InvalidOperationException exc)
            {
                throw new InvalidOperationException("Matrix stack is empty", exc);
            }
        }
    }
}