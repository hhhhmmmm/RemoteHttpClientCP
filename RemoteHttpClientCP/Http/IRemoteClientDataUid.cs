namespace RemoteHttpClient.Http
    {
    /// <summary>
    /// Интерфейс уникального идентификатора отправляемых данных
    /// </summary>
    public interface IRemoteClientDataUid
        {
        /// <summary>
        /// Уникальный идентификатор запроса
        /// </summary>
        string RemoteClientDataUid
            {
            get;
            }

        /// <summary>
        /// Адрес назначения как его задал клиент
        /// </summary>
        string Url
            {
            get;
            }
        }
    }