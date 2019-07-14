using System;
using System.Collections.Generic;
using System.Linq;
using Humper.Base;
using Humper.Responses;
using Mandarin.Common;
using Mandarin.Common.Misc;

namespace Humper
{
    public class World
    {
        private Pool _pool = new Pool();
        public  Rect Bounds => _broadPhase.Bounds;
        public World(IBroadPhase broadPhase)
        {
            _broadPhase = broadPhase;
        }

        #region Diagnostics

        public void DrawDebug(Rect area, Action<Rect, float> drawCell, Action<Box> drawBox, Action<string, int, int, float> drawString)
        {
            // Drawing boxes
            foreach(var box in Boxes)
            {
                drawBox(box);
            }

            _broadPhase.DrawDebug(area, drawCell, drawBox, drawString);
        }

        #endregion

        #region Boxes

        private readonly IBroadPhase _broadPhase;
        public readonly  List<Box>   Boxes = new List<Box>();

        public Box Create(Rect area)
        {
            var box = new Box(this, area);
            Boxes.Add(box);
            _broadPhase.Add(box);
            return box;
        }

        public void Find(Rect area, ISet<Box> boxes)
        {
            _broadPhase.QueryBoxes(area, boxes);
        }

        public bool Remove(Box box)
        {
            Boxes.Remove(box);
            return _broadPhase.Remove(box);
        }

        public void Update(Box box, Rect from)
        {
            _broadPhase.Update(box, from);
        }

        #endregion

        #region Hits

        public Hit Hit(Rect origin, Rect destination, IEnumerable<Box> ignoring = null)
        {
            var boxes = _pool.GetHashSet<Box>();
            var wrap = Rect.Union(origin, destination);
            Find(wrap, boxes);

            if(ignoring != null)
            {
                boxes.ExceptWith(ignoring);
            }

            Hit nearest = new Hit();

            foreach(var other in boxes)
            {
                var hit = Humper.Hit.Resolve(origin, destination, other);

                if(hit.IsHit && (!nearest.IsHit || hit.IsNearest(nearest, origin.Position)))
                {
                    nearest = hit;
                }
            }
            _pool.Return(boxes);
            return nearest;
        }

        #endregion

        #region Movements

        public Movement Simulate(Box box, Vector2 destination, CollisionResponse response)
        {
            var origin = box.Bounds;
            var goal = new Rect(destination, box.Bounds.Size);

            var hits = new List<Hit>();

            var result = new Movement
            {
                Origin = origin,
                Goal = goal,
                Destination = SimulateDestination(hits, new List<Box>
                                                      {box}, box, origin, goal, response),
                Hits = hits
            };

            return result;
        }

        private Rect SimulateDestination(List<Hit> hits, List<Box> ignoring, Box box, Rect origin, Rect destination, CollisionResponse response)
        {
            var nearest = Hit(origin, destination, ignoring);

            if(nearest.IsHit)
            {
                hits.Add(nearest);

                var impact = new Rect(nearest.Position, origin.Size);
                var collision = new Collision
                    {Box = box, Hit = nearest, Goal = destination, Origin = origin};

                var dest = response(collision);

                if(dest != null && destination != dest)
                {
                    ignoring.Add(nearest.Box);
                    if(ignoring.Count > 2)
                    {
                        return dest.Value;
                    }
                    else
                    {
                        return SimulateDestination(hits, ignoring, box, impact, dest.Value, response);
                    }
                }
            }

            return destination;
        }

        #endregion
    }
}