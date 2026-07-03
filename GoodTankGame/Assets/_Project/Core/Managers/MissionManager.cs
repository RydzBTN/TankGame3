using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zarządza konkretną misją spawn, zadania,konic gry
/// </summary>
public class MissionManager
{
    private MissionData missionData;
    private MissionDetailsData missionDetailsData;
    private ObjectiveData[] objectives;

    public void Initialize(MissionData mission)
    {
        missionData = mission;
        missionDetailsData = GameManager.Instance.soDataProvider.GetMissionDetails(mission.id);
        objectives = missionDetailsData.objectives;

        StartMission();
    }

    private void StartMission()
    {
        SpawnForces();
        AssignOrders();
    }
    private void SpawnForces()
    {
        foreach(UnitData unit in missionDetailsData.units)
        {
            switch (unit.type)
            {
                case UnitData.Type.Tank:
                    GameObject tank = GameObject.Instantiate(
                        GameManager.Instance.unitDatabase.GetUnitPrefab(unit.prefabId, unit.isPlayer),
                        unit.spawnPos,
                        Quaternion.Euler(0, 0, 0)
                        );

                    if (unit.isPlayer)
                    {
                        Debug.Log("zmiana UI");
                        UIManager.Instance.ChangeUIToBattle(tank.GetComponent<TankStatus>());
                    }

                    tank.GetComponent<TankController>().Initialize(unit);
                    break;
            }
        }
    }
    private void AssignOrders()
    {

    }
    
}
