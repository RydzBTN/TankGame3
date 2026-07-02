using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PeriodSO", menuName = "Game/PeriodSO")]
public class PeriodSO : ScriptableObject
{
    public string id;
    public string title;
    public Sprite background;
    public string start;
    public string end;
    [TextArea]
    public string description;

    public List<CampaignSO> campaigns;
}
