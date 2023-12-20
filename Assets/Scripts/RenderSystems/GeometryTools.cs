using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HugeScale.RenderSystems
{
    public static class GeometryTools
    {
        public static bool CheckPointInsideTriangle(Vector2d point, double fov, double depth)
        {
            if (point.x < 0 || point.x > depth)
                return false;

            var lineY = Mathd.Tan(fov / 2 * Mathd.Deg2Rad) * point.x;

            return Mathd.Abs(point.y) <= lineY;
        }
        
        public static bool CheckPointInsideRectangle(Vector2d point, double horizontalSize, double verticalSize) =>
            point.x >= -horizontalSize / 2 && point.x <= horizontalSize / 2
            && point.y >= -verticalSize / 2 && point.y <= verticalSize / 2;

        public static bool CheckCircleAndLineIntersections(Vector2d circleCoordinate, double circleRadius, double lineK,
            out IEnumerable<Vector2d> intersectionPoints)
        {
            intersectionPoints = null;

            var a = 1 + lineK * lineK;
            var b = -2 * circleCoordinate.x + 2 * lineK * -circleCoordinate.y;
            var c = circleCoordinate.x * circleCoordinate.x + circleCoordinate.y * circleCoordinate.y - circleRadius * circleRadius;

            var discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
                return false;

            var discriminantSqrt = Mathd.Sqrt(discriminant);

            var x1 = (-b + discriminantSqrt) / (2 * a);
            var x2 = (-b - discriminantSqrt) / (2 * a);

            var y1 = lineK * x1;
            var y2 = lineK * x2;

            intersectionPoints = new[]
            {
                new Vector2d(x1, y1),
                new Vector2d(x2, y2),
            };

            return true;
        }
        
        public static bool CheckCircleAndVerticalLineIntersections(Vector2d circleCoordinate, double circleRadius, double lineX,
            out IEnumerable<Vector2d> intersectionPoints)
        {
            intersectionPoints = null;

            if (Mathd.Abs(circleCoordinate.x - lineX) > circleRadius)
                return false;

            var sqrt = Mathd.Sqrt(circleRadius * circleRadius - (circleCoordinate.x - lineX) * (circleCoordinate.x - lineX));

            var y1 = circleCoordinate.y + sqrt;
            var y2 = circleCoordinate.y - sqrt;

            if (y1 > y2)
                (y1, y2) = (y2, y1);

            intersectionPoints = new[]
            {
                new Vector2d(lineX, y1),
                new Vector2d(lineX, y2),
            };

            return true;
        }

        public static bool CheckCircleAndHorizontalLineIntersections(Vector2d circleCoordinate, double circleRadius, double lineY,
            out IEnumerable<Vector2d> intersectionPoints)
        {
            intersectionPoints = null;
            
            var intersected = CheckCircleAndVerticalLineIntersections(
                new Vector2d(circleCoordinate.y, circleCoordinate.x),
                circleRadius,
                lineY,
                out var verticalIntersectionPoints);

            if (intersected)
                intersectionPoints = verticalIntersectionPoints.Select(point => new Vector2d(point.y, point.x));

            return intersected;
        }

        public static IEnumerable<Vector2d> GetCircleBorderPoints(double xc, double yc, double r) =>
            new[]
            {
                new Vector2d(xc + r, yc),
                new Vector2d(xc - r, yc),
                new Vector2d(xc, yc + r),
                new Vector2d(xc, yc - r)
            };
    }
}