using System;

public enum Language
{
	fr_FR,
	en_EN,
	de,
	it,
	es,
	ar,
	gr,
	no,
	ru,
	pt,
	nl,
	oc_provenc,
	zh_CHS,
	zh_CHT,
	jp,
	kr,
	hr,
	hu,
	pl,
	sk,
	sme,
	so,
	wz,
	none
}

public static class WezitLanguage
{
	public static string french { get { return "fr_FR"; } }
	public static string english { get { return "en_EN"; } }
	public static string german { get { return "de"; } }
	public static string italian { get { return "it"; } }
	public static string spanish { get { return "es"; } }
	public static string arabic { get { return "ar"; } }
	public static string greek { get { return "gr"; } }
	public static string norwegian { get { return "no"; } }
	public static string russian { get { return "ru"; } }
	public static string portuguese { get { return "pt"; } }
	public static string dutch { get { return "nl"; } }
	public static string provencal { get { return "oc_provec"; } }
	public static string chinese_simplified { get { return "zh_CHS"; } }
	public static string chinese_traditional { get { return "zh_CHT"; } }
	public static string japanese { get { return "jp"; } }
	public static string korean { get { return "kr"; } }
	public static string croatian { get { return "hr"; } }
	public static string hungarian { get { return "hu"; } }
	public static string polish { get { return "pl"; } }
	public static string slovak { get { return "sk"; } }
	public static string sami { get { return "sme"; } }
	public static string somalian { get { return "so"; } }
	public static string wezit { get { return "wz"; } }
}


public static class LanguageExtensions
{
	public static Language From(this string language)
	{
		switch (language)
		{
			case "fr_FR": return Language.fr_FR;
			case "en_EN": return Language.en_EN;
			case "de": return Language.de;
			case "no": return Language.no;
			case "nl": return Language.nl;
			default: throw new ArgumentOutOfRangeException("language");
		}
	}

	public static string To(Language language)
	{
		switch (language)
		{
			case Language.fr_FR: return "FR";
			case Language.en_EN: return "EN";
			case Language.de: return "DE";
			default: throw new ArgumentOutOfRangeException("language");
		}
	}
}