using Mandarin.Common.Misc;

namespace Humper
{
    public class Collision : ICollision
    {
        public bool HasCollided => Hit.IsHit;

        public Box Box { get; set; }

        public Box Other => Hit.Box;

        public Rect Origin { get; set; }

        public Rect Goal { get; set; }

        public Hit Hit { get; set; }
    }
}