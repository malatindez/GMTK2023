using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialText : MonoBehaviour
{
    [SerializeField] private string _initialText;
    [SerializeField] private float _verticalOffset;
    [SerializeField] private float _horizontalOffset;
    [SerializeField] private float _size = .024f;

    private Text _text;
    private RectTransform _textTransform;

    public Text Text => _text;

    public RectTransform TextTransform => _textTransform;

    public float VerticalOffset => _verticalOffset;

    public float HorizontalOffset => _horizontalOffset;

    public float Size => _size;

    private void Start()
    {
        _text = TutorialManager.Instance.Register(this);
        _textTransform = _text.GetComponent<RectTransform>();

        if (!string.IsNullOrEmpty(_initialText))
        {
            _text.text = _initialText;
        }
    }

    private void OnEnable()
    {
        if (_text) _text.enabled = true;
    }

    private void OnDisable()
    {
        if (_text) _text.enabled = false;
    }
}
