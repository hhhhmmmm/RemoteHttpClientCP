using System;
using System.Net;
using System.Text;

namespace RemoteHttpClient.Http
	{
	/// <summary>
	/// Класс с настройками прокси
	/// </summary>
	public sealed class HttpProxySettings
		{
		private WebProxy _instance;

		#region Конструкторы

		/// <summary>
		/// Конструктор
		/// </summary>
		private HttpProxySettings()
			{
			}

		#endregion Конструкторы

		#region Свойства

		/// <summary>
		/// Использовать прокси
		/// </summary>
		public bool UseProxy
			{
			get;
			private set;
			}

		/// <summary>
		/// IP адрес прокси-сервера
		/// </summary>
		public string ServerIPAddress
			{
			get;
			private set;
			}

		/// <summary>
		/// Порт прокси-сервера
		/// </summary>
		public int ServerPort
			{
			get;
			private set;
			}

		/// <summary>
		/// Использовать имя пользователя и пароль для аутентификации прокси сервера
		/// </summary>
		public bool UseCustomNetworkCredential
			{
			get;
			private set;
			}

		/// <summary>
		/// Имя пользователя прокси
		/// </summary>
		public string NetworkCredentialUserName
			{
			get;
			private set;
			}

		/// <summary>
		/// Пароль пользователя прокси
		/// </summary>
		public string NetworkCredentialPassword
			{
			get;
			private set;
			}

		/// <summary>
		/// Домен пользователя прокси
		/// </summary>
		public string NetworkCredentialDomain
			{
			get;
			private set;
			}

		/// <summary>
		/// Не использовать прокси для локальных адресов
		/// </summary>
		public bool BypassProxyOnLocal
			{
			get;
			private set;
			}

		/// <summary>
		/// Список адресов для которых прокси не используется
		/// </summary>
		public string[] BypassList
			{
			get;
			private set;
			}

		#endregion Свойства

		#region Фабрика

		/// <summary>
		/// Создать экземпляр
		/// </summary>
		/// <param name="useProxy">Использовать прокси</param>
		/// <param name="serverIPAddress">IP адрес прокси-сервера</param>
		/// <param name="serverPort">Порт прокси-сервера</param>
		/// <param name="useCustomNetworkCredential">Использовать имя пользователя и пароль </param>
		/// <param name="networkCredentialUserName">Имя пользователя прокси</param>
		/// <param name="networkCredentialPassword">Пароль пользователя прокси</param>
		/// <param name="networkCredentialDomain">Домен пользователя прокси</param>
		/// <param name="bypassProxyOnLocal">Не использовать прокси для локальных адресов</param>
		/// <param name="bypassList">Список адресов для которых прокси не используется</param>
		/// <returns></returns>
		public static HttpProxySettings Create(
				bool useProxy,
				string serverIPAddress,
				int serverPort,
				bool useCustomNetworkCredential,
				string networkCredentialUserName,
				string networkCredentialPassword,
				string networkCredentialDomain,
				bool bypassProxyOnLocal,
				string[] bypassList
				)
			{
			var ps = new HttpProxySettings();
			ps.UseProxy = useProxy;
			ps.ServerIPAddress = serverIPAddress;
			ps.ServerPort = serverPort;
			ps.UseCustomNetworkCredential = useCustomNetworkCredential;
			ps.NetworkCredentialUserName = networkCredentialUserName;
			ps.NetworkCredentialPassword = networkCredentialPassword;
			ps.NetworkCredentialDomain = networkCredentialDomain;
			ps.BypassProxyOnLocal = bypassProxyOnLocal;

			if (bypassList != null && bypassList.Length > 0)
				{
				ps.BypassList = bypassList;
				}

			return ps;
			}

		#endregion Фабрика

		#region Методы

		/// <summary>
		/// Получить описание настроек прокси
		/// </summary>
		/// <returns></returns>
		public string GetDescription()
			{
			var str = $"uri - {_instance.Address}";
			return str;
			}

		/// <summary>
		/// Создать экземпляр прокси-сервера
		/// </summary>
		/// <returns></returns>
		public IWebProxy CreateWebProxy()
			{
			if (!UseProxy)
				{
				return null;
				}

			if (string.IsNullOrEmpty(ServerIPAddress))
				{
				throw new ArgumentException("Включено использование прокси-сервера, но не указан адрес прокси-сервера");
				}

			var uriBuilder = new UriBuilder();
			uriBuilder.Host = ServerIPAddress;

			if (ServerPort > 0)
				{
				uriBuilder.Port = ServerPort;
				}
			else
				{
				throw new ArgumentException("Номер порта прокси-сервера должен быть > 0");
				}

			var uriAddress = uriBuilder.ToString();
			var webProxy = new WebProxy(uriAddress, BypassProxyOnLocal);

			var cred = GetNetworkCredentialForProxy();

			if (cred.Equals(CredentialCache.DefaultCredentials))
				{
				webProxy.UseDefaultCredentials = true;
				}
			else
				{
				webProxy.UseDefaultCredentials = false;
				webProxy.Credentials = cred;
				}

			webProxy.BypassProxyOnLocal = BypassProxyOnLocal;

			if (BypassList != null && BypassList.Length > 0)
				{
				webProxy.BypassList = BypassList;
				}

			_instance = webProxy;
			return webProxy;
			}

		/// <summary>
		/// Возвращает атрибуты отправителя
		/// </summary>
		/// <returns>ICredentials для прокси, логин и пароль для подключения к прокси</returns>
		private ICredentials GetNetworkCredentialForProxy()
			{
			if (UseCustomNetworkCredential)
				{
				if (string.IsNullOrEmpty(NetworkCredentialUserName))
					{
					throw new ArgumentException("Включено использование имени пользователя прокси-сервера, но не указано имя пользователя прокси-сервера");
					}
				return new NetworkCredential(NetworkCredentialUserName, NetworkCredentialPassword, NetworkCredentialDomain);
				}
			return CredentialCache.DefaultCredentials;
			}

		#endregion Методы
		}
	}