namespace Testbed.Abstractions
{
    public readonly struct KeyInputEventArgs
    {
        public readonly KeyCodes Key;

        public readonly KeyModifiers Modifiers;

        public readonly bool IsRepeat;

        public bool Alt => Modifiers.HasSetFlag(KeyModifiers.Alt);

        public bool Control => Modifiers.HasSetFlag(KeyModifiers.Ctrl);

        public bool Shift => Modifiers.HasSetFlag(KeyModifiers.Shift);

        public bool Command => Modifiers.HasSetFlag(KeyModifiers.Super);

        public KeyInputEventArgs(KeyCodes key, KeyModifiers modifiers, bool isRepeat)
        {
            Key = key;
            Modifiers = modifiers;
            IsRepeat = isRepeat;
        }
    }
}