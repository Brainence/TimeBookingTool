using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBT.App.Helpers
{
    public class LanguageItem:EqualityComparer<LanguageItem>
    {
        public string Culture { get; set; }
        public string Flag { get; set; }

        public override bool Equals(LanguageItem x, LanguageItem y)
        {
            return x.Culture == y.Culture;
        }

        public override int GetHashCode(LanguageItem obj)
        {
            return base.GetHashCode();
        }
    }
}
