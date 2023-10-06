using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharacterController : EasyCharacterMovement.AgentCharacter
{
    public static MainCharacterController Instance { get; private set; }

    [Space]
    [SerializeField] private LayerMask _enemyMask;
    [SerializeField] private float _infectMindRadius = 5;
    [SerializeField] private float _startControlDelay = 1;

    [Space]
    [SerializeField] VisibilityCone _visibilityCone;

    private EnemyController _enemy;
    private bool _isWaitngForConfirm;

    public bool IsInSafeZone { get; set; } = true;

    public EasyCharacterMovement.AgentCharacter ActiveController => _enemy != null ? _enemy : this;

    protected override void OnAwake()
    {
        base.OnAwake();

        Instance = this;
    }

    protected override void Start()
    {
        if (camera == null)
            camera = Camera.main;

        base.Start();
    }

    protected override void CustomRotationMode()
    {
        if (!handleInput) return;

        CustomRotation.Rotate(transform, camera, rotationRate);
    }

    protected override void OnUpdate()
    {
        if (handleInput)
        {
            if (_isWaitngForConfirm)
            {
                return;
            }

            // check if user not clicked on enemy to controll him.  
            // if no let parent class handle input

            if (Input.GetMouseButtonDown(0))
            {
                if (TryGetEnemy(out EnemyController enemy))
                {
                    _isWaitngForConfirm = true;
                    StartCoroutine(WaitForConfirm(enemy));
                    return;
                }
            }

            HandleInput();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                _enemy.StopControl();
                _enemy = null;
                handleInput = true;
                _visibilityCone.enabled = true;
                CameraAttacher.Instance.SetTarget(transform);

            }
        }
    }

    private IEnumerator WaitForConfirm(EnemyController enemy)
    {
        float startTime = Time.time;
        enemy.EnterBeforeControl();

        while (true)
        {
            yield return null;

            if (Input.GetMouseButtonUp(0))
            {
                enemy.ExitBeforeControl();

                bool timePassed = Time.time - startTime >= _startControlDelay; 

                if (timePassed && handleInput && TryGetEnemy(out EnemyController enemy2) && enemy == enemy2 && enemy.CanBeControlled)
                {
                    agent.SetDestination(transform.position); // stop moving
                    StartEnemyControll(enemy);
                }

                _isWaitngForConfirm = false;
                yield break;
            }
        }
    }

    private void StartEnemyControll(EnemyController enemy)
    {
        _visibilityCone.enabled = false;
        handleInput = false;
        _enemy = enemy;
        _enemy.StartControll();
        CameraAttacher.Instance.SetTarget(_enemy.transform);
    }

    private bool TryGetEnemy(out EnemyController enemy)
    {
        Vector2 mousePosition = Input.mousePosition;

        Ray ray = camera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitResult, Mathf.Infinity, _enemyMask))
        {
            if (Vector3.Distance(transform.position, hitResult.point) <= _infectMindRadius
                && hitResult.collider.TryGetComponent(out enemy))
            {
                return true;
            }
        }

        enemy = null;
        return false;
    }

    public void Kill()
    {
        Debug.Log("Played dead");
        handleInput = false;
        agent.SetDestination(transform.position);
    }
}
