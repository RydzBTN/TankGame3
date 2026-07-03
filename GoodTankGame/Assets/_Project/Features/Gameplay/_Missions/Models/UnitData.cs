using UnityEngine;

[System.Serializable]
public class UnitData
{
    public enum Type
    {
        Tank,
        Troops,
        Stacionary
    }

    public string id;
    public Type type;
    public string prefabId;
    public Vector3 spawnPos;
    public bool isPlayer;
    public TankController.Team team;

    public AmmoRack.AmmoSlot[] ammo;
    public AmmoRack.MgAmmoBeltSlot mgAmmo;
    public float fuel;

    public AIOrderData[] orders;
}
