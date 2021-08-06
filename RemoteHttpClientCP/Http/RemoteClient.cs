using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using RemoteHttpClient.Debuggers;
using RemoteHttpClient.Helpers;
using RemoteHttpClient.Http.Performance;

namespace RemoteHttpClient.Http
    {
    /// <summary>
    /// Класс удаленного клиента http
    /// </summary>
    public class RemoteClient : IDisposableImplementation<RemoteClient>, IDebuggable, ICookieHelper
        {
        #region Мемберы

        /// <summary>
        /// Клиент HttpClient
        /// </summary>
        private HttpClient _httpClient;

        /// <summary>
        /// The default message handler used by HttpClient
        /// </summary>
        private HttpClientHandler _httpClientHandler;

        /// <summary>ы
        /// прокси-сервер используемый для запрсоов
        /// </summary>
        private IWebProxy _proxy;

        /// <summary>
        /// Локер
        /// </summary>
        protected readonly object Locker = new object();

        #endregion Мемберы

        #region Свойства

        /// <summary>
        ///  Фильтр по умолчанию для клиента HttpClient
        /// </summary>
        public HttpClientHandler DefaultHttpClientHandler
            {
            get
                {
                return _httpClientHandler;
                }
            }

        /// <summary>
        /// Одиночный экземпляр
        /// </summary>
        public bool IsStandalone
            {
            get;
            private set;
            }

        /// <summary>
        /// Собирать статистику производительности
        /// </summary>
        public bool CollectPerformanceStatistics
            {
            get;
            private set;
            }

        /// <summary>
        /// Запретить логгирование запросов в журнал
        /// </summary>
        public bool DisableLogging
            {
            get;
            private set;
            }

        /// <summary>
        /// Объект инициализирован
        /// </summary>
        public bool IsInitialized
            {
            get;
            private set;
            }

        /// <summary>
        /// Клиент http
        /// </summary>
        public HttpClient httpClient
            {
            get
                {
                return _httpClient;
                }
            }

        /// <summary>
        /// Уникальный идентификатор экземпляра HttpClient
        /// </summary>
        public string Uid
            {
            get;
            private set;
            }

        /// <summary>
        /// BaseAddress
        /// </summary>
        public Uri BaseAddress
            {
            get
                {
                if (_httpClient != null)
                    {
                    return _httpClient.BaseAddress;
                    }
                return null;
                }
            }

        /// <summary>
        /// Таймаут по умолчанию
        /// </summary>
        public TimeSpan? DefaultTimeout
            {
            get;
            set;
            }

        /// <summary>
        /// User-agent по умолчанию
        /// </summary>
        public string DefaultUserAgent
            {
            get;
            set;
            }

        /// <summary>
        /// Uri по умолчанию
        /// </summary>
        public Uri DefaultUri
            {
            get;
            set;
            }

        /// <summary>
        /// Accept по умолчанию
        /// </summary>
        public string DefaultAccept
            {
            get;
            set;
            } = HttpConstants.MediaTypeApplicationJson;

        #endregion Свойства

        #region Конструкторы

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="url">Url хоста по умолчанию</param>
        /// <param name="collectPerformanceStatistics">Собирать статистику производительности</param>
        public RemoteClient(string url, bool collectPerformanceStatistics = false)
            {
            if (!string.IsNullOrEmpty(url))
                {
                var builder = new UriBuilder(url);
                DefaultUri = builder.Uri;
                }

            CollectPerformanceStatistics = collectPerformanceStatistics;
            }

        #endregion Конструкторы

        #region Реализация интерфейса IDisposable

        /// <summary>
        /// Освободить управляемые ресурсы
        /// </summary>
        protected override void DisposeManagedResources()
            {
            if (IsStandalone)
                {
                _httpClient?.Dispose();
                _httpClientHandler?.Dispose();
                }
            base.DisposeManagedResources();
            }

        #endregion Реализация интерфейса IDisposable

        #region Реализация интерфейса IDebuggable

        /// <summary>
        /// Интерфейс отладчика
        /// </summary>
        private IDebugger _iDebugger;

        /// <summary>
        /// Отладчик присоединен, идет какая-то отладка
        /// </summary>
        public bool IsDebugging
            {
            get
                {
                if (_iDebugger != null)
                    {
                    return true;
                    }

                return false;
                }
            }

        /// <summary>
        /// Интерфейс отладчика
        /// </summary>
        public IDebugger Debugger
            {
            get
                {
                return _iDebugger;
                }
            }

        /// <summary>
        /// Установить интерфейс отладчика
        /// </summary>
        /// <param name="debugger">интерфейс отладчика</param>
        public void SetIDebugger(IDebugger debugger)
            {
            _iDebugger = debugger;
            }

        /// <summary>
        /// Присоединить отладчик к объекту, если получится
        /// </summary>
        /// <param name="o">Объект к которому присоединяют отладчик</param>
        public void AttachDebugger(object o)
            {
            if (o == null)
                {
                return;
                }

            if (o.Equals(this))
                {
                return;
                }

            var idebuggable = o as IDebuggable;

            if (idebuggable == null)
                {
                return;
                }

            idebuggable.SetIDebugger(Debugger);
            }

        #endregion Реализация интерфейса IDebuggable

        #region Реализация интерфейса ICookieHelper

        /// <summary>
        /// Контейнер куков
        /// </summary>
        private CookieContainer _CookieContainer;

        #region Свойства

        /// <summary>
        /// Контейнер куков
        /// </summary>
        public CookieContainer CookieContainer
            {
            get
                {
                return _CookieContainer;
                }
            }

        /// <summary>
        /// Контейнер куков в виде массива
        /// </summary>
        public Cookie[] CookiesArray
            {
            get
                {
                if (CookieContainer == null)
                    {
                    return Array.Empty<Cookie>();
                    }

                var cookieCollection = GetAllCookiesCollection();
                var cookies = new List<Cookie>();
                foreach (Cookie cookie in cookieCollection)
                    {
                    cookies.Add(cookie);
                    }

                return cookies.ToArray();
                }
            }

        /// <summary>
        /// Кука аутентификации FormsAuthentication
        /// ".ASPXAUTH"
        /// </summary>
        public Cookie FormsAuthenticationCookie
            {
            get
                {
                var array = CookiesArray;
                if (array == null)
                    {
                    return null;
                    }
                var collection = GetAllCookiesCollection();
                var cookie = collection[".ASPXAUTH"];
                return cookie;
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
            _CookieContainer = cookieContainer;
            _httpClientHandler.CookieContainer = _CookieContainer;
            }

        /// <summary>
        /// Создать и установить контейнер для куков
        /// </summary>
        public void CreateAndSetCookieContainer()
            {
            var tmpCookieContainer = new CookieContainer();
            SetCookieContainer(tmpCookieContainer);
            }

        /// <summary>
        /// Получить коллекцию всех куков
        /// </summary>
        /// <returns>Коллекция всех куков</returns>
        public CookieCollection GetAllCookiesCollection()
            {
            return InternalGetAllCookiesCollection(CookieContainer);
            }

        /// <summary>
        /// Добавить куку
        /// </summary>
        /// <param name="name">название куки</param>
        /// <param name="value">значение куки</param>
        public void AddCookie(string name, string value)
            {
            if (CookieContainer == null)
                {
                return;
                }

            if (string.IsNullOrEmpty(name))
                {
                throw new ArgumentNullException(nameof(name));
                }

            var newCookie = new Cookie(name, value);
            newCookie.Domain = BaseAddress.Host;
            CookieContainer.Add(newCookie);
            }

        /// <summary>
        /// Создать и добавить куку из существующей
        /// Берется только название и значение куки
        /// </summary>
        /// <param name="cookie">кука</param>
        public void AddCookieFromExisting(Cookie cookie)
            {
            if (cookie == null)
                {
                throw new ArgumentNullException(nameof(cookie));
                }
            AddCookie(cookie.Name, cookie.Value);
            }

        /// <summary>
        /// Добавить все куки из существующего контейнера куков
        /// </summary>
        /// <param name="cookieContainer">Контейнер куков</param>
        public void AddAllCookiesFromCookieContainer(CookieContainer cookieContainer)
            {
            var collection = InternalGetAllCookiesCollection(cookieContainer);
            if (collection == null)
                {
                return;
                }
            foreach (Cookie cookie in collection)
                {
                AddCookieFromExisting(cookie);
                }
            }

        #endregion Методы

        #endregion Реализация интерфейса ICookieHelper

        #region Методы

        /// <summary>
        /// Установить признак одиночности экземпляра
        /// </summary>
        public void SetIsStandalone()
            {
            IsStandalone = true;
            }

        /// <summary>
        /// Установить запрет на логгирование
        /// </summary>
        public void SetDisableLogging()
            {
            DisableLogging = true;
            }

        /// <summary>
        /// Установить уникальный идентификатор экземпляра
        /// </summary>
        /// <param name="uid"></param>
        public void SetUid(string uid)
            {
            Uid = uid;
            }

        /// <summary>
        /// Установить использование прокси-сервера
        /// </summary>
        /// <param name="proxy">Интерфейс прокси</param>
        public void SetProxy(IWebProxy proxy)
            {
            _proxy = proxy;
            }

        /// <summary>
        ///  Инициализовать объект
        /// </summary>
        /// <returns>true в случае успеха</returns>
        public bool Initialize()
            {
            lock (Locker)
                {
                if (IsInitialized)
                    {
                    return true;
                    }
                var bres = InternalCreate();
                if (!bres)
                    {
                    return bres;
                    }
                bres = InternalInitialize();
                IsInitialized = bres;
                return bres;
                } // end locker
            }

        /// <summary>
        /// Проверка инициализации
        /// </summary>
        /// <returns>true если инициализирован</returns>
        protected bool CheckIsInitialized()
            {
            if (!IsInitialized)
                {
                throw new InvalidOperationException("Объект не инициализирован");
                }
            return IsInitialized;
            }

        /// <summary>
        /// Создание объекта и присвоение ему назначенных свойств
        /// </summary>
        /// <returns>true в случае успеха</returns>
        private bool InternalCreate()
            {
            _httpClientHandler = new DefaultClientHttpClientHandler();

            #region Прокси-сервер

            if (_proxy != null)
                {
                _httpClientHandler.Proxy = _proxy;
                _httpClientHandler.UseProxy = true;
                _httpClientHandler.PreAuthenticate = true;
                // _httpClientHandler.UseDefaultCredentials = false; по умолчанию
                // _httpClientHandler.AllowAutoRedirect = true; по умолчанию
                }

            #endregion Прокси-сервер

            if (IsStandalone)
                {
                _httpClient = new HttpClient(_httpClientHandler, true);
                }
            else
                {
                _httpClient = new HttpClient(_httpClientHandler, false);
                }

            #region Таймаут

            if (DefaultTimeout == null)
                {
                // DefaultTimeout = TimeSpan.FromSeconds(90);
                DefaultTimeout = TimeSpan.FromMinutes(60);
                }

            _httpClient.Timeout = (TimeSpan)DefaultTimeout;

            #endregion Таймаут

            #region UserAgent

            if (!string.IsNullOrEmpty(DefaultUserAgent))
                {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(DefaultUserAgent);
                }
            else
                {
                var assembly = Assembly.GetExecutingAssembly();
                var assemblyName = assembly.GetName();

                var header = new ProductHeaderValue(assemblyName.Name, assemblyName.Version.ToString());
                var userAgent = new ProductInfoHeaderValue(header);
                DefaultUserAgent = userAgent.Product.ToString();
                _httpClient.DefaultRequestHeaders.UserAgent.Add(userAgent);
                }

            #endregion UserAgent

            #region Uri

            if (DefaultUri != null)
                {
                _httpClient.BaseAddress = DefaultUri;
                }
            else
                {
                DefaultUri = _httpClient.BaseAddress;
                }

            #endregion Uri

            #region DefaultAccept

            if (!string.IsNullOrEmpty(DefaultAccept))
                {
                var tt = new MediaTypeWithQualityHeaderValue(DefaultAccept);
                if (!_httpClient.DefaultRequestHeaders.Accept.Contains(tt))
                    {
                    _httpClient.DefaultRequestHeaders.Accept.Add(tt);
                    }
                }

            #endregion DefaultAccept

            #region Куки

            CreateAndSetCookieContainer();

            #endregion Куки

            return true;
            }

        /// <summary>
        /// Добавить авторизацию Bearer
        /// </summary>
        /// <param name="token">Токен авторизации</param>
        public void AddBearerAuthorization(string token)
            {
            if (string.IsNullOrEmpty(token))
                {
                throw new ArgumentNullException(nameof(token));
                }

            CheckIsInitialized();

            token = token.Trim();

            if (!string.IsNullOrEmpty(token) && (_httpClient.DefaultRequestHeaders.Authorization == null))
                {
                var authValue = new AuthenticationHeaderValue("Bearer", token);
                httpClient.DefaultRequestHeaders.Authorization = authValue;
                }
            }

        #endregion Методы

        #region Перегружаемые методы

        /// <summary>
        /// Инициализовать объект - перегружаемый метод
        /// </summary>
        /// <returns>true в случае успеха</returns>
        protected virtual bool InternalInitialize()
            {
            return true;
            }

        #endregion Перегружаемые методы

        #region Базовый SendAsync

        // http://www.tugberkugurlu.com/archive/streaming-with-newnet-httpclient-and-httpcompletionoption-responseheadersread

        // C#: HttpClient should NOT be disposed
        // https://medium.com/@nuno.caneco/c-httpclient-should-not-be-disposed-or-should-it-45d2a8f568bc

        /// <summary>
        /// Отправить запрос асинхронно
        /// </summary>
        /// <param name="method">Метод отправки</param>
        /// <param name="consumer">Интерфейс клиента класса RemoteClient</param>
        /// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
        /// <param name="wantedResponseType">Тип желаемого загружаемого контента</param>
        /// <param name="cancellationTokenSource">Токен отмены выполнения</param>
        /// <returns>false в случае каких-либо ошибок, true в случае успеха</returns>
        private async Task<bool> SendAsync(HttpMethod method, IRemoteClientConsumer consumer, IRemoteClientDataUid remoteClientDataUid, WantedResponseType wantedResponseType, CancellationTokenSource cancellationTokenSource)
            {
            CheckIsInitialized();

            if (consumer == null)
                {
                throw new ArgumentNullException(nameof(consumer), $"{nameof(consumer)} не может быть null");
                }
            if (remoteClientDataUid == null)
                {
                throw new ArgumentNullException(nameof(remoteClientDataUid), $"{nameof(remoteClientDataUid)} не может быть null");
                }

            HttpRequestMessage request = null;
            HttpResponseMessage response = null;

            // Счетчики производительности
            PerformanceData performanceData = null;

            try
                {
                if (CollectPerformanceStatistics)
                    {
                    performanceData = new PerformanceData
                        {
                        RemoteClientDataUid = remoteClientDataUid.RemoteClientDataUid,
                        ResponseType = wantedResponseType
                        };
                    }

                #region Подготовка сообщения к отправке

                request = new HttpRequestMessage(method, remoteClientDataUid.Url);

                #region !!! Установка внутренних приватных свойств, трогать не нужно !!!

                var privateProperties = new RemoteClientPrivateProperties
                    {
                    RemoteClientConsumer = consumer,
                    RemoteClientDataUid = remoteClientDataUid,
                    PerformanceDataInstance = performanceData,
                    DisableLogging = DisableLogging
                    };

                request.SetRemoteClientPrivateProperties(privateProperties);

                #endregion !!! Установка внутренних приватных свойств, трогать не нужно !!!

                performanceData?.PrepareHttpRequestMessageAsync?.Start();
                await consumer.PrepareHttpRequestMessageAsync(method, request, remoteClientDataUid).ConfigureAwait(false);
                performanceData?.PrepareHttpRequestMessageAsync?.Stop();

                if (
                    !method.Equals(HttpMethod.Get) && // Для метода GET  содержимое body запрещено
                    !method.Equals(HttpMethod.Head)   // Для метода HEAD содержимое body запрещено
                    )
                    {
                    if (request.Content == null)
                        {
                        performanceData?.GetHttpContent?.Start();
                        request.Content = await consumer.GetHttpContent(remoteClientDataUid).ConfigureAwait(false);
                        performanceData?.GetHttpContent?.Stop();
                        }
                    }

                #endregion Подготовка сообщения к отправке

                #region Отправка сообщения

                Task<HttpResponseMessage> responseTask = null;

#pragma warning disable 4014  // Because this call is not awaited, execution of the current method continues before the call is completed.

                var completionOption = HttpCompletionOption.ResponseContentRead;

                if (wantedResponseType == WantedResponseType.Stream)
                    {
                    // Efficiently Streaming Large HTTP Responses With HttpClient
                    // http://www.tugberkugurlu.com/archive/efficiently-streaming-large-http-responses-with-httpclient
                    //
                    // если читаем ответ в поток, то выставляем опцию - только заголовок
                    // в этом случае ответ не буферизуется и мы получаем прямой сетевой поток

                    completionOption = HttpCompletionOption.ResponseHeadersRead;
                    }

                performanceData?.SendAsync?.Start();
                if (cancellationTokenSource != null)
                    {
                    responseTask = _httpClient.SendAsync(request, completionOption, cancellationTokenSource.Token);
                    }
                else
                    {
                    responseTask = _httpClient.SendAsync(request, completionOption);
                    }
                performanceData?.SendAsync?.Stop();

                responseTask.ConfigureAwait(false);

#pragma warning restore 4014

                #endregion Отправка сообщения

                #region Ожидание ответа сервера

                try
                    {
                    performanceData?.responseTask?.Start();
                    response = await responseTask;
                    performanceData?.responseTask?.Stop();
                    }
                catch (InvalidOperationException) // Handler did not return a response message
                    {
                    // consumer.ProcessException(method, exception, remoteClientDataUid); -- не нужно, уже обрабатывается в DefaultClientHttpClientHandler
                    return false;
                    }

                if (responseTask.Status != TaskStatus.RanToCompletion)
                    {
                    return false;
                    }

                if (response == null)
                    {
                    return false;
                    }

                #endregion Ожидание ответа сервера

                var processResult = false;

                #region Уведомление о приходе ответа сервера

                performanceData?.ResponseReceivedAsync?.Start();
                processResult = await consumer.ResponseReceivedAsync(method, response, remoteClientDataUid).ConfigureAwait(false);
                performanceData?.ResponseReceivedAsync?.Stop();

                // если ответ не понравился, то завершаем обработку
                if (!processResult)
                    {
                    return false;
                    }

                #endregion Уведомление о приходе ответа сервера

                // Pedro Félix's shared memory
                // https://blog.pedrofelix.org/2012/01/16/the-new-system-net-http-classes-message-content/

                // #region Тип контента

                processResult = await InternalReadAndProcessAsync(method, consumer, remoteClientDataUid, performanceData, response, wantedResponseType);

                return processResult;
                } // end try
            catch (AggregateException ae)
                {
                if (ae != null && ae.InnerException != null)
                    foreach (Exception e in ae.InnerExceptions)
                        {
                        consumer.ProcessException(method, e, remoteClientDataUid);
                        }
                }
            catch (Exception exception)
                {
                consumer.ProcessException(method, exception, remoteClientDataUid);
                }
            finally
                {
                if (IsDebugging)
                    {
                    if (CollectPerformanceStatistics && performanceData != null)
                        {
                        performanceData.Method = request.Method.Method;
                        performanceData.Uid = Uid;
                        performanceData.Url = request.RequestUri.ToString();

                        Debugger.ReportHttpPerformance(performanceData);
                        }
                    }

                request?.Dispose();
                response?.Dispose();
                }
            return false;
            }

        #endregion Базовый SendAsync

        #region Внутренние методы

        /// <summary>
        /// Получить коллекцию всех куков
        /// </summary>
        /// <param name="cookieContainer">Существующий контейнер куков</param>
        /// <returns></returns>
        private static CookieCollection InternalGetAllCookiesCollection(CookieContainer cookieContainer)
            {
            var cookies = cookieContainer;
            var allCookies = new CookieCollection();
            if (cookies == null)
                {
                return allCookies; // пустая коллекция
                }

            var domainTableField = cookies.GetType().GetRuntimeFields().FirstOrDefault(x => x.Name == "m_domainTable");
            var domains = (IDictionary)domainTableField.GetValue(cookies);

            foreach (var val in domains.Values)
                {
                var type = val.GetType().GetRuntimeFields().First(x => x.Name == "m_list");
                var values = (IDictionary)type.GetValue(val);
                foreach (CookieCollection cookiesInCol in values.Values)
                    {
                    allCookies.Add(cookiesInCol);
                    }
                }
            return allCookies;
            }

        /// <summary>
        /// Прочитать и обработать данные как строку
        /// </summary>
        /// <param name="method">Метод отправки</param>
        /// <param name="consumer">Интерфейс клиента класса RemoteClient</param>
        /// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
        /// <param name="performanceData">Счетчики производительности</param>
        /// <param name="response">Ответ сервера</param>
        /// <param name="wantedResponseType">Данные ответа сервера</param>
        /// <returns>Прочитанные данные в виде строки</returns>
        private static async Task<bool> InternalReadAndProcessAsync(HttpMethod method, IRemoteClientConsumer consumer, IRemoteClientDataUid remoteClientDataUid, PerformanceData performanceData, HttpResponseMessage response, WantedResponseType wantedResponseType)
            {
            var processResult = false;
            WantedResponse wr = null;
            switch (wantedResponseType)
                {
                case WantedResponseType.String:
                    {
                    #region Чтение данных как строки

                    performanceData?.ReadAsync?.Start();
                    var stringResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    performanceData?.ReadAsync?.Stop();

                    wr = WantedResponse.Create(response, stringResult);

                    #endregion Чтение данных как строки

                    break;
                    }
                case WantedResponseType.ByteArray:
                    {
                    #region Чтение данных как массива байт

                    performanceData?.ReadAsync?.Start();
                    var binaryResult = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    performanceData?.ReadAsync?.Stop();

                    wr = WantedResponse.Create(response, binaryResult);

                    #endregion Чтение данных как массива байт

                    break;
                    }
                case WantedResponseType.Stream:
                    {
                    //	Efficiently Streaming Large HTTP Responses With HttpClient
                    //	http://www.tugberkugurlu.com/archive/efficiently-streaming-large-http-responses-with-httpclient

                    Stream streamToReadFrom = null;

                    try
                        {
                        performanceData?.ReadAsync?.Start();
                        streamToReadFrom = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                        performanceData?.ReadAsync?.Stop();

                        wr = WantedResponse.Create(response, streamToReadFrom);
                        }
                    finally
                        {
                        // streamToReadFrom?.Dispose(); // Dispose() вызывается в WantedResponse()
                        }

                    break;
                    }
                default:
                    {
                    throw new InvalidOperationException($"Непонятный тип WantedResponseType = {wantedResponseType}");
                    }
                }

            #region Обработка данных

            performanceData?.ProcessResponseAsync?.Start();
            processResult = await consumer.ProcessResponseAsync(method, response, wr, remoteClientDataUid).ConfigureAwait(false);
            performanceData?.ProcessResponseAsync?.Stop();

            #endregion Обработка данных

            return processResult;
            }

        #endregion Внутренние методы

        #region Методы

        /// <summary>
        /// Отправить GET запрос асинхронно
        /// </summary>
        /// <param name="consumer">Интерфейс клиента класса RemoteClient</param>
        /// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
        /// <param name="wantedResponseType">Тип желаемого загружаемого контента</param>
        /// <param name="cancellationTokenSource">Токен отмены выполнения</param>
        /// <returns>false в случае каких-либо ошибок, true в случае успеха</returns>
        public async Task<bool> GetAsync(IRemoteClientConsumer consumer, IRemoteClientDataUid remoteClientDataUid, WantedResponseType wantedResponseType, CancellationTokenSource cancellationTokenSource)
            {
            return await SendAsync(HttpMethod.Get, consumer, remoteClientDataUid, wantedResponseType, cancellationTokenSource).ConfigureAwait(false);
            }

        /// <summary>
        /// Отправить HEAD запрос асинхронно
        /// </summary>
        /// <param name="consumer">Интерфейс клиента класса RemoteClient</param>
        /// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
        /// <param name="wantedResponseType">Тип желаемого загружаемого контента</param>
        /// <param name="cancellationTokenSource">Токен отмены выполнения</param>
        /// <returns>false в случае каких-либо ошибок, true в случае успеха</returns>
        public async Task<bool> HeadAsync(IRemoteClientConsumer consumer, IRemoteClientDataUid remoteClientDataUid, WantedResponseType wantedResponseType, CancellationTokenSource cancellationTokenSource)
            {
            return await SendAsync(HttpMethod.Head, consumer, remoteClientDataUid, wantedResponseType, cancellationTokenSource).ConfigureAwait(false);
            }

        /// <summary>
        /// Отправить POST запрос асинхронно
        /// </summary>
        /// <param name="consumer">Интерфейс клиента класса RemoteClient</param>
        /// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
        /// <param name="wantedResponseType">Тип желаемого загружаемого контента</param>
        /// <param name="cancellationTokenSource">Токен отмены выполнения</param>
        /// <returns>false в случае каких-либо ошибок, true в случае успеха</returns>
        public async Task<bool> PostAsync(IRemoteClientConsumer consumer, IRemoteClientDataUid remoteClientDataUid, WantedResponseType wantedResponseType, CancellationTokenSource cancellationTokenSource)
            {
            return await SendAsync(HttpMethod.Post, consumer, remoteClientDataUid, wantedResponseType, cancellationTokenSource).ConfigureAwait(false);
            }

        /// <summary>
        /// Отправить PUT запрос асинхронно
        /// </summary>
        /// <param name="consumer">Интерфейс клиента класса RemoteClient</param>
        /// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
        /// <param name="wantedResponseType">Тип желаемого загружаемого контента</param>
        /// <param name="cancellationTokenSource">Токен отмены выполнения</param>
        /// <returns>false в случае каких-либо ошибок, true в случае успеха</returns>
        public async Task<bool> PutAsync(IRemoteClientConsumer consumer, IRemoteClientDataUid remoteClientDataUid, WantedResponseType wantedResponseType, CancellationTokenSource cancellationTokenSource)
            {
            return await SendAsync(HttpMethod.Put, consumer, remoteClientDataUid, wantedResponseType, cancellationTokenSource).ConfigureAwait(false);
            }

        /// <summary>
        /// Отправить DELETE запрос асинхронно
        /// </summary>
        /// <param name="consumer">Интерфейс клиента класса RemoteClient</param>
        /// <param name="remoteClientDataUid">Интерфейс уникального идентификатора отправляемых данных</param>
        /// <param name="wantedResponseType">Тип желаемого загружаемого контента</param>
        /// <param name="cancellationTokenSource">Токен отмены выполнения</param>
        /// <returns>false в случае каких-либо ошибок, true в случае успеха</returns>
        public async Task<bool> DeleteAsync(IRemoteClientConsumer consumer, IRemoteClientDataUid remoteClientDataUid, WantedResponseType wantedResponseType, CancellationTokenSource cancellationTokenSource)
            {
            return await SendAsync(HttpMethod.Delete, consumer, remoteClientDataUid, wantedResponseType, cancellationTokenSource).ConfigureAwait(false);
            }

        #endregion Методы

        // https://stackoverflow.com/questions/44312426/set-header-when-call-httpclient-postasync
        }
    }