using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using RemoteHttpClient.Helpers;
using RemoteHttpClient.Http;

namespace RemoteHttpClient.RemoteClientConsumers
    {
    /// <summary>
    /// Простая реализация потребителя
    /// </summary>
    public class IrccConsumer : IrccImplementation, ICookieHelper
        {
        #region Конструкторы

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="url">Адрес назначения как его задал клиент</param>
        /// <param name="uid">Интерфейс уникального идентификатора отправляемых данных</param>
        /// <param name="proxy">Прокси сервер</param>
        /// <param name="standaloneInstance">Использовать отдельный экземпляр</param>
        public IrccConsumer(string url, IRemoteClientDataUid uid, IWebProxy proxy, bool standaloneInstance) : base(url)
            {
            Proxy = proxy;

            if (uid != null)
                {
                Uid = uid;
                }
            else
                {
                Uid = this;
                }
            StandaloneInstance = standaloneInstance;
            RemoteClientInstance = CreateRemoteClient();
            }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="url">Адрес назначения как его задал клиент</param>
        /// <param name="uid">Интерфейс уникального идентификатора отправляемых данных</param>
        /// <param name="proxy">Прокси сервер</param>
        public IrccConsumer(string url, IRemoteClientDataUid uid, IWebProxy proxy) : this(url, uid, proxy, false)
            {
            }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="url">Адрес назначения как его задал клиент</param>
        /// <param name="proxy">Прокси сервер</param>
        public IrccConsumer(string url, IWebProxy proxy) : this(url, null, proxy)
            {
            Uid = this;
            }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="url">Адрес назначения как его задал клиент</param>
        /// <param name="uid">Интерфейс уникального идентификатора отправляемых данных</param>
        public IrccConsumer(string url, IRemoteClientDataUid uid) : this(url, uid, null)
            {
            }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="url">Адрес назначения как его задал клиент</param>
        public IrccConsumer(string url) : this(url, null, null)
            {
            Uid = this;
            }

        #endregion Конструкторы

        #region Свойства

        /// <summary>
        /// Использовать отдельный экземпляр
        /// </summary>
        public bool StandaloneInstance
            {
            get;
            protected set;
            }

        /// <summary>
        /// Прокси сервер
        /// </summary>
        public IWebProxy Proxy
            {
            get;
            protected set;
            }

        /// <summary>
        /// Интерфейс уникального идентификатора отправляемых данных
        /// </summary>
        public IRemoteClientDataUid Uid
            {
            get;
            protected set;
            }

        /// <summary>
        /// Результат выполнения команды
        /// </summary>
        public string StringResult
            {
            get;
            protected set;
            }

        /// <summary>
        /// Код ответа
        /// </summary>
        public HttpStatusCode StatusCode
            {
            get;
            protected set;
            }

        /// <summary>
        /// Код ответа успешен
        /// </summary>
        public bool IsSuccessStatusCode
            {
            get;
            protected set;
            }

        /// <summary>
        /// Экземпляр клиента http
        /// </summary>
        public RemoteClient RemoteClientInstance
            {
            get;
            private set;
            }

        /// <summary>
        /// Токен отмены выполнения
        /// </summary>
        public CancellationTokenSource CancellationTokenSource
            {
            get;
            private set;
            }

        #endregion Свойства

        #region Перегруженные методы

        /// <summary>
        /// Обработать результат извлеченный из HttpResponseMessage
        /// </summary>
        /// <param name="method">Метод отправки</param>
        /// <param name="response">Полученный ответ</param>
        /// <param name="wantedResponse">Результат извлеченный из HttpResponseMessage</param>
        /// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
        /// <returns></returns>
        public override Task<bool> ProcessResponseAsync(HttpMethod method, HttpResponseMessage response, WantedResponse wantedResponse, IRemoteClientDataUid remoteClientDataUid)
            {
            StringResult = wantedResponse.StringResponse;
            StatusCode = wantedResponse.StatusCode;
            IsSuccessStatusCode = wantedResponse.IsSuccessStatusCode;

            wantedResponse?.Dispose();
            return TaskHelpers.CompletedTaskBoolTrue;
            }

        #endregion Перегруженные методы

        #region Вспомогательные методы

        /// <summary>
        /// Установить токен отмены
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        public void SetCancellationTokenSource(CancellationTokenSource cancellationTokenSource)
            {
            CancellationTokenSource = cancellationTokenSource;
            }

        /// <summary>
        /// Создать клиента
        /// </summary>
        /// <returns></returns>
        private RemoteClient CreateRemoteClient()
            {
            bool bres;
            var remoteclientFactory = RemoteClientFactory.GetInstance();
            if (remoteclientFactory == null)
                {
                return null;
                }

            RemoteClient remoteclient = null;

            if (StandaloneInstance)
                {
                remoteclient = remoteclientFactory.GetStandaloneInstance(Url, Proxy);
                }
            else
                {
                remoteclient = remoteclientFactory.GetInstance(Url, Proxy);
                }

            if (remoteclient == null)
                {
                return null;
                }

            bres = remoteclient.Initialize();
            if (!bres)
                {
                remoteclient?.Dispose();
                return null;
                }

            remoteclient.CreateAndSetCookieContainer();

            return remoteclient;
            }

        /// <summary>
        /// Получить UID запроса
        /// </summary>
        /// <returns>UID запроса</returns>
        private IRemoteClientDataUid GetIRemoteClientDataUid()
            {
            return Uid;
            }

        #endregion Вспомогательные методы

        #region Методы

        /// <summary>
        /// Отправить Post запрос асинхронно
        /// </summary>
        /// <returns></returns>
        public async Task<bool> PostAsync()
            {
            if (RemoteClientInstance == null)
                {
                return false;
                }

            var remoteClientDataUid = GetIRemoteClientDataUid();

            var bres = await RemoteClientInstance.PostAsync(this, remoteClientDataUid, WantedResponseType.String, CancellationTokenSource);
            if (!bres)
                {
                return false;
                }

            return true;
            }

        /// <summary>
        /// Отправить GET запрос асинхронно
        /// </summary>
        /// <returns></returns>
        public async Task<bool> GetAsync()
            {
            if (RemoteClientInstance == null)
                {
                return false;
                }

            var remoteClientDataUid = GetIRemoteClientDataUid();

            var bres = await RemoteClientInstance.GetAsync(this, remoteClientDataUid, WantedResponseType.String, CancellationTokenSource);
            if (!bres)
                {
                return false;
                }

            return true;
            }

        /// <summary>
        /// Отправить PUT запрос асинхронно
        /// </summary>
        /// <returns></returns>
        public async Task<bool> PutAsync()
            {
            if (RemoteClientInstance == null)
                {
                return false;
                }

            var remoteClientDataUid = GetIRemoteClientDataUid();

            var bres = await RemoteClientInstance.PutAsync(this, remoteClientDataUid, WantedResponseType.String, CancellationTokenSource);
            if (!bres)
                {
                return false;
                }

            return true;
            }

        /// <summary>
        /// Отправить DELETE запрос асинхронно
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DeleteAsync()
            {
            if (RemoteClientInstance == null)
                {
                return false;
                }

            var remoteClientDataUid = GetIRemoteClientDataUid();

            var bres = await RemoteClientInstance.PutAsync(this, remoteClientDataUid, WantedResponseType.String, CancellationTokenSource);
            if (!bres)
                {
                return false;
                }

            return true;
            }

        /// <summary>
        /// Отправить HEAD запрос асинхронно
        /// </summary>
        /// <returns></returns>
        public async Task<bool> HeadAsync()
            {
            if (RemoteClientInstance == null)
                {
                return false;
                }

            var remoteClientDataUid = GetIRemoteClientDataUid();

            var bres = await RemoteClientInstance.HeadAsync(this, remoteClientDataUid, WantedResponseType.String, CancellationTokenSource);
            if (!bres)
                {
                return false;
                }

            return true;
            }

        #endregion Методы

        #region Реализация интерфейса ICookieHelper

        #region Свойства

        /// <summary>
        /// Контейнер куков
        /// </summary>
        public CookieContainer CookieContainer
            {
            get
                {
                return RemoteClientInstance.CookieContainer;
                }
            }

        /// <summary>
        /// Контейнер куков в виде массива
        /// </summary>
        Cookie[] ICookieHelper.CookiesArray
            {
            get
                {
                return RemoteClientInstance.CookiesArray;
                }
            }

        /// <summary>
        /// Кука аутентификации FormsAuthentication
        /// ".ASPXAUTH"
        /// </summary>
        Cookie ICookieHelper.FormsAuthenticationCookie
            {
            get
                {
                return RemoteClientInstance.FormsAuthenticationCookie;
                }
            }

        #endregion Свойства

        #region Методы

        /// <summary>
        /// Установить контейнер с куками
        /// </summary>
        /// <param name="cookieContainer">Контейнер с куками</param>
        public void SetCookieContainer(CookieContainer cookieContainer)
            {
            RemoteClientInstance.SetCookieContainer(cookieContainer);
            }

        /// <summary>
        /// Создать и установить контейнер для куков
        /// </summary>
        public void CreateAndSetCookieContainer()
            {
            RemoteClientInstance.CreateAndSetCookieContainer();
            }

        /// <summary>
        /// Получить коллекцию всех куков
        /// </summary>
        /// <returns>Коллекция всех куков</returns>
        CookieCollection ICookieHelper.GetAllCookiesCollection()
            {
            return RemoteClientInstance.GetAllCookiesCollection();
            }

        /// <summary>
        /// Добавить куку
        /// </summary>
        /// <param name="name">название куки</param>
        /// <param name="value">значение куки</param>
        void ICookieHelper.AddCookie(string name, string value)
            {
            RemoteClientInstance.AddCookie(name, value);
            }

        /// <summary>
        /// Создать и добавить куку из существующей
        /// Берется только название и значение куки
        /// </summary>
        /// <param name="cookie">кука</param>
        void ICookieHelper.AddCookieFromExisting(Cookie cookie)
            {
            RemoteClientInstance.AddCookieFromExisting(cookie);
            }

        /// <summary>
        /// Добавить все куки из существующего контейнера куков
        /// </summary>
        /// <param name="cookieContainer">Контейнер куков</param>
        void ICookieHelper.AddAllCookiesFromCookieContainer(CookieContainer cookieContainer)
            {
            RemoteClientInstance.AddAllCookiesFromCookieContainer(cookieContainer);
            }

        #endregion Методы

        #endregion Реализация интерфейса ICookieHelper

        #region Реализация интерфейса IDisposable

        /// <summary>
        /// Освободить управляемые ресурсы
        /// </summary>
        protected override void DisposeManagedResources()
            {
            if (RemoteClientInstance != null)
                {
                RemoteClientInstance.Dispose();
                RemoteClientInstance = null;
                }
            base.DisposeManagedResources();
            }

        #endregion Реализация интерфейса IDisposable

        #region Простая фабрика

        public static IrccConsumer CreateStandaloneInstance(string url, IRemoteClientDataUid uid = null, IWebProxy proxy = null)
            {
            var consumer = new IrccConsumer(url, uid, proxy, true);
            return consumer;
            }

        #endregion Простая фабрика
        }
    }