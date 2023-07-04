using System.Runtime.InteropServices;

namespace TrunkAD.Core.GameSystem.GameHelper
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct bx_5k_area_header
    {
        public byte AreaType;
        public ushort AreaX;
        public ushort AreaY;
        public ushort AreaWidth;
        public ushort AreaHeight;
        public byte DynamicAreaLoc;
        public byte Lines_sizes;
        public byte RunMode;
        public short Timeout;
        public byte Reserved1;
        public byte Reserved2;
        public byte Reserved3;
        public byte SingleLine;
        public byte NewLine;
        public byte DisplayMode;
        public byte ExitMode;
        public byte Speed;
        public byte StayTime;
        public int DataLen;
    }
}