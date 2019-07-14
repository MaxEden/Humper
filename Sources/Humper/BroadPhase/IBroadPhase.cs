using System;
using System.Collections.Generic;
using Mandarin.Common.Misc;

namespace Humper
{
    public interface IBroadPhase
    {
        Rect Bounds { get; }
        void Add(Box box);
        void QueryBoxes(Rect area, ISet<Box> boxes);
        bool Remove(Box box);
        void Update(Box box, Rect from);
        void DrawDebug(Rect area, Action<Rect, float> drawCell, Action<Box> drawBox, Action<string, int, int, float> drawString);
    }
}