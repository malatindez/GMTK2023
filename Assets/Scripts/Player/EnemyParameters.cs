using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyParameters : MonoBehaviour
{
    [SerializeField] private float _controlCooldown = 2;
    //[SerializeField] private float _controlTime = 10000;

    public float ControlCooldown => _controlCooldown;
    //public float ControlTime => _controlTime; 

}
