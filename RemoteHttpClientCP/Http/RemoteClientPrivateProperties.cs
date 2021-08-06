using System;
using System.Text;
using RemoteHttpClient.Http.Performance;

namespace RemoteHttpClient.Http
    {
    /// <summary>
    /// Класс с приватными свойствами отправляемого запроса
    /// </summary>
    public sealed class RemoteClientPrivateProperties
        {
        /// <summary>
        /// Интерфейс клиента класса RemoteClient
        /// </summary>
        public IRemoteClientConsumer RemoteClientConsumer
            {
            get;
            set;
            }

        /// <summary>
        /// Интерфейс уникального идентификатора отправляемых данных
        /// </summary>
        public IRemoteClientDataUid RemoteClientDataUid
            {
            get;
            set;
            }

        /// <summary>
        /// Счетчик производительности запроса
        /// </summary>
        public PerformanceData PerformanceDataInstance
            {
            get;
            set;
            }

		/// <summary>
		/// Запретить логгирование запросов в журнал
		/// </summary>
		public bool DisableLogging
			{
			get;
			set;
			}
		}
    }