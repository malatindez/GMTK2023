using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ButtonText : MonoBehaviour
{
    private Text _text;

    public string Text
    {
        get => _text.text;
        set => _text.text = value;
    }

    private void Awake()
    {
        _text = GetComponent<Text>();
    }
}
