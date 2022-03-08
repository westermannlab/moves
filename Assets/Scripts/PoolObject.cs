using System.Collections;
using UnityEngine;

public class PoolObject : MonoBehaviour
{
    protected Transform Tf;
    protected GameObject Go;
    
    protected virtual void Awake()
    {
        Tf = transform;
        Go = gameObject;
    }

    public virtual void Init()
    {
        
    }

    public virtual void Reset()
    {
       Go.SetActive(false); 
    }

    public virtual void Return(float delay = 0f)
    {
        if (delay > 0f)
        {
            StartCoroutine(ReturnRoutine(delay));
            return;
        }
        SetParent(References.PoolTf);
        References.Pool.Return(this);
    }

    public virtual void SetParent(Transform parent)
    {
        Tf.parent = parent;
    }

    public virtual void SetPosition(Vector2 position)
    {
        Tf.position = position;
    }

    public void SetLocalPosition(float x, float y)
    {
        SetLocalPosition(new Vector2(x, y));
    }

    public virtual void SetLocalPosition(Vector2 localPosition)
    {
        Tf.localPosition = localPosition;
    }
    
    public virtual void SetLocalPosition(Vector3 localPosition)
    {
        Tf.localPosition = localPosition;
    }

    public void SetLocalRotation(Vector3 eulerRotation)
    {
        Tf.localRotation = Quaternion.Euler(eulerRotation);
    }

    protected virtual IEnumerator ReturnRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Return();
    }
}

[System.Serializable]
public class PoolObjectData
{
    public PoolObject Prefab;
    public int InitialAmount;
    public int MaxAmount;
    private int _id;

    public void CreateInitialPrefabs()
    {
        if (InitialAmount > 0)
        {
            References.Pool.Create(this, InitialAmount);
        }
    }

    public int Id
    {
        get => _id;
        set => _id = value;
    }
}
