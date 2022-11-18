using System.Runtime.CompilerServices;
using Box2DSharp.Dynamics;
using Box2DSharp.Dynamics.Contacts;

namespace Box2DSharp.Common
{
    public static class EnumExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasSetFlag(this BodyFlags flags, BodyFlags flag)
        {
            return (flags & flag) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasSetFlag(this Contact.ContactFlag flags, Contact.ContactFlag flag)
        {
            return (flags & flag) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasSetFlag(this DrawFlag flags, DrawFlag flag)
        {
            return (flags & flag) != 0;
        }
    }
}