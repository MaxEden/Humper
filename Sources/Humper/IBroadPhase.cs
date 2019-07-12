using System;
using System.Collections.Generic;
using Humper.Base;
using Mandarin.Common.Misc;

namespace Humper
{
    public interface IBroadPhase
    {
        Rect Bounds { get; }
        void Add(Box box);
        IList<Box> QueryBoxes(Rect area);
        bool Remove(Box box);
        void Update(Box box, Rect from);
        void DrawDebug(Rect area, Action<Rect, float> drawCell, Action<Box> drawBox, Action<string, int, int, float> drawString);
    }
}