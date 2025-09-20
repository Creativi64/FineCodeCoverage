using System.Windows.Forms;

namespace FineCodeCoverage.Readme
{
    internal static class MessageExtensions
    {
        public static bool? IsKeyUpOrDown(this Message message)
        {
            bool isKeyDown = message.IsKeyDown();

            if (!isKeyDown)
            {
                bool isKeyUp = message.IsKeyUp();
                if (!isKeyUp)
                {
                    return null;
                }
            }

            return isKeyDown;
        }

        public static bool IsKeyDown(this Message message) => message.Msg == 0x0100;

        public static bool IsKeyUp(this Message m)
            => m.Msg == 0x0101 /* WM_KEYUP */ || m.Msg == 0x0105 /* WM_SYSKEYUP */;

        public static Keys GetKeys(this Message message) => (Keys)(int)message.WParam;

        public static bool HasFlag(this Message message, Keys keys) => message.GetKeys().HasFlag(keys);
    }
}
