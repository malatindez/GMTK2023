using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathScreen : MonoBehaviour
{
    public static DeathScreen Instance { get; private set; }

    [SerializeField] private GameObject _screen;

    private void Awake()
    {
        Instance = this;
    }

    public void Show()
    {
        _screen.SetActive(true);
    }
}
