using System.Collections;
using UnityEngine;

public class RainTrigger : MonoBehaviour
{
    private PickUp _parentNote;
    private Collider2D _trigger;
    private Sparkle _sparkle;
    private float _lastSparkleTime;

    private float _sparkleTimer;
    private const float MaxSparkleTime = 8f;
    private int _sparkleId;
    private bool _isSparkling;
    
    private void Awake()
    {
        _parentNote = GetComponentInParent<PickUp>();
        _trigger = GetComponent<Collider2D>();
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_parentNote == null || _parentNote.ColorId == -1 || References.Actions.Rain.RainMode == Rain.Mode.Heavy) return;
        
        References.Events.RainHitsNote(References.Entities.ColorIdToPlayerId(_parentNote.ColorId));
        
        _sparkleTimer = Mathf.Min(_sparkleTimer + 0.25f, MaxSparkleTime);
        
        if (!_isSparkling)
        {
            _sparkleId = Utility.AddOne(_sparkleId);
            StartCoroutine(SparkleRoutine(_sparkleId));
        }
    }

    public void SetActive(bool isActive)
    {
        if (_trigger != null)
        {
            _trigger.enabled = isActive;
        }   
    }

    private IEnumerator SparkleRoutine(int id)
    {
        _isSparkling = true;

        if (_parentNote != null)
        {
            _parentNote.FlagAsSparkling(_isSparkling);
        }
        
        while (_sparkleTimer > 0f && _parentNote != null && !_parentNote.HasBeenPickedUp && _sparkleId == id)
        {
            _sparkleTimer -= Time.deltaTime;
            if (Time.time > _lastSparkleTime + 0.25f)
            {
                _sparkle = References.Prefabs.GetSparkle();
                if (_sparkle != null)
                {
                    _sparkle.SetColor(References.Entities.ColorIdToSoftColor(_parentNote.ColorId));
                    _sparkle.Play(_parentNote.Position + Random.insideUnitCircle / 2f);
                }

                _lastSparkleTime = Time.time;
            }
            yield return null;
        }

        if (_sparkleId == id)
        {
            _isSparkling = false;
            
            if (_parentNote != null)
            {
                _parentNote.FlagAsSparkling(_isSparkling);
            }
        }
        
        yield return null;
    }
}
