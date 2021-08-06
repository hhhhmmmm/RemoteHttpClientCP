using System;
using System.Diagnostics;
using NUnit.Framework;
using RemoteHttpClient.Helpers;
using RemoteHttpClient.Http;

namespace RemoteHttpClient.Unit.Test
	{
	public class MessageHandlerBase : IMessageChannel
		{
		#region Конструкторы

		public MessageHandlerBase()
			{
			RemoteHttpClient.RemoteHttpClientGlobals.SetMessageHandlers(sRaiseError, sRaiseWarning, sRaiseMessage);
			RemoteHttpClient.RemoteHttpClientGlobals.SetLogHandlers(sLogError, sLogWarning, sLogInfo, sLogDebug, sLogException, sLogAggregateException);
			InitRemoteHttpClient();
			}

		#endregion Конструкторы

		#region Статические методы IMessageChannel

		public static void sRaiseError(string errorText)
			{
			Debug.WriteLine(errorText);
			}

		public static void sRaiseMessage(string messageText)
			{
			Debug.WriteLine(messageText);
			}

		public static void sRaiseWarning(string warningText)
			{
			Debug.WriteLine(warningText);
			}

		#endregion Статические методы IMessageChannel

		#region Статические методы лога

		public static void sLogError(string errorMessage)
			{
			Debug.WriteLine(errorMessage);
			}

		public static void sLogWarning(string warningMessage)
			{
			Debug.WriteLine(warningMessage);
			}

		public static void sLogInfo(string infoMessage)
			{
			Debug.WriteLine(infoMessage);
			}

		public static void sLogDebug(string debugMessage)
			{
			Debug.WriteLine(debugMessage);
			}

		public static void sLogException(Exception e, string text)
			{
			Debug.WriteLine($"Exception = {e}, text = {text}");
			}

		public static void sLogAggregateException(AggregateException ae, string text)
			{
			Debug.WriteLine($"AggregateException = {ae}, text = {text}");
			}

		#endregion Статические методы лога

		#region IMessageChannel

		public void RaiseError(string errorText)
			{
			sRaiseError(errorText);
			}

		public void RaiseMessage(string messageText)
			{
			sRaiseMessage(messageText);
			}

		public void RaiseWarning(string warningText)
			{
			sRaiseWarning(warningText);
			}

		#endregion IMessageChannel

		/// <summary>
		/// Инициализация библиотеки
		/// </summary>
		public static void InitRemoteHttpClient()
			{
			if (!RemoteHttpClient.RemoteHttpClientGlobals.IsInitialized)
				{
				var bres = RemoteHttpClient.RemoteHttpClientGlobals.Init();
				Assert.IsTrue(bres);
				}
			}

		#region Свойства

		/// <summary>
		/// Фабрика клиентов
		/// </summary>
		public RemoteClientFactory ClientFactory
			{
			get;
			set;
			}

		#endregion Свойства
		}
	}