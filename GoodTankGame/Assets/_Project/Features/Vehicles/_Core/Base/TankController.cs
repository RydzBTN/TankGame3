using System.Collections.Generic;
using UnityEngine;

public class TankController : MonoBehaviour
{
    public enum Team
    {
        A,
        B
    }
    
    // ==========================================
    // USTAWIENIA W EDYTORZE (INSPECTOR)
    // ==========================================
    public Team team;
    public bool isPlayer = false;
    public bool isDead = false;
    public string id;

    [Space(15)]
    [Header("MODULES")]
    [SerializeField] private List<Module> modules = new List<Module>();

    // ==========================================
    // PRIVATE
    // ==========================================
    private AmmoRack ammoRack;
    private TankStatus status;
    private Rigidbody rb;
    private WeaponsController weaponsController;
    private ProjectilePoolProvider projectilePools;
    private bool ammoIsDetonated = false;
    private bool turretIsDetonated = false;
    
    
    private void Awake()
    {
        if (isPlayer)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (!TryGetComponent<TankStatus>(out status))
            Debug.LogWarning($"TankStatus Component not found on: {gameObject.name}");

        if (!TryGetComponent<AmmoRack>(out ammoRack))
            Debug.LogWarning($"AmmoRack Component not found on: {gameObject.name}");

        if (!TryGetComponent<Rigidbody>(out rb))
            Debug.LogWarning($"Rigidbody Component not found on: {gameObject.name}");

        if (!TryGetComponent<ProjectilePoolProvider>(out projectilePools))
            Debug.LogWarning($"ProjectilePoolProvider Component not found on: {gameObject.name}");

        if (!TryGetComponent<WeaponsController>(out weaponsController))
            Debug.LogWarning($"WeaponsController Component not found on: {gameObject.name}");
    }

    public void Initialize(UnitData data)
    {
        isPlayer = data.isPlayer;
        team = data.team;
        id = data.id;

        weaponsController.InitializeWeapons(ammoRack, rb, projectilePools, status);
        ammoRack.Initialize(data.ammo, data.mgAmmo);
    }
    
    private void OnEnable()
    {
        status.OnModuleHpChanged += CheckModule;
        status.OnTankDestroyed += DisableTank;
    }
    private void OnDisable()
    {
        status.OnModuleHpChanged -= CheckModule;
        status.OnTankDestroyed -= DisableTank;
    }

    #region Damage and Modules
    private void CheckModule(Module module)
    {
        modules.Add(module);
    }

    private void DisableTank()
    {
        if (isPlayer)
        {
            GetComponent<PlayerTankInput>().enabled = false;
            GetComponent<VehicleCameraController>().enabled = false;
        }
        else
        {
            GetComponent<TankAIController>().enabled = false;
            GetComponent<TankAIPerception>().enabled = false;
            GetComponent<TankNavigation>().enabled = false;
        }

        GetComponent<ProjectilePoolProvider>().enabled = false;
        GetComponent<AimingController>().enabled = false;
        GetComponent<VehicleAudioController>().enabled = false;
        weaponsController.DisableWeapons();
        GetComponent<PowertrainSystem>().isEngineRunning = false;
        isDead = true;
    }

   

    #endregion
}