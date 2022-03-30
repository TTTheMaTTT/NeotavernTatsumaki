using System.Collections.Generic;

namespace LanguageTools
{
    // Используемые языки.
    public enum TLanguage
    {
        English,
        Russian
    }

    public static class LanguageValues
    {
        public static readonly Dictionary<TLanguage, string> LanguageNamesMap = new Dictionary<TLanguage, string>
        {
            { TLanguage.English, "English" },
            { TLanguage.Russian, "Russian" }
        };
    }

}
