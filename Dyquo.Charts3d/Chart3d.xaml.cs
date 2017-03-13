using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dyquo.Charts3d
{
    public partial class Chart3d : UserControl
    {
        public Chart3d()
        {
            InitializeComponent();            
        }

        public void ZoomExtents()
        {
            View3d.ZoomExtents(0.5);
        }

        public void AddSurface(Surface3dViewModel surface)
        {
            surface.UpdateSurface();

            var plot = new SurfacePlotVisual3D();
            Bind(plot, SurfacePlotVisual3D.PointsProperty, surface, "Data");
            Bind(plot, SurfacePlotVisual3D.ColorValuesProperty, surface, "ColorValues");
            Bind(plot, SurfacePlotVisual3D.SurfaceBrushProperty, surface, "SurfaceBrush");

            View3d.Children.Add(plot);
        }

        public void AddContent(Model3D content)
        {
            var visual = new ModelVisual3D();
            visual.Content = content;
            View3d.Children.Add(visual);
        }

        private static void Bind(SurfacePlotVisual3D target, DependencyProperty targetProperty, object source, string sourcePath)
        {
            var b = new Binding();
            b.Source = source;
            b.Path = new PropertyPath(sourcePath);
            BindingOperations.SetBinding(target, targetProperty, b);
        }
    }
}
