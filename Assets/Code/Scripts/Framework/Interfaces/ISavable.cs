using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public interface ISavable
{
    bool ObjectActiveOnSave { get; set; }
    void OnSave(StreamWriter sw, out int addedCount);
    void OnLoad(string[] savedData, int startIndex);
}
