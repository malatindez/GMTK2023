using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : Character
{
	[SerializeField, Min(0)] private float _maxPerceptionShiftRange  = 3;
	
	public float MaxPerceptionShiftRange => _maxPerceptionShiftRange;

    private void Start()
    {
		StartCoroutine(Spawn());
    }

    public void TryKill()
	{
		// [TODO]: fix

		//StopAllCoroutines();
		//IsWalking = false;
		//_animator.SetTrigger(Constants.PlayerDiedTrigger);
	}

	private IEnumerator Spawn()
	{
		yield return null;

		if (CheckPointManager.TryGetSpawnPosition(out Vector3 position))
		{
			transform.position = position;
		}
	}
}