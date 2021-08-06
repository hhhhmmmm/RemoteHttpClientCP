using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

//using RemoteHttpClient.Database;

namespace RemoteHttpClient.Http
	{
	// https://thomaslevesque.com/tag/httpclient/

	/// <summary>
	/// Фильтр по умолчанию для клиента HttpClient
	/// </summary>
	public class DefaultClientHttpClientHandler : HttpClientHandler
		{
		#region Конструкторы

		/// <summary>
		/// Конструктор
		/// </summary>
		public DefaultClientHttpClientHandler()
			{
			AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

			//UseProxy = false;
			//Proxy = null;
			// UseCookies = false;
			}

		#endregion Конструкторы

		/// <summary>
		/// Перегруженный метод SendAsync
		/// </summary>
		/// <param name="request">Отправляемый запрос</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		protected override async Task<HttpResponseMessage> SendAsync
			(
			HttpRequestMessage request,
			System.Threading.CancellationToken cancellationToken
			)
			{
			IRemoteClientConsumer remoteClientConsumer = null;
			IRemoteClientDataUid iRemoteClientDataUid = null;
			var DisableLogging = false;
			var DumpRequestOrResponse = false;

			try
				{
				var properties = request.GetRemoteClientPrivateProperties();
				if (properties != null)
					{
					remoteClientConsumer = properties.RemoteClientConsumer;
					iRemoteClientDataUid = properties.RemoteClientDataUid;
					DisableLogging = properties.DisableLogging;
					if (remoteClientConsumer != null)
						{
						DumpRequestOrResponse = remoteClientConsumer.DumpRequest | remoteClientConsumer.DumpResponse;
						}
					}

				if (remoteClientConsumer != null && remoteClientConsumer.DumpRequest && (!DisableLogging)) // выводится из свойства properties
					{
					var requestText = await request.DumpRequestToStringAsync().ConfigureAwait(false);
					await remoteClientConsumer.DumpAsync(true, requestText, properties.RemoteClientDataUid).ConfigureAwait(false);
					}

				var response = await InternalSendAsync(request, cancellationToken, properties, remoteClientConsumer, iRemoteClientDataUid);

				if (response != null)
					{
					if (remoteClientConsumer != null && remoteClientConsumer.DumpResponse && (!DisableLogging))
						{
						var responseText = await response.DumpResponseToStringAsync().ConfigureAwait(false);
						await remoteClientConsumer.DumpAsync(false, responseText, properties.RemoteClientDataUid).ConfigureAwait(false);
						}

					#region Журналирование запроса если не отключено

					if (DumpRequestOrResponse && (iRemoteClientDataUid != null) && (!DisableLogging))
						{
						var requestAndResponse = new RequestAndResponse(iRemoteClientDataUid.RemoteClientDataUid, request, response);

						if (remoteClientConsumer != null)
							{
							await remoteClientConsumer.LogRequestAndResponseAsync(requestAndResponse);
							}
						}

					#endregion Журналирование запроса если не отключено
					}

				return response;
				} // end try
			catch (InvalidOperationException e)
				{
				remoteClientConsumer?.ProcessException(request.Method, e, iRemoteClientDataUid);
				}
			catch (Exception e)
				{
				remoteClientConsumer?.ProcessException(request.Method, e, iRemoteClientDataUid);
				}
			return null;
			}

		/// <summary>
		/// Внутренний метод вызываемый из SendAsync
		/// </summary>
		/// <param name="request">Отправляемый запрос</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <param name="remoteClientPrivateProperties">Класс с приватными свойствами отправляемого запроса</param>
		/// <param name="remoteClientConsumer">Интерфейс клиента класса RemoteClient</param>
		/// <param name="iRemoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
		/// <returns></returns>
		private async Task<HttpResponseMessage> InternalSendAsync
			(
			HttpRequestMessage request,
			System.Threading.CancellationToken cancellationToken,
			RemoteClientPrivateProperties remoteClientPrivateProperties,
			IRemoteClientConsumer remoteClientConsumer,
			IRemoteClientDataUid iRemoteClientDataUid
			)
			{
			const int DELAY_TIME_1_S = 1000; // одна секунда

			// const int MAX_SENDING_ATTEMPTS = 4; // всего - 10 секунд

			var nSendingAttempt = 0; // Попыток отправить

			for (; ; )
				{
				try
					{
					#region Отправка сообщения

#pragma warning disable 4014  // Because this call is not awaited, execution of the current method continues before the call is completed.

					var responseTask = base.SendAsync(request, cancellationToken);
					responseTask.ConfigureAwait(false);

					var response = await responseTask;

#pragma warning restore 4014

					return response;

					#endregion Отправка сообщения
					} // end try
				catch (HttpRequestException e)
					{
					WebException webException = null;

					#region Ждем интервал перед повторной отправкой запроса

					await Task.Delay(DELAY_TIME_1_S * RemoteHttpClientGlobals.ResendInternalInSeconds, cancellationToken);

					#endregion Ждем интервал перед повторной отправкой запроса

					#region Вычисление WebException

					if ((e.InnerException != null) && e.InnerException is WebException we)
						{
						webException = we;
						}

					if (webException == null) // Не WebException, разбираемся уровнем выше
						{
						throw;
						}

					#endregion Вычисление WebException

					switch (webException.Status)
						{
						case WebExceptionStatus.SecureChannelFailure: // "The request was aborted: Could not create SSL/TLS secure channel."
								{
								remoteClientPrivateProperties?.PerformanceDataInstance?.IncrementSslTlsErrorCount();
								nSendingAttempt++;
								if (nSendingAttempt <= RemoteHttpClientGlobals.MaxSendingAttempts)
									{
									await Task.Delay(DELAY_TIME_1_S * nSendingAttempt, cancellationToken);
									continue; // продолжаем попытки отправить сообщение
									}
								goto process_exception; // завершение цикла обработки
								}

						case WebExceptionStatus.NameResolutionFailure:
								{
								nSendingAttempt++;
								if (nSendingAttempt <= RemoteHttpClientGlobals.MaxSendingAttempts)
									{
									await Task.Delay(DELAY_TIME_1_S * nSendingAttempt, cancellationToken);
									continue; // продолжаем попытки отправить сообщение
									}
								goto process_exception; // завершение цикла обработки
								}

						default:
								{
								break;
								}
						} // end switch

process_exception:

					if (webException != null)
						{
						remoteClientConsumer?.ProcessException(request.Method, webException, iRemoteClientDataUid);
						}
					else
						{
						remoteClientConsumer?.ProcessException(request.Method, e, iRemoteClientDataUid);
						}

					goto end_loop; // завершение цикла обработки
					} // end catch (HttpRequestException
				} // end for

end_loop:
			return null;
			}
		}
	}