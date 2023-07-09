using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private int _priority;

    private void Awake()
    {
        if (CheckPointManager.ActiveCheckpoint == _priority)
            CheckPointManager.RequestSpawn(transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (CheckPointManager.ActiveCheckpoint < _priority)
            CheckPointManager.ActiveCheckpoint = _priority; 
    }
}
