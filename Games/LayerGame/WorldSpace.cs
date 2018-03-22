using System;
using AGS.API;
using AGS.Engine;

namespace LayerGame
{
    public class WorldSpace
    {
        private readonly WorldSystem _system;
        private readonly float _distance;
        private readonly IRenderLayer _renderLayer;
        private IConcurrentHashSet<IObject> _objects = new AGSConcurrentHashSet<IObject>();
        private IObject _parent;

        public WorldSystem System { get => _system; }
        public IObject ParentObject
        {
            get => _parent;
            // TODO: need to properly uncouple existing objects, etc
            //set { _parent = value; updateObject(_parent); }
        }
        public IRenderLayer RenderLayer { get => _renderLayer; }
        public int Z { get => _renderLayer.Z; }
        public float Distance { get => _distance; }

        public IConcurrentHashSet<IObject> Objects { get => _objects; }

        public WorldSpace(WorldSystem system, float distance, int z)
        {
            _system = system;
            _distance = distance;
            PointF parallax = new PointF(1f / (system.ParallaxPerDistance.X * distance),
                1f / (system.ParallaxPerDistance.Y * distance));
            if (!MathEx.ValidFloat(parallax.X) || !MathEx.ValidFloat(parallax.Y))
                parallax = new PointF(1f, 1f);
            _renderLayer = new AGSRenderLayer(z, parallax);
        }

        public void Attach(IObject o)
        {
            _objects.Add(o);
            if (_parent == null)
            {
                _parent = o;
                o.X = o.X + _system.PerspectiveShiftPerDistance.X * _distance;
                o.Y = o.Y + _system.PerspectiveShiftPerDistance.Y * _distance;
                o.ScaleX = 1f - _system.ScalePerDistance.X * _distance;
                o.ScaleY = 1f - _system.ScalePerDistance.Y * _distance;
                byte shade = (byte)(255 - Math.Round(_system.ShadePerDistance * _distance));
                o.Tint = Color.FromArgb(255, shade, 255, 255);
            }
            else
            {
                _parent.TreeNode.AddChild(o);
            }
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
