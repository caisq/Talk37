using System;
using System.Windows.Interop;

namespace TeamGleason.SpeakFaster.BasicKeyboard.Keyboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        protected override void OnSourceInitialized(EventArgs ee)
        {
            base.OnSourceInitialized(ee);

            //Set the window style to noactivate.
            var helper = new WindowInteropHelper(this);
            InteropHelper.SetMainWindowStyle(helper.Handle);

            TheKeyboard.SendKey += (s, e) => InteropHelper.SendKey(sendDown: e.SendDown, sendUp: e.SendUp, keyName: e.KeyName);
            TheKeyboard.SendText += (s, e) => InteropHelper.SendText(isShift: e.IsShift, isCtrl: e.IsCtrl, isAlt: e.IsAlt, isWindows: e.IsWindows, text: e.Text);
        }
    }
}
