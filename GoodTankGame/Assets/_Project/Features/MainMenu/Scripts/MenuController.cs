using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuController
{
    private VisualElement root;

    //////////////
    //  Left Panel
    //////////////
    private Button button_SelectCampain;
    private Button button_Settings;
    private Button button_Quit;

    //////////////
    //  Missions Panel
    //////////////
    private Label periodName;
    private Label periodDate;
    private Label campaignName;
    private Label campaignDate;
    private Button BackButton;
    private ListView missionList;
    

    // działanie
    private GameManager gameManager;

    private List<PeriodData> periods;
    private List<CampaignData> campaigns;
    private List<MissionData> missions;

    public MenuController(VisualElement root)
    {
        this.root = root;
        gameManager = GameManager.Instance;

        // Left Menu
        button_SelectCampain = root.Q<Button>("btn-campaign");
        button_Settings = root.Q<Button>("btn-settings");
        button_Quit = root.Q<Button>("btn-quit");

        //  Missions Panel
        periodName = root.Q<Label>("period_name");
        periodDate = root.Q<Label>("period-date");
        campaignName = root.Q<Label>("campaign-name");
        campaignDate = root.Q<Label>("campaign-date");
        BackButton = root.Q<Button>("Back");
        missionList = root.Q<ListView>("mission-list");


        BackButton.clicked += OnBackClicked;

        BuildPeriodsList();
    }






    #region Mission Explorer
    private void OnBackClicked()
    {
        if(missionList.itemsSource == missions)
        {
            BackToCampaignsList();
        }
        else if(missionList.itemsSource == campaigns)
        {
            BuildPeriodsList();
        }
    }

    public void BuildPeriodsList()
    {
        periods = gameManager.soDataProvider.GetAllPeriods();
        missionList.itemsSource = periods;

        missionList.makeItem = MakeButton;
        missionList.bindItem = BindPeriodButton;

        missionList.fixedItemHeight = 80;
        missionList.Rebuild();

        periodName.text = "Historical";
        periodDate.text = string.Empty;
        campaignName.text = "periods";
        campaignDate.text = string.Empty;
    }
    public void BuildCampaignsList(PeriodData period)
    {
        campaigns = gameManager.soDataProvider.GetAllCampaignsInPeriod(period);
        missionList.itemsSource = campaigns;

        missionList.makeItem= MakeButton;
        missionList.bindItem= BindCampaignButton;

        missionList.fixedItemHeight = 80;
        missionList.Rebuild();

        periodName.text = period.title;
        periodDate.text = $"{period.start} - {period.end}";
        campaignName.text = string.Empty;
        campaignDate.text = string.Empty;
    }
    public void BackToCampaignsList()
    {
        if(campaigns == null)
        {
            Debug.LogError("campaign list is null");
            return;
        }
        missionList.itemsSource = campaigns;

        missionList.makeItem = MakeButton;
        missionList.bindItem = BindCampaignButton;

        missionList.fixedItemHeight = 80;
        missionList.Rebuild();

        campaignName.text = string.Empty;
        campaignDate.text = string.Empty;
    }
    public void BuildMissionsList(CampaignData campaign)
    {
        missions = gameManager.soDataProvider.GetAllMissionsInCampaign(campaign);
        missionList.itemsSource = missions;

        missionList.makeItem = MakeButton;
        missionList.bindItem = BindMissionButton;

        missionList.fixedItemHeight = 80;
        missionList.Rebuild();

        campaignName.text = campaign.title;
        campaignDate.text = $"{campaign.start} - {campaign.end}";
    }
    private void OpenMissionDetails()
    {

    }


    private VisualElement MakeButton()
    {
        VisualElement root = new VisualElement();
        root.AddToClassList("mission-item");

        Button button = new Button();
        button.name = "btn";
        button.AddToClassList("mission-button");
        
        button.clicked += () =>
        {
            if(button.userData is PeriodData period) // is == sprawdzenie i przypisanie - zapamiętać
            {
                BuildCampaignsList(period);
            }
            else if(button.userData is CampaignData campaign)
            {
                BuildMissionsList(campaign);
            }
            else if(button.userData is MissionData mission)
            {
                GameManager.Instance.LoadMission(mission);
            }
        };

        root.Add(button);
        return root;
    }


    private void BindPeriodButton(VisualElement element, int id)
    {
        Button button = element.Q<Button>("btn");
        PeriodData period = periods[id];

        button.text = period.title;
        button.userData = period;
    }
    private void BindCampaignButton(VisualElement element, int id)
    {
        Button button = element.Q<Button>("btn");
        CampaignData campaign = campaigns[id];

        button.text = campaign.title;
        button.userData = campaign;
    }
    private void BindMissionButton(VisualElement element, int id)
    {
        Button button = element.Q<Button>("btn");
        MissionData mission = missions[id];

        button.text = mission.title;
        button.userData = mission;
    }
    #endregion


    public void Dispose()
    {
        BackButton.clicked -= OnBackClicked;
    }

}
