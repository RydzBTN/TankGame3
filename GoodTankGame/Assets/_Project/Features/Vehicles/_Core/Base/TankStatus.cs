using System;
using System.Collections;
using UnityEngine;
using static AmmoRack;


/// <summary>
/// Contains metchods invoked by other tank controllers to inform others about tank status
/// </summary>
public class TankStatus : MonoBehaviour
{
    // ENGINE
    public event Action<float, float> OnRPMChanged;
    public event Action<int> OnGearChanged;
    public event Action<float, float> OnFuelChanged;


    // MOVEMENT
    public event Action<Quaternion> OnTurretRotationChanged;
    public event Action<int> OnSpeedChanged;


    // AMMUNITION
    public event Action<float, float> OnReloadTimeChanged;
    public event Action<AmmoData> OnLoadedShellChanged;
    public event Action<AmmoData> OnSelectedShellChanged;
    public event Action<AmmoSlot[]> OnAmmoChanged;
    public event Action<MgAmmoBeltSlot> OnMgAmmoChanged;


    // DAMAGE
    public event Action<Module> OnModuleHpChanged;
    public event Action OnTankDestroyed;


    private Rigidbody rb;
    private float lastSpeed = -1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        CalculateSpeed();
    }

    #region Metchods invoked by other tank controllers
    // Ammo
    public void SelectedShellChanged(AmmoData shellData) => OnSelectedShellChanged?.Invoke(shellData);
    public void NotifyLoadedShellChange(AmmoData shellData) => OnLoadedShellChanged?.Invoke(shellData);
    public void NotifyReload(float reloadTime) => StartCoroutine(Reload(reloadTime));
    private IEnumerator Reload(float reloadTime)
    {
        float remainingTime = reloadTime;

        while (remainingTime > 0f)
        {
            OnReloadTimeChanged?.Invoke(remainingTime, reloadTime);
            remainingTime -= Time.deltaTime;
            yield return null;
        }

        OnReloadTimeChanged?.Invoke(0, reloadTime);
    }
    public void NotifyMainGunAmmoChange(AmmoRack ammoRack) => OnAmmoChanged?.Invoke(ammoRack.GetAmmoQuantity());
    public void NotifyMgAmmoChanged(MgAmmoBeltSlot ammoBelt) => OnMgAmmoChanged?.Invoke(ammoBelt);

    // Damage
    public void NotifyModuleHP(Module module) => OnModuleHpChanged?.Invoke(module);
    public void NotifyTankDestruction()
    {
        OnTankDestroyed?.Invoke();
    }

    // Movement
    public void NotifyTurretRotation(Quaternion rotation) => OnTurretRotationChanged?.Invoke(rotation);

    // Engine
    public void NotifyRPMChange(float rpm, float percentage) => OnRPMChanged?.Invoke(rpm, percentage);
    public void NotifyGearChange(int gear) => OnGearChanged?.Invoke(gear);
    public void NotifyFuelChange(float currentFuel, float percentage) => 
        OnFuelChanged?.Invoke(currentFuel, percentage);
    #endregion

    private void CalculateSpeed()
    {
        float speed = Vector3.Dot(rb.linearVelocity, transform.forward);
        int displaySpeed = Mathf.RoundToInt(speed * 3.6f);

        if (displaySpeed != Mathf.RoundToInt(lastSpeed))
        {
            lastSpeed = displaySpeed;
            // Pamiętaj, żeby zmienić Action<float> OnSpeedChanged na Action<int>
            // lub zrzutować do float:
            OnSpeedChanged?.Invoke(displaySpeed);
        }
    }

   
}
