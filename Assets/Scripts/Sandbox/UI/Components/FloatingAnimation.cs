using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.UI.Components
{
    public class FloatingAnimation : MonoBehaviour
    {
        [Header("Scale Animation")]
        [SerializeField] private float _scaleAmount = 0.1f;
        [SerializeField] private float _scaleSpeed = 1.0f;
        
        [Header("Rotation Animation")]
        [SerializeField] private float _rotationAmount = 5.0f;
        [SerializeField] private float _rotationSpeed = 0.8f;
        
        [Header("Position Animation")]
        [SerializeField] private Vector2 _positionAmount = new Vector2(3.0f, 5.0f);
        [SerializeField] private float _positionSpeed = 1.2f;
        
        [Header("Animation Timing")]
        [SerializeField] private float _animationOffset = 0.0f;
        [SerializeField] private bool _useRandomOffset = true;
        
        [Header("Animation Curves")]
        [SerializeField] private AnimationCurve _animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private RectTransform _rectTransform;
        private Vector3 _originalScale;
        private Vector3 _originalPosition;
        private float _originalRotation;
        
        private float _scaleOffset;
        private float _rotationOffset;
        private Vector2 _positionOffset;

        void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            
            if (_rectTransform == null)
            {
                Debug.LogError($"FloatingAnimation requires a RectTransform component on {gameObject.name}");
                enabled = false;
                return;
            }
            
            // Store original values
            _originalScale = _rectTransform.localScale;
            _originalPosition = _rectTransform.anchoredPosition;
            _originalRotation = _rectTransform.localEulerAngles.z;
            
            // Combine manual offset with optional random offsets
            float baseOffset = _animationOffset * Mathf.PI * 2f; // Convert 0-1 range to 0-2π
            float randomVariation = _useRandomOffset ? Random.Range(0f, 0.5f) : 0f;
            
            _scaleOffset = baseOffset + randomVariation;
            _rotationOffset = baseOffset + (_useRandomOffset ? Random.Range(0f, 0.5f) : 0f);
            _positionOffset = new Vector2(
                baseOffset + (_useRandomOffset ? Random.Range(0f, 0.5f) : 0f),
                baseOffset + (_useRandomOffset ? Random.Range(0f, 0.5f) : 0f)
            );
        }

        void Update()
        {
            if (_rectTransform == null) return;
            
            float time = Time.time;
            
            // Scale animation
            float scaleTime = (time * _scaleSpeed + _scaleOffset) % (2f * Mathf.PI);
            float scaleCurveValue = _animationCurve.Evaluate(scaleTime / (2f * Mathf.PI));
            float scaleMultiplier = 1f + (scaleCurveValue - 0.5f) * 2f * _scaleAmount;
            _rectTransform.localScale = _originalScale * scaleMultiplier;
            
            // Rotation animation
            float rotationTime = (time * _rotationSpeed + _rotationOffset) % (2f * Mathf.PI);
            float rotationCurveValue = _animationCurve.Evaluate(rotationTime / (2f * Mathf.PI));
            float rotationValue = _originalRotation + (rotationCurveValue - 0.5f) * 2f * _rotationAmount;
            _rectTransform.localEulerAngles = new Vector3(0, 0, rotationValue);
            
            // Position animation (both X and Y floating)
            float positionTimeX = (time * _positionSpeed + _positionOffset.x) % (2f * Mathf.PI);
            float positionTimeY = (time * _positionSpeed + _positionOffset.y) % (2f * Mathf.PI);
            float positionCurveValueX = _animationCurve.Evaluate(positionTimeX / (2f * Mathf.PI));
            float positionCurveValueY = _animationCurve.Evaluate(positionTimeY / (2f * Mathf.PI));
            float positionX = _originalPosition.x + (positionCurveValueX - 0.5f) * 2f * _positionAmount.x;
            float positionY = _originalPosition.y + (positionCurveValueY - 0.5f) * 2f * _positionAmount.y;
            _rectTransform.anchoredPosition = new Vector2(positionX, positionY);
        }
    }
}