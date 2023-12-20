using HugeScale.SceneObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace HugeScale.RenderSystems.Simple
{
    public class SimpleRenderSystem : RenderSystemBase
    {
        [SerializeField]
        private Camera _camera;
        
        [SerializeField]
        private double _meterToUnit = 1e-09d;
        
        protected override void RenderUpdate()
        {
            UpdateCameraTransform();
            UpdateSceneObject();
        }

        private void UpdateCameraTransform()
        {
            var cameraTransform = _camera.transform;
            
            cameraTransform.localPosition = Converter.MeterToUnit(CameraCoordinate, _meterToUnit);
            cameraTransform.localRotation = CameraRotation;
        }

        private void UpdateSceneObject()
        {
            foreach (var sceneObject in SceneObjects) 
                sceneObject.ApplyRenderParams(CalculateRenderParams(sceneObject));
        }
        
        private SceneObjectRenderParams CalculateRenderParams(SceneObject sceneObject) =>
            new(
                sceneObject,
                Converter.MeterToUnit(sceneObject.Coordinate, _meterToUnit),
                2 * Converter.MeterToUnit(sceneObject.Radius, _meterToUnit),
                Layers.Default
            );
    }
}