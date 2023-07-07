using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFolowManager : MonoBehaviour
{
    [SerializeField] private Transform _cameraTransform;
    
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset;
    [SerializeField] private float _speed = 1f;


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
