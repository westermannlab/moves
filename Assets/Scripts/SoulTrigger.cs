using UnityEngine;

public class SoulTrigger : MonoBehaviour
{
    private ControllableObject _controllableObject;
    public ControllableObject ControllableObject => _controllableObject;
    private Collider2D _trigger;

    private void Awake()
    {
        _controllableObject = GetComponentInParent<ControllableObject>();
        _trigger = GetComponent<Collider2D>();
    }

    public void SetActive(bool isActive)
    {
        if (_trigger != null)
        {
            _trigger.enabled = isActive;
        }   
    }
}
