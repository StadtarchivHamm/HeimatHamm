using System;

public static class WezitSourceTransformation
{
	public static string original { get { return "original"; } }
	public static string default_base { get { return "default"; } }
	public static string default2kpx { get { return "default2kpx"; } }
	public static string thumbnail { get { return "thumbnail"; } }
	public static string tiles { get { return "tiles"; } }
	public static string tilesZip { get { return "tiles-zip"; } }
	public static string all { get { return "all"; } }
}

public enum WezitSourceTransformationEnum
{
	original,
	default_base,
	default2kpx,
	thumbnail,
	tile,
	tilesZip
}
