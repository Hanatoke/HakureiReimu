using System;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Node.VFX.Mover;
using MegaCrit.Sts2.Core.Helpers;

namespace HakureiReimu.HakureiReimuMod.Node.VFX
{
    public partial class FlyingVFX :Node2D
    {
        private float _time;
        public float Duration = 0.5f;

        private float _curveLength;
        private Vector2 _lastPos;
        public IMover Mover;
        public Action<float,double> UpdateMethod;
        public Func<float, float> EaseFunc;
        public Action OnHit;
        private TaskCompletionSource _hitTcs = new();
        public Task HitTask => _hitTcs.Task;

        public static FlyingVFX Create(IMover mover)
        {
            FlyingVFX vfx = new();
            vfx.Mover = mover;
            return vfx;
        }
        public override void _Process(double delta)
        {
            float dt = (float)delta;

            _time += dt;

            float t = Mathf.Clamp(_time / Duration, 0f, 1f);
            if (EaseFunc != null) t = EaseFunc(t);
            Vector2 pos = Mover.CurrentPosition(this, t,(float)delta);
            GlobalPosition = pos;
            UpdateRotation(pos);
            UpdateMethod?.Invoke(t,delta);
            if (Mover.IsHit(this,t,(float)delta)||t>=1)
            {
                OnHit?.Invoke();
                _hitTcs.TrySetResult();
                this.QueueFreeSafelyNoPool();
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