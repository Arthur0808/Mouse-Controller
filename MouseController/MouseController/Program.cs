using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace MouseController
{
    class InterceptKeys
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static bool isActive, leftP, rightP, upP, downP, mlP, mrP;
        private static int dx, dy, speed, delayTime;
        private static Thread thread;
        public static void Main()
        {
            ShowWindow(GetConsoleWindow(), 0);
            isActive = false;
            leftP = false;
            rightP = false;
            upP = false;
            downP = false;
            mlP = false;
            mrP = false;
            dx = 0;
            dy = 0;
            delayTime = 1;
            speed = 1;
            thread = new Thread(new ThreadStart(NewThread));
            thread.Start();
            thread.Suspend();
            _hookID = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookID);
        }

        static void NewThread()
        {
            while (true)
            {
                if (dx != 0 || dy != 0)
                {
                    Mouse.Point p = Mouse.Position;
                    p.x += dx;
                    p.y += dy;
                    Mouse.Position = p;
                }
                Thread.Sleep(delayTime);
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);
        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    if(vkCode == 110 || vkCode == 96 || vkCode == 107 || vkCode == 109)
                        return (IntPtr)1;
                    if (isActive)
                    {
                        if (vkCode == 97 || vkCode == 35)
                        {
                            if (!leftP)
                            {
                                leftP = true;
                                dx -= speed;
                            }
                            return (IntPtr)1;
                        }
                        if (vkCode == 98 || vkCode == 40)
                        {
                            if (!downP)
                            {
                                downP = true;
                                dy += speed;
                            }
                            return (IntPtr)1;
                        }
                        if (vkCode == 99 || vkCode == 34)
                        {
                            if (!rightP)
                            {
                                rightP = true;
                                dx += speed;
                            }
                            return (IntPtr)1;
                        }
                        if (vkCode == 101 || vkCode == 12)
                        {
                            if (!upP)
                            {
                                upP = true;
                                dy -= speed;
                            }
                            return (IntPtr)1;
                        }
                        if (vkCode == 100)
                        {
                            if (!mlP)
                            {
                                mlP = true;
                                Mouse.Simulate(Mouse.Buttons.Left.Down);
                            }
                            return (IntPtr)1;
                        }
                        if (vkCode == 102)
                        {
                            if (!mrP)
                            {
                                mrP = true;
                                Mouse.Simulate(Mouse.Buttons.Right.Down);
                            }
                            return (IntPtr)1;
                        }
                    }
                }
                else if(wParam == (IntPtr)WM_KEYUP)
                {
                    if (vkCode == 110)
                    {
                        isActive = !isActive;
                        if (isActive) thread.Resume();
                        else thread.Suspend();
                        return (IntPtr)1;
                    }
                    if (isActive)
                    {
                        if (vkCode == 97 || vkCode == 35)
                        {
                            leftP = false;
                            dx += speed;
                            return (IntPtr)1;
                        }
                        if (vkCode == 98 || vkCode == 40)
                        {
                            downP = false;
                            dy -= speed;
                            return (IntPtr)1;
                        }
                        if (vkCode == 99 || vkCode == 34)
                        {
                            rightP = false;
                            dx -= speed;
                            return (IntPtr)1;
                        }
                        if (vkCode == 101 || vkCode == 12)
                        {
                            upP = false;
                            dy += speed; 
                            return (IntPtr)1;
                        }
                        if (vkCode == 100)
                        {
                            mlP = false;
                            Mouse.Simulate(Mouse.Buttons.Left.Up);
                            return (IntPtr)1;
                        }
                        if (vkCode == 102)
                        {
                            mrP = false;
                            Mouse.Simulate(Mouse.Buttons.Right.Up);
                            return (IntPtr)1;
                        }
                        if (vkCode == 107 && dx == 0 && dy == 0)
                        {
                            if (delayTime < 2)
                                speed++;
                            else
                                delayTime--;
                            return (IntPtr)1;
                        }
                        if (vkCode == 109 && dx == 0 && dy == 0)
                        {
                            if (speed > 1)
                                speed--;
                            else
                                delayTime++;
                            return (IntPtr)1;
                        }
                    }
                    if (vkCode == 96)
                    {
                        if (dx != 0 || dy != 0)
                        {
                            dx = 0;
                            dy = 0;
                        }
                        else
                        {
                            thread.Suspend();
                            Application.Exit();
                            Environment.Exit(0);
                        }
                        return (IntPtr)1;
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
