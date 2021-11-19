using PInvoke;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TeamGleason.SpeakFaster.BasicKeyboard.Layout;

namespace TeamGleason.SpeakFaster.BasicKeyboard.App.Universal
{
    class InteropHelper
    {
        private static User32.SafeHookHandle s_oldHook;

        public static bool IsShift
        {
            get
            {
                var value = User32.GetKeyState((int)User32.VirtualKey.VK_SHIFT);
                return value < 0;
            }
        }

        public static bool IsControl
        {

            get
            {
                var value = User32.GetKeyState((int)User32.VirtualKey.VK_CONTROL);
                return value < 0;
            }
        }

        public static bool IsCapsLock
        {

            get
            {
                var value = User32.GetKeyState((int)User32.VirtualKey.VK_CAPITAL);
                return (value & 1) != 0;
            }
        }


        public static event EventHandler StateChange
        {
            add
            {
                if (_stateChangeListenerCount == 0)
                {
                    StartListening();
                }
                _stateChangeListenerCount++;

                _stateChange += value;
            }

            remove
            {
                // TODO: Something's wrong here, it doesn't appear to remove the event handler.
                _stateChange -= value;

                _stateChangeListenerCount--;
                if (_stateChangeListenerCount == 0)
                {
                    StopListening();
                }
            }
        }


        public static void SendKey(bool sendDown, bool sendUp, KeyName keyName)
        {
            var code = User32.VirtualKey.VK_NO_KEY;

            switch (keyName)
            {
                case KeyName.Backspace: code = User32.VirtualKey.VK_BACK; break;
                case KeyName.Tab: code = User32.VirtualKey.VK_TAB; break;
                case KeyName.Enter: code = User32.VirtualKey.VK_RETURN; break;
                case KeyName.Space: code = User32.VirtualKey.VK_SPACE; break;
                case KeyName.Home: code = User32.VirtualKey.VK_HOME; break;
                case KeyName.End: code = User32.VirtualKey.VK_END; break;
                case KeyName.Delete: code = User32.VirtualKey.VK_DELETE; break;
                case KeyName.PageUp: code = User32.VirtualKey.VK_PRIOR; break;
                case KeyName.PageDown: code = User32.VirtualKey.VK_NEXT; break;
                case KeyName.ArrowUp: code = User32.VirtualKey.VK_UP; break;
                case KeyName.ArrowDown: code = User32.VirtualKey.VK_DOWN; break;
                case KeyName.ArrowLeft: code = User32.VirtualKey.VK_LEFT; break;
                case KeyName.ArrowRight: code = User32.VirtualKey.VK_RIGHT; break;
                case KeyName.F1: code = User32.VirtualKey.VK_F1; break;
                case KeyName.F2: code = User32.VirtualKey.VK_F2; break;
                case KeyName.F3: code = User32.VirtualKey.VK_F3; break;
                case KeyName.F4: code = User32.VirtualKey.VK_F4; break;
                case KeyName.F5: code = User32.VirtualKey.VK_F5; break;
                case KeyName.F6: code = User32.VirtualKey.VK_F6; break;
                case KeyName.F7: code = User32.VirtualKey.VK_F7; break;
                case KeyName.F8: code = User32.VirtualKey.VK_F8; break;
                case KeyName.F9: code = User32.VirtualKey.VK_F9; break;
                case KeyName.F10: code = User32.VirtualKey.VK_F10; break;
                case KeyName.F11: code = User32.VirtualKey.VK_F11; break;
                case KeyName.F12: code = User32.VirtualKey.VK_F12; break;
                case KeyName.Escape: code = User32.VirtualKey.VK_ESCAPE; break;
            }

            if (code != 0 && (sendDown || sendUp))
            {
                var inputs = new User32.INPUT[2];

                inputs[0].type = User32.InputType.INPUT_KEYBOARD;
                inputs[0].Inputs.ki.wVk = code;
                inputs[0].Inputs.ki.dwFlags = sendDown ? 0 : User32.KEYEVENTF.KEYEVENTF_KEYUP;

                inputs[1].type = User32.InputType.INPUT_KEYBOARD;
                inputs[1].Inputs.ki.wVk = code;
                inputs[1].Inputs.ki.dwFlags = User32.KEYEVENTF.KEYEVENTF_KEYUP;

                SendInput((sendUp && sendDown) ? 2 : 1, inputs);
            }
        }

        unsafe static void SendInput(int nInputs, User32.INPUT[] inputs)
        {
            fixed (User32.INPUT* p = &inputs[0])
            {
                var size = Marshal.SizeOf(typeof(User32.INPUT));
                var uSent = User32.SendInput(nInputs, p, size);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern short VkKeyScan(char ch);

        public static void SendText(bool isShift, bool isCtrl, bool isAlt, bool isWindows, String text)
        {
            var length = text.Length;
            for (var i = 0; i < length; i++)
            {
                var ch = text[i];

                var code = VkKeyScan(ch);

                if ((code & 0xFFFF) != 0xFFFF)
                {
                    var isShiftNeeded = isShift || (code & 0x0100) != 0;
                    var isCtrlNeeded = isCtrl || (code & 0x0200) != 0;
                    var isAltNeeded = isAlt || (code & 0x0400) != 0;
                    //var isHankakuNeeded = !!(code & 0x0800);

                    var inputs = new User32.INPUT[8];

                    var count = 0;

                    if (isShiftNeeded)
                    {
                        inputs[count].type = User32.InputType.INPUT_KEYBOARD;
                        inputs[count].Inputs.ki.wVk = User32.VirtualKey.VK_SHIFT;
                        count++;
                    }

                    if (isCtrlNeeded)
                    {
                        inputs[count].type = User32.InputType.INPUT_KEYBOARD;
                        inputs[count].Inputs.ki.wVk = User32.VirtualKey.VK_CONTROL;
                        count++;
                    }

                    if (isAltNeeded)
                    {
                        inputs[count].type = User32.InputType.INPUT_KEYBOARD;
                        inputs[count].Inputs.ki.wVk = User32.VirtualKey.VK_MENU;
                        count++;
                    }

                    inputs[count].type = User32.InputType.INPUT_KEYBOARD;
                    inputs[count].Inputs.ki.wVk = (User32.VirtualKey)((int)code & 0xFF);
                    count++;

                    var countdown = count;
                    while (countdown != 0)
                    {
                        countdown--;

                        inputs[count].type = inputs[countdown].type;
                        inputs[count].Inputs.ki.wVk = inputs[countdown].Inputs.ki.wVk;
                        inputs[count].Inputs.ki.dwFlags = inputs[countdown].Inputs.ki.dwFlags ^ User32.KEYEVENTF.KEYEVENTF_KEYUP;
                        count++;
                    }

                    SendInput(count, inputs);
                }
                else
                {
                    Debug.WriteLine($"Cannot send {ch}");
                }
            }
        }

        static void SendCharacter(char ch)
        {
            var inputs = new User32.INPUT[2];

            inputs[0].type = User32.InputType.INPUT_KEYBOARD;
            inputs[0].Inputs.ki.wVk = (User32.VirtualKey)ch;

            inputs[1].type = User32.InputType.INPUT_KEYBOARD;
            inputs[1].Inputs.ki.wVk = (User32.VirtualKey)ch;
            inputs[1].Inputs.ki.dwFlags = User32.KEYEVENTF.KEYEVENTF_KEYUP;

            SendInput(inputs.Length, inputs);
        }

        public static void SetMainWindowStyle(IntPtr handle)
        {
            User32.SetWindowLong(handle, User32.WindowLongIndexFlags.GWL_EXSTYLE,
                (User32.SetWindowLongFlags)(User32.GetWindowLong(handle, User32.WindowLongIndexFlags.GWL_EXSTYLE) | (int)User32.SetWindowLongFlags.WS_EX_NOACTIVATE));
        }

        static void RaiseStateChange()
        {
            if (_stateChange != null)
            {
                _stateChange.Invoke(null, EventArgs.Empty);
            }
        }

        static void LookForStateChange()
        {
            var changed = IsChanged(ref _isShift, IsShift) ||
                IsChanged(ref _isControl, IsControl) ||
                IsChanged(ref _isCapsLock, IsCapsLock);
            if (changed)
            {
                RaiseStateChange();
            }
        }

        static EventHandler _stateChange = null;
        static int _stateChangeListenerCount = 0;
        static bool _isShift = false;
        static bool _isControl = false;
        static bool _isCapsLock = false;

        static bool IsChanged(ref bool previous, bool now)
        {
            var changed = previous != now;
            if (changed)
            {
                previous = now;
            }
            return changed;
        }

        static void StartListening()
        {
            // s_oldHook = User32.SetWindowsHookEx(User32.WindowsHookType.WH_KEYBOARD_LL, LowLevelKeyboardProc, IntPtr.Zero, 0);
        }

        static void StopListening()
        {
            s_oldHook.Dispose();
            s_oldHook = null;
        }

        //static User32.LRESULT LowLevelKeyboardProc(
        //                       int nCode,
        //   WPARAM wParam,
        //   User32.LPARAM lParam)
        //{
        //    InteropHelper.LookForStateChange();

        //    var value = CallNextHookEx(s_oldHook, nCode, wParam, lParam);

        //    return value;
        //}
    }
}
