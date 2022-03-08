using UnityEngine;

public class PrefabManager : ScriptableObject
{
    public AssessmentScreen[] AssessmentScreens;

    public AssessmentScreen GetAssessmentScreen(int itemCount)
    {
        var index = Mathf.Clamp(itemCount, 0, AssessmentScreens.Length - 1);
        var assessmentScreen = Instantiate(AssessmentScreens[index], Controllers.Ui.UiTf);
        assessmentScreen.transform.localPosition = Vector3.zero;
        return assessmentScreen;
    }

    public BackgroundTile GetBackgroundTile()
    {
        return References.Pool.Get(typeof(BackgroundTile)) as BackgroundTile;
    }
    
    public BoostParticle GetBoostParticle()
    {
        return References.Pool.Get(typeof(BoostParticle)) as BoostParticle;
    }

    public Companion GetCompanion()
    {
        return References.Pool.Get(typeof(Companion)) as Companion;
    }

    public UiCursor GetCursor()
    {
        return References.Pool.Get(typeof(UiCursor)) as UiCursor;
    }
    
    public Letter GetLetter()
    {
        return References.Pool.Get(typeof(Letter)) as Letter;
    }

    public Lightning GetLightning()
    {
        return References.Pool.Get(typeof(Lightning)) as Lightning;
    }

    public LightningSegment GetLightningSegment()
    {
        return References.Pool.Get(typeof(LightningSegment)) as LightningSegment;
    }
    
    public Pixel GetPixel()
    {
        return References.Pool.Get(typeof(Pixel)) as Pixel;
    }
    
    public PixelExplosion GetPixelExplosion()
    {
        return References.Pool.Get(typeof(PixelExplosion)) as PixelExplosion;
        //return Instantiate(PixelExplosion);
    }

    public Raindrop GetRaindrop()
    {
        return References.Pool.Get(typeof(Raindrop)) as Raindrop;
    }

    public SlowdownIcon GetSlowdownIcon()
    {
        return References.Pool.Get(typeof(SlowdownIcon)) as SlowdownIcon;
    }

    public SoulPixel GetSoulPixel()
    {
        return References.Pool.Get(typeof(SoulPixel)) as SoulPixel;
    }

    public Sparkle GetSparkle()
    {
        return References.Pool.Get(typeof(Sparkle)) as Sparkle;
    }

    public Sparks GetSparks()
    {
        return References.Pool.Get(typeof(Sparks)) as Sparks;
    }

    public Speaker GetSpeaker()
    {
        return References.Pool.Get(typeof(Speaker)) as Speaker;
    }

    public TerminalEntry GetTerminalEntry()
    {
        return References.Pool.Get(typeof(TerminalEntry)) as TerminalEntry;
    }

    public TextBox GetTextBox()
    {
        return References.Pool.Get(typeof(TextBox)) as TextBox;
    }

    public WaveFormDisplay GetWaveFormDisplay()
    {
        return References.Pool.Get(typeof(WaveFormDisplay)) as WaveFormDisplay;
    }
}
