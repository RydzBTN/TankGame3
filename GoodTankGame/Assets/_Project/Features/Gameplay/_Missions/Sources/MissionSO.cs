using UnityEngine;

/// <summary>
/// Easy to setup mission data in editor can be converted to 2 classes
/// MissionData and MissionDetailsData
/// </summary>
[CreateAssetMenu(fileName = "MissionSO", menuName = "Game/MissionSO")]
public class MissionSO : ScriptableObject
{
    public string id;
    public string title;
    public Sprite background;
    public string sceneId;
    [TextArea]
    public string description;

    [Space(20), Header("Details")]
    public UnitData[] units;
    public ObjectiveData[] objectives;
}
