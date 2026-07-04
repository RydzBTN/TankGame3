using System;
using System.Collections.Generic;
using UnityEngine;
using static TankController;

public class CaptureZone : MonoBehaviour
{
    private List<TankController> tanksInZone = new List<TankController>();
    private (int progress, Team team) captureStatus;

    public event Action<Team> zoneCaptured;
    public event Action<Team, int> captureProgressChange;

    private static float CAP_TIME_INTERVAL = 1f;
    private float lastUpdate;

    private void OnTriggerEnter(Collider col)
    {
        if(col.TryGetComponent<TankController>(out TankController tank)) 
            tanksInZone.Add(tank);
    }
    private void OnTriggerExit(Collider col)
    {
        if (col.TryGetComponent<TankController>(out TankController tank) && tanksInZone.Contains(tank)) 
            tanksInZone.Remove(tank);
    }

    private void Update()
    {
        if (Time.time - lastUpdate < CAP_TIME_INTERVAL) return;
        lastUpdate = Time.time;

        //TODO Zmiana teamu w krotce oraz clamp na progress 0-100

        if (tanksInZone.Count <= 0)
        {
            captureStatus.progress--;
            return;
        }

        foreach (TankController tank in tanksInZone)
        {
            switch (tank.team) 
            {
                case Team.A:
                    if (captureStatus.team == Team.B) captureStatus.progress--;
                    else captureStatus.Item1++;
                    break;

                case Team.B:
                    if (captureStatus.team == Team.A) captureStatus.progress--;
                    else captureStatus.progress++;
                    break;
            }
        }

        Debug.Log($"druzyna {captureStatus.team} zajela baze w {captureStatus.progress}%");
        captureProgressChange?.Invoke(captureStatus.team, captureStatus.progress);
        if (captureStatus.progress >= 100) zoneCaptured?.Invoke(captureStatus.team);
    }
}
