﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurfacePlotVisual3D.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Gets or sets the points defining the surface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace Dyquo.Charts3d
{
    public class SurfacePlotVisual3D : ModelVisual3D
    {
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(Point3D[,]), typeof(SurfacePlotVisual3D),
                                        new UIPropertyMetadata(null, ModelChanged));

        public static readonly DependencyProperty ColorValuesProperty =
            DependencyProperty.Register("ColorValues", typeof(double[,]), typeof(SurfacePlotVisual3D),
                                        new UIPropertyMetadata(null, ModelChanged));

        public static readonly DependencyProperty SurfaceBrushProperty =
            DependencyProperty.Register("SurfaceBrush", typeof(Brush), typeof(SurfacePlotVisual3D),
                                        new UIPropertyMetadata(null, ModelChanged));

        private readonly ModelVisual3D mVisualChild;

        public SurfacePlotVisual3D()
        {
            IntervalX = 1;
            IntervalY = 1;
            IntervalZ = 0.25;
            FontSize = 0.1;
            LineThickness = 0.01;

            mVisualChild = new ModelVisual3D();
            Children.Add(mVisualChild);
        }

        /// <summary>
        /// Gets or sets the points defining the surface.
        /// </summary>
        public Point3D[,] Points
        {
            get { return (Point3D[,])GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the color values corresponding to the Points array.
        /// The color values are used as Texture coordinates for the surface.
        /// Remember to set the SurfaceBrush, e.g. by using the BrushHelper.CreateGradientBrush method.
        /// If this property is not set, the z-value of the Points will be used as color value.
        /// </summary>
        public double[,] ColorValues
        {
            get { return (double[,])GetValue(ColorValuesProperty); }
            set { SetValue(ColorValuesProperty, value); }
        }

        /// <summary>
        /// Gets or sets the brush used for the surface.
        /// </summary>
        public Brush SurfaceBrush
        {
            get { return (Brush)GetValue(SurfaceBrushProperty); }
            set { SetValue(SurfaceBrushProperty, value); }
        }


        // todo: make Dependency properties
        public double IntervalX { get; set; }
        public double IntervalY { get; set; }
        public double IntervalZ { get; set; }
        public double FontSize { get; set; }
        public double LineThickness { get; set; }

        private static void ModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SurfacePlotVisual3D)d).UpdateModel();
        }

        private void UpdateModel()
        {
            mVisualChild.Content = CreateModel();
        }

        private Model3D CreateModel()
        {
            var plotModel = new Model3DGroup();

            int rows = Points.GetUpperBound(0) + 1;
            int columns = Points.GetUpperBound(1) + 1;
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            double minZ = double.MaxValue;
            double maxZ = double.MinValue;
            double minColorValue = double.MaxValue;
            double maxColorValue = double.MinValue;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                {
                    double x = Points[i, j].X;
                    double y = Points[i, j].Y;
                    double z = Points[i, j].Z;
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                    maxZ = Math.Max(maxZ, z);
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    minZ = Math.Min(minZ, z);
                    if (ColorValues != null)
                    {
                        maxColorValue = Math.Max(maxColorValue, ColorValues[i, j]);
                        minColorValue = Math.Min(minColorValue, ColorValues[i, j]);
                    }
                }

            // make color value 0 at texture coordinate 0.5
            if (Math.Abs(minColorValue) < Math.Abs(maxColorValue))
                minColorValue = -maxColorValue;
            else
                maxColorValue = -minColorValue;

            // set the texture coordinates by z-value or ColorValue
            var texcoords = new Point[rows, columns];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                {
                    double u = (Points[i, j].Z - minZ) / (maxZ - minZ);
                    if (ColorValues != null)
                        u = (ColorValues[i, j] - minColorValue) / (maxColorValue - minColorValue);
                    texcoords[i, j] = new Point(u, u);
                }

            var surfaceMeshBuilder = new MeshBuilder();
            surfaceMeshBuilder.AddRectangularMesh(Points, texcoords);

            var surfaceModel = new GeometryModel3D(surfaceMeshBuilder.ToMesh(),
                                                   MaterialHelper.CreateMaterial(SurfaceBrush, null, null, 1, 0));
            surfaceModel.BackMaterial = surfaceModel.Material;

            var axesMeshBuilder = new MeshBuilder();
            for (double x = minX; x <= maxX; x += IntervalX)
            {
                double j = (x - minX) / (maxX - minX) * (columns - 1);
                var path = new List<Point3D> { new Point3D(x + 0.00001, minY, minZ) };      // + 0.0001 to avoid ZoomExtens bug if any geometry has 
                for (int i = 0; i < rows; i++)
                {
                    path.Add(BilinearInterpolation(Points, i, j));
                }
                path.Add(new Point3D(x, maxY, minZ));

                axesMeshBuilder.AddTube(path, LineThickness, 4, false);
                GeometryModel3D label = TextCreator.CreateTextLabelModel3D(x.ToString(), Brushes.Black, true, FontSize,
                                                                           new Point3D(x, minY - FontSize * 2.5, minZ),
                                                                           new Vector3D(1, 0, 0), new Vector3D(0, 1, 0));
                plotModel.Children.Add(label);
            }

            {
                GeometryModel3D label = TextCreator.CreateTextLabelModel3D("X-axis", Brushes.Black, true, FontSize,
                                                                           new Point3D((minX + maxX) * 0.5,
                                                                                       minY - FontSize * 6, minZ),
                                                                           new Vector3D(1, 0, 0), new Vector3D(0, 1, 0));
                plotModel.Children.Add(label);
            }

            for (double y = minY; y <= maxY; y += IntervalY)
            {
                double i = (y - minY) / (maxY - minY) * (rows - 1);
                var path = new List<Point3D> { new Point3D(minX, y + 0.00001, minZ) };
                for (int j = 0; j < columns; j++)
                {
                    path.Add(BilinearInterpolation(Points, i, j));
                }
                path.Add(new Point3D(maxX, y, minZ));

                axesMeshBuilder.AddTube(path, LineThickness, 4, false);
                GeometryModel3D label = TextCreator.CreateTextLabelModel3D(y.ToString(), Brushes.Black, true, FontSize,
                                                                           new Point3D(minX - FontSize * 3, y, minZ),
                                                                           new Vector3D(1, 0, 0), new Vector3D(0, 1, 0));
                plotModel.Children.Add(label);
            }
            {
                GeometryModel3D label = TextCreator.CreateTextLabelModel3D("Y-axis", Brushes.Black, true, FontSize,
                                                                           new Point3D(minX - FontSize * 10,
                                                                                       (minY + maxY) * 0.5, minZ),
                                                                           new Vector3D(0, 1, 0), new Vector3D(-1, 0, 0));
                plotModel.Children.Add(label);
            }


            double z0 = (int)(minZ / IntervalZ) * IntervalZ;
            for (double z = z0; z <= maxZ + double.Epsilon; z += IntervalZ)
            {
                GeometryModel3D label = TextCreator.CreateTextLabelModel3D(z.ToString(), Brushes.Black, true, FontSize,
                                                                           new Point3D(minX - FontSize * 3, maxY, z),
                                                                           new Vector3D(1, 0, 0), new Vector3D(0, 0, 1));
                plotModel.Children.Add(label);
            }
            {
                GeometryModel3D label = TextCreator.CreateTextLabelModel3D("Z-axis", Brushes.Black, true, FontSize,
                                                                           new Point3D(minX - FontSize * 10, maxY,
                                                                                       (minZ + maxZ) * 0.5),
                                                                           new Vector3D(0, 0, 1), new Vector3D(1, 0, 0));
                plotModel.Children.Add(label);
            }


            var bb = new Rect3D(minX, minY, minZ, maxX - minX, maxY - minY, 0 * (maxZ - minZ));
            axesMeshBuilder.AddBoundingBox(bb, LineThickness);

            var axesModel = new GeometryModel3D(axesMeshBuilder.ToMesh(), Materials.Black);

            plotModel.Children.Add(surfaceModel);
            plotModel.Children.Add(axesModel);

            return plotModel;
        }

        private static Point3D BilinearInterpolation(Point3D[,] p, double i, double j)
        {
            int n = p.GetUpperBound(0);
            int m = p.GetUpperBound(1);
            var i0 = (int)i;
            var j0 = (int)j;
            if (i0 + 1 >= n) i0 = n - 2;
            if (j0 + 1 >= m) j0 = m - 2;

            if (i < 0) i = 0;
            if (j < 0) j = 0;
            double u = i - i0;
            double v = j - j0;
            Vector3D v00 = p[i0, j0].ToVector3D();
            Vector3D v01 = p[i0, j0 + 1].ToVector3D();
            Vector3D v10 = p[i0 + 1, j0].ToVector3D();
            Vector3D v11 = p[i0 + 1, j0 + 1].ToVector3D();
            Vector3D v0 = v00 * (1 - u) + v10 * u;
            Vector3D v1 = v01 * (1 - u) + v11 * u;
            return (v0 * (1 - v) + v1 * v).ToPoint3D();
        }
    }
}