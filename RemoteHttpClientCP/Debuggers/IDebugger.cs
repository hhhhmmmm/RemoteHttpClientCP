using System;
using System.Text;
using RemoteHttpClient.Http.Performance;

namespace RemoteHttpClient.Debuggers
    {
    /// <summary>
    /// Отладчик библиотеки OfdProxy
    /// </summary>
    public interface IDebugger
        {
        #region Отладка запросов типа GET

        /// <summary>
        /// Список транзакций для GET
        /// </summary>
        string[] TransactionsListForGet
            {
            get;
            }

        /// <summary>
        /// Отлаживаются запросы типа GET
        /// </summary>
        bool DebugGetRequests
            {
            get;
            }

        #endregion Отладка запросов типа GET

        #region Производительность запросов

        /// <summary>
        /// Добавить строку производительности
        /// </summary>
        /// <param name="performanceData">Данные о производительности запроса</param>
        void ReportHttpPerformance(PerformanceData performanceData);

        #endregion Производительность запросов
        }
    }