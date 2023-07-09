using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent (typeof(FieldOfView))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] private PatrolPoint[] _patrolPoints;
    [SerializeField] private GameObject _noticeFx; 
    [SerializeField] private VisibilityCone _visibilityCone;

    private Animator _animator;
    private NavMeshAgent _agent;
    private AudioSource _noticeSound;
    private FieldOfView _fieldOfView;
    private bool _isPlayerNoticed;
    private GameObject _target;
    private PlayerV2 _currentOwner;

    private Queue<PatrolPoint> _points;
    private PatrolPoint _currentPoint;

    private bool IsWalking
    {
        get => _animator.GetBool(nameof(IsWalking));
        set => _animator.SetBool(nameof(IsWalking), value);
    }

    public bool IsUnderControl { get; private set; }

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _noticeSound = GetComponent<AudioSource>();
        _agent = GetComponent<NavMeshAgent>();
        _fieldOfView = GetComponent<FieldOfView>();
        _points = new Queue<PatrolPoint>(_patrolPoints);
        StartCoroutine(Patrol());
    }

    public void StartTelepaty(PlayerV2 owner)
    {
        StopAllCoroutines();
        IsWalking = false;
        IsUnderControl = true;
        _currentPoint = null;

        _currentOwner = owner;
        _visibilityCone.enabled = true;

        StartCoroutine(Control());
    }

    public void StopTelepaty()
    {
        StopAllCoroutines();
        StartCoroutine(Patrol());
        IsUnderControl = false;
        _visibilityCone.enabled = false;
        _currentOwner.EndTelepaty();
    }

    private IEnumerator Control()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                StopTelepaty();
                yield break;
            }

            Move();
            RotateToMouse();

            yield return null;
        }
    }

    private void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        //camera forward and right vectors:
        var forward = Camera.main.transform.forward;
        var right = Camera.main.transform.right;

        //project forward and right vectors on the horizontal plane (y = 0)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        //this is the direction in the world space we want to move:
        var desiredMoveDirection = forward * vertical + right * horizontal;

        var dir = transform.position + desiredMoveDirection * 2;
        _agent.SetDestination(dir);
        IsWalking = horizontal != 0f || vertical != 0f;
    }

    private void RotateToMouse()
    {
        //Get the Screen positions of the object
        Vector2 positionOnScreen = Camera.main.WorldToViewportPoint(transform.position);

        //Get the Screen position of the mouse
        Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);

        //Get the angle between the points
        float angle = AngleBetweenTwoPoints(positionOnScreen, mouseOnScreen);

        //Ta Daaa
        transform.rotation = Quaternion.Euler(new Vector3(0f, -angle + 180f, 0f));

    }

    private float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }


    private IEnumerator Patrol()
    {
        while (true)
        {
            if (_fieldOfView.CanSeeTarget)
            {
                OnNoticed();
                _isPlayerNoticed = true;
                _target = _fieldOfView.Target;
                yield return new WaitForSeconds(1f);
                _agent.SetDestination(_target.transform.position);
                StartCoroutine(Hunt());
                _agent.speed = 7;
                

                yield break;
            }

            if (_currentPoint == null)
            {
                _currentPoint = _points.Dequeue();
                _points.Enqueue(_currentPoint);

                _agent.SetDestination(_currentPoint.transform.position);
                IsWalking = true;
            }

            if (ComparePoints(transform.position, _currentPoint.transform.position))
            {
                if (_currentPoint.Delay > 0)
                {
                    IsWalking = false;
                    yield return new WaitForSeconds(_currentPoint.Delay);
                }

                _currentPoint = null;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.TryGetComponent(out PlayerV2 player))
        {
            player.TryKill();
            StopAllCoroutines();
            IsWalking = false;

            _agent.enabled = false;
            _agent.SetDestination(transform.position);
        }
    }

    private bool ComparePoints(Vector3 left, Vector3 right)
    {
        var a = new Vector2(left.x, left.z);
        var b = new Vector2(right.x, right.z);
         
        return Vector2.Distance(a, b) < 0.1f;
    }

    private IEnumerator Hunt()
    {
        while (true)
        {
            _agent.SetDestination(_target.transform.position);

            yield return new WaitForFixedUpdate();
        }
    }

    private void OnNoticed()
    {
        _animator.SetTrigger(Constants.PlayerNoticedTrigger);
        _noticeFx.SetActive(true);
        _noticeSound.Play();
    }
}
