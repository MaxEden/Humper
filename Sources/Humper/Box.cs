using System;
using System.Linq;
using Humper.Responses;
using Mandarin.Common.Misc;

namespace Humper
{
    public class Box
    {
        private readonly World _world;
        private          Rect  _bounds;
        public           bool  IsActive;
        
        public Rect Bounds => _bounds;
        public   object Data;

        public Box(World world, Rect area)
        {
            this._world = world;
            _bounds = area;
        }

        #region Movements

        public Movement Move(Vector2 destination, CollisionResponse filter)
        {
            IsActive = true;
            var movement = _world.Simulate(this, destination, filter);
            _bounds = movement.Destination;
            _world.Update(this, movement.Origin);
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