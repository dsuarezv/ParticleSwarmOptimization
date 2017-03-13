// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   No color coding, use coloured lights
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Dyquo.Charts3d
{

    public class Surface3dViewModel : INotifyPropertyChanged
    {
        public double MinX { get; set; } = 0;
        public double MinY { get; set; } = 3;
        public double MaxX { get; set; } = 0;
        public double MaxY { get; set; } = 3;
        public int StepsX { get; set; } = 100;
        public int StepsY { get; set; } = 100;


        public Func<double, double, double> Function { get; set; }
        public Point3D[,] Data { get; set; }
        public double[,] ColorValues { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;


        public Brush SurfaceBrush
        {
            get
            {
                return BrushHelper.CreateGradientBrush(Colors.BlueViolet, Colors.Magenta);
            }
        }


        internal void UpdateSurface()
        {
            if (Function == null) return;

            Data = CreateDataArray(Function);
            
            ColorValues = null;

            OnPropertyChanged("Data");
            OnPropertyChanged("ColorValues");
            OnPropertyChanged("SurfaceBrush");
        }

        private Point3D[,] CreateDataArray(Func<double, double, double> f)
        {
            var data = new Point3D[StepsX, StepsY];
            for (int i = 0; i < StepsX; i++)
                for (int j = 0; j < StepsY; j++)
                {
                    var pt = GetPointFromIndex(i, j);
                    data[i, j] = new Point3D(pt.X, pt.Y, f(pt.X, pt.Y));
                }
            return data;
        }

        private Point GetPointFromIndex(int i, int j)
        {
            double x = MinX + (double)j / (StepsY - 1) * (MaxX - MinX);
            double y = MinY + (double)i / (StepsX - 1) * (MaxY - MinY);
            return new Point(x, y);
        }

        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
