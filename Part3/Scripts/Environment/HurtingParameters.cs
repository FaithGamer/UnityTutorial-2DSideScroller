using UnityEngine;

[CreateAssetMenu(fileName = "HurtingParameters", menuName = "MyAssets/HurtingParameters")]
public class HurtingParameters : ScriptableObject
{
    public float pushPower = 2f;
    public int lifePoints = 1;

}