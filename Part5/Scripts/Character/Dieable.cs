using UnityEngine;

public struct DieEventData
{
    public float destroyDelay;
}
public class Dieable : MonoBehaviour
{
    public delegate void OnDie(DieEventData data);
    public event OnDie DieEvent;
    [SerializeField] float m_destroyDelay = 1.0f;

    private void Start()
    {
        CharacterStats stats = GetComponent<CharacterStats>();
        if(stats != null)
        {
            stats.ChangeStatEvent += OnChangeStat;
        }
    }
    public void OnChangeStat(ChangeStatEventData data)
    {
        if(data.statName == "health" && data.finalValue <= 0)
        {
            Die();
        }
    }
    public void Die()
    {
        var data = new DieEventData();
        data.destroyDelay = m_destroyDelay;
        DieEvent?.Invoke(data);
        Destroy(gameObject, m_destroyDelay);
    }
}