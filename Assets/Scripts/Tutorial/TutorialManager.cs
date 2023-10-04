using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class TutorialManager : MonoBehaviour
{
    #region Singleton

    private static TutorialManager _instance;
    public static TutorialManager Instance => _instance;

    #endregion


    [SerializeField] private Text _textTemplate;

    private Canvas _canvas;

    private readonly List<TutorialText> _cache = new List<TutorialText>();

    private void Awake()
    {
        _instance = this;
        _canvas = GetComponent<Canvas>();
        _textTemplate.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        var renderSize = _canvas.renderingDisplaySize;

        foreach (var tutorial in _cache)
        {
            if (!tutorial.isActiveAndEnabled)
            {
                continue;   
            }

            tutorial.Text.fontSize = Mathf.RoundToInt(renderSize.x * tutorial.Size);

            Vector3 screenPos = Camera.main.WorldToScreenPoint(tutorial.transform.position);
            screenPos.y += (renderSize.y * tutorial.VerticalOffset);
            //screenPos.x -= (tutorial.Text.preferredWidth / 2);
            screenPos.x +=  (renderSize.x * tutorial.HorizontalOffset);
            tutorial.TextTransform.sizeDelta = new Vector2(tutorial.Text.preferredWidth, tutorial.Text.preferredHeight);
            
            tutorial.Text.transform.position = screenPos;
        }
    }

    public Text Register(TutorialText gameObject)
    {
        Text text = Instantiate(_textTemplate, _textTemplate.transform.parent);
        text.gameObject.SetActive(true);

        _cache.Add(gameObject);

        return text;
    }
}
