using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTargetChanger : MonoBehaviour
{
	[SerializeField] private CharacterMoveController _characterMoveController;
	[SerializeField] private Player _player;

	
	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
			OnMouseButtonClick();
		else if (Input.GetKeyDown(KeyCode.Q))
			_characterMoveController.ChangeTarget(_player);
	}

	private void OnMouseButtonClick()
	{
		if (_characterMoveController.Target is not Player) return;
		
		if (TryGetTargetWithinPlayerRange(out var target))
			_characterMoveController.ChangeTarget(target);
	}

	private bool TryGetTargetWithinPlayerRange(out Enemy target)
	{
		target = default;

		bool WithinRange(Transform transform)
			=> Vector3.Distance(transform.position, _player.transform.position) <= _player.MaxPerceptionShiftRange;

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out var hit) && WithinRange(hit.transform))
			return hit.collider.TryGetComponent<Enemy>(out target);
		
		return false;
	}
	
	
}
