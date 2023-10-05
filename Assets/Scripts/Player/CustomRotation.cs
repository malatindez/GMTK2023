using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class CustomRotation
{
    public static void Rotate(Transform transform, Camera camera, float rotationRate)
    {
        //Get the Screen positions of the object
        Vector2 positionOnScreen = camera.WorldToViewportPoint(transform.position);

        //Get the Screen position of the mouse
        Vector2 mouseOnScreen = (Vector2)camera.ScreenToViewportPoint(Input.mousePosition);

        //Get the angle between the points
        float angle = AngleBetweenTwoPoints(positionOnScreen, mouseOnScreen);

        //black magic from jorge floid
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(new Vector3(0f, camera.transform.eulerAngles.y - angle - 90, 0f)),
            Time.deltaTime * rotationRate);
    }

    public static float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }
}
