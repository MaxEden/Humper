using System;
using System.Linq;
using Humper.Base;
using Humper.Responses;

namespace Humper
{
    public class Box
    {
        #region Constructors 

        public Box(World world, Rect area)
        {
            this.world = world;
            bounds = area;
        }

        #endregion

        #region Fields

        private readonly World world;

        private Rect bounds;

        #endregion

        #region Properties

        public Rect Bounds => bounds;

        public   object Data;
        internal object BroadPhaseData;

        #endregion

        #region Movements

        public IMovement Simulate(Vector2 destination, Func<ICollision, ICollisionResponse> filter)
        {
            return world.Simulate(this, destination, filter);
        }

        public IMovement Simulate(Vector2 destination, Func<ICollision, CollisionResponses> filter)
        {
            return Simulate(destination, col =>
            {
                if(col.Hit == null)
                {
                    return null;
                }

                return CollisionResponse.Create(col, filter(col));
            });
        }

        public IMovement Move(Vector2 destination, Func<ICollision, ICollisionResponse> filter)
        {
            var movement = Simulate(destination, filter);
            bounds.X = movement.Destination.X;
            bounds.Y = movement.Destination.Y;
            world.Update(this, movement.Origin);
            return movement;
        }

        public IMovement Move(Vector2 destination, Func<ICollision, CollisionResponses> filter)
        {
            var movement = Simulate(destination, filter);
            bounds.X = movement.Destination.X;
            bounds.Y = movement.Destination.Y;
            world.Update(this, movement.Origin);
            return movement;
        }

        #endregion

        #region Tags

        private Enum tags;

        public Box AddTags(params Enum[] newTags)
        {
            foreach(var tag in newTags)
            {
                AddTag(tag);
            }

            return this;
        }

        public Box RemoveTags(params Enum[] newTags)
        {
            foreach(var tag in newTags)
            {
                RemoveTag(tag);
            }

            return this;
        }

        private void AddTag(Enum tag)
        {
            if(tags == null)
            {
                tags = tag;
            }
            else
            {
                var t = tags.GetType();
                var ut = Enum.GetUnderlyingType(t);

                if(ut != typeof(ulong))
                {
                    tags = (Enum)Enum.ToObject(t, Convert.ToInt64(tags) | Convert.ToInt64(tag));
                }
                else
                {
                    tags = (Enum)Enum.ToObject(t, Convert.ToUInt64(tags) | Convert.ToUInt64(tag));
                }
            }
        }

        private void RemoveTag(Enum tag)
        {
            if(tags != null)
            {
                var t = tags.GetType();
                var ut = Enum.GetUnderlyingType(t);

                if(ut != typeof(ulong))
                {
                    tags = (Enum)Enum.ToObject(t, Convert.ToInt64(tags) & ~Convert.ToInt64(tag));
                }
                else
                {
                    tags = (Enum)Enum.ToObject(t, Convert.ToUInt64(tags) & ~Convert.ToUInt64(tag));
                }
            }
        }

        public bool HasTag(params Enum[] values)
        {
            return tags != null && values.Any(value => tags.HasFlag(value));
        }

        public bool HasTags(params Enum[] values)
        {
            return tags != null && values.All(value => tags.HasFlag(value));
        }

        #endregion
    }
}