using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteHttpClient.Http
	{
	/// <summary>
	/// Метод чтения ответа.
	/// Влияет на то, как будет читаться ответ
	/// </summary>
	public enum WantedResponseType
		{
		/// <summary>
		/// Прочитать ответ сервера как строку
		/// </summary>
		String,

		/// <summary>
		/// Прочитать ответ сервера как массив байт
		/// </summary>
		ByteArray,

		/// <summary>
		/// Прочитать ответ сервера как поток
		/// </summary>
		Stream
		}
	}
