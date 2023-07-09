using System.Collections;
using UnityEngine;

public class JailWall : MonoBehaviour
{
    [SerializeField] private float AutoCloseTime;

    public void OpenWall()
    {
        gameObject.GetComponent<BoxCollider>().enabled = false;
    }
}
