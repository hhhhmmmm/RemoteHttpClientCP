using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace RemoteHttpClient.Http
	{
	/// <summary>
	/// Фабрика клиентов http - синглтон
	/// </summary>
	public class RemoteClientFactory
		{
		#region Статические мемберы

		/// <summary>
		/// Экземпляр синглтона
		/// </summary>
		private static RemoteClientFactory m_FactoryInstance;

		/// <summary>
		/// Статический локер
		/// </summary>
		private static readonly object StaticLocker = new object();

		#endregion Статические мемберы

		#region Свойства

		/// <summary>
		/// Собирать статистику производительности
		/// </summary>
		public static bool CollectPerformanceStatistics
			{
			get;
			set;
			}

		/// <summary>
		/// Собирать статистику производительности
		/// </summary>
		public static IWebProxy HttpProxy
			{
			get;
			set;
			}

		#endregion Свойства

		#region Статические методы

		/// <summary>
		/// Получить экземпляр фабрики
		/// </summary>
		/// <returns></returns>
		public static RemoteClientFactory GetInstance()
			{
			lock (StaticLocker)
				{
				if (m_FactoryInstance == null)
					{
					m_FactoryInstance = new RemoteClientFactory();
					}
				return m_FactoryInstance;
				} // end lock
			}

		#endregion Статические методы

		#region Мемберы

		/// <summary>
		/// Локер
		/// </summary>
		private readonly object Locker = new object();

		/// <summary>
		/// Набор HttpClient'ов
		/// </summary>
		private readonly Dictionary<Tuple<string, IWebProxy>, RemoteClient> m_Instances = new Dictionary<Tuple<string, IWebProxy>, RemoteClient>();

		#endregion	Мемберы

		#region Конструкторы

		/// <summary>
		/// Конструктор
		/// </summary>
		private RemoteClientFactory()
			{
			}

		#endregion Конструкторы

		#region Внутренние методы

		/// <summary>
		/// Нормализовать url
		/// </summary>
		/// <param name="Url">Адрес куда отправляем запрос</param>
		/// <returns></returns>
		private static string NormalizeBaseUrl(string Url)
			{
			return Url.EndsWith("/") ? Url : Url + "/";
			}

		/// <summary>
		/// Генератор уникальных номеров RemoteClient
		/// </summary>
		private int UidGenerator;

		/// <summary>
		/// Создать экземпляр клиента
		/// </summary>
		/// <param name="url">Адрес куда отправляем запрос</param>
		/// <param name="standAlone">Не помещать в кэш</param>
		/// <param name="collectPerformanceStatistics">Адрес куда отправляем запрос</param>
		/// <param name="proxy">"Экзепляр прокси</param>
		/// <returns></returns>
		private RemoteClient GetRemoteClient(string url, bool standAlone, bool collectPerformanceStatistics, IWebProxy proxy)
			{
			lock (Locker)
				{
				url = NormalizeBaseUrl(url);
				var client = new RemoteClient(url, collectPerformanceStatistics);
				Interlocked.Increment(ref UidGenerator);
				if (standAlone)
					{
					client.SetIsStandalone();
					client.SetUid((-UidGenerator).ToString());
					}
				else
					{
					client.SetUid(UidGenerator.ToString());
					}

				client.SetProxy(proxy);
				return client;
				}
			}

		#endregion Внутренние методы

		#region Создатели экземпляров

		/// <summary>
		/// Получить экземпляр клиента
		/// </summary>
		/// <param name="url">Адрес куда отправляем запрос</param>
		/// <returns></returns>
		public RemoteClient GetInstance(string url)
			{
			return GetInstance(url, HttpProxy, GetRemoteClient);
			}

		/// <summary>
		/// Получить экземпляр клиента
		/// </summary>
		/// <param name="url">Адрес куда отправляем запрос</param>
		/// <param name="proxy">Экземпляр прокси сервера</param>
		/// <returns></returns>
		public RemoteClient GetInstance(string url, IWebProxy proxy)
			{
			return GetInstance(url, proxy, GetRemoteClient);
			}

		/// <summary>
		/// Получить экземпляр клиента
		/// </summary>
		/// <param name="Url">Адрес куда отправляем запрос</param>
		/// <param name="proxy">Экземпляр прокси сервера</param>
		/// <param name="remoteClientCreator">Функция создания удаленного клиента для заданого Url</param>
		/// <returns></returns>
		public RemoteClient GetInstance(string Url, IWebProxy proxy, Func<string, bool, bool, IWebProxy, RemoteClient> remoteClientCreator)
			{
			if (string.IsNullOrEmpty(Url))
				{
				throw new ArgumentNullException(nameof(Url));
				}

			if (remoteClientCreator == null)
				{
				throw new ArgumentNullException(nameof(remoteClientCreator));
				}
			lock (Locker)
				{
				var key = Tuple.Create<string, IWebProxy>(Url, proxy); // ключ Url - прокси
				if (m_Instances.ContainsKey(key)) // первый поиск
					{
					return m_Instances[key];
					}

#pragma warning disable DF0001 // Marks undisposed anonymous objects from method invocations.
				var newclient = remoteClientCreator?.Invoke(Url, false, CollectPerformanceStatistics, proxy);
#pragma warning restore DF0001 // Marks undisposed anonymous objects from method invocations.
				if (newclient != null)
					{
					m_Instances.Add(key, newclient);
					return newclient;
					}
				return null;
				} // end lock
			}

		/// <summary>
		/// Получить экземпляр клиента не присутствующий в кеше
		/// </summary>
		/// <param name="url">Адрес куда отправляем запрос</param>
		/// <param name="proxy">Экземпляр прокси сервера</param>
		/// <param name="remoteClientCreator">Функция создания удаленного клиента для заданого Url</param>
		/// <returns></returns>
		public RemoteClient GetStandaloneInstance(string url, IWebProxy proxy, Func<string, bool, bool, IWebProxy, RemoteClient> remoteClientCreator)
			{
			if (string.IsNullOrEmpty(url))
				{
				throw new ArgumentNullException(nameof(url));
				}
			var newclient = remoteClientCreator?.Invoke(url, true, CollectPerformanceStatistics, proxy);
			return newclient;
			}

		/// <summary>
		/// Получить экземпляр клиента не присутствующий в кеше
		/// </summary>
		/// <param name="url">Адрес куда отправляем запрос</param>
		/// <param name="proxy">Экземпляр прокси сервера</param>
		/// <returns></returns>
		public RemoteClient GetStandaloneInstance(string url, IWebProxy proxy)
			{
			var newclient = GetStandaloneInstance(url, proxy, GetRemoteClient);
			return newclient;
			}

		#endregion Создатели экземпляров
		}
	}