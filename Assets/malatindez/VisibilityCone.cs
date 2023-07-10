using System.Collections;
using UnityEngine;
[ExecuteInEditMode]
public class VisibilityCone : MonoBehaviour
{
    #region Fields
    [SerializeField] private VisibilityManager visibilityManager;
    [SerializeField] private GameObject HighlightCenter;
    public int numRaysPerDegree = 30;
    public int maximumAmountOfStepsPerRay = 1024;
    public float viewAngle = 90.0f;
    public float viewDistance = 10.0f;
    public bool enableDebugRays = true;
    public bool enableDebugRaysInGame = true;
    public float highlightRadius = 1.0f;

    #endregion Fields

    #region Methods

    private void Start()
    {
        _ = StartCoroutine(SkipFirst());
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

    private IEnumerator SkipFirst()
    {
        yield return null; // skip first update

        while (true)
        {
            if (Application.isPlaying)
            {
                visibilityManager.UpdateVisibilityMask(
                    transform.forward,
                    transform.position,
                    HighlightCenter.transform.position,
                    viewAngle,
                    viewDistance,
                    numRaysPerDegree,
                    maximumAmountOfStepsPerRay,
                    highlightRadius);

                if (enableDebugRaysInGame)
                {
                    drawDebugRays();
                }
            }
            else if (enableDebugRays)
            {
                drawDebugRays();
            }

            yield return null;
        }
    }

    #endregion Methods
}