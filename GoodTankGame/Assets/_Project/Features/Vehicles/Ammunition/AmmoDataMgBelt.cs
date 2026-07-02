using UnityEngine;

[CreateAssetMenu(fileName = "AmmoDataMgBelt", menuName = "Scriptable Objects/AmmoDataMgBelt")]
public class AmmoDataMgBelt : ScriptableObject
{
    public string beltName;
    public AmmoData[] ammoDatas;
}
