using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerV2 : MonoBehaviour
{
    [SerializeField] private float _controlRange = 6;
    [SerializeField] private float _moveSpeed = 6;
    [SerializeField] private LayerMask _clickTargets;

    private Rigidbody _rigidbody;
    private Animator _animator;
    private Camera _camera;

    private bool IsWalking
    {
        get => _animator.GetBool(nameof(IsWalking));
        set => _animator.SetBool(nameof(IsWalking), value);
    }

    public bool IsAlive { get; private set; }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _camera = Camera.main;

        StartCoroutine(CharacterLoop());
    }

    public void EndTelepaty() 
    {
        StartCoroutine(CharacterLoop());
        CameraAttacher.Instance.SetTarget(transform);
    }

    public void TryKill()
    {
        StopAllCoroutines();
        _rigidbody.velocity = Vector3.zero;
        _animator.SetTrigger(Constants.PlayerDiedTrigger);
        StartCoroutine(Kill());
    }

    private IEnumerator Kill()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        DeathScreen.Instance.Show();
        yield return new WaitForSecondsRealtime(2f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator CharacterLoop()
    {
        while (true)
        {
            Move();
            RotateToMouse();
            CheckControl();

            yield return null;
        }
    }

    private void CheckControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100, _clickTargets))
            {
                Debug.Log("Physics.Raycast");
                Collider[] rangeChecks = Physics.OverlapSphere(transform.position, _controlRange, _clickTargets);

                if (rangeChecks.Any(t => t.gameObject == hit.collider.gameObject))
                {
                    Debug.Log("rangeChecks.Any");

                    if (hit.collider.TryGetComponent(out EnemyAI enemyAI))
                    {
                        StopAllCoroutines();
                        enemyAI.StartTelepaty(this);
                        CameraAttacher.Instance.SetTarget(enemyAI.transform);
                    }
                }

            }
        }
    }

    private void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        //camera forward and right vectors:
        var forward = _camera.transform.forward;
        var right = _camera.transform.right;

        //project forward and right vectors on the horizontal plane (y = 0)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        //this is the direction in the world space we want to move:
        var desiredMoveDirection = forward * vertical + right * horizontal;

        _rigidbody.velocity = desiredMoveDirection * _moveSpeed;

        IsWalking = _rigidbody.velocity != Vector3.zero;
    }

    private void RotateToMouse()
    {
        //Get the Screen positions of the object
        Vector2 positionOnScreen = _camera.WorldToViewportPoint(transform.position);

        //Get the Screen position of the mouse
        Vector2 mouseOnScreen = (Vector2)_camera.ScreenToViewportPoint(Input.mousePosition);

        //Get the angle between the points
        float angle = AngleBetweenTwoPoints(positionOnScreen, mouseOnScreen);

        //Ta Daaa
        transform.rotation = Quaternion.Euler(new Vector3(0f, -angle + 270f, 0f));

    }

    private float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }
}
