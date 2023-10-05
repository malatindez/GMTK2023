using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
    [SerializeField] private string _displayName;
    [SerializeField] private float _autoCloseDoorTime;
    [SerializeField] private UnityEvent _doorStateChanged = new UnityEvent();

    [Space]
    [SerializeField] private string _openAnimationName = "IsOpen";

    private int _openAnimation;

    private bool _state = false;
    private Animator _animator;
    private NavMeshObstacle _obstacle;
    private Collider _collider;

    public string DisplayName => _displayName;
    public bool IsOpen => _state;
    public event UnityAction DoorStateChanged
    {
        add => _doorStateChanged.AddListener(value);
        remove => _doorStateChanged.RemoveListener(value);
    }

    private void Start() 
    { 
        _animator = GetComponent<Animator>();
        _obstacle = GetComponent<NavMeshObstacle>();
        _collider = GetComponent<Collider>();

        _openAnimation = Animator.StringToHash(_openAnimationName);
    }

    public void DoorOpen()
    {
        if (!_state)
        {
            _animator.SetBool(_openAnimation, true);
            _state = true;
            StartCoroutine(AutoCloseDoor(_autoCloseDoorTime));
            _obstacle.enabled = false;
            _collider.enabled = false;
            OnDoorStateChanged();
        }
    }

    private IEnumerator AutoCloseDoor(float seconds)
    {
        if (seconds == 0) yield break;

        yield return new WaitForSeconds(seconds);
        DoorClose();
    }
    public void DoorClose() 
    { 
        _animator.SetBool(_openAnimation, false);
        _obstacle.enabled = true;
        _collider.enabled = true;
        _state = false;
        OnDoorStateChanged();
    }

    [ExecuteInEditMode]
    [ContextMenu("Disable Manual Open")]
    public void DisableManualOpen()
    {
        DoorTrigger trigger = GetComponentInChildren<DoorTrigger>();
        TutorialText text = GetComponentInChildren<TutorialText>();

        trigger.gameObject.SetActive(false);
        text.gameObject.SetActive(false);
    }

    [ExecuteInEditMode]
    [ContextMenu("Enable Manual Open")]
    public void EnableManualOpen()
    {
        DoorTrigger trigger = GetComponentInChildren<DoorTrigger>();
        TutorialText text = GetComponentInChildren<TutorialText>();

        trigger.gameObject.SetActive(true);
        text.gameObject.SetActive(true);
    }

    protected virtual void OnDoorStateChanged()
    {
        _doorStateChanged?.Invoke();
    }

    private void ReversState() => _state = !_state;
}
