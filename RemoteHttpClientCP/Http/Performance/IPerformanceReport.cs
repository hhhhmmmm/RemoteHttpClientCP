using System;
using System.Data;
using System.Text;

namespace RemoteHttpClient.Http.Performance
    {
    /// <summary>
    /// Интерфейс с данными о производительности
    /// </summary>
    public interface IPerformanceData
        {
        /// <summary>
        /// Таблица с данными о производительности
        /// </summary>
        DataTable PerformanceTable
            {
            get;
            }

        /// <summary>
        /// Таблица производительности в виде одной многострочной строки
        /// </summary>
        string PerformanceTableAsString
            {
            get;
            }

        /// <summary>
        /// Обновить информацию о производительности
        /// </summary>
        void UpdatePerformanceInformation();

        /// <summary>
        /// Обобщенная таблица с данными о производительности
        /// </summary>
        DataTable AggregatedPerformanceTable
            {
            get;
            }

        /// <summary>
        /// Обобщенная таблица производительности в виде одной многострочной строки
        /// </summary>
        string AggregatedPerformanceTableAsString
            {
            get;
            }
        }
    }