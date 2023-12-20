using HugeScale.SceneObjects;
using UnityEngine;

namespace HugeScale.RenderSystems.ConeScale
{
    public class SceneObjectRadialLayer
    {
        public readonly SceneObject SceneObject;
        
        public readonly float StartRadius;
        
        public readonly float EndRadius;

        public readonly SceneObjectRenderParams RenderParams;

        public SceneObjectRadialLayer(SceneObject sceneObject, Vector3d cameraCoordinate, float startRadius)
        {
            SceneObject = sceneObject;
            StartRadius = startRadius;
            
            RenderParams = CalculateRenderParams(sceneObject, cameraCoordinate, startRadius);
            
            EndRadius = StartRadius + RenderParams.Scale;
        }

        private SceneObjectRenderParams CalculateRenderParams(SceneObject sceneObject, Vector3d cameraCoordinate, float startRadius)
        {
            var vectorToObjectCenter = sceneObject.Coordinate - cameraCoordinate;
            var distanceToObjectCenter = vectorToObjectCenter.magnitude;

            var tgAlpha = sceneObject.Radius / distanceToObjectCenter;
            var d1 = startRadius;

            var r2 = tgAlpha * d1 / (1 - tgAlpha);
            var d2 = d1 + r2;

            return new SceneObjectRenderParams(
                sceneObject,
                (vectorToObjectCenter.normalized * d2).ToVector3(),
                (float)(r2 * 2),
                Layers.Default);
        }
    }
}