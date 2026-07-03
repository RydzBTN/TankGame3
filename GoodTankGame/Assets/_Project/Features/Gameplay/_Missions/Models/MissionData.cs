[System.Serializable]
public class MissionData
{
    public string id;
    public string campaignId;
    public string title;
    public string backgroundId;
    public string sceneId;
    public string description;

    public bool isCustom;
}

/// <summary>
/// "Heavy" mission data, that doesnt have to get
/// loaded to show mission in the UI
/// </summary>
[System.Serializable]
public class MissionDetailsData
{
    public string missionId;

    public ObjectiveData[] objectives;
    public UnitData[] units;
}
