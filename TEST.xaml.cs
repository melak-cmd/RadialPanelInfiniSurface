using System;
using System.Collections.Generic;
using System.Windows;

namespace RadialPanelInfiniSurface
{
    /// <summary>
    /// Interaction logic for TEST.xaml
    /// </summary>
    public partial class TEST : SurfaceUserControl
    {
        public List<string> currentList = new List<string>();

        private Affine2DManipulationProcessor manipulationProcessor;
        private Affine2DInertiaProcessor inertiaProcessor;

        private RadialPanelInfinite panel = null;

        public TEST()
        {
            InitializeComponent();

            /*
            int i;
            for (i = 1; i <= 20; i++)
            {
                currentList.Add(i.ToString());
            }
            */

            currentList.Add("1.jpg");
            currentList.Add("2.jpg");
            currentList.Add("3.jpg");
            currentList.Add("4.jpg");
            currentList.Add("1.jpg");
            currentList.Add("2.jpg");
            currentList.Add("3.jpg");
            currentList.Add("4.jpg");
            currentList.Add("1.jpg");
            currentList.Add("2.jpg");
            currentList.Add("3.jpg");
            currentList.Add("4.jpg");
            currentList.Add("1.jpg");
            currentList.Add("2.jpg");
            currentList.Add("3.jpg");
            currentList.Add("4.jpg");

            this.CarouselObj.DataContext = this.currentList;

            InitializeManipulationInertiaProcessor();
        }

        private void InitializeManipulationInertiaProcessor()
        {
            manipulationProcessor = new Affine2DManipulationProcessor(Affine2DManipulations.TranslateX | Affine2DManipulations.TranslateY, CarouselObj);
            inertiaProcessor = new Affine2DInertiaProcessor();

            manipulationProcessor.Affine2DManipulationDelta += new EventHandler<Affine2DOperationDeltaEventArgs>(onManipulationDelta);
            manipulationProcessor.Affine2DManipulationCompleted += new EventHandler<Affine2DOperationCompletedEventArgs>(onManipulationCompleted);

            inertiaProcessor.Affine2DInertiaDelta += new EventHandler<Affine2DOperationDeltaEventArgs>(onManipulationDelta);
        }

        private void RadialPanel_Loaded(object sender, RoutedEventArgs e)
        {
            panel = sender as RadialPanelInfinite;
        }

        private void onManipulationCompleted(object sender, Affine2DOperationCompletedEventArgs e)
        {
            inertiaProcessor.DesiredDeceleration = 0.0004;
            inertiaProcessor.DesiredAngularDeceleration = 1000;
            inertiaProcessor.DesiredExpansionDeceleration = 1000;

            inertiaProcessor.InitialOrigin = e.ManipulationOrigin;
            inertiaProcessor.InitialVelocity = e.Velocity;
            inertiaProcessor.InitialExpansionVelocity = 0;
            inertiaProcessor.InitialAngularVelocity = 0;

            inertiaProcessor.Begin();
        }

        private void onManipulationDelta(object sender, Affine2DOperationDeltaEventArgs e)
        {
            if (panel != null)
            {
                Point p1 = new Point(e.ManipulationOrigin.X - this.ActualWidth / 2, e.ManipulationOrigin.Y - this.ActualHeight / 2);
                Point p2 = new Point(p1.X + e.Delta.X, p1.Y + e.Delta.Y);

                panel.ManipulationDeltaAngle(getDeltaAngle(p1, p2));
            }
        }

        private void SSV_ContactDown(object sender, ContactEventArgs e)
        {
            if (panel != null && !this.IsAnyContactCaptured)
            {
                Point p = e.Contact.GetPosition(this);

                p.X -= this.ActualWidth / 2;
                p.Y -= this.ActualHeight / 2;
                double radius = Math.Sqrt(p.X * p.X + p.Y * p.Y);
                if (radius <= 400 && radius >= 180)
                {
                    this.CaptureContact(e.Contact);
                    manipulationProcessor.BeginTrack(e.Contact);
                }
            }
        }

        private double getAngle(Point p)
        {
            double angle = 0;
            double r = Math.Sqrt(p.X * p.X + p.Y * p.Y);
            if (r > 0)
            {
                angle = Math.Acos(p.X / r);
                if (p.Y < 0)
                {
                    angle = -angle;
                }
            }
            return angle;
        }

        private double getDeltaAngle(Point p1, Point p2)
        {
            double a1 = getAngle(p1);
            double a2 = getAngle(p2);
            double delta = a1 - a2;

            if (delta > 3 * Math.PI / 2)
            {
                delta -= 2 * Math.PI;
            }
            else if (delta < -3 * Math.PI / 2)
            {
                delta += 2 * Math.PI;
            }

            return delta;
        }

    }
}
