using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLCLib;
using DLCLib.Character;
using DLCLib.Physics;
using DLCLib.World.Props;
using Microsoft.Xna.Framework;

namespace DLCQuestipelago.Items.Traps
{
    internal class TemporarySpike : Spike
    {
        public int RemainingKills { get; set; }
        public TemporarySpike(Vector2 position, DirectionEnum direction, int maxKills) : base(position, direction)
        {
            RemainingKills = maxKills;
            physicsObject.IgnoreGravity = false;
            physicsObject.IsStatic = false;
        }

        public override bool OnCollision(PhysicsObject objA, PhysicsObject objB)
        {
            if (objB.Entity is NPC)
            {
                return false;
            }

            if (objB.Entity is Player { IsAlive: true })
            {
                RemainingKills -= 1;
            }
            else if (objB.Entity is Mob { IsAlive: true })
            {
                RemainingKills -= 1;
            }

            var collide = base.OnCollision(objA, objB);

            if (RemainingKills <= 0)
            {
                SceneManager.Instance.CurrentScene.RemoveFromScene(this);
            }

            return collide;
        }
    }
}
