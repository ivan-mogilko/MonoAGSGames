using System.Collections.Generic;
using AGS.API;

namespace LayerGame
{
    public class WorldSystem
    {
        IList<WorldSpace> _spaces = new List<WorldSpace>();

        public IList<WorldSpace> Spaces { get => _spaces; }

        public PointF ScalePerDistance { get; set; }
        public PointF ParallaxPerDistance { get; set; }
        public PointF PerspectiveShiftPerDistance { get; set; }
        public float ShadePerDistance { get; set; }
    }
}
