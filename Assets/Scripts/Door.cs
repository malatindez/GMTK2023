using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Door : MonoBehaviour
{
    [SerializeField] private float _autoCloseDoorTime;
    private bool _state = false;
    private Animator _animator;
    private NavMeshObstacle _obstacle;

    private void Start() 
    { 
        _animator = GetComponent<Animator>();
        _obstacle = GetComponent<NavMeshObstacle>();
    }

    public void DoorOpen()
    {
        if (!_state)
        {
            _animator.Play(Constants.DoorOpenStageName);
            _state = true;
            StartCoroutine(AutoCloseDoor(_autoCloseDoorTime));
            _obstacle.enabled = false;
        }
    }

    private IEnumerator AutoCloseDoor(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (seconds != 0) DoorClose();
    }
    public void DoorClose() 
    { 
        _animator.Play(Constants.DoorCloseStageName);
        _obstacle.enabled = true;
    }
    private void ReversState() => _state = !_state;
}
