using System;
using AGS.API;
using AGS.Engine;

namespace LayerGame
{
    public class WorldSpace
    {
        private readonly string _id;
        private readonly WorldSystem _system;
        private readonly float _distance;
        private readonly IRenderLayer _renderLayer;
        private IConcurrentHashSet<IObject> _objects = new AGSConcurrentHashSet<IObject>();
        private readonly IObject _parent;

        public string ID { get => _id; }
        public WorldSystem System { get => _system; }
        public IObject ParentObject { get => _parent; }
        public IRenderLayer RenderLayer { get => _renderLayer; }
        public int Z { get => _renderLayer.Z; }
        public float Distance { get => _distance; }

        public IConcurrentHashSet<IObject> Objects { get => _objects; }

        public WorldSpace(string id, WorldSystem system, float distance, int z)
        {
            _id = id;
            _system = system;
            _distance = distance;
            PointF parallax = new PointF(1f / (system.ParallaxPerDistance.X * distance),
                1f / (system.ParallaxPerDistance.Y * distance));
            if (!MathEx.ValidFloat(parallax.X) || !MathEx.ValidFloat(parallax.Y))
                parallax = new PointF(1f, 1f);
            _renderLayer = new AGSRenderLayer(z, parallax);

            _parent = _system.Game.Factory.Object.GetObject($"worldspace.{system.ID}.{id}.parent");
            _parent.X = _system.Game.Settings.VirtualResolution.Width / 2 * -(parallax.X - 1f) + _system.PerspectiveShiftPerDistance.X * _distance;
            _parent.Y = _system.Baseline + _system.PerspectiveShiftPerDistance.Y * _distance;
            _parent.BaseSize = new SizeF(1f, 1f);
            _parent.ScaleX = 1f - _system.ScalePerDistance.X * _distance;
            _parent.ScaleY = 1f - _system.ScalePerDistance.Y * _distance;
            _parent.RenderLayer = _renderLayer;
        }

        public void Attach(IObject o)
        {
            _objects.Add(o);
            _parent.TreeNode.AddChild(o);
            updateObject(o);
        }

        private void updateObject(IObject o)
        {
            o.RenderLayer = _renderLayer;
        }
    }

    public static class WorldSpaceExtensions
    {
        public static void AttachToWorld(this IObject o, WorldSpace space)
        {
            space.Attach(o);
        }
    }
}
