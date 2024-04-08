using System;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;

namespace DLCQuestipelago.Archipelago.EnergyLink
{
    static class EnergyLinkExtensions
    {
        public static void SendEnergy(this ArchipelagoSession session, string energyLinkKey, long joules)
        {
            session.Socket.SendPacket(new EnergyLinkSetPacket
            {
                Key = energyLinkKey,
                DefaultValue = 0,
                Slot = session.ConnectionInfo.Slot,
                Operations = new OperationSpecification[]
                {
                    new() { OperationType = OperationType.Add, Value = joules }
                }
            });
        }

        public static void DrainEnergy(this ArchipelagoSession session, string energyLinkKey, long joules)
        {
            session.Socket.SendPacket(new EnergyLinkSetPacket
            {
                Key = energyLinkKey,
                DefaultValue = 0,
                Slot = session.ConnectionInfo.Slot,
                Operations = new OperationSpecification[]
                {
                    new() { OperationType = OperationType.Add, Value = -joules }
                }
            });
        }
    }
}
