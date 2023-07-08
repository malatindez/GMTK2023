using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : Character
{
	[SerializeField, Min(0)] private float _maxPerceptionShiftRange  = 3;
	
	public float MaxPerceptionShiftRange => _maxPerceptionShiftRange;

	public void TryKill()
	{
		Debug.Log("PLAYER DEAD!!!");
	}
}