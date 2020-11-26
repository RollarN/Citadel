using UnityEngine;

public class WorldDialogData
{
    private Transform m_Parent = null;
    private Vector3 m_Offset = default;
    private DialogStyle m_DialogStyle = default;
    private string m_Title = string.Empty;
    private string m_Text = string.Empty;
    private int m_ID = -1;

    public DialogStyle DialogStyle => m_DialogStyle;
    public Vector3 Offset => m_Offset;
    public string Title => m_Title;
    public string Text => m_Text;
    private int ID => m_ID;

    public WorldDialogData(Transform parent, Vector3 offset, DialogStyle style, string title, string text, int id)
    {
        m_Parent = parent;
        m_Offset = offset;
        m_DialogStyle = style;
        m_Title = title;
        m_Text = text;
        m_ID = id;
    }
}
