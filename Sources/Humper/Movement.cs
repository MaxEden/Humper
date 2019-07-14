using System.Collections.Generic;
using System.Linq;
using Humper.Base;
using Mandarin.Common.Misc;

namespace Humper
{
    public class Movement
    {
        public Movement()
        {
            Hits = new Hit[0];
        }

        public IEnumerable<Hit> Hits { get; set; }

        public bool HasCollided => Hits.Any();

        public Rect Origin { get; set; }

        public Rect Destination { get; set; }

        public Rect Goal { get; set; }
    }
}