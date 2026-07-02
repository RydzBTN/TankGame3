using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePoolProvider : MonoBehaviour
{
    [Space(15)]
    [Header("PROJECTILE POOLS")]
    private Dictionary<AmmoData, IObjectPool<AmmoControler>> mainGunPools;
    [SerializeField] private int mainGunPoolSize = 3;
    [SerializeField] private int mainGunMaxPoolSize = 10;

    private Dictionary<AmmoData, IObjectPool<AmmoControler>> machineGunPools;
    [SerializeField] private int mg_PoolSize = 20;
    [SerializeField] private int mg_MaxPoolSize = 50;


    public AmmoControler GetMainGunProjectile(AmmoData ammoData)
    {
        return mainGunPools[ammoData].Get();
    }

    public IObjectPool<AmmoControler> GetMainGunPool(AmmoData ammoData)
    {
        return mainGunPools[ammoData];
    }

    public AmmoControler GetMachineGunProjectile(AmmoData ammoData)
    {
        return machineGunPools[ammoData].Get();
    }

    public IObjectPool<AmmoControler> GetMachineGunPool(AmmoData ammoData)
    {
        return machineGunPools[ammoData];
    }


    public void Initialize(AmmoRack ammoRack, Transform mainGunProjectileSP, AmmoRack.MgAmmoBeltSlot mgAmmoBelt, Transform mgProjectileSP)
    {
        mainGunPools = new Dictionary<AmmoData, IObjectPool<AmmoControler>>();
        machineGunPools = new Dictionary<AmmoData, IObjectPool<AmmoControler>>();

        // Inicjalizacja puli prefabrykatów pociskow dla 
        // GŁÓWNEGO DZIAŁA
        foreach (AmmoData ammoData in ammoRack.GetUniqueMainGunAmmoTypes())
        {
            if (!mainGunPools.ContainsKey(ammoData))
                mainGunPools[ammoData] = CreatePool(ammoData, mainGunProjectileSP, mainGunPoolSize, mainGunMaxPoolSize);
        }


        // Inicjalizacja puli prefabrykatów pociskow dla 
        // KARABINU MASZYNOWEGO
        foreach (AmmoData ammoData in mgAmmoBelt.beltData.ammoDatas)
        {
            if (!machineGunPools.ContainsKey(ammoData))
                machineGunPools[ammoData] = CreatePool(ammoData, mgProjectileSP, mg_PoolSize, mg_MaxPoolSize);
        }
    }

    private IObjectPool<AmmoControler> CreatePool(AmmoData ammoData, Transform defaultTransform, int capacity, int maxCapacity)
    {
        return new ObjectPool<AmmoControler>(
            createFunc: () => Instantiate(ammoData.projectilePrefab.GetComponent<AmmoControler>()),
            actionOnGet: ammo =>
            {
                ammo.transform.SetPositionAndRotation(defaultTransform.position, defaultTransform.rotation);
                ammo.gameObject.SetActive(true);
            },
            actionOnRelease: ammo => ammo.gameObject.SetActive(false),
            actionOnDestroy: ammo => Destroy(ammo.gameObject),
            collectionCheck: true,
            defaultCapacity: capacity,
            maxSize: maxCapacity
        );
    }
}
