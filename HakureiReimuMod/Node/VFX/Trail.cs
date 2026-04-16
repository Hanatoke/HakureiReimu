using Godot;

namespace HakureiReimu.HakureiReimuMod.Node.VFX
{
    [GlobalClass]
    public partial class Trail : Line2D
    {
        private Node2D _target;
        [Export()]
        public int MaxSegments = 30;

        public override void _Ready() => this._target = this.GetParent<Node2D>();

        public override void _Process(double delta)
        {
            this.GlobalPosition = Vector2.Zero;
            this.GlobalRotation = 0.0f;
            // this.GlobalScale = Vector2.One;
            this.AddPoint(this._target.GlobalPosition/GlobalScale);
            while (this.Points.Length > this.MaxSegments)
            {
                this.RemovePoint(0);
            }
        }
    }
}