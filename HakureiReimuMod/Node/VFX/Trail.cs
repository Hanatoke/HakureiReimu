using System.Collections.Generic;
using System.Linq;
using Godot;

namespace HakureiReimu.HakureiReimuMod.Node.VFX
{
    [GlobalClass]
    public partial class Trail : Line2D
    {
        private Node2D _target;
        [Export()]
        public int MaxSegments = 30;

        public List<Vector2> OriginPoints = new();

        public override void _Ready() => this._target = this.GetParent<Node2D>();

        public override void _Process(double delta)
        {
            this.GlobalPosition = Vector2.Zero;
            this.GlobalRotation = 0.0f;
            // this.AddPoint(this._target.GlobalPosition/GlobalScale);
            // while (this.Points.Length > this.MaxSegments)
            // {
            //     this.RemovePoint(0);
            // }
            OriginPoints.Add(_target.GlobalPosition);
            while (OriginPoints.Count > MaxSegments)
            {
                OriginPoints.RemoveAt(0);
            }
            this.SetPoints(OriginPoints.Select(p=>p/GlobalScale).ToArray());
        }
    }
}