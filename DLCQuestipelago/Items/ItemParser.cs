using System.Collections.Generic;
using System.Reflection;
using DLCLib.DLC;
using DLCQuestipelago.Archipelago;

namespace DLCQuestipelago.Items
{
    public class ItemParser
    {
        public HashSet<string> ReceivedDLCs { get; }

        public ItemParser()
        {
            ReceivedDLCs = new HashSet<string>();
        }

        public void ProcessItem(ReceivedItem item)
        {
            DLCPack unlockedDLC = null;
            foreach (var x in DLCManager.Instance.Packs)
            {
                if (x.Value.Data.DisplayName == item.ItemName)
                {
                    unlockedDLC = x.Value;
                    break;
                }
            }

            if (unlockedDLC != null)
            {
                ReceivedDLCs.Add(unlockedDLC.Data.Name);
                if (unlockedDLC.Data.PurchaseEvent == null || unlockedDLC.Data.PurchaseEvent.Equals(string.Empty) || unlockedDLC.Data.IsBossDLC)
                    return;
                typeof(DLCPurchaseEventUtil).InvokeMember(unlockedDLC.Data.PurchaseEvent, BindingFlags.InvokeMethod, (Binder)null, (object)null, new object[0]);
            }

            // Handle other items
        }
    }
}