using Humper.Base;
using Mandarin.Common.Misc;

namespace Humper.Responses
{
    public class TouchResponse : ICollisionResponse
    {
        public TouchResponse(ICollision collision)
        {
            Destination = new Rect(collision.Hit.Position, collision.Goal.Size);
        }

        public Rect Destination { get; }
    }
}