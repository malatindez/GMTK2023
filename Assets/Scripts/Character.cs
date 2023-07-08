using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
	[SerializeField, Min(0)] private float _speed = 10;
	[SerializeField, Min(0)] private float _rotationSpeed = 1;

	private CharacterController _characterController;
	private Transform _transform;


	private void Start()
	{
		_characterController = GetComponent<CharacterController>();
		_transform = transform;
	}

	public void LookAt(Vector3 point)
	{
		var position = _transform.position;
		var targetRotation = Quaternion.LookRotation(
			new Vector3(point.x, position.y, point.z) - position);

		_transform.rotation = Quaternion.Slerp(_transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
	}

	public void MoveTo(Vector3 direction)
	{
		_characterController.Move(direction * _speed);
	}
}
