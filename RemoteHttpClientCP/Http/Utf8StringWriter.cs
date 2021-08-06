using System.IO;
using System.Text;

namespace RemoteHttpClient.Http
	{
	/// <summary>
	/// Перегруженный писатель
	/// </summary>
	public class Utf8StringWriter : StringWriter
		{
		#region Свойства

		/// <summary>
		/// Возвращает необходимую кодировку
		/// </summary>
		public override Encoding Encoding
			{
			get
				{
				return Encoding.UTF8;
				}
			}

		#endregion Свойства
		}
	}