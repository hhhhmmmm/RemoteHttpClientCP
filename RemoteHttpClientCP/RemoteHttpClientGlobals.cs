using System;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using RemoteHttpClient.Helpers;
using RemoteHttpClient.Http;

namespace RemoteHttpClient
	{
	/// <summary>
	/// Глобальные переменные и настройки
	/// </summary>
	public static class RemoteHttpClientGlobals
		{
		#region Внутренние переменные

		/// <summary>
		/// Библиотека инициализирована
		/// </summary>
		private static bool s_IsInitialized;

		/// <summary>
		/// Настройки из файла конфигурации RemoteHttpClient.dll.config
		/// </summary>
		private static Config.ConfigFileSettings s_ConfigFileSettings;

		#endregion Внутренние переменные

		#region Свойства

		/// <summary>
		/// Библиотека инициализирована
		/// </summary>
		public static bool IsInitialized
			{
			get
				{
				return s_IsInitialized;
				}
			}

		/// <summary>
		/// Реализация IMessageChannel по умолчанию
		/// </summary>
		public static IMessageChannel MessageChannel
			{
			get;
			private set;
			} = new MessageChannelImplementation();

		/// <summary>
		/// Настройки файла конфигурации библиотеки
		/// </summary>
		public static Config.ConfigFileSettings DllConfiguration
			{
			get
				{
				return s_ConfigFileSettings;
				}
			}

		/// <summary>
		/// Вывести дамп отправляемого запроса
		/// </summary>
		public static bool DumpRequest
			{
			get;
			private set;
			}

		/// <summary>
		/// Вывести дамп полученного ответа
		/// </summary>
		public static bool DumpResponse
			{
			get;
			private set;
			}

		/// <summary>
		/// Интервал перед повторной отправкой запроса
		/// в секундах
		/// </summary>
		public static int ResendInternalInSeconds
			{
			get;
			private set;
			}

		/// <summary>
		/// Собирать статистику о производительности
		/// </summary>
		public static bool CollectPerformanceStatistics
			{
			get;
			private set;
			}

		/// <summary>
		/// Максимальное количество попыток отправки запроса
		/// </summary>
		public static int MaxSendingAttempts
			{
			get;
			private set;
			}

		/// <summary>
		/// Внешний метод-валидатор сертификатов SSL
		/// </summary>
		public static Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> RemoteCertificateValidator
			{
			get;
			private set;
			}

		#endregion Свойства

		#region Свойства прокси-сервера

		/// <summary>
		/// Интерфейс прокси-сервера
		/// </summary>
		public static IWebProxy HttpProxy
			{
			get;
			private set;
			}

		/// <summary>
		/// Экземпляр прокси-сервера
		/// </summary>
		public static HttpProxySettings ProxySettings
			{
			get;
			private set;
			}

		#endregion Свойства прокси-сервера

		#region Вспомогательные методы

		/// <summary>
		/// Установка функции-валидатора сертификатов
		/// </summary>
		/// <param name="remoteCertificateValidator">Функция-валидатор сертификатов</param>
		public static void SetRemoteCertificateValidator(Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> remoteCertificateValidator)
			{
			RemoteCertificateValidator = remoteCertificateValidator;
			}

		/// <summary>
		/// Установить настройки прокси для библиотеки
		/// </summary>
		/// <param name="proxySettings">Настройки прокси</param>
		public static void SetHttpProxy(HttpProxySettings proxySettings)
			{
			var iwebproxy = proxySettings.CreateWebProxy();
			if (iwebproxy != null)
				{
				ProxySettings = proxySettings;
				HttpProxy = iwebproxy;
				RemoteClientFactory.HttpProxy = HttpProxy;
				}
			}

		/// <summary>
		/// Возвращает версию файла сборки (та, которая указывается в AssemblyFileVersion в файле AssemblyInfo.cs)
		/// </summary>
		/// <returns></returns>
		public static string GetDllFileVersion()
			{
			var ExecutingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
			var AssemblyLocation = new Uri(ExecutingAssembly.GetName().CodeBase).LocalPath;
			var fvi = FileVersionInfo.GetVersionInfo(AssemblyLocation);
			var version = fvi.FileVersion;
			return version;
			}

		#endregion Вспомогательные методы

		#region Разные логи

		/// <summary>
		/// Обработчик - показыватель ошибок
		/// </summary>
		public static Action<string> LogErrorHandler
			{
			get;
			private set;
			}

		/// <summary>
		/// Обработчик - показыватель предупреждений
		/// </summary>
		public static Action<string> LogWarningHandler
			{
			get;
			private set;
			}

		/// <summary>
		/// Обработчик - показыватель информации
		/// </summary>
		public static Action<string> LogInfoHandler
			{
			get;
			private set;
			}

		/// <summary>
		/// Обработчик - показыватель отладочной информации
		/// </summary>
		public static Action<string> LogDebugHandler
			{
			get;
			private set;
			}

		/// <summary>
		/// Обработчик исключений
		/// </summary>
		public static Action<Exception, string> LogExceptionHandler
			{
			get;
			private set;
			}

		/// <summary>
		/// Обработчик набора исключений
		/// </summary>
		public static Action<AggregateException, string> LogAggregateExceptionHandler
			{
			get;
			private set;
			}

		/// <summary>
		/// Вывести сообщение об ошибке
		/// </summary>
		/// <param name="errorMessage">Текст сообщения об ошибке</param>
		public static void LogError(string errorMessage)
			{
			LogErrorHandler?.Invoke(errorMessage);
			}

		/// <summary>
		/// Вывести сообщение-предупреждение
		/// </summary>
		/// <param name="warningMessage">Текст сообщения-предупреждения</param>
		public static void LogWarning(string warningMessage)
			{
			LogWarningHandler?.Invoke(warningMessage);
			}

		/// <summary>
		/// Вывести информационное сообщение
		/// </summary>
		/// <param name="infoMessage">Текст информационного сообщения</param>
		public static void LogInfo(string infoMessage)
			{
			LogInfoHandler?.Invoke(infoMessage);
			}

		/// <summary>
		/// Вывести отладочное сообщение
		/// </summary>
		/// <param name="debugMessage">Текст отладочного сообщения</param>
		public static void LogDebug(string debugMessage)
			{
			LogDebugHandler?.Invoke(debugMessage);
			}

		/// <summary>
		/// Вывести сообщение об исключении
		/// </summary>
		/// <param name="e">Исключение</param>
		/// <param name="text">Текст примечания</param>
		public static void LogException(Exception e, string text)
			{
			LogExceptionHandler?.Invoke(e, text);
			}

		/// <summary>
		/// Вывести сообщение об агрегированном исключении
		/// </summary>
		/// <param name="ae">Агрегированное исключение</param>
		/// <param name="text">Текст примечания</param>
		public static void LogAggregateException(AggregateException ae, string text)
			{
			LogAggregateExceptionHandler?.Invoke(ae, text);
			}

		/// <summary>
		/// Установить обработчики - показыватели сообщений
		/// </summary>
		/// <param name="errorHandler">Обработчик - показыватель ошибок</param>
		/// <param name="warningHandler">Обработчик - показыватель предупреждений</param>
		/// <param name="infoHandler">Обработчик - показыватель сообщений</param>
		/// <param name="debugHandler">Обработчик - показыватель сообщений</param>
		/// <param name="exceptionHandler">Обработчик - показыватель сообщений</param>
		/// <param name="aexceptionHandler">Обработчик - показыватель сообщений</param>
		public static void SetLogHandlers
			(
			Action<string> errorHandler,
			Action<string> warningHandler,
			Action<string> infoHandler,
			Action<string> debugHandler,
			Action<Exception, string> exceptionHandler,
			Action<AggregateException, string> aexceptionHandler
			)
			{
			LogErrorHandler = errorHandler;
			LogWarningHandler = warningHandler;
			LogInfoHandler = infoHandler;
			LogDebugHandler = debugHandler;
			LogExceptionHandler = exceptionHandler;
			LogAggregateExceptionHandler = aexceptionHandler;
			}

		#endregion Разные логи

		#region Разные Message

		/// <summary>
		/// Обработчик - показыватель ошибок
		/// </summary>
		public static Action<string> ErrorHandler
			{
			get;
			private set;
			}

		/// <summary>
		/// Обработчик - показыватель предупреждений
		/// </summary>
		public static Action<string> WarningHandler
			{
			get;
			private set;
			}

		/// <summary>
		/// Обработчик - показыватель сообщений
		/// </summary>
		public static Action<string> InfoHandler
			{
			get;
			private set;
			}

		/// <summary>
		/// Установить обработчики - показыватели сообщений
		/// </summary>
		/// <param name="errorHandler">Обработчик - показыватель ошибок</param>
		/// <param name="warningHandler">Обработчик - показыватель предупреждений</param>
		/// <param name="infoHandler">Обработчик - показыватель сообщений</param>
		public static void SetMessageHandlers(Action<string> errorHandler, Action<string> warningHandler, Action<string> infoHandler)
			{
			ErrorHandler = errorHandler;
			WarningHandler = warningHandler;
			InfoHandler = infoHandler;
			}

		/// <summary>
		/// Показать Message с ошибкой
		/// </summary>
		/// <param name="Text">Текст сообщения</param>
		public static void RaiseError(string Text)
			{
			ErrorHandler?.Invoke(Text);
			}

		/// <summary>
		/// Показать Message с предупреждением
		/// </summary>
		/// <param name="Text">Текст сообщения</param>
		public static void RaiseWarning(string Text)
			{
			WarningHandler?.Invoke(Text);
			}

		/// <summary>
		/// Показать Message
		/// </summary>
		/// <param name="Text">Текст сообщения</param>
		public static void RaiseMessage(string Text)
			{
			InfoHandler?.Invoke(Text);
			}

		#endregion Разные Message

		#region Инициализация

		/// <summary>
		/// Инициализация сетевых настроек
		/// </summary>
		/// <param name="messageChannel">Канал сообщений</param>
		private static bool InitNetwork(IMessageChannel messageChannel)
			{
			LogInfo("Инициализация сетевых настроек:");

			RemoteClientFactory.CollectPerformanceStatistics = CollectPerformanceStatistics;
			LogInfo($"CollectPerformanceStatistics = {RemoteClientFactory.CollectPerformanceStatistics.ToString()}");

			// ServicePointManager.Expect100Continue = true; // true
			ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
			// https://stackoverflow.com/questions/2859790/the-request-was-aborted-could-not-create-ssl-tls-secure-channel
			ServicePointManager.DefaultConnectionLimit = 100; // 2

			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
				   | SecurityProtocolType.Tls11
				   | SecurityProtocolType.Tls12;
			// | SecurityProtocolType.Ssl3;

			//ServicePointManager.ReusePort = true; // false
			//ServicePointManager.UseNagleAlgorithm = false; //

			var bres = true;
			if (!bres)
				{
				LogWarning("Ошибка инициализации сетевых настроек");
				}
			return bres;
			}

		/// <summary>
		/// Валидатор сертификатов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="certificate"></param>
		/// <param name="chain"></param>
		/// <param name="sslPolicyErrors"></param>
		/// <returns></returns>
		private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
			{
			if (RemoteCertificateValidator != null)
				{
				var bres = RemoteCertificateValidator(sender, certificate, chain, sslPolicyErrors);
				return bres;
				}

			return true;
			}

		/// <summary>
		/// Установить признак того, что библиотека НЕ инициализирована
		/// </summary>
		private static void SetNotInitialized()
			{
			s_IsInitialized = false;
			}

		/// <summary>
		/// Установить признак того, что библиотека инициализирована
		/// </summary>
		private static void SetInitialized()
			{
			s_IsInitialized = true;
			}

		/// <summary>
		/// Инициализация настроек прокси-сервера
		/// </summary>
		/// <returns></returns>
		private static bool InitProxyServerSettings()
			{
			var proxySettings = DllConfiguration.ProxySettings;
			if (proxySettings == null)
				{
				return true;
				}

			if (!proxySettings.UseProxy)
				{
				return true;
				}

			try
				{
				SetHttpProxy(proxySettings);
				}
			catch (ArgumentException ae)
				{
				RaiseError(ae.ToString());
				return false;
				}
			return true;
			}

		/// <summary>
		/// Инициализация глобальных настроек из DllConfiguration
		/// </summary>
		private static void InitGlobalVariables()
			{
			DumpRequest = DllConfiguration.DumpRequestAndResponse;
			DumpResponse = DllConfiguration.DumpRequestAndResponse;
			CollectPerformanceStatistics = DllConfiguration.CollectPerformanceStatistics;
			MaxSendingAttempts = DllConfiguration.MaxSendingAttempts;
			ResendInternalInSeconds = DllConfiguration.ResendInternalInSeconds;
			}

		/// <summary>
		/// Инициализация библиотеки из приложения - должна вызываться первой
		/// </summary>
		public static bool Init()
			{
			bool bres;

			if (IsInitialized)
				{
				throw new InvalidOperationException("Библиотека уже инициализирована");
				}

			s_ConfigFileSettings = new Config.ConfigFileSettings(MessageChannel);

			var configFileName = s_ConfigFileSettings.AssemblyConfigFileName;
			bres = s_ConfigFileSettings.Init();
			if (!bres)
				{
				RaiseWarning($"Ошибка чтения настроек библиотеки из файла {configFileName}");
				}
			else
				{
				LogInfo($"Файл конфигурации {configFileName} прочитан");
				}

			InitGlobalVariables();

			bres = InitNetwork(MessageChannel);
			if (!bres)
				{
				RaiseWarning("Ошибка инициализации сетевых настроек");
				}

			bres = InitProxyServerSettings();
			if (!bres)
				{
				RaiseWarning("Ошибка инициализации настроек прокси-сервера");
				return bres;
				}

			LogInfo($"Установлен интервал повторной отправки запросов - {ResendInternalInSeconds} секунд");
			SetInitialized();
			return true;
			}

		#endregion Инициализация
		}
	}