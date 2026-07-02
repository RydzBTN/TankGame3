using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    

    [SerializeField] private VisualTreeAsset MenuUI;
    [Space(15)]
    [Header("HUD in Battle")]
    [SerializeField] private VisualTreeAsset BattleUI;
    [SerializeField] private VisualTreeAsset ammoSlotPrefab;

    [Space(15)]
    [Header("PlayerTank")]
    [SerializeField] private TankStatus tank;

    private UIDocument playerUI;
    private MenuController menuController;
    private HUDController hc;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);



        if (!TryGetComponent<UIDocument>(out playerUI))
            Debug.LogError("UI Manager doesnt have required UI Document component");

    }

    private void Start()
    {
        SwitchUIToBattle();

        if (tank == null)
        {
            // tymczasowe szybkie przypisanie TankStatus szukając pierwszego wynisku na scenie
            // zrobić w game manager obsługe spawnu gracza ktory przypisuje hud do czołgu
            TankStatus playerTank = FindObjectsByType<TankStatus>()
                .FirstOrDefault(ts => ts.GetComponent<TankController>().isPlayer);

            if (playerTank != null) BindToTank(playerTank);
        }
    }

    // Przypisywanie metod Hud controllera
    // do eventów wywoływanych na przypisanym czołgu
    // TODO przypisywanie przy SwitchUI
    public void BindToTank(TankStatus tankToBind)
    {
        if (tank != null) Unbind();

        tank = tankToBind;


        if (hc == null) return;

        tank.OnReloadTimeChanged += hc.ChangeReloadData;
        tank.OnLoadedShellChanged += hc.ChangeLoadedShell;
        tank.OnAmmoChanged += hc.ChangeAmmoQuantity;
        tank.OnMgAmmoChanged += hc.ChangeMgAmmoQuantity;

        tank.OnRPMChanged += hc.ChangeEngineRpm;
        tank.OnSpeedChanged += hc.ChangeSpeed;
        tank.OnGearChanged += hc.ChangeGear;
        tank.OnFuelChanged += hc.ChangeFuelPercentage;


    }

    // odpinanie eventow inaczej śmieci w pamięci ktorych chyba GC nie usunie
    private void Unbind()
    {
        if (tank == null || hc == null) return;

        tank.OnReloadTimeChanged -= hc.ChangeReloadData;
        tank.OnLoadedShellChanged -= hc.ChangeLoadedShell;
        tank.OnAmmoChanged -= hc.ChangeAmmoQuantity;
        tank.OnMgAmmoChanged -= hc.ChangeMgAmmoQuantity;

        tank.OnRPMChanged -= hc.ChangeEngineRpm;
        tank.OnSpeedChanged -= hc.ChangeSpeed;
        tank.OnGearChanged -= hc.ChangeGear;
        tank.OnFuelChanged -= hc.ChangeFuelPercentage;
    }



    public void SwitchUIToMenu()
    {
        CleanupControllers();
        ChangeUI(MenuUI);
        menuController = new MenuController(playerUI.rootVisualElement);
    }
    public void SwitchUIToBattle()
    {
        CleanupControllers();
        ChangeUI(BattleUI);
        hc = new HUDController(playerUI.rootVisualElement, ammoSlotPrefab);
    }
    private void ChangeUI(VisualTreeAsset asset)
    {
        playerUI.visualTreeAsset = asset;
    }
    private void CleanupControllers()
    {
        menuController?.Dispose();
        menuController = null;

        hc?.Dispose();
        hc = null;
    }
    public void HideUI()
    {
        CleanupControllers();
        playerUI.visualTreeAsset = null;
    }
}
