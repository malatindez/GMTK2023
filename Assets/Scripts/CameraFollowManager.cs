using UnityEngine;

public class CameraFollowManager : MonoBehaviour
{
    [SerializeField] private Vector3 _offset;
    [SerializeField] private float _speed = 1f;

    private Transform _cameraTransform;
    private Transform _target;
    
    
    private void Start()
    {
        _cameraTransform = Camera.main.transform;
    }

    public void ChangeTarget(Transform target)
    {
        _target = target;
    }

    private void LateUpdate()
    {
        var desiredPosition = _target.position + _offset;
        var smoothedPosition = Vector3.Lerp(_cameraTransform.position, desiredPosition, _speed / 100);
        
        _cameraTransform.position = smoothedPosition;
    }
}
