using System;
using System.Globalization;
using UnityEngine;
using System.Text.RegularExpressions;

public class StringUtils
{
	public const string htmlTagEm = "em";
	public const string htmlTagI = "i";

	public static string ReplaceTag(string input, string htmlTagToReplace, string htmlTagNew)
	{
		string result = input;

		result = result.Replace(string.Format("<{0}>", htmlTagToReplace), string.Format("<{0}>", htmlTagNew));
		result = result.Replace(string.Format("</{0}>", htmlTagToReplace), string.Format("</{0}>", htmlTagNew));

		return result;
	}

	public static string RemoveWezitLinefeed(string input)
	{
		string result = input;
		result = result.Replace("<br />", "");
		result = result.Replace("<BR />", "");
		return result;
	}

	public static string ReplaceWezitLinefeed(string input)
	{
		string result = input;
		result = result.Replace("<br />", "\n");
		result = result.Replace("<BR />", "\n");
		return result;
	}

	public static string RemoveUnwantedTags(string input)
{
	string result = input;
	result = result.Replace("<p>", "");
	result = result.Replace("</p>", "");
    result = Regex.Replace(result, @"<span[^>]*>", "", RegexOptions.IgnoreCase);
    result = result.Replace("</span>", "");
    result = result.Replace("&amp;", "&");
    return result;
}

	public static string RemoveUnderlineTag(string input)
	{
		string result = input;
		result = result.Replace("<u>", "");
		result = result.Replace("</u>", "");

		return result;
	}

	public static string ReplaceBoldTags(string input)
	{
		return input.Replace("strong>", "b>");
	}

	public static string CleanFromWezit(string input, bool replaceLineBreak = false)
	{
		if (string.IsNullOrEmpty(input)) return input;
		string result = input;

		result = ReplaceTag(result, htmlTagEm, htmlTagI);
		result = replaceLineBreak ? ReplaceWezitLinefeed(result) : RemoveWezitLinefeed(result);
		result = ReplaceBoldTags(result);
		result = RemoveUnwantedTags(result);

		return result;
	}

	public static string AddCustomTagsFromWezit(string input)
	{
		string result = string.IsNullOrEmpty(input) ? "" : input.Replace('[', '<').Replace(']', '>');

		return result;
	}

	//Convert a string into a Vector3. String has to be in the (x, y, z) format
	public static Vector3 StringToVector3(string stringVector)
	{
		// Remove the parentheses
		if (stringVector.StartsWith("(") && stringVector.EndsWith(")"))
		{
			stringVector = stringVector.Substring(1, stringVector.Length - 2);
		}

		// split the items
		string[] stringArray = stringVector.Split(',');

		// store as a Vector3
		Vector3 realVector3 = new Vector3(
			float.Parse(stringArray[0], CultureInfo.InvariantCulture),
			float.Parse(stringArray[1], CultureInfo.InvariantCulture),
			float.Parse(stringArray[2], CultureInfo.InvariantCulture));

		return realVector3;
	}

	public static string ScrambleString(string input, bool keepSpaces = true)
	{
		string tempString = input;
		string scrumbledString = "";
		while (tempString != "")
		{
			int charIndex = UnityEngine.Random.Range(0, tempString.Length);
			if (tempString[charIndex] == ' ')
			{
				if (keepSpaces)
				{
					scrumbledString += tempString[charIndex].ToString();
				}
			}
			else
			{
				scrumbledString += tempString[charIndex].ToString();
			}
			tempString = tempString.Substring(0, charIndex) + tempString.Substring(charIndex + 1, tempString.Length - (charIndex + 1));
		}
		return scrumbledString;
	}

	public static float GetStringAsFloat(string input, float defaultValue = 0)
	{
		if (string.IsNullOrEmpty(input))
		{
			Debug.LogWarning("String is empty");
			return defaultValue;
		}

		if (float.TryParse(input,
						   NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowTrailingSign,
						   new CultureInfo("en-US"),
						   out float value))
		{
			return value;
		}
		else
		{
			Debug.LogWarning("String not parseable as float, returning default value");
			return defaultValue;
		}
	}

	public static Color GetStringAsColor(string colorStr)
	{
		if (string.IsNullOrEmpty(colorStr))
		{
		Debug.LogWarning("String is empty");
			return Color.black;
		}

		if (colorStr.Contains("#"))
		{
			if (ColorUtility.TryParseHtmlString(colorStr, out Color color))
			{
				return color;
			}
			return Color.black;
		}
		else if (colorStr.Contains(","))
		{
			colorStr = colorStr.Substring(4, colorStr.Length - 5);
			string[] colorRgb = colorStr.Split(new char[] { ',' });

			Color color = new Color(float.Parse(colorRgb[0]) / 255, float.Parse(colorRgb[1]) / 255, float.Parse(colorRgb[2]) / 255);
			return color;
		}

		Debug.LogWarning("String could not be parsed as color");
		return Color.black;
	}
}
