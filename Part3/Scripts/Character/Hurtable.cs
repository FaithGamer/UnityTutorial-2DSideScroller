using UnityEngine;

public struct HurtEventData
{
    public Hurtable whosHurt;
    public GameObject whosHurting;
    public HurtingParameters parameters;
}
public class Hurtable : MonoBehaviour
{
    public delegate void OnHurtEventData(HurtEventData data);
    public event OnHurtEventData HurtEvent;
    public event OnHurtEventData EnterGraceEvent;
    public event OnHurtEventData ExitGraceEvent;

    public bool isGraced
    {
        get
        {
            return m_graceTimer <= m_graceTime;
        }
        private set { }
    }

    private float m_graceTime = 1.2f;
    private float m_graceTimer = 999f;
    private HurtEventData m_lastHurtData;

    public void Hurt(HurtEventData data)
    {
        data.whosHurt = this;
        HurtEvent?.Invoke(data);
        if(m_graceTime > 0f)
        {
            m_lastHurtData = data;
            m_graceTimer = 0f;
            EnterGraceEvent?.Invoke(data);
        }
    }
    private void Update()
    {
        if(m_graceTimer > m_graceTime)
            return;
        m_graceTimer += Time.deltaTime;
        if(m_graceTimer > m_graceTime)
        {
            ExitGraceEvent?.Invoke(m_lastHurtData);
        }
    }
    
}