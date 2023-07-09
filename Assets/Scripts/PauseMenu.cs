using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject _menu;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_menu.activeSelf)
            {
                _menu.SetActive(false);
                Time.timeScale = 1f;
            }
            else
            {
                _menu.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }

    public void Continue()
    {
        _menu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Options()
    {

    }

    public void Exit()
    {
        Application.Quit();
    }
}
