using HugeScale.SceneObjects;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HugeScale.Cameras
{
    public class OrbitCameraController : MonoBehaviour
    {
        [SerializeField]
        private CameraDataProvider _cameraDataProvider;

        [SerializeField]
        private SceneObject _startTargetSceneObject;
        
        [SerializeField]
        private EventTrigger _eventTrigger;

        [SerializeField]
        private float _draggingScaleFactor = 0.001f;
        
        [SerializeField]
        private float _scrollingScaleFactor = 0.01f;
        
        [SerializeField]
        private float _defaultAzimuth = 180f;
        
        [SerializeField]
        private float _defaultElevation = -45f;
        
        [SerializeField]
        private float _elevationLimit = 85;
        
        [SerializeField]
        private float _defaultZoomRadius = 5f;
        
        [SerializeField]
        private float _minZoomRadius = 1.1f;
        
        [SerializeField]
        private float _maxZoomRadius = 30f;

        private float _azimuth;
        private float _elevation;
        private float _zoom;

        private EventTrigger.Entry _dragEntry;
        private EventTrigger.Entry _scrollEntry;
        
        public SceneObject TargetSceneObject { get; private set; }

        private void Awake()
        {
            _dragEntry = new EventTrigger.Entry();
            
            _dragEntry.eventID = EventTriggerType.Drag;
            _dragEntry.callback.AddListener(data => { OnDrag((PointerEventData)data); });
            
            _scrollEntry = new EventTrigger.Entry();
            
            _scrollEntry.eventID = EventTriggerType.Scroll;
            _scrollEntry.callback.AddListener(data => { OnScroll((PointerEventData)data); });

            SetTargetSceneObject(_startTargetSceneObject);
        }

        private void OnEnable()
        {
            _eventTrigger.triggers.Add(_dragEntry);
            _eventTrigger.triggers.Add(_scrollEntry);
        }
        
        private void OnDisable()
        {
            _eventTrigger.triggers.Remove(_dragEntry);
            _eventTrigger.triggers.Remove(_scrollEntry);
        }

        private void OnDrag(PointerEventData data)
        {
            _azimuth += data.delta.x * _draggingScaleFactor * Screen.dpi;
            _elevation += data.delta.y * _draggingScaleFactor * Screen.dpi;

            _azimuth %= 360;
            _elevation = Mathf.Clamp(_elevation, -_elevationLimit, _elevationLimit);
        }
        
        private void OnScroll(PointerEventData data)
        {
            _zoom += data.scrollDelta.y * _scrollingScaleFactor;
            _zoom = Mathf.Clamp(_zoom, _minZoomRadius, _maxZoomRadius);
        }
        
        public void SetTargetSceneObject(SceneObject sceneObject)
        {
            TargetSceneObject = sceneObject;
            
            Reset();
        }

        private void Reset()
        {
            _azimuth = _defaultAzimuth;
            _elevation = _defaultElevation;
            
            _zoom = _defaultZoomRadius;
        }

        private void UpdateCameraCoordinate()
        {
            var offset = new Vector3d(
                 Quaternion.AngleAxis(_azimuth, Vector3.up) * Quaternion.AngleAxis(_elevation, Vector3.right) * Vector3.forward);
                
            _cameraDataProvider.CameraCoordinate = TargetSceneObject.Coordinate + offset.normalized * TargetSceneObject.Radius * _zoom;
            
            _cameraDataProvider.CameraRotation = Quaternion.LookRotation(
                (TargetSceneObject.Coordinate - _cameraDataProvider.CameraCoordinate).ToVector3(),
                Vector3.up);
        }

        private void Update() => UpdateCameraCoordinate();
    }
}