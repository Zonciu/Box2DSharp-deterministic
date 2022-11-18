using System.Runtime.CompilerServices;

namespace Testbed.Abstractions
{
    public static class EnumExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasSetFlag(this KeyModifiers flags, KeyModifiers flag)
        {
            return (flags & flag) != 0;
        }
    }
}