using UnityEngine;


public class Enemy : Character
{
	[SerializeField] private EnemyAI _enemyAI;
	
	
	public void StartTelepathy()
	{
		_enemyAI.Disable();
		_characterController.enabled = true;
	}

	public void StopTelepathy()
	{
		_characterController.enabled = false;
		_enemyAI.Enable();
	}
}
