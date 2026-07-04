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
    private int currentObjectiveIndex = 0;

    private CaptureZone captureZone = null;

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
        CreateObjective();
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
                        TankStatus status = tank.GetComponent<TankStatus>();
                        UIManager.Instance.ChangeUIToBattle(status);
                    }

                    tank.GetComponent<TankController>().Initialize(unit);
                    break;
            }
        }
    }
    private void CreateObjective()
    {
        ObjectiveData objective = objectives[currentObjectiveIndex];
        switch (objective.type)
        {
            case ObjectiveData.ObjectiveType.CaptureZone:
                captureZone = CraeteCaptureZone(objective.point, objective.zoneRadius);
                captureZone.zoneCaptured += OnZoneCaptured;
                break;
        }
    }
    private void AssignOrders()
    {

    }


    private CaptureZone CraeteCaptureZone(Vector3 position, float radius)
    {
        GameObject zone = GameObject.Instantiate(GameManager.Instance.CaptureZonePrefab, position, Quaternion.Euler(90, 0, 0));
        zone.transform.localScale = new Vector3(radius, radius, radius);
        return zone.GetComponent<CaptureZone>();
    }
    private void OnZoneCaptured(TankController.Team team)
    {
        captureZone.zoneCaptured -= OnZoneCaptured;
        GameObject.Destroy(captureZone.gameObject);
    }
}
