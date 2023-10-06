using EasyCharacterMovement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyParameters))]
[RequireComponent(typeof(FieldOfView))]
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
    private FieldOfView _fieldOfView;

    public bool CanBeControlled { get; private set; } = true;

    public bool IsUnderControl { get; private set; }

    public bool IsPatrolling { get; private set; }

    protected override void OnAwake()
    {
        base.OnAwake();

        _parameters = GetComponent<EnemyParameters>();
        _fieldOfView = GetComponent<FieldOfView>();
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

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        if (IsPatrolling)
        {
            if (_fieldOfView.CanSeeTarget && _fieldOfView.Target.TryGetComponent(out MainCharacterController player))
            {
                if (player.IsInSafeZone) return;

                StopPatrol();
                StartCoroutine(OnNoticed(player));
            }
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

    protected bool CanReachTarget(Vector3 target)
    {
        NavMeshPath navMeshPath = new NavMeshPath();
        //create path and check if it can be done
        // and check if navMeshAgent can reach its target
        if (agent.CalculatePath(target, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
        {
            //agent.SetPath(navMeshPath);

            return true;
        }
        else
        {
            return false;
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
        StopPatrol();
        agent.SetDestination(transform.position); // stop moving

        _visibilityCone.enabled = true;
        handleInput = true;

        IsUnderControl = true;

        //_controlStartSound?.Play();
    }

    public void StopControl()
    {
        agent.SetDestination(transform.position); // stop moving

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
        IsPatrolling = true;

        while (true)
        {
            _isPointReached = false;
            PatrolPoint point = GetNextPoint();
            MoveToLocation(point.transform.position);

            while (!_isPointReached)
                yield return CoroutineStaticInstructions.WaitForFixedUpdate;

            this.characterMovement.velocity = Vector3.zero;
            if (point.Delay != 0)
                yield return new WaitForSeconds(point.Delay);
        }
    }

    private void StopPatrol()
    {
        IsPatrolling = false;
        StopCoroutine(_patrolCoroutine);
    }

    private IEnumerator OnNoticed(MainCharacterController player)
    {
        _playerNoticedSound.Play();
        
        while (true)
        {
            agent.SetDestination(player.transform.position);

            yield return CoroutineStaticInstructions.WaitForFixedUpdate;

            if (IsUnderControl)
            {
                yield break;
            }

            if (Vector3.Distance(transform.position, player.transform.position) <= 1)
            {
                player.Kill();
                yield break;
            }
        }
    }

    private PatrolPoint GetNextPoint()
    {
        PatrolPoint result = _patrolPointsQueue.Dequeue();
        _patrolPointsQueue.Enqueue(result);

        return result;
    }
}
