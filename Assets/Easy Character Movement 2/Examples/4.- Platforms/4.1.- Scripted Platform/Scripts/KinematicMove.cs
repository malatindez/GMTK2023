using UnityEngine;

namespace EasyCharacterMovement.Examples.Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class KinematicMove : MonoBehaviour
    {
        #region FIELDS

        [SerializeField]
        public float _moveTime = 3.0f;

        [SerializeField]
        private Vector3 _offset;

        #endregion

        #region PRIVATE FIELDS

        private Rigidbody _rigidbody;

        private Vector3 _startPosition;
        private Vector3 _targetPosition;

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

        #endregion

        #region MONOBEHAVIOUR

        public void OnValidate()
        {
            moveTime = _moveTime;
        }

        public void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = true;

            _startPosition = transform.position;
            _targetPosition = _startPosition + offset;
        }

        public void FixedUpdate()
        {
            float t = EaseInOut(Mathf.PingPong(Time.time, _moveTime), _moveTime);
            Vector3 p = Vector3.Lerp(_startPosition, _targetPosition, t);

            _rigidbody.MovePosition(p);
        }

        #endregion
    }
}
