using UnityEngine;
using UnityEngine.UI;

public class ButtonClosePopup : MonoBehaviour
{
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(Remove);
    }

    private void Remove()
    {
        Destroy(transform.parent.gameObject);
    }

}
