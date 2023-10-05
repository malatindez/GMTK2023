using UnityEngine;

namespace EasyCharacterMovement.CharacterMovementExamples
{
    public class SimulatablePlatform : MonoBehaviour, ISimulatable
    {
        #region FIELDS

        [SerializeField]
        public float _moveTime = 3.0f;

        [SerializeField]
        private Vector3 _offset;

        #endregion

        #region PRIVATE FIELDS

        private Transform _transform;
        private Rigidbody _rigidbody;

        private Vector3 _startPosition;
        private Vector3 _targetPosition;

        private Vector3 _initialFramePosition;
        private Vector3 _updatedPosition;

        #endregion

        #region PROPERTIES
        
        public float moveTime
        {
            get => _moveTime;
            set => _moveTime = Mathf.Max(0.0001f, value);
        }

        public Vector3 offset
        {
            get => _offset;
            set => _offset = value;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Sinusoidal ease function.
        /// </summary>

        public static float EaseInOut(float time, float duration)
        {
            return -0.5f * (Mathf.Cos(Mathf.PI * time / duration) - 1.0f);
        }

        private float _timer;

        /// <summary>
        /// Move platform.
        /// </summary>

        public void Simulate(float deltaTime)
        {
            // Update platform position
            
            float t = EaseInOut(Mathf.PingPong(_timer, _moveTime), _moveTime);
            _updatedPosition = Vector3.Lerp(_startPosition, _targetPosition, t);

            // Update rigidbody and transform positions so characters can read its most up to date state

            _rigidbody.position = _updatedPosition;
            _transform.position = _updatedPosition;

            // Update timer

            _timer += deltaTime;
            if (_timer > _moveTime * 2.0f)
                _timer = 0.0f;
        }

        #endregion

        #region ISimulatable

        public void PrePhysicsUpdate(float deltaTime)
        {
            // Save last updated position

            _initialFramePosition = _updatedPosition;

            // Makesure rigidbody and transform are in its updated position (NOT the interpolated one)

            _rigidbody.position = _updatedPosition;
            _transform.position = _updatedPosition;

            // Sim this platform

            Simulate(deltaTime);
        }

        public void PostPhysicsUpdate(float deltaTime)
        {
            // EMPTY (Not used here)
        }

        public void Interpolate(float interpolationFactor)
        {
            // Set interpolated transform position

            Vector3 interpolatedPosition = Vector3.Lerp(_initialFramePosition, _updatedPosition, interpolationFactor);

            _transform.position = interpolatedPosition;
        }

        #endregion

        #region MONOBEHAVIOUR

        public void OnValidate()
        {
            moveTime = _moveTime;
        }

        public void Awake()
        {
            _transform = GetComponent<Transform>();

            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = true;
            _rigidbody.interpolation = RigidbodyInterpolation.None;

            _startPosition = _transform.position;
            _targetPosition = _startPosition + offset;
        }

        private void OnEnable()
        {
            SimulationManager.instance.AddSimulatable(this);
        }

        private void OnDisable()
        {
            SimulationManager.instance.RemoveSimulatable(this);
        }

        #endregion
    }
}
