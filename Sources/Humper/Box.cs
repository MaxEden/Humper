namespace Humper
{
    using System;
    using System.Linq;
    using Base;
    using Responses;

    public class Box : IBox
    {
        #region Constructors 

        public Box(World world, RectangleF area)
        {
            this.world = world;
            this.bounds = area;
        }

        #endregion

        #region Fields

        private World world;

        private RectangleF bounds;

        #endregion

        #region Properties

        public RectangleF Bounds
        {
            get { return bounds; }
        }

        public object Data { get; set; }
        
        #endregion

        #region Movements

        public IMovement Simulate(Vector2 destination, Func<ICollision, ICollisionResponse> filter)
        {
            return world.Simulate(this, destination, filter);
        }

        public IMovement Simulate(Vector2 destination, Func<ICollision, CollisionResponses> filter)
        {
            return Simulate(destination, (col) =>
            {
                if(col.Hit == null)
                    return null;

                return CollisionResponse.Create(col, filter(col));
            });
        }

        public IMovement Move(Vector2 destination, Func<ICollision, ICollisionResponse> filter)
        {
            var movement = this.Simulate(destination, filter);
            this.bounds.X = movement.Destination.X;
            this.bounds.Y = movement.Destination.Y;
            this.world.Update(this, movement.Origin);
            return movement;
        }

        public IMovement Move(Vector2 destination, Func<ICollision, CollisionResponses> filter)
        {
            var movement = this.Simulate(destination, filter);
            this.bounds.X = movement.Destination.X;
            this.bounds.Y = movement.Destination.Y;
            this.world.Update(this, movement.Origin);
            return movement;
        }

        #endregion

        #region Tags

        private Enum tags;

        public IBox AddTags(params Enum[] newTags)
        {
            foreach(var tag in newTags)
            {
                this.AddTag(tag);
            }

            return this;
        }

        public IBox RemoveTags(params Enum[] newTags)
        {
            foreach(var tag in newTags)
            {
                this.RemoveTag(tag);
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
                var t = this.tags.GetType();
                var ut = Enum.GetUnderlyingType(t);

                if(ut != typeof(ulong))
                    this.tags = (Enum)Enum.ToObject(t, Convert.ToInt64(this.tags) | Convert.ToInt64(tag));
                else
                    this.tags = (Enum)Enum.ToObject(t, Convert.ToUInt64(this.tags) | Convert.ToUInt64(tag));
            }
        }

        private void RemoveTag(Enum tag)
        {
            if(tags != null)
            {
                var t = this.tags.GetType();
                var ut = Enum.GetUnderlyingType(t);

                if(ut != typeof(ulong))
                    this.tags = (Enum)Enum.ToObject(t, Convert.ToInt64(this.tags) & ~Convert.ToInt64(tag));
                else
                    this.tags = (Enum)Enum.ToObject(t, Convert.ToUInt64(this.tags) & ~Convert.ToUInt64(tag));
            }
        }

        public bool HasTag(params Enum[] values)
        {
            return (tags != null) && values.Any((value) => this.tags.HasFlag(value));
        }

        public bool HasTags(params Enum[] values)
        {
            return (tags != null) && values.All((value) => this.tags.HasFlag(value));
        }

        #endregion
    }
}