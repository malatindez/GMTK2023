using UnityEngine;


public class MoveTargetChanger : MonoBehaviour
{
	[SerializeField] private CharacterMoveController _characterMoveController;

	private Player _player;
	private Camera _camera;
	private Character _target;

	
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
			ChangeTarget(_player);
	}

	private void OnMouseButtonClick()
	{
		if (_characterMoveController.Target is not Player) return;

		if (TryGetTargetWithinPlayerRange(out var target))
			ChangeTarget(target);
	}

	private void ChangeTarget(Character target)
	{
		if (target is Enemy enemy)
		{
			enemy.StartTelepathy();
			_characterMoveController.ChangeTarget(enemy);
		}
		else
		{
			if (_target != null && _target is Enemy usedEnemy)
				usedEnemy.StopTelepathy();
			
			_characterMoveController.ChangeTarget(_player);
		}
	}

	private bool TryGetTargetWithinPlayerRange(out Enemy target)
	{
		target = default; 
		var maxDistance = _player.MaxPerceptionShiftRange;

		bool WithinRange(Transform characterTransform)
			=> Vector3.Distance(characterTransform.position, _player.transform.position) <= maxDistance;

		var ray = _camera.ScreenPointToRay(Input.mousePosition);

		if (!Physics.Raycast(ray, out var hit) || !WithinRange(hit.transform) ||
		    !hit.collider.TryGetComponent(out target)) return false;
		
		var playerPosition = _player.transform.position;
		var targetPosition = target.transform.position;
		ray = new Ray(playerPosition, (targetPosition - playerPosition).normalized);

		return !(Physics.Raycast(ray, out hit, maxDistance) && hit.collider.TryGetComponent<Obstacle>(out _));

	}
}
