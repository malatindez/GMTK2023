using TMPro;
using UnityEngine;

public class Prompt : MonoBehaviour
{
    private TextMeshProUGUI _text;

    private void Awake() => _text = transform.GetChild(1).GetComponent<TextMeshProUGUI>();

    public void SetText(string text) => _text.text = text;
}
