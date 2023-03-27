using System;
using System.Collections.Generic;
using System.IO;
using DLCDataTypes;
using DLCLib;
using DLCLib.AI;
using DLCLib.Character;
using DLCLib.Character.LFOD;
using DLCLib.Physics;
using DLCLib.Render;
using DLCQuestipelago.DualContentManager;
using Microsoft.Xna.Framework;

namespace DLCQuestipelago.Items.Traps
{
    public class ZombieSheepTrap : ZombieSheep
    {
        private DLCDualContentManager _dualContentManager;

        public ZombieSheepTrap(DLCDualContentManager dualContentManager) : base()
        {
            _dualContentManager = dualContentManager;
        }

        public override void LoadContent()
        {
            var mobLoadContentPointer = typeof(Mob).GetMethod("LoadContent").MethodHandle.GetFunctionPointer();
            var mobLoadContent = (Action)Activator.CreateInstance(typeof(Action), this, mobLoadContentPointer);
            mobLoadContent();

            var zombieSheepAnimationsPath = Path.Combine("data", "animations", "zombiesheep");
            var zombieSheepAnimations = _dualContentManager.Load<List<AnimationData>>(zombieSheepAnimationsPath);
            AnimationUtil.ConstructAnimations(zombieSheepAnimations, out animations);
            animPlayer.Play(animations["idle"]);
            controller = new AIController(new WanderBiteBehavior(this)
            {
                MinWanderTime = 0.25f,
                MaxWanderTime = 1f,
                MinDelayTime = 0.25f,
                MaxDelayTime = 0.5f
            });
            damageRectangle = new DamageRectangle(new Vector2(0.45f), this);
            damageRectangle.GetPhysicsObject().OnCollision += this.OnBiteCollision;
            SceneManager.Instance.CurrentScene.AddToScene(this.damageRectangle);
        }
    }
}
