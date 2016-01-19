namespace BoardCraft.Output.Wpf
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using Drawing;
    using Canvas = Drawing.Canvas;
    using Canvas1 = System.Windows.Controls.Canvas;
    using Mat = System.Windows.Media.Matrix;
    using Matrix = Drawing.Matrix;
    using Point = Drawing.Point;
    using Transform = System.Windows.Media.Transform;

    public sealed class WpfCanvas : Canvas
    {
        private IDictionary<DrawingMode, Brush> Brushes { get; }

        public WpfCanvas(Canvas1 canvas)
        {
            if (canvas == null)
            {
                throw new ArgumentNullException(nameof(canvas));
            }

            Brushes = new Dictionary<DrawingMode, Brush>
            {
                [DrawingMode.Component] = new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                [DrawingMode.BottomCopper] = new SolidColorBrush(Color.FromRgb(63, 63, 255)),
                [DrawingMode.TopCopper] = new SolidColorBrush(Color.FromRgb(255, 255, 63)),
                [DrawingMode.DrillHole] = new SolidColorBrush(Color.FromRgb(127, 127, 127)),
                [DrawingMode.Pad] = new SolidColorBrush(Color.FromRgb(63, 255, 63)),
                [DrawingMode.Via] = new SolidColorBrush(Color.FromRgb(127, 127, 255)),
                [DrawingMode.BoardBoundary] = new SolidColorBrush(Color.FromRgb(63, 200, 200))
#if DEBUG
                ,[DrawingMode.TopWave] = new SolidColorBrush(Color.FromRgb(255, 255, 63))
                ,[DrawingMode.BottomWave] = new SolidColorBrush(Color.FromRgb(255, 63, 63))
#endif
            };

            CurrentCanvas = canvas;
            var trans = new TransformGroup();

            trans.Children.Add(new ScaleTransform(1, -1, 0.5, 0.5));
            trans.Children.Add(new ScaleTransform(0.2, 0.2));            

            CurrentCanvas.LayoutTransform = trans;            
            Clear();
        }

        private Canvas1 CurrentCanvas { get; }

        public Transform ApplyTransform()
        {
            var mat = ConvertMatrix(Transform.Matrix);
            return new MatrixTransform(mat);
        }

        public Transform ApplyTransform(double x, double y)
        {
            var m = new Matrix(1, 0, 0, 1, x, y);
            var mat = ConvertMatrix(m * Transform.Matrix);
            return new MatrixTransform(mat);
        }

        public override void Clear()
        {
            CurrentCanvas.Children.Clear();
        }

        Brush GetBrush(DrawingMode mode)
        {
            var i = (int)mode;
            if (i < 1000)
            {
                return Brushes[mode];
            }

            return new SolidColorBrush(ColorHelper.ColorFromHSV((i-1000), 1, 1));
        }

        public override void DrawRectangle(DrawingMode mode, Point bottomLeft, double width, double height)
        {
            width += 8;
            height += 8;

            var blx = bottomLeft.X - 4;
            var bly = bottomLeft.Y - 4;

            var originX = -blx / width;
            var originY = -bly / height;            

            var rect = new Rectangle
            {
                Width = width,
                Height = height,
                Stroke = GetBrush(mode),
                StrokeThickness = 8,
                RenderTransform = ApplyTransform(blx, bly),
                RenderTransformOrigin = new System.Windows.Point(originX, originY)
            };
            CurrentCanvas.Children.Add(rect);
        }

        public override void DrawLine(DrawingMode mode, Point point1, Point point2)
        {
            var rect = new Line
            {
                X1 = point1.X,
                Y1 = point1.Y,
                X2 = point2.X,
                Y2 = point2.Y,
                Stroke = Brushes[mode],
                StrokeThickness = mode == DrawingMode.BottomCopper || mode == DrawingMode.TopCopper ? 8 : 8,
                RenderTransform = ApplyTransform()
            };

            CurrentCanvas.Children.Add(rect);
        }

        public override void DrawEllipse(DrawingMode mode, Point center, double xRadius, double yRadius)
        {
            xRadius += 4;
            yRadius += 4;

            var originX = (-center.X / (2 * xRadius)) + 0.5;
            var originY = (-center.Y / (2 * yRadius)) + 0.5;

            var rect = new Ellipse
            {
                Width = (2 * xRadius),
                Height = (2 * yRadius),
                Stroke = Brushes[mode],
                StrokeThickness = 8,
                RenderTransform = ApplyTransform(center.X - xRadius, center.Y - yRadius),
                RenderTransformOrigin = new System.Windows.Point(originX, originY)
            };

            CurrentCanvas.Children.Add(rect);
        }

        public void Text(double x, double y, string text, Color color)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(color),
                RenderTransform = new ScaleTransform(1, -1, 0.5, 0.5),
                LayoutTransform = ApplyTransform()
            };

            Canvas1.SetLeft(textBlock, x);
            Canvas1.SetTop(textBlock, y);
            CurrentCanvas.Children.Add(textBlock);
        }

        private static Mat ConvertMatrix(Matrix matrix)
        {
            // this weird conversion because we use column vector and WPF uses row vector
            // See remarks in https://msdn.microsoft.com/en-us/library/system.windows.media.matrix(v=vs.110).aspx
            var mat = new Mat(matrix.M11, matrix.M21, matrix.M12, matrix.M22, matrix.Dx, matrix.Dy);
            return mat;
        }

        public override void DrawFilledRectangle(DrawingMode mode, Point bottomLeft, double width, double height)
        {
            var originX = -bottomLeft.X / width;
            var originY = -bottomLeft.Y / height;

            var rect = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = Brushes[mode],
                StrokeThickness = 0,
                RenderTransform = ApplyTransform(bottomLeft.X, bottomLeft.Y),
                RenderTransformOrigin = new System.Windows.Point(originX, originY)
            };

            CurrentCanvas.Children.Add(rect);
        }

        public override void DrawFilledEllipse(DrawingMode mode, Point center, double xRadius, double yRadius)
        {
            var originX = (-center.X / (2 * xRadius)) + 0.5;
            var originY = (-center.Y / (2 * yRadius)) + 0.5;

            var rect = new Ellipse
            {
                Width = 2 * xRadius,
                Height = 2 * yRadius,
                Fill = Brushes[mode],
                StrokeThickness = 0,
                RenderTransform = ApplyTransform(center.X - xRadius, center.Y - yRadius),
                RenderTransformOrigin = new System.Windows.Point(originX, originY)
            };

            CurrentCanvas.Children.Add(rect);
        }

        static double NormalizeAngle(double angle)
        {
            const double m = 2 * Math.PI;
            return (angle % m + m) % m;
        }

        static System.Windows.Point AngleAt(Point center, double xRadius, double yRadius, double angle)
        {
            var sin = Math.Sin(angle);
            var cos = Math.Cos(angle);

            var xsin = xRadius * sin;
            var ycos = yRadius * cos;

            var r = (xRadius * yRadius) / Math.Sqrt(xsin * xsin + ycos * ycos);
            var xx = r * cos;
            var yy = r * sin;

            return new System.Windows.Point(xx + center.X, yy + center.Y);
        }

        public override void DrawArcSegment(DrawingMode mode, Point center, double xRadius, double yRadius, double startAngle, double endAngle)
        {
            var sp = AngleAt(center, xRadius, yRadius, startAngle);
            var ep = AngleAt(center, xRadius, yRadius, endAngle);
            var sweepAngle = NormalizeAngle(endAngle - startAngle);

            var p = new Path
            {
                StrokeThickness = 8,
                Stroke = Brushes[mode],
                Data = new PathGeometry
                {
                    Figures =
                    {
                        new PathFigure
                        {
                            StartPoint = sp,
                            Segments =
                            {
                                new ArcSegment
                                {
                                    Point = ep,
                                    Size = new Size(xRadius, yRadius),
                                    IsLargeArc = sweepAngle > Math.PI,
                                    SweepDirection = SweepDirection.Clockwise
                                }
                            }
                        }
                    }
                },
                RenderTransform = ApplyTransform()
            };

            CurrentCanvas.Children.Add(p);
        }
    }
}