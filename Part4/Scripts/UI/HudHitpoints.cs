using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudHitpoints : MonoBehaviour
{

    [SerializeField] Sprite m_spriteFull;
    [SerializeField] Sprite m_spriteEmpty;
    [SerializeField] CharacterStats m_characterStats;

    List<GameObject> m_hitpointsObjects = new List<GameObject>();
    int m_hitpointMax;
    int m_hitpointCurrent;

    void Start()
    {
        SetCharacterStats(m_characterStats);
        SetMaxHitPoint(m_characterStats.GetStatMax("health"));
        SetCurrentHitPoint(m_characterStats.GetStat("health"));
    }
    public void SetCharacterStats(CharacterStats stats)
    {
        if (m_characterStats != null)
        {
            m_characterStats.ChangeStatEvent -= OnChangeStat;
        }
        m_characterStats = stats;
        m_characterStats.ChangeStatEvent += OnChangeStat;

    }
    private GameObject CreateHitpoint(int position, bool full)
    {
        GameObject hitpoint = new GameObject("Hitpoint" + position.ToString());
        hitpoint.AddComponent<CanvasRenderer>();
        Image image = hitpoint.AddComponent<Image>();
        if (full)
        {
            image.sprite = m_spriteFull;
        }
        else
        {
            image.sprite = m_spriteEmpty;
        }

        image.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        image.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        image.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        image.rectTransform.sizeDelta = new Vector2(m_spriteEmpty.rect.width, m_spriteEmpty.rect.height);
        image.rectTransform.localScale = new Vector3(8, 8, 8);

        return hitpoint;
    }
    public void OnChangeStat(ChangeStatEventData data)
    {
        if (data.statName != "health")
            return;

        SetCurrentHitPoint(data.finalValue);
    }
    private void SetMaxHitPoint(int maxValue)
    {
        m_hitpointMax = maxValue;
        foreach (GameObject hitpoint in m_hitpointsObjects)
        {
            Destroy(hitpoint);
        }
        m_hitpointsObjects.Clear();

        for (int i = 0; i < m_hitpointMax; i++)
        {
            bool full = i < m_hitpointCurrent;
            GameObject hitpoint = CreateHitpoint(i, full);
            var instance = GameObject.Instantiate(hitpoint, new Vector3(0, 0, 0), Quaternion.identity, transform);
            Destroy(hitpoint);
            instance.GetComponent<Image>().rectTransform.anchoredPosition = new Vector2(i * 90, 0);

            m_hitpointsObjects.Add(instance);
        }
    }
    private void SetCurrentHitPoint(int currentValue)
    {
        m_hitpointCurrent = currentValue;
        int i = 0;
        foreach (GameObject hitpoint in m_hitpointsObjects)
        {
            if (i < currentValue)
                hitpoint.GetComponent<Image>().sprite = m_spriteFull;
            else
                hitpoint.GetComponent<Image>().sprite = m_spriteEmpty;
            i++;
        }
    }
}
