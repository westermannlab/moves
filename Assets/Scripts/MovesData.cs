[System.Serializable]
public class MovesData
{
    public string version;
    public string vp;
    public int[] roomOrder;
    public int[] roomVisits;
    public string[] roomCaptions;
    public float duration;
    public int debug;
    public int recordings;
    
    public string msgRoomAlreadyCompleted;
    public string msgRoomNotYetAvailable;
    public string msgAllRoomsClear;
    public string msgThankYou;
    public string msgNoVpCode;
    public string msgError;
    public string msgSendLogError;
    public string msgFileNotFound;
    public string msgFileCorrupted;
    public string msgUnknownError;

    public string msgEnactCloud;
    public string msgLeaveCloud;
    public string msgEnactGround;
    public string msgLeaveGround;
    public string msgEnactCart;
    public string msgLeaveCart;
    public string msgEnactHandcar;
    public string msgLeaveHandcar;
    public string msgTiltLeft;
    public string msgTiltRight;
    public string msgRain;
    public string msgShake;
    public string msgBrake;
    public string msgBoost;
    
    public string colorRed;
    public string colorOrange;
    public string colorYellow;
    public string colorGreen;
    public string colorBlue;
    public string colorPurple;
    
    public LikertScaleData[] scales;

    public MovesData ()
    {
        version = "unknown";
        vp = "-1";
        roomOrder = new[] {1, 2, 3, 4, 5};
        roomVisits = new[] {0, 0, 0, 0, 0, 0, 0};
        roomCaptions = new[] {"Tutorial", "Level 1", "Level 2", "Level 3", "Level 4", "Level 5"};
        duration = 180f;
        debug = 0;
        msgRoomAlreadyCompleted = "";
        msgRoomNotYetAvailable = "";
        msgAllRoomsClear = "";
        msgThankYou = "";
        msgNoVpCode = "";
        scales = new LikertScaleData[0];
    }
    
    public void Init()
    {
        if (duration < 1f)
        {
            duration = 1f;
        }
    }

    public override string ToString()
    {
        var order = "";
        var visits = "";
        for (var i = 0; i < roomOrder.Length; i++)
        {
            order += roomOrder[i];
            if (i < roomOrder.Length - 1) order += ", ";
        }
        for (var i = 0; i < roomVisits.Length; i++)
        {
            visits += roomVisits[i];
            if (i < roomVisits.Length - 1) visits += ", ";
        }
        return string.Concat("MovesData:", '\n', "VP: ", vp, '\n', "Room order: ", order, '\n', "Room visits: ", visits, '\n', "Duration: ", duration, "s\n", "Debug mode: ", debug == 2 ? "On" : "Off", '\n', "Scales: ", scales.Length);
    }
}
