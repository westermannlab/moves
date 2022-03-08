using System.Collections.Generic;
using UnityEngine;

public class VehicleTrigger : MonoBehaviour
{
    public event System.Action OnAgentEnter;
    public event System.Action OnAgentExit;
    private Collider2D _trigger;
    
    private readonly List<Collider2D> _collidersInsideTrigger = new List<Collider2D>();

    private void Awake()
    {
        _trigger = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_collidersInsideTrigger.Contains(collision))
        {
            return;
        }
        _collidersInsideTrigger.Add(collision);
        OnAgentEnter?.Invoke();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!_collidersInsideTrigger.Contains(collision))
        {
            return;
        }

        _collidersInsideTrigger.Remove(collision);

        if (_collidersInsideTrigger.Count == 0)
        {
            OnAgentExit?.Invoke();
        }
    }
    
    public void SetActive(bool isActive)
    {
        if (_trigger != null)
        {
            _trigger.enabled = isActive;
        }   
    }
}

