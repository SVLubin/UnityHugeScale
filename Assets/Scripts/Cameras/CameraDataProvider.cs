using UnityEngine;

namespace HugeScale.Cameras
{
    public class CameraDataProvider : MonoBehaviour
    {
        [field: SerializeField]
        public Vector3d CameraCoordinate { get; set; }

        [field: SerializeField]
        public Quaternion CameraRotation { get; set; }
    }
}