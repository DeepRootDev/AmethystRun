using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class GhostDataManager : MonoBehaviour
{
    public static void SaveToXML(string filePath, GhostData ghostData)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(GhostData));
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            serializer.Serialize(writer, ghostData);
        }
    }

    public static GhostData LoadFromXML(string filePath)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(GhostData));
        using (StreamReader reader = new StreamReader(filePath))
        {
            return (GhostData)serializer.Deserialize(reader);
        }
    }
}
