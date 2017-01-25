using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

using System.IO;
using Newtonsoft.Json;
using System.Linq;

public class BodySourcePlayerView : MonoBehaviour 
{
    public Material BoneMaterial;
    public GameObject BodySourcePlayer;
    
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourcePlayer _BodyManager;

    public string _Path =  System.IO.Directory.GetCurrentDirectory() + "//SerializationConfirm.json";

    public int RotationCoef = 0;
    private Vector3 RotationPivot = new Vector3(0, 1, 0);
    private Vector3 groundPosition;

    private Windows.Kinect.Vector4 _floorData;
    public double _cameraAngle;
    
    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };
    
    void Update () 
    {
        if (BodySourcePlayer == null)
        {
            return;
        }
        _BodyManager = BodySourcePlayer.GetComponent<BodySourcePlayer>();
        if (_BodyManager == null)
        {
            return;
        }
        
        EmitBody[] data = _BodyManager.EGetData();
        _floorData = _BodyManager.FloorPlane;

        if (data == null)
        {
            return;
        }

        List<ulong> trackedIds = new List<ulong>();
        groundPosition =  transform.position;

        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
              }
                
            if(body.IsTracked)
            {   
                trackedIds.Add (body.TrackingId);
            }
        }
        
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
        // First delete untracked bodies
        foreach(ulong trackingId in knownIds)
        {
            if(!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }

        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
            }
            
            if(body.IsTracked)
            {
                if(!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }
                RefreshBodyObject(body, _Bodies[body.TrackingId]);
            }
        }
    }
    
    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);
        
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.SetVertexCount(2);
            lr.material = BoneMaterial;
            lr.SetWidth(0.05f, 0.05f);
            
            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
        }
        
        return body;
    }
    
    private void RefreshBodyObject(EmitBody body, GameObject bodyObject)
    {
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;
            
            if(_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
            }
            
            Transform jointObj = bodyObject.transform.FindChild(jt.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint, groundPosition);
            jointObj.RotateAround(RotationPivot, transform.up, RotationCoef);
            
            LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if(targetJoint.HasValue)
            {
                lr.SetPosition(0, jointObj.localPosition);

                Vector3 endpoint = RotateAroundPoint(GetVector3FromJoint(targetJoint.Value, groundPosition), RotationPivot, Quaternion.Euler(0, RotationCoef, 0));
                lr.SetPosition(1, endpoint);
                lr.SetColors(GetColorForState (sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
            }
            else
            {
                lr.enabled = false;
            }
        }
    }
    
    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
        case Kinect.TrackingState.Tracked:
            return Color.green;

        case Kinect.TrackingState.Inferred:
            return Color.red;

        default:
            return Color.black;
        }
    }
    
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint,  Vector3 groundPosition)
    {
        return new Vector3(
            (joint.Position.X * 10) + groundPosition.x, 
            (joint.Position.Y * 10) + groundPosition.y, 
            (joint.Position.Z * 10) + groundPosition.z
        );
    }
    
    private static Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion angle) 
    {
     return angle * ( point - pivot) + pivot;
    }

    private static Vector3 GetFloorClipPlane (EJoint jointhead, Windows.Kinect.Vector4 _floor) 
    {
        return new Vector3(
            (float)0,
            (float)-(_floor.X * jointhead.Position.X + _floor.Z * jointhead.Position.Z + _floor.W) / _floor.Y * 10,
            (float)0
        );
    }


    private static double getCameraAngle (Windows.Kinect.Vector4 _floor) 
    {
        double cameraAngleRadians = System.Math.Atan(_floor.Z / _floor.Y); 
        // return System.Math.Cos(cameraAngleRadians); 
        return cameraAngleRadians * 180 / System.Math.PI;
    }

    void OnGUI () 
    {
        // テキストフィールドを表示する
        GUI.TextField(new Rect(10, 10, 300, 50), _cameraAngle.ToString());
        GUI.TextField(new Rect(10, 10, 300, 50), _cameraAngle.ToString());
    }
}
