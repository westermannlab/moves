using System.Collections;
using UnityEngine;

public class Pixel : SpriteObject
{
    public GameObject ModelGo;
    
    private Rigidbody2D _rb;
    private BoxCollider2D _boxCollider2D;
    private Bezier _bezier;

    private Vector2 _initialPosition;
    private int _changeGravityId;

    private int _moveOnBezierId;
    private float _moveOnBezierTimer;
    private float _baseScale;

    public event System.Action<Pixel> OnHasReachedTarget;

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody2D>();
        _boxCollider2D = GetComponentInChildren<BoxCollider2D>();
        _boxCollider2D.isTrigger = true;
        _initialPosition = Tf.localPosition;
    }

    public override void Reset()
    {
        base.Reset();
        //SetLayer(0);
        SetSpriteLayer("Default");
        SetBaseScale(1f);
    }

    public void ResetPosition()
    {
        ChangeGravity(0f, 0f);
        Tf.localPosition = _initialPosition;
        Tf.localRotation = Quaternion.identity;
    }

    public void SetBaseScale(float baseScale)
    {
        _baseScale = baseScale;
        SetScale(baseScale);
    }

    public void SetScale(float xScale, float yScale)
    {
        Tf.localScale = new Vector3(xScale, yScale, 1f);
    }

    public void Move(Vector2 delta)
    {
        Tf.position += (Vector3)delta;
    }

    public void MoveLocally(Vector2 delta)
    {
        Tf.localPosition += (Vector3)delta;
    }

    public void SetSpriteLayer(string layerName)
    {
        Sr.sortingLayerName = layerName;
    }
    
    public void SetLayer (int layer)
    {
        ModelGo.layer = layer;
    }
    
    public void AddForce(Vector2 direction, float amount)
    {
        _boxCollider2D.isTrigger = false;
        _boxCollider2D.size = Vector2.one * Random.Range(0.03125f, 0.125f);
        _rb.AddForce(direction.normalized * amount);
    }

    public void ChangeGravity(float targetGravity, float duration)
    {
        _changeGravityId++;
        StartCoroutine(ChangeGravityRoutine(targetGravity, duration, _changeGravityId));
    }

    public void CreateBezier(Vector2 start, Vector2 control, Vector2 end)
    {
        _bezier = new Bezier(start, control, end);
    }

    public void MoveOnBezier(float duration, bool squared = false)
    {
        if (_bezier == null) return;
        _moveOnBezierId++;
        StartCoroutine(MoveOnBezierRoutine(duration, squared, _moveOnBezierId));
    }

    private IEnumerator ChangeGravityRoutine(float targetGravity, float duration, int id)
    {
        var timer = 0f;
        while (timer < duration && _changeGravityId == id)
        {
            timer += Time.deltaTime;
            _rb.gravityScale = Mathf.Lerp(0f, targetGravity, timer / duration);
            yield return null;
        }

        if (_changeGravityId == id)
        {
            _rb.gravityScale = targetGravity;
        }
        
        yield return null;
    }

    private IEnumerator MoveOnBezierRoutine(float duration, bool squared, int id)
    {
        _moveOnBezierTimer = 0f;
        while (_moveOnBezierTimer < duration && _moveOnBezierId == id)
        {
            _moveOnBezierTimer += Time.deltaTime;
            SetPosition(_bezier.GetPointAtTime(squared ? Mathf.Pow(_moveOnBezierTimer / duration, 2f) : _moveOnBezierTimer / duration));
            SetScale(_baseScale + 0.5f * Utility.EaseInOut(_moveOnBezierTimer / duration));
            yield return null;
        }

        if (_moveOnBezierId == id)
        {
            SetPosition(_bezier.GetPointAtTime(1f));
            OnHasReachedTarget?.Invoke(this);
        }
        
        yield return null;
    }
}
