using System.Collections;
using UnityEngine;
public class VisibilityCone : MonoBehaviour
{
    #region Fields
    [SerializeField] private GameObject HighlightCenter;
    public int numRaysPerDegree = 30;
    public int maximumAmountOfStepsPerRay = 1024;
    public float viewAngle = 90.0f;
    public float viewDistance = 10.0f;
    public bool enableDebugRays = true;
    public bool enableDebugRaysInGame = true;
    public float highlightRadius = 1.0f;
    private bool firstFrameSkipped = false;
    #endregion Fields

    #region Methods

    private void Start()
    {
        
    }

    private void drawDebugRays()
    {
        for (int i = 0; i < numRaysPerDegree * viewAngle; i++)
        {
            float angle = ((float)i / numRaysPerDegree) - (viewAngle / 2.0f);
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
            Debug.DrawRay(transform.position, dir * viewDistance, Color.red);
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
            {
#endif
            if (!firstFrameSkipped) { firstFrameSkipped = true; return; }
            VisibilityManager.Instance.UpdateVisibilityMask(
                    transform.forward,
                    transform.position,
                    HighlightCenter.transform.position,
                    viewAngle,
                    viewDistance,
                    numRaysPerDegree,
                    maximumAmountOfStepsPerRay,
                    highlightRadius);

#if UNITY_EDITOR
            if (enableDebugRaysInGame)
                {
                    drawDebugRays();
                }
            }
            else if (enableDebugRays)
            {
                drawDebugRays();
            }
#endif
    }

    #endregion Methods
}