using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Localization
{
    internal partial class BUTRTextObject
    {
        public static readonly BUTRTextObject Empty = new();

        internal string? Value;

        public Dictionary<string, object>? Attributes { get; private set; }

        public int Length => Value?.Length ?? -1;

        public BUTRTextObject(string? value = "", Dictionary<string, object>? attributes = null)
        {
            Value = value;
            Attributes = attributes;
        }

        public BUTRTextObject(int value, Dictionary<string, object>? attributes = null)
        {
            Value = value.ToString();
            Attributes = attributes;
        }

        public BUTRTextObject(float value, Dictionary<string, object>? attributes = null)
        {
            Value = value.ToString("R");
            Attributes = attributes;
        }

        public override string ToString()
        {
            string text;
            try
            {
                text = BUTRLocalizationManager.ProcessTextToString(this, true);
            }
            catch (Exception ex)
            {
                text = $"Error at id: {GetID()}. Lang: {BUTRLocalizationManager.ActiveLanguage}";
            }

            return text;
        }

        public string ToStringWithoutClear()
        {
            string text;
            try
            {
                text = BUTRLocalizationManager.ProcessTextToString(this, false);
            }
            catch (Exception ex)
            {
                text = $"Error at id: {GetID()}. Lang: {BUTRLocalizationManager.ActiveLanguage}";
            }

            return text;
        }

        public bool Contains(BUTRTextObject to)
        {
            return Value.Contains(to.Value);
        }

        public bool Contains(string text)
        {
            return Value.Contains(text);
        }

        public bool Equals(BUTRTextObject to)
        {
            return Value == to.Value && ((Attributes == null && to.Attributes == null) || (Attributes != null && to.Attributes != null && Attributes.SequenceEqual(to.Attributes)));
        }

        public bool HasSameValue(BUTRTextObject to) => Value == to.Value;

        private BUTRTextObject SetTextVariableFromObject(string tag, object variable)
        {
            Attributes ??= new Dictionary<string, object>();
            Attributes[tag] = variable;
            return this;
        }

        public BUTRTextObject SetTextVariable(string tag, BUTRTextObject variable)
        {
            return SetTextVariableFromObject(tag, variable);
        }

        public BUTRTextObject SetTextVariable(string tag, string variable)
        {
            SetTextVariableFromObject(tag, variable);
            return this;
        }

        public BUTRTextObject SetTextVariable(string tag, float variable)
        {
            SetTextVariableFromObject(tag, variable);
            return this;
        }

        public BUTRTextObject SetTextVariable(string tag, int variable)
        {
            SetTextVariableFromObject(tag, variable);
            return this;
        }

        public void AddIDToValue(string id)
        {
            if (!Value.Contains(id) && !Value.StartsWith("{="))
            {
                var value = Value;
                Value = $"{{={id}}}{value}";
            }
        }

        public int GetValueHashCode()
        {
            return Value.GetHashCode();
        }

        public BUTRTextObject CopyTextObject()
        {
            var dictionary = Attributes;
            if (Attributes != null && Attributes.Any<KeyValuePair<string, object>>())
            {
                var dictionary2 = new Dictionary<string, object>();
                foreach (var keyValuePair in Attributes)
                {
                    dictionary2.Add(keyValuePair.Key, keyValuePair.Value);
                }

                dictionary = dictionary2;
            }

            return new BUTRTextObject(Value, dictionary);
        }

        public string GetID()
        {
            var mbstringBuilder = default(MBStringBuilder);
            mbstringBuilder.Initialize();
            if (Value is { Length: > 2 } && Value[0] == '{' && Value[1] == '=')
            {
                var num = 2;
                while (num < Value.Length && Value[num] != '}')
                {
                    mbstringBuilder.Append(Value[num]);
                    num++;
                }
            }

            return mbstringBuilder.ToStringAndRelease();
        }
    }
}