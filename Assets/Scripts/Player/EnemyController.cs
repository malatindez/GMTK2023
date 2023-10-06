using EasyCharacterMovement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyParameters))]
public class EnemyController : AgentCharacter
{
    [Space]
    [SerializeField] VisibilityCone _visibilityCone;
    [SerializeField] GameObject _beforControlFx;

    [Space]
    [SerializeField] AudioSource _beforeControlSound;
    [SerializeField] AudioSource _controlStartSound;
    [SerializeField] AudioSource _playerNoticedSound;

    [Space]
    [SerializeField] private PatrolPoint[] _patrolPoints;

    private Queue<PatrolPoint> _patrolPointsQueue;

    private bool _isPointReached;

    private Coroutine _patrolCoroutine;

    private EnemyParameters _parameters;

    public bool CanBeControlled { get; private set; } = true;

    public bool IsUnderControl { get; private set; }

    protected override void OnAwake()
    {
        base.OnAwake();

        _parameters = GetComponent<EnemyParameters>();
        _patrolPointsQueue = new Queue<PatrolPoint>(_patrolPoints);
    }

    protected override void OnStart()
    {
        base.OnStart();

        if (camera == null)
        {
            camera = Camera.main;
        }

        if (_patrolPoints != null && _patrolPoints.Length > 0)
        {
            _patrolCoroutine = StartCoroutine(StartPatrol());
        }
    }

    protected override void SyncNavMeshAgent()
    {
        //agent.angularSpeed = rotationRate;

        agent.speed = GetMaxSpeed();
        agent.acceleration = GetMaxAcceleration();

        agent.velocity = GetVelocity();

        agent.nextPosition = GetPosition();
    }

    protected override void CustomRotationMode()
    {
        if (!handleInput)
        {
            characterMovement.RotateTowards(MovementDirection, rotationRate * rotationRate * deltaTime, true);
        }
        else
        {
            CustomRotation.Rotate(transform, camera, rotationRate);
        }
    }



    public override void OnArrive()
    {
        base.OnArrive();

        _isPointReached = true;
    }

    public void EnterBeforeControl()
    {
        _beforControlFx.SetActive(true);
        _beforeControlSound?.Play(); 
    }

    public void ExitBeforeControl()
    {
        _beforControlFx.SetActive(false);
    }

    public void StartControll()
    {
        StopCoroutine(_patrolCoroutine);
        agent.SetDestination(transform.position); // stop moving
        _visibilityCone.enabled = true;
        handleInput = true;

        IsUnderControl = true;

        //_controlStartSound?.Play();
    }

    public void StopControl()
    {
        _visibilityCone.enabled = false;
        handleInput = false;

        IsUnderControl = false;

        StartCoroutine(WaitBeforePatrol(_parameters.ControlCooldown));
    }

    private IEnumerator WaitBeforePatrol(float cooldown)
    {
        CanBeControlled = false;
        yield return new WaitForSeconds(cooldown);
        CanBeControlled = true;

        if (handleInput) yield break;

        _patrolCoroutine = StartCoroutine(StartPatrol());
    }

    private IEnumerator StartPatrol()
    {
        while (true)
        {
            _isPointReached = false;
            PatrolPoint point = GetNextPoint();
            MoveToLocation(point.transform.position);

            while (!_isPointReached)
                yield return new WaitForFixedUpdate();

            this.characterMovement.velocity = Vector3.zero;
            if (point.Delay != 0)
                yield return new WaitForSeconds(point.Delay);
        }
    }

    private PatrolPoint GetNextPoint()
    {
        PatrolPoint result = _patrolPointsQueue.Dequeue();
        _patrolPointsQueue.Enqueue(result);

        return result;
    }
}
