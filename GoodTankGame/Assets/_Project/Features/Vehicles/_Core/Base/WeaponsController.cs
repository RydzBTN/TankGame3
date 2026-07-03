using UnityEngine;

public class WeaponsController : MonoBehaviour
{
    [SerializeField] private MainGun mainGun;
    [SerializeField] private MachineGun machineGun;


    public void TryFireMainGun() => mainGun.TryFire();
    public void TryFireMG() => machineGun.TryFire();

    public void InitializeWeapons(
        AmmoRack ammoRack,
        Rigidbody rb,
        ProjectilePoolProvider pool,
        TankStatus status)
    {
        mainGun.Initialize(ammoRack, rb, pool, status);
        machineGun.Initialize(ammoRack, rb, pool, status);

        pool.Initialize(ammoRack,
            mainGun.projectileSP,
            machineGun.mg_AmmoBelt,
            machineGun.projectileSP
        );
    }

    public void DisableWeapons()
    {
        mainGun.enabled = false;
        machineGun.enabled = false;
    }
}
