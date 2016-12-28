using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using  System.IO;
using Windows.Kinect;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;


public class BodySourceManager : MonoBehaviour 
{
    private KinectSensor _Sensor;
    private BodyFrameReader _Reader;
    private Body[] _Data = null;

    private JsonSerializer serializer = new JsonSerializer();
    private bool saveFlag = false;


    public Body[] GetData()
    {
        return _Data;
    }
    
    public string _Path = null;  
    static List<Body> trackedBodies = new List<Body>();
    public System.IO.FileStream _File;
    public StreamWriter _file;
    public JsonWriter _jw;

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

        /* key's vinding for save json. */    
        if (Input.GetKeyDown(KeyCode.Space)) {
           saveFlag = !saveFlag;
           Debug.Log("saveFlag=: "+saveFlag);
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
        if (_file != null)
        {
            _file.Close();
        }
        
    }

    void SaveJsonData()
    {
        if (!saveFlag) 
        {
            return;
        } 
        Debug.Log("saving..");

        string kinectBodyDataString = null;
        JArray jarray = null;
        trackedBodies = _Data.Where(b => b.IsTracked == true).ToList();
        
        if (trackedBodies.Count() > 0) {
            kinectBodyDataString = JsonConvert.SerializeObject(trackedBodies);
            jarray = JArray.Parse(kinectBodyDataString);
            foreach (JObject content in jarray.Children<JObject>())
            {
                removeFields(content, new string[] { "JointOrientations" });
            } 
        }

        using (FileStream fs = File.Open(ReturnPath(), FileMode.Append))
        using (StreamWriter sw = new StreamWriter(fs))
        using (JsonWriter jw = new JsonTextWriter(sw))
        {
            // TODO: seperate file for future.
            Debug.Log(sw);

            jw.Formatting = Formatting.Indented;

            JsonSerializer serializer = new JsonSerializer();

            Dictionary<uint, JArray> ret = new Dictionary<uint, JArray>();
            ret.Add(ReturnTimeStamp(0), jarray);
            serializer.Serialize(jw, ret);
        }
    }

    // check: ここをうまく使えば時間ごとにjsonを切り出せる
    /*
    // state: 0 => millisec, 1 => sec, 2, => min
    */
    static uint ReturnTimeStamp(int state)
    {   
        uint retInt = 0;
        switch (state) 
        {
            case 0:
                retInt = (uint)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalMilliseconds;
            break;
            case 1:
                retInt = (uint)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
            break;
            case 2:
                retInt = (uint)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalMinutes;
            break;
        }
        return retInt;
    } 

    static string ReturnPath()
    {
        return  System.IO.Directory.GetCurrentDirectory() + "//SerializationOverview"+ ReturnTimeStamp(2) +".json" ;
    }

    /// util ///
    private void removeFields(JToken token, string[] fields)
    {
        JContainer container = token as JContainer;
        if (container == null) return;

        List<JToken> removeList = new List<JToken>();
        foreach (JToken el in container.Children())
        {
            JProperty p = el as JProperty;
            if (p != null && fields.Contains(p.Name))
            {
                removeList.Add(el);
            }
            removeFields(el, fields);
        }

        foreach (JToken el in removeList)
        {
            el.Remove();
        }
    }
}
