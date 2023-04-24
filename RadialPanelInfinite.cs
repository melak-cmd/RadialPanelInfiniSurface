using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RadialPanelInfiniSurface
{
    public class RadialPanelInfinite : Panel
    {
        #region properties
        //- - - - - - - - - - - - - - - - - - - - - - - -
        private int _angleStart = 0;
        private double _rotateAngle = 0;
        private double _radiusPercentage = 100;
        private int _maxItems = 10;
        private double _angle = 0;
        //- - - - - - - - - - - - - - - - - - - - - - - -
        public int maxItems
        {
            get { return _maxItems; }
            set { _maxItems = value; this.UpdateLayout(); }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        public int angleStart
        {
            get { return _angleStart; }
            set { _angleStart = value; this.UpdateLayout(); }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        public double rotateAngle
        {
            get { return _rotateAngle; }
            set { _rotateAngle = value; this.UpdateLayout(); }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        public double radiusPercentage
        {
            get { return _radiusPercentage; }
            set { _radiusPercentage = value; this.UpdateLayout(); }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        #endregion

        #region Manipulations Affine2D
        //- - - - - - - - - - - - - - - - - - - - - - - -
        public void ManipulationDeltaAngle(double angle)
        {
            RotateTransform rt = this.RenderTransform as RotateTransform;
            if (rt != null)
            {
                _angle += Rad2Deg(angle);
                this.InvalidateArrange();
                //Debug.Print(_angle.ToString());
            }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        #endregion

        #region Measure + Arrange Override
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected override Size MeasureOverride(Size availableSize)
        {
            RotateTransform rt = this.RenderTransform as RotateTransform;
            if (rt == null)
            {
                this.RenderTransform = new RotateTransform();
            }
            this.RenderTransformOrigin = new Point(0.5, 0.5);

            foreach (UIElement elem in Children)
            {
                elem.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                elem.RenderTransformOrigin = new Point(0.5, 0.5);
                elem.RenderTransform = new RotateTransform();
            }

            return base.MeasureOverride(availableSize);
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected int getBoundedIndex(int i)
        {
            int x = i;
            while (x < 0) { x += Children.Count; }
            x = x % Children.Count;
            return x;
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected Point RotatePoint(Point p, Point center, double angle)
        {
            return new Point(
                center.X + (p.X - center.X) * Math.Cos(angle) - (p.Y - center.Y) * Math.Sin(angle),
                center.Y + (p.X - center.X) * Math.Sin(angle) + (p.Y - center.Y) * Math.Cos(angle)
            );
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count == 0)
            {
                return finalSize;
            }

            int nbItems = Children.Count;
            double angle;
            double angle_spacing = 360.0 / nbItems;
            double radiusX = finalSize.Width * radiusPercentage / 200;
            double radiusY = finalSize.Height * radiusPercentage / 200;

            double cos = 0, sin = 0;
            double pct = 0.5;

            Point p = new Point();
            Point A, B, C, M;
            Color C1, C2;

            if (nbItems > maxItems)
            {
                foreach (UIElement elem in Children)
                {
                    elem.Visibility = Visibility.Hidden;
                }

                angle_spacing = 360.0 / maxItems;

                int iStart = (int)Math.Ceiling((_angle - angle_spacing / 2) / angle_spacing);

                angle = _angleStart - _angle + iStart * angle_spacing;

                for (int i = 0; i <= maxItems; i++)
                {
                    UIElement elem = Children[getBoundedIndex(i + iStart)];

                    elem.Visibility = Visibility.Visible;

                    ((RotateTransform)elem.RenderTransform).Angle = angle + _rotateAngle;

                    pct = 1 - Math.Abs(_angle - angle_spacing / 2 - iStart * angle_spacing) / angle_spacing;

                    cos = Math.Cos(Deg2Rad(angle));
                    sin = Math.Sin(Deg2Rad(angle));

                    p = new Point(
                        finalSize.Width / 2 + cos * radiusX,
                        finalSize.Height / 2 + sin * radiusY
                    );

                    if (i == 0 || i == maxItems)
                    {
                        LinearGradientBrush lgb = new LinearGradientBrush();

                        A = new Point(0, 0);
                        B = new Point(0, 1);
                        C = new Point(0.5, 0.5);
                        if (i == 0)
                        {
                            M = new Point(0, pct);
                            C1 = Colors.Transparent;
                            C2 = Colors.White;
                        }
                        else
                        {
                            M = new Point(0, 1 - pct);
                            C1 = Colors.White;
                            C2 = Colors.Transparent;
                        }

                        A = RotatePoint(A, C, Deg2Rad(-_rotateAngle));
                        B = RotatePoint(B, C, Deg2Rad(-_rotateAngle));
                        M = RotatePoint(M, C, Deg2Rad(-_rotateAngle));

                        A = RotatePoint(A, M, Deg2Rad(-angle + _angleStart));
                        B = RotatePoint(B, M, Deg2Rad(-angle + _angleStart));

                        lgb.StartPoint = A;
                        lgb.EndPoint = B;

                        lgb.GradientStops.Add(new GradientStop(C1, 0));
                        lgb.GradientStops.Add(new GradientStop(C1, pct));
                        lgb.GradientStops.Add(new GradientStop(C2, pct + 0.00001));
                        lgb.GradientStops.Add(new GradientStop(C2, 1));

                        //Debug.Print(lgb.StartPoint.ToString() + "   " + lgb.EndPoint.ToString() + "   " + pct.ToString());
                        elem.OpacityMask = lgb;
                    }
                    else
                    {
                        elem.OpacityMask = null;
                    }

                    elem.Arrange(new Rect(
                        p.X - elem.DesiredSize.Width / 2,
                        p.Y - elem.DesiredSize.Height / 2,
                        elem.DesiredSize.Width,
                        elem.DesiredSize.Height
                    ));

                    angle += angle_spacing;
                }
            }
            else
            {
                angle = _angleStart - _angle;

                foreach (UIElement elem in Children)
                {
                    p = new Point(
                        finalSize.Width / 2 + Math.Cos(Deg2Rad(angle)) * radiusX,
                        finalSize.Height / 2 + Math.Sin(Deg2Rad(angle)) * radiusY
                    );

                    ((RotateTransform)elem.RenderTransform).Angle = angle + _rotateAngle;

                    elem.Arrange(new Rect(
                        p.X - elem.DesiredSize.Width / 2,
                        p.Y - elem.DesiredSize.Height / 2,
                        elem.DesiredSize.Width,
                        elem.DesiredSize.Height
                    ));

                    angle += angle_spacing;
                }
            }

            return finalSize;
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected double Deg2Rad(double angle)
        {
            return angle * Math.PI / 180;
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected double Rad2Deg(double angle)
        {
            return angle * 180 / Math.PI;
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        #endregion

    }
}
