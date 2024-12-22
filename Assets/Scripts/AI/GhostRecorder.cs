using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class GhostRecorder : MonoBehaviour
{
    public GhostData ghostData = new GhostData();
    public float recordInterval = 1f; 
    private float timer = 0f;
    public string saveFilePath = "Assets/ghost_data.xml"; 
    [SerializeField]
    PlayerMovement mvnt;
    public int currentPathID = 0;
    public bool begin;
    private void Start()
    {
        mvnt = GetComponent<PlayerMovement>();
    }
    void Update()
    {
        if (begin)
        {
            timer += Time.deltaTime;

            if (timer >= recordInterval)
            {
                RecordFrame();
                timer = 0f;
            }
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            begin = !begin;
            if (begin)
            {
                StartNewPath();
                print("Ghost Recording Has Begun on Path " + currentPathID);
            }
            else
            {
                SaveGhostData();
                print("Ghost Recording Has Ended on Path " + currentPathID);
                currentPathID++;
            }
            
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            
            print("Path " + currentPathID);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            SaveGhostData();
            begin = false;
        }
    }
    public void StartNewPath()
    {
        GhostPath newPath = new GhostPath { pathID = currentPathID};
        ghostData.paths.Add(newPath);
    }
    public void RecordFrame()
    {
        GhostPath currentPath = ghostData.paths.Find(path => path.pathID == currentPathID);
        if (currentPath != null)
        {
            GhostFrame frame = new GhostFrame
            {
                position = transform.position,
                rotation = transform.rotation,
                isGliding = mvnt.isGliding
            };
            currentPath.frames.Add(frame);
        }
        else
        {
            print("Not Found");
        }
    }
    public void SaveGhostData()
    {
        GhostDataManager.SaveToXML(saveFilePath, ghostData);
        Debug.Log("Ghost data saved to: " + saveFilePath);
    }

    public void LoadGhostData()
    {
        ghostData = GhostDataManager.LoadFromXML(saveFilePath);
        Debug.Log("Ghost data loaded from: " + saveFilePath);
    }
  
}
[System.Serializable]
public class GhostFrame
{
    public Vector3 position;
    public Quaternion rotation;
    public bool isGliding;
}
[Serializable]
public class GhostPath
{
    [XmlAttribute("PathID")]
    public int pathID;

    [XmlElement("Frame")]
    public List<GhostFrame> frames = new List<GhostFrame>();
}
[Serializable]
[XmlRoot("GhostData")]
public class GhostData
{
    [XmlElement("Path")]
    public List<GhostPath> paths = new List<GhostPath>();
}

