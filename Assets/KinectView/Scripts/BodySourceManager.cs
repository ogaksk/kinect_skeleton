using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using  System.IO;
using Windows.Kinect;
using Newtonsoft.Json;
using System.Linq;


public class BodySourceManager : MonoBehaviour 
{
    private KinectSensor _Sensor;
    private BodyFrameReader _Reader;
    private Body[] _Data = null;

    public Body[] GetData()
    {
        return _Data;
    }
    
    public string _Path =  System.IO.Directory.GetCurrentDirectory() + "//SerializationOverview.json";  
    static List<Body> trackedBodies = new List<Body>();
    public System.IO.FileStream _File;

    void Start () 
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.BodyFrameSource.OpenReader();
            
            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }   
    }
    
    void Update () 
    {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            
            if (frame != null)
            {
                if (_Data == null)
                {
                    _Data = new Body[_Sensor.BodyFrameSource.BodyCount];
                }
                
                frame.GetAndRefreshBodyData(_Data);
                
                SaveJsonData();
                
                frame.Dispose();
                frame = null;
            }
        }    
    }
    
    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }
        
        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }
            
            _Sensor = null;
        }
        if (_File != null)
        {
            _File.Close();
        }
        
    }

    void SaveJsonData()
    {
        trackedBodies = _Data.Where(b => b.IsTracked == true).ToList();
        if (trackedBodies.Count() > 0) {
            string kinectBodyDataString = JsonConvert.SerializeObject(trackedBodies);
            using (StreamWriter file = File.CreateText(_Path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, kinectBodyDataString);
            }
        }
        
    }
    
}
