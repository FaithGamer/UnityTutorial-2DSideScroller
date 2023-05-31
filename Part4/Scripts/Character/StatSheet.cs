using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StatSheet", menuName = "MyAssets/StatSheet")]
public class StatSheet : ScriptableObject
{
    public List<Stat> stats;
}