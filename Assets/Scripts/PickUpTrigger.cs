using UnityEngine;

public class PickUpTrigger : MonoBehaviour
{
    public event System.Action<Player> OnCollect;
    private Vehicle _vehicle;
    private Player _player;
    private Collider2D _trigger;

    private void Awake()
    {
        _trigger = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _player = null;
        _vehicle = collision.GetComponentInParent<Vehicle>();
        if (_vehicle != null)
        {
            _player = _vehicle.CurrentPlayer;
            if (_player == null && _vehicle.GetOtherVehicle() != null)
            {
                _player = _vehicle.GetOtherVehicle().CurrentPlayer;
            }
        }

        if (_player != null)
        {
            OnCollect?.Invoke(_player);   
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
