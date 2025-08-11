using System;
using System.Windows.Forms;

namespace FineCodeCoverage.Readme
{
    internal static class MessageExtensions
    {
        public static bool IsFindMessage(this Message message, FindKeys findKeys = FindKeys.CtrlF | FindKeys.F3)
        {
            if (!message.IsKeyDown())
            {
                return false;
            }

            Keys key = message.GetKeys();
            return IsFind(key, findKeys);
        }

        private static bool IsFind(Keys key, FindKeys findKeys)
            => ((findKeys & FindKeys.CtrlF) == FindKeys.CtrlF &&
                key == Keys.F &&
                (System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control)
            || ((findKeys & FindKeys.F3) == FindKeys.F3 &&
                key == Keys.F3 &&
                System.Windows.Forms.Control.ModifierKeys == Keys.None);

        public static bool IsKeyDown(this Message message) => message.Msg == 0x0100;

        public static Keys GetKeys(this Message message) => (Keys)(int)message.WParam;
    }

    [Flags]
    internal enum FindKeys
    {
        CtrlF = 1,
        F3 = 2,
    }
}
