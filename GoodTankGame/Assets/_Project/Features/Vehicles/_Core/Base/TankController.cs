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
    public bool isPlayer;
    public bool isDead = false;


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
 
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
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
    private void Start()
    {
        weaponsController.InitializeWeapons(ammoRack, rb, projectilePools, status);
    }
    private void LateUpdate()
    {
        
    }
    private void OnEnable()
    {
        status.OnModuleDamaged += CheckModule;
    }
    private void OnDisable()
    {
        status.OnModuleDamaged -= CheckModule;
    }

    #region Damage and Modules
    private void CheckModule(Module module)
    {
        modules.Add(module);
    }

    //private IEnumerator RepairModule(ModuleType type)
    //{
    //    yield return 1f;
    //}

    #endregion
}