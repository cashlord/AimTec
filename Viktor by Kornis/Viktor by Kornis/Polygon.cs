using Aimtec;
using Aimtec.SDK.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aimtec.SDK.Util.ThirdParty;

namespace Viktor_By_Kornis
{
    public class DrawHelper
    {

        public void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            var screenStart = start.ToScreenPosition();
            var screenEnd = end.ToScreenPosition();
            Render.Line(screenStart, screenEnd, color);
        }
    }

    public abstract class Polygon : DrawHelper
    {
        public Polygon()
        {

        }

        public List<Vector3> Points = new List<Vector3>();

        public abstract void Draw(Color color);

        public List<IntPoint> ClipperPoints
        {
            get
            {
                List<IntPoint> clipperpoints = new List<IntPoint>();

                foreach (var p in this.Points)
                {
                    clipperpoints.Add(new IntPoint(p.X, p.Z));
                }

                return clipperpoints;
            }
        }

        public bool Contains(Vector3 point)
        {
            var p = new IntPoint(point.X, point.Z);
            var inpolygon = Clipper.PointInPolygon(p, this.ClipperPoints);
            return inpolygon == 1;

        }
    }

    public class Rectangle : Polygon
    {
        public Rectangle(Vector3 startPosition, Vector3 endPosition, float width)
        {
            var direction = (startPosition - endPosition).Normalized();
            var perpendicular = Perpendicular(direction);

            var leftBottom = startPosition + width * perpendicular;
            var leftTop = startPosition - width * perpendicular;

            var rightBottom = endPosition - width * perpendicular;
            var rightLeft = endPosition + width * perpendicular;

            this.Points.Add(leftBottom);
            this.Points.Add(leftTop);
            this.Points.Add(rightBottom);
            this.Points.Add(rightLeft);
        }


        public Vector3 Perpendicular(Vector3 v)
        {
            return new Vector3(-v.Z, v.Y, v.X);
        }

        public override void Draw(Color color)
        {
            if (Points.Count < 4)
            {
                return;
            }

            for (var i = 0; i <= Points.Count - 1; i++)
            {
                var p2 = (Points.Count - 1 == i) ? 0 : (i + 1);
                this.DrawLine(Points[i], Points[p2], color);
            }
        }
    }
}