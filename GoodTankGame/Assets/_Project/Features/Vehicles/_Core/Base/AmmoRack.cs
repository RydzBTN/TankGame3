using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AmmoRack : MonoBehaviour
{
    [Serializable]
    public class AmmoSlot
    {
        public AmmoData data;
        public int quantity;
    }
    [Serializable]
    public class MgAmmoBeltSlot
    {
        public AmmoDataMgBelt beltData;
        public int quantity;

        public AmmoData GetNextAmmoData()
        {
            return beltData.ammoDatas[quantity % beltData.ammoDatas.Length];
        }
    }

    [Header("Ammo Storage")]
    [SerializeField] private List<AmmoSlot> ammoSlots = new List<AmmoSlot>();
    [SerializeField] private int maxAmmo = 100;

    // Pojazd ma tylko 1 wybrany typ aminicji do km'a
    [Space(15)]
    [SerializeField] private MgAmmoBeltSlot ammoBeltSlot = new MgAmmoBeltSlot();
    [SerializeField] private int maxMgAmmo = 3000;

    [Header("Exposion")]
    [SerializeField] private ParticleSystem[] ammoBurnEffects;
    [SerializeField] private bool isDetonated = false;
    [SerializeField] private GameObject turret;
    private TankStatus status;

    private void Awake()
    {
        if (!TryGetComponent<TankStatus>(out status))
            Debug.LogWarning($"TankStatus Component not found on: {gameObject.name}");
    }
    private void OnEnable()
    {
        status.OnModuleDamaged += CheckExplosion;
    }
    private void OnDisable()
    {
        status.OnModuleDamaged -= CheckExplosion;
    }

    public void Initialize(AmmoSlot[] mainGunAmmo, MgAmmoBeltSlot mgAmmo)
    {
        ClearAmmoRack();

        ammoSlots.AddRange(mainGunAmmo);

        ammoBeltSlot.beltData = mgAmmo.beltData;
        ammoBeltSlot.quantity = mgAmmo.quantity;
    }

    private void ClearAmmoRack()
    {
        ammoSlots.Clear();
        ammoBeltSlot.beltData = null;
        ammoBeltSlot.quantity = 0;
    }

    public bool TryGetAmmo(AmmoData requestedAmmo)
    {
        AmmoSlot slot = ammoSlots.Find(x => x.data == requestedAmmo);
        if(slot != null && slot.quantity > 0)
        {
            slot.quantity--;
            return true;
        }

        return false;
    }
    public bool TryGetClip(AmmoData requestedAmmo, int requestedQuantity, out int receivedQuantity)
    {
        receivedQuantity = 0;

        AmmoSlot slot = ammoSlots.Find(x => x.data == requestedAmmo);
        if (slot != null && slot.quantity > 0)
        {
            if (slot.quantity >= requestedQuantity)
            {
                receivedQuantity = requestedQuantity;
                slot.quantity -= requestedQuantity;
            }
            else
            {
                receivedQuantity = slot.quantity;
                slot.quantity = 0;
            }
            return true;
        }

        return false;
    }
    public bool TryGetMgBelt(AmmoDataMgBelt requestedBeltType, int requestedQuantity, out int receivedQuantity)
    {
        
        receivedQuantity = 0;
        if(ammoBeltSlot != null && ammoBeltSlot.quantity > 0)
        {
            if(ammoBeltSlot.quantity >= requestedQuantity)
            {
                receivedQuantity = requestedQuantity;
                ammoBeltSlot.quantity -= requestedQuantity;
            }
            else
            {
                receivedQuantity = ammoBeltSlot.quantity;
                ammoBeltSlot.quantity = 0;
            }
            return true;
        }

        return false;
    }
    public AmmoData[] GetUniqueMainGunAmmoTypes()
    {
        AmmoData[] uniqueTypes = ammoSlots.Select(s => s.data).Distinct().ToArray();
        return uniqueTypes;
    }
    public AmmoSlot[] GetAmmoQuantity()
    {
        return ammoSlots.ToArray();
    }
    public MgAmmoBeltSlot GetMgAmmoQuantity()
    {
        return ammoBeltSlot;
    }

    #region AMMO RACK EXPLOSION
    private void CheckExplosion(Module module)
    {
        if (module.type != Module.ModuleType.Ammo) return;
        if (module.hp <= 0 && isDetonated == false) StartCoroutine(DetonateAmmoCoroutine());
    }
    private IEnumerator DetonateAmmoCoroutine()
    {
        isDetonated = true;
        float burnTime = UnityEngine.Random.Range(1f, 3f);

        PlayBurnEffects(burnTime);
        yield return new WaitForSeconds(burnTime);

        BlowUpTurret(GetExposionForce());

        status.NotifyTankDestruction();
    }
    private void PlayBurnEffects(float time)
    {
        foreach(ParticleSystem effect in ammoBurnEffects)
        {
            // Wrapper
            var duration = effect.main.duration;
            duration = time;
            effect.Play();
        }
    }
    private void BlowUpTurret(float force)
    {
        turret.transform.parent = null;

        Rigidbody turretRigidbody = turret.GetComponent<Rigidbody>();
        if (turretRigidbody == null)
        {
            turretRigidbody = turret.AddComponent<Rigidbody>();
        }

        turretRigidbody.mass = 3000f;
        turretRigidbody.isKinematic = false;
        turretRigidbody.constraints = RigidbodyConstraints.None;

        turretRigidbody.linearVelocity = transform.up * 10f;
        turretRigidbody.angularVelocity = UnityEngine.Random.insideUnitSphere * 2f;

    }
    public float GetExposionForce()
    {
        return 200000f;
    }
    #endregion

}
