using System;
using System.Collections.Generic;
namespace Wezit
{
	[Serializable]
	public class Category : Base
	{
		public string categoryId;
		public string slug;

		public override string ToString()
		{
			return base.ToString() + string.Format(
				"categoryId: {0}\n" +
				"slug: {1}\n" +
				"pid: {2}\n",
				categoryId,
				slug,
				pid
			);
		}

		public string CategoryName 
        {
			get { string[] tree = slug.Split('/');
				return tree[tree.Length - 1]; }
        }
	}

}
