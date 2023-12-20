using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace HugeScale.UI
{
    public class MinFPSCounter : MonoBehaviour
    {
        [SerializeField]
        private Text _text;
        
        private int _lastFrameIndex;

        [SerializeField]
        private int _frameArrayLength = 300;

        [SerializeField]
        private float _updateTextInterval = 1f;
        
        private float _updateTimer;
        
        private float[] _frameDeltaTimeArray;

        private void Start()
        {
            _frameDeltaTimeArray = new float[_frameArrayLength];
            _updateTimer = _updateTextInterval;
            
            _text.enabled = false;
        }

        private void Update() 
        {
            _frameDeltaTimeArray[_lastFrameIndex] = Time.unscaledDeltaTime;
            _lastFrameIndex = (_lastFrameIndex + 1) % _frameDeltaTimeArray.Length;

            if (_updateTimer <= 0)
            {
                _text.enabled = true;
                _text.text = Mathf.RoundToInt(CalculateFPS()).ToString();
                
                _updateTimer = _updateTextInterval;
            }
            else
                _updateTimer -= Time.deltaTime;
        }

        private float CalculateFPS() => 
            _frameDeltaTimeArray.Length / _frameDeltaTimeArray.Sum();
    }
}