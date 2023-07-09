using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonFocusManager : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private Text _text;
    [SerializeField] private bool _isDefault;

    private RectTransform _rectTransform;
    private Button _button;

    public void OnPointerEnter(PointerEventData eventData)
    {
        FocusVisual.Instance.SetTarget(_button);
    }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _rectTransform = GetComponent<RectTransform>();

        if (_text == null)
            _text = GetComponentInChildren<Text>();
    }

    private void OnEnable()
    {
        if (_isDefault && FocusVisual.Instance != null)
        {
            FocusVisual.Instance.SetTarget(_button);
        }
    }
}
