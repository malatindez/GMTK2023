using UnityEngine;

[RequireComponent(typeof(CameraFollowManager))]
public class CharacterMoveController : MonoBehaviour
{
	private CameraFollowManager _cameraFollowManager;
	private Character _target;
	private Camera _camera;

	public Character Target => _target;
	
	
	private void Start()
	{
		_cameraFollowManager = GetComponent<CameraFollowManager>();
		ChangeTarget(PlayerProvider.Instance.GetPlayer);
		_camera = Camera.main;
	}

	public void ChangeTarget(Character target)
	{
		_target = target;
		_cameraFollowManager.ChangeTarget(target.transform);
	}

	private void Update()
	{
		var moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));


		_target.IsWalking = moveDirection != Vector3.zero;
		_target.MoveTo(moveDirection.normalized * Time.deltaTime);

		var cameraRay = _camera.ScreenPointToRay(Input.mousePosition);
		var groundPlane = new Plane(Vector3.up, Vector3.zero);

		if (groundPlane.Raycast(cameraRay, out var rayLength))
			_target.LookAt(cameraRay.GetPoint(rayLength));
	}
}
