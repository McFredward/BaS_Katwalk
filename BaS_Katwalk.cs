﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThunderRoad;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using HarmonyLib;
using static KATNativeSDK;
using System.Collections;
using System.Reflection;
using System.IO;
using UnityEngine.Events;
using System.Data;
using System.Timers;

namespace BaS_Katwalk
{
    public class Main : ThunderScript
    {

        
        #region Public Parameters
        public static ModOptionFloat[] speedMulOption = {
            new ModOptionFloat("0.1", 0.1f),
            new ModOptionFloat("0.2", 0.2f),
            new ModOptionFloat("0.3", 0.3f),
            new ModOptionFloat("0.4", 0.4f),
            new ModOptionFloat("0.5", 0.5f),
            new ModOptionFloat("0.6", 0.6f),
            new ModOptionFloat("0.7", 0.7f),
            new ModOptionFloat("0.8", 0.8f),
            new ModOptionFloat("0.9", 0.9f),
            new ModOptionFloat("1", 1.0f)
        };

        public static ModOptionFloat[] speedMaxRangeOption = {
            new ModOptionFloat("0.5", 0.5f),
            new ModOptionFloat("0.75", 0.75f),
            new ModOptionFloat("1", 1.0f),
            new ModOptionFloat("1.25", 1.25f),
            new ModOptionFloat("1.5", 1.5f),
            new ModOptionFloat("1.75", 1.75f),
            new ModOptionFloat("2", 2.0f),
            new ModOptionFloat("2.25", 2.25f),
            new ModOptionFloat("2.5", 2.5f),
            new ModOptionFloat("2.75", 2.75f),
            new ModOptionFloat("3", 3.0f),
            new ModOptionFloat("3.25", 3.25f),
            new ModOptionFloat("3.5", 3.5f),
            new ModOptionFloat("3.75", 3.75f),
            new ModOptionFloat("4", 4.0f),
            new ModOptionFloat("4.25", 4.25f),
            new ModOptionFloat("4.5", 4.5f),
            new ModOptionFloat("4.75", 4.75f),
            new ModOptionFloat("5", 5.0f)
        };

        public static ModOptionFloat[] speedExponentOption = {
            new ModOptionFloat("0.5 (square root)", 0.5f),
            new ModOptionFloat("0.55", 0.55f),
            new ModOptionFloat("0.6", 0.6f),
            new ModOptionFloat("0.65", 0.65f),
            new ModOptionFloat("0.7", 0.7f),
            new ModOptionFloat("0.75", 0.75f),
            new ModOptionFloat("0.8", 0.8f),
            new ModOptionFloat("0.85", 0.85f),
            new ModOptionFloat("0.9", 0.9f),
            new ModOptionFloat("0.95", 0.95f),
            new ModOptionFloat("1 (linear)", 1.0f),
            new ModOptionFloat("1.05", 1.05f),
            new ModOptionFloat("1.1", 1.1f),
            new ModOptionFloat("1.15", 1.15f),
            new ModOptionFloat("1.2", 1.2f),
            new ModOptionFloat("1.25", 1.25f),
            new ModOptionFloat("1.3", 1.3f),
            new ModOptionFloat("1.35", 1.35f),
            new ModOptionFloat("1.4", 1.4f),
            new ModOptionFloat("1.45", 1.45f),
            new ModOptionFloat("1.5", 1.5f),
            new ModOptionFloat("1.55", 1.55f),
            new ModOptionFloat("1.6", 1.6f),
            new ModOptionFloat("1.65", 1.65f),
            new ModOptionFloat("1.7", 1.7f),
            new ModOptionFloat("1.75", 1.75f),
            new ModOptionFloat("1.8", 1.8f),
            new ModOptionFloat("1.85", 1.85f),
            new ModOptionFloat("1.9", 1.9f),
            new ModOptionFloat("1.95", 1.95f),
            new ModOptionFloat("2 (quadratic)", 2.0f)
        };

        public static ModOptionBool[] turningDisabledOption = {
            new ModOptionBool("Disabled", false),
            new ModOptionBool("Enabled", true)
        };

        #endregion



        #region Private Parameters

        [ModOption(name: "Speed Multiplicator", tooltip: "Modifies how the speed of the KatWalk is translated to the in-game speed", valueSourceName: nameof(speedMulOption), category = "Speed options",defaultValueIndex = 2)]
        private static float speedMul;

        [ModOption(name: "Speed max. Range", tooltip: "Modifies what maximal ingame speed value corresponds to the maximal KatWalk speed (5 m/s)", valueSourceName: nameof(speedMaxRangeOption), category = "Speed options", defaultValueIndex = 8)]
        private static float speedMaxRange;

        [ModOption(name: "Speed curve exponent", tooltip: "Modifies the speed curve which determines how fast the maximal speed is reached", valueSourceName: nameof(speedExponentOption), category = "Speed options", defaultValueIndex = 5)]
        private static float speedExponent;

        [ModOption(name: "joystick disabled", tooltip: "Disabling the Joystick-Movement", category = "Others")]
        private static bool joystick_disabled = false;

        [ModOption(name: "Truning disabled", tooltip: "Disabling Joystick-turn. Recommended, since you have to recenter every time after using the turn.", valueSourceName: nameof(turningDisabledOption), category ="Others", defaultValueIndex = 1)]
        private static bool turning_disabled; //default true



        private static float yawCorrection = 0.0f;
        private Harmony harmony;
        private static bool invoke_auto_calibrate = false;
        private static System.Timers.Timer invoke_auto_calibrate_time = new System.Timers.Timer();
        private static bool allow_recalibration = true;

        private void AutoCalibrateTimeElapsed(object sender, ElapsedEventArgs e)
        {
            Debug.Log("BaS_Katwalk: Invoke auto calibration");
            invoke_auto_calibrate = true;
        }

        #endregion

        public override void ScriptLoaded(ModManager.ModData modData)
        {
            EventManager.OnPlayerPrefabSpawned += new EventManager.PlayerPrefabSpawnedEvent(StartMod);
            base.ScriptLoaded(modData);
        }


        public void StartMod()
        {
            harmony = new Harmony("bas.katwalk");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            invoke_auto_calibrate_time.Interval = 2000; //2 seconds
            invoke_auto_calibrate_time.Elapsed += AutoCalibrateTimeElapsed;
            Debug.Log("BaS_Katwalk: All methods patched");
        }

        //MoveWeighted(Vector3 direction,Transform bodyTransform, float heightRatio,float runSpeedRatio, float acceleration)

        //--- Deactivate standart running by swinging arms ---

        [HarmonyPatch(typeof(PlayerControl), "Awake", new Type[] { })]
        public static class BaS_Awake
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerControl __instance)
            {
                __instance.currentRunSwingRatio = 0.0f;
            }
        }

        //Method returns a ratio dependend on how the arms a moved to simulate a "run"
        [HarmonyPatch(typeof(PlayerControl), "UpdateRunning", new Type[] { })]
        public static class BaS_UpdateRunning
        {
            [HarmonyPrefix]
            public static bool Prefix(PlayerControl __instance)
            {
                return false; //Bypass the whole function
            }
        }

        //Method returns a ratio dependend on how the arms a moved to simulate a "run"
        [HarmonyPatch(typeof(LevelManager), "LoadLevelCoroutine", new Type[] { typeof(LevelData), typeof(LevelData.Mode), typeof(Dictionary<string,string>) })]
        public static class BaS_LoadLevel
        {
            [HarmonyPrefix]
            public static void Postfix()
            {
                invoke_auto_calibrate_time.Start();
            }
        }

        //--- Invoke move if KatWalk is used & recalibrate ---

        [HarmonyPatch(typeof(PlayerControl), "ManagedUpdate", new Type[] { })]
        public static class BaS_Test
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerControl __instance)
            {
                if (turning_disabled && Player.local.locomotion.turnSpeed != 0.0f)
                {
                    Player.local.locomotion.turnSpeed = 0.0f;
                }
                else if(!turning_disabled && Player.local.locomotion.turnSpeed != 1.0f)
                {
                    Player.local.locomotion.turnSpeed = 1.0f;
                }

                var ws = KATNativeSDK.GetWalkStatus();
                var lastCalibrationTime = KATNativeSDK.GetLastCalibratedTimeEscaped();


                if(lastCalibrationTime > 3.0f) //Prohibit multi recalibrations behind each other: Only one per 0.5s
                {
                    allow_recalibration = true;
                }


                //Debug.Log("btnPressed: " + ws.deviceDatas[0].btnPressed.ToString() + " last CalibrationTime: " + lastCalibrationTime.ToString());
                if ((ws.deviceDatas[0].btnPressed && allow_recalibration) || invoke_auto_calibrate)
                {
                    Debug.Log("BaS_Katwalk: Recalibrate");
                    invoke_auto_calibrate = false;
                    allow_recalibration = false;
                    invoke_auto_calibrate_time.Stop();
                    try
                    {
                        //Do recalibration
                        var hmdYaw = Player.local.head.transform.rotation.eulerAngles.y;
                        var bodyYaw = ws.bodyRotationRaw.eulerAngles.y;

                        yawCorrection = bodyYaw - hmdYaw;

                        var pos = Player.local.transform.position;
                        var eyePos = Player.local.head.transform.position;
                        eyePos.x = pos.x;
                        eyePos.z = pos.z;
                        Player.local.head.transform.position = eyePos;
                    }
                    catch (Exception e) {
                        Debug.Log("Unable to recalibrate:\n"+e.ToString()); //Can happen while loading the menu
                    }
                }

                float walking_speed = ws.moveSpeed.z;
                if(walking_speed != 0.0f) 
                {
                    __instance.Move(Side.Left, new Vector2(0.0f, 0.0f));
                }    
            }
        }

        //-- Insert KatWalk direction and speed data ---

        [HarmonyPatch(typeof(PlayerControl), "GetMoveDirection", new Type[] { typeof(Side), typeof(UnityEngine.Vector2) })]
        public static class BaS_GetMoveDirection
        {
            [HarmonyPostfix]
            public static UnityEngine.Vector3 Postfix(UnityEngine.Vector3 __result)
            {
                var ws = KATNativeSDK.GetWalkStatus();
                //Debug.Log("speedMul: "+  speedMul.ToString());
                //Debug.Log("speedExponent" + speedExponent.ToString());
                //Debug.Log("speedMaxRange"+speedMaxRange.ToString());


                //(0,BodyYaw,0)
                //UnityEngine.Vector3 direction = (ws.bodyRotationRaw * Quaternion.Inverse(Quaternion.Euler(new UnityEngine.Vector3(0, yawCorrection, 0)))).eulerAngles;
                Quaternion direction = ws.bodyRotationRaw * Quaternion.Inverse(Quaternion.Euler(new UnityEngine.Vector3(0, yawCorrection, 0)));
                //(0,0,Speed)
                UnityEngine.Vector3 speed = ws.moveSpeed * speedMul;
                Vector3 velocity = direction * speed;
                //Debug.Log("velocity: " + velocity.ToString());
                float ratio1 = CalculateRatioModified(Mathf.Abs(velocity.x), 0.0f, 5f, 0.0f, speedMaxRange);
                float ratio2 = CalculateRatioModified(Mathf.Abs(velocity.z), 0.0f, 5f, 0.0f, speedMaxRange);
                UnityEngine.Vector3 kat_result = new UnityEngine.Vector3((double)velocity.x > 0.0 ? ratio1 : -ratio1, 0.0f, (double)velocity.z > 0.0 ? ratio2 : -ratio2);
                //Direction x values are increased when loading a new level, why?!

                if (joystick_disabled)
                {
                    return kat_result;
                }
                else
                {
                    if(kat_result == Vector3.zero)
                    {
                        return __result;
                    }
                    else
                    {
                        return kat_result;
                    }
                }
            }
        }

        //Based on Utils.CalculateRatio
        public static float CalculateRatioModified(
          float input,
          float inputMin,
          float inputMax,
          float outputMin,
          float outputMax)
        {
            if ((double)input > (double)inputMax)
                input = inputMax;
            if ((double)input < (double)inputMin)
                input = inputMin;

            double normalized_input = ((double)input - (double)inputMin) / ((double)inputMax - (double)inputMin);

            return (float)(Math.Pow(normalized_input, (Double)speedExponent) * ((double)outputMax - (double)outputMin) + outputMin);
        }


        /*

        [HarmonyPatch(typeof(Locomotion), "MoveWeighted", new Type[] { typeof(Vector3),typeof(Transform),typeof(float),typeof(float),typeof(float) })]
        public static class BaS_Locomotion
        {
            [HarmonyPrefix]
            public static void Prefix(Locomotion __instance, float ___forwardSpeedMultiplier, float ___backwardSpeedMultiplier, float ___strafeSpeedMultiplier, float runSpeedRatio, float acceleration, Vector3 direction)
            {
                if(__instance.player != null)
                {
                    //foreward, backward, strafe speed constant on 0.16
                    Debug.Log("-------------------------------");
                    //Debug.Log("forwardSpeed: " + __instance.forwardSpeed.ToString());
                    //Debug.Log("forwardSpeedMultiplier: " + ___forwardSpeedMultiplier.ToString());
                    //Debug.Log("backwardSpeed: " + __instance.backwardSpeed.ToString());
                    //Debug.Log("backwardSpeedMultiplier: " + ___backwardSpeedMultiplier.ToString());
                    //Debug.Log("strafeSpeed: " + __instance.strafeSpeed.ToString());
                    //Debug.Log("strafeSpeedMultiplier: " + ___strafeSpeedMultiplier.ToString());
                    //Debug.Log("customGravity: " + __instance.customGravity);
                    //Debug.Log("runSpeedRatio: " + runSpeedRatio.ToString());
                    //Debug.Log("acceleration: " + acceleration.ToString());
                    Debug.Log("direction: " + direction.ToString());
                    //Debug.Log("transform.lossyScale: " + __instance.transform.lossyScale.ToString());
                    //Debug.Log("transform.lossyScale.y: " + __instance.transform.lossyScale.y.ToString());
                    //Debug.Log("Mathf.Clamp01(this.transform.lossyScale.y): " + Mathf.Clamp01(__instance.transform.lossyScale.y).ToString());
                    Debug.Log("-------------------------------");
                }
            }
        }

        */
    }
}