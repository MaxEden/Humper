using Humper.Base;
using Humper.Responses;
using Mandarin.Common.Misc;

namespace Humper
{
    public class SlideResponse : ICollisionResponse
    {
        public SlideResponse(ICollision collision)
        {
            var velocity = collision.Goal.Center - collision.Origin.Center;
            var normal = collision.Hit.Normal;
            //var dot = collision.Hit.Remaining * new Vector2(-velocity.X * normal.Y, velocity.Y * normal.X);
            //var slide = normal * dot;

            float dot = (velocity.X * normal.Y + velocity.Y * normal.X) * collision.Hit.Remaining;
            var slide = new Vector2(dot * normal.Y, dot * normal.X);

            Destination = new Rect(collision.Hit.Position + slide, collision.Goal.Size);
        }

        public Rect Destination { get; }
    }
}