using System.Collections.Generic;
using System.Linq;
using HugeScale.SceneObjects;
using UnityEngine;

namespace HugeScale.RenderSystems.ConeScale
{
    public class ConeScaleRenderSystem : RenderSystemBase
    {
        [SerializeField]
        private Camera _camera;
        
        [SerializeField]
        private float _startLayerRadius = 1f;
        
        [SerializeField]
        private float _layerOffset = 0.5f;
        
        private Vector3d _coordinateCenter;

        private double _meterToUnit;
        
        protected override void RenderUpdate()
        {
            UpdateCameraTransform();
            UpdateSolidObjects();
            UpdateOtherObjects();
        }
        
        private void UpdateCameraTransform()
        {
            var cameraTransform = _camera.transform;
            
            cameraTransform.localPosition = Vector3.zero;
            cameraTransform.localRotation = CameraRotation;
        }

        private void UpdateSolidObjects()
        {
            var sceneObjects = GetSceneSolidObjectsOrderByDistanceToCamera();
            var layerRadius = _startLayerRadius;

            SceneObjectRadialLayer firstLayer = null;

            foreach (var sceneObject in sceneObjects)
            {
                if (Vector3d.Distance(sceneObject.Coordinate, CameraCoordinate) < sceneObject.Radius)
                {
                    sceneObject.Hide();
                    continue;
                }

                var radialRenderLayer = new SceneObjectRadialLayer(sceneObject, CameraCoordinate, layerRadius);
                
                layerRadius = radialRenderLayer.EndRadius + _layerOffset;
                
                sceneObject.ApplyRenderParams(radialRenderLayer.RenderParams);

                firstLayer ??= radialRenderLayer;
            }

            if (firstLayer != null) 
                UpdateFirstLayerParamsForInfographics(firstLayer);
            else
                Debug.LogError("There are no solid objects");
        }
        
        private void UpdateOtherObjects()
        {
            foreach (var sceneObject in SceneObjects.Where(x => !x.Solid)) 
                sceneObject.ApplyRenderParams(CalculateInfographicRenderParams(sceneObject));
        }
        
        private IEnumerable<SceneObject> GetSceneSolidObjectsOrderByDistanceToCamera() => 
            SceneObjects.Where(x => x.Solid)
                .OrderBy(x => (x.Coordinate - CameraCoordinate).sqrMagnitude).ToArray();

        private void UpdateFirstLayerParamsForInfographics(SceneObjectRadialLayer radialLayer)
        {
            _meterToUnit = radialLayer.RenderParams.Scale / (2 * radialLayer.SceneObject.Radius);
            _coordinateCenter = radialLayer.SceneObject.Coordinate - new Vector3d(radialLayer.RenderParams.Position) / _meterToUnit;
        }
        
        private SceneObjectRenderParams CalculateInfographicRenderParams(SceneObject infographicObject) =>
            new(
                infographicObject,
                Converter.MeterToUnit(infographicObject.Coordinate - _coordinateCenter, _meterToUnit),
                2 * Converter.MeterToUnit(infographicObject.Radius, _meterToUnit),
                Layers.Default
            );
    }
}