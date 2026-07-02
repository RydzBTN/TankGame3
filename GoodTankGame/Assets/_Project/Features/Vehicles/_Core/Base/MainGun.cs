using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGun : MonoBehaviour
{

    public Transform projectileSP;
    [SerializeField] private bool isAutocanon = false;
    [SerializeField] private AmmoData loadedShell;
    [SerializeField] private AmmoData selectedShell;
    [SerializeField] private float reloadTime = 5f;
    [SerializeField] private float clipReloadTime = 5f;
    [SerializeField] private float gunRecoilDistance = 0.1f;
    [SerializeField] private float gunRecoilTime = 1.0f;
    private Queue<AmmoData> clip = new Queue<AmmoData>();
    [SerializeField] private int clipSize = 5;

    [Space(20)]
    [SerializeField] private AudioSource gunAudio;
    [SerializeField] private AmmoRack ammoRack;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private ProjectilePoolProvider projectilePools;
    [SerializeField] private TankStatus status;

    private bool canShoot = true;

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

        status.NotifyMainGunAmmoChange(ammoRack);
    }

    public void TryFire()
    {
        if (!canShoot) return;

        if (loadedShell == null)
        {
            StartCoroutine(Reload());
            return;
        }

        FireLoadedShell();

    }
    private void FireLoadedShell()
    {
        AmmoControler ammo = projectilePools.GetMainGunProjectile(loadedShell);

        ammo.transform.SetPositionAndRotation(projectileSP.position, projectileSP.rotation);
        ammo.Init(projectilePools.GetMainGunPool(loadedShell), projectileSP.forward, rb.linearVelocity);

        rb.AddForceAtPosition(
            loadedShell.mass * loadedShell.muzzleVelocity * -transform.forward,
            transform.position,
            ForceMode.Impulse
        );


        loadedShell = null;
        if(gunAudio != null) gunAudio.Play();

        StartCoroutine(RecoilCoroutine(transform, gunRecoilDistance, gunRecoilTime));
        StartCoroutine(Reload());
    }
    private IEnumerator RecoilCoroutine(Transform gun, float distance, float time)
    {
        Vector3 originalPosition = gun.localPosition;
        Vector3 recoilPosition = originalPosition - Vector3.forward * distance;

        float recoilTime = time * 0.1f;
        float returnTime = time * 0.9f;

        float timer = 0f;
        while (timer < recoilTime)
        {
            timer += Time.deltaTime;
            gun.localPosition = Vector3.Lerp(originalPosition, recoilPosition, timer / recoilTime);
            yield return null;
        }

        timer = 0f;
        while (timer < returnTime)
        {
            timer += Time.deltaTime;
            gun.localPosition = Vector3.Lerp(recoilPosition, originalPosition, timer / returnTime);
            yield return null;
        }

        gun.localPosition = originalPosition;
    }
    private IEnumerator Reload()
    {
        canShoot = false;

        if (isAutocanon)
        {
            if (clip.TryDequeue(out AmmoData nextShell))
            {
                status.NotifyReload(reloadTime);
                yield return new WaitForSeconds(reloadTime);
                loadedShell = nextShell;
                status.NotifyLoadedShellChange(nextShell);
                status.NotifyMainGunAmmoChange(ammoRack);

                canShoot = true;
            }
            else
            {
                if (ammoRack.TryGetClip(selectedShell, clipSize, out int receivedSize))
                {
                    status.NotifyReload(clipReloadTime);
                    yield return new WaitForSeconds(clipReloadTime);

                    for (int i = 0; i < receivedSize; i++)
                    {
                        clip.Enqueue(selectedShell);
                    }

                    status.NotifyReload(reloadTime);
                    yield return new WaitForSeconds(reloadTime);

                    AmmoData dequeuedShell = clip.Dequeue();
                    loadedShell = dequeuedShell;
                    status.NotifyLoadedShellChange(dequeuedShell);
                    status.NotifyMainGunAmmoChange(ammoRack);

                    canShoot = true;
                }
                else
                {
                    Debug.Log("nie ma juz takiego klipu amunicji w magazynie");
                    canShoot = true;
                }
            }
        }
        else
        {
            if (ammoRack.TryGetAmmo(selectedShell))
            {
                status.NotifyReload(reloadTime);
                yield return new WaitForSeconds(reloadTime);
                loadedShell = selectedShell;
                status.NotifyLoadedShellChange(selectedShell);
                status.NotifyMainGunAmmoChange(ammoRack);

                canShoot = true;
            }
            else
            {
                Debug.Log("nie ma juz takiej amunicji w magazynie");
                canShoot = true;
            }
        }

    }

}
