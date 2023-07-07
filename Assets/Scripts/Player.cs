using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
	[SerializeField, Min(0)] private float _speed;
	private CharacterController _characterController;
    
	private void Start()
	{
		_characterController = GetComponent<CharacterController>();
	}

	private void Update()
	{
		Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

		if (moveDirection != Vector3.zero)
		{
			transform.rotation = Quaternion.LookRotation(moveDirection);
		}

		_characterController.Move(moveDirection.normalized * Time.deltaTime * _speed);
	}
}