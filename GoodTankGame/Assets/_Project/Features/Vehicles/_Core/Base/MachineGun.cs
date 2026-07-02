using System.Collections;
using UnityEngine;
using static AmmoRack;

public class MachineGun : MonoBehaviour
{
    public Transform projectileSP;
    [Header("Machine Gun")]
    [SerializeField] private int mg_RPM = 400;
    private float mg_TimeBeetweenRounds = 60f / 400f;
    [SerializeField] private int mg_MaxAmmo = 300;
    [SerializeField] private float mg_RealoadTime = 8f;
    public MgAmmoBeltSlot mg_AmmoBelt;

    [Space(20)]
    [SerializeField] private AudioSource gunAudio;
    [SerializeField] private AmmoRack ammoRack;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private ProjectilePoolProvider projectilePools;
    [SerializeField] private TankStatus status;

    private bool canShoot = true;

    private void Awake()
    {
        mg_TimeBeetweenRounds = 60f/ mg_RPM;
    }

    public void Initialize(
           AmmoRack ammoRack,
           Rigidbody tankRigidbody,
           ProjectilePoolProvider projectilePools,
           TankStatus status)
    {
        this.ammoRack = ammoRack;
        this.rb = tankRigidbody;
        this.projectilePools = projectilePools;
        this.status = status;
        TryGetComponent<AudioSource>(out gunAudio);

        status.NotifyMgAmmoChanged(mg_AmmoBelt);
    }

    public void TryFire()
    {
        if (!canShoot) return;
        if(mg_AmmoBelt.quantity <= 0) return;

        FireMG();
    }

    private void FireMG()
    {
        AmmoData ammoToShoot = mg_AmmoBelt.GetNextAmmoData();
        AmmoControler ammo = projectilePools.GetMachineGunProjectile(ammoToShoot);

        ammo.transform.SetPositionAndRotation(projectileSP.position, projectileSP.rotation);
        ammo.Init(projectilePools.GetMachineGunPool(ammoToShoot), projectileSP.forward, rb.linearVelocity);
        mg_AmmoBelt.quantity--;
        status.NotifyMgAmmoChanged(mg_AmmoBelt);
        if (gunAudio  != null) gunAudio.Play();
        StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        canShoot = false;
        if (mg_AmmoBelt.quantity > 0)
        {
            yield return new WaitForSeconds(mg_TimeBeetweenRounds);
            canShoot = true;
        }
        else
        {
            if (ammoRack.TryGetMgBelt(mg_AmmoBelt.beltData, mg_MaxAmmo, out int receivedQuantity))
            {
                yield return new WaitForSeconds(mg_RealoadTime);
                yield return new WaitForSeconds(mg_TimeBeetweenRounds);
                mg_AmmoBelt.quantity = receivedQuantity;
            }
            else
            {
                Debug.Log("nie ma już amunicji do karabinu maszynowego");
            }
            canShoot = true;
        }
    }
}
