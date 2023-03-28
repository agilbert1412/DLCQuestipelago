using DLCLib;
using DLCLib.Character;
using Microsoft.Xna.Framework;

namespace DLCQuestipelago.ItemShufflePatches
{
    public class ConversationStarter
    {
        public bool StartConversation(NPC npc, string conversation)
        {
            if (!npc.IsAlive)
            {
                return false;
            }

            npc.SetCurrentConversation(conversation);
            SceneManager.Instance.CurrentScene.ConversationManager.StartConversation(npc.CurrentConversation);
            SceneManager.Instance.CurrentScene.Player.GetPhysicsObject().Velocity = Vector2.Zero;
            SceneManager.Instance.CurrentScene.Player.GetPhysicsObject().Acceleration = Vector2.Zero;
            return true;
        }
    }
}
