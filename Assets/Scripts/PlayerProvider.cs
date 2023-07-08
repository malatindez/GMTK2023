using UnityEngine;

public class PlayerProvider : MonoBehaviour
{
	[SerializeField] private Player _player;
	
	public static PlayerProvider Instance;


	public Player GetPlayer
	{
		get
		{
			if (_player == null)
				_player = FindObjectOfType<Player>();
			
			return _player;
		}
	}

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}
}