using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KB.Jarvis.App.Native;

public static class NativeInput
{
    private const uint InputKeyboard = 1;
    private const uint InputMouse = 0;
    private const uint KeyEventUnicode = 0x0004;
    private const uint KeyEventKeyUp = 0x0002;
    private const uint MouseEventLeftDown = 0x0002;
    private const uint MouseEventLeftUp = 0x0004;
    private const uint MouseEventRightDown = 0x0008;
    private const uint MouseEventRightUp = 0x0010;
    private const uint MouseEventWheel = 0x0800;
    private const uint MouseEventAbsolute = 0x8000;
    private const uint MouseEventMove = 0x0001;
    private const uint MouseEventVirtualDesk = 0x4000;

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint Type;
        public InputUnion Union;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public MouseInput Mouse;
        [FieldOffset(0)] public KeyboardInput Keyboard;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int Dx;
        public int Dy;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public nuint ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInput
    {
        public ushort VirtualKey;
        public ushort ScanCode;
        public uint Flags;
        public uint Time;
        public nuint ExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint numberOfInputs, Input[] inputs, int sizeOfInput);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(nint windowHandle);

    [DllImport("user32.dll")]
    private static extern bool ShowWindowAsync(nint windowHandle, int command);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int index);

    public static async Task<bool> ActivateProcessWindowAsync(Process process, TimeSpan timeout, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(process);
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            process.Refresh();
            if (process.HasExited)
            {
                return false;
            }

            var handle = process.MainWindowHandle;
            if (handle != nint.Zero)
            {
                ShowWindowAsync(handle, 9);
                if (SetForegroundWindow(handle))
                {
                    await Task.Delay(250, cancellationToken).ConfigureAwait(false);
                    return true;
                }
            }

            await Task.Delay(150, cancellationToken).ConfigureAwait(false);
        }

        return false;
    }

    public static void TypeUnicode(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var inputs = new List<Input>(text.Length * 2);
        foreach (var character in text)
        {
            inputs.Add(new Input
            {
                Type = InputKeyboard,
                Union = new InputUnion
                {
                    Keyboard = new KeyboardInput { ScanCode = character, Flags = KeyEventUnicode }
                }
            });
            inputs.Add(new Input
            {
                Type = InputKeyboard,
                Union = new InputUnion
                {
                    Keyboard = new KeyboardInput { ScanCode = character, Flags = KeyEventUnicode | KeyEventKeyUp }
                }
            });
        }
        Send(inputs);
    }

    public static void PressKey(ushort virtualKey) => PressShortcut(virtualKey);

    public static void PressShortcut(params ushort[] virtualKeys)
    {
        if (virtualKeys.Length == 0) return;
        var inputs = new List<Input>(virtualKeys.Length * 2);
        foreach (var key in virtualKeys) inputs.Add(KeyInput(key, keyUp: false));
        for (var index = virtualKeys.Length - 1; index >= 0; index--) inputs.Add(KeyInput(virtualKeys[index], keyUp: true));
        Send(inputs);
    }

    public static void ClickNormalized(int x, int y, string button = "left", int clickCount = 1)
    {
        var clampedX = Math.Clamp(x, 0, 1000);
        var clampedY = Math.Clamp(y, 0, 1000);
        var left = GetSystemMetrics(76);
        var top = GetSystemMetrics(77);
        var width = Math.Max(1, GetSystemMetrics(78));
        var height = Math.Max(1, GetSystemMetrics(79));
        var pixelX = left + (int)Math.Round(clampedX / 1000d * Math.Max(0, width - 1));
        var pixelY = top + (int)Math.Round(clampedY / 1000d * Math.Max(0, height - 1));
        MovePointer(pixelX, pixelY);
        var right = string.Equals(button, "right", StringComparison.OrdinalIgnoreCase);
        for (var index = 0; index < Math.Clamp(clickCount, 1, 3); index++)
        {
            Send(new[]
            {
                MouseButtonInput(right ? MouseEventRightDown : MouseEventLeftDown),
                MouseButtonInput(right ? MouseEventRightUp : MouseEventLeftUp)
            });
            if (clickCount > 1) Thread.Sleep(90);
        }
    }

    public static void ClickVirtualScreenPoint(int x, int y)
    {
        MovePointer(x, y);
        Send(new[] { MouseButtonInput(MouseEventLeftDown), MouseButtonInput(MouseEventLeftUp) });
    }

    public static void Scroll(int amount)
    {
        Send(new[]
        {
            new Input
            {
                Type = InputMouse,
                Union = new InputUnion
                {
                    Mouse = new MouseInput
                    {
                        MouseData = unchecked((uint)(amount * 120)),
                        Flags = MouseEventWheel
                    }
                }
            }
        });
    }

    public static (int Left, int Top, int Width, int Height) GetVirtualScreenGeometry() =>
        (GetSystemMetrics(76), GetSystemMetrics(77), Math.Max(1, GetSystemMetrics(78)), Math.Max(1, GetSystemMetrics(79)));

    private static void MovePointer(int x, int y)
    {
        var geometry = GetVirtualScreenGeometry();
        var normalizedX = (int)Math.Clamp((x - geometry.Left) * 65535d / Math.Max(1, geometry.Width - 1), 0, 65535);
        var normalizedY = (int)Math.Clamp((y - geometry.Top) * 65535d / Math.Max(1, geometry.Height - 1), 0, 65535);
        Send(new[]
        {
            new Input
            {
                Type = InputMouse,
                Union = new InputUnion
                {
                    Mouse = new MouseInput
                    {
                        Dx = normalizedX,
                        Dy = normalizedY,
                        Flags = MouseEventMove | MouseEventAbsolute | MouseEventVirtualDesk
                    }
                }
            }
        });
    }

    private static Input MouseButtonInput(uint flags) => new()
    {
        Type = InputMouse,
        Union = new InputUnion { Mouse = new MouseInput { Flags = flags } }
    };

    private static Input KeyInput(ushort key, bool keyUp) => new()
    {
        Type = InputKeyboard,
        Union = new InputUnion
        {
            Keyboard = new KeyboardInput { VirtualKey = key, Flags = keyUp ? KeyEventKeyUp : 0 }
        }
    };

    private static void Send(IReadOnlyCollection<Input> inputs)
    {
        var array = inputs.ToArray();
        var sent = SendInput((uint)array.Length, array, Marshal.SizeOf<Input>());
        if (sent != array.Length)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"SendInput accepted {sent} of {array.Length} events.");
        }
    }
}