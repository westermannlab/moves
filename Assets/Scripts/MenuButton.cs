using System.Collections;
using UnityEngine;

public class MenuButton : UiElement
{
    public enum Type { Open, Close, Keys }

    public Type ButtonType;
    public SoundEffect ClickSound;

    private Menu _menu;
    private BoxCollider2D _boxCollider2D;

    protected override void Awake()
    {
        base.Awake();
        _menu = GetComponentInParent<Menu>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        References.Events.OnChangeMenuState += ChangeButtonState;
    }

    private void OnDisable()
    {
        References.Events.OnChangeMenuState -= ChangeButtonState;
    }

    private void OnMouseDown()
    {
        Controllers.Audio.PlaySoundEffect(ClickSound);
        switch (ButtonType)
        {
            case Type.Open:
                _menu.Open(0.125f);
                break;
            case Type.Close:
                _menu.Close(0.125f);
                break;
            case Type.Keys:
                Controllers.Input.KeyBindings =
                    (InputController.KeyBindingType) Mathf.Abs((int) Controllers.Input.KeyBindings - 1);
                References.Events.ChangeKeyBindings();
                break;
        }
    }

    private void ChangeButtonState(int menuState)
    {
        switch (menuState)
        {
            case 0:
                switch (ButtonType)
                {
                    case Type.Open:
                        Activate();
                        break;
                    case Type.Close:
                        Deactivate();
                        break;
                    case Type.Keys:
                        Deactivate();
                        break;
                }
                break;
            case 1:
                switch (ButtonType)
                {
                    case Type.Open:
                        Deactivate();
                        break;
                    case Type.Close:
                        Activate();
                        break;
                    case Type.Keys:
                        Activate();
                        break;
                }
                break;
        }
    }

    public override void Activate(float duration = 0.125f)
    {
        base.Activate(duration);
        _boxCollider2D.enabled = true;
    }

    public override void Deactivate(float duration = 0.125f)
    {
        base.Deactivate(duration);
        _boxCollider2D.enabled = false;
    }
}
