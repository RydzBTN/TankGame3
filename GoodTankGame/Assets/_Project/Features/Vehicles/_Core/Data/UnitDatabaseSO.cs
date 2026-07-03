using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitDatabaseSO", menuName = "Scriptable Objects/UnitDatabaseSO")]
public class UnitDatabaseSO : ScriptableObject
{
    [SerializeField] private UnitDefinitionSO[] units;
    private Dictionary<string , UnitDefinitionSO> unitsById;


    public void Initialize()
    {
        unitsById = new Dictionary<string , UnitDefinitionSO>();

        for(int i = 0; i < units.Length; i++)
        {
            if (units[i] == null) continue;
            if (string.IsNullOrWhiteSpace(units[i].name))
            {
                Debug.LogWarning($"Unit definition without id {units[i].name}");
                continue;
            }
            if (unitsById.ContainsKey(units[i].id))
            {
                Debug.LogWarning($"Unit duplicat {units[i].name}");
                continue;
            }

            unitsById.Add(units[i].id, units[i]);
        }
    }

    public GameObject GetUnitPrefab(string id, bool isPlayer)
    {
        if(!unitsById.TryGetValue(id, out UnitDefinitionSO unitDef))
        {
            Debug.LogWarning($"Unit {id} not found");
            return null;
        }

        return isPlayer ? unitDef.playerPrefab : unitDef.aiPrefab;
    }










}
