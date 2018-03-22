using System.Threading.Tasks;
using AGS.API;
using AGS.Engine;

namespace LayerGame
{
    public class ParallaxRoom : RoomScript
    {
        private const string ROOM_ID = "ParallaxRoom";
        private IRenderLayer[] _backgroundLayers;
        private IRenderLayer[] _foregroundLayers;
        private RectangleF _roomBounds;

        public ParallaxRoom(IGame game) : base(game, ROOM_ID)
        {
        }

        protected async Task<IObject> addTerra(string name, string gfile, int x, int y)
        {
            IObject o = await addObject(name, gfile, x, y);
            o.Pivot = new PointF();
            return o;
        }

        protected override async Task<IRoom> loadAsync()
        {
            IGameFactory f = _game.Factory;
            _room = f.Room.GetRoom(ROOM_ID);

            PointF[] parallaxVals = new PointF[6]
            {
                new PointF(0f, 0f), new PointF(0.15f, 0.2f), new PointF(0.1f, 0.1f), new PointF(0.3f, 0.4f),
                new PointF(1f, 1f), new PointF(2.4f, 2.2f)
            };

            Point[] coordVals = new Point[6]
            {
                new Point(-200, 20), new Point(-320, 250), new Point(-320, 100), new Point(-240, -50),
                new Point(0, -55), new Point (400, -120)
            };

            _backgroundLayers = new IRenderLayer[4];
            for (int i = 0, z = AGSLayers.Background.Z - 1; i < 4; ++i, --z)
            {
                _backgroundLayers[i] = new AGSRenderLayer(z, parallaxVals[i]);
                IObject o = await addTerra($"layer{i}", $"parallax_l{i + 1}.png", coordVals[i].X, coordVals[i].Y);
                o.RenderLayer = _backgroundLayers[i];
            }

            IObject foreground = await addTerra("foreground", "parallax_l5.png", coordVals[4].X, coordVals[4].Y);

            _foregroundLayers = new IRenderLayer[1];
            for (int i = 5, l = 0, z = AGSLayers.Foreground.Z - 1; i < 6; ++i, --z)
            {
                _foregroundLayers[l] = new AGSRenderLayer(z, parallaxVals[i]);
                IObject o = await addTerra($"layer{i}", $"parallax_l{i + 1}.png", coordVals[i].X, coordVals[i].Y);
                o.RenderLayer = _foregroundLayers[l];
            }

            int ry = _game.Settings.VirtualResolution.Height;
            _roomBounds = new RectangleF(foreground.X, foreground.Y, foreground.Width, ry - foreground.Y);
            _room.RoomLimitsProvider = AGSRoomLimits.Custom(_roomBounds);
            return _room;
        }

        protected override void onActivate()
        {
            _game.Events.OnRepeatedlyExecute.Subscribe(onRepExec);
        }

        protected override void onDeactivate()
        {
            _game.Events.OnRepeatedlyExecute.Unsubscribe(onRepExec);
        }

        private void onRepExec(IRepeatedlyExecuteEventArgs args)
        {
            // TODO: move to shared code
            var input = _game.Input;
            var view = _game.State.Viewport;
            int rx = _game.Settings.VirtualResolution.Width;
            int ry = _game.Settings.VirtualResolution.Height;
            float viewspeed = 100f * (float)args.DeltaTime;
            float x = view.X;
            float y = view.Y;
            if (input.IsKeyDown(Key.Left))
                x -= viewspeed;
            if (input.IsKeyDown(Key.Right))
                x += viewspeed;
            if (input.IsKeyDown(Key.Up))
                y += viewspeed;
            if (input.IsKeyDown(Key.Down))
                y -= viewspeed;
            view.X = MathUtils.Clamp(x, _roomBounds.X, _roomBounds.X + _roomBounds.Width - rx);
            view.Y = MathUtils.Clamp(y, _roomBounds.Y, _roomBounds.Y + _roomBounds.Height - ry);
        }
    }
}
