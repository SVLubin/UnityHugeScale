using HugeScale.Cameras;
using UnityEngine;
using UnityEngine.Serialization;

namespace HugeScale.SceneObjects.Infographics
{
    public class HaloObject : SceneObject
    {
        [SerializeField]
        private CameraDataProvider _cameraDataProvider;

        [SerializeField]
        private MeshRenderer _meshRenderer;
        
        [SerializeField]
        private SceneObject _targetSceneObject;

        [SerializeField]
        private double _minVisibleRadius = 100;
        
        [SerializeField]
        private double _maxVisibleRadius = 1000;
        
        private double _ownRadius;

        private void Start() => _ownRadius = Radius;

        private void Update()
        {
            var distanceToCamera = Vector3d.Distance(
                _cameraDataProvider.CameraCoordinate, 
                _targetSceneObject.Coordinate);

            _meshRenderer.enabled =
                distanceToCamera >= _targetSceneObject.Radius * _minVisibleRadius
                && distanceToCamera <= _targetSceneObject.Radius * _maxVisibleRadius;

            Radius = _ownRadius * distanceToCamera / (_targetSceneObject.Radius * _minVisibleRadius);
            Coordinate = _targetSceneObject.Coordinate;
        }
    }
}