using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using Core.Spatial;
using DLCLib;
using DLCLib.Physics;

namespace DLCQuestipelago.AntiCrashes
{
    public class NodeCleaner
    {
        public const string NODE_DOES_NOT_HAVE_PARENT_ERROR =
            "This node does not contain item - it should not receive this event!";

        private static ManualLogSource _log;

        public NodeCleaner(ManualLogSource log)
        {
            _log = log;
        }

        public void CleanItemsToNodesRelationships(PhysicsManager physicsManager)
        {
            try
            {
                var dynamicPhysicalEntitiesField = typeof(PhysicsManager).GetField("dynamicPhysicalEntities",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var dynamicPhysicalEntities = (List<IPhysical>)dynamicPhysicalEntitiesField.GetValue(physicsManager);
                foreach (var dynamicPhysicalEntity in dynamicPhysicalEntities)
                {
                    var physicsObject = dynamicPhysicalEntity.GetPhysicsObject();
                    CleanItemToNodesRelationships(physicsObject);
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(NodeCleaner)}.{nameof(CleanItemsToNodesRelationships)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }

        private static void CleanItemToNodesRelationships(PhysicsObject physicsObject)
        {
            try
            {
                var itemsField = typeof(QuadTreeNode<Entity>).GetField("items", BindingFlags.NonPublic | BindingFlags.Instance);
                for (var i = physicsObject.Nodes.Count - 1; i >= 0; i--)
                {
                    var node = physicsObject.Nodes[i];
                    var items = (List<QuadTreeItem<Entity>>)itemsField.GetValue(node);
                    var indexOfItem = items.IndexOf(physicsObject);
                    if (indexOfItem > -1)
                    {
                        continue;
                    }

                    physicsObject.Nodes.RemoveAt(i);
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(NodeCleaner)}.{nameof(CleanItemToNodesRelationships)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
