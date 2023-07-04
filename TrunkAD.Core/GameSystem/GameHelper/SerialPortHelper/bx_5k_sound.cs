using System.Runtime.InteropServices;

namespace TrunkAD.Core.GameSystem.GameHelper
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct bx_5k_sound
    {
        public byte StoreFlag;
        public byte SoundPerson;//一个字节
        public byte SoundVolum;
        public byte SoundSpeed;
        public int SoundDataLen;
    }
}