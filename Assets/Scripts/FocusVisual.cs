using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FocusVisual : MonoBehaviour
{
    [SerializeField] private float _speed = 5;

    public static FocusVisual Instance { get; private set; }


    private void Awake()
    {
        Instance = this;
    }

    public void SetTarget(Button btn)
    {
        StopAllCoroutines();
        StartCoroutine(MoveToButton(btn));
    }

    private IEnumerator MoveToButton(Button btn)
    {
        while (true)
        {
            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(pos.y, btn.transform.position.y, _speed * Time.unscaledDeltaTime);

            transform.position = pos;

            yield return null;
        }
    }
}
