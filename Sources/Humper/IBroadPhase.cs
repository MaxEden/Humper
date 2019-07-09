using System;
using System.Collections.Generic;
using Humper.Base;

namespace Humper {
    public interface IBroadPhase {
        void Add(Box box);
        IEnumerable<Box> QueryBoxes(Rect area);
        Rect Bounds { get; }
        bool Remove(Box box);
        void Update(Box box, Rect @from);
        void DrawDebug(Rect area, Action<Rect, float> drawCell, Action<Box> drawBox, Action<string, int, int, float> drawString);
    }
}