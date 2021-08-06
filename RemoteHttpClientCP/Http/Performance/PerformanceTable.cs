using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using RemoteHttpClient.Helpers;

namespace RemoteHttpClient.Http.Performance
	{
	/// <summary>
	/// Вспомогательный класс для измерения производительности
	/// </summary>
	public static class PerformanceTable
		{
		#region Методы расширения

		/// <summary>
		/// Метод расширения - сложить несколько значений TimeSpan
		/// </summary>
		/// <param name="timeSpanCollection"></param>
		/// <returns></returns>
		public static TimeSpan Sum(this IEnumerable<TimeSpan> timeSpanCollection)
			{
			return timeSpanCollection.Sum(s => s);
			}

		/// <summary>
		/// Метод расширения
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <param name="source"></param>
		/// <param name="func"></param>
		/// <returns></returns>
		public static TimeSpan Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, TimeSpan> func)
			{
			return new TimeSpan(source.Sum(item => func(item).Ticks));
			}

		#endregion Методы расширения

		#region Названия колонок

		/// <summary>
		/// Номер строки
		/// </summary>
		public static readonly string N = "N";

		/// <summary>
		/// Уникальный идентификатор HttpClient
		/// </summary>
		public static readonly string HttpClientUid = "HttpClientUid";

		/// <summary>
		/// Время начала выполнения запроса
		/// </summary>
		public static readonly string CreationDate = "Время начала выполнения запроса";

		/// <summary>
		/// Метод вызова
		/// </summary>
		public static readonly string Method = "Method";

		/// <summary>
		/// Уникальный идентификатор запроса
		/// </summary>
		public static readonly string RemoteClientDataUid = "RemoteClientDataUid";

		/// <summary>
		/// Url
		/// </summary>
		public static readonly string Url = "Url";

		/// <summary>
		/// Время выполнения PrepareHttpRequest
		/// </summary>
		public static readonly string PrepareHttpRequest = "Время PrepareHttpRequest";

		/// <summary>
		/// Время выполнения GetHttpContent
		/// </summary>
		public static readonly string GetHttpContent = "Время GetHttpContent";

		/// <summary>
		/// Время выполнения отправки SendAsync
		/// </summary>
		public static readonly string SendAsync = "Время отправки SendAsync";

		/// <summary>
		/// Время выполнения ResponseTask
		/// </summary>
		public static readonly string ResponseTask = "Время ResponseTask";

		/// <summary>
		/// Время выполнения ResponseReceivedAsync
		/// </summary>
		public static readonly string ResponseReceivedAsync = "Время ResponseReceivedAsync";

		/// <summary>
		/// Время выполнения ReadAsync
		/// </summary>
		public static readonly string ReadAsync = "Время ReadAsync";

		/// <summary>
		/// Метод чтения ответа
		/// </summary>
		public static readonly string WantedResponseType = "Метод чтения ответа";

		/// <summary>
		/// Время обработки ProcessResponseAsync
		/// </summary>
		public static readonly string ProcessResponseAsync = "Время ProcessResponseAsync";

		/// <summary>
		/// Количество ошибок The request was aborted: Could not create SSL/TLS secure channel
		/// </summary>
		public static readonly string SslTlsErrorCount = "К-во ошибок SSL/TLS";

		#endregion Названия колонок

		#region Публичные методы

		/// <summary>
		/// Сформировать название файла
		/// </summary>
		/// <param name="table">Таблица производительности</param>
		/// <returns></returns>
		public static string FormatFileName(DataTable table)
			{
			var sb = new StringBuilder();
			int nCount = table.Rows.Count;
			var maxDegreeOfParallelism = Helpers.TaskHelpers.GetMaxDegreeOfParallelism();

#if DEBUG
			sb.Append($"Статистика производительности - {nCount} строк, MDP_{maxDegreeOfParallelism}.txt");
#else
			sb.Append($"Статистика производительности - {nCount} строк.txt");
#endif // DEBUG

			return sb.ToString();
			}

		/// <summary>
		/// Подсчитать итого и сложить итоги
		/// </summary>
		/// <param name="table">Таблица производительности</param>
		/// <param name="additionalData">Дополнительные данные</param>
		public static void ComputeTotal(DataTable table, out string additionalData)
			{
			additionalData = null;

			#region Строка итогов

			var totalPrepareHttpRequest = ComputeTotalTimeSpanForColumn(table, PrepareHttpRequest);
			var totalGetHttpContent = ComputeTotalTimeSpanForColumn(table, GetHttpContent);
			var totalSendAsync = ComputeTotalTimeSpanForColumn(table, SendAsync);
			var totalResponseTask = ComputeTotalTimeSpanForColumn(table, ResponseTask);
			var totalResponseReceivedAsync = ComputeTotalTimeSpanForColumn(table, ResponseReceivedAsync);
			var totalReadAsStringAsync = ComputeTotalTimeSpanForColumn(table, ReadAsync);
			var totalProcessResponseAsync = ComputeTotalTimeSpanForColumn(table, ProcessResponseAsync);

			var totalSslTlsErrorCount = ComputeTotalLongForColumn(table, SslTlsErrorCount);
			AddPerformanceRow(table, string.Empty, string.Empty, null, "------", string.Empty, null, null, null, null, null, null, null, null, 0);
			AddPerformanceRow(table, string.Empty, string.Empty, null, "Итого:", string.Empty, totalPrepareHttpRequest, totalGetHttpContent, totalSendAsync, totalResponseTask, totalResponseReceivedAsync, totalReadAsStringAsync, null, totalProcessResponseAsync, totalSslTlsErrorCount);

			#endregion Строка итогов

			#region Расшифровка

			const string separator = "-------------------------------------";

			var sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendLine();
			sb.AppendLine("Значения колонок:");
			sb.AppendLine("PrepareHttpRequest - время подготовки запроса внутри ПО");
			sb.AppendLine("GetHttpContent - время подготовки контекста запроса по данным PrepareHttpRequest внутри ПО");
			sb.AppendLine("SendAsync - время отправки запроса из ПО на удаленный сервер");
			sb.AppendLine(separator);
			sb.AppendLine("ResponseTask - время ожидания ответа от удаленного сервера");
			sb.AppendLine(separator);
			sb.AppendLine("ResponseReceivedAsync - время обработки ответа внутри ПО");
			sb.AppendLine("ReadAsync - время чтения входных данных внутри ПО");
			sb.AppendLine("WantedResponseType - метод чтения ответа");
			sb.AppendLine("ProcessResponseAsync - время обработки загруженных данных внутри ПО");

			sb.AppendLine();
			sb.AppendLine();

			var performanceFrequency = HighPerformanceCounter.PerformanceFrequency;
			sb.AppendLine($"Точность счетчиков (отсчетов в секунду): {performanceFrequency}");

			sb.AppendLine($"Ошибок 'The request was aborted: Could not create SSL/TLS secure channel': {totalSslTlsErrorCount}");

			var outputTime = Sum(totalPrepareHttpRequest, totalGetHttpContent, totalSendAsync);
			sb.AppendLine($"Общее время подготовки и отправки (PrepareHttpRequest + GetHttpContent + SendAsync): {outputTime.ToString()}");

			var waitTime = totalResponseTask;
			sb.AppendLine($"Общее время ожидания ответа от удаленного сервера (ResponseTask): {waitTime.ToString()}");

			var inputTime = Sum(totalResponseReceivedAsync, totalReadAsStringAsync, totalProcessResponseAsync);
			sb.AppendLine($"Общее время обработки результата в ПО (ResponseReceivedAsync + ReadAsync + ProcessResponseAs[String+ByteArray+Stream]Async): {inputTime.ToString()}");

			var maxDegreeOfParallelism = Helpers.TaskHelpers.GetMaxDegreeOfParallelism();
			sb.AppendLine($"MaxDegreeOfParallelism = {maxDegreeOfParallelism}");

			// окончательный вывод
			additionalData = sb.ToString();

			#endregion Расшифровка
			}

		/// <summary>
		/// Создать таблицу со счетчиками производительности
		/// </summary>
		/// <param name="tableName">Название таблицы</param>
		/// <returns></returns>
		public static DataTable CreatePerformanceTable(string tableName = null)
			{
			DataTable table = null;
			if (string.IsNullOrEmpty(tableName))
				{
#pragma warning disable CC0021 // Use nameof
				table = new DataTable("Performance");
#pragma warning restore CC0021 // Use nameof
				}
			else
				{
				table = new DataTable(tableName);
				}

			var column = table.Columns.Add(N, typeof(int));
			column.AutoIncrement = true;
			column.AutoIncrementSeed = 1;
			column.AutoIncrementStep = 1;

			table.Columns.Add(HttpClientUid, typeof(string));
			table.Columns.Add(RemoteClientDataUid, typeof(string));
			table.Columns.Add(CreationDate, typeof(TimeSpan));
			table.Columns.Add(Method, typeof(string));
			table.Columns.Add(Url, typeof(string));
			table.Columns.Add(PrepareHttpRequest, typeof(TimeSpan));
			table.Columns.Add(GetHttpContent, typeof(TimeSpan));
			table.Columns.Add(SendAsync, typeof(TimeSpan));
			table.Columns.Add(ResponseTask, typeof(TimeSpan));
			table.Columns.Add(ResponseReceivedAsync, typeof(TimeSpan));
			table.Columns.Add(ReadAsync, typeof(TimeSpan));
			table.Columns.Add(WantedResponseType, typeof(string));
			table.Columns.Add(ProcessResponseAsync, typeof(TimeSpan));

			table.Columns.Add(SslTlsErrorCount, typeof(long));

			return table;
			}

		/// <summary>
		/// Вставить данные о производительности в таблицу
		/// </summary>
		/// <param name="table"></param>
		/// <param name="performanceData"></param>
		public static void InsertPerformanceDataInPerformanceTable(DataTable table, PerformanceData performanceData)
			{
			AddPerformanceRow
				(
				table,
				 performanceData.Uid,
				 performanceData.RemoteClientDataUid,
				 performanceData.CreationDate,
				 performanceData.Method,
				 performanceData.Url,
				 performanceData.PrepareHttpRequestMessageAsync.ElapsedTime,
				 performanceData.GetHttpContent.ElapsedTime,
				 performanceData.SendAsync.ElapsedTime,
				 performanceData.responseTask.ElapsedTime,
				 performanceData.ResponseReceivedAsync.ElapsedTime,
				 performanceData.ReadAsync.ElapsedTime,
				 performanceData.ResponseType,
				 performanceData.ProcessResponseAsync.ElapsedTime,
				 performanceData.SslTlsErrorCount

				);
			}

		/// <summary>
		/// Добавить строку производительности
		/// </summary>
		/// <param name="table">таблица производительности</param>
		/// <param name="remoteClientDataUid">Уникальный идентификатор запроса</param>
		/// <param name="httpClientUid">Уникальный идентификатор HttpClient</param>
		/// <param name="creationDate">Время начала выполнения запроса</param>
		/// <param name="method">Метод вызова</param>
		/// <param name="url">Url</param>
		/// <param name="prepareHttpRequest">Время выполнения PrepareHttpRequest</param>
		/// <param name="getHttpContent">Время выполнения getHttpContent</param>
		/// <param name="sendAsync">Время выполнения отправки SendAsync</param>
		/// <param name="responseTask">Время выполнения ResponseTask</param>
		/// <param name="responseReceivedAsync">Время выполнения ResponseReceivedAsync</param>
		/// <param name="readAsync">Время выполнения ReadAsync</param>
		/// <param name="wantedResponseType">Метод чтения ответа</param>
		/// <param name="processResponseAsync">Время обработки ProcessResponseAsync</param>
		/// <param name="sslTlsErrorCount">Количество ошибок The request was aborted: Could not create SSL/TLS secure channel</param>
		public static void AddPerformanceRow(
			DataTable table,
			string httpClientUid,
			string remoteClientDataUid,
			TimeSpan? creationDate,
			string method,
			string url,
			TimeSpan? prepareHttpRequest,
			TimeSpan? getHttpContent,
			TimeSpan? sendAsync,
			TimeSpan? responseTask,
			TimeSpan? responseReceivedAsync,
			TimeSpan? readAsync,
			WantedResponseType? wantedResponseType,
			TimeSpan? processResponseAsync,
			long sslTlsErrorCount
			)
			{
			var newrow = table.NewRow();
			newrow[HttpClientUid] = httpClientUid;
			newrow[RemoteClientDataUid] = remoteClientDataUid;
			newrow[CreationDate] = DbNullIfDbNullOrEmpty(creationDate);
			newrow[Method] = method;
			newrow[Url] = url;
			newrow[PrepareHttpRequest] = DbNullIfDbNullOrEmpty(prepareHttpRequest);
			newrow[GetHttpContent] = DbNullIfDbNullOrEmpty(getHttpContent);
			newrow[SendAsync] = DbNullIfDbNullOrEmpty(sendAsync);
			newrow[ResponseTask] = DbNullIfDbNullOrEmpty(responseTask);
			newrow[ResponseReceivedAsync] = DbNullIfDbNullOrEmpty(responseReceivedAsync);
			newrow[ReadAsync] = DbNullIfDbNullOrEmpty(readAsync);

			if (wantedResponseType != null)
				{
				switch (wantedResponseType)
					{
					case Http.WantedResponseType.String:
							{
							newrow[WantedResponseType] = "String";
							break;
							}
					case Http.WantedResponseType.ByteArray:
							{
							newrow[WantedResponseType] = "ByteArray";
							break;
							}
					case Http.WantedResponseType.Stream:
							{
							newrow[WantedResponseType] = "Stream";
							break;
							}
					default:
							{
							throw new ArgumentException($"{wantedResponseType}");
							}
					}
				}

			newrow[WantedResponseType] = wantedResponseType?.ToString();

			newrow[ProcessResponseAsync] = DbNullIfDbNullOrEmpty(processResponseAsync);
			newrow[SslTlsErrorCount] = sslTlsErrorCount;
			table.Rows.Add(newrow);
			}

		/// <summary>
		/// Таблица производительности в виде одной многострочной строки
		/// </summary>
		/// <param name="table">Таблица</param>
		/// <returns></returns>
		public static string PerformanceTableAsString(DataTable table)
			{
			if (table == null)
				{
				return string.Empty;
				}
			var s = TableFormatter.FormatTableAsString(table, true);
			return s;
			}

		#endregion Публичные методы

		#region Вспомогательные методы

		/// <summary>
		/// Сложить несколько значений TimeSpan
		/// </summary>
		/// <param name="data">Набор значений</param>
		/// <returns></returns>
		private static TimeSpan Sum(params TimeSpan[] data)
			{
			var en = data.AsEnumerable();
			return en.Sum();
			}

		/// <summary>
		/// Подсчитать сумму TimeSpan в колонке
		/// </summary>
		/// <param name="table">Таблица производительности</param>
		/// <param name="columnName">Название колонки</param>
		/// <returns></returns>
		private static TimeSpan ComputeTotalTimeSpanForColumn(DataTable table, string columnName)
			{
			var rows = table.AsEnumerable();
			var cells = rows.Select(x => x[columnName] as TimeSpan?);
			var cells2 = cells.OfType<TimeSpan>();
			var total = cells2.Sum();
			return total;
			}

		/// <summary>
		/// Подсчитать сумму TimeSpan в колонке
		/// </summary>
		/// <param name="table">Таблица производительности</param>
		/// <param name="columnName">Название колонки</param>
		/// <returns></returns>
		private static long ComputeTotalLongForColumn(DataTable table, string columnName)
			{
			var rows = table.AsEnumerable();
			var cells = rows.Select(x => x[columnName] as long?);
			var cells2 = cells.OfType<long>();
			var total = cells2.Sum();
			return total;
			}

		/// <summary>
		/// DBNull.Value если объект null, NULL или пустая строка
		/// </summary>
		/// <param name="o">Объект</param>
		/// <returns></returns>
		private static object DbNullIfDbNullOrEmpty(object o)
			{
			var res = NullIfDbNullOrEmpty(o);
			if (res == null)
				{
				return DBNull.Value;
				}
			return res;
			}

		/// <summary>
		/// Null если объект null, NULL или пустая строка
		/// </summary>
		/// <param name="o">Объект</param>
		/// <returns></returns>
		private static string NullIfDbNullOrEmpty(object o)
			{
			if (o == null)
				{
				return null;
				}

			if (o.Equals(DBNull.Value))
				{
				return null;
				}

			if (o is string s)
				{
				if (string.IsNullOrEmpty(s))
					{
					return null;
					}
				}

			return o.ToString();
			}

		#endregion Вспомогательные методы
		}
	}