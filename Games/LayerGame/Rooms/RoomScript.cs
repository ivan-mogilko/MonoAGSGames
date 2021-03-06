﻿using System.Threading.Tasks;
using AGS.API;

namespace LayerGame
{
    // TODO: move the class to shared library
    public abstract class RoomScript
    {
        protected readonly IGame _game;
        protected readonly string _roomAssetFolder;

        protected IRoom _room;

        public IRoom Room { get => _room; }

        public RoomScript(IGame game, string assetFolder)
        {
            _game = game;
            _roomAssetFolder = Assets.RoomAssetFolder + assetFolder + "/";
        }

        public async Task<IRoom> PrepareRoom()
        {
            if (_room != null)
                return _room;

            _room = await loadAsync();
            _game.State.Rooms.Add(_room);
            _room.Events.OnBeforeFadeIn.Subscribe(onActivate);
            _room.Events.OnAfterFadeOut.Subscribe(onDeactivate);
            return _room;
        }

        public async Task GotoAsync()
        {
            await PrepareRoom();
            await _game.State.ChangeRoomAsync(_room);
        }

        protected virtual async Task<IRoom> loadAsync() { return null; }
        protected virtual void onActivate() { }
        protected virtual void onDeactivate()
        {
            _game.State.Viewport.Pivot = new PointF();
            _game.State.Viewport.ScaleX = 1f;
            _game.State.Viewport.ScaleY = 1f;
            _game.State.Viewport.Angle = 0f;
        }

        protected IObject getObject(string name, IImage image, int x = 0, int y = 0)
        {
            IObject o = _game.Factory.Object.GetObject(name);
            o.Image = image;
            o.X = x;
            o.Y = y;
            return o;
        }

        protected async Task<IObject> getObject(string name, string gfile, int x = 0, int y = 0)
        {
            return getObject(name,
                await _game.Factory.Graphics.LoadImageAsync(_roomAssetFolder + gfile),
                x, y);
        }

        protected IObject addObject(string name, IImage image, int x = 0, int y = 0)
        {
            IObject o = getObject(name, image, x, y);
            _room.Objects.Add(o);
            return o;
        }

        protected async Task<IObject> addObject(string name, string gfile, int x = 0, int y = 0)
        {
            return addObject(name,
                await _game.Factory.Graphics.LoadImageAsync(_roomAssetFolder + gfile),
                x, y);
        }

        /// <summary>
        /// Inverts vector Y coordinate, transforming it from AGS to MonoAGS-compatible
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        protected Vector2 compatVector(int x, int y)
        {
            return new Vector2(x, _room.Limits.Height - y);
        }
    }
}
