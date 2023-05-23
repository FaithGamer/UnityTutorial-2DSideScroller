using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtingSurface : MonoBehaviour
{
    [SerializeField] HurtingParameters m_params;
    List<Hurtable> m_insideSurface = new List<Hurtable>();

    private void Update()
    {
        foreach (Hurtable hurtable in m_insideSurface)
        {
            if (!hurtable.isGraced)
            {
                Hurt(hurtable);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D obj)
    {
        var hurtable = obj.GetComponent<Hurtable>();
        if (hurtable == null)
            return;

        m_insideSurface.Add(hurtable);
        if (!hurtable.isGraced)
            Hurt(hurtable);
    }
    private void OnTriggerExit2D(Collider2D obj)
    {
        var hurtable = obj.GetComponent<Hurtable>();
        if (hurtable == null)
            return;

        m_insideSurface.Remove(hurtable);
    }
    private void Hurt(Hurtable hurtable)
    {
        var data = new HurtEventData();
        data.whosHurting = this.gameObject;
        data.parameters = m_params;
        hurtable.Hurt(data);
    }


}
