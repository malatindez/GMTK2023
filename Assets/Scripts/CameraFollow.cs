using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	[SerializeField] private Transform _target;
	[SerializeField] private Vector3 _offset;
	[SerializeField] private float _speed = 1f;

	private Transform _transform;
	

	private void Start()
	{
		_transform = transform;
	}

	private void LateUpdate()
	{
		var desiredPosition = _target.position + _offset;
		var smoothedPosition = Vector3.Lerp(_transform.position, desiredPosition, _speed / 100);
		_transform.position = smoothedPosition;
	}
}