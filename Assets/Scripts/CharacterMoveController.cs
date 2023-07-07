using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CameraFolowManager))]
public class CharacterMoveController : MonoBehaviour
{
	[SerializeField] private Player _player;

	private CameraFolowManager _cameraFolowManager;
	private Character _target;

	public Character Target => _target;
	
    
	private void Start()
	{
		_cameraFolowManager = GetComponent<CameraFolowManager>();
		_target = _player;
	}

	public void ChangeTarget(Character target)
	{
		_target = target;
		_cameraFolowManager.ChangeTarget(target.transform);
	}

	private void Update()
	{
		Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

		_target.MoveTo(moveDirection.normalized * Time.deltaTime);

		var cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		var groundPlane = new Plane(Vector3.up, Vector3.zero);

		if (groundPlane.Raycast(cameraRay, out var rayLength))
			_target.LookAt(cameraRay.GetPoint(rayLength));
	}
}
