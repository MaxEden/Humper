using System;
using Humper.Base;
using Mandarin.Common.Misc;

namespace Humper.Responses
{
    public class BounceResponse : ICollisionResponse
    {
        public BounceResponse(ICollision collision)
        {
            var velocity = collision.Goal.Center - collision.Origin.Center;
            var deflected = velocity * collision.Hit.Amount;

            if(Math.Abs(collision.Hit.Normal.X) > 0.00001f)
            {
                deflected.X *= -1;
            }

            if(Math.Abs(collision.Hit.Normal.Y) > 0.00001f)
            {
                deflected.Y *= -1;
            }

            Destination = new Rect(collision.Hit.Position + deflected, collision.Goal.Size);
        }

        public Rect Destination { get; }
    }
}