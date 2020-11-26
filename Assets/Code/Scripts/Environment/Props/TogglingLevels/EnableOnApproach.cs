using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class EnableOnApproach : MonoBehaviour
{
    [SerializeField] private float m_DetectionRadius = 100f;
    [SerializeField] [HideInInspector] private float m_SqrDetectionRadius = 10000f;
    [SerializeField] private Vector3 m_DetectionOffset = Vector3.zero;
    [SerializeField] List<GameObject> m_ChildrenToIgnore = new List<GameObject>();
    [SerializeField] GameManager m_GameManager = null;

    private bool m_LevelEnabled = true;


    public float DetectionRadius => m_DetectionRadius;
    public Vector3 DetectionOffset => m_DetectionOffset;
    public bool LevelEnabled => m_LevelEnabled;

    private void Start()
    {
        InvokeRepeating(nameof(LoadLevelIfCloseEnough), 0, 1);
    }

    private void LoadLevelIfCloseEnough()
    {
        if (ShouldLevelLoad())
        {
            if (!m_LevelEnabled)
            {
                ToggleLevel(true);
            }
        }
        else
        {
            if (m_LevelEnabled)
            {
                ToggleLevel(false);
            }
        }
    }

    public void ToggleLevel(bool activate)
    {
        foreach (Transform child in transform)
        {
            if (!m_ChildrenToIgnore.Contains(child.gameObject))
            {
                child.gameObject.SetActive(activate);
            }
        }
        m_LevelEnabled = activate;
    }

    private bool ShouldLevelLoad()
    {
        return (m_GameManager.Player.transform.position - (transform.position + m_DetectionOffset)).sqrMagnitude < m_SqrDetectionRadius;
    }

    private void OnValidate()
    {
        m_SqrDetectionRadius = m_DetectionRadius * m_DetectionRadius;
    }
}
