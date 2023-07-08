using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private float _autoCloseDoorTime;
    private bool _state = false;
    private Animator _animator;

    private void Start() => _animator = GetComponent<Animator>();

    public void DoorOpen()
    {
        if (!_state)
        {
            _animator.Play("door-open");
            _state = true;
            StartCoroutine(AutoCloseDoor(_autoCloseDoorTime));
        }
    }

    private IEnumerator AutoCloseDoor(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (seconds != 0) DoorClose();
    }
    public void DoorClose() => _animator.Play("door-close");
    private void ReversState() => _state = !_state;
}
