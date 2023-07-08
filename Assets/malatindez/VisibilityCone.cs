using UnityEngine;
[ExecuteInEditMode]
public class VisibilityCone : MonoBehaviour
{
    #region Fields
    [SerializeField] private VisibilityManager visibilityManager;
    public int numRaysPerDegree = 30;
    public int numSteps = 1024;
    public float viewAngle = 90.0f;
    public float viewDistance = 10.0f;
    public bool enableDebugRays = true;
    public bool enableDebugRaysInGame = true;
    #endregion Fields

    #region Methods


    private void Update()
    {
        // if not editor
        if (Application.isPlaying)
        {
            visibilityManager.UpdateVisibilityMask(
                               transform.forward,
                                              transform.position,
                                                             viewAngle,
                                                                            viewDistance,
                                                                                           numRaysPerDegree,
                                                                                                          numSteps,
                                                                                                                         true
                                                                                                                                        );
            if (enableDebugRaysInGame)
            {
                drawDebugRays();
            }
        }
        else if (enableDebugRays)
        {
            drawDebugRays();
        }
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

    #endregion Methods
}