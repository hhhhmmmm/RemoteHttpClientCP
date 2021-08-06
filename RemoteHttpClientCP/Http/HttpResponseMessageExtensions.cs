using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RemoteHttpClient.Http
	{
	/// <summary>
	/// Класс расширяет набор методов HttpResponseMessage
	/// </summary>
	public static class HttpResponseMessageExtensions
		{
		#region Дамп заголовков

		/// <summary>
		/// Возвращает набор заголовков в виде пар ключ - набор значений
		/// </summary>
		/// <param name="response">Ответ</param>
		/// <returns></returns>
		public static IEnumerable<Tuple<string, string>> GetHeaders(this HttpResponseMessage response)
			{
			var enumpairs = HttpHelpers.GetHeaders(response.Headers);
			foreach (var pair in enumpairs)
				{
				yield return pair;
				}
			}

		/// <summary>
		/// Возвращает набор заголовков в виде строки
		/// </summary>
		/// <param name="response">Ответ</param>
		/// <returns></returns>
		public static string GetHeadersAsString(this HttpResponseMessage response)
			{
			return HttpHelpers.GetHeadersAsString(response.Headers);
			}

		#endregion Дамп заголовков

		/// <summary>
		/// Получить полученное содержимое как строку
		/// </summary>
		/// <param name="response">Ответ</param>
		/// <returns></returns>
		public static async Task<string> GetContentAsStringAsync(this HttpResponseMessage response)
			{
			var sb = new StringBuilder();
			if (response.Content == null)
				{
				sb.Append("Body: no, Length = 0");
				sb.Append(Environment.NewLine);
				return sb.ToString();
				}

			if (response.Content.Headers != null)
				{
				var contentHeaders = GetContentHeadersAsString(response.Content.Headers);
				sb.Append(contentHeaders);
				sb.Append(Environment.NewLine);
				}

			if (response.Content is StreamContent s)
				{
				var str = await s.ReadAsStringAsync().ConfigureAwait(false);
				sb.Append($"Body: yes, Length = {str.Length}");
				sb.Append(Environment.NewLine);
				sb.Append("StreamContent:");
				sb.Append(Environment.NewLine);
				sb.Append(str);
				sb.Append(Environment.NewLine);
				}

			if (response.Content is StringContent c)
				{
				var str = await c.ReadAsStringAsync().ConfigureAwait(false);
				sb.Append($"Body: yes, Length = {str.Length}");
				sb.Append(Environment.NewLine);
				sb.Append("StringContent:");
				sb.Append(Environment.NewLine);
				sb.Append(str);
				sb.Append(Environment.NewLine);
				}
			return sb.ToString();
			}

		/// <summary>
		/// Возвращает набор заголовков в виде пар ключ - набор значений
		/// </summary>
		/// <param name="headers">Заголовки ответа</param>
		/// <returns></returns>
		public static IEnumerable<Tuple<string, string>> GetContentHeaders(this HttpContentHeaders headers)
			{
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
		/// <param name="headers">Заголовки ответа</param>
		/// <returns></returns>
		public static string GetContentHeadersAsString(this HttpContentHeaders headers)
			{
			var sb = new StringBuilder();
			var arr = GetContentHeaders(headers).ToArray();

			sb.Append($"Content headers ({arr.Length}):");
			sb.Append(Environment.NewLine);
			sb.Append(HttpHelpers.separator);

			foreach (var kv in arr)
				{
				sb.Append(kv.Item1);
				sb.Append(": ");
				sb.Append(kv.Item2);
				sb.Append(Environment.NewLine);
				}
			if (arr.Length == 0)
				{
				sb.Append(Environment.NewLine);
				}
			sb.Append(HttpHelpers.separator);

			return sb.ToString();
			}

		/// <summary>
		/// Вывести содержимое ответа в строку
		/// </summary>
		/// <param name="response">Ответ</param>
		/// <returns></returns>
		public static async Task<string> DumpResponseToStringAsync(this HttpResponseMessage response)
			{
			var sb = new StringBuilder();

			sb.Append(Environment.NewLine);
			sb.Append(HttpHelpers.separator);
			sb.Append($"RESPONSE from: {response.RequestMessage.Method.Method} {response.RequestMessage.RequestUri.ToString()}");
			sb.Append(Environment.NewLine);
			sb.Append(HttpHelpers.separator);

			sb.Append($"IsSuccessStatusCode: {response.IsSuccessStatusCode.ToString()}");
			sb.Append(Environment.NewLine);

			var iStatusCode = (int) response.StatusCode;
			sb.Append($"StatusCode: {response.StatusCode} ({iStatusCode})");
			sb.Append(Environment.NewLine);

			sb.Append($"ReasonPhrase: {response.ReasonPhrase}");
			sb.Append(Environment.NewLine);

			sb.Append($"Version: {response.Version.ToString()}");
			sb.Append(Environment.NewLine);

			sb.Append(HttpHelpers.separator);

			var headers = GetHeadersAsString(response);
			sb.Append(headers);
			sb.Append(HttpHelpers.separator);

			var bodyContent = await GetContentAsStringAsync(response).ConfigureAwait(false);
			sb.Append(bodyContent);
			sb.Append(HttpHelpers.separator);

			return sb.ToString();
			}
		}
	}