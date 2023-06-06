using UnityEngine;

public struct PlayerObjectChangedEventData
{
    public GameObject instance;
}
public class PlayerFinder : MonoBehaviour
{
    public delegate void OnPlayerObjectChanged(PlayerObjectChangedEventData data);
    public event OnPlayerObjectChanged PlayerObjectChangedEvent;
    private GameObject m_lastInstance = null;

    private void Update()
    {
        if(m_lastInstance != null)
            return;

        var player = GameObject.FindWithTag("Player");
        if(player != null && player != m_lastInstance)
        {
            m_lastInstance = player;
            var data = new PlayerObjectChangedEventData();
            data.instance = player;
            PlayerObjectChangedEvent?.Invoke(data);
        }
    }
}