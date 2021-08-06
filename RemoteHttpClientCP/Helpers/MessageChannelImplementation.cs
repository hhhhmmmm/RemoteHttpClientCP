using System;
using System.Text;

namespace RemoteHttpClient.Helpers
    {
    /// <summary>
    /// Вспомогательная реализация интерфейса IMessageChannel
    /// </summary>
    public sealed class MessageChannelImplementation : IMessageChannel, ILogChannel
		{
        #region Реализация интерфейса IMessageChannel

        /// <summary>
        /// Вывести сообщение
        /// </summary>
        /// <param name="messageText">Текст сообщения</param>
        public void RaiseMessage(string messageText)
            {
			RemoteHttpClientGlobals.RaiseMessage(messageText);
            }

        /// <summary>
        /// Вывести предупреждение
        /// </summary>
        /// <param name="warningText">Текст сообщения</param>
        public void RaiseWarning(string warningText)
            {
			RemoteHttpClientGlobals.RaiseWarning(warningText);
            }

        /// <summary>
        /// Вывести сообщение об ошибке
        /// </summary>
        /// <param name="errorText">Текст сообщения об ошибке</param>
        public void RaiseError(string errorText)
            {
			RemoteHttpClientGlobals.RaiseError(errorText);
            }

		/// <summary>
		/// Вывести сообщение об ошибке
		/// </summary>
		/// <param name="errorMessage">Текст сообщения об ошибке</param>
		public void LogError(string errorMessage)
			{
			RemoteHttpClientGlobals.LogError(errorMessage);
			}

		/// <summary>
		/// Вывести сообщение-предупреждение
		/// </summary>
		/// <param name="warningMessage">Текст сообщения-предупреждения</param>
		public void LogWarning(string warningMessage)
			{
			RemoteHttpClientGlobals.LogWarning(warningMessage);
			}

		/// <summary>
		/// Вывести информационное сообщение
		/// </summary>
		/// <param name="infoMessage">Текст информационного сообщения</param>
		public void LogInfo(string infoMessage)
			{
			RemoteHttpClientGlobals.LogInfo(infoMessage);
			}

		/// <summary>
		/// Вывести отладочное сообщение
		/// </summary>
		/// <param name="debugMessage">Текст отладочного сообщения</param>
		public void LogDebug(string debugMessage)
			{
			RemoteHttpClientGlobals.LogDebug(debugMessage);
			}

		/// <summary>
		/// Вывести сообщение об исключении
		/// </summary>
		/// <param name="e">Исключение</param>
		/// <param name="text">Текст примечания</param>
		public void LogException(Exception e, string text)
			{
			RemoteHttpClientGlobals.LogException(e,text);
			}

		/// <summary>
		/// Вывести сообщение об агрегированном исключении
		/// </summary>
		/// <param name="ae">Агрегированное исключение</param>
		/// <param name="text">Текст примечания</param>
		public void LogAggregateException(AggregateException ae, string text)
			{
			RemoteHttpClientGlobals.LogException(ae, text);
			}

		#endregion Реализация интерфейса IMessageChannel
		}
    }