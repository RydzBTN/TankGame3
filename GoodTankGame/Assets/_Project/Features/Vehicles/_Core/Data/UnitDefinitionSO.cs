using UnityEngine;

[CreateAssetMenu(fileName = "UnitDefinitionSO", menuName = "Scriptable Objects/UnitDefinitionSO")]
public class UnitDefinitionSO : ScriptableObject
{
    public string id;
    public string displayName;
    public GameObject playerPrefab;
    public GameObject aiPrefab;
}
