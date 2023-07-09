using UnityEngine;
using UnityEngine.UI;

public class ButtonOpenDoor : MonoBehaviour
{
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void addDoor(Door door)
    {
        _button.onClick.AddListener(door.DoorOpen);
    }

}
