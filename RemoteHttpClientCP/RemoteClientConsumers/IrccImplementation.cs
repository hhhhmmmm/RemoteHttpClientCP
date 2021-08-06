using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using RemoteHttpClient.Helpers;
using RemoteHttpClient.Http;

using static RemoteHttpClient.Helpers.TaskHelpers;

namespace RemoteHttpClient.RemoteClientConsumers
	{
	/// <summary>
	/// Реализация интерфейса IRemoteClientConsumer
	/// </summary>
	public class IrccImplementation : IDisposableImplementation<IrccImplementation>, IRemoteClientConsumer, IRemoteClientDataUid
		{
		#region Свойства

		/// <summary>
		/// Собранный контент
		/// </summary>
		protected HttpContent httpContent
			{
			get;
			private set;
			}

		#endregion Свойства

		#region Конструкторы

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="url">Адрес назначения как его задал клиент</param>
		public IrccImplementation(string url)
			{
			if (string.IsNullOrEmpty(url))
				{
				throw new ArgumentNullException(nameof(url));
				}

			this.Url = url;
			}

		#endregion Конструкторы

		/// <summary>
		/// Установить отправляемое содержимое
		/// Кодировка - UTF8
		/// MediaType - application/json
		/// </summary>
		/// <param name="content">Отправляемое содержимое</param>
		public void SetStringContent_UTF8_JSON(string content)
			{
			SetStringContent(content, Encoding.UTF8, HttpConstants.MediaTypeApplicationJson);
			}

		/// <summary>
		/// Установить отправляемое содержимое
		/// </summary>
		/// <param name="stringContent">Отправляемое содержимое</param>
		/// <param name="encoding">Кодировка отправляемого сообщения</param>
		/// <param name="mediaType">Название MIMЕ типа</param>
		public void SetStringContent(string stringContent, Encoding encoding, string mediaType = HttpConstants.DefaultMediaType)
			{
			var _mediaType = (mediaType == null) ? HttpConstants.DefaultMediaType : mediaType;

			string _stringContent;
			if (!string.IsNullOrEmpty(stringContent))
				{
				_stringContent = stringContent;
				}
			else
				{
				_stringContent = string.Empty;
				}

			Encoding _encoding;

			if (encoding != null)
				{
				_encoding = encoding;
				}
			else
				{
				_encoding = Encoding.UTF8;
				}

			httpContent = new StringContent(_stringContent, _encoding, _mediaType);
			}

		#region Реализация интерфейса IDisposable

		// <summary>
		// Освободить управляемые ресурсы
		// </summary>
		protected override void DisposeManagedResources()
			{
			if (httpContent != null)
				{
				httpContent.Dispose();
				httpContent = null;
				}
			base.DisposeManagedResources();
			}

		#endregion Реализация интерфейса IDisposable

		#region Реализация интерфейса IRemoteClientConsumer

		/// <summary>
		/// Вывести дамп отправляемого запроса
		/// </summary>
		public bool DumpRequest
			{
			get
				{
				return RemoteHttpClientGlobals.DumpRequest;
				}
			}

		/// <summary>
		/// Вывести дамп полученного ответа
		/// </summary>
		public bool DumpResponse
			{
			get
				{
				return RemoteHttpClientGlobals.DumpResponse;
				}
			}

		/// <summary>
		/// Выводит текстовый дамп отправляемого запроса или полученного ответа
		/// </summary>
		/// <param name="isRequest">Если true то это отправляемый запрос, иначе - полученный ответ</param>
		/// <param name="data">Дамп в виде строки текста</param>
		/// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
		/// <returns></returns>
		public async Task DumpAsync(bool isRequest, string data, IRemoteClientDataUid remoteClientDataUid)
			{
			var task = Task.Run(() =>
			{
				RemoteHttpClientGlobals.LogDebug(data);
			});
			await task;
			}

		/// <summary>
		/// Препроцессор отправляемого запроса.
		/// Вызывается после забора содержимого Content.
		/// Предназначен для выставления различных заголовков
		/// </summary>
		/// <param name="method">Метод отправки</param>
		/// <param name="request">Отправляемое сообщение</param>
		/// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
		public virtual Task PrepareHttpRequestMessageAsync(HttpMethod method, HttpRequestMessage request, IRemoteClientDataUid remoteClientDataUid)
			{
			return Task.CompletedTask;
			}

		/// <summary>
		/// Отправляемое содержимое.
		/// Забирается во время вызова PrepareHttpRequestMessageAsync
		/// </summary>
		/// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
		public virtual Task<HttpContent> GetHttpContent(IRemoteClientDataUid remoteClientDataUid)
			{
			return Task.FromResult<HttpContent>(httpContent);
			}

		/// <summary>
		/// Обработчик полученного ответа
		/// </summary>
		/// <param name="method">Метод отправки</param>
		/// <param name="response">Полученный ответ</param>
		/// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
		/// <returns>Если true то обработка будет продолжена</returns>
		public virtual Task<bool> ResponseReceivedAsync(HttpMethod method, HttpResponseMessage response, IRemoteClientDataUid remoteClientDataUid)
			{
			return CompletedTaskBoolTrue;
			}

		/// <summary>
		/// Обработать результат извлеченный из HttpResponseMessage
		/// </summary>
		/// <param name="method">Метод отправки</param>
		/// <param name="response">Полученный ответ</param>
		/// <param name="wantedResponse">Результат извлеченный из HttpResponseMessage</param>
		/// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
		/// <returns></returns>
		public virtual async Task<bool> ProcessResponseAsync(HttpMethod method, HttpResponseMessage response, WantedResponse wantedResponse, IRemoteClientDataUid remoteClientDataUid)
			{
			if (wantedResponse != null && wantedResponse.StreamResponse != null && (!wantedResponse.StreamResponseIsMemoryStream))
				{
				using (MemoryStream streamToWriteTo = new MemoryStream())
					{
					await wantedResponse.StreamResponse.CopyToAsync(streamToWriteTo);
					}
				}

			wantedResponse?.Dispose();
			return true;
			}

		/// <summary>
		/// Обработчик исключений
		/// </summary>
		/// <param name="method">Метод отправки</param>
		/// <param name="exception">Возникшее исключение</param>
		/// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
		public virtual void ProcessException(HttpMethod method, Exception exception, IRemoteClientDataUid remoteClientDataUid)
			{
			var str = $"ProcessException: Method: {method.Method}, url: {remoteClientDataUid.Url}";
			RemoteHttpClientGlobals.LogException(exception, str);
			}

		/// <summary>
		/// Функция логгирования.
		/// Записать (например в БД) запрос и полученный ответ
		/// </summary>
		/// <param name="requestAndResponse">запрос и ответ</param>
		public Task LogRequestAndResponseAsync(RequestAndResponse requestAndResponse)
			{
			return CompletedTaskBoolTrue;
			}

		#endregion Реализация интерфейса IRemoteClientConsumer

		#region Реализация интерфейса IRemoteClientDataUid

		/// <summary>
		/// Уникальный идентификатор запроса
		/// </summary>
		public string RemoteClientDataUid
			{
			get;
			} = Guid.NewGuid().ToString().ToLower();

		/// <summary>
		/// Адрес назначения как его задал клиент
		/// </summary>
		public string Url
			{
			get;
			private set;
			}

		#endregion Реализация интерфейса IRemoteClientDataUid
		}
	}