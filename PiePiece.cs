using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChatWheel
{
    /// <summary>
    ///     A pie piece shape
    /// </summary>
    internal class PiePiece : UserControl
    {
        private Point[] pieBounds;
        private ScaleTransform RTScaleDown;
        private ScaleTransform RTScaleUp;

        protected override void OnRender(DrawingContext drawingContext)
        {
            DrawingContext(drawingContext);
            base.OnRender(drawingContext);
        }

        /// <summary>
        ///     Draws the pie piece
        /// </summary>
        private void DrawingContext(DrawingContext context)
        {
            var startPoint = new Point(CenterX, CenterY);
            RTScaleUp = new ScaleTransform(1.0, 1.0, CenterX, CenterY);
            RTScaleDown = new ScaleTransform(0.9, 0.9, CenterX, CenterY);
            var innerArcStartPoint = Utils.ComputeCartesianCoordinate(RotationAngle, InnerRadius);
            innerArcStartPoint.Offset(CenterX, CenterY);

            var innerArcEndPoint = Utils.ComputeCartesianCoordinate(RotationAngle + WedgeAngle, InnerRadius);
            innerArcEndPoint.Offset(CenterX, CenterY);

            var outerArcStartPoint = Utils.ComputeCartesianCoordinate(RotationAngle, Radius);
            outerArcStartPoint.Offset(CenterX, CenterY);

            var outerArcEndPoint = Utils.ComputeCartesianCoordinate(RotationAngle + WedgeAngle, Radius);
            outerArcEndPoint.Offset(CenterX, CenterY);

            var midArcStartPoint = new Point((innerArcStartPoint.X + outerArcStartPoint.X)/2,
                (innerArcStartPoint.Y + outerArcStartPoint.Y)/2);
            var midArcEndPoint = new Point((innerArcEndPoint.X + outerArcEndPoint.X)/2,
                (innerArcEndPoint.Y + outerArcEndPoint.Y)/2);


            var largeArc = WedgeAngle > 180.0;

            if (PushOut > 0)
            {
                var offset = Utils.ComputeCartesianCoordinate(RotationAngle + WedgeAngle/2, PushOut);
                innerArcStartPoint.Offset(offset.X, offset.Y);
                innerArcEndPoint.Offset(offset.X, offset.Y);
                outerArcStartPoint.Offset(offset.X, offset.Y);
                outerArcEndPoint.Offset(offset.X, offset.Y);
            }

            var outerArcSize = new Size(Radius, Radius);
            var innerArcSize = new Size(InnerRadius, InnerRadius);
            pieBounds = new[]
            {
                innerArcStartPoint,
                outerArcStartPoint,
                outerArcEndPoint,
                innerArcEndPoint,
                innerArcStartPoint
            };
            var streamGeometry = new StreamGeometry();
            using (var geometryContext = streamGeometry.Open())
            {
                geometryContext.BeginFigure(innerArcStartPoint, true, true);
                geometryContext.LineTo(outerArcStartPoint, true, true);
                geometryContext.ArcTo(outerArcEndPoint, outerArcSize, 0, largeArc, SweepDirection.Clockwise, true, true);
                geometryContext.LineTo(innerArcEndPoint, true, true);
                geometryContext.ArcTo(innerArcStartPoint, innerArcSize, 0, largeArc, SweepDirection.Counterclockwise,
                    true, true);
            }

            context.DrawGeometry(FillColor, new Pen(Brushes.White, 0), streamGeometry);
            var mid = new Point();
            for (var pIndex = 1; pIndex < pieBounds.Length; pIndex++)
            {
                if (mid.X == 0)
                    mid = pieBounds[pIndex];
                else
                {
                    mid.X += pieBounds[pIndex].X;
                    mid.Y += pieBounds[pIndex].Y;
                }
            }
            mid = new Point(mid.X / 4, mid.Y / 4);
            #region Debug

            if (Debugger.IsAttached)
            {
                var prev = new Point();
                foreach (var pieBound in pieBounds)
                {
                    if (prev.X == 0) prev = pieBound;
                    context.DrawLine(new Pen(new SolidColorBrush(Colors.Red), 1.0), prev, pieBound);
                    prev = pieBound;
                }
                
                context.DrawEllipse(new SolidColorBrush(Colors.White), new Pen(), mid, 4, 4);
                context.DrawLine(new Pen(new SolidColorBrush(Colors.Lime), 1.0), midArcStartPoint, midArcEndPoint);
            }

            #endregion

            var rotAngle = RotationAngle + WedgeAngle/2;
            var shouldRotate = Math.Abs(180 - rotAngle) < 100 && Math.Abs(180 - rotAngle) > -100;

            context.PushTransform(new RotateTransform(shouldRotate ? rotAngle + 180 : rotAngle, mid.X, mid.Y));
            var formattedUnitString = new FormattedText(ShortText,
                CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Tahoma"), FontStyles.Normal, FontWeights.Normal,
                    FontStretches.Normal), 16.0, Brushes.White);

            context.DrawText(formattedUnitString,
                new Point(mid.X - formattedUnitString.Width/2.0, mid.Y + (shouldRotate ? 0 : -25)));
            context.Pop();

            ReactToMouseLeave();
        }

        public bool IsAngleOnControl(double angle)
        {
            return Utils.IsAngleBetween(angle, RotationAngle, RotationAngle + WedgeAngle);
        }

        public void ReactToMouseEnter()
        {
            RenderTransform = RTScaleUp;
        }

        public void ReactToMouseLeave()
        {
            RenderTransform = RTScaleDown;
        }

        #region dependency properties

        public Brush FillColor { get; set; }

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("RadiusProperty", typeof (double), typeof (PiePiece),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     The radius of this pie piece
        /// </summary>
        public double Radius
        {
            get { return (double) GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public static readonly DependencyProperty PushOutProperty =
            DependencyProperty.Register("PushOutProperty", typeof (double), typeof (PiePiece),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     The distance to 'push' this pie piece out from the centre.
        /// </summary>
        public double PushOut
        {
            get { return (double) GetValue(PushOutProperty); }
            set { SetValue(PushOutProperty, value); }
        }

        public static readonly DependencyProperty InnerRadiusProperty =
            DependencyProperty.Register("InnerRadiusProperty", typeof (double), typeof (PiePiece),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     The inner radius of this pie piece
        /// </summary>
        public double InnerRadius
        {
            get { return (double) GetValue(InnerRadiusProperty); }
            set { SetValue(InnerRadiusProperty, value); }
        }

        public static readonly DependencyProperty WedgeAngleProperty =
            DependencyProperty.Register("WedgeAngleProperty", typeof (double), typeof (PiePiece),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     The wedge angle of this pie piece in degrees
        /// </summary>
        public double WedgeAngle
        {
            get { return (double) GetValue(WedgeAngleProperty); }
            set
            {
                SetValue(WedgeAngleProperty, value);
                Percentage = (value/360.0);
            }
        }

        public static readonly DependencyProperty RotationAngleProperty =
            DependencyProperty.Register("RotationAngleProperty", typeof (double), typeof (PiePiece),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     The rotation, in degrees, from the Y axis vector of this pie piece.
        /// </summary>
        public double RotationAngle
        {
            get { return (double) GetValue(RotationAngleProperty); }
            set { SetValue(RotationAngleProperty, value); }
        }

        public static readonly DependencyProperty CenterXProperty =
            DependencyProperty.Register("CenterXProperty", typeof (double), typeof (PiePiece),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     The X coordinate of centre of the circle from which this pie piece is cut.
        /// </summary>
        public double CenterX
        {
            get { return (double) GetValue(CenterXProperty); }
            set { SetValue(CenterXProperty, value); }
        }

        public static readonly DependencyProperty CenterYProperty =
            DependencyProperty.Register("CenterYProperty", typeof (double), typeof (PiePiece),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     The Y coordinate of centre of the circle from which this pie piece is cut.
        /// </summary>
        public double CenterY
        {
            get { return (double) GetValue(CenterYProperty); }
            set { SetValue(CenterYProperty, value); }
        }

        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register("PercentageProperty", typeof (double), typeof (PiePiece),
                new FrameworkPropertyMetadata(0.0));

        /// <summary>
        ///     The percentage of a full pie that this piece occupies.
        /// </summary>
        public double Percentage
        {
            get { return (double) GetValue(PercentageProperty); }
            private set { SetValue(PercentageProperty, value); }
        }

        public static readonly DependencyProperty PieceValueProperty =
            DependencyProperty.Register("PieceValueProperty", typeof (double), typeof (PiePiece),
                new FrameworkPropertyMetadata(0.0));

        /// <summary>
        ///     The text that this pie piece represents.
        /// </summary>
        public double PieceValue
        {
            get { return (double) GetValue(PieceValueProperty); }
            set { SetValue(PieceValueProperty, value); }
        }


        public static readonly DependencyProperty FullTextProperty =
            DependencyProperty.Register("FullTextProperty", typeof (string), typeof (PiePiece),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));


        /// <summary>
        ///     The text of this pie piece.
        /// </summary>
        public string FullText
        {
            get { return (string) GetValue(FullTextProperty); }
            set { SetValue(FullTextProperty, value); }
        }

        public static readonly DependencyProperty ShortTextProperty =
            DependencyProperty.Register("ShortTextProperty", typeof (string), typeof (PiePiece),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));


        /// <summary>
        ///     The text of this pie piece.
        /// </summary>
        public string ShortText
        {
            get { return (string) GetValue(ShortTextProperty); }
            set { SetValue(ShortTextProperty, value); }
        }

        #endregion
    }
}