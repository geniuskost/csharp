using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SystemProgramming.HooksRegistry;

public class HookManager : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WH_MOUSE_LL = 14;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_LBUTTONDOWN = 0x0201;

    private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    private IntPtr _keyboardHookID = IntPtr.Zero;
    private IntPtr _mouseHookID = IntPtr.Zero;
    private HookProc _keyboardProc;
    private HookProc _mouseProc;

    private bool _isLogging;
    private readonly HashSet<Keys> _pressedKeys = new();
    private readonly string _logFilePath = "keylog.txt";

    private readonly Form _mainForm;
    private readonly Button _escapingButton;
    private readonly Random _random = new();

    public HookManager(Form mainForm, Button escapingButton)
    {
        _mainForm = mainForm;
        _escapingButton = escapingButton;

        _keyboardProc = KeyboardHookCallback;
        _mouseProc = MouseHookCallback;

        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            IntPtr moduleHandle = GetModuleHandle(curModule.ModuleName);
            _keyboardHookID = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, moduleHandle, 0);
            _mouseHookID = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, moduleHandle, 0);
        }
    }

    public void SetLogging(bool isLogging)
    {
        _isLogging = isLogging;
        if (!_isLogging)
        {
            _pressedKeys.Clear();
        }
    }

    private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && _isLogging)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            Keys key = (Keys)vkCode;

            if (wParam == (IntPtr)WM_KEYDOWN)
            {
                if (_pressedKeys.Add(key))
                {
                    LogKey(key);
                }
            }
            else if (wParam == (IntPtr)WM_KEYUP)
            {
                _pressedKeys.Remove(key);
            }
        }
        return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
    }

    private void LogKey(Keys key)
    {
        try
        {
            File.AppendAllText(_logFilePath, $"{key} ");
        }
        catch
        {
            // Ignore write errors to prevent interrupting normal workflow
        }
    }

    private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_LBUTTONDOWN)
        {
            MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            Point clickPoint = new Point(hookStruct.pt.x, hookStruct.pt.y);

            // Check if coordinates match the escaping button
            if (_mainForm.IsHandleCreated && !_mainForm.IsDisposed && _mainForm.Visible)
            {
                bool intercepted = false;
                _mainForm.Invoke(new Action(() =>
                {
                    Point buttonScreenLoc = _escapingButton.PointToScreen(Point.Empty);
                    Rectangle buttonRect = new Rectangle(buttonScreenLoc, _escapingButton.Size);

                    if (buttonRect.Contains(clickPoint))
                    {
                        MoveEscapingButton();
                        intercepted = true;
                    }
                }));

                if (intercepted)
                {
                    return (IntPtr)1; // Block the click
                }
            }
        }
        return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
    }

    private void MoveEscapingButton()
    {
        int maxX = _mainForm.ClientSize.Width - _escapingButton.Width;
        int maxY = _mainForm.ClientSize.Height - _escapingButton.Height;
        
        int newX = _random.Next(0, maxX);
        int newY = _random.Next(0, maxY);

        _escapingButton.Location = new Point(newX, newY);
    }

    public void Dispose()
    {
        UnhookWindowsHookEx(_keyboardHookID);
        UnhookWindowsHookEx(_mouseHookID);
    }
}
