using System;
using RemoteHttpClient.Helpers;
using RemoteHttpClient.Http;
 
namespace RemoteHttpClient.Config
	{
	/// <summary>
	/// Настройки из файла конфигурации
	/// </summary>
	public class ConfigFileSettings : ConfigFileSettingsBase
		{
		#region Конструкторы
 
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="messageChannel">канал сообщений</param>
		public ConfigFileSettings(IMessageChannel messageChannel) : base(messageChannel)
			{
			}

		#endregion Конструкторы

		#region Константы

		/// <summary>
		/// Выводить на экран запрос и ответ
		/// </summary>
		private const string cDumpRequestAndResponse = "DumpRequestAndResponse";

		/// <summary>
		/// Собирать статистику о производительности
		/// </summary>
		private const string сCollectPerformanceStatistics = "CollectPerformanceStatistics";

		/// <summary>
		/// Максимальное количество попыток отправки запроса
		/// </summary>
		private const string сMaxSendingAttempts = "MaxSendingAttempts";

		/// <summary>
		/// Интервал перед повторной отправкой запроса
		/// в секундах
		/// </summary>
		private const string сResendInternalInSeconds = "ResendInternalInSeconds";

		#endregion Константы

		#region Свойства

		/// <summary>
		/// Собирать статистику о производительности
		/// </summary>
		public bool CollectPerformanceStatistics
			{
			get
				{
				return GetAppConfigBool(сCollectPerformanceStatistics);
				}
			}

		/// <summary>
		/// Выводить на экран запрос и ответ
		/// </summary>
		public bool DumpRequestAndResponse
			{
			get
				{
				return GetAppConfigBool(cDumpRequestAndResponse);
				}
			}

		/// <summary>
		/// Максимальное количество попыток отправки запроса
		/// </summary>
		public int MaxSendingAttempts
			{
			get
				{
				int maxSendingAttempts;
				if (GetAppConfigInt(сMaxSendingAttempts, out maxSendingAttempts))
					{
					if (maxSendingAttempts > 0)
						{
						return maxSendingAttempts;
						}
					}
				return 4;
				}
			}

		/// <summary>
		/// Интервал перед повторной отправкой запроса
		/// в секундах
		/// </summary>
		public int ResendInternalInSeconds
			{
			get
				{
				if (GetAppConfigInt(сResendInternalInSeconds, out int resendInternalInSeconds))
					{
					if (resendInternalInSeconds > 0)
						{
						return resendInternalInSeconds;
						}
					}
				return 1;
				}
			}

		/// <summary>
		/// Настройки прокси сервера
		/// </summary>
		public HttpProxySettings ProxySettings
			{
			get
				{
				var useProxy = GetAppConfigBool("UseProxy");
				var serverIPAddress = GetAppConfigString("ProxyServerIPAddress");

				int serverPort;

				if (GetAppConfigInt("ProxyServerPort", out serverPort))
					{
					}

				var useCustomNetworkCredential = GetAppConfigBool("ProxyUseCustomNetworkCredential");
				var networkCredentialUserName = GetAppConfigString("ProxyNetworkCredentialUserName");
				var networkCredentialPassword = GetAppConfigString("ProxyNetworkCredentialPassword");
				var networkCredentialDomain = GetAppConfigString("ProxyNetworkCredentialDomain");
				var bypassProxyOnLocal = GetAppConfigBool("ProxyBypassProxyOnLocal");
				var bypassList = GetAppConfigString("ProxyBypassList");
				var seps = new char[] { ';', ',' };
				string[] bypassListArr = null;
				if (!string.IsNullOrEmpty(bypassList))
					{
					bypassListArr = bypassList.Split(seps, StringSplitOptions.RemoveEmptyEntries);
					}

				var ps = HttpProxySettings.Create(
					useProxy,
					serverIPAddress,
					serverPort,
					useCustomNetworkCredential,
					networkCredentialUserName,
					networkCredentialPassword,
					networkCredentialDomain,
					bypassProxyOnLocal,
					bypassListArr
					);
				return ps;
				}
			}

		#endregion Свойства
		}
	}