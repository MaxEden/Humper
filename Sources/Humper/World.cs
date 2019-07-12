﻿using System;
using System.Collections.Generic;
using System.Linq;
using Humper.Base;
using Humper.Responses;
using Mandarin.Common.Misc;

namespace Humper
{
    public class World
    {
        public Rect Bounds => _broadPhase.Bounds;
        public World(IBroadPhase broadPhase)
        {
            _broadPhase = broadPhase;
        }

        #region Diagnostics

        public void DrawDebug(Rect area, Action<Rect, float> drawCell, Action<Box> drawBox, Action<string, int, int, float> drawString)
        {
            // Drawing boxes
            var boxes = _broadPhase.QueryBoxes(area);
            foreach(var box in boxes)
            {
                drawBox(box);
            }

            _broadPhase.DrawDebug(area, drawCell, drawBox, drawString);
        }

        #endregion

        #region Boxes

        private readonly IBroadPhase _broadPhase;

        public int Boxes { get; private set; }

        public Box Create(Rect area)
        {
            var box = new Box(this, area);
            _broadPhase.Add(box);
            Boxes++;
            return box;
        }

        public IList<Box> Find(Rect area)
        {
            return _broadPhase.QueryBoxes(area);
        }

        public bool Remove(Box box)
        {
            Boxes--;
            return _broadPhase.Remove(box);
        }

        public void Update(Box box, Rect from)
        {
            _broadPhase.Update(box, from);
        }

        #endregion

        #region Hits

        public IHit Hit(Rect origin, Rect destination, IEnumerable<Box> ignoring = null)
        {
            var wrap = Rect.Union(origin, destination);
            var boxes = Find(wrap);

            if(ignoring != null)
            {
                boxes = boxes.Except(ignoring).ToList();
            }

            IHit nearest = null;

            foreach(var other in boxes)
            {
                var hit = Humper.Hit.Resolve(origin, destination, other);

                if(hit != null && (nearest == null || hit.IsNearest(nearest, origin.Position)))
                {
                    nearest = hit;
                }
            }

            return nearest;
        }

        #endregion

        #region Movements

        public IMovement Simulate(Box box, Vector2 destination,  CollisionResponse response)
        {
            var origin = box.Bounds;
            var goal = new Rect(destination, box.Bounds.Size);

            var hits = new List<IHit>();

            var result = new Movement
            {
                Origin = origin,
                Goal = goal,
                Destination = Simulate(hits, new List<Box>
                                           {box}, box, origin, goal, response),
                Hits = hits
            };

            return result;
        }

        private Rect Simulate(List<IHit> hits, List<Box> ignoring, Box box, Rect origin, Rect destination, CollisionResponse response)
        {
            var nearest = Hit(origin, destination, ignoring);

            if(nearest != null)
            {
                hits.Add(nearest);

                var impact = new Rect(nearest.Position, origin.Size);
                var collision = new Collision
                    {Box = box, Hit = nearest, Goal = destination, Origin = origin};
                var dest = response(collision);

                if(response != null && destination != dest)
                {
                    ignoring.Add(nearest.Box);
                    if(ignoring.Count > 20)
                    {
                        return dest;
                    }
                    else
                    {
                        return Simulate(hits, ignoring, box, impact, dest, response);
                    }
                }
            }

            return destination;
        }

        #endregion
    }
}