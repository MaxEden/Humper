using System.Collections.Generic;
using Humper.Base;

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