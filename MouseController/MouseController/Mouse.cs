using System.Runtime.InteropServices;

namespace MouseController
{
    public static class Mouse
    {
        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        static extern void SetCursorPos(int X, int Y);
        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int i1 = 0, int i2 = 0, int i3 = 0, int i4 = 0);
        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int x { get; set; }
            public int y { get; set; }

            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out Point lpPoint);
        public static class Buttons
        {
            public class Button
            {
                public int Up { get; private set; }
                public int Down { get; private set; }
                public Button(int Up, int Down)
                {
                    this.Up = Up;
                    this.Down = Down;
                }
            }
            public static Button Left { get; } = new Button(0x0004, 0x0002);
            public static Button Middle { get; } = new Button(0x0040, 0x0020);
            public static Button Right { get; } = new Button(0x0010, 0x0008);

        }
        public static Point Position
        {
            get
            {
                Point point;
                GetCursorPos(out point);
                return point;
            }
            set
            {
                SetCursorPos(value.x, value.y);
            }
        }
        public static void Simulate(int ev)
        {
            mouse_event(ev);
        }
        public static void Click(Buttons.Button btn)
        {
            Simulate(btn.Down);
            Simulate(btn.Up);
        }
        public static void Move(int x, int y)
        {
            Point p = Position;
            Position = new Point(p.x + x, p.y + y);
        }
    }
}
