//using System.Collections.Generic;
//using System.Linq;

//namespace DLCQuestipelago.Locations
//{
//    public class LocationChecker
//    {
//        private static ILogger _logger;
//        private ArchipelagoClient _archipelago;
//        private Dictionary<string, long> _checkedLocations;

//        public LocationChecker(ILogger logger, ArchipelagoClient archipelago, List<string> locationsAlreadyChecked)
//        {
//            _logger = logger;
//            _archipelago = archipelago;
//            _checkedLocations = locationsAlreadyChecked.ToDictionary(x => x, _ => (long)-1);
//        }

//        public List<string> GetAllLocationsAlreadyChecked()
//        {
//            return _checkedLocations.Keys.ToList();
//        }

//        public bool IsLocationChecked(string locationName)
//        {
//            return _checkedLocations.ContainsKey(locationName);
//        }

//        public bool IsLocationMissing(string locationName)
//        {
//            return !IsLocationChecked(locationName);
//        }

//        public bool IsLocationMissingAndExists(string locationName)
//        {
//            return _archipelago.LocationExists(locationName) && IsLocationMissing(locationName);
//        }

//        public void RememberCheckedLocation(string locationName)
//        {
//            if (_checkedLocations.ContainsKey(locationName))
//            {
//                return;
//            }

//            var locationId = _archipelago.GetLocationId(locationName);

//            if (locationId == -1)
//            {
//                _console.LogError($"Location \"{locationName}\" could not be converted to an Archipelago id");
//            }

//            _console.LogInfo($"Checking Location {locationName}!");
//            _checkedLocations.Add(locationName, locationId);
//        }

//        public void RememberCheckedLocation(string[] locationNames)
//        {
//            foreach (var locationName in locationNames)
//            {
//                RememberCheckedLocation(locationName);
//            }
//        }

//        public void AddCheckedLocation(string[] locationNames)
//        {
//            RememberCheckedLocation(locationNames);
//            SendAllLocationChecks();
//        }

//        public void AddCheckedLocation(string locationName)
//        {
//            RememberCheckedLocation(locationName);
//            SendAllLocationChecks();
//        }

//        public void SendAllLocationChecks()
//        {
//            if (!_archipelago.IsConnected)
//            {
//                return;
//            }

//            TryToIdentifyUnknownLocationNames();

//            var allCheckedLocations = new List<long>();
//            allCheckedLocations.AddRange(_checkedLocations.Values);

//            allCheckedLocations = allCheckedLocations.Distinct().Where(x => x > -1).ToList();

//            if (_archipelago.HasReceivedItem("Day One Patch Pack"))
//            {
//                _archipelago.ReportCheckedLocationsAsync(allCheckedLocations.ToArray());
//            }
//            else
//            {
//                _archipelago.ReportCheckedLocations(allCheckedLocations.ToArray());
//            }
//        }

//        public void VerifyNewLocationChecksWithArchipelago()
//        {
//            var allCheckedLocations = _archipelago.GetAllCheckedLocations();
//            foreach (var checkedLocation in allCheckedLocations)
//            {
//                var locationName = checkedLocation.Key;
//                var locationId = checkedLocation.Value;
//                if (!_checkedLocations.ContainsKey(locationName))
//                {
//                    _checkedLocations.Add(locationName, locationId);
//                }
//            }
//        }

//        private void TryToIdentifyUnknownLocationNames()
//        {
//            foreach (var locationName in _checkedLocations.Keys.ToArray())
//            {
//                if (_checkedLocations[locationName] > -1)
//                {
//                    continue;
//                }

//                var locationId = _archipelago.GetLocationId(locationName);
//                if (locationId == -1)
//                {
//                    continue;
//                }

//                _checkedLocations[locationName] = locationId;
//            }
//        }

//        public void ForgetLocations(IEnumerable<string> locations)
//        {
//            foreach (var location in locations)
//            {
//                if (!_checkedLocations.ContainsKey(location))
//                {
//                    continue;
//                }

//                _checkedLocations.Remove(location);
//            }
//        }
//    }
//}
