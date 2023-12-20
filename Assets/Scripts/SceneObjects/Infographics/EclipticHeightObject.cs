using UnityEngine;

namespace HugeScale.SceneObjects.Infographics
{
    public class EclipticHeightObject : SceneObject
    {
        [SerializeField]
        private SceneObject _targetSceneObject;

        [SerializeField]
        private Transform _cylinderTransform;

        [SerializeField]
        private MeshRenderer _meshRenderer;

        [SerializeField]
        private double _minHeight = 10;

        [SerializeField]
        private float _thickness = 0.001f;

        private void Update()
        {
            Coordinate = new Vector3d(
                _targetSceneObject.Coordinate.x,
                0,
                _targetSceneObject.Coordinate.z);

            Radius = _targetSceneObject.Coordinate.y;

            _meshRenderer.enabled = Radius >= _minHeight;

            if (_meshRenderer.enabled)
            {
                var scale = (float)(_thickness * _targetSceneObject.Radius / Radius);

                _cylinderTransform.localScale = new Vector3(scale, 0.25f, scale);
            }
        }
    }
}