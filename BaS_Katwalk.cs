using System;
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
            new ModOptionFloat("1", 1.0f),
            new ModOptionFloat("1.5", 1.5f),
            new ModOptionFloat("2", 2.0f),
            new ModOptionFloat("2.5", 2.5f),
            new ModOptionFloat("3", 3.0f),
            new ModOptionFloat("3.5", 3.5f),
            new ModOptionFloat("4", 4.0f),
            new ModOptionFloat("4.5", 4.5f),
            new ModOptionFloat("5", 5.0f)
        };

        public static ModOptionFloat[] speedExponentOption = {
            new ModOptionFloat("1 (linear)", 1.0f),
            new ModOptionFloat("1.1", 1.1f),
            new ModOptionFloat("1.2", 1.2f),
            new ModOptionFloat("1.3", 1.3f),
            new ModOptionFloat("1.4", 1.4f),
            new ModOptionFloat("1.5", 1.5f),
            new ModOptionFloat("1.6", 1.6f),
            new ModOptionFloat("1.7", 1.7f),
            new ModOptionFloat("1.8", 1.8f),
            new ModOptionFloat("1.9", 1.9f),
            new ModOptionFloat("2 (quadratic)", 2.0f)
        };

        #endregion


        #region Private Parameters

        [ModOption(name: "Speed Multiplicator", tooltip: "Modifies how the speed of the KatWalk is translated to the in-game speed", valueSourceName: nameof(speedMulOption), category = "Speed options",defaultValueIndex = 3)]
        private static float speedMul;

        [ModOption(name: "Speed max. Range", tooltip: "Modifies what maximal ingame speed value corresponds to the maximal KatWalk speed (5 m/s)", valueSourceName: nameof(speedMaxRangeOption), category = "Speed options", defaultValueIndex = 4)]
        private static float speedMaxRange;

        [ModOption(name: "Speed curve exponent", tooltip: "Modifies the speed curve which determines how fast the maximal speed is reached", valueSourceName: nameof(speedExponentOption), category = "Speed options", defaultValueIndex = 1)]
        private static float speedExponent;

        [ModOption(name: "joystick disabled", tooltip: "Disabling the Joystick-Movement", category = "Others")]
        private static bool joystick_disabled = false;



        private static float yawCorrection = 0.0f;
        private Harmony harmony;

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

        //--- Invoke move if KatWalk is used & recalibrate ---

        [HarmonyPatch(typeof(PlayerControl), "ManagedUpdate", new Type[] { })]
        public static class BaS_Test
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerControl __instance)
            {
                var ws = KATNativeSDK.GetWalkStatus();
                var lastCalibrationTime = KATNativeSDK.GetLastCalibratedTimeEscaped();

                if (ws.deviceDatas[0].btnPressed || lastCalibrationTime < 0.08)
                {
                    Debug.Log("BaS_Katwalk: Recalibrate");
                    //Do recalibration
                    var hmdYaw = Player.local.head.transform.rotation.eulerAngles.y;
                    var bodyYaw = ws.bodyRotationRaw.eulerAngles.y;

                    yawCorrection = bodyYaw - hmdYaw;

                    var pos = Player.local.transform.position;
                    var eyePos = Player.local.head.transform.position;
                    pos.x = eyePos.x;
                    pos.z = eyePos.z;
                    Player.local.transform.position = pos;
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


                //(0,BodyYaw,0)
                //UnityEngine.Vector3 direction = (ws.bodyRotationRaw * Quaternion.Inverse(Quaternion.Euler(new UnityEngine.Vector3(0, yawCorrection, 0)))).eulerAngles;
                Quaternion direction = ws.bodyRotationRaw * Quaternion.Inverse(Quaternion.Euler(new UnityEngine.Vector3(0, yawCorrection, 0)));
                //(0,0,Speed)
                UnityEngine.Vector3 speed = ws.moveSpeed * speedMul;
                Vector3 velocity = direction * speed;
                float ratio1 = CalculateRatioModified(Mathf.Abs(velocity.x), 0.0f, 5f, 0.0f, speedMaxRange);
                float ratio2 = CalculateRatioModified(Mathf.Abs(velocity.z), 0.0f, 5f, 0.0f, speedMaxRange);
                UnityEngine.Vector3 kat_result = new UnityEngine.Vector3((double)velocity.x > 0.0 ? ratio1 : -ratio1, 0.0f, (double)velocity.z > 0.0 ? ratio2 : -ratio2);

                if (joystick_disabled)
                {
                    return kat_result;
                }
                else
                {
                    return kat_result + __result; //return sum, so that a mixture of Joystick+KatWalk is possible
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

    }
}