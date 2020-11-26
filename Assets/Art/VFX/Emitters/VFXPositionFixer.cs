using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXPositionFixer : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Transform[] m_PfxSystemsToMove;
    [SerializeField] private LayerMask m_GroundLayers;
    [SerializeField] private float maxHeightDifference;
    Ray ray = new Ray();
    RaycastHit hit;
    private void OnEnable()
    {
        if (m_PfxSystemsToMove == null || m_PfxSystemsToMove.Length == 0 || m_PfxSystemsToMove.Equals(null))
            return;

        Vector3 targetPos;
        ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out hit, maxHeightDifference, m_GroundLayers, QueryTriggerInteraction.Ignore))
            targetPos = hit.point + Vector3.up * 0.05f;
        else
            targetPos = ray.GetPoint(maxHeightDifference);

        for (int i = 0; i < m_PfxSystemsToMove.Length; i++) 
            m_PfxSystemsToMove[i].transform.position = targetPos;
        
    }
}
