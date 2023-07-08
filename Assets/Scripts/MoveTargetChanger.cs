using UnityEngine;

public class MoveTargetChanger : MonoBehaviour
{
	[SerializeField] private CharacterMoveController _characterMoveController;

	private Player _player;
	private Camera _camera;

	
	private void Start()
	{
		_player = PlayerProvider.Instance.GetPlayer;
		_camera = Camera.main;
	}

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

		bool WithinRange(Transform characterTransform)
			=> Vector3.Distance(characterTransform.position, _player.transform.position) <= _player.MaxPerceptionShiftRange;

		var ray = _camera.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out var hit) && WithinRange(hit.transform))
			return hit.collider.TryGetComponent<Enemy>(out target);
		
		return false;
	}
}
