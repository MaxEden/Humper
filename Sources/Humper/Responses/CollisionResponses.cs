using Mandarin.Common.Misc;

namespace Humper.Responses
{
    public delegate Rect CollisionResponse(ICollision collision);

    public static class Response
    {
        public static Rect Touch(ICollision collision)
        {
            return new Rect(collision.Hit.Position, collision.Goal.Size);
        }

        public static Rect Cross(ICollision collision)
        {
            return collision.Goal;
        }

        public static Rect Slide(ICollision collision)
        {
            var velocity = collision.Goal.Center - collision.Origin.Center;
            var normal = collision.Hit.Normal;

            float dot = (velocity.X * normal.Y + velocity.Y * normal.X) * collision.Hit.Remaining;
            var slide = new Vector2(dot * normal.Y, dot * normal.X);

            return new Rect(collision.Hit.Position + slide, collision.Goal.Size);
        }

        public static Rect Bounce(ICollision collision)
        {
            var velocity = collision.Goal.Center - collision.Origin.Center;
            var deflected = velocity * collision.Hit.Amount;

            if(Mathf.Abs(collision.Hit.Normal.X) > 0.00001f)
            {
                deflected.X *= -1;
            }

            if(Mathf.Abs(collision.Hit.Normal.Y) > 0.00001f)
            {
                deflected.Y *= -1;
            }

            return new Rect(collision.Hit.Position + deflected, collision.Goal.Size);
        }
    }
}