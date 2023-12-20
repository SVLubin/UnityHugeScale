using UnityEngine;

namespace HugeScale.RenderSystems.Multilayer
{
    public class CameraRenderParams
    {
        public readonly Vector3 CameraPosition;
        
        public readonly float NearClip;

        public readonly float FarClip;

        public CameraRenderParams(Vector3 cameraPosition, float nearClip, float farClip)
        {
            CameraPosition = cameraPosition;
            NearClip = nearClip;
            FarClip = farClip;
        }
    }
}