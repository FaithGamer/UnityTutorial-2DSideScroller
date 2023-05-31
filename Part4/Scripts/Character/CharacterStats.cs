using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct Stat
{
    public string name;
    public int value;
}

public struct ChangeStatEventData
{
    public ChangeStatEventData(string StatName, int ChangedValue, int FinalValue)
    {
        statName = StatName;
        changedValue = ChangedValue;
        finalValue = FinalValue;
    }
    public string statName;
    public int changedValue;
    public int finalValue;
}

public class CharacterStats : MonoBehaviour
{
    public delegate void OnChangeStat(ChangeStatEventData data);    
    public event OnChangeStat ChangeStatEvent;
    [SerializeField] StatSheet m_stats;
    Dictionary<string, int> m_values = new Dictionary<string, int>();

    void Awake()
    {
        AssignValuesOnStart();
        var hurtable = GetComponent<Hurtable>();
        if (hurtable)
        {
            hurtable.HurtEvent += OnHurt;
        }
    }
    private void AssignValuesOnStart()
    {
        foreach (var stat in m_stats.stats)
        {
            m_values.Add(stat.name, stat.value);
        }
    }

    public void OnHurt(HurtEventData data)
    {
        int hitPoints = data.parameters.lifePoints;
        if (HaveStat("health"))
        {
            ChangeStat("health", -hitPoints);
        }
    }
    public void ChangeStat(string name, int modifValue)
    {
        if (!m_values.ContainsKey(name))
        {
            Debug.LogWarning("Trying to change stat that doesn't exists on this CharacterStats, stat: " + name + " gameObjectInstanceID:" + gameObject.GetInstanceID());
            return;
        }
        m_values[name] += modifValue;
        ChangeStatEventData data = new ChangeStatEventData(name, modifValue, m_values[name]);
        ChangeStatEvent?.Invoke(data);
    }
    public bool HaveStat(string name)
    {
        return m_values.ContainsKey(name);
    }
    public int GetStat(string name)
    {
        int value = 0;
        if (!m_values.TryGetValue(name, out value))
        {
            Debug.LogWarning("Trying to get stat that doesn't exists on this CharacterStats, stat: " + name + " gameObjectInstanceID:" + gameObject.GetInstanceID());
        }
        return value;
    }
    public int GetStatMax(string name)
    {
        //To do optimize
        foreach (var stat in m_stats.stats)
        {
            if (stat.name == name)
            {
                return stat.value;
            }
        }
        return 0;
    }
}
