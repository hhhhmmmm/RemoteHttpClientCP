using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteHttpClient.Helpers
	{
	/// <summary>
	/// Интерфейс - канал вывода сообщений в лог
	/// </summary>
	public interface ILogChannel
		{
		/// <summary>
		/// Вывести сообщение об ошибке
		/// </summary>
		/// <param name="errorMessage">Текст сообщения об ошибке</param>
		void LogError(string errorMessage);

		/// <summary>
		/// Вывести сообщение-предупреждение
		/// </summary>
		/// <param name="warningMessage">Текст сообщения-предупреждения</param>
		void LogWarning(string warningMessage);

		/// <summary>
		/// Вывести информационное сообщение
		/// </summary>
		/// <param name="infoMessage">Текст информационного сообщения</param>
		void LogInfo(string infoMessage);

		/// <summary>
		/// Вывести отладочное сообщение
		/// </summary>
		/// <param name="debugMessage">Текст отладочного сообщения</param>
		void LogDebug(string debugMessage);

		/// <summary>
		/// Вывести сообщение об исключении
		/// </summary>
		/// <param name="e">Исключение</param>
		/// <param name="text">Текст примечания</param>
		void LogException(Exception e, string text);

		/// <summary>
		/// Вывести сообщение об агрегированном исключении
		/// </summary>
		/// <param name="ae">Агрегированное исключение</param>
		/// <param name="text">Текст примечания</param>
		void LogAggregateException(AggregateException ae, string text);
		}
	}
