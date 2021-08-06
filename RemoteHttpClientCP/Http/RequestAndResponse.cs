using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace RemoteHttpClient.Http
	{
	/// <summary>
	/// Класс несущий в себе текстовое представление отправленного запроса и текстового ответа
	/// </summary>
	public sealed class RequestAndResponse
		{
		/// <summary>
		/// Разделитель
		/// </summary>
		private readonly string Separator = Environment.NewLine;

		#region Конструкторы

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="remoteClientDataUid">Уникальный идентификатор запроса</param>
		/// <param name="request">Запрос</param>
		/// <param name="response">Ответ</param>
		public RequestAndResponse(string remoteClientDataUid, HttpRequestMessage request, HttpResponseMessage response)
			{
			RemoteClientDataUid = remoteClientDataUid;

			if (request != null)
				{
				FillRequest(request);
				}

			if (response != null)
				{
				FillResponse(response);
				}
			}

		#endregion Конструкторы

		#region Свойства

		/// <summary>
		/// Уникальный идентификатор запроса
		/// </summary>
		public string RemoteClientDataUid
			{
			get;
			private set;
			}

		/// <summary>
		/// Запрос получен через веб-сервер
		/// </summary>
		public bool GotFromWebService
			{
			get;
			set;
			}

		/// <summary>
		/// Адрес вызова
		/// </summary>
		public string Url
			{
			get;
			private set;
			}

		/// <summary>
		/// Метод вызова
		/// </summary>
		public string Method
			{
			get;
			private set;
			}

		/// <summary>
		/// Заголовки запроса
		/// </summary>
		public string RequestHeaders
			{
			get;
			private set;
			}

		/// <summary>
		/// Свойства запроса
		/// </summary>
		public string RequestProperties
			{
			get;
			private set;
			}

		/// <summary>
		/// Тело запроса
		/// </summary>
		public string RequestBody
			{
			get;
			private set;
			}

		/// <summary>
		/// Заголовки ответа
		/// </summary>
		public string ResponseHeaders
			{
			get;
			private set;
			}

		/// <summary>
		/// Тело ответа
		/// </summary>
		public string ResponseBody
			{
			get;
			private set;
			}

		/// <summary>
		/// Код ответа
		/// </summary>
		public HttpStatusCode StatusCode
			{
			get;
			private set;
			}

		#endregion Свойства

		/// <summary>
		/// Установить уникальный идентификатор запроса
		/// </summary>
		/// <param name="remoteClientDataUid">Уникальный идентификатор запроса</param>
		public void SetRemoteClientDataUid(string remoteClientDataUid)
			{
			RemoteClientDataUid = remoteClientDataUid;
			}

		/// <summary>
		/// Заполнить свойства запроса
		/// </summary>
		/// <param name="request">Запрос</param>
		private async void FillRequest(HttpRequestMessage request)
			{
			Url = request.RequestUri.ToString();
			Method = request.Method.Method;

			#region Заголовки

			if (request.Headers != null)
				{
				var headers = request.GetHeaders().ToArray();
				var sb = new StringBuilder();
				foreach (var kv in headers)
					{
					sb.Append(kv.Item1);
					sb.Append(": ");
					sb.Append(kv.Item2);
					sb.Append(Separator);
					}

				if (sb.Length > 0)
					{
					RequestHeaders = sb.ToString();
					}
				}

			#endregion Заголовки

			#region Свойства

			if (request.Properties != null)
				{
				var props = request.GetProperties();
				var sb = new StringBuilder();
				foreach (var kv in props)
					{
					sb.Append(kv.Item1);
					sb.Append(": ");
					sb.Append(kv.Item2);
					sb.Append(Separator);
					}
				if (sb.Length > 0)
					{
					RequestProperties = sb.ToString();
					}
				}

			#endregion Свойства

			#region Тело

			if (request.Content != null)
				{
				if (request.Content is StringContent c)
					{
					var body = await c.ReadAsStringAsync().ConfigureAwait(false);
					if (!string.IsNullOrEmpty(body))
						{
						RequestBody = body;
						}
					}
				}

			#endregion Тело
			}

		/// <summary>
		/// Заполнить свойства ответа
		/// </summary>
		/// <param name="response">Ответ</param>
		private async void FillResponse(HttpResponseMessage response)
			{
			#region Заголовки

			if (response.Headers != null)
				{
				var headers = response.GetHeaders().ToArray();
				var sb = new StringBuilder();
				foreach (var kv in headers)
					{
					sb.Append(kv.Item1);
					sb.Append(": ");
					sb.Append(kv.Item2);
					sb.Append(Separator);
					}

				if (sb.Length > 0)
					{
					ResponseHeaders = sb.ToString();
					}
				}

			#endregion Заголовки

			#region Тело

			if (response.Content != null)
				{
				if (response.Content is StreamContent s)
					{
					var body = await s.ReadAsStringAsync().ConfigureAwait(false);
					if (!string.IsNullOrEmpty(body))
						{
						ResponseBody = body;
						}
					}

				if (response.Content is StringContent c)
					{
					var body = await c.ReadAsStringAsync().ConfigureAwait(false);
					if (!string.IsNullOrEmpty(body))
						{
						ResponseBody = body;
						}
					}
				}

			#endregion Тело

			StatusCode = response.StatusCode;
			}
		}
	}