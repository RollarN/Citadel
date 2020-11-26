using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResetTransformAndVelocityOnLoad : MonoBehaviour, ISavable
{
    public bool ObjectActiveOnSave { get; set; }

    public void OnSave(StreamWriter sw, out int addedCount)
    {
        sw.WriteLine(transform.position.Serialize());
        sw.WriteLine(transform.rotation.Serialize());

        addedCount = 2;
    }
    public void OnLoad(string[] savedData, int startIndex)
    {
        transform.position = savedData[startIndex++].DeserializeToVector3();
        transform.rotation = savedData[startIndex++].DeserializeToQuaternion();

        gameObject.SetActive(ObjectActiveOnSave);
    }
}
