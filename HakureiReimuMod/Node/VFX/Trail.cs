using System;
using System.Collections.Generic;
using Godot;

namespace HakureiReimu.HakureiReimuMod.Node.VFX
{
    [GlobalClass]
    public partial class Trail : Line2D
    {
        private Node2D _target;
        [Export]
        public float PointDuration = 0.25f;
        private readonly List<float> _pointAge = new();
        private const float MinSpawnDist = 12f;
        private const float MaxSpawnDist = 48f;
        private Vector2? _lastPointPosition;

        public override void _Ready()
        {
            this._target = this.GetParent<Node2D>();
            long num = (long)this.Connect(CanvasItem.SignalName.VisibilityChanged,
                Callable.From(new Action(this.OnToggleVisibility)));
        }

        public override void _Process(double delta)
        {
            this.GlobalPosition = Vector2.Zero;
            this.GlobalRotation = 0.0f;
            this.CreatePoint(this._target.GlobalPosition, delta);
            float num = (float)delta;
            for (int index = 0; index < this.Points.Length; ++index)
            {
                if ((double)this._pointAge[index] > (double)this.PointDuration)
                {
                    this.RemovePoint(0);
                    this._pointAge.RemoveAt(0);
                }
                else
                    this._pointAge[index] += num;
            }
        }

        private void OnToggleVisibility()
        {
            this.ProcessMode = this.Visible ? Godot.Node.ProcessModeEnum.Inherit : Godot.Node.ProcessModeEnum.Disabled;
            this.ClearPoints();
        }

        private void CreatePoint(Vector2 pointPos, double delta)
        {
            if (this._lastPointPosition.HasValue)
            {
                float num1 = pointPos.DistanceTo(this._lastPointPosition.Value);
                if ((double)num1 < MinSpawnDist)
                    return;
                int pointCount = this.GetPointCount();
                if (pointCount > 2 && (double)num1 > MaxSpawnDist)
                {
                    Vector2 pointPosition1 = this.GetPointPosition(pointCount - 2);
                    Vector2 pointPosition2 = this.GetPointPosition(pointCount - 1);
                    for (float num2 = MaxSpawnDist; (double)num2 < (double)num1 - MinSpawnDist; num2 += MaxSpawnDist)
                    {
                        float weight = (float)(0.5 + (double)num2 / (double)num1 * 0.5);
                        Vector2 position = pointPosition1.Lerp(pointPosition2, weight)
                            .Lerp(pointPosition2.Lerp(pointPos, weight), weight);
                        this._pointAge.Add((float)delta * weight);
                        this.AddPoint(position);
                    }
                }
            }

            this._pointAge.Add(0.0f);
            this.AddPoint(pointPos);
            this._lastPointPosition = pointPos;
        }
    }
}