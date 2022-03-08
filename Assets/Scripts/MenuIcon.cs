using UnityEngine;

public class MenuIcon : UiElement
{
    public enum Type { Ping }

    public Type IconType;
    public Gradient PingGradient;

    protected override void Awake()
    {
        base.Awake();
        switch (IconType)
        {
            case Type.Ping:
                break;
        }
    }
    
    private void OnEnable()
    {
        References.Events.OnChangeMenuState += ChangeIconState;
    }

    private void OnDisable()
    {
        References.Events.OnChangeMenuState -= ChangeIconState;
    }
    
    private void ChangeIconState(int menuState)
    {
        switch (menuState)
        {
            case 0:
                switch (IconType)
                {
                    case Type.Ping:
                        Deactivate();
                        break;
                }
                break;
            case 1:
                switch (IconType)
                {
                    case Type.Ping:
                        Activate();
                        break;
                }
                break;
        }
    }
}
