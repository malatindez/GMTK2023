using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(TerminalButtonProperties))]
public class TerminalButton : Button
{
    public bool IsInitialized { get; protected set; }

    private ITerminalCommand _command;

    public ITerminalCommand Command
    {
        get => _command;
        set => SetCommand(value);
    }


    protected object CommandParameter { get; set; }

    public RectTransform RectTransform => _rectTransform;

    private RectTransform _rectTransform;

    private TerminalButtonProperties _properties;
    private ButtonText _mainText;

    private Text[] _textes;
    private CanvasGroup[] _groups;

    private SelectionState _lastState = SelectionState.Normal;

    protected override void Awake()
    {
        base.Awake();

        _rectTransform = GetComponent<RectTransform>();
        _properties = GetComponent<TerminalButtonProperties>();
        _mainText = GetComponentInChildren<ButtonText>();
        _textes = GetComponentsInChildren<Text>();
        _groups = new CanvasGroup[_textes.Length];

        for (int i = 0; i < _textes.Length; i++)
        {
            _groups[i] = _textes[i].GetComponent<CanvasGroup>();
        }

        // subscribe to main event
        onClick.AddListener(OnClick);

        // check command
        if (_command != null)
        {
            _mainText.Text = _command.Text;
            enabled = _command.CanExecute(CommandParameter);
        }

        IsInitialized = true;

    }

    private void Update()
    {
        Color selectedColor;
        
        if (_lastState != currentSelectionState)
        {
            switch (currentSelectionState)
            {
                case SelectionState.Normal:
                    selectedColor = _properties.TextColors.normalColor;
                    break;
                case SelectionState.Highlighted:
                    selectedColor = _properties.TextColors.highlightedColor;
                    break;
                case SelectionState.Selected:
                    selectedColor = _properties.TextColors.selectedColor;
                    break;
                case SelectionState.Pressed:
                    selectedColor = _properties.TextColors.pressedColor;
                    break;
                case SelectionState.Disabled:
                    selectedColor = _properties.TextColors.disabledColor;
                    break;

                default:
                    selectedColor = _properties.TextColors.normalColor;
                    break;
            }

            // [TODO]: replace or rework this part
            SetIgnoreParentGroups(currentSelectionState != SelectionState.Normal);

            ChangeColor(selectedColor, false);
            _lastState = currentSelectionState;
        }
    }

    

    private void ChangeColor(Color color, bool instant)
    {
        for (int i = 0; i < _textes.Length; i++)
        {
            _textes[i].CrossFadeColor(color, instant ? 0f : colors.fadeDuration, true, true);
        }
    }

    private void SetIgnoreParentGroups(bool ignoreParentGroups)
    {
        for (int i = 0; i < _groups.Length; i++)
        {
            if (_groups[i] == null) continue;

            _groups[i].ignoreParentGroups = ignoreParentGroups;
        }
    }

    private void OnClick()
    {
        if (Command != null && Command.CanExecute(CommandParameter))
        {
            Command.Execute(CommandParameter);
        }
    }

    private void SetCommand(ITerminalCommand command)
    {
        if (_command != null)
        {
            // unsubscribe from old command events
            _command.TextChanged -= OnCommandTextChanged;
            _command.CanExecuteChanged -= OnCommandCanExecuteChanged;
        }

        // fields
        _command = command;

        // proeprties
        if (IsInitialized)
        {
            _mainText.Text = _command.Text;
            enabled = _command.CanExecute(CommandParameter);
        }

        // events subscribption
        _command.TextChanged += OnCommandTextChanged;
        _command.CanExecuteChanged += OnCommandCanExecuteChanged;
    }

    private void OnCommandCanExecuteChanged(object sender, System.EventArgs e)
    {
        _mainText.Text = _command.Text;
    }

    private void OnCommandTextChanged(object sender, System.EventArgs e)
    {
        enabled = _command.CanExecute(CommandParameter);
    }
}


