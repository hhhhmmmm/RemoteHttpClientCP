using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using RemoteHttpClient.Helpers;

namespace RemoteHttpClient.Http
	{
	/// <summary>
	/// Ответ сервера
	/// </summary>
	public sealed class WantedResponse : IDisposableImplementation<WantedResponse>
		{
		#region Конструкторы

		/// <summary>
		/// Конструктор
		/// </summary>
		private WantedResponse()
			{
			}

		#endregion Конструкторы

		#region Создатели

		/// <summary>
		/// Создатель
		/// </summary>
		/// <param name="response">ответ сервера</param>
		/// <param name="stringResponse">ответ сервера в виде строки</param>
		/// <returns></returns>
		public static WantedResponse Create(HttpResponseMessage response, string stringResponse)
			{
			var wr = new WantedResponse();
			wr.StringResponse = stringResponse;
			wr.ResponseType = WantedResponseType.String;
			wr.StatusCode = response.StatusCode;
			wr.IsSuccessStatusCode = response.IsSuccessStatusCode;
			wr.SetAllHeaders(response);
			return wr;
			}

		/// <summary>
		/// Создатель
		/// </summary>
		/// <param name="response">ответ сервера</param>
		/// <param name="byteArrayResponse">ответ сервера в виде массива</param>
		/// <returns></returns>
		public static WantedResponse Create(HttpResponseMessage response, byte[] byteArrayResponse)
			{
			var wr = new WantedResponse();
			wr.ByteArrayResponse = byteArrayResponse;
			wr.ResponseType = WantedResponseType.ByteArray;
			wr.StatusCode = response.StatusCode;
			wr.IsSuccessStatusCode = response.IsSuccessStatusCode;
			wr.SetAllHeaders(response);
			return wr;
			}

		/// <summary>
		/// Создатель
		/// </summary>
		/// <param name="response">ответ сервера</param>
		/// <param name="streamResponse">ответ сервера в виде потока</param>
		/// <returns></returns>
		public static WantedResponse Create(HttpResponseMessage response, Stream streamResponse)
			{
			var wr = new WantedResponse();
			wr.StreamResponse = streamResponse;
			wr.ResponseType = WantedResponseType.Stream;
			wr.StatusCode = response.StatusCode;
			wr.IsSuccessStatusCode = response.IsSuccessStatusCode;
			wr.SetAllHeaders(response);
			return wr;
			}

		#endregion Создатели

		#region Вспомогательные методы

		/// <summary>
		/// Сохранить заголовки контента если он есть
		/// </summary>
		/// <param name="response">Ответ сервера</param>
		private void SetContentHeaders(HttpResponseMessage response)
			{
			if (response != null && response.Content != null && response.Content.Headers != null)
				{
				var d = new Dictionary<string, string>();
				var h = response.Content.Headers.GetContentHeaders();
				foreach (var t in h)
					{
					d.Add(t.Item1, t.Item2);
					}
				ContentHeaders = d;
				}
			}

		/// <summary>
		/// Сохранить заголовки ответа
		/// </summary>
		/// <param name="response">Ответ сервера</param>
		private void SetHeaders(HttpResponseMessage response)
			{
			if (response != null)
				{
				var d = new Dictionary<string, string>();
				var h = response.GetHeaders();
				foreach (var t in h)
					{
					d.Add(t.Item1, t.Item2);
					}
				Headers = d;
				}
			}

		/// <summary>
		/// Сохранить все заголовки
		/// </summary>
		/// <param name="response">Ответ сервера</param>
		private void SetAllHeaders(HttpResponseMessage response)
			{
			SetHeaders(response);
			SetContentHeaders(response);
			}

		#endregion Вспомогательные методы

		#region Свойства

		/// <summary>
		/// Набор заголовков содержимого
		/// </summary>
		public Dictionary<string, string> ContentHeaders
			{
			get;
			private set;
			}

		/// <summary>
		/// Набор заголовков ответа
		/// </summary>
		public Dictionary<string, string> Headers
			{
			get;
			private set;
			}

		/// <summary>
		/// Код ответа успешный
		/// </summary>
		public bool IsSuccessStatusCode
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

		/// <summary>
		/// Код ответа в виде int
		/// </summary>
		public int iStatusCode
			{
			get
				{
				return (int) StatusCode;
				}
			}

		/// <summary>
		/// Тип ответа сервера
		/// </summary>
		public WantedResponseType ResponseType
			{
			get;
			private set;
			}

		/// <summary>
		/// Ответ в виде строки
		/// </summary>
		public string StringResponse
			{
			get;
			private set;
			}

		/// <summary>
		/// Ответ в виде массива байт
		/// </summary>
		public byte[] ByteArrayResponse
			{
			get;
			private set;
			}

		/// <summary>
		/// Ответ в виде потока
		/// </summary>
		public Stream StreamResponse
			{
			get;
			private set;
			}

		/// <summary>
		/// Входной поток - поток к памяти
		/// </summary>
		public bool StreamResponseIsMemoryStream
			{
			get
				{
				if (StreamResponse == null)
					{
					return false;
					}
				if (StreamResponse is MemoryStream)
					{
					return true;
					}
				return false;
				}
			}

		#endregion Свойства

		#region Реализация интерфейса IDisposable

		/// <summary>
		/// Освободить управляемые ресурсы
		/// </summary>
		protected override void DisposeManagedResources()
			{
			StreamResponse?.Dispose();
			base.DisposeManagedResources();
			}

		#endregion Реализация интерфейса IDisposable
		}
	}