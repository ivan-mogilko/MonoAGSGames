using System.Collections.Generic;
using AGS.API;

namespace LayerGame
{
    public class WorldSystem
    {
        readonly string _id;
        readonly IGame _game;
        readonly IList<WorldSpace> _spaces = new List<WorldSpace>();

        public string ID { get => _id; }
        public IGame Game { get => _game; }
        public IList<WorldSpace> Spaces { get => _spaces; }

        public float Baseline { get; set; }

        public PointF ScalePerDistance { get; set; }
        public PointF ParallaxPerDistance { get; set; }
        public PointF PerspectiveShiftPerDistance { get; set; }

        public WorldSystem(string id, IGame game)
        {
            _id = id;
            _game = game;
        }
    }
}
