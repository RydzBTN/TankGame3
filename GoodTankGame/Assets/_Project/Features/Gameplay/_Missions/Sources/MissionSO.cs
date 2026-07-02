using UnityEngine;

[CreateAssetMenu(fileName = "MissionSO", menuName = "Game/MissionSO")]
public class MissionSO : ScriptableObject
{
    public string id;
    public string title;
    public Sprite background;
    public string sceneId;
    [TextArea]
    public string description;
}
