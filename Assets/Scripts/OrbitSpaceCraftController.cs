using HugeScale.Cameras;
using HugeScale.SceneObjects;
using UnityEngine;

namespace HugeScale
{
    public class OrbitSpaceCraftController : MonoBehaviour
    {
        [SerializeField]
        private OrbitCameraController _orbitCameraController;
        
        [SerializeField]
        private SceneObject OrbitSpaceCraftObject;
        
        [SerializeField]
        private double _spaceCraftOrbitRadius = 1.1f;
        
        [SerializeField]
        private float _spaceCraftAzimuth = -70f;
        
        [SerializeField]
        private float _spaceCraftElevation = -10f;
        
        private void Update()
        {
            var targetSceneObject = _orbitCameraController.TargetSceneObject;
            
            var offset = new Vector3d(
                Quaternion.AngleAxis(_spaceCraftAzimuth, Vector3.up) * Quaternion.AngleAxis(_spaceCraftElevation, Vector3.right) * Vector3.forward);
                
            OrbitSpaceCraftObject.Coordinate = targetSceneObject.Coordinate + offset.normalized * targetSceneObject.Radius * _spaceCraftOrbitRadius;
        }
    }
}