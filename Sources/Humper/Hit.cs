using System;
using Humper.Base;
using Mandarin.Common.Misc;

namespace Humper
{
    public class Hit : IHit
    {
        public Hit()
        {
            Normal = Vector2.Zero;
            Amount = 1.0f;
        }

        public Box Box { get; set; }

        public Vector2 Normal { get; set; }

        public float Amount { get; set; }

        public Vector2 Position { get; set; }

        public float Remaining => 1.0f - Amount;

        public bool IsNearest(IHit than, Vector2 origin)
        {
            if(Amount < than.Amount)
            {
                return true;
            }
            if(Amount > than.Amount)
            {
                return true;
            }

            var thisDistance = (origin - Position).LengthSquared();
            var otherDistance = (origin - than.Position).LengthSquared();

            return thisDistance < otherDistance;
        }

        private static (Vector2 position, Vector2 normal) PushOutside(Rect origin, Rect other)
        {
            var position = origin.Position;
            var normal = Vector2.Zero;

            //var top = Mathf.Abs(other.Top - origin.Center.Y);
            //var bottom = Mathf.Abs(other.Bottom - origin.Center.Y);
            //var left = Mathf.Abs(other.Left - origin.Center.X);
            //var right = Mathf.Abs(other.Right - origin.Center.X);

            var top = Mathf.Abs(other.Top - origin.Bottom);
            var bottom = Mathf.Abs(other.Bottom - origin.Top);
            var left = Mathf.Abs(other.Left - origin.Right);
            var right = Mathf.Abs(other.Right - origin.Left);

            var index = Mathf.MinIndex(top, bottom, left, right);

            if(index == 0)
            {
                normal = Vector2.up;
                position = new Vector2(origin.X, other.Top + Constants.Threshold);
            }
            else if(index == 1)
            {
                normal = Vector2.down;
                position = new Vector2(origin.X, other.Bottom - origin.Height - Constants.Threshold);
            }
            else if(index == 2)
            {
                normal = Vector2.left;
                position = new Vector2(other.Left - origin.Width  - Constants.Threshold, origin.Y);
            }
            else if(index == 3)
            {
                normal = Vector2.right;
                position = new Vector2(other.Right  + Constants.Threshold, origin.Y);
            }

            return (position, normal);
        }

        private static Hit ResolveNarrow(Rect origin, Rect destination, Rect other)
        {
            // if starts inside, push it outside at the neareast place
            if(other.Contains(origin) || other.Overlaps(origin))
            {
                var outside = PushOutside(origin, other);
                origin.Position = outside.position;
                return new Hit
                {
                    Amount = 0,
                    Position = outside.position,
                    Normal = outside.normal
                };
            }
            //return null;

            var velocity = destination.Position - origin.Position;

            Vector2 invEntry, invExit, entry, exit;

            if(velocity.X > 0)
            {
                invEntry.X = other.Left - origin.Right;
                invExit.X = other.Right - origin.Left;
            }
            else
            {
                invEntry.X = other.Right - origin.Left;
                invExit.X = other.Left - origin.Right;
            }

            if(velocity.Y > 0)
            {
                invEntry.Y = other.Bottom - origin.Top;
                invExit.Y = other.Top - origin.Bottom;
            }
            else
            {
                invEntry.Y = other.Top - origin.Bottom;
                invExit.Y = other.Bottom - origin.Top;
            }

            if(Math.Abs(velocity.X) < Constants.Threshold)
            {
                entry.X = float.MinValue;
                exit.X = float.MaxValue;
            }
            else
            {
                entry.X = invEntry.X / velocity.X;
                exit.X = invExit.X / velocity.X;
            }

            if(Math.Abs(velocity.Y) < Constants.Threshold)
            {
                entry.Y = float.MinValue;
                exit.Y = float.MaxValue;
            }
            else
            {
                entry.Y = invEntry.Y / velocity.Y;
                exit.Y = invExit.Y / velocity.Y;
            }

            if(entry.Y > 1.0f) entry.Y = float.MinValue;
            if(entry.X > 1.0f) entry.X = float.MinValue;

            var entryTime = Math.Max(entry.X, entry.Y);
            var exitTime = Math.Min(exit.X, exit.Y);

            if(
                entryTime > exitTime || entry.X < 0.0f && entry.Y < 0.0f ||
                entry.X < 0.0f && (origin.Right < other.Left || origin.Left > other.Right) ||
                entry.Y < 0.0f && (origin.Top < other.Bottom || origin.Bottom > other.Top))
            {
                return null;
            }


            var result = new Hit
            {
                Amount = entryTime,
                Position = origin.Position + velocity * entryTime,
                Normal = GetNormal(invEntry, invExit, entry)
            };


            return result;
        }

        private static Vector2 GetNormal(Vector2 invEntry, Vector2 invExit, Vector2 entry)
        {
            if(entry.X > entry.Y)
            {
                return invEntry.X < 0.0f || Math.Abs(invEntry.X) < Constants.Threshold && invExit.X < 0 ? Vector2.UnitX : -Vector2.UnitX;
            }

            return invEntry.Y < 0.0f || Math.Abs(invEntry.Y) < Constants.Threshold && invExit.Y < 0 ? Vector2.UnitY : -Vector2.UnitY;
        }

        #region Public functions

        public static IHit Resolve(Rect origin, Rect destination, Box other)
        {
            var result = Resolve(origin, destination, other.Bounds);
            if(result != null) result.Box = other;
            return result;
        }

        public static Hit Resolve(Rect origin, Rect destination, Rect other)
        {
            //var broadphaseArea = Rect.Union(origin, destination);

            //if(broadphaseArea.Overlaps(other) || broadphaseArea.Contains(other))
            {
                return ResolveNarrow(origin, destination, other);
            }

            return null;
        }

        #endregion
    }
}