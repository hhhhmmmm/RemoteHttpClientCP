using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RemoteHttpClient.Http
	{
	/// <summary>
	/// Интерфейс клиента класса RemoteClient
	/// </summary>
	public interface IRemoteClientConsumer
		{
		#region Раздел отладки

		/// <summary>
		/// Вывести дамп отправляемого запроса
		/// </summary>
		bool DumpRequest
			{
			get;
			}

		/// <summary>
		/// Вывести дамп полученного ответа
		/// </summary>
		bool DumpResponse
			{
			get;
			}

		/// <summary>
		/// Выводит текстовый дамп отправляемого запроса или полученного ответа
		/// </summary>
		/// <param name="isRequest">Если true то это отправляемый запрос, иначе - полученный ответ</param>
		/// <param name="data">Дамп в виде строки текста</param>
		/// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
		/// <returns></returns>
		Task DumpAsync(bool isRequest, string data, IRemoteClientDataUid remoteClientDataUid);

		#endregion Раздел отладки

		/// <summary>
		/// Препроцессор отправляемого запроса.
		/// Вызывается до забора содержимого Content.
		/// 1. Предназначен для выставления различных заголовков
		/// 2. Но так же может быть выставлено и свойство request.Content,
		/// в этом случае вызов GetHttpContent не произыодится
		/// </summary>
		/// <param name="method">Метод отправки</param>
		/// <param name="request">Отправляемое сообщение</param>
		/// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
		Task PrepareHttpRequestMessageAsync(HttpMethod method, HttpRequestMessage request, IRemoteClientDataUid remoteClientDataUid);

		/// <summary>
		/// Отправляемое содержимое.
		/// Забирается во время вызова PrepareHttpRequestMessageAsync
		/// если во во время вызова PrepareHttpRequestMessageAsync свойство request.Content
		/// не было заполнено
		/// </summary>
		/// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
		Task<HttpContent> GetHttpContent(IRemoteClientDataUid remoteClientDataUid);

		/// <summary>
		/// Обработчик полученного ответа
		/// </summary>
		/// <param name="method">Метод отправки</param>
		/// <param name="response">Полученный ответ</param>
		/// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
		/// <returns>Если true то обработка будет продолжена</returns>
		Task<bool> ResponseReceivedAsync(HttpMethod method, HttpResponseMessage response, IRemoteClientDataUid remoteClientDataUid);

		/// <summary>
		/// Обработать результат извлеченный из HttpResponseMessage
		/// </summary>
		/// <param name="method">Метод отправки</param>
		/// <param name="response">Полученный ответ</param>
		/// <param name="wantedResponse">Результат извлеченный из HttpResponseMessage</param>
		/// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
		/// <returns></returns>
		Task<bool> ProcessResponseAsync(HttpMethod method, HttpResponseMessage response, WantedResponse wantedResponse, IRemoteClientDataUid remoteClientDataUid);

		/// <summary>
		/// Обработчик исключений
		/// </summary>
		/// <param name="method">Метод отправки</param>
		/// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
		/// <param name="exception">Возникшее исключение</param>
		void ProcessException(HttpMethod method, Exception exception, IRemoteClientDataUid remoteClientDataUid);

		/// <summary>
		/// Функция логгирования.
		/// Записать (например в БД) запрос и полученный ответ
		/// </summary>
		/// <param name="requestAndResponse">запрос и ответ</param>
		/// <returns></returns>
		Task LogRequestAndResponseAsync(RequestAndResponse requestAndResponse);
		}
	}