using System;
using Godot;
using MegaCrit.Sts2.Core.Helpers;

namespace HakureiReimu.HakureiReimuMod.Node.VFX
{
    public partial class FollowVFX :Node2D
    {
        public Func<Vector2?> Target;
        public float Revolution=GD.RandRange(0,360);//环绕
        public float RevolutionSpeed;
        public float OrbitalRotation=GD.RandRange(0,360);
        public float OrbitalRotationSpeed;
        public float A;
        public float B;
        protected bool AtStart = true;

        public static FollowVFX Create(Func<Vector2?> target,float? a=null, float? b=null,float? revolutionSpeed=null,float? orbitalRotationSpeed=null)
        {
            FollowVFX vfx = new();
            vfx.Target = target;
            vfx.A = (float)(a ?? GD.RandRange(150f, 300));
            vfx.B = (float)(b ?? GD.RandRange(120f, 240));
            vfx.RevolutionSpeed = (float)(revolutionSpeed ?? GD.RandRange(30f, 90f)*RandomSign);
            vfx.OrbitalRotationSpeed = (float)(orbitalRotationSpeed ?? GD.RandRange(1f, 30f)*RandomSign);
            return vfx;
        }

        protected static float RandomSign => GD.RandRange(0, 1) > 0 ? 1 : -1;

        protected Vector2 GetOrbitPosition(float angle)
        {
            float rad=Mathf.DegToRad(angle);
            float rot=Mathf.DegToRad(OrbitalRotation);
            float x = A * Mathf.Cos(rad);
            float y = B * Mathf.Sin(rad);
            float rotateX = x * Mathf.Cos(rot) - y * Mathf.Sin(rot);
            float rotateY = x * Mathf.Sin(rot) + y * Mathf.Cos(rot);
            return new Vector2(rotateX, rotateY);
        }

        public override void _Ready()
        {
            Vector2? target = Target?.Invoke();
            if (target==null)
            {
                this.QueueFreeSafely();
                return;
            }
            this.GlobalPosition = target.Value;
        }

        public override void _Process(double delta)
        {
            Vector2? target = Target?.Invoke();
            if (target==null)
            {
                this.QueueFreeSafely();
                return;
            }
            Revolution = (float)((Revolution + RevolutionSpeed * delta + 360) % 360);
            OrbitalRotation=(float)((OrbitalRotation + OrbitalRotationSpeed * delta + 360) % 360);
            if (AtStart)
            {
                Vector2 t = target.Value + GetOrbitPosition(Revolution);
                GlobalPosition = GlobalPosition.Lerp(t, (float)(5f* delta));
                if (GlobalPosition.DistanceTo(t)<=20)
                {
                    AtStart = false;
                }
            }
            else
            {
                GlobalPosition = target.Value + GetOrbitPosition(Revolution);
            }
        }
    }
}