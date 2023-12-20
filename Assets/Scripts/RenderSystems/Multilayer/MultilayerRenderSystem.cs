using System;
using System.Collections.Generic;
using System.Linq;
using HugeScale.SceneObjects;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace HugeScale.RenderSystems.Multilayer
{
    public class MultilayerRenderSystem : RenderSystemBase
    {
        [SerializeField]
        private Camera[] _overlayCameras;
        
        [SerializeField]
        private Camera _mainCamera;
        
        [SerializeField]
        private UniversalAdditionalCameraData _mainCameraUniversalRendererData;
        
        [SerializeField]
        private double _meterToUnit = 1e-09d;
        
        [SerializeField]
        private double _sceneRadius = 1e+13d;

        [SerializeField]
        private float _firstCameraNearClip = 0.1f;
        
        [SerializeField]
        private float _minDistanceToCamera = 0.5f;
        
        [SerializeField]
        private float _renderLayerSize = 5000;

        [SerializeField]
        private double _minDepthForNonSolidObjects = 100000;
        
        private Vector3d _coordinateCenter;
        
        private double _farLayerMinDepth;

        private float _cameraVerticalFov;
        
        private float _cameraHorizontalFov;

        private CameraRenderParams _mainCameraRenderParams;
        
        private IEnumerable<SceneObjectRenderParams> _currentOverlaySceneObjectRenderParams;

        private readonly List<RenderLayer> _renderLayerList = new();

        private void Start() => 
            _farLayerMinDepth = Converter.UnitToMeter(_firstCameraNearClip, _meterToUnit);

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera == _mainCamera)
                OnBeginMainCameraRendering();
            else if (TryFindOverlayCamera(camera, out var index))
                OnBeginOverlayCameraRendering(camera, index);
        }
        
        private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera == _mainCamera)
                return;

            if (TryFindOverlayCamera(camera, out var index))
            {
                OnEndOverlayCameraRendering();

                if (index == 0)
                    OnEndLastOverlayCameraRendering();
            }
        }

        private bool TryFindOverlayCamera(Camera camera, out int index)
        {
            index = Array.IndexOf(_overlayCameras, camera);

            return index != -1;
        }

        protected override void RenderUpdate()
        {
            UpdateCameraFov();
            UpdateCoordinateCenter();
            UpdateRenderLayers();
            UpdateCamerasEnable();
            UpdateMainCameraRenderParams();
            UpdateMainCameraEnablePostProcessing();
        }

        private void UpdateCameraFov()
        {
            _cameraVerticalFov = _mainCamera.fieldOfView;
            
            _cameraHorizontalFov = 
                Camera.VerticalToHorizontalFieldOfView(_cameraVerticalFov, _mainCamera.aspect);
        }

        private void UpdateCoordinateCenter() =>
            _coordinateCenter = CameraCoordinate;

        private void UpdateRenderLayers()
        {
            _renderLayerList.Clear();
            
            var viewPyramid = CreateCameraViewPyramid();
            var sceneObjectLayers = CheckObjectsInViewPyramid(SceneObjects, viewPyramid).ToArray();
            
            if (!sceneObjectLayers.Any())
                return;
            
            var currentDepth = DetermineMinDepth(sceneObjectLayers);

            if (Mathd.Approximately(currentDepth, 0))
                throw new Exception($"{nameof(currentDepth)} == 0");

            do
            {
                if (_renderLayerList.Count + 1 > _overlayCameras.Length)
                {
                    Debug.LogError("The count of render layers is more than the count of overlay cameras");
                    break;
                }
                
                var layerCtu = _minDistanceToCamera / currentDepth;
                var layerDepthSize = Converter.UnitToMeter(_renderLayerSize, layerCtu);
                
                var renderLayer = new RenderLayer(
                    currentDepth,
                    Mathd.Min(currentDepth + layerDepthSize, _farLayerMinDepth),
                    layerCtu,
                    Layers.CustomRender);
                
                AddObjectLayersToRenderLayer(sceneObjectLayers, renderLayer);
                
                _renderLayerList.Add(renderLayer);

                currentDepth += layerDepthSize;
            } 
            while (currentDepth < _farLayerMinDepth);
        }

        private double DetermineMinDepth(IEnumerable<SceneObjectLayer> sceneObjectLayers)
        {
            var minDepthForSolidObjects = FindMinDepth(sceneObjectLayers.Where(x => x.SceneObject.Solid));
            var minDepthForNonSolidObjects = FindMinDepth(sceneObjectLayers.Where(x => !x.SceneObject.Solid));
            
            if (minDepthForSolidObjects != null && minDepthForNonSolidObjects != null)
            {
                if (minDepthForSolidObjects < _minDepthForNonSolidObjects)
                    return minDepthForSolidObjects.Value;

                if (minDepthForNonSolidObjects.Value < _minDepthForNonSolidObjects)
                    return _minDepthForNonSolidObjects;
                
                return Mathd.Min(minDepthForSolidObjects.Value, minDepthForNonSolidObjects.Value);
            }
            
            if (minDepthForSolidObjects == null && minDepthForNonSolidObjects != null)
                return Mathd.Max(minDepthForNonSolidObjects.Value, _minDepthForNonSolidObjects);
            
            if (minDepthForNonSolidObjects == null && minDepthForSolidObjects != null)
                return minDepthForSolidObjects.Value;
            
            throw new Exception("There are no objects");
        }

        private double? FindMinDepth(IEnumerable<SceneObjectLayer> sceneObjectLayers)
        {
            if (!sceneObjectLayers.Any())
                return null;
            
            return sceneObjectLayers.Min(x => x.MinDepth);
        }

        private static void AddObjectLayersToRenderLayer(IEnumerable<SceneObjectLayer> sceneObjectLayers, RenderLayer renderLayer)
        {
            foreach (var sceneObjectLayer in sceneObjectLayers)
                if (renderLayer.IntersectObjectLayer(sceneObjectLayer))
                    renderLayer.AddSceneObject(sceneObjectLayer.SceneObject);
        }

        private void UpdateCamerasEnable()
        {
            for (var i = 0; i < _overlayCameras.Length; i++) 
                _overlayCameras[i].gameObject.SetActive(i < _renderLayerList.Count);
        }

        private void UpdateMainCameraRenderParams()
        {
            var cameraPosition = Converter.MeterToUnit(CameraCoordinate - _coordinateCenter, _meterToUnit);
            var cameraFarClip = Converter.MeterToUnit(2 * _sceneRadius, _meterToUnit);
            
            _mainCameraRenderParams = new CameraRenderParams(cameraPosition, _firstCameraNearClip, cameraFarClip);
        }

        private void UpdateMainCameraEnablePostProcessing() =>
            _mainCameraUniversalRendererData.renderPostProcessing = _renderLayerList.Count == 0;

        private static IEnumerable<SceneObjectLayer> CheckObjectsInViewPyramid(IEnumerable<SceneObject> sceneObjects, ViewPyramid viewPyramid)
        {
            var sceneObjectLayerList = new List<SceneObjectLayer>();
            
            foreach (var sceneObject in sceneObjects)
                if (viewPyramid.CheckSphere(
                        sceneObject.Coordinate,
                        sceneObject.Radius,
                        out var cornerInsideSphere,
                        out var minDepth,
                        out var maxDepth) 
                    && (!cornerInsideSphere || !sceneObject.Solid))
                    sceneObjectLayerList.Add(
                        new SceneObjectLayer(sceneObject, minDepth.Value, maxDepth.Value));

            return sceneObjectLayerList;
        }
        
        private ViewPyramid CreateCameraViewPyramid() =>
            new(
                CameraCoordinate,
                CameraRotation,
                _farLayerMinDepth,
                _cameraVerticalFov,
                _cameraHorizontalFov);

        private void OnBeginMainCameraRendering()
        {
            ApplyRenderParamsToCamera(_mainCamera, _mainCameraRenderParams);
            
            foreach (var sceneObject in SceneObjects) 
                sceneObject.ApplyRenderParams(CalculateRenderParams(sceneObject));
        }
        
        private SceneObjectRenderParams CalculateRenderParams(SceneObject sceneObject) =>
            new(
                sceneObject,
                Converter.MeterToUnit(sceneObject.Coordinate - _coordinateCenter, _meterToUnit),
                2 * Converter.MeterToUnit(sceneObject.Radius, _meterToUnit),
                Layers.Default
            );

        private void OnBeginOverlayCameraRendering(Camera camera, int layerIndex)
        {
            var renderLayer = _renderLayerList[layerIndex];
            
            var cameraRenderParams = renderLayer.CalculateCameraRenderParams(CameraCoordinate, _coordinateCenter);
            
            _currentOverlaySceneObjectRenderParams = renderLayer.CalculateSceneObjectRenderParams(_coordinateCenter);

            ApplyRenderParamsToCamera(camera, cameraRenderParams);

            foreach (var sceneObjectRenderParam in _currentOverlaySceneObjectRenderParams)
                sceneObjectRenderParam.SceneObject.ApplyRenderParams(sceneObjectRenderParam);
        }

        private void OnEndOverlayCameraRendering()
        {
            foreach (var sceneObjectRenderParam in _currentOverlaySceneObjectRenderParams)
                sceneObjectRenderParam.SceneObject.ResetLayer();
        }

        private void OnEndLastOverlayCameraRendering()
        {
#if UNITY_EDITOR
            // Resetting objects params just for the correct view in the unity scene
            foreach (var sceneObject in SceneObjects) 
                sceneObject.ApplyRenderParams(CalculateRenderParams(sceneObject));
#endif
        }

        private void ApplyRenderParamsToCamera(Camera camera, CameraRenderParams renderParams)
        {
            var cameraTransform = camera.transform;
            
            cameraTransform.localPosition = renderParams.CameraPosition;
            cameraTransform.localRotation = CameraRotation;

            camera.nearClipPlane = renderParams.NearClip;
            camera.farClipPlane = renderParams.FarClip;
        }
    }
}