using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccessCard : MonoBehaviour
{
    [SerializeField] private AccessLevel _accessLevel;

    public AccessLevel AccessLevel => _accessLevel; 
}
