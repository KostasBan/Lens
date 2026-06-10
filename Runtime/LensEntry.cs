using System;
using System.Globalization;

namespace KostasBan.Lens
{
    public readonly struct LensEntry
    {
        public const string DefaultRedactedValue = "[redacted]";

        private readonly string readOnlyValue;
        private readonly Func<bool> boolGetter;
        private readonly Action<bool> boolSetter;
        private readonly Func<string> textGetter;
        private readonly Action<string> textSetter;
        private readonly Func<float> numberGetter;
        private readonly Action<float> numberSetter;
        private readonly Action action;

        public LensEntry(string key, string value)
            : this(key, value, false, DefaultRedactedValue)
        {
        }

        public LensEntry(string key, string value, bool isSensitive)
            : this(key, value, isSensitive, DefaultRedactedValue)
        {
        }

        public LensEntry(string key, string value, bool isSensitive, string redactedValue)
        {
            Key = key ?? string.Empty;
            Kind = LensEntryKind.ReadOnly;
            ActionLabel = string.Empty;
            IsSensitive = isSensitive;
            RedactedValue = string.IsNullOrEmpty(redactedValue) ? DefaultRedactedValue : redactedValue;
            RequiresConfirmation = false;
            ConfirmationMessage = string.Empty;
            readOnlyValue = value ?? string.Empty;
            boolGetter = null;
            boolSetter = null;
            textGetter = null;
            textSetter = null;
            numberGetter = null;
            numberSetter = null;
            action = null;
        }

        public string Key { get; }

        public LensEntryKind Kind { get; }

        public string ActionLabel { get; }

        public bool IsSensitive { get; }

        public string RedactedValue { get; }

        public bool RequiresConfirmation { get; }

        public string ConfirmationMessage { get; }

        public string DisplayValue => IsSensitive ? RedactedValue : Value;

        public string Value
        {
            get
            {
                switch (Kind)
                {
                    case LensEntryKind.Toggle:
                        return GetBoolValue().ToString();
                    case LensEntryKind.Text:
                        return GetTextValue();
                    case LensEntryKind.Number:
                        return GetNumberValue().ToString("0.###", CultureInfo.InvariantCulture);
                    case LensEntryKind.Button:
                        return ActionLabel;
                    default:
                        return readOnlyValue ?? string.Empty;
                }
            }
        }

        public static LensEntry ReadOnly(string key, string value)
        {
            return new LensEntry(key, value);
        }

        public static LensEntry ReadOnly(string key, string value, bool isSensitive)
        {
            return new LensEntry(key, value, isSensitive);
        }

        public static LensEntry ReadOnly(string key, string value, bool isSensitive, string redactedValue)
        {
            return new LensEntry(key, value, isSensitive, redactedValue);
        }

        public static LensEntry Toggle(string key, Func<bool> getValue, Action<bool> setValue, bool isSensitive = false, string redactedValue = DefaultRedactedValue)
        {
            if (getValue == null)
            {
                throw new ArgumentNullException(nameof(getValue));
            }

            if (setValue == null)
            {
                throw new ArgumentNullException(nameof(setValue));
            }

            return new LensEntry(key, LensEntryKind.Toggle, string.Empty, null, getValue, setValue, null, null, null, null, null, isSensitive, redactedValue, false, null);
        }

        public static LensEntry Text(string key, Func<string> getValue, Action<string> setValue, bool isSensitive = false, string redactedValue = DefaultRedactedValue)
        {
            if (getValue == null)
            {
                throw new ArgumentNullException(nameof(getValue));
            }

            if (setValue == null)
            {
                throw new ArgumentNullException(nameof(setValue));
            }

            return new LensEntry(key, LensEntryKind.Text, string.Empty, null, null, null, getValue, setValue, null, null, null, isSensitive, redactedValue, false, null);
        }

        public static LensEntry Number(string key, Func<float> getValue, Action<float> setValue, bool isSensitive = false, string redactedValue = DefaultRedactedValue)
        {
            if (getValue == null)
            {
                throw new ArgumentNullException(nameof(getValue));
            }

            if (setValue == null)
            {
                throw new ArgumentNullException(nameof(setValue));
            }

            return new LensEntry(key, LensEntryKind.Number, string.Empty, null, null, null, null, null, getValue, setValue, null, isSensitive, redactedValue, false, null);
        }

        public static LensEntry Button(string label, Action onClick, bool requiresConfirmation = false, string confirmationMessage = null)
        {
            return Button(label, label, onClick, requiresConfirmation, confirmationMessage);
        }

        public static LensEntry Button(string key, string label, Action onClick, bool requiresConfirmation = false, string confirmationMessage = null)
        {
            if (onClick == null)
            {
                throw new ArgumentNullException(nameof(onClick));
            }

            return new LensEntry(key, LensEntryKind.Button, label, null, null, null, null, null, null, null, onClick, false, DefaultRedactedValue, requiresConfirmation, confirmationMessage);
        }

        public bool GetBoolValue()
        {
            return boolGetter != null && boolGetter();
        }

        public void SetBoolValue(bool value)
        {
            boolSetter?.Invoke(value);
        }

        public string GetTextValue()
        {
            return textGetter != null ? textGetter() ?? string.Empty : string.Empty;
        }

        public void SetTextValue(string value)
        {
            textSetter?.Invoke(value ?? string.Empty);
        }

        public float GetNumberValue()
        {
            return numberGetter != null ? numberGetter() : 0f;
        }

        public void SetNumberValue(float value)
        {
            numberSetter?.Invoke(value);
        }

        public void ExecuteAction()
        {
            action?.Invoke();
        }

        private LensEntry(
            string key,
            LensEntryKind kind,
            string actionLabel,
            string readOnlyValue,
            Func<bool> boolGetter,
            Action<bool> boolSetter,
            Func<string> textGetter,
            Action<string> textSetter,
            Func<float> numberGetter,
            Action<float> numberSetter,
            Action action,
            bool isSensitive,
            string redactedValue,
            bool requiresConfirmation,
            string confirmationMessage)
        {
            Key = key ?? string.Empty;
            Kind = kind;
            ActionLabel = actionLabel ?? string.Empty;
            IsSensitive = isSensitive;
            RedactedValue = string.IsNullOrEmpty(redactedValue) ? DefaultRedactedValue : redactedValue;
            RequiresConfirmation = requiresConfirmation;
            ConfirmationMessage = confirmationMessage ?? string.Empty;
            this.readOnlyValue = readOnlyValue ?? string.Empty;
            this.boolGetter = boolGetter;
            this.boolSetter = boolSetter;
            this.textGetter = textGetter;
            this.textSetter = textSetter;
            this.numberGetter = numberGetter;
            this.numberSetter = numberSetter;
            this.action = action;
        }
    }
}
