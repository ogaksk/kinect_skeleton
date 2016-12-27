using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Windows.Kinect;
using Newtonsoft.Json;
using System.Linq;

public class UserModel
{
    public int UserID { get; set; }
    public string Username { get; set; }
}

public class EmitBody
{
    public PointF Lean { get; set; }
    public bool IsTracked { get; set; }
    public ulong TrackingId { get;  set; }
    public Dictionary<JointType, Windows.Kinect.Joint> Joints { get; set; }

}

 public enum DJointType 
 {
    SpineBase = 0,
    SpineMid,
    Neck,
    Head,
    ShoulderLeft,
    ElbowLeft,
    WristLeft,
    HandLeft,
    ShoulderRight,
    ElbowRight,
    WristRight,
    HandRight,
    HipLeft,
    KneeLeft,
    AnkleLeft,
    FootLeft, 
    HipRight,
    KneeRight,
    AnkleRight,
    FootRight,
    SpineShoulder,
    HandTipLeft,
    ThumbLeft,
    HandTipRight,
    ThumbRight
}

public struct DJoint 
{
    public DJointType JointType { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 TrackingState { get; set; }
}


public class BodySourcePlayer : MonoBehaviour 
{
    private KinectSensor _Sensor;
    private BodyFrameReader _Reader;
    private Body[] _Data = null;
    private int counter = 0;

    public System.IO.FileStream _File;

    public Body[] GetData()
    {
        return _Data;
    }
    
    private EmitBody[] _EData = null;
    public string _Path =  System.IO.Directory.GetCurrentDirectory() + "//SerializationDummy.json";  
    static List<Body> _trackedBodies;
    public Body[] _bodies { get; set; }
    public EmitBody[] _eBodies  { get; set; }
    public EmitBody[] EGetData()  
    { 
        return _EData;
    }


    void Start () 
    {
        /*
        var data = new UserModel();
        data.UserID = 100;
        data.Username = "太郎";

        string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        UserModel model1 = JsonConvert.DeserializeObject<UserModel>(json);
        Debug.Log(JsonConvert.DeserializeObject<UserModel>(json));
        */

        if (_bodies == null)
        {
            string json = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "//SerializationOverview.json");
        //    _testbody = new Body[1];

            _eBodies = JsonConvert.DeserializeObject<EmitBody[]>(json);
            // _bodies = JsonConvert.DeserializeObject<Body[]>(json);
           // var _testbody1 = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(_Path));
            Debug.Log(_eBodies[0].Joints[0].Position.X);
        }   
    }
    
    void Update () 
    {

        if (_eBodies != null)
        {
            _EData = _eBodies;
            /*
            PropertyInfo[] infoArray = _testbody.GetType().GetProperties();
            foreach (PropertyInfo info in infoArray)
            {
                Debug.Log(info.Name + ": " + info.GetValue(_testbody,null));
            }
            */

            // Debug.Log(_testbody[0].Lean.X);
        }   
        /*
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
        */  
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
    
}



/*

            PropertyInfo[] infoArray = _testbody.GetType().GetProperties();
            foreach (PropertyInfo info in infoArray)
            {
                Debug.Log(info.Name + ": " + info.GetValue(_testbody,null));
            }

*/