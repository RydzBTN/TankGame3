using System.Collections.Generic;
using System.Linq;

public static class MissionConverter
{
    public static PeriodData GetPeriodDataFromSO(PeriodSO so)
    {
        return new PeriodData
        {
            id = so.id,
            title = so.title,
            backgroundId = so.background != null ? so.background.name : string.Empty,
            start = so.start,
            end = so.end,
            description = so.description,
        };
    }

    public static CampaignData GetCampaignDataFromSO(CampaignSO so, string periodId)
    {
        return new CampaignData
        {
            id = so.id,
            periodId = periodId,
            title = so.title,
            backgroundId = so.background != null ? so.background.name : string.Empty,
            start = so.start,
            end = so.end,
            description = so.description,
        };
    }

    public static MissionData GetMissionDataFromSO(MissionSO so, string campaignId)
    {
        return new MissionData
        {
            id = so.id,
            campaignId = campaignId,
            title = so.title,
            backgroundId = so.background != null ? so.background.name : string.Empty,
            sceneId = so.sceneId,
            description = so.description,
            isCustom = false
        };
    }
    
}
