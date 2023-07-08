using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Character : MonoBehaviour
{
	[SerializeField, Min(0)] private float _speed = 10;
	[SerializeField, Min(0)] private float _rotationSpeed = 1;

	protected CharacterController _characterController;
	private Transform _transform;
	private Animator _animator;
	
	public bool IsWalking
	{
		get => _animator.GetBool(nameof(IsWalking));
		set => _animator.SetBool(nameof(IsWalking), value);
	}

	private void Start()
	{
		_characterController = GetComponent<CharacterController>();
		_animator = GetComponent<Animator>();
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
