using HugeScale.RenderSystems;
using UnityEngine;

namespace HugeScale.CullingTest
{
    public class ViewPyramidCullingTestSceneController : MonoBehaviour
    {
        [SerializeField]
        private Camera _mainCamera;

        [SerializeField]
        private double _depth = 100;
        
        [SerializeField]
        private Transform _targetSphereTransform;
        
        [SerializeField]
        private Camera _dummyCamera;
        
        [SerializeField]
        private Transform _dummySphereTransform;

        [SerializeField]
        private Vector3 _dummyObjectsOffset;
        
        [SerializeField]
        private bool _check;

        private void Update()
        {
            var cameraTransform = _mainCamera.transform;
            var cameraVerticalFov = _mainCamera.fieldOfView;
            
            var viewPyramid = new ViewPyramid(
                new Vector3d(cameraTransform.localPosition),
                cameraTransform.localRotation,
                _depth,
                cameraVerticalFov,
                Camera.VerticalToHorizontalFieldOfView(cameraVerticalFov, _mainCamera.aspect));
            
            _check = viewPyramid.CheckSphere(
                new Vector3d(_targetSphereTransform.localPosition),
                _targetSphereTransform.localScale.x / 2,
                out var inside,
                out var minDepth,
                out var maxDepth);

            if (_check)
            {
                _mainCamera.nearClipPlane = (float)minDepth;
                _mainCamera.farClipPlane = (float)maxDepth;
            }
            else
            {
                _mainCamera.nearClipPlane = (float)0.1;
                _mainCamera.farClipPlane = (float)_depth;
            }

            UpdateDummyObjects();
        }

        private void UpdateDummyObjects()
        {
            _dummyCamera.nearClipPlane = 0.01f;
            _dummyCamera.farClipPlane = (float)_depth;

            _dummyCamera.transform.localPosition = _dummyObjectsOffset;
            
            _dummySphereTransform.transform.localPosition = 
                Quaternion.Inverse(_mainCamera.transform.localRotation) 
                * (_targetSphereTransform.transform.localPosition - _mainCamera.transform.localPosition)
                + _dummyObjectsOffset;
        }
    }
}