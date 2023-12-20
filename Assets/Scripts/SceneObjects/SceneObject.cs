using UnityEngine;

namespace HugeScale.SceneObjects
{
    public class SceneObject : MonoBehaviour
    {
        public double Radius;

        public Vector3d Coordinate;

        [field:SerializeField]
        public bool Solid { get; private set; }

        private int? _lastLayer;

        private SceneObjectsRegistry _sceneObjectsRegistry;

        public virtual void ApplyRenderParams(SceneObjectRenderParams renderParams)
        {
            var gameObjectTransform = gameObject.transform;
            
            gameObjectTransform.localPosition = renderParams.Position;
            gameObjectTransform.localScale = Vector3.one * renderParams.Scale;

            if (_lastLayer == null || _lastLayer != renderParams.Layer)
                SetLayerRecursive(gameObject, renderParams.Layer);
        }

        public void Hide() => 
            SetLayerRecursive(gameObject, Layers.Hide);

        public void ResetLayer() => 
            SetLayerRecursive(gameObject, Layers.Default);

        private static void SetLayerRecursive(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;

            var gameObjectTransform = gameObject.transform;

            for (var i = 0; i < gameObjectTransform.childCount; i++)
                SetLayerRecursive(gameObjectTransform.GetChild(i).gameObject, layer);
        }

        private void Awake() => 
            _sceneObjectsRegistry = GetComponentInParent<SceneObjectsRegistry>();

        private void OnEnable() => 
            _sceneObjectsRegistry.RegisterSceneObject(this);

        private void OnDisable() => 
            _sceneObjectsRegistry.UnregisterSceneObject(this);
    }
}