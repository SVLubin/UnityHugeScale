using UnityEngine;

namespace HugeScale.SceneObjects
{
    public class SceneObjectRenderParams
    {
        public readonly SceneObject SceneObject;
        
        public readonly Vector3 Position;
        
        public readonly float Scale;

        public readonly int Layer;

        public SceneObjectRenderParams(SceneObject sceneObject, Vector3 position, float scale, int layer)
        {
            SceneObject = sceneObject;
            Position = position;
            Scale = scale;
            Layer = layer;
        }
    }
}