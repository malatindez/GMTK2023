using UnityEngine;

public class VisibilityCone : MonoBehaviour
{
    #region Fields
    [SerializeField] private VisibilityManager visibilityManager;
    public int numRaysPerDegree = 30;
    public int numSteps = 1024;
    public float viewAngle = 90.0f;
    public float viewDistance = 10.0f;

    #endregion Fields

    #region Methods


    private void Update()
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
    }

    #endregion Methods
}