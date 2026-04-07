using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Hooks;

internal sealed class HookManager : IDisposable
{
    private readonly NativeMethods.HookProc keyboardProc;
    private readonly NativeMethods.HookProc mouseProc;
    private readonly Action toggleVisibility;
    private readonly Rectangle cursorLockArea;

    private IntPtr keyboardHook = IntPtr.Zero;
    private IntPtr mouseHook = IntPtr.Zero;

    private bool leftCtrlDown;
    private bool rightCtrlDown;
    private bool leftShiftDown;
    private bool rightShiftDown;
    private bool leftAltDown;
    private bool rightAltDown;
    private bool secretHotkeyLatched;
    private bool disposed;

    public HookManager(Action toggleVisibility, Rectangle cursorLockArea)
    {
        this.toggleVisibility = toggleVisibility ?? throw new ArgumentNullException(nameof(toggleVisibility));
        this.cursorLockArea = cursorLockArea;

        keyboardProc = KeyboardHookCallback;
        mouseProc = MouseHookCallback;

        InstallHooks();
    }

    public bool AltDown => leftAltDown || rightAltDown;

    private void InstallHooks()
    {
        var moduleHandle = NativeMethods.GetModuleHandle(null);
        if (moduleHandle == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Не вдалося отримати модуль для підключення хуків.");
        }

        keyboardHook = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, keyboardProc, moduleHandle, 0);
        if (keyboardHook == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Не вдалося встановити клавіатурний хук.");
        }

        mouseHook = NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, mouseProc, moduleHandle, 0);
        if (mouseHook == IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(keyboardHook);
            keyboardHook = IntPtr.Zero;
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Не вдалося встановити хук миші.");
        }
    }

    private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= NativeMethods.HC_ACTION)
        {
            var message = wParam.ToInt32();
            var keyboardData = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
            var key = (Keys)keyboardData.vkCode;

            if (message is NativeMethods.WM_KEYDOWN or NativeMethods.WM_SYSKEYDOWN)
            {
                UpdateModifierState(key, true);

                if (!secretHotkeyLatched && IsSecretHotkeyPressed(key))
                {
                    secretHotkeyLatched = true;
                    toggleVisibility();
                }
            }
            else if (message is NativeMethods.WM_KEYUP or NativeMethods.WM_SYSKEYUP)
            {
                UpdateModifierState(key, false);

                if (key == Keys.Q)
                {
                    secretHotkeyLatched = false;
                }
            }
        }

        return NativeMethods.CallNextHookEx(keyboardHook, nCode, wParam, lParam);
    }

    private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= NativeMethods.HC_ACTION && wParam.ToInt32() == NativeMethods.WM_MOUSEMOVE && AltDown)
        {
            var mouseData = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);
            var clampedX = Math.Clamp(mouseData.pt.X, cursorLockArea.Left, cursorLockArea.Right - 1);
            var clampedY = Math.Clamp(mouseData.pt.Y, cursorLockArea.Top, cursorLockArea.Bottom - 1);

            if (clampedX != mouseData.pt.X || clampedY != mouseData.pt.Y)
            {
                NativeMethods.SetCursorPos(clampedX, clampedY);
                return (IntPtr)1;
            }
        }

        return NativeMethods.CallNextHookEx(mouseHook, nCode, wParam, lParam);
    }

    private void UpdateModifierState(Keys key, bool isDown)
    {
        switch (key)
        {
            case Keys.LControlKey:
            case Keys.ControlKey:
                leftCtrlDown = isDown;
                break;
            case Keys.RControlKey:
                rightCtrlDown = isDown;
                break;
            case Keys.LShiftKey:
            case Keys.ShiftKey:
                leftShiftDown = isDown;
                break;
            case Keys.RShiftKey:
                rightShiftDown = isDown;
                break;
            case Keys.LMenu:
            case Keys.Menu:
                leftAltDown = isDown;
                break;
            case Keys.RMenu:
                rightAltDown = isDown;
                break;
        }
    }

    private bool IsSecretHotkeyPressed(Keys key)
    {
        return key == Keys.Q && (leftCtrlDown || rightCtrlDown) && (leftShiftDown || rightShiftDown);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        if (keyboardHook != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(keyboardHook);
            keyboardHook = IntPtr.Zero;
        }

        if (mouseHook != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(mouseHook);
            mouseHook = IntPtr.Zero;
        }

        GC.SuppressFinalize(this);
    }
}