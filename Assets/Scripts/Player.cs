using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
		StartCoroutine(Kill());
	}

	private IEnumerator Kill()
	{
		DeathScreen.Instance.Show();
        yield return new WaitForSecondsRealtime(2f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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