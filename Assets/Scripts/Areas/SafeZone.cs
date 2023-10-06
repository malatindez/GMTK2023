using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SafeZone : MonoBehaviour
{
    [Tooltip("The doors that must be closed to stay in safe zone.")]
    [SerializeField] private Door[] _doors;

    private Func<bool> _getDoorClosed;

    private void Awake()
    {
        if (_doors == null || _doors.Length == 0)
        {
            _getDoorClosed = () => true;
        }
        else
        {
            _getDoorClosed = () => _doors.All(t => !t.IsOpen);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out MainCharacterController controller))
        {
            controller.IsInSafeZone = _getDoorClosed();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out MainCharacterController controller))
        {
            controller.IsInSafeZone = false;
        }
    }

}
