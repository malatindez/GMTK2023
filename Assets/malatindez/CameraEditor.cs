using NaughtyAttributes;
using UnityEngine;

public class CameraAspectRatio : MonoBehaviour
{
    #region Fields

    // Set the desired aspect ratio
    [SerializeField] private float aspectRatioX = 16f;

    [SerializeField] private float aspectRatioY = 9f;

    #endregion Fields

    // For example, for a 16:9 aspect ratio

    #region Methods

    [Button("Update Camera")]
    private void UpdateCamera()
    {
        Camera camera = GetComponent<Camera>();

        float windowAspect = Screen.width / (float)Screen.height;

        float scaleHeight = windowAspect / aspectRatioX * aspectRatioY;

        // If scaled height is less than current height, add letterbox
        if (scaleHeight < 1.0f)
        {
            Rect rect = camera.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            camera.rect = rect;
        }
        else // If scaled height is greater than current height, add pillarbox
        {
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = camera.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            camera.rect = rect;
        }
    }

    #endregion Methods
}