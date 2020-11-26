using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class HUDManager : MonoBehaviour
{
    [Header("Bars")]
    public Image HealthImage;
    public Image FireImage;
    public Image PoisonImage;
    public Image IceImage;
    public Image LightningImage;
    public Image ChargeImage;
    public Image DashImage;
    public Image PushBackImage;

    [Header("Old Other HUD things")]
    
    public GameObject LegendPanel;
    public GameObject TabPanel;


    [Header("Mouse things")]
    [SerializeField] private Texture2D m_MouseTexture;
    [SerializeField] private CursorMode m_CursorMode = CursorMode.Auto;
    [SerializeField] private Vector2 m_HotSpot = Vector2.zero;

    public Image GetResourceImage(ElementType elementType)
    {
        switch (elementType)
        {
            case ElementType.Fire:
                return FireImage;
            case ElementType.Ice:
                return IceImage;
            case ElementType.Poison:
                return PoisonImage;
            case ElementType.Lightning:
                return LightningImage;
            default: return null;
        }
    }

    public void HighlightResourceImage(ElementType elementType)
    {
        FireImage.transform.GetChild(0).GetComponent<Image>().enabled = false;
        PoisonImage.transform.GetChild(0).GetComponent<Image>().enabled = false;
        IceImage.transform.GetChild(0).GetComponent<Image>().enabled = false;
        LightningImage.transform.GetChild(0).GetComponent<Image>().enabled = false;
        GetResourceImage(elementType).transform.GetChild(0).GetComponent<Image>().enabled = true;
    }

    public void Start()
    {
        TabPanel.SetActive(true);

        Cursor.SetCursor(m_MouseTexture, m_HotSpot, m_CursorMode);
    }

    private void OnMouseEnter()
    {
        Cursor.SetCursor(m_MouseTexture, m_HotSpot, m_CursorMode);
    }
    private void OnMouseExit()
    {
        Cursor.SetCursor(null, Vector2.zero, m_CursorMode);
    }



    public void Update()
    {
        LegendPanel.SetActive(Input.GetKey(KeyCode.Tab));
        if (Input.GetKeyDown(KeyCode.Y))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
