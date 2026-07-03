using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Odpowiedzialny za stan gry, przejścia między scenami, zapis i wczytanie gry.
/// posiada referencje do managerow pomocniczych
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public UnitDatabaseSO unitDatabase;
    public ScriptableObjectDataProvider soDataProvider = new ScriptableObjectDataProvider();
    private SceneLoader loader = new SceneLoader();
    private MissionManager missionManager;

    private MissionData mission;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        unitDatabase.Initialize();
        soDataProvider.RefreshData();
    }

    private void Start()
    {
        SceneManager.LoadScene("MenuScene");
        UIManager.Instance.ChangeUIToMenu();
    }



    public void LoadMission(MissionData mission)
    {
        Debug.Log($"loading mission {mission.title}");
        if(soDataProvider.GetMissionDetails(mission.id) == null)
        {
            Debug.LogError($"Mission {mission.title} doesn't have DetailsData. Cancelling scene loading");
            return;
        }
        this.mission = mission;

        SceneManager.sceneLoaded += OnMissionSceneLoaded;
        SceneManager.LoadScene(mission.sceneId);
    }
    private void OnMissionSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnMissionSceneLoaded;

        missionManager = new MissionManager();
        missionManager.Initialize(mission);
        mission = null;
    }


    public void ExitMission()
    {
        missionManager = null;
    }
}
