using UnityEngine;

public class VisibilityCone : MonoBehaviour
{
    #region Fields

    public Transform MapFloor = null;
    public int maskResolution = 512;
    public int numRaysPerDegree = 1;
    public float viewAngle = 90.0f;
    public float viewDistance = 10.0f;
    public Texture2D visibilityMask;

    #endregion Fields

    #region Methods

    private Vector2 CalculateMaskUV(Vector3 worldPosition)
    {
        Vector3 localPosition = MapFloor.InverseTransformPoint(worldPosition);
        Vector2 maskUV = new(localPosition.x / MapFloor.localScale.x, localPosition.z / MapFloor.localScale.z);
        maskUV += Vector2.one * 0.5f;
        return maskUV;
    }

    private void Start()
    {
        visibilityMask = new Texture2D(maskResolution, maskResolution, TextureFormat.R8, false);
    }

    private void Update()
    {
        // i know this is a dumbass way, but gotta go fast
        // prolly would be better to move this stuff to compute shader, but whatever

        Vector3 playerPosition = transform.position;
        Vector3 viewDirection = transform.forward;

        float halfViewAngle = viewAngle / 2.0f;
        int numRays = Mathf.CeilToInt(viewAngle * numRaysPerDegree);

        Color[] blackPixels = new Color[maskResolution * maskResolution];
        visibilityMask.SetPixels(blackPixels);
        for (int i = 0; i < numRays; ++i)
        {
            float angle = -halfViewAngle + (i / (float)(numRays - 1) * viewAngle);
            Vector3 direction = Quaternion.Euler(0.0f, angle, 0.0f) * viewDirection;

            Ray ray = new(playerPosition, direction);

            if (Physics.Raycast(ray, out RaycastHit hit, viewDistance))
            {
                Vector2 maskUV = CalculateMaskUV(hit.point);
                visibilityMask.SetPixel(Mathf.RoundToInt(maskUV.x * maskResolution), Mathf.RoundToInt(maskUV.y * maskResolution), Color.white);
            }
        }
        visibilityMask.Apply();
    }

    #endregion Methods
}