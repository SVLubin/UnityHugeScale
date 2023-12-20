using HugeScale.Cameras;
using UnityEngine;

namespace HugeScale.SceneObjects.Infographics
{
    public class EclipticPlaneObject : SceneObject
    {
        [SerializeField]
        private MeshRenderer _meshRenderer;

        [SerializeField]
        private CameraDataProvider _cameraDataProvider;

        private static readonly int ShaderPropertyRadius = Shader.PropertyToID("_Radius");
        private static readonly int ShaderPropertyCameraCoordinate = Shader.PropertyToID("_CameraCoordinate");

        public override void ApplyRenderParams(SceneObjectRenderParams renderParams)
        {
            base.ApplyRenderParams(renderParams);
            
            _meshRenderer.material.SetFloat(ShaderPropertyRadius, (float)Radius);
            
            _meshRenderer.material.SetVector(
                ShaderPropertyCameraCoordinate, 
                _cameraDataProvider.CameraCoordinate.ToVector3());
        }
    }
}