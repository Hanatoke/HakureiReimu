using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using HakureiReimu.HakureiReimuMod.Node.VFX.Mover;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HakureiReimu.HakureiReimuMod.Command
{
    public static class FlyingVFXCmd
    {
        public static async Task DanmakuCurveToTarget(Creature source, Creature target, int num = 1, float scale = 1,
            float speedScale = 1)
        {
            NCreature s = NCombatRoom.Instance?.GetCreatureNode(source);
            NCreature t = NCombatRoom.Instance?.GetCreatureNode(target);
            if (s != null && t != null)
            {
                List<Task> tasks = new();
                for (var i = 0; i < num; i++)
                {
                    Color color = Color.FromHsv(GD.Randf(), 1, 1);
                    Vector3 v = new Vector3(
                        (float)GD.RandRange(0, 1),
                        (float)GD.RandRange(-0.5, 1),
                        (float)GD.RandRange(-0.5, 0.5)
                    ).Normalized() * 1000 * speedScale;
                    if (s.VfxSpawnPosition.X < t.VfxSpawnPosition.X)
                    {
                        v.X *= -1;
                    }

                    FlyingVFX vfx = FlyingVFX.Create(
                        new Steering3DMover(s.VfxSpawnPosition,
                            t.VfxSpawnPosition + RandomOffset(scale),
                            v, turnSpeed: 10 * speedScale, acceleration: 2000 * speedScale));
                    vfx.Duration = 3 / speedScale;
                    vfx.OnHit = () =>
                    {
                        NDebugAudioManager.Instance?.Play("blunt_attack.mp3");
                        AddVFXOnTarget(NDanmakuImpact.Create(scale, color), vfx.GlobalPosition);
                    };
                    vfx.AddChildSafely(NDanmaku.Create(scale, color,100));
                    NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                    tasks.Add(vfx.HitTask);
                }

                await Task.WhenAny(tasks);
            }
        }

        public static async Task DanmakuLineToTarget(Creature source, Creature target, float scale = 1,
            float duration = 0.5f, Color? color = null)
        {
            NCreature s = NCombatRoom.Instance?.GetCreatureNode(source);
            NCreature t = NCombatRoom.Instance?.GetCreatureNode(target);
            if (s != null && t != null)
            {
                Color c = color ?? Color.FromHsv(GD.Randf(), 1, 1);
                FlyingVFX vfx = FlyingVFX.Create(new LineMover(s.VfxSpawnPosition,
                    t.VfxSpawnPosition + RandomOffset(scale)));
                vfx.Duration = duration;
                vfx.OnHit = () =>
                {
                    NDebugAudioManager.Instance?.Play("blunt_attack.mp3");
                    AddVFXOnTarget(NDanmakuImpact.Create(scale, c), vfx.GlobalPosition);
                };
                vfx.AddChildSafely(NDanmaku.Create(scale, c,100));
                NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                await vfx.HitTask;
            }
        }

        public static Vector2 RandomOffset(float scale = 1) =>
            new Vector2(GD.RandRange(-25, 25), GD.RandRange(-25, 25)) * scale;

        public static void AddVFXOnTarget(Node2D source, Vector2 position)
        {
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(source);
            source.GlobalPosition = position;
        }
    }
}