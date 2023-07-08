using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent (typeof(FieldOfView))]
public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent _agent;
    private FieldOfView _fieldOfView;
    private bool _isPlayerNoticed;
    private GameObject _target;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _fieldOfView = GetComponent<FieldOfView>();
        StartCoroutine(Patrol());
    }

    private IEnumerator Patrol()
    {
        if (_fieldOfView.CanSeeTarget)
        {
            _isPlayerNoticed = true;
            _target = _fieldOfView.Target;
            _agent.nextPosition = _target.transform.position;
            StartCoroutine(Hunt());

            yield break;
        }

        yield return new FixedUpdate();
    }

    private IEnumerator Hunt()
    {
        yield return new FixedUpdate();
    }
}
