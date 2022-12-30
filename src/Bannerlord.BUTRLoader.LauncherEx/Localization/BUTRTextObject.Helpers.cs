using System.Collections.Generic;

namespace Bannerlord.BUTRLoader.Localization
{
    internal partial class BUTRTextObject
    {
        private static BUTRTextObject? TryGetOrCreateFromObject(object o) => o switch
        {
            BUTRTextObject textObject => textObject,
            string s => new BUTRTextObject(s),
            int i => new BUTRTextObject(i),
            float f => new BUTRTextObject(f),
            _ => null
        };

        public static bool IsNullOrEmpty(BUTRTextObject? to) => to == Empty || to == null || (string.IsNullOrEmpty(to.Value) && to.Attributes == null);
    }
}