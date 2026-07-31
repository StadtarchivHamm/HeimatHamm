using System;

namespace Wezit
{

	[Serializable]
	public class APIResponse<T> where T : new()
	{
		public int status;
		public string message;
		public T data = new T();
	}

}
