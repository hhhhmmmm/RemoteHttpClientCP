using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RemoteHttpClient.RemoteClientConsumers;
using RemoteHttpClient.Helpers;
using RemoteHttpClient.Http;
using System.Net.Http;
using System.IO;

namespace RemoteHttpClient.Unit.Test
	{
	public class ProxyTestIrccImplementation : IrccImplementation
		{
		public ProxyTestIrccImplementation(string url) : base(url)
			{
			}

		#region Свойства

		/// <summary>
		/// Строковый результат
		/// </summary>
		public string StringResult
			{
			get;
			private set;
			}

		public HttpStatusCode StatusCode
			{
			get;
			private set;
			}

		public bool IsSuccessStatusCode
			{
			get;
			private set;
			}

		#endregion Свойства


		/// <summary>
		/// Обработать результат извлеченный из HttpResponseMessage
		/// </summary>
		/// <param name="method">Метод отправки</param>
		/// <param name="response">Полученный ответ</param>
		/// <param name="wantedResponse">Результат извлеченный из HttpResponseMessage</param>
		/// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
		/// <returns></returns>
		public override async Task<bool> ProcessResponseAsync(HttpMethod method, HttpResponseMessage response, WantedResponse wantedResponse, IRemoteClientDataUid remoteClientDataUid)
			{
			if (wantedResponse != null && wantedResponse.StreamResponse != null)
				{
				using (MemoryStream streamToWriteTo = new MemoryStream())
					{
					await wantedResponse.StreamResponse.CopyToAsync(streamToWriteTo);
					}
				}

			StringResult = wantedResponse.StringResponse;
			StatusCode = wantedResponse.StatusCode;
			IsSuccessStatusCode = wantedResponse.IsSuccessStatusCode;

			wantedResponse?.Dispose();
			return true;
			}

		/// <summary>
		/// Обработать результат в виде строки извлеченный из HttpResponseMessage
		/// </summary>
		/// <param name="method">Метод отправки</param>
		/// <param name="response">Полученный ответ</param>
		/// <param name="stringResult">Результат в виде строки извлеченный из HttpResponseMessage</param>
		/// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
		/// <returns></returns>
		//public override Task<bool> ProcessResponseAsStringAsync(HttpMethod method, HttpResponseMessage response, string stringResult, IRemoteClientDataUid remoteClientDataUid)
		//	{
		//	StringResult = stringResult;
		//	StatusCode = response.StatusCode;
		//	IsSuccessStatusCode = response.IsSuccessStatusCode;
		//	return TaskHelpers.CompletedTaskBoolTrue;
		//	}
		}
	}
