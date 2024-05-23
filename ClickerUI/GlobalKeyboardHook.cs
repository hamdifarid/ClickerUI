using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class GlobalKeyboardHook
{
    // Import the necessary WinAPI functions
    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll")]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    // Define the delegate for the keyboard hook procedure
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    // Define constants
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

    // Define fields
    private IntPtr _hookID = IntPtr.Zero;
    private LowLevelKeyboardProc _proc;

    // Define the event for the key press
    public event EventHandler<KeyPressedEventArgs> KeyPressed;

    // Constructor
    public GlobalKeyboardHook()
    {
        _proc = HookCallback;
        _hookID = SetHook(_proc);
    }

    // Destructor
    ~GlobalKeyboardHook()
    {
        UnhookWindowsHookEx(_hookID);
    }

    // Set the hook
    private IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
        using (var curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    // Keyboard hook callback function
    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
        {
            int vkCode = Marshal.ReadInt32(lParam);
            Keys key = (Keys)vkCode;

            // Raise the KeyPressed event
            KeyPressed?.Invoke(this, new KeyPressedEventArgs(key));
        }

        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }
}

// Event argument class for the key press event
public class KeyPressedEventArgs : EventArgs
{
    public Keys KeyPressed { get; private set; }

    public KeyPressedEventArgs(Keys keyPressed)
    {
        KeyPressed = keyPressed;
    }
}
