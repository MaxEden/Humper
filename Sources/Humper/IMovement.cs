using System.Collections.Generic;
using Humper.Base;
using Mandarin.Common.Misc;

namespace Humper
{
    public interface IMovement
    {
        IEnumerable<IHit> Hits { get; }

        bool HasCollided { get; }

        Rect Origin { get; }

        Rect Goal { get; }

        Rect Destination { get; }
    }
}