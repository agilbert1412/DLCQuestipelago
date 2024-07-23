using System.Reflection;
using DLCLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago
{
    public class SpeedChanger
    {
        private const float DEFAULT_GROUND_SPEED = 45f;
        private const float DEFAULT_AIR_SPEED = 30f;

        private readonly ILogger _logger;
        private readonly FieldInfo _inputGroundField;
        private readonly FieldInfo _inputAirField;

        public SpeedChanger(ILogger logger)
        {
            _logger = logger;
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

            _logger.LogInfo($"Current Speed: {inputGroundValue}");
            _logger.LogInfo($"Boost to process: {multiplier}");

            SetGroundSpeed(inputGroundValue * multiplier);
            SetAirSpeed(inputAirValue * multiplier);

            _logger.LogInfo($"New Speed: {(float)_inputGroundField.GetValue(null)}");
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
