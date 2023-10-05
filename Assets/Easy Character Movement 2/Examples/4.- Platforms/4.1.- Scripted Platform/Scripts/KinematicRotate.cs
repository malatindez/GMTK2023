using UnityEngine;

namespace EasyCharacterMovement.Examples.Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class KinematicRotate : MonoBehaviour
    {
        #region FIELDS

        [SerializeField]
        private float _rotationSpeed = 30.0f;

        #endregion

        #region PRIVATE FIELDS

        private Rigidbody _rigidbody;

        private float _angle;

        #endregion

        #region PROPERTIES

        public float rotationSpeed
        {
            get => _rotationSpeed;
            set => _rotationSpeed = value;
        }

        public float angle
        {
            get => _angle;
            set => _angle = MathLib.Clamp0360(value);
        }

        #endregion

        #region MONOBEHAVIOUR

        public void OnValidate()
        {
            rotationSpeed = _rotationSpeed;
        }

        public void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = true;
        }

        public void FixedUpdate()
        {
            angle += rotationSpeed * Time.deltaTime;
            
            Quaternion rotation = Quaternion.Euler(0.0f, angle, 0.0f);
            _rigidbody.MoveRotation(rotation);
        }

        #endregion
    }
}
