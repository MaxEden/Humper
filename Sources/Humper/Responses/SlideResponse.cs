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
            var dot = collision.Hit.Remaining * (velocity.X * normal.Y + velocity.Y * normal.X);
            var slide = normal* dot;

            Destination = new Rect(collision.Hit.Position + slide, collision.Goal.Size);
        }

        public Rect Destination { get; }
    }
}