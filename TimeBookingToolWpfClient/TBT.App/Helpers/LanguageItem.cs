using System.Collections.Generic;

namespace TBT.App.Helpers
{
    public class LanguageItem : EqualityComparer<LanguageItem>
    {
        public string Culture { get; set; }
        public string Flag { get; set; }
        public string LanguageName { get; set; }

        public override bool Equals(LanguageItem x, LanguageItem y)
        {
            if (x == null || y == null) return false;
            return x.Culture == y.Culture;
        }

        public override int GetHashCode(LanguageItem obj)
        {
            return base.GetHashCode();
        }
    }
}
