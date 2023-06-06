using UnityEngine;

public class LevelSpawn : MonoBehaviour
{
    [SerializeField] GameObject m_characterToSpawn;
    [SerializeField] bool m_respawn = true;
    private float m_timer = -1f;
    private float m_delay;
    private GameObject m_characterInstance;

    public void Spawn()
    {
        m_characterInstance = GameObject.Instantiate(m_characterToSpawn, transform.position, Quaternion.identity);
        if(m_respawn)
        {
            var dieable = m_characterInstance.GetComponent<Dieable>();
            if(dieable != null)
            {
                dieable.DieEvent += OnDie;
            }
        }
    }
    public void OnDie(DieEventData data)
    {
        if(m_respawn)
        {
            m_timer = data.destroyDelay;
        }
    }
    private void Awake()
    {
        Spawn();
    }
    private void Update()
    {
        if (m_timer > 0f)
        {
            m_timer -= Time.deltaTime;
            if (m_timer <= 0f)
            {
                Spawn();
            }
        }
    }
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "player_spawn_gizmo.png", true);
    }
    #endif
}