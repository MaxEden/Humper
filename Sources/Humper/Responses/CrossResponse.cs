using Humper.Base;

namespace Humper.Responses
{
    public class CrossResponse : ICollisionResponse
    {
        public CrossResponse(ICollision collision)
        {
            Destination = collision.Goal;
        }

        public Rect Destination { get; }
    }
}