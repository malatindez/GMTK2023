using System.Collections.Generic;
using UnityEngine;

public class TerminalTrigger : MonoBehaviour
{
    [SerializeField] private int _terminalAccess;
    [SerializeField] private List<Door> _doors;
    [SerializeField] private GameObject _prefab;
    [SerializeField] private string _header;
    [SerializeField] private string _text;
    private GameObject temp = null;
    private void OnTriggerStay(Collider other)
    {
        //if (Input.GetKey(KeyCode.E) && other.TryGetComponent<AccessLevel>(out AccessLevel accessLevel) && accessLevel.accessLevel >= _terminalAccess)
        //{
        //    if (temp == null)
        //    {
        //        temp = Instantiate(_prefab);
        //        foreach (Door door in _doors) temp.GetComponentInChildren<ButtonOpenDoor>().addDoor(door);
        //        foreach (JailWall wall in _walls) temp.GetComponentInChildren<ButtonOpenJailWall>().addJailWall(wall);
        //        temp.GetComponent<Terminal>().SetHeader(_header);
        //        temp.GetComponent<Terminal>().SetText(_text);
        //    }
        //}
    }
    private void OnTriggerExit(Collider other)
    {
        if (temp != null)
        {
            Destroy(temp);
        }
    }
}
