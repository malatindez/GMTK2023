using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private Door _door;
    [SerializeField] private int _doorAccess;
    private void OnTriggerStay(Collider other) { if (Input.GetKey(KeyCode.E) && other.TryGetComponent<AccessLevel>(out AccessLevel accessLevel) && accessLevel.accessLevel >= _doorAccess) _door.DoorOpen();}
}
