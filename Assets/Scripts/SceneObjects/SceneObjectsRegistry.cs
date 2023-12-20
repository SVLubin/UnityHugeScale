using System.Collections.Generic;
using UnityEngine;

namespace HugeScale.SceneObjects
{
    public class SceneObjectsRegistry : MonoBehaviour
    {
        public IEnumerable<SceneObject> SceneObjects => _sceneObjectsList;
        
        private readonly HashSet<SceneObject> _sceneObjectsList = new();

        public void RegisterSceneObject(SceneObject sceneObject) => 
            _sceneObjectsList.Add(sceneObject);

        public void UnregisterSceneObject(SceneObject sceneObject) => 
            _sceneObjectsList.Remove(sceneObject);
    }
}