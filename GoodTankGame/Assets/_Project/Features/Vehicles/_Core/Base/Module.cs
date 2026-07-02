using System;
using UnityEngine;

public class Module : MonoBehaviour
{
    public enum ModuleType
    {
        Engine,
        Transmission,
        Ammo,
        Stabilizer,
        Driver,
        Gunner,
        MachineGunner,
        Loader,
        Commander
    }

    public ModuleType type;
    public float hp = 100;

    private TankStatus status;

    private void Awake()
    {
        status = GetComponentInParent<TankStatus>();
        if (status == null) Debug.LogWarning($"Tank Status Component not found on: {gameObject.name}");
    }
   
    private void Start()
    {
        status.NotifyModuleDamage(this);
    }


    public void GetDamage(float damage)
    {
        hp -= damage;
        status.NotifyModuleDamage(this);
    }
}
