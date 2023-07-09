using UnityEngine;
using UnityEngine.UI;

public class ButtonOpenJailWall : MonoBehaviour
{
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void addJailWall(JailWall jailWall)
    {
        _button.onClick.AddListener(jailWall.OpenWall);
    }
}
