using System;
using HugeScale.SceneObjects;
using UnityEngine;

namespace HugeScale
{
    public class SpaceObjectsCoordinateController : MonoBehaviour
    {
        [SerializeField]
        private SceneObject[] _targetSceneObjects;

        [SerializeField] 
        private Vector3d[] _coordinates;
        
        private void OnEnable()
        {
            if (_targetSceneObjects.Length != _coordinates.Length)
                throw new Exception($"{nameof(_targetSceneObjects)}.Length != {nameof(_coordinates)}.Length");

            for (var i = 0; i < _targetSceneObjects.Length; i++) 
                _targetSceneObjects[i].Coordinate = _coordinates[i];
        }
    }
}