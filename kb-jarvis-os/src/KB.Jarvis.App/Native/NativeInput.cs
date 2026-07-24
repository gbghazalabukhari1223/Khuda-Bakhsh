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
                ShowWindowAsync(handle, 9); // SW_RESTORE
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
                    Keyboard = new KeyboardInput
                    {
                        ScanCode = character,
                        Flags = KeyEventUnicode
                    }
                }
            });
            inputs.Add(new Input
            {
                Type = InputKeyboard,
                Union = new InputUnion
                {
                    Keyboard = new KeyboardInput
                    {
                        ScanCode = character,
                        Flags = KeyEventUnicode | KeyEventKeyUp
                    }
                }
            });
        }

        Send(inputs);
    }

    public static void PressShortcut(params ushort[] virtualKeys)
    {
        if (virtualKeys.Length == 0)
        {
            return;
        }

        var inputs = new List<Input>(virtualKeys.Length * 2);
        foreach (var key in virtualKeys)
        {
            inputs.Add(KeyInput(key, keyUp: false));
        }
        for (var index = virtualKeys.Length - 1; index >= 0; index--)
        {
            inputs.Add(KeyInput(virtualKeys[index], keyUp: true));
        }

        Send(inputs);
    }

    public static void ClickVirtualScreenPoint(int x, int y)
    {
        var left = GetSystemMetrics(76);  // SM_XVIRTUALSCREEN
        var top = GetSystemMetrics(77);   // SM_YVIRTUALSCREEN
        var width = Math.Max(1, GetSystemMetrics(78));
        var height = Math.Max(1, GetSystemMetrics(79));
        var normalizedX = (int)Math.Clamp((x - left) * 65535d / Math.Max(1, width - 1), 0, 65535);
        var normalizedY = (int)Math.Clamp((y - top) * 65535d / Math.Max(1, height - 1), 0, 65535);

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
            },
            new Input { Type = InputMouse, Union = new InputUnion { Mouse = new MouseInput { Flags = MouseEventLeftDown } } },
            new Input { Type = InputMouse, Union = new InputUnion { Mouse = new MouseInput { Flags = MouseEventLeftUp } } }
        });
    }

    private static Input KeyInput(ushort key, bool keyUp) => new()
    {
        Type = InputKeyboard,
        Union = new InputUnion
        {
            Keyboard = new KeyboardInput
            {
                VirtualKey = key,
                Flags = keyUp ? KeyEventKeyUp : 0
            }
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
