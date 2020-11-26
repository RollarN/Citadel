using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider))]
public class EndingAnimation : MonoBehaviour
{
    [SerializeField] private Camera m_Camera = null;
    private float m_SourceHeight = 2f;
    [SerializeField] Transform m_TargetTransform = null;
    private float m_Speed = 3f;
    private float m_StartTime = 0f;
    [SerializeField] private float m_TimeToTarget = 6f;
    [SerializeField] private float m_RotationSpeed = 10f;
    private Vector3 m_StartPosition;
    [SerializeField] private PlayerController m_PlayerController = null;
    private bool m_Animating = false;
    [SerializeField] private GameObject m_EndPainting = null;
    [SerializeField] private MeshRenderer m_EndPaintingMeshRenderer = null;

    [SerializeField] private LayerMask m_PlayerLayer = default;
    [SerializeField] private Material m_ToMaterial = null;
    [SerializeField] private Material m_FromMaterial = null;

    private bool m_ChangingMaterial = false;
    private float m_StartMaterialChangeTime = 0f;
    private float m_ChangeMaterialDuration = 2f;


    private MaterialPropertyBlock m_Mpb = null;

    private bool m_MoveBackFromPaintingAnimation = false;
    private float m_TimeAtMoveBack = 0f;
    [SerializeField] private float m_MoveBackDuration = 4f;
    [SerializeField] private Transform m_TargetMoveBackTransform = null;

    [SerializeField] private Image m_EndingImage = null;
    [SerializeField] private float m_EndingImageFadeInDuration = 2.7f;
    private bool m_FadeInImage = false;
    private float m_TimeAtFadeIn = 0f;

    [SerializeField] private Text m_Credits = null;
    [SerializeField] private float m_ScrollSpeed = 10f;
    private bool m_ScrollCredits = false;

    private bool m_FadeOutImage = false;
    [SerializeField] private float m_EndingImageFadeOutDuration = 2.7f;
    private float m_TimeAtFadeOut = 0f;

    private void Awake()
    {
        m_Mpb = new MaterialPropertyBlock();
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Animating && Time.time - m_StartTime < m_TimeToTarget)
        {
            float step = (Time.time - m_StartTime) / m_TimeToTarget;
            Vector3 targetPosition = transform.position
                                        + (m_TargetTransform.position - transform.position) * step;
            m_Camera.transform.position = m_StartPosition + (targetPosition - m_StartPosition) * step;

            m_Camera.transform.rotation = Quaternion.RotateTowards(m_Camera.transform.rotation,
                Quaternion.LookRotation(m_EndPainting.transform.position - m_Camera.transform.position, Vector3.up), m_RotationSpeed * Time.deltaTime);
        }
        else if(m_Animating && Time.time - m_StartTime >= m_TimeToTarget)
        {
            m_Camera.transform.position = m_TargetTransform.position;
            m_Camera.transform.rotation = Quaternion.LookRotation(m_EndPainting.transform.position - m_Camera.transform.position, Vector3.up);
            //m_EndPaintingMeshRenderer.materials[1] = m_EndPaintingNewMaterial;
            m_StartMaterialChangeTime = Time.time;
            m_Animating = false;
            m_ChangingMaterial = true;
        }

        if(m_ChangingMaterial && Time.time - m_StartMaterialChangeTime < m_ChangeMaterialDuration)
        {
            float step = (Time.time - m_StartMaterialChangeTime) / m_ChangeMaterialDuration;
            m_Mpb.SetFloat("_LerpValue", step);
            m_EndPaintingMeshRenderer.SetPropertyBlock(m_Mpb, 1);
        }
        else if(m_ChangingMaterial && Time.time - m_StartMaterialChangeTime >= m_ChangeMaterialDuration)
        {
            m_Mpb.SetFloat("_LerpValue", 1f);
            m_EndPaintingMeshRenderer.SetPropertyBlock(m_Mpb, 1);
            m_TimeAtMoveBack = Time.time;
            m_ChangingMaterial = false;
            m_MoveBackFromPaintingAnimation = true;
        }

        if (m_MoveBackFromPaintingAnimation && Time.time - m_TimeAtMoveBack < m_MoveBackDuration)
        {
            float step = (Time.time - m_TimeAtMoveBack) / m_MoveBackDuration;
            m_Camera.transform.position = m_TargetMoveBackTransform.position
                                            + (m_TargetMoveBackTransform.position - m_TargetTransform.position) * step;
        }
        else if(m_MoveBackFromPaintingAnimation && Time.time - m_TimeAtMoveBack >= m_MoveBackDuration)
        {
            m_TimeAtFadeIn = Time.time;
            m_MoveBackFromPaintingAnimation = false;
            m_FadeInImage = true;
            m_EndingImage.gameObject.SetActive(true);
        }

        if (m_FadeInImage && Time.time - m_TimeAtFadeIn < m_EndingImageFadeInDuration)
        {
            float step = (Time.time - m_TimeAtFadeIn) / m_EndingImageFadeInDuration;
            Color color = m_EndingImage.color;
            color.a = step;
            m_EndingImage.color = color;
        }
        else if (m_FadeInImage && Time.time - m_TimeAtFadeIn >= m_EndingImageFadeInDuration)
        {
            Color color = m_EndingImage.color;
            color.a = 1f;
            m_TimeAtFadeOut = Time.time;
            m_EndingImage.color = color;
            m_FadeInImage = false;
            m_FadeOutImage = true;
        }

        if(m_FadeOutImage && Time.time - m_TimeAtFadeOut < m_EndingImageFadeOutDuration)
        {
            float step = (Time.time - m_TimeAtFadeOut) / m_EndingImageFadeOutDuration;
            Color color = m_EndingImage.color;
            color.r = 1 - step;
            color.g = 1 - step;
            color.b = 1 - step;
            m_EndingImage.color = color;
            
        }
        else if(m_FadeOutImage && Time.time - m_TimeAtFadeOut >= m_EndingImageFadeOutDuration)
        {
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }

        //if (m_ScrollCredits)
        //{
        //    Vector3 newPosition = m_Credits.rectTransform.anchoredPosition;
        //    newPosition.y -= m_ScrollSpeed * Time.deltaTime;
        //    m_Credits.rectTransform.anchoredPosition = newPosition;
        //}
    }

    

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & m_PlayerLayer) != 0)
        {
            m_Camera.GetComponent<CameraMovement>().enabled = false;
            m_PlayerController.enabled = false;
            m_StartTime = Time.time;
            m_StartPosition = m_Camera.transform.position;
            m_Animating = true;
            GetComponent<BoxCollider>().enabled = false;
            m_PlayerController.GetComponent<Animator>().enabled = false;
        }
    }
}
