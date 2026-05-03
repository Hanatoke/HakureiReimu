using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using HakureiReimu.HakureiReimuMod.Node.VFX.Mover;
using MegaCrit.Sts2.Core.Helpers;

namespace HakureiReimu.HakureiReimuMod.Node.VFX
{
    public partial class FlyingVFX :Node2D
    {
        private float _time;
        public float Duration = 0.5f;
        public float DestroyDuration = 0f;
        public bool IsHit ;

        private float _curveLength;
        private Vector2 _lastPos;
        public IMover Mover;
        public OnUpdate UpdateMethod;
        public Ease EaseFunc;
        public Action OnHit;
        private TaskCompletionSource _hitTcs = new();
        public Task HitTask => _hitTcs.Task;
        public delegate void OnUpdate(float time,double delta);
        public delegate float Ease(float time);

        public static FlyingVFX Create(IMover mover)
        {
            FlyingVFX vfx = new();
            vfx.Mover = mover;
            return vfx;
        }
        public override void _Process(double delta)
        {
            float dt = (float)delta;
            if (IsHit)
            {
                DestroyDuration -= dt;
                if (DestroyDuration <= 0)
                {
                    this.QueueFreeSafelyNoPool();
                }
                return;
            }
            
            _time += dt;

            float t = Mathf.Clamp(_time / Duration, 0f, 1f);
            if (EaseFunc != null) t = EaseFunc(t);
            Vector2 pos = Mover.CurrentPosition(this, t,(float)delta);
            GlobalPosition = pos;
            UpdateRotation(pos);
            UpdateMethod?.Invoke(t,delta);
            if (Mover.IsHit(this,t,(float)delta)||t>=1)
            {
                _hitTcs.TrySetResult();
                OnHit?.Invoke();
                if (DestroyDuration<=0)
                {
                    this.QueueFreeSafelyNoPool();
                }
                IsHit = true;
            }
        }
        protected virtual void UpdateRotation(Vector2 pos)
        {
            Vector2 velocity = pos - _lastPos;

            if (velocity.Length() > 0.01f)
                Rotation = velocity.Angle();

            _lastPos = pos;
        }
    }
}