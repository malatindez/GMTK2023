using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaTerminal : MonoBehaviour
{
    [SerializeField] private Text _title;
    [SerializeField] private Text _description;
    [SerializeField] private TerminalButton _buttonTemplate;

    [Space]
    [SerializeField] private float _initSpace = 5;
    [SerializeField] private float _spaceBeetwenButtons = 2;


    private readonly List<TerminalButton> _cachedButtons = new List<TerminalButton>();

    private void Start()
    {
        _buttonTemplate.gameObject.SetActive(false);
    }

    public void ShowTerminal(string title, string description, IEnumerable<ITerminalCommand> commands)
    {
        _title.text = title;
        _description.text = description;

        Debug.Assert(_description.preferredHeight != 0);

        int i = 0;
        float initY = _description.preferredHeight + _initSpace;
        foreach (ITerminalCommand cmd in commands)
        {
            TerminalButton button;

            if (_cachedButtons.Count > i)
                button = _cachedButtons[i];
            else
                button = Instantiate(_buttonTemplate.gameObject, _buttonTemplate.transform.parent).GetComponent<TerminalButton>();

            button.gameObject.SetActive(true);
            button.Command = cmd;
            button.RectTransform.SetInsetAndSizeFromParentEdge(
                RectTransform.Edge.Top, 
                initY + button.RectTransform.rect.height * i + _spaceBeetwenButtons * i,
                button.RectTransform.rect.height);

            i++;
        }

        // hide unused buttons
        if (_cachedButtons.Count > i)
        {
            for (; i < _cachedButtons.Count; i++)
            {
                _cachedButtons[i].gameObject.SetActive(false);
            }
        }
    }

    [ContextMenu("Show Test Message")]
    public void ShowTestMessage()
    {
        ShowTerminal("Test Message", "Here text with 2 lines and 3 buttons!\nEND OF LINE", new ITerminalCommand[]
        {
            new TerminalCommand(() => Debug.Log("Button 1 pressed")) { Text = "Open Door 1" } ,
            new TerminalCommand(() => Debug.Log("Button 2 pressed")) { Text = "Open Door 2" } ,
            new TerminalCommand(() => Debug.Log("Button 3 pressed")) { Text = "Open Door 3" } ,
        });
    }

}
