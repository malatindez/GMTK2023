using TMPro;
using UnityEngine;

public class Terminal : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI _header;
    [SerializeField] private TextMeshProUGUI _text;

    public void SetText(string text)
    {
        _text.text = text;
    }

    public void SetHeader(string text)
    {
        _header.text = text;
    }
}
