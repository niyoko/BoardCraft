﻿namespace BoardCraft.Output.Wpf
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
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
        private IDictionary<ColorPallete, IDictionary<DrawingMode, Brush>> Brushes { get; }

        public bool Component { get; set; }
        public bool BottomCopper { get; set; }
        public bool TopCopper { get; set; }
        public bool Pad { get; set; }
        public bool DrillHole { get; set; }
        public bool Via { get; set; }
        public bool BoardEdge { get; set; }        

        public ColorPallete ColorPallete { get; set; }

        public WpfCanvas(Canvas1 canvas)
        {
            if (canvas == null)
            {
                throw new ArgumentNullException(nameof(canvas));
            }

            Brushes = new Dictionary<ColorPallete, IDictionary<DrawingMode, Brush>> {
                [ColorPallete.Color] = new Dictionary<DrawingMode, Brush>
                    {
                        [DrawingMode.Component] = new SolidColorBrush(Color.FromRgb(0, 190, 75)),
                        [DrawingMode.BottomCopper] = new SolidColorBrush(Color.FromRgb(0, 0, 255)),
                        [DrawingMode.TopCopper] = new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                        [DrawingMode.ViaDrillHole] = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                        [DrawingMode.PadDrillHole] = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                        [DrawingMode.Pad] = new SolidColorBrush(Color.FromRgb(220, 0, 220)),
                        [DrawingMode.Via] = new SolidColorBrush(Color.FromRgb(220, 220, 0)),
                        [DrawingMode.BoardEdge] = new SolidColorBrush(Color.FromRgb(0, 0, 0))
                    },
                [ColorPallete.Black] = new Dictionary<DrawingMode, Brush>
                    {
                        [DrawingMode.Component] = System.Windows.Media.Brushes.Black,
                        [DrawingMode.BottomCopper] = System.Windows.Media.Brushes.Black,
                        [DrawingMode.TopCopper] = System.Windows.Media.Brushes.Black,
                        [DrawingMode.ViaDrillHole] = System.Windows.Media.Brushes.Black,
                        [DrawingMode.PadDrillHole] = System.Windows.Media.Brushes.Black,
                        [DrawingMode.Pad] = System.Windows.Media.Brushes.Black,
                        [DrawingMode.Via] = System.Windows.Media.Brushes.Black,
                        [DrawingMode.BoardEdge] = System.Windows.Media.Brushes.Black
                    }
            };

            NativeCanvas = canvas;
            var trans = new TransformGroup();

            trans.Children.Add(new ScaleTransform(1, -1, 0.5, 0.5));
            trans.Children.Add(new ScaleTransform(0.192, 0.192));            

            NativeCanvas.LayoutTransform = trans;            
            Clear();
        }

        public Canvas1 NativeCanvas { get; }

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
            NativeCanvas.Children.Clear();
        }

        private Brush GetBrush(DrawingMode mode)
        {
            //this is hack. Should be fixed in the next opportunity
            if (mode == DrawingMode.PadDrillHole && Pad && !DrillHole)
            {
                return System.Windows.Media.Brushes.White;
            }

            if (mode == DrawingMode.ViaDrillHole && Via && !DrillHole)
            {
                return System.Windows.Media.Brushes.White;
            }

            return Brushes[ColorPallete][mode];
        }

        private bool ShouldDraw(DrawingMode mode)
        {
            switch (mode)
            {
                case DrawingMode.Component: return Component;
                case DrawingMode.BottomCopper: return BottomCopper;
                case DrawingMode.TopCopper: return TopCopper;
                case DrawingMode.ViaDrillHole: return DrillHole || Via;
                case DrawingMode.PadDrillHole: return DrillHole || Pad;
                case DrawingMode.Pad: return Pad;
                case DrawingMode.Via: return Via;
                case DrawingMode.BoardEdge: return BoardEdge;
            }
            
            return false;
        }

        public override void DrawRectangle(DrawingMode mode, Point bottomLeft, double width, double height)
        {
            if (!ShouldDraw(mode))
            {
                return;                
            }

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
            NativeCanvas.Children.Add(rect);
        }

        public override void DrawLine(DrawingMode mode, Point point1, Point point2)
        {
            if (!ShouldDraw(mode))
            {
                return;
            }

            var rect = new Line
            {
                X1 = point1.X,
                Y1 = point1.Y,
                X2 = point2.X,
                Y2 = point2.Y,
                Stroke = GetBrush(mode),
                StrokeThickness = mode == DrawingMode.TopCopper || mode == DrawingMode.BottomCopper ? 20 : 8,
                RenderTransform = ApplyTransform()
            };

            NativeCanvas.Children.Add(rect);
        }

        public override void DrawEllipse(DrawingMode mode, Point center, double xRadius, double yRadius)
        {
            if (!ShouldDraw(mode))
            {
                return;
            }

            xRadius += 4;
            yRadius += 4;

            var originX = (-center.X / (2 * xRadius)) + 0.5;
            var originY = (-center.Y / (2 * yRadius)) + 0.5;

            var rect = new Ellipse
            {
                Width = (2 * xRadius),
                Height = (2 * yRadius),
                Stroke = GetBrush(mode),
                StrokeThickness = 8,
                RenderTransform = ApplyTransform(center.X - xRadius, center.Y - yRadius),
                RenderTransformOrigin = new System.Windows.Point(originX, originY)
            };

            NativeCanvas.Children.Add(rect);
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
            if (!ShouldDraw(mode))
            {
                return;
            }

            var originX = -bottomLeft.X / width;
            var originY = -bottomLeft.Y / height;

            var rect = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = GetBrush(mode),
                StrokeThickness = 0,
                RenderTransform = ApplyTransform(bottomLeft.X, bottomLeft.Y),
                RenderTransformOrigin = new System.Windows.Point(originX, originY)
            };

            NativeCanvas.Children.Add(rect);
        }

        public override void DrawFilledEllipse(DrawingMode mode, Point center, double xRadius, double yRadius)
        {
            if (!ShouldDraw(mode))
            {
                return;
            }

            var originX = (-center.X / (2 * xRadius)) + 0.5;
            var originY = (-center.Y / (2 * yRadius)) + 0.5;

            var rect = new Ellipse
            {
                Width = 2 * xRadius,
                Height = 2 * yRadius,
                Fill = GetBrush(mode),
                StrokeThickness = 0,
                RenderTransform = ApplyTransform(center.X - xRadius, center.Y - yRadius),
                RenderTransformOrigin = new System.Windows.Point(originX, originY)
            };

            NativeCanvas.Children.Add(rect);
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
            if (!ShouldDraw(mode))
            {
                return;
            }

            var sp = AngleAt(center, xRadius, yRadius, startAngle);
            var ep = AngleAt(center, xRadius, yRadius, endAngle);
            var sweepAngle = NormalizeAngle(endAngle - startAngle);

            var p = new Path
            {
                StrokeThickness = 8,
                Stroke = GetBrush(mode),
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

            NativeCanvas.Children.Add(p);
        }

        public override void DrawPolyline(DrawingMode mode, IEnumerable<Point> nodes)
        {
            if (!ShouldDraw(mode))
            {
                return;
            }
        
            var pts = nodes.Select(x => new System.Windows.Point(x.X, x.Y));
            var p = new Polyline
            {
                Stroke = GetBrush(mode),
                StrokeThickness = mode == DrawingMode.TopCopper || mode == DrawingMode.BottomCopper ? 20 : 8,
                RenderTransform = ApplyTransform(),
                Points = new PointCollection(pts)
            };
            NativeCanvas.Children.Add(p);
        }
    }
}