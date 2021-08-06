using System;
using System.Text;

namespace RemoteHttpClient.Http
    {
    /// <summary>
    /// Простейшая реализация интерфейса IRemoteClientDataUid
    /// </summary>
    public class RemoteClientDataUidImplementation : IRemoteClientDataUid
        {
        #region Конструкторы

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="url">Адрес назначения как его задал клиент</param>
        /// <param name="remoteClientDataUid">Уникальный идентификатор запроса</param>
        public RemoteClientDataUidImplementation(string url, string remoteClientDataUid)
            {
            if (string.IsNullOrEmpty(url))
                {
                throw new ArgumentException(nameof(url));
                }

            if (string.IsNullOrEmpty(remoteClientDataUid))
                {
                throw new ArgumentException(nameof(remoteClientDataUid));
                }

            Url = url;
            RemoteClientDataUid = remoteClientDataUid;
            }

        #endregion Конструкторы

        #region Реализация интерфейса IRemoteClientDataUid

        /// <summary>
        /// Уникальный идентификатор запроса
        /// </summary>
        public string RemoteClientDataUid
            {
            get;
            private set;
            }

        /// <summary>
        /// Адрес назначения как его задал клиент
        /// </summary>
        public string Url
            {
            get;
            private set;
            }

        #endregion Реализация интерфейса IRemoteClientDataUid
        }
    }