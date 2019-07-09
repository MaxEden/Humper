using System.Collections.Generic;
using System.Linq;
using Humper.Base;
using Mandarin.Common.Misc;

namespace Humper
{
    public class Movement : IMovement
    {
        public Movement()
        {
            Hits = new IHit[0];
        }

        public IEnumerable<IHit> Hits { get; set; }

        public bool HasCollided => Hits.Any();

        public Rect Origin { get; set; }

        public Rect Destination { get; set; }

        public Rect Goal { get; set; }
    }
}