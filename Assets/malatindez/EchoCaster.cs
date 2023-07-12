using NaughtyAttributes;
using System.Collections;
using UnityEngine;
public class EchoCaster : MonoBehaviour
{
    
    #region Fields
    [SerializeField] private float _maximumTimeAlive = 3.0f;
    [SerializeField] private float _initialIntensity = 5.0f;
    [SerializeField] private float _speed = 0.5f;
    [SerializeField] private bool _omnidirectional = true;
    [SerializeField] private bool _activateOnTimer = true;
    [SerializeField, ShowIf("_activateOnTimer")] private float _activationTime = 3.0f;
    [SerializeField] public bool _enableDebugRays = true;
    [SerializeField] public bool _enableDebugRaysInGame = true;
    private bool _firstFrameSkipped = false;
    private float _lastCast = float.MinValue;
    #endregion Fields
    #region Methods

    private void Start()
    {

    }

    private void drawDebugRays()
    {
        for (int i = 0; i < 360; i++)
        {
            Vector3 dir = Quaternion.AngleAxis(i, Vector3.up) * transform.forward;
            Debug.DrawRay(new Vector3(transform.position.x, VisibilityManager.Instance.EchoHeight, transform.position.z), dir * 1, Color.red);
        }
    }
    public void CastEcho()
    {
        if (_omnidirectional)
        {
            VisibilityManager.Instance.AddEchoPoint(
                transform.position,
                _maximumTimeAlive,
                _initialIntensity,
                _speed
            );
        }
        else
        {
            VisibilityManager.Instance.AddEchoPoint(
                transform.position,
                _maximumTimeAlive,
                _initialIntensity,
                _speed,
                transform.forward
            );
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            if (_enableDebugRaysInGame)
            {
                drawDebugRays();
            }
#endif
            if (!_firstFrameSkipped) { _firstFrameSkipped = true; return; }
            if (!_activateOnTimer || _lastCast > Time.time - _activationTime) return;
            CastEcho();
            _lastCast = Time.time;

#if UNITY_EDITOR
        }
        else if (_enableDebugRays)
        {
            drawDebugRays();
        }
#endif
    }

    #endregion Methods
}