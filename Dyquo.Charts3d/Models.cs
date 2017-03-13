using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Dyquo.Charts3d
{
    public class Models
    {
        public static Model3D GetCube(Brush brush)
        {
            var mb = new MeshBuilder();
            mb.AddCube(BoxFaces.All);
            
            var model = new GeometryModel3D(mb.ToMesh(), MaterialHelper.CreateMaterial(brush));
            return model;
        }

        public static Transform3D GetTransform(Point3D position, Size3D size)
        {
            var transform = new Matrix3D();
            transform.Scale(new Vector3D(size.X, size.Y, size.Z));
            transform.Translate(new Vector3D(position.X, position.Y, position.Z));
            return new MatrixTransform3D(transform);
        }
    }
}
