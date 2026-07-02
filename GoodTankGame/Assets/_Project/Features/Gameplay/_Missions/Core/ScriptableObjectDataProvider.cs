using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ScriptableObjectDataProvider
{
    [SerializeField] private PeriodSO[] allPeriods;

    private Dictionary<string, MissionSO> missionDatabase = new Dictionary<string, MissionSO>();

    public void RefreshData()
    {
        foreach(PeriodSO periodSO in allPeriods)
        foreach(CampaignSO campaignSO in periodSO.campaigns)
        foreach(MissionSO missionSO in campaignSO.missions)
        {
            missionDatabase.Add(missionSO.id, missionSO);
        }
    }

    public List<PeriodData> GetAllPeriods()
    {
        return allPeriods
            .Select(p => MissionConverter.GetPeriodDataFromSO(p))
            .ToList();
    }
    public List<CampaignData> GetAllCampaignsInPeriod(PeriodData period)
    {
        PeriodSO periodSO = allPeriods
            .Where(p => p.id == period.id).FirstOrDefault();

        return periodSO.campaigns
            .Select(c => MissionConverter.GetCampaignDataFromSO(c, periodSO.id))
            .ToList();
    }
    public List<MissionData> GetAllMissionsInCampaign(CampaignData campaign)
    {
        CampaignSO campaignSO = allPeriods
            .FirstOrDefault(p => p.id == campaign.periodId)?
            .campaigns
            .FirstOrDefault(c => c.id == campaign.id);

        return campaignSO.missions
            .Select(m => MissionConverter.GetMissionDataFromSO(m, campaignSO.id))
            .ToList();
    }
}
