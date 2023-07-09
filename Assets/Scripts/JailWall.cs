using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class JailWall : MonoBehaviour
{
    [SerializeField] private float AutoCloseTime;

    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void OpenWall()
    {
        //gameObject.GetComponent<BoxCollider>().enabled = false;
        _animator.SetTrigger("Open");
    }
}
