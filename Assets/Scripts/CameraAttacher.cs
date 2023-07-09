using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAttacher : MonoBehaviour
{
    public static CameraAttacher Instance { get; private set; }

    [SerializeField] private Transform _target;
    [SerializeField] private float _smoothTime = 0.3f;

    private Vector3 _offset;
    private Vector3 _velocity = Vector3.zero;

    private void Start()
    {
        _offset = transform.position - _target.position;

        Instance = this;
    }

    private void LateUpdate()
    {
        // update position
        Vector3 targetPosition = _target.position + _offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, _smoothTime);
    } 

    public void SetTarget(Transform target)
    {
        _target = target;
    }
}
