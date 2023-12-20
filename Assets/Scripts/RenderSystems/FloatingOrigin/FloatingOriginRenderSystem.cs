using HugeScale.SceneObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace HugeScale.RenderSystems.FloatingOrigin
{
    public class FloatingOriginRenderSystem : RenderSystemBase
    {
        [SerializeField]
        private Camera _camera;
        
        [SerializeField]
        private double _meterToUnit = 1e-09d;

        private Vector3d _coordinateCenter;
        
        protected override void RenderUpdate()
        {
            UpdateCoordinateCenter();
            UpdateCameraTransform();
            UpdateSceneObjects();
        }

        private void UpdateCoordinateCenter() =>
            _coordinateCenter = CameraCoordinate;

        private void UpdateCameraTransform()
        {
            var cameraTransform = _camera.transform;
            
            cameraTransform.localPosition = Converter.MeterToUnit(CameraCoordinate - _coordinateCenter, _meterToUnit);
            cameraTransform.localRotation = CameraRotation;
        }

        private void UpdateSceneObjects()
        {
            foreach (var sceneObject in SceneObjects) 
                sceneObject.ApplyRenderParams(CalculateRenderParams(sceneObject));
        }

        private SceneObjectRenderParams CalculateRenderParams(SceneObject sceneObject) =>
            new(
                sceneObject,
                Converter.MeterToUnit(sceneObject.Coordinate - _coordinateCenter, _meterToUnit),
                2 * Converter.MeterToUnit(sceneObject.Radius, _meterToUnit),
                Layers.Default
            );
    }
}