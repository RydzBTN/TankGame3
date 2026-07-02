using UnityEngine;

public class ArmorPlate : MonoBehaviour
{
    private enum ArmorType
    {
        RolledHomogeneousArmour,    // 1.00
        CastHomogeneousArmour,      // 0.94
        HighHardnessRolledArmour,   // 1.25
        StructuralSteel,            // 0.45    
        Tracks,                     // 0.75
        Wheel,                      // 0.30
        Wood                        // 0.05
    }

    [SerializeField] private ArmorType type;
    [SerializeField] private float armorThickness = 10;

    public float GetEffectiveThickness(float impactAngle)
    {
        float angleInRadians = impactAngle * Mathf.Deg2Rad;

        switch(type)
        {
            case ArmorType.RolledHomogeneousArmour: return (armorThickness * 1.00f) / Mathf.Cos(angleInRadians);
            case ArmorType.CastHomogeneousArmour:   return (armorThickness * 0.94f) / Mathf.Cos(angleInRadians);
            case ArmorType.HighHardnessRolledArmour:return (armorThickness * 1.25f) / Mathf.Cos(angleInRadians);
            case ArmorType.StructuralSteel:         return (armorThickness * 0.45f) / Mathf.Cos(angleInRadians);
            case ArmorType.Tracks:                  return (armorThickness * 0.75f) / Mathf.Cos(angleInRadians);
            case ArmorType.Wheel:                   return (armorThickness * 0.30f) / Mathf.Cos(angleInRadians);
            case ArmorType.Wood:                    return (armorThickness * 0.05f) / Mathf.Cos(angleInRadians);
            default:                                return armorThickness / Mathf.Cos(angleInRadians);
        }
    }
}
