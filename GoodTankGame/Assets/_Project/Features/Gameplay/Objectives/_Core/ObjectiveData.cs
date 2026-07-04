using UnityEngine;

[System.Serializable]
public class ObjectiveData
{
    public enum ObjectiveType 
    {
        DestroyTargets,
        CaptureZone,
        DefendZone,
        ReachPoint,
        Escort,
    }
    public string id;
    public string title;
    public ObjectiveType type;


    public string[] targetUnitIds;
    public Vector3 point;
    public float zoneRadius;
}
