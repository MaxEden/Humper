using System;
using System.Diagnostics;

namespace Humper.Base
{
    /// <summary>
    ///     Describes a floating point 2D-rectangle.
    /// </summary>
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Rect : IEquatable<Rect>
    {
        /// <summary>
        ///     The x coordinate of the top-left corner of this <see cref="Rect" />.
        /// </summary>
        public float X;

        /// <summary>
        ///     The y coordinate of the top-left corner of this <see cref="Rect" />.
        /// </summary>
        public float Y;

        public Vector2 Min
        {
            get => new Vector2(Left, Bottom);
            set
            {
                X = value.X;
                Height = value.Y - Y;
            }
        }

        public Vector2 Max
        {
            get => new Vector2(Right, Top);
            set
            {
                Width = value.X - X;
                Y = value.Y;
            }
        }

        public Rect Expand(Vector2 indents)
        {
            return new Rect(Min - indents, Max + 2 * indents);
        }
        /// <summary>
        ///     The width of this <see cref="Rect" />.
        /// </summary>
        public float Width;

        /// <summary>
        ///     The height of this <see cref="Rect" />.
        /// </summary>
        public float Height;

        /// <summary>
        ///     Returns a <see cref="Rect" /> with X=0, Y=0, Width=0, Height=0.
        /// </summary>
        public static Rect Empty { get; } = new Rect();

        /// <summary>
        ///     Returns the x coordinate of the left edge of this <see cref="Rect" />.
        /// </summary>
        public float Left
        {
            get => X;
            set => X = value;
        }

        /// <summary>
        ///     Returns the x coordinate of the right edge of this <see cref="Rect" />.
        /// </summary>
        public float Right
        {
            get => X + Width;
            set => Width = value - X;
        }

        /// <summary>
        ///     Returns the y coordinate of the top edge of this <see cref="Rect" />.
        /// </summary>
        public float Top
        {
            get => Y;
            set => Y = value;
        }

        /// <summary>
        ///     Returns the y coordinate of the bottom edge of this <see cref="Rect" />.
        /// </summary>
        public float Bottom
        {
            get => Y + Height;
            set => Height = value - Y;
        }

        /// <summary>
        ///     Whether or not this <see cref="Rect" /> has a <see cref="Width" /> and
        ///     <see cref="Height" /> of 0, and a <see cref="Location" /> of (0, 0).
        /// </summary>
        public bool IsEmpty => Width.Equals(0) && Height.Equals(0) && X.Equals(0) && Y.Equals(0);

        /// <summary>
        ///     The top-left coordinates of this <see cref="Rect" />.
        /// </summary>
        public Vector2 Location
        {
            get => new Vector2(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        /// <summary>
        ///     The width-height coordinates of this <see cref="Rect" />.
        /// </summary>
        public Vector2 Size
        {
            get => new Vector2(Width, Height);
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        public float Perimeter => 2.0f * (Width + Height);

        /// <summary>
        ///     A <see cref="Vector2" /> located in the center of this <see cref="Rect" />.
        /// </summary>
        public Vector2 Center => new Vector2(X + Width / 2f, Y + Height / 2f);

        internal string DebugDisplayString => string.Concat(X, "  ", Y, "  ", Width, "  ", Height);

        /// <summary>
        ///     Creates a new instance of <see cref="Rect" /> struct, with the specified
        ///     position, width, and height.
        /// </summary>
        /// <param name="x">The x coordinate of the top-left corner of the created <see cref="Rect" />.</param>
        /// <param name="y">The y coordinate of the top-left corner of the created <see cref="Rect" />.</param>
        /// <param name="width">The width of the created <see cref="Rect" />.</param>
        /// <param name="height">The height of the created <see cref="Rect" />.</param>
        public Rect(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Humper.Base.RectangleF" /> that contains the two given rectangles.
        /// </summary>
        /// <param name="one">One.</param>
        /// <param name="two">Two.</param>
        public Rect(Rect one, Rect two)
        {
            var left = Math.Min(one.Left, two.Left);
            var right = Math.Max(one.Right, two.Right);
            var top = Math.Min(one.Top, two.Top);
            var bottom = Math.Max(one.Bottom, two.Bottom);

            X = left;
            Y = top;
            Width = right - left;
            Height = bottom - top;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="Rect" /> struct, with the specified
        ///     location and size.
        /// </summary>
        /// <param name="location">The x and y coordinates of the top-left corner of the created <see cref="Rect" />.</param>
        /// <param name="size">The width and height of the created <see cref="Rect" />.</param>
        public Rect(Vector2 location, Vector2 size)
        {
            X = location.X;
            Y = location.Y;
            Width = size.X;
            Height = size.Y;
        }

        /// <summary>
        ///     Compares whether two <see cref="Rect" /> instances are equal.
        /// </summary>
        /// <param name="a"><see cref="Rect" /> instance on the left of the equal sign.</param>
        /// <param name="b"><see cref="Rect" /> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Rect a, Rect b)
        {
            const float epsilon = 0.00001f;
            return Math.Abs(a.X - b.X) < epsilon
                   && Math.Abs(a.Y - b.Y) < epsilon
                   && Math.Abs(a.Width - b.Width) < epsilon
                   && Math.Abs(a.Height - b.Height) < epsilon;
        }

        /// <summary>
        ///     Compares whether two <see cref="Rect" /> instances are not equal.
        /// </summary>
        /// <param name="a"><see cref="Rect" /> instance on the left of the not equal sign.</param>
        /// <param name="b"><see cref="Rect" /> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(Rect a, Rect b)
        {
            return !(a == b);
        }

        /// <summary>
        ///     Gets whether or not the provided coordinates lie within the bounds of this <see cref="Rect" />.
        /// </summary>
        /// <param name="x">The x coordinate of the point to check for containment.</param>
        /// <param name="y">The y coordinate of the point to check for containment.</param>
        /// <returns><c>true</c> if the provided coordinates lie inside this <see cref="Rect" />; <c>false</c> otherwise.</returns>
        public bool Contains(int x, int y)
        {
            return X <= x && x < X + Width && Y <= y && y < Y + Height;
        }

        public Rect GetBoundingRectangle()
        {
            return this;
        }

        /// <summary>
        ///     Gets whether or not the provided coordinates lie within the bounds of this <see cref="Rect" />.
        /// </summary>
        /// <param name="x">The x coordinate of the point to check for containment.</param>
        /// <param name="y">The y coordinate of the point to check for containment.</param>
        /// <returns><c>true</c> if the provided coordinates lie inside this <see cref="Rect" />; <c>false</c> otherwise.</returns>
        public bool Contains(float x, float y)
        {
            return X <= x && x < X + Width && Y <= y && y < Y + Height;
        }

        /// <summary>
        ///     Gets whether or not the provided <see cref="Vector2" /> lies within the bounds of this <see cref="Rect" />.
        /// </summary>
        /// <param name="value">The coordinates to check for inclusion in this <see cref="Rect" />.</param>
        /// <returns>
        ///     <c>true</c> if the provided <see cref="Vector2" /> lies inside this <see cref="Rect" />; <c>false</c>
        ///     otherwise.
        /// </returns>
        public bool Contains(Vector2 value)
        {
            return X <= value.X && value.X < X + Width && Y <= value.Y && value.Y < Y + Height;
        }

        /// <summary>
        ///     Gets whether or not the provided <see cref="Vector2" /> lies within the bounds of this <see cref="Rect" />.
        /// </summary>
        /// <param name="value">The coordinates to check for inclusion in this <see cref="Rect" />.</param>
        /// <param name="result">
        ///     <c>true</c> if the provided <see cref="Vector2" /> lies inside this <see cref="Rect" />;
        ///     <c>false</c> otherwise. As an output parameter.
        /// </param>
        public void Contains(ref Vector2 value, out bool result)
        {
            result = X <= value.X && value.X < X + Width && Y <= value.Y && value.Y < Y + Height;
        }

        /// <summary>
        ///     Gets whether or not the provided <see cref="Rect" /> lies within the bounds of this <see cref="Rect" />.
        /// </summary>
        /// <param name="value">The <see cref="Rect" /> to check for inclusion in this <see cref="Rect" />.</param>
        /// <returns>
        ///     <c>true</c> if the provided <see cref="Rect" />'s bounds lie entirely inside this <see cref="Rect" />;
        ///     <c>false</c> otherwise.
        /// </returns>
        public bool Contains(Rect value)
        {
            return X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y && value.Y + value.Height <= Y + Height;
        }

        /// <summary>
        ///     Gets whether or not the provided <see cref="Rect" /> lies within the bounds of this <see cref="Rect" />.
        /// </summary>
        /// <param name="value">The <see cref="Rect" /> to check for inclusion in this <see cref="Rect" />.</param>
        /// <param name="result">
        ///     <c>true</c> if the provided <see cref="Rect" />'s bounds lie entirely inside this
        ///     <see cref="Rect" />; <c>false</c> otherwise. As an output parameter.
        /// </param>
        public void Contains(ref Rect value, out bool result)
        {
            result = X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y && value.Y + value.Height <= Y + Height;
        }

        /// <summary>
        ///     Compares whether current instance is equal to specified <see cref="Object" />.
        /// </summary>
        /// <param name="obj">The <see cref="Object" /> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            return obj is Rect && this == (Rect)obj;
        }

        /// <summary>
        ///     Compares whether current instance is equal to specified <see cref="Rect" />.
        /// </summary>
        /// <param name="other">The <see cref="Rect" /> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Rect other)
        {
            return this == other;
        }

        /// <summary>
        ///     Gets the hash code of this <see cref="Rect" />.
        /// </summary>
        /// <returns>Hash code of this <see cref="Rect" />.</returns>
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode();
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }

        /// <summary>
        ///     Adjusts the edges of this <see cref="Rect" /> by specified horizontal and vertical amounts.
        /// </summary>
        /// <param name="horizontalAmount">Value to adjust the left and right edges.</param>
        /// <param name="verticalAmount">Value to adjust the top and bottom edges.</param>
        public void Inflate(int horizontalAmount, int verticalAmount)
        {
            X -= horizontalAmount;
            Y -= verticalAmount;
            Width += horizontalAmount * 2;
            Height += verticalAmount * 2;
        }

        /// <summary>
        ///     Adjusts the edges of this <see cref="Rect" /> by specified horizontal and vertical amounts.
        /// </summary>
        /// <param name="horizontalAmount">Value to adjust the left and right edges.</param>
        /// <param name="verticalAmount">Value to adjust the top and bottom edges.</param>
        public void Inflate(float horizontalAmount, float verticalAmount)
        {
            X -= horizontalAmount;
            Y -= verticalAmount;
            Width += horizontalAmount * 2;
            Height += verticalAmount * 2;
        }

        /// <summary>
        ///     Gets whether or not the other <see cref="Rect" /> intersects with this RectangleF.
        /// </summary>
        /// <param name="value">The other rectangle for testing.</param>
        /// <returns><c>true</c> if other <see cref="Rect" /> intersects with this rectangle; <c>false</c> otherwise.</returns>
        public bool Intersects(Rect value)
        {
            return value.Left < Right && Left < value.Right &&
                   value.Top < Bottom && Top < value.Bottom;
        }


        /// <summary>
        ///     Gets whether or not the other <see cref="Rect" /> intersects with this rectangle.
        /// </summary>
        /// <param name="value">The other rectangle for testing.</param>
        /// <param name="result">
        ///     <c>true</c> if other <see cref="Rect" /> intersects with this rectangle; <c>false</c> otherwise.
        ///     As an output parameter.
        /// </param>
        public void Intersects(ref Rect value, out bool result)
        {
            result = value.Left < Right && Left < value.Right &&
                     value.Top < Bottom && Top < value.Bottom;
        }

        /// <summary>
        ///     Creates a new <see cref="Rect" /> that contains overlapping region of two other rectangles.
        /// </summary>
        /// <param name="value1">The first <see cref="Rect" />.</param>
        /// <param name="value2">The second <see cref="Rect" />.</param>
        /// <returns>Overlapping region of the two rectangles.</returns>
        public static Rect Intersect(Rect value1, Rect value2)
        {
            Rect rectangle;
            Intersect(ref value1, ref value2, out rectangle);
            return rectangle;
        }

        /// <summary>
        ///     Creates a new <see cref="Rect" /> that contains overlapping region of two other rectangles.
        /// </summary>
        /// <param name="value1">The first <see cref="Rect" />.</param>
        /// <param name="value2">The second <see cref="Rect" />.</param>
        /// <param name="result">Overlapping region of the two rectangles as an output parameter.</param>
        public static void Intersect(ref Rect value1, ref Rect value2, out Rect result)
        {
            if(value1.Intersects(value2))
            {
                var rightSide = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
                var leftSide = Math.Max(value1.X, value2.X);
                var topSide = Math.Max(value1.Y, value2.Y);
                var bottomSide = Math.Min(value1.Y + value1.Height, value2.Y + value2.Height);
                result = new Rect(leftSide, topSide, rightSide - leftSide, bottomSide - topSide);
            }
            else
            {
                result = new Rect(0, 0, 0, 0);
            }
        }

        public Rect Offset(Vector2 amount)
        {
            return new Rect(Location + amount, Size);
        }
        /// <summary>
        ///     Returns a <see cref="String" /> representation of this <see cref="Rect" /> in the format:
        ///     {X:[<see cref="X" />] Y:[<see cref="Y" />] Width:[<see cref="Width" />] Height:[<see cref="Height" />]}
        /// </summary>
        /// <returns><see cref="String" /> representation of this <see cref="Rect" />.</returns>
        public override string ToString()
        {
            return "{X:" + X + " Y:" + Y + " Width:" + Width + " Height:" + Height + "}";
        }

        /// <summary>
        ///     Creates a new <see cref="Rect" /> that completely contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first <see cref="Rect" />.</param>
        /// <param name="value2">The second <see cref="Rect" />.</param>
        /// <returns>The union of the two rectangles.</returns>
        public static Rect Union(Rect value1, Rect value2)
        {
            var x = Math.Min(value1.X, value2.X);
            var y = Math.Min(value1.Y, value2.Y);
            return new Rect(x, y,
                            Math.Max(value1.Right, value2.Right) - x,
                            Math.Max(value1.Bottom, value2.Bottom) - y);
        }

        /// <summary>
        ///     Creates a new <see cref="Rect" /> that completely contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first <see cref="Rect" />.</param>
        /// <param name="value2">The second <see cref="Rect" />.</param>
        /// <param name="result">The union of the two rectangles as an output parameter.</param>
        public static void Union(ref Rect value1, ref Rect value2, out Rect result)
        {
            result = default;
            result.X = Math.Min(value1.X, value2.X);
            result.Y = Math.Min(value1.Y, value2.Y);
            result.Width = Math.Max(value1.Right, value2.Right) - result.X;
            result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
        }

        /// <summary>
        ///     Creates a new <see cref="Rect" /> from two points.
        /// </summary>
        /// <param name="point0">The top left or bottom right corner</param>
        /// <param name="point1">The bottom left or top right corner</param>
        /// <returns></returns>
        public static Rect FromPoints(Vector2 point0, Vector2 point1)
        {
            var x = Math.Min(point0.X, point1.X);
            var y = Math.Min(point0.Y, point1.Y);
            var width = Math.Abs(point0.X - point1.X);
            var height = Math.Abs(point0.Y - point1.Y);
            var rectangle = new Rect(x, y, width, height);
            return rectangle;
        }

        /// <summary>
        ///     Calculates the signed depth of intersection between two rectangles.
        /// </summary>
        /// <returns>
        ///     The amount of overlap between two intersecting rectangles. These
        ///     depth values can be negative depending on which wides the rectangles
        ///     intersect. This allows callers to determine the correct direction
        ///     to push objects in order to resolve collisions.
        ///     If the rectangles are not intersecting, Vector2.Zero is returned.
        /// </returns>
        public Vector2 IntersectionDepth(Rect other)
        {
            // Calculate half sizes.
            var thisHalfWidth = Width / 2.0f;
            var thisHalfHeight = Height / 2.0f;
            var otherHalfWidth = other.Width / 2.0f;
            var otherHalfHeight = other.Height / 2.0f;

            // Calculate centers.
            var centerA = new Vector2(Left + thisHalfWidth, Top + thisHalfHeight);
            var centerB = new Vector2(other.Left + otherHalfWidth, other.Top + otherHalfHeight);

            // Calculate current and minimum-non-intersecting distances between centers.
            var distanceX = centerA.X - centerB.X;
            var distanceY = centerA.Y - centerB.Y;
            var minDistanceX = thisHalfWidth + otherHalfWidth;
            var minDistanceY = thisHalfHeight + otherHalfHeight;

            // If we are not intersecting at all, return (0, 0).
            if(Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
            {
                return Vector2.Zero;
            }

            // Calculate and return intersection depths.
            var depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
            var depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;
            return new Vector2(depthX, depthY);
        }

        public Vector2 Extents => 0.5f * new Vector2(Width, Height);
    }

    public struct RayCastInput
    {
        public Vector2 From, To;
        public float   MaxFraction;
    }
}