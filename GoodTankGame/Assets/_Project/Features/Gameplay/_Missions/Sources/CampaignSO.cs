using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CamapignSO", menuName = "Game/CamapignSO")]
public class CampaignSO : ScriptableObject
{
    public string id;
    public string title;
    public Sprite background;
    public string start;
    public string end;
    [TextArea]
    public string description;

    public List<MissionSO> missions;
}
