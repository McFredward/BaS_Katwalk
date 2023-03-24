using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class KATXRWalker : MonoBehaviour
{
    [Range(0.5f,10.0f)]
    public float speedMul = 1.0f;

    public GameObject xr;
    public GameObject eye;

    public enum ExecuteMethod
    {
        RigidBody,
        CharactorController,
        MovePosition
    }

    public ExecuteMethod executeMethod = ExecuteMethod.RigidBody;

    protected Vector3 lastPosition = Vector3.zero;
    //protected Vector3 defaultEyeOffset = Vector3.zero;
    //Unity is the left-handed coordinate system. The yaw value from the SDK should be assigned to the y-axis of the object
    protected float yawCorrection;

    void Start()
    {
        
    }

   
    void FixedUpdate()
    {
         
        var ws = KATNativeSDK.GetWalkStatus();
        var device = LocoSExtraData.GetExtraInfoLoco(ws);
        //ÓÒ½Åpitch
        //Debug.Log(device.R_Pitch);
        //×ó½Åpitch
        Debug.Log(device.L_Pitch);

        if (!ws.connected)
        {
            return;
        }

        //Calibration Stage 
        var lastCalibrationTime = KATNativeSDK.GetLastCalibratedTimeEscaped();                 //Get last calibration time as double

        //DeviceData [0] is the calibration button. After pressing the calibration button, the head display direction will be positive by default
        //Check if need calibration

     


        if (ws.deviceDatas[0].btnPressed || lastCalibrationTime < 0.08)                                               
        {
            var hmdYaw = eye.transform.eulerAngles.y;
            //Here we get the Euler angle converted from quaternion
            var bodyYaw = ws.bodyRotationRaw.eulerAngles.y;

            yawCorrection = bodyYaw - hmdYaw;

            //Pos is the position of the current prefabricated body
            var pos = transform.position;
            var eyePos = eye.transform.position;
            pos.x = eyePos.x;
            pos.z = eyePos.z;
            transform.position = pos;
            lastPosition = transform.position;
            return;
        }

        transform.rotation = ws.bodyRotationRaw * Quaternion.Inverse( Quaternion.Euler(new Vector3(0,yawCorrection,0)));
        
      

        switch(executeMethod)
        {
            case ExecuteMethod.CharactorController: 
                {
                    var ch = GetComponent<CharacterController>();
                    ch.SimpleMove(transform.rotation * ws.moveSpeed);
                }
                break;
            case ExecuteMethod.MovePosition:
                {
                    transform.position += (transform.rotation * ws.moveSpeed * Time.fixedDeltaTime);
                }
                break;
            case ExecuteMethod.RigidBody:
                {
                    var r = GetComponent<Rigidbody>();
                    r.velocity = transform.rotation * ws.moveSpeed;
                }
                break;
        } 
    }


    //
    void LateUpdate()
    {
        var offset = transform.position - lastPosition;
        offset.y = 0;
        xr.transform.position += offset;

        lastPosition = transform.position;
    }
}
