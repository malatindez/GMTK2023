using UnityEngine;
using TMPro;

public class TextOverflowResizer : MonoBehaviour
{
    private TextMeshProUGUI _textMeshPro;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _textMeshPro = GetComponent<TextMeshProUGUI>();
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (_textMeshPro.isTextOverflowing)
        {
            float newHeight = _textMeshPro.preferredHeight;
            Vector2 newSize = new Vector2(_rectTransform.sizeDelta.x, newHeight);
            _rectTransform.sizeDelta = newSize;
        }
    }
}