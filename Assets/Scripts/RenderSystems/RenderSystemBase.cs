using System.Collections.Generic;
using HugeScale.Cameras;
using HugeScale.SceneObjects;
using UnityEngine;

namespace HugeScale.RenderSystems
{
    public abstract class RenderSystemBase : MonoBehaviour
    {
        [SerializeField]
        private CameraDataProvider _cameraDataProvider;
        
        [SerializeField]
        private SceneObjectsRegistry _sceneObjectsRegistry;

        protected IEnumerable<SceneObject> SceneObjects => _sceneObjectsRegistry.SceneObjects;
        
        protected Vector3d CameraCoordinate => _cameraDataProvider.CameraCoordinate;

        protected Quaternion CameraRotation => _cameraDataProvider.CameraRotation;

        private void LateUpdate() => RenderUpdate();

        protected abstract void RenderUpdate();
    }
}