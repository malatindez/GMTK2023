using UnityEngine;

public class TerminalTrigger : MonoBehaviour
{
    [SerializeField] private int _terminalAccess;
    [SerializeField] private Door _door;
    [SerializeField] private GameObject _prefab;
    [SerializeField] private string _header;
    [SerializeField] private string _text;
    private GameObject temp = null;
    private void OnTriggerStay(Collider other)
    {
        if (Input.GetKey(KeyCode.E) && other.TryGetComponent<AccessLevel>(out AccessLevel accessLevel) && accessLevel.accessLevel >= _terminalAccess)
        {
            if (temp == null)
            {
                temp = Instantiate(_prefab);
                temp.GetComponentInChildren<ButtonOpenDoor>().addDoor(_door);
                temp.GetComponent<Terminal>().SetHeader(_header);
                temp.GetComponent<Terminal>().SetText(_text);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (temp != null)
        {
            Destroy(temp);
        }
    }
}
