using UnityEngine;

public class EntityManager : ScriptableObject
{
    public Cloud Cloud;
    public Ground Ground;
    public Handcar Handcar;
    public Cart Cart;

    public Player[] Players;

    public Player PlayerOne;
    public Player PlayerTwo;

    public void Init()
    {
        var cloudGo = GameObject.Find("Cloud");
        if (cloudGo != null)
        {
            Cloud = cloudGo.GetComponent<Cloud>();
        }

        var groundGo = GameObject.Find("Ground");
        if (groundGo != null)
        {
            Ground = groundGo.GetComponent<Ground>();
        }

        var handcarGo = GameObject.Find("Handcar");
        if (handcarGo != null)
        {
            Handcar = handcarGo.GetComponent<Handcar>();
        }

        var cartGo = GameObject.Find("Cart");
        if (cartGo != null)
        {
            Cart = cartGo.GetComponent<Cart>();   
        }
        
        if (PlayerOne && PlayerTwo)
        {
            PlayerOne.Counterpart = PlayerTwo;
            PlayerTwo.Counterpart = PlayerOne;
        }

        if (PlayerOne)
        {
            PlayerOne.PlayerId = 0;
            PlayerOne.Identifier = "h0";
            PlayerOne.InitSoul();
            PlayerOne.TakeControl();
            // Controllers.Audio.MusicBox.SetSong(PlayerOne.Music, PlayerOne.PlayerId);
            Controllers.Audio.MusicBox.SetSong(References.Audio.Variations[PlayerTwo.ColorId], PlayerOne.PlayerId);
        }

        if (PlayerTwo)
        {
            PlayerTwo.PlayerId = 1;
            PlayerTwo.Identifier = "c0";
            PlayerTwo.InitSoul();
            // Controllers.Audio.MusicBox.SetSong(PlayerTwo.Music, PlayerTwo.PlayerId);
            Controllers.Audio.MusicBox.SetSong(References.Audio.PlayerThemes[PlayerTwo.ColorId], PlayerTwo.PlayerId);
        }
    }

    public void ActivateObject(ControllableObject.Type objectType, float duration)
    {
        switch (objectType)
        {
            case ControllableObject.Type.Cloud:
                if (Cloud == null)
                {
                    References.Terminal.AddEntry("<red>Cloud is missing.</>");
                    return;
                }

                Cloud.Show(duration);
                break;
            
            case ControllableObject.Type.Handcar:
                if (Handcar == null)
                {
                    References.Terminal.AddEntry("<red>Handcar is missing.</>");
                    return;
                }

                Handcar.Show(duration);
                if (Cart.IsVisible)
                {
                    Handcar.ChangeCouplingAlpha(1f, duration);
                }
                break;
            
            case ControllableObject.Type.Cart:
                if (Cart == null)
                {
                    References.Terminal.AddEntry("<red>Cart is missing.</>");
                    return;
                }

                Cart.Show(duration);
                if (Handcar.IsVisible)
                {
                    Handcar.ChangeCouplingAlpha(1f, duration);
                }
                break;
            
            case ControllableObject.Type.Ground:
                if (Ground == null)
                {
                    References.Terminal.AddEntry("<red>Ground is missing.</>");
                    return;
                }

                Ground.Show(duration);
                break;
        }
    }
    
    public void DeactivateObject(ControllableObject.Type objectType, float duration)
    {
        switch (objectType)
        {
            case ControllableObject.Type.Cloud:
                if (Cloud == null)
                {
                    References.Terminal.AddEntry("<red>Cloud is missing.</>");
                    return;
                }
                
                Cloud.Hide(duration);
                break;
            
            case ControllableObject.Type.Handcar:
                if (Handcar == null)
                {
                    References.Terminal.AddEntry("<red>Handcar is missing.</>");
                    return;
                }
                
                Handcar.Hide(duration);
                Handcar.ChangeCouplingAlpha(0f, duration);
                break;
            
            case ControllableObject.Type.Cart:
                if (Cart == null)
                {
                    References.Terminal.AddEntry("<red>Cart is missing.</>");
                    return;
                }
                
                Cart.Hide(duration);
                Handcar.ChangeCouplingAlpha(0f, duration);
                break;
            
            case ControllableObject.Type.Ground:
                if (Ground == null)
                {
                    References.Terminal.AddEntry("<red>Ground is missing.</>");
                    return;
                }
                
                Ground.Hide(duration);
                break;
        }
    }

    public bool IsHandcarMovingUpSlope()
    {
        return Ground.CurrentEulerAngle > 0f && Handcar.LastMoveDirection.x < 0f || Ground.CurrentEulerAngle < 0f && Handcar.LastMoveDirection.x > 0f;
    }

    public bool AreBothVehiclesAnimated()
    {
        return Handcar.CurrentPlayer != null && Cart.CurrentPlayer != null &&
               Handcar.CurrentPlayer != Cart.CurrentPlayer;
    }

    public int ColorIdToPlayerId(int colorId)
    {
        return PlayerOne.ColorId == colorId ? PlayerOne.PlayerId :
            PlayerTwo.ColorId == colorId ? PlayerTwo.PlayerId : -1;
    }

    public Color ColorIdToColor(int colorId)
    {
        if (colorId < 0 || colorId >= Players.Length) return Color.white;
        return Players[colorId].Color;
    }
    
    public Color ColorIdToSoftColor(int colorId)
    {
        if (colorId < 0 || colorId >= Players.Length) return Color.white;
        return Players[colorId].SoftColor;
    }

    public ControllableObject GetObjectByType(ControllableObject.Type type, int playerId = 1)
    {
        switch (type)
        {
            case ControllableObject.Type.Cloud:
                return Cloud;
            case ControllableObject.Type.Handcar:
                return Handcar;
            case ControllableObject.Type.Ground:
                return Ground;
            case ControllableObject.Type.Cart:
                return Cart;
            case ControllableObject.Type.Soul:
                return playerId == 1 ? PlayerTwo.Soul : PlayerOne.Soul;
            default:
                return null;
        }
    }
}
