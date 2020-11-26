using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveOnTrigger : MonoBehaviour
{
    [SerializeField] private SaveManager m_SaveManager = null;
    [SerializeField] private LayerMask m_LayerToTriggerSaving = default;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & m_LayerToTriggerSaving) != 0)
        {

            m_SaveManager.SaveGame();
            gameObject.SetActive(false);
        }
    }
}
