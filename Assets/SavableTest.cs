using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SavableTest : MonoBehaviour, ISavable
{
    public float FloatTest = 0f;
    public string StringTest = string.Empty;
    public bool BoolTest = false;
    public int IntTest = 0;
    public bool ObjectActiveOnSave { get; set; }
    

    public void OnSave(StreamWriter sw, out int addedCount)
    {
        sw.WriteLine(FloatTest);
        sw.WriteLine(StringTest);
        sw.WriteLine(BoolTest);
        sw.WriteLine(IntTest);
        addedCount = 4;
    }

    public void OnLoad(string[] savedData, int startIndex)
    {
        FloatTest = float.Parse(savedData[startIndex]);
        StringTest = savedData[++startIndex];
        BoolTest = bool.Parse(savedData[++startIndex]);
        IntTest = int.Parse(savedData[++startIndex]);
        gameObject.SetActive(ObjectActiveOnSave);
    }
}
