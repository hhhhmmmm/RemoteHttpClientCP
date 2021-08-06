using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RemoteHttpClient.Http
    {
    /// <summary>
    /// Класс расширяет набор методов HttpRequestMessage
    /// </summary>
    public static class HttpRequestMessageExtensions
        {
        #region Приватное свойство добавляемое в каждый запрос

        /// <summary>
        /// Приватное свойство добавляемое в каждый запрос
        /// </summary>
        private static readonly string RemoteClientPrivatePropertiesName = "Http_RemoteClientPrivateProperties";

        /// <summary>
        /// Получить приватное свойство RemoteClientPrivatePropertiesName добавляемое в каждый запрос
        /// </summary>
        /// <param name="request">Запрос</param>
        /// <returns></returns>
        public static RemoteClientPrivateProperties GetRemoteClientPrivateProperties(this HttpRequestMessage request)
            {
            var tmp = request.Properties[RemoteClientPrivatePropertiesName];
            if (tmp is RemoteClientPrivateProperties rcrp)
                {
                return rcrp;
                }
            return null;
            }

        /// <summary>
        /// Установить приватное свойство
        /// </summary>
        /// <param name="request">Запрос</param>
        /// <param name="property">Значение свойства</param>
        /// <returns></returns>
        public static void SetRemoteClientPrivateProperties(this HttpRequestMessage request, RemoteClientPrivateProperties property)
            {
            request.Properties[RemoteClientPrivatePropertiesName] = property;
            }

        #endregion Приватное свойство добавляемое в каждый запрос

        #region Дамп заголовков

        /// <summary>
        /// Возвращает набор заголовков в виде пар ключ - набор значений
        /// </summary>
        /// <param name="request">Запрос</param>
        /// <returns></returns>
        public static IEnumerable<Tuple<string, string>> GetHeaders(this HttpRequestMessage request)
            {
            var enumpairs = HttpHelpers.GetHeaders(request.Headers);
            foreach (var pair in enumpairs)
                {
                yield return pair;
                }
            }

        /// <summary>
        /// Возвращает набор заголовков в виде строки
        /// </summary>
        /// <param name="request">Запрос</param>
        /// <returns></returns>
        public static string GetHeadersAsString(this HttpRequestMessage request)
            {
            return HttpHelpers.GetHeadersAsString(request.Headers);
            }

        #endregion Дамп заголовков

        #region Дамп свойств

        /// <summary>
        /// Возвращает набор свойств в виде пар ключ - набор значений
        /// </summary>
        /// <param name="request">Запрос</param>
        /// <returns></returns>
        public static IEnumerable<Tuple<string, string>> GetProperties(this HttpRequestMessage request)
            {
            if (request.Properties == null)
                {
                yield break;
                }

            var _arr = request.Properties.ToArray();
            var filtered = _arr.Where(x => !x.Key.Equals(RemoteClientPrivatePropertiesName)).ToArray();

            foreach (var prop in filtered)
                {
                var key = prop.Key;
                var value = prop.Value.ToString();
                yield return new Tuple<string, string>(key, value);
                }
            }

        /// <summary>
        /// Возвращает набор свойств в виде строки
        /// </summary>
        /// <param name="request">Запрос</param>
        /// <returns></returns>
        public static string GetPropertiesAsString(this HttpRequestMessage request)
            {
            var sb = new StringBuilder();

            var _arr = request.Properties.ToArray();
            var filtered = _arr.Where(x => !x.Key.Equals(RemoteClientPrivatePropertiesName)).ToArray();

            sb.Append($"Properties ({filtered.Length}):");

            foreach (var kv in filtered)
                {
                sb.Append(kv.Key);
                sb.Append(": ");
                sb.Append(kv.Value.ToString());
                sb.Append(System.Environment.NewLine);
                }
            if (filtered.Length == 0)
                {
                sb.Append(System.Environment.NewLine);
                }
            return sb.ToString();
            }

        #endregion Дамп свойств

        /// <summary>
        /// Получить отправляемое содержимое как строку
        /// </summary>
        /// <param name="request">Запрос</param>
        /// <returns></returns>
        public static async Task<string> GetContentAsStringAsync(this HttpRequestMessage request)
            {
            var sb = new StringBuilder();
            if (request.Content == null)
                {
                sb.Append("Body: no, Length = 0");
                sb.Append(System.Environment.NewLine);
                return sb.ToString();
                }
            if (request.Content is StringContent c)
                {
                var str = await c.ReadAsStringAsync().ConfigureAwait(false);
                sb.Append($"Body: yes, Length = {str.Length}");
                sb.Append(System.Environment.NewLine);
                sb.Append("StringContent:");
                sb.Append(System.Environment.NewLine);
                sb.Append(str);
                sb.Append(System.Environment.NewLine);
                }
            return sb.ToString();
            }

        /// <summary>
        /// Вывести содержимое запроса в строку
        /// </summary>
        /// <param name="request">Запрос</param>
        /// <returns></returns>
        public static async Task<string> DumpRequestToStringAsync(this HttpRequestMessage request)
            {
            var sb = new StringBuilder();

            sb.Append(System.Environment.NewLine);
            sb.Append(HttpHelpers.separator);
            sb.Append($"REQUEST: {request.Method.Method} '{request.RequestUri.ToString()}'");
            sb.Append(System.Environment.NewLine);

            sb.Append($"Method: {request.Method.Method}");
            sb.Append(System.Environment.NewLine);

            sb.Append($"Uri: {request.RequestUri.ToString()}");
            sb.Append(System.Environment.NewLine);

            sb.Append($"Version: {request.Version.ToString()}");
            sb.Append(System.Environment.NewLine);
            sb.Append(HttpHelpers.separator);

            var headers = GetHeadersAsString(request);
            sb.Append(headers);
            sb.Append(HttpHelpers.separator);

            var properties = GetPropertiesAsString(request);
            sb.Append(properties);
            sb.Append(HttpHelpers.separator);

            var bodyContent = await GetContentAsStringAsync(request).ConfigureAwait(false);
            sb.Append(bodyContent);
            sb.Append(HttpHelpers.separator);

            return sb.ToString();
            }
        }
    }