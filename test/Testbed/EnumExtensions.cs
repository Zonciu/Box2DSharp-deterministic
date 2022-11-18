using System.Runtime.CompilerServices;
using TKKeyModifiers = OpenTK.Windowing.GraphicsLibraryFramework.KeyModifiers;

namespace Testbed;

public static class EnumExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasSetFlag(this TKKeyModifiers flags, TKKeyModifiers flag)
    {
        return (flags & flag) != 0;
    }
}