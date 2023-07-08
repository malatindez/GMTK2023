using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent (typeof(FieldOfView))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] private PatrolPoint[] _patrolPoints;
    [SerializeField] private GameObject _noticeFx; 

    private Animator _animator;
    private NavMeshAgent _agent;
    private FieldOfView _fieldOfView;
    private bool _isPlayerNoticed;
    private GameObject _target;

    private Queue<PatrolPoint> _points;
    private PatrolPoint _currentPoint;

    private bool IsWalking
    {
        get => _animator.GetBool(nameof(IsWalking));
        set => _animator.SetBool(nameof(IsWalking), value);
    }

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _fieldOfView = GetComponent<FieldOfView>();
        _points = new Queue<PatrolPoint>(_patrolPoints);
        StartCoroutine(Patrol());
    }

    private IEnumerator Patrol()
    {
        while (true)
        {
            if (_fieldOfView.CanSeeTarget)
            {
                Debug.Log("_fieldOfView.CanSeeTarget");
                OnNoticed();
                _isPlayerNoticed = true;
                _target = _fieldOfView.Target;
                yield return new WaitForSeconds(1f);
                _agent.SetDestination(_target.transform.position);
                StartCoroutine(Hunt());

                yield break;
            }

            if (_currentPoint == null)
            {
                _currentPoint = _points.Dequeue();
                _points.Enqueue(_currentPoint);

                _agent.SetDestination(_currentPoint.transform.position);
                IsWalking = true;
            }

            if (ComaprePoints(transform.position, _currentPoint.transform.position))
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
        if (collision.collider.gameObject.TryGetComponent(out Player player))
        {
            player.TryKill();
        }
    }

    private bool ComaprePoints(Vector3 left, Vector3 right)
    {
        Vector2 a = new Vector2(left.x, left.z);
        Vector2 b = new Vector2(right.x, right.z);

        return Vector2.Distance(a, b) < 0.1f;
    }

    private IEnumerator Hunt()
    {
        while (true)
        {
            Debug.Log("TODO: ADD LOGIC");

            yield return new WaitForFixedUpdate();
        }
    }

    private void OnNoticed()
    {
        _animator.SetTrigger("PlayerNoticed");
        _noticeFx.SetActive(true);

    }
}
