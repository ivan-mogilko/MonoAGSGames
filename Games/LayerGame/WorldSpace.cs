using System.Collections.Generic;
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
        private AGSConcurrentHashSet<IObject> _objects = new AGSConcurrentHashSet<IObject>();
        private List<IArea> _areas = new List<IArea>();
        private readonly IObject _parent;

        public string ID { get => _id; }
        public WorldSystem System { get => _system; }
        public IObject ParentObject { get => _parent; }
        public IRenderLayer RenderLayer { get => _renderLayer; }
        public int Z { get => _renderLayer.Z; }
        public float Distance { get => _distance; }

        public PointF ScaleFactor { get; }

        public IEnumerable<IObject> Objects { get => _objects; }
        public IReadOnlyList<IArea> Areas { get => _areas; }

        public WorldSpace(string id, WorldSystem system, float distance, int z)
        {
            _id = id;
            _system = system;
            _distance = distance;
            PointF parallax = new PointF(1f / ((1f + system.ParallaxPerDistance.X) * distance),
                1f / ((1f + system.ParallaxPerDistance.Y) * distance));
            if (!MathEx.ValidFloat(parallax.X) || !MathEx.ValidFloat(parallax.Y))
                parallax = new PointF(1f, 1f);
            _renderLayer = new AGSRenderLayer(z, parallax);

            _parent = _system.Game.Factory.Object.GetObject($"worldspace.{system.ID}.{id}.parent");
            // TODO: alright, I don't remember why I did this formula for parent.X...
            _parent.X = _system.Baseline.X * -(parallax.X - 1f) + _system.PerspectiveShiftPerDistance.X * _distance;
            _parent.Y = _system.Baseline.Y + _system.PerspectiveShiftPerDistance.Y * _distance;
            _parent.BaseSize = new SizeF(1f, 1f);
            ScaleFactor = new PointF(1f / ((1f + _system.ScalePerDistance.X) * _distance),
                1f / ((1f + _system.ScalePerDistance.Y) * _distance));
            _parent.ScaleX = ScaleFactor.X;
            _parent.ScaleY = ScaleFactor.Y;
            _parent.RenderLayer = _renderLayer;
        }

        public void Attach(IObject o)
        {
            _objects.Add(o);
            _parent.TreeNode.AddChild(o);
            foreach (IArea area in _areas)
                area.AllowEntity(o);
            o.RenderLayer = _renderLayer;
        }

        public void Detach(IObject o)
        {
            _objects.Remove(o);
            _parent.TreeNode.RemoveChild(o);
            foreach (IArea area in _areas)
                area.DisallowEntity(o);
            o.RenderLayer = null;
        }

        public void Attach(IArea a)
        {
            _areas.Add(a);
            if (!a.HasComponent<IAreaRestriction>())
                a.AddComponent<IAreaRestriction>().RestrictionType = RestrictionListType.WhiteList;
            if (!a.HasComponent<ITranslateComponent>())
                a.AddComponent<ITranslateComponent>();
            if (!a.HasComponent<IRotateComponent>())
                a.AddComponent<IRotateComponent>();
            if (!a.HasComponent<IScaleComponent>())
                a.AddComponent<IScaleComponent>();
            // TODO: need a way to add area to object parent!!
            // probably IObject should have not ObjectTree but EntityTree component??
            foreach (IObject o in _objects)
                a.AllowEntity(o);
            foreach (WorldSpace s in _system.Spaces)
                if (s != this)
                    foreach (IObject o in s.Objects)
                        a.DisallowEntity(o);
            IZoomArea zoom = a.GetComponent<IZoomArea>();
            if (zoom == null)
                zoom = a.AddComponent<IZoomArea>();
            zoom.ZoomCamera = true;
            // TODO: this is not really ideal
            zoom.MinZoom = 1 / ScaleFactor.X;
            zoom.MaxZoom = 1 / ScaleFactor.X;
            IScalingArea scaling = a.GetComponent<IScalingArea>();
            if (scaling == null)
                scaling = a.AddComponent<IScalingArea>();
            // TODO: this ofcourse is not good
            scaling.MinScaling = ScaleFactor.X;
            scaling.MaxScaling = ScaleFactor.X;

            if (a.Mask.DebugDraw != null)
                Attach(a.Mask.DebugDraw);
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
