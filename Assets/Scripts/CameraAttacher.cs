using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAttacher : MonoBehaviour
{
    public static CameraAttacher Instance { get; private set; }

    [SerializeField] private Transform _target;
    [SerializeField] private float _translateSmoothTime = 0.3f;
    [SerializeField] private float _rotateSmoothTime = 0.01f;
    [SerializeField, Min(0.5f)] private float _minimumZoom = 2.0f;
    [SerializeField, Min(1.0f)] private float _maximumZoom = 50.0f;
    [SerializeField, Range(-180.0f, 180.0f)] private float _minimumPitch;
    [SerializeField, Range(-180.0f, 180.0f)] private float _maximumPitch;
    [SerializeField] private float _zoomSensitivity = 0.5f;
    [SerializeField] private float _pitchSensitivity = 0.1f;
    [SerializeField] private float _yawSensitivity = 0.5f;

    [SerializeField] private float _zoom = 0.0f;
    [SerializeField] private float _pitch = 0.0f;
    [SerializeField] private float _yaw = 0.0f;

    private Vector3 _velocity = Vector3.zero;

    private void Start()
    {
        Instance = this;
    }

    private void LateUpdate()
    {
        // Check for mouse input to update zoom and rotation.
        if (Input.mouseScrollDelta.y != 0)
        {
            _zoom += Input.mouseScrollDelta.y * _zoomSensitivity;
            _zoom = Mathf.Clamp(_zoom, _minimumZoom, _maximumZoom);
        }
        if (Input.GetMouseButton(1))
        {
            _yaw += Input.GetAxis("Mouse X") * _yawSensitivity;
         }
        if(Input.GetMouseButton(2))
        {
            _pitch += Input.GetAxis("Mouse Y") * _pitchSensitivity;
            _pitch = Mathf.Clamp(_pitch, _minimumPitch, _maximumPitch);
        }

        // Update position and rotation.
        UpdateCameraRotation();
        UpdateCameraPosition();
    }
    private void UpdateCameraPosition()
    {
        Vector3 targetPosition = _target.position - transform.forward * _zoom;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, _translateSmoothTime);
    }

    private void UpdateCameraRotation()
    {
        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0.0f);
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }
}
