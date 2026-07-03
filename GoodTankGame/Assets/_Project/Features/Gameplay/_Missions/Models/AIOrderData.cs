using UnityEngine;

[System.Serializable]
public class AIOrderData
{
    public enum AIOrderType
    {
        Patrol,
        Escort,
        Attack,
        Defend,
        MoveTo
    }
    public AIOrderType type;

    public Vector3 position;
    public string[] targetIds;
    public Vector3[] pathPoints;

    public bool waitUntillFinished = true;
}
