using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TerminalButtonProperties : MonoBehaviour
{
    [FormerlySerializedAs("textColors")]
    [SerializeField] private ColorBlock _textColors = ColorBlock.defaultColorBlock;

    public ColorBlock TextColors => _textColors;
}
