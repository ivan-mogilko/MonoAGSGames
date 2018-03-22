﻿using System.Threading.Tasks;
using AGS.API;
using AGS.Engine;

namespace LayerGame
{
    public class IslandsRoom : RoomScript
    {
        private const string ROOM_ID = "IslandsRoom";
        private IRenderLayer[] _backgroundLayers;
        private IRenderLayer[] _foregroundLayers;
        private RectangleF _roomBounds;

        private WorldSystem _wsystem;
        private WorldSpace _spaceIsland1;
        private WorldSpace _spaceIsland2;
        private WorldSpace _spaceIsland3;
        private float[] _islandWalklines;

        private IObject _robot;
        private int _curIsland;

        public IslandsRoom(IGame game) : base(game, ROOM_ID)
        {
        }

        protected async Task<IObject> addTerra(string name, string gfile, int x = 0, int y = 0)
        {
            IObject o = await addObject(name, gfile, x, y);
            o.Pivot = new PointF();
            return o;
        }

        protected async Task<IObject> addIsland(string name, string gfile, int x = 0, int y = 0)
        {
            IObject o = await addObject(name, gfile, x, y);
            o.Pivot = new PointF(0.5f, 1f);
            return o;
        }

        protected override async Task<IRoom> loadAsync()
        {
            IGameFactory f = _game.Factory;
            _room = f.Room.GetRoom(ROOM_ID);

            PointF[] parallaxVals = new PointF[]
            {
                new PointF(0f, 0f), new PointF(0.05f, 0.02f)
            };

            Point[] coordVals = new Point[]
            {
                new Point(0, 0), new Point(0, 0)
            };

            _backgroundLayers = new IRenderLayer[2];
            for (int i = 0, z = AGSLayers.Background.Z - 1; i < 2; ++i, --z)
            {
                _backgroundLayers[i] = new AGSRenderLayer(z, parallaxVals[i]);
                IObject o = await addTerra($"layer{i}", $"islands_l{i + 1}.png", coordVals[i].X, coordVals[i].Y);
                o.RenderLayer = _backgroundLayers[i];
            }

            int rx = _game.Settings.VirtualResolution.Width;
            int ry = _game.Settings.VirtualResolution.Height;

            // TODO: use world spaces bounds
            _roomBounds = new RectangleF(-rx, 0, rx * 3, 800f);
            _room.RoomLimitsProvider = AGSRoomLimits.Custom(_roomBounds);

            Point islandPos = new Point(rx / 2, ry / 4);

            // Creating world spaces (a number of islands on different layers of parallax)
            _wsystem = new WorldSystem();
            _wsystem.ParallaxPerDistance = new PointF(1.4f, 1.1f);
            _wsystem.ScalePerDistance = new PointF(0.1f, 0.1f);
            _wsystem.PerspectiveShiftPerDistance = new PointF(0f, 15f);
            _wsystem.ShadePerDistance = 30f;
            int wz = AGSLayers.Foreground.Z;
            _spaceIsland1 = new WorldSpace(_wsystem, 0f, wz);
            IObject island = await addIsland("island1", "island_plat1.png", islandPos.X, islandPos.Y);
            _spaceIsland1.Attach(island);
            _spaceIsland2 = new WorldSpace(_wsystem, 2f, wz + 1);
            island = await addIsland("island2", "island_plat2.png", islandPos.X, islandPos.Y);
            _spaceIsland2.Attach(island);
            _spaceIsland3 = new WorldSpace(_wsystem, 4f, wz + 2);
            island = await addIsland("island3", "island_plat3.png", islandPos.X, islandPos.Y);
            _spaceIsland3.Attach(island);
            _wsystem.Spaces.Add(_spaceIsland1);
            _wsystem.Spaces.Add(_spaceIsland2);
            _wsystem.Spaces.Add(_spaceIsland3);

            _islandWalklines = new float[3] { 122f, 79f, 175f };

            _robot = await addObject("robot", "robot.png");
            jumpToIsland(_robot, 0);
            return _room;
        }

        protected override void onActivate()
        {
            _game.State.Viewport.Camera.Target = getRobot;
            _game.Events.OnRepeatedlyExecute.Subscribe(onRepExec);
            _game.Input.KeyDown.Subscribe(onKeyDown);
            _game.Input.KeyUp.Subscribe(onKeyUp);
        }

        protected override void onDeactivate()
        {
            _game.State.Viewport.Camera.Target = null;
            _game.Events.OnRepeatedlyExecute.Unsubscribe(onRepExec);
        }

        private void jumpToIsland(IObject o, int num)
        {
            if (num < 0 || num > _wsystem.Spaces.Count)
                return;
            o.AttachToWorld(_wsystem.Spaces[num]);
            o.Y = _islandWalklines[num];
            _curIsland = num;
        }

        private void onRepExec(IRepeatedlyExecuteEventArgs args)
        {
            // TODO: move to shared code
            var input = _game.Input;

            if (input.IsKeyDown(Key.LShift))
            {
                var view = _game.State.Viewport;
                int rx = _game.Settings.VirtualResolution.Width;
                int ry = _game.Settings.VirtualResolution.Height;
                float vx = view.X;
                float vy = view.Y;
                float viewspeed = 100f * (float)args.DeltaTime;
                if (input.IsKeyDown(Key.Left))
                    vx -= viewspeed;
                if (input.IsKeyDown(Key.Right))
                    vx += viewspeed;
                if (input.IsKeyDown(Key.Up))
                    vy += viewspeed;
                if (input.IsKeyDown(Key.Down))
                    vy -= viewspeed;
                view.X = MathUtils.Clamp(vx, _roomBounds.X, _roomBounds.X + _roomBounds.Width - rx);
                view.Y = MathUtils.Clamp(vy, _roomBounds.Y, _roomBounds.Y + _roomBounds.Height - ry);
            }
            else
            {
                float robotspeed = 200f * (float)args.DeltaTime;
                float rox = _robot.X;
                if (input.IsKeyDown(Key.Left))
                    rox -= robotspeed;
                if (input.IsKeyDown(Key.Right))
                    rox += robotspeed;
                _robot.X = rox;
            }
        }

        private void onKeyDown(KeyboardEventArgs args)
        {
            if (args.Key == Key.ShiftLeft)
                _game.State.Viewport.Camera.Target = null;
        }

        private void onKeyUp(KeyboardEventArgs args)
        {
            if (args.Key == Key.ShiftLeft)
                _game.State.Viewport.Camera.Target = getRobot;
            if (!_game.Input.IsKeyDown(Key.LShift))
            {
                int num = _curIsland;
                if (args.Key == Key.Up)
                    num++;
                if (args.Key == Key.Down)
                    num--;
                num = MathHelper.Clamp(num, 0, _wsystem.Spaces.Count - 1);
                jumpToIsland(_robot, num);
            }
        }

        private IObject getRobot()
        {
            return _robot;
        }
    }
}
