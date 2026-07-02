using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] public ScriptableObjectDataProvider soDataProvider = new ScriptableObjectDataProvider();

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        soDataProvider.RefreshData();

        SceneManager.sceneLoaded += ChangeUIToScene;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= ChangeUIToScene;
    }


    private void ChangeUIToScene(Scene scena, LoadSceneMode mode)
    {
        //if (scena.name.Equals("MenuScene"))
        //{
        //    UIManager.Instance.SwitchUIToMenu();
        //}
        //else
        //{
        //    UIManager.Instance.SwitchUIToBattle();
        //}
    }
}
