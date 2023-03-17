using BepInEx.Logging;
using DLCQuestipelago.Archipelago;
using System.Collections.Generic;
using System.Linq;

namespace DLCQuestipelago.Locations
{
    public class LocationChecker
    {
        private static ManualLogSource _console;
        private ArchipelagoClient _archipelago;
        private Dictionary<string, long> _checkedLocations;

        public LocationChecker(ManualLogSource console, ArchipelagoClient archipelago, List<string> locationsAlreadyChecked)
        {
            _console = console;
            _archipelago = archipelago;
            _checkedLocations = locationsAlreadyChecked.ToDictionary(x => x, _ => (long)-1);
        }

        public List<string> GetAllLocationsAlreadyChecked()
        {
            return _checkedLocations.Keys.ToList();
        }

        public bool IsLocationChecked(string locationName)
        {
            return _checkedLocations.ContainsKey(locationName);
        }

        public bool IsLocationMissing(string locationName)
        {
            return !IsLocationChecked(locationName);
        }

        public void AddCheckedLocation(string locationName)
        {
            if (_checkedLocations.ContainsKey(locationName))
            {
                return;
            }

            var locationId = _archipelago.GetLocationId(locationName);

            if (locationId == -1)
            {
                _console.LogError($"Location \"{locationName}\" could not be converted to an Archipelago id");
            }

            _checkedLocations.Add(locationName, locationId);
            SendAllLocationChecks();
        }

        public void SendAllLocationChecks()
        {
            if (!_archipelago.IsConnected)
            {
                return;
            }

            TryToIdentifyUnknownLocationNames();

            var allCheckedLocations = new List<long>();
            allCheckedLocations.AddRange(_checkedLocations.Values);

            allCheckedLocations = allCheckedLocations.Distinct().Where(x => x > -1).ToList();

            _archipelago.ReportCheckedLocations(allCheckedLocations.ToArray());
        }

        public void VerifyNewLocationChecksWithArchipelago()
        {
            var allCheckedLocations = _archipelago.GetAllCheckedLocations();
            foreach (var checkedLocation in allCheckedLocations)
            {
                var locationName = checkedLocation.Key;
                var locationId = checkedLocation.Value;
                if (!_checkedLocations.ContainsKey(locationName))
                {
                    _checkedLocations.Add(locationName, locationId);
                }
            }
        }

        private void TryToIdentifyUnknownLocationNames()
        {
            foreach (var locationName in _checkedLocations.Keys.ToArray())
            {
                if (_checkedLocations[locationName] > -1)
                {
                    continue;
                }

                var locationId = _archipelago.GetLocationId(locationName);
                if (locationId == -1)
                {
                    continue;
                }

                _checkedLocations[locationName] = locationId;
            }
        }

        public void ForgetLocations(IEnumerable<string> locations)
        {
            foreach (var location in locations)
            {
                if (!_checkedLocations.ContainsKey(location))
                {
                    continue;
                }

                _checkedLocations.Remove(location);
            }
        }
    }
}
