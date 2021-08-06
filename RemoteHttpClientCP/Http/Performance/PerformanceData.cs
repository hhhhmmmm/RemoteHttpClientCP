using System;
using System.Threading;

namespace RemoteHttpClient.Http.Performance
	{
	/// <summary>
	/// Класс для сбора информации о производительности
	/// </summary>
	public sealed class PerformanceData
		{
		#region Мемберы

		/// <summary>
		/// Количество ошибок The request was aborted: Could not create SSL/TLS secure channel.
		/// </summary>
		private long _SslTlsErrorCount;

		#endregion Мемберы

		#region Методы

		/// <summary>
		/// Увеличить количество ошибок протокола TlsSsl
		/// </summary>
		public void IncrementSslTlsErrorCount()
			{
			Interlocked.Increment(ref _SslTlsErrorCount);
			}

		#endregion Методы

		#region Конструкторы

		/// <summary>
		/// Конструктор
		/// </summary>
		public PerformanceData()
			{
			CreationDate = DateTime.Now.TimeOfDay;
			}

		#endregion Конструкторы

		#region Свойства

		/// <summary>
		/// Количество ошибок The request was aborted: Could not create SSL/TLS secure channel
		/// </summary>
		public long SslTlsErrorCount
			{
			get
				{
				return _SslTlsErrorCount;
				}
			}

		/// <summary>
		/// Время начала выполнения запроса
		/// </summary>
		public TimeSpan CreationDate
			{
			get;
			private set;
			}

		/// <summary>
		/// Уникальный идентификатор запроса
		/// </summary>
		public string RemoteClientDataUid
			{
			get;
			set;
			}

		/// <summary>
		/// Метод вызова
		/// </summary>
		public string Method
			{
			get;
			set;
			}

		/// <summary>
		/// Уникальный идентификатор экземпляра HttpClient
		/// </summary>
		public string Uid
			{
			get;
			set;
			}

		/// <summary>
		/// Url
		/// </summary>
		public string Url
			{
			get;
			set;
			}

		/// <summary>
		/// Метод чтения ответа
		/// </summary>
		public WantedResponseType ResponseType
			{
			get;
			set;
			}

		/// <summary>
		/// Счетчик подготовки сообщения
		/// </summary>
		public HighPerformanceCounter PrepareHttpRequestMessageAsync
			{
			get;
			} = new HighPerformanceCounter("PrepareHttpRequestMessageAsync");

		/// <summary>
		/// Счетчик получения контента для отправки
		/// </summary>
		public HighPerformanceCounter GetHttpContent
			{
			get;
			} = new HighPerformanceCounter("GetHttpContent");

		/// <summary>
		/// Счетчик отправки данных
		/// </summary>
		public HighPerformanceCounter SendAsync
			{
			get;
			} = new HighPerformanceCounter("SendAsync");

		/// <summary>
		/// Счетчик обработки полученных данных
		/// </summary>
		public HighPerformanceCounter ResponseReceivedAsync
			{
			get;
			} = new HighPerformanceCounter("ResponseReceivedAsync");

		/// <summary>
		/// Счетчик получения данных и их обработки
		/// </summary>
		public HighPerformanceCounter responseTask
			{
			get;
			} = new HighPerformanceCounter("responseTask");

		/// <summary>
		/// Счетчик получения данных
		/// </summary>
		public HighPerformanceCounter ReadAsync
			{
			get;
			} = new HighPerformanceCounter("ReadAsync");

		/// <summary>
		/// Счетчик обработки данных
		/// </summary>
		public HighPerformanceCounter ProcessResponseAsync
			{
			get;
			} = new HighPerformanceCounter("ProcessResponseAsync");

		#endregion Свойства
		}
	}