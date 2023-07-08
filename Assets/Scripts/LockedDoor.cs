using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    private string _password;

    public string Password => _password;

    public bool IsOpened { get; private set; }

    public bool IsPermanentLocked { get; private set; }

    private byte _tries = 3;

    private void Start()
    {
        _password = GeneratePassword();
    }

    private static string GeneratePassword()
    {
        return Random.Range(1000, 1_0000).ToString();
    }

    public bool TryOpen(string password)
    {
        if (IsPermanentLocked) return false;

        if (password != _password)
        {
            _tries--;

            if (_tries == 0)
            {
                IsPermanentLocked = true;
            }

            return false;
        }

        _tries = 3;
        IsOpened = true;

        return true;
    }
}
