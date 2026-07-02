using UnityEngine;

[CreateAssetMenu(fileName = "NewAmmunitionData", menuName = "Scriptable Objects/AmmoData")]
public class AmmoData : ScriptableObject
{
    [Header("General info")]
    public string ammoName;
    public AmmunitionType typ;
    [TextArea]
    public string description;
    public GameObject projectilePrefab;

    [Header("Ballistics & Damage")]
    public float muzzleVelocity = 800f;
    public float mass;
    public float caliber;
    public float dragCoefficient;
    public AnimationCurve penetration;

    [Space(5)]
    [Header("Game Settings")]
    public float destroyDistance = 3000f;

    

    
    public float GetFrontSurfaceArea()
    {
        float diameter = caliber * 0.001f;
        return (Mathf.PI * diameter) / 4f;
    }
    
    public float GetPenetrationPower(float distance)
    {
        return penetration.Evaluate(distance) > 0 ? penetration.Evaluate(distance) : 10;
    }


    public enum AmmunitionType
    {
        AP,
        APC,
        APBC,
        APCBC,
        AP_HE,
        APC_HE,
        APBC_HE,
        APCBC_HE,
        HE,
        APCR,
    } 
}
