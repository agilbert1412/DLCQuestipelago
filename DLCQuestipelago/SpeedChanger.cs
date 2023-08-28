using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using DLCLib;

namespace DLCQuestipelago
{
    public class SpeedChanger
    {
        private const float DEFAULT_GROUND_SPEED = 45f;
        private const float DEFAULT_AIR_SPEED = 30f;

        private readonly ManualLogSource _log;
        private readonly FieldInfo _inputGroundField;
        private readonly FieldInfo _inputAirField;

        public SpeedChanger(ManualLogSource log)
        {
            _log = log;
            // private static float PLAYER_INPUT_SCALE_GROUND = 45f;
            // private static float PLAYER_INPUT_SCALE_AIR = 30f;
            _inputGroundField = typeof(Player).GetField("PLAYER_INPUT_SCALE_GROUND", BindingFlags.NonPublic | BindingFlags.Static);
            _inputAirField = typeof(Player).GetField("PLAYER_INPUT_SCALE_AIR", BindingFlags.NonPublic | BindingFlags.Static);
        }

        public void ResetPlayerSpeedToDefault()
        {
            SetGroundSpeed(DEFAULT_GROUND_SPEED);
            SetAirSpeed(DEFAULT_AIR_SPEED);
        }

        public void AddMultiplierToPlayerSpeed(float multiplier)
        {
            var inputGroundValue = GetCurrentGroundSpeed();
            var inputAirValue = GetCurrentAirSpeed();

            _log.LogInfo($"Current Speed: {inputGroundValue}");
            _log.LogInfo($"Boost to process: {multiplier}");

            SetGroundSpeed(inputGroundValue * multiplier);
            SetAirSpeed(inputAirValue * multiplier);

            _log.LogInfo($"New Speed: {(float)_inputGroundField.GetValue(null)}");
        }

        private float GetCurrentGroundSpeed()
        {
            return (float)_inputGroundField.GetValue(null);
        }

        private float GetCurrentAirSpeed()
        {
            return (float)_inputAirField.GetValue(null);
        }

        private void SetGroundSpeed(float speed)
        {
            _inputGroundField.SetValue(null, speed);
        }

        private void SetAirSpeed(float speed)
        {
            _inputAirField.SetValue(null, speed);
        }
    }
}
