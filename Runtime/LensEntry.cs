using System;
using System.Collections.Generic;
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
        private readonly LensSliderData sliderData;
        private readonly ILensSelectData selectData;
        private readonly LensProgressData progressData;
        private readonly Func<object, string> customDisplayValue;
        private readonly Func<object, string> customSearchText;
        private readonly Func<object, string> customReportValue;

        public LensEntry(string key, string value)
            : this(key, value, false, DefaultRedactedValue, null)
        {
        }

        public LensEntry(string key, string value, bool isSensitive)
            : this(key, value, isSensitive, DefaultRedactedValue, null)
        {
        }

        public LensEntry(string key, string value, bool isSensitive, string redactedValue)
            : this(key, value, isSensitive, redactedValue, null)
        {
        }

        public LensEntry(string key, string value, bool isSensitive, string redactedValue, string infoText)
        {
            Key = key ?? string.Empty;
            Kind = LensEntryKind.ReadOnly;
            ActionLabel = string.Empty;
            IsSensitive = isSensitive;
            RedactedValue = string.IsNullOrEmpty(redactedValue) ? DefaultRedactedValue : redactedValue;
            RequiresConfirmation = false;
            ConfirmationMessage = string.Empty;
            InfoText = infoText ?? string.Empty;
            CustomTypeId = string.Empty;
            CustomPayload = null;
            readOnlyValue = value ?? string.Empty;
            boolGetter = null;
            boolSetter = null;
            textGetter = null;
            textSetter = null;
            numberGetter = null;
            numberSetter = null;
            action = null;
            sliderData = null;
            selectData = null;
            progressData = null;
            customDisplayValue = null;
            customSearchText = null;
            customReportValue = null;
        }

        public string Key { get; }

        public LensEntryKind Kind { get; }

        public string ActionLabel { get; }

        public bool IsSensitive { get; }

        public string RedactedValue { get; }

        public bool RequiresConfirmation { get; }

        public string ConfirmationMessage { get; }

        public string InfoText { get; }

        public bool HasInfo => !string.IsNullOrWhiteSpace(InfoText);

        public string CustomTypeId { get; }

        public object CustomPayload { get; }

        public string DisplayValue => IsSensitive ? RedactedValue : Value;

        public string ReportValue => IsSensitive ? RedactedValue : GetReportValue();

        public string SearchText => IsSensitive ? RedactedValue : GetSearchText();

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
                        return FormatNumber(GetNumberValue());
                    case LensEntryKind.Button:
                        return ActionLabel;
                    case LensEntryKind.Slider:
                        return FormatSliderValue(GetSliderValue());
                    case LensEntryKind.SingleSelect:
                    case LensEntryKind.MultiSelect:
                        return selectData != null ? selectData.DisplayValue : string.Empty;
                    case LensEntryKind.Progress:
                        return GetProgressDisplayValue();
                    case LensEntryKind.Custom:
                        return customDisplayValue != null ? customDisplayValue(CustomPayload) ?? string.Empty : string.Empty;
                    default:
                        return readOnlyValue ?? string.Empty;
                }
            }
        }

        public static LensEntry ReadOnly(string key, string value, string infoText = null)
        {
            return new LensEntry(key, value, false, DefaultRedactedValue, infoText);
        }

        public static LensEntry ReadOnly(string key, string value, bool isSensitive, string redactedValue = DefaultRedactedValue, string infoText = null)
        {
            return new LensEntry(key, value, isSensitive, redactedValue, infoText);
        }

        public static LensEntry Toggle(string key, Func<bool> getValue, Action<bool> setValue, bool isSensitive = false, string redactedValue = DefaultRedactedValue, string infoText = null)
        {
            if (getValue == null)
            {
                throw new ArgumentNullException(nameof(getValue));
            }

            if (setValue == null)
            {
                throw new ArgumentNullException(nameof(setValue));
            }

            return new LensEntry(key, LensEntryKind.Toggle, string.Empty, null, getValue, setValue, null, null, null, null, null, null, null, null, null, null, isSensitive, redactedValue, false, null, infoText, null, null, null);
        }

        public static LensEntry Text(string key, Func<string> getValue, Action<string> setValue, bool isSensitive = false, string redactedValue = DefaultRedactedValue, string infoText = null)
        {
            if (getValue == null)
            {
                throw new ArgumentNullException(nameof(getValue));
            }

            if (setValue == null)
            {
                throw new ArgumentNullException(nameof(setValue));
            }

            return new LensEntry(key, LensEntryKind.Text, string.Empty, null, null, null, getValue, setValue, null, null, null, null, null, null, null, null, isSensitive, redactedValue, false, null, infoText, null, null, null);
        }

        public static LensEntry Number(string key, Func<float> getValue, Action<float> setValue, bool isSensitive = false, string redactedValue = DefaultRedactedValue, string infoText = null)
        {
            if (getValue == null)
            {
                throw new ArgumentNullException(nameof(getValue));
            }

            if (setValue == null)
            {
                throw new ArgumentNullException(nameof(setValue));
            }

            return new LensEntry(key, LensEntryKind.Number, string.Empty, null, null, null, null, null, getValue, setValue, null, null, null, null, null, null, isSensitive, redactedValue, false, null, infoText, null, null, null);
        }

        public static LensEntry Button(string label, Action onClick, bool requiresConfirmation = false, string confirmationMessage = null, string infoText = null)
        {
            return Button(label, label, onClick, requiresConfirmation, confirmationMessage, infoText);
        }

        public static LensEntry Button(string key, string label, Action onClick, bool requiresConfirmation = false, string confirmationMessage = null, string infoText = null)
        {
            if (onClick == null)
            {
                throw new ArgumentNullException(nameof(onClick));
            }

            return new LensEntry(key, LensEntryKind.Button, label, null, null, null, null, null, null, null, onClick, null, null, null, null, null, false, DefaultRedactedValue, requiresConfirmation, confirmationMessage, infoText, null, null, null);
        }

        public static LensEntry Slider(string key, Func<float> getValue, Action<float> setValue, float min, float max, float step = 0f, string valueFormat = "0.###", bool isSensitive = false, string redactedValue = DefaultRedactedValue, string infoText = null)
        {
            if (getValue == null)
            {
                throw new ArgumentNullException(nameof(getValue));
            }

            if (setValue == null)
            {
                throw new ArgumentNullException(nameof(setValue));
            }

            return new LensEntry(key, LensEntryKind.Slider, string.Empty, null, null, null, null, null, null, null, null, new LensSliderData(getValue, setValue, min, max, step, valueFormat), null, null, null, null, isSensitive, redactedValue, false, null, infoText, null, null, null);
        }

        public static LensEntry SingleSelect<T>(string key, Func<T> getValue, Action<T> setValue, IReadOnlyList<LensOption<T>> options, bool isSensitive = false, string redactedValue = DefaultRedactedValue, string infoText = null)
        {
            if (getValue == null)
            {
                throw new ArgumentNullException(nameof(getValue));
            }

            if (setValue == null)
            {
                throw new ArgumentNullException(nameof(setValue));
            }

            return new LensEntry(key, LensEntryKind.SingleSelect, string.Empty, null, null, null, null, null, null, null, null, null, new LensSelectData<T>(getValue, setValue, null, null, options), null, null, null, isSensitive, redactedValue, false, null, infoText, null, null, null);
        }

        public static LensEntry MultiSelect<T>(string key, Func<IReadOnlyList<T>> getValues, Action<IReadOnlyList<T>> setValues, IReadOnlyList<LensOption<T>> options, bool isSensitive = false, string redactedValue = DefaultRedactedValue, string infoText = null)
        {
            if (getValues == null)
            {
                throw new ArgumentNullException(nameof(getValues));
            }

            if (setValues == null)
            {
                throw new ArgumentNullException(nameof(setValues));
            }

            return new LensEntry(key, LensEntryKind.MultiSelect, string.Empty, null, null, null, null, null, null, null, null, null, new LensSelectData<T>(null, null, getValues, setValues, options), null, null, null, isSensitive, redactedValue, false, null, infoText, null, null, null);
        }

        public static LensEntry Progress(string key, Func<float> getCurrent, Func<float> getMax, string label = null, string infoText = null)
        {
            if (getCurrent == null)
            {
                throw new ArgumentNullException(nameof(getCurrent));
            }

            if (getMax == null)
            {
                throw new ArgumentNullException(nameof(getMax));
            }

            return new LensEntry(key, LensEntryKind.Progress, string.Empty, null, null, null, null, null, null, null, null, null, null, new LensProgressData(getCurrent, getMax, label), null, null, false, DefaultRedactedValue, false, null, infoText, null, null, null);
        }

        public static LensEntry Custom(string key, string typeId, object payload, Func<object, string> displayValue, Func<object, string> searchText = null, Func<object, string> reportValue = null, bool isSensitive = false, string redactedValue = DefaultRedactedValue, string infoText = null)
        {
            if (string.IsNullOrWhiteSpace(typeId))
            {
                throw new ArgumentException("Custom entry type id is required.", nameof(typeId));
            }

            if (displayValue == null)
            {
                throw new ArgumentNullException(nameof(displayValue));
            }

            return new LensEntry(key, LensEntryKind.Custom, string.Empty, null, null, null, null, null, null, null, null, null, null, null, displayValue, searchText, isSensitive, redactedValue, false, null, infoText, typeId, payload, reportValue);
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

        public float GetSliderValue()
        {
            return sliderData != null ? sliderData.GetValue() : 0f;
        }

        public float GetSliderMin()
        {
            return sliderData != null ? sliderData.Min : 0f;
        }

        public float GetSliderMax()
        {
            return sliderData != null ? sliderData.Max : 1f;
        }

        public void SetSliderValue(float value)
        {
            sliderData?.SetValue(value);
        }

        public float ClampSliderValue(float value)
        {
            return sliderData != null ? sliderData.ClampAndSnap(value) : value;
        }

        public string FormatSliderValue(float value)
        {
            return sliderData != null ? sliderData.Format(value) : FormatNumber(value);
        }

        public int GetOptionCount()
        {
            return selectData != null ? selectData.Count : 0;
        }

        public string GetOptionLabel(int index)
        {
            return selectData.GetLabel(index);
        }

        public bool IsOptionSelected(int index)
        {
            return selectData != null && selectData.IsSelected(index);
        }

        public bool[] GetSelectedOptionDraft()
        {
            return selectData != null ? selectData.GetSelectedDraft() : new bool[0];
        }

        public void SetSingleOption(int index)
        {
            selectData.SetSingle(index);
        }

        public void SetMultiOptions(bool[] selected)
        {
            selectData.SetMulti(selected);
        }

        public float GetProgressCurrent()
        {
            return progressData != null ? progressData.GetCurrent() : 0f;
        }

        public float GetProgressMax()
        {
            return progressData != null ? progressData.GetMax() : 0f;
        }

        public float GetProgressRatio()
        {
            var max = GetProgressMax();
            return max <= 0f ? 0f : Math.Max(0f, Math.Min(1f, GetProgressCurrent() / max));
        }

        public string GetProgressLabel()
        {
            return progressData != null ? progressData.Label : string.Empty;
        }

        private string GetReportValue()
        {
            if (Kind == LensEntryKind.Button)
            {
                return $"[Action] {(string.IsNullOrWhiteSpace(ActionLabel) ? Key : ActionLabel)}";
            }

            if (Kind == LensEntryKind.Custom && customReportValue != null)
            {
                return customReportValue(CustomPayload) ?? string.Empty;
            }

            return Value;
        }

        private string GetSearchText()
        {
            if (Kind == LensEntryKind.Custom && customSearchText != null)
            {
                return customSearchText(CustomPayload) ?? string.Empty;
            }

            return Value;
        }

        private string GetProgressDisplayValue()
        {
            var current = GetProgressCurrent();
            var max = GetProgressMax();
            var percent = GetProgressRatio() * 100f;
            var label = GetProgressLabel();
            var value = $"{FormatNumber(current)} / {FormatNumber(max)} ({percent.ToString("0.#", CultureInfo.InvariantCulture)}%)";
            return string.IsNullOrWhiteSpace(label) ? value : $"{label}: {value}";
        }

        private static string FormatNumber(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
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
            LensSliderData sliderData,
            ILensSelectData selectData,
            LensProgressData progressData,
            Func<object, string> customDisplayValue,
            Func<object, string> customSearchText,
            bool isSensitive,
            string redactedValue,
            bool requiresConfirmation,
            string confirmationMessage,
            string infoText,
            string customTypeId,
            object customPayload,
            Func<object, string> customReportValue)
        {
            Key = key ?? string.Empty;
            Kind = kind;
            ActionLabel = actionLabel ?? string.Empty;
            IsSensitive = isSensitive;
            RedactedValue = string.IsNullOrEmpty(redactedValue) ? DefaultRedactedValue : redactedValue;
            RequiresConfirmation = requiresConfirmation;
            ConfirmationMessage = confirmationMessage ?? string.Empty;
            InfoText = infoText ?? string.Empty;
            CustomTypeId = customTypeId ?? string.Empty;
            CustomPayload = customPayload;
            this.readOnlyValue = readOnlyValue ?? string.Empty;
            this.boolGetter = boolGetter;
            this.boolSetter = boolSetter;
            this.textGetter = textGetter;
            this.textSetter = textSetter;
            this.numberGetter = numberGetter;
            this.numberSetter = numberSetter;
            this.action = action;
            this.sliderData = sliderData;
            this.selectData = selectData;
            this.progressData = progressData;
            this.customDisplayValue = customDisplayValue;
            this.customSearchText = customSearchText;
            this.customReportValue = customReportValue;
        }

        private sealed class LensSliderData
        {
            private readonly Func<float> getValue;
            private readonly Action<float> setValue;
            private readonly float min;
            private readonly float max;
            private readonly float step;
            private readonly string valueFormat;

            public LensSliderData(Func<float> getValue, Action<float> setValue, float min, float max, float step, string valueFormat)
            {
                this.getValue = getValue;
                this.setValue = setValue;
                this.min = Math.Min(min, max);
                this.max = Math.Max(min, max);
                this.step = Math.Max(0f, step);
                this.valueFormat = string.IsNullOrWhiteSpace(valueFormat) ? "0.###" : valueFormat;
            }

            public float Min => min;

            public float Max => max;

            public float GetValue()
            {
                return ClampAndSnap(getValue());
            }

            public void SetValue(float value)
            {
                setValue(ClampAndSnap(value));
            }

            public float ClampAndSnap(float value)
            {
                var clamped = Math.Max(min, Math.Min(max, value));
                if (step <= 0f)
                {
                    return clamped;
                }

                return Math.Max(min, Math.Min(max, (float)(Math.Round((clamped - min) / step) * step + min)));
            }

            public string Format(float value)
            {
                return ClampAndSnap(value).ToString(valueFormat, CultureInfo.InvariantCulture);
            }
        }

        private interface ILensSelectData
        {
            int Count { get; }

            string DisplayValue { get; }

            string SearchText { get; }

            string GetLabel(int index);

            bool IsSelected(int index);

            bool[] GetSelectedDraft();

            void SetSingle(int index);

            void SetMulti(bool[] selected);
        }

        private sealed class LensSelectData<T> : ILensSelectData
        {
            private readonly Func<T> getSingle;
            private readonly Action<T> setSingle;
            private readonly Func<IReadOnlyList<T>> getMulti;
            private readonly Action<IReadOnlyList<T>> setMulti;
            private readonly IReadOnlyList<LensOption<T>> options;

            public LensSelectData(Func<T> getSingle, Action<T> setSingle, Func<IReadOnlyList<T>> getMulti, Action<IReadOnlyList<T>> setMulti, IReadOnlyList<LensOption<T>> options)
            {
                this.getSingle = getSingle;
                this.setSingle = setSingle;
                this.getMulti = getMulti;
                this.setMulti = setMulti;
                this.options = options ?? throw new ArgumentNullException(nameof(options));
            }

            public int Count => options.Count;

            public string DisplayValue
            {
                get
                {
                    if (getSingle != null)
                    {
                        var index = FindSingleIndex();
                        return index >= 0 ? options[index].Label : "(none)";
                    }

                    return FormatMulti(GetSelectedDraft());
                }
            }

            public string SearchText => string.Join(" ", GetLabels());

            public string GetLabel(int index)
            {
                return options[index].Label;
            }

            public bool IsSelected(int index)
            {
                return GetSelectedDraft()[index];
            }

            public bool[] GetSelectedDraft()
            {
                var selected = new bool[options.Count];

                if (getSingle != null)
                {
                    var index = FindSingleIndex();
                    if (index >= 0)
                    {
                        selected[index] = true;
                    }

                    return selected;
                }

                var values = getMulti() ?? Array.Empty<T>();
                for (var i = 0; i < options.Count; i++)
                {
                    for (var valueIndex = 0; valueIndex < values.Count; valueIndex++)
                    {
                        if (EqualityComparer<T>.Default.Equals(options[i].Value, values[valueIndex]))
                        {
                            selected[i] = true;
                            break;
                        }
                    }
                }

                return selected;
            }

            public void SetSingle(int index)
            {
                setSingle(options[index].Value);
            }

            public void SetMulti(bool[] selected)
            {
                var values = new List<T>();
                for (var i = 0; i < options.Count && i < selected.Length; i++)
                {
                    if (selected[i])
                    {
                        values.Add(options[i].Value);
                    }
                }

                setMulti(values);
            }

            private int FindSingleIndex()
            {
                var current = getSingle();
                for (var i = 0; i < options.Count; i++)
                {
                    if (EqualityComparer<T>.Default.Equals(options[i].Value, current))
                    {
                        return i;
                    }
                }

                return -1;
            }

            private IEnumerable<string> GetLabels()
            {
                for (var i = 0; i < options.Count; i++)
                {
                    yield return options[i].Label;
                }
            }

            private string FormatMulti(bool[] selected)
            {
                var selectedCount = 0;
                var lastSelectedIndex = -1;
                for (var i = 0; i < selected.Length; i++)
                {
                    if (!selected[i])
                    {
                        continue;
                    }

                    selectedCount++;
                    lastSelectedIndex = i;
                }

                if (selectedCount == 0)
                {
                    return "None";
                }

                if (selectedCount == options.Count)
                {
                    return "Everything";
                }

                if (selectedCount == 1)
                {
                    return options[lastSelectedIndex].Label;
                }

                return "Mixed...";
            }
        }

        private sealed class LensProgressData
        {
            private readonly Func<float> getCurrent;
            private readonly Func<float> getMax;

            public LensProgressData(Func<float> getCurrent, Func<float> getMax, string label)
            {
                this.getCurrent = getCurrent;
                this.getMax = getMax;
                Label = label ?? string.Empty;
            }

            public string Label { get; }

            public float GetCurrent()
            {
                return getCurrent();
            }

            public float GetMax()
            {
                return getMax();
            }
        }
    }
}
