using System;

namespace CStick
{
    public enum FlickDirection
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public static class StickFlick
    {
        public static FlickDirection FromMagnitudes(float up, float down, float left, float right, float threshold = 0.5f)
        {
            float x = right - left;
            float y = up - down;
            if (Math.Abs(x) < threshold && Math.Abs(y) < threshold)
            {
                return FlickDirection.None;
            }

            if (Math.Abs(y) >= Math.Abs(x))
            {
                return y > 0f ? FlickDirection.Up : FlickDirection.Down;
            }

            return x > 0f ? FlickDirection.Right : FlickDirection.Left;
        }

        public static bool IsNewFlick(FlickDirection previous, FlickDirection current)
        {
            return current != FlickDirection.None && previous == FlickDirection.None;
        }
    }
}
