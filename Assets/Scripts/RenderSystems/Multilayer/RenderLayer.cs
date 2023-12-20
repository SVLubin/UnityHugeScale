using System.Collections.Generic;
using System.Linq;
using HugeScale.SceneObjects;
using UnityEngine;

namespace HugeScale.RenderSystems.Multilayer
{
    public class RenderLayer
    {
        private readonly double _minDepth;

        private readonly double _maxDepth;
        
        private readonly double _ctu;

        private readonly int _layer;

        private readonly List<SceneObject> _sceneObject = new();
        
        public RenderLayer(double minDepth, double maxDepth, double ctu, int layer)
        {
            _minDepth = minDepth;
            _maxDepth = maxDepth;
            
            _ctu = ctu;
            _layer = layer;
        }
        
        public void AddSceneObject(SceneObject sceneObject) => 
            _sceneObject.Add(sceneObject);

        public bool IntersectObjectLayer(SceneObjectLayer objectLayer) => 
            !(objectLayer.MaxDepth < _minDepth || objectLayer.MinDepth > _maxDepth);

        public CameraRenderParams CalculateCameraRenderParams(Vector3d cameraCoordinate, Vector3d coordinateCenter) =>
            new (
                Converter.MeterToUnit(cameraCoordinate - coordinateCenter, _ctu),
                Converter.MeterToUnit(_minDepth, _ctu),
                Converter.MeterToUnit(_maxDepth, _ctu));

        public IEnumerable<SceneObjectRenderParams> CalculateSceneObjectRenderParams(Vector3d coordinateCenter) => 
            _sceneObject.Select(x => CalculateRenderParams(x, coordinateCenter));

        private SceneObjectRenderParams CalculateRenderParams(SceneObject sceneObject, Vector3d coordinateCenter) =>
            new(
                sceneObject,
                Converter.MeterToUnit(sceneObject.Coordinate - coordinateCenter, _ctu),
                2 * Converter.MeterToUnit(sceneObject.Radius, _ctu),
                _layer);
    }
}