using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Serialization;

namespace RemoteHttpClient.Http
	{
	/// <summary>
	/// Вспомогательные методы для работы с http
	/// </summary>
	public static class HttpHelpers
		{
		/// <summary>
		/// Разделитель для вывода
		/// </summary>
		public static readonly string separator = $"------------------------------------------{System.Environment.NewLine}";

		/// <summary>
		/// Возвращает набор заголовков в виде пар ключ - набор значений
		/// </summary>
		/// <param name="coll">Набор заголовков</param>
		/// <returns></returns>
		public static IEnumerable<Tuple<string, string>> GetHeaders(NameValueCollection coll)
			{
			if (coll == null)
				{
				yield break;
				}

			if (coll.Count == 0)
				{
				yield break;
				}

			var keys = coll.AllKeys;
			for (int j = 0; j < coll.Count; j++)
				{
				var key = keys[j];
				var arr = coll.GetValues(j);
				if (arr == null)
					{
					yield return new Tuple<string, string>(key, string.Empty);
					yield break;
					}

				var sb = new StringBuilder();
				for (int i = 0; i < arr.Length; i++)
					{
					var value = arr[i];
					sb.Append(value);
					if (i != (arr.Length - 1))
						{
						sb.Append(",");
						}
					}
				yield return new Tuple<string, string>(key, sb.ToString());
				}
			}

		/// <summary>
		/// Возвращает набор заголовков в виде пар ключ - набор значений
		/// </summary>
		/// <param name="headers">Набор заголовков</param>
		/// <returns></returns>
		public static IEnumerable<Tuple<string, string>> GetHeaders(HttpHeaders headers)
			{
			if (headers == null)
				{
				yield break;
				}

			foreach (var header in headers)
				{
				var key = header.Key;
				var sb = new StringBuilder();
				var arr = header.Value.ToArray();
				for (int i = 0; i < arr.Length; i++)
					{
					var value = arr[i];
					sb.Append(value);
					if (i != (arr.Length - 1))
						{
						sb.Append(",");
						}
					}
				yield return new Tuple<string, string>(key, sb.ToString());
				}
			}

		/// <summary>
		/// Возвращает набор заголовков в виде строки
		/// </summary>
		/// <param name="headers">Набор заголовков</param>
		/// <returns></returns>
		public static string GetHeadersAsString(IEnumerable<Tuple<string, string>> headers)
			{
			var sb = new StringBuilder();
			var arr = headers.ToArray();

			sb.Append($"Headers ({arr.Length}):");
			sb.Append(System.Environment.NewLine);

			foreach (var kv in arr)
				{
				sb.Append(kv.Item1);
				sb.Append(": ");
				sb.Append(kv.Item2);
				sb.Append(System.Environment.NewLine);
				}
			if (arr.Length == 0)
				{
				sb.Append(System.Environment.NewLine);
				}
			return sb.ToString();
			}

		/// <summary>
		/// Возвращает набор заголовков в виде строки
		/// </summary>
		/// <param name="headers">Набор заголовков</param>
		/// <returns></returns>
		public static string GetHeadersAsString(HttpHeaders headers)
			{
			var arr = GetHeaders(headers);
			return GetHeadersAsString(arr);
			}

		#region Сериализаторы

		/// <summary>
		/// Десериализовать объект в xml
		/// </summary>
		/// <typeparam name="T">Класс объекта</typeparam>
		/// <param name="toDeserialize">строка для десериализации</param>
		/// <returns></returns>
		public static T DeserializeFromXml<T>(string toDeserialize)
			{
			var xmlSerializer = new XmlSerializer(typeof(T));
			using (var textReader = new StringReader(toDeserialize))
				{
				return (T) xmlSerializer.Deserialize(textReader);
				}
			}

		/// <summary>
		/// Сериализовать объект в xml
		/// </summary>
		/// <typeparam name="T">Класс объекта</typeparam>
		/// <param name="t">объект</param>
		/// <returns></returns>
		public static string SerializeToXml<T>(T t)
			{
			string result = null;
			var xmlSerializer = new XmlSerializer(typeof(T));
			using (var textWriter = new Utf8StringWriter())
				{
				xmlSerializer.Serialize(textWriter, t);
				result = textWriter.ToString();
				// var result2 = XmlConvert.DecodeName(result);
				}

#if DEBUG
			Debug.WriteLine(result);
#endif // DEBUG
			return result;
			}

		#endregion Сериализаторы
		}
	}