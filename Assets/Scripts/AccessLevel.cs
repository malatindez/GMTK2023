using UnityEngine;

public class AccessLevel : MonoBehaviour
{
    [SerializeField] private int _accessLevel;

    public int accessLevel { get { return _accessLevel; } }
}
