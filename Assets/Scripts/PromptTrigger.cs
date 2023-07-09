using UnityEngine;

public class PromptTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private string _promptText;
    private GameObject temp = null;

    private void OnTriggerEnter(Collider other)
    {
        if (temp == null)
        {
            temp = Instantiate(_prefab);
            temp.GetComponent<Prompt>().SetText(_promptText);
        }
    }

    private void OnTriggerExit(Collider other) => Destroy(temp);
}
