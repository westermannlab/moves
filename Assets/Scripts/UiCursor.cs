using UnityEngine;

public class UiCursor : SpriteObject
{
    public enum Shape { None, AssessmentLevel, CloseButton }
    public Sprite[] Shapes;

    public void UpdateShape(Shape shape)
    {
        if ((int) shape >= Shapes.Length) return;
        Sr.sprite = Shapes[(int) shape];
    }

    public override void SetParent(Transform parent)
    {
        base.SetParent(parent);
        Tf.localScale = Vector3.one;
    }
}
