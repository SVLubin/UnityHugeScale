using HugeScale.SceneObjects;

namespace HugeScale.RenderSystems.Multilayer
{
    public class SceneObjectLayer
    {
        public readonly SceneObject SceneObject;
        
        public readonly double MinDepth;

        public readonly double MaxDepth;
        
        public SceneObjectLayer(SceneObject sceneObject, double minDepth, double maxDepth)
        {
            SceneObject = sceneObject;
            
            MinDepth = minDepth;
            MaxDepth = maxDepth;
        }
    }
}