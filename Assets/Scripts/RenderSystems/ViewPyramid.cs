using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HugeScale.RenderSystems
{
    public class ViewPyramid
    {
        #region Sides classes

        private class BorderSide
        {
            private readonly float _sideFov;

            private readonly double _sideDepth;

            private readonly Vector3d _forward;

            private readonly Vector3d _left;

            private readonly Vector3d _up;

            private readonly double _rotationAngleCos;

            public BorderSide(Vector3 leftDirection, Vector3 upDirection, float rotationAngle, float initialFov, double initialDepth)
            {
                _rotationAngleCos = Mathd.Cos(rotationAngle * Mathd.Deg2Rad);

                _sideDepth = initialDepth / _rotationAngleCos;
                
                _sideFov = (float)(2 * Mathd.Rad2Deg * Mathd.Atan2(
                    Mathd.Tan(initialFov / 2 * Mathd.Deg2Rad) * initialDepth,
                    _sideDepth));

                var rotation = Quaternion.AngleAxis(rotationAngle, leftDirection);

                _forward = new Vector3d(rotation * Vector3.forward);
                
                _left = new Vector3d(rotation * leftDirection);
                _up = new Vector3d(rotation * upDirection);
            }

            public bool CheckSphereIntersections(Vector3d sphereLocalCoordinate, double sphereRadius,
                out double? minDepth, out double? maxDepth)
            {
                minDepth = null;
                maxDepth = null;
                
                var projectionCoordinate = Vector3d.ProjectOnPlane(sphereLocalCoordinate, _up);
                
                var distanceToCenter = Vector3d.Distance(projectionCoordinate, sphereLocalCoordinate);

                if (distanceToCenter >= sphereRadius)
                    return false;
                
                var xProjection = Vector3d.Project(projectionCoordinate, _forward);
                var yProjection = Vector3d.Project(projectionCoordinate, _left);
                
                var xc = xProjection.magnitude * Vector3d.Dot(xProjection.normalized, _forward);
                var yc = yProjection.magnitude * Vector3d.Dot(yProjection.normalized, _left);
                
                var circleCoordinate = new Vector2d(xc, yc);
                var circleRadius = Mathd.Sqrt(sphereRadius * sphereRadius - distanceToCenter * distanceToCenter);
                
                var intersectionPointsList = new List<Vector2d>();

                if (CheckAngleBorderIntersections(circleCoordinate, circleRadius, _sideFov, out var angleBorderIntersectionPoints))
                    intersectionPointsList.AddRange(angleBorderIntersectionPoints);

                if (GeometryTools.CheckCircleAndVerticalLineIntersections(circleCoordinate, circleRadius, _sideDepth, out var farBorderIntersectionPoints))
                    intersectionPointsList.AddRange(farBorderIntersectionPoints);

                intersectionPointsList.AddRange(GeometryTools.GetCircleBorderPoints(xc, yc, circleRadius));

                intersectionPointsList = intersectionPointsList
                    .Where(x => GeometryTools.CheckPointInsideTriangle(x, _sideFov, _sideDepth)).ToList();

                if (intersectionPointsList.Count == 0)
                    return false;

                minDepth = intersectionPointsList.Min(point => point.x);
                maxDepth = intersectionPointsList.Max(point => point.x);

                minDepth *= _rotationAngleCos;
                maxDepth *= _rotationAngleCos;

                return true;
            }

            private static bool CheckAngleBorderIntersections(Vector2d circleCoordinate, double circleRadius, double fov,
                out IEnumerable<Vector2d> intersectionPoints)
            {
                var intersectionPointList = new List<Vector2d>();

                var lineK = Mathd.Tan(fov / 2 * Mathd.Deg2Rad);

                if (GeometryTools.CheckCircleAndLineIntersections(circleCoordinate, circleRadius, lineK, out var upIntersectionPoints))
                    intersectionPointList.AddRange(upIntersectionPoints);

                if (GeometryTools.CheckCircleAndLineIntersections(circleCoordinate, circleRadius, -lineK, out var downIntersectionPoints))
                    intersectionPointList.AddRange(downIntersectionPoints);

                intersectionPoints = intersectionPointList;

                return intersectionPointList.Count > 0;
            }
        }
        
        private class FarSide
        {
            private readonly double _depth;

            private readonly double _horizontalSize;
            
            private readonly double _verticalSize;
            
            public FarSide(double depth, float horizontalFov, float verticalFov)
            {
                _depth = depth;

                _horizontalSize = 2 * Mathd.Tan(horizontalFov / 2 * Mathd.Deg2Rad) * depth;
                _verticalSize = 2 * Mathd.Tan(verticalFov / 2 * Mathd.Deg2Rad) * depth;
            }

            public bool CheckSphereIntersections(Vector3d sphereLocalCoordinate, double sphereRadius)
            {
                if (!(sphereLocalCoordinate.z - sphereRadius < _depth 
                      && sphereLocalCoordinate.z + sphereRadius > _depth))
                    return false;

                var depthDistance = Mathd.Abs(_depth - sphereLocalCoordinate.z);
                
                var circleRadius = Mathd.Sqrt(sphereRadius * sphereRadius - depthDistance * depthDistance);
                var circleCoordinate = new Vector2d(sphereLocalCoordinate.x, sphereLocalCoordinate.y);

                if (circleCoordinate.magnitude <= circleRadius 
                    || GeometryTools.CheckPointInsideRectangle(sphereLocalCoordinate, _horizontalSize, _verticalSize))
                    return true;
                
                var intersectionPointList = new List<Vector2d>();
                
                if (GeometryTools.CheckCircleAndHorizontalLineIntersections(circleCoordinate, circleRadius,_verticalSize / 2, out var upIntersectionPoints))
                    intersectionPointList.AddRange(upIntersectionPoints);
                
                if (GeometryTools.CheckCircleAndHorizontalLineIntersections(circleCoordinate, circleRadius,-_verticalSize / 2, out var downIntersectionPoints))
                    intersectionPointList.AddRange(downIntersectionPoints);
                
                if (GeometryTools.CheckCircleAndVerticalLineIntersections(circleCoordinate, circleRadius,-_horizontalSize / 2, out var leftIntersectionPoints))
                    intersectionPointList.AddRange(leftIntersectionPoints);
                
                if (GeometryTools.CheckCircleAndVerticalLineIntersections(circleCoordinate, circleRadius,_horizontalSize / 2, out var rightIntersectionPoints))
                    intersectionPointList.AddRange(rightIntersectionPoints);
                
                return intersectionPointList.Any(x => GeometryTools.CheckPointInsideRectangle(x, _horizontalSize, _verticalSize));
            }
        }

        #endregion

        public readonly Vector3d Coordinate;

        public readonly Quaternion Rotation;

        public readonly double Depth;

        public readonly float VerticalFov;

        public readonly float HorizontalFov;

        private readonly Quaternion _inverseRotation;

        private readonly BorderSide[] _borderSides;

        private readonly FarSide _farSide;

        public ViewPyramid(Vector3d coordinate, Quaternion rotation, double depth, float verticalFov,
            float horizontalFov)
        {
            Coordinate = coordinate;
            Rotation = rotation;
            Depth = depth;
            VerticalFov = verticalFov;
            HorizontalFov = horizontalFov;
            
            _inverseRotation = Quaternion.Inverse(rotation);

            var upSide = new BorderSide(
                Vector3.left,
                Vector3.up,
                VerticalFov / 2,
                HorizontalFov,
                Depth);
            
            var downSide = new BorderSide(
                Vector3.left,
                Vector3.up,
                -VerticalFov / 2,
                HorizontalFov,
                Depth);
            
            var leftSide = new BorderSide(
                Vector3.down,
                Vector3.left,
                HorizontalFov / 2,
                VerticalFov,
                Depth);
            
            var rightSide = new BorderSide(
                Vector3.down,
                Vector3.right,
                -HorizontalFov / 2,
                VerticalFov,
                Depth);

            _borderSides = new[]
            {
                upSide,
                downSide,
                leftSide,
                rightSide
            };

            _farSide = new FarSide(depth, HorizontalFov, VerticalFov);
        }

        public bool CheckSphere(Vector3d sphereCoordinate, double sphereRadius,
            out bool cornerInsideSphere, out double? minDepth, out double? maxDepth)
        {
            minDepth = null;
            maxDepth = null;

            var sphereLocalCoordinate = CalculateSphereLocalCoordinate(sphereCoordinate);
            
            cornerInsideSphere = sphereLocalCoordinate.magnitude < sphereRadius;
            
            var borderSidesIntersected = CheckSphereBorderSidesIntersections(sphereLocalCoordinate, sphereRadius, out var sideMinDepth, out var sideMaxDepth);
            var farSideIntersected = _farSide.CheckSphereIntersections(sphereLocalCoordinate, sphereRadius);

            var frontPointInside = CheckPointInsidePyramid(sphereLocalCoordinate + Vector3d.back * sphereRadius);
            var backPointInside = CheckPointInsidePyramid(sphereLocalCoordinate + Vector3d.forward * sphereRadius);

            if (!cornerInsideSphere && !borderSidesIntersected && !frontPointInside && !backPointInside && !farSideIntersected)
                return false;
            
            if (cornerInsideSphere) 
                minDepth = 0;
            else if (frontPointInside)
                minDepth = sphereLocalCoordinate.z - sphereRadius;
            
            if (farSideIntersected)
                maxDepth = Depth;
            else if (backPointInside)
                maxDepth = sphereLocalCoordinate.z + sphereRadius;

            if (borderSidesIntersected)
                UpdateMinMax(ref minDepth, ref maxDepth, sideMinDepth.Value, sideMaxDepth.Value);

            return true;
        }

        private Vector3d CalculateSphereLocalCoordinate(Vector3d sphereCoordinate)
        {
            var sphereLocalCoordinate = sphereCoordinate - Coordinate;

            // We don't have Quaternion with double precision, so we doing transformation in this strange way, to keep the precision
            sphereLocalCoordinate =
                new Vector3d(_inverseRotation * sphereLocalCoordinate.normalized.ToVector3()) *
                sphereLocalCoordinate.magnitude;

            return sphereLocalCoordinate;
        }
        
        private bool CheckSphereBorderSidesIntersections(Vector3d sphereLocalCoordinate, double sphereRadius, out double? minDepth, out double? maxDepth)
        {
            minDepth = null;
            maxDepth = null;
            
            var anySideIntersected = false;

            foreach (var side in _borderSides)
                if (side.CheckSphereIntersections(
                        sphereLocalCoordinate,
                        sphereRadius,
                        out var sideMinDepth,
                        out var sideMaxDepth))
                {
                    anySideIntersected = true;

                    UpdateMinMax(ref minDepth, ref maxDepth, sideMinDepth.Value, sideMaxDepth.Value);
                }

            return anySideIntersected;
        }

        private bool CheckPointInsidePyramid(Vector3d point) =>
            GeometryTools.CheckPointInsideTriangle(new Vector2d(point.z, point.x), HorizontalFov, Depth)
            && GeometryTools.CheckPointInsideTriangle(new Vector2d(point.z, point.y), VerticalFov, Depth);

        private static void UpdateMinMax(ref double? oldMin, ref double? oldMax, double min, double max)
        {
            oldMin = oldMin == null ? min : Mathd.Min(oldMin.Value, min);
            oldMax = oldMax == null ? max : Mathd.Max(oldMax.Value, max);
        }
    }
}