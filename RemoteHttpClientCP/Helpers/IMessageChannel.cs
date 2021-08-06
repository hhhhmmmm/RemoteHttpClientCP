using System;
using System.Text;

namespace RemoteHttpClient.Helpers
    {
    /// <summary>
    /// Интерфейс - канал сообщений
    /// </summary>
    public interface IMessageChannel
        {
        /// <summary>
        /// Вывести сообщение
        /// </summary>
        /// <param name="messageText">Текст сообщения</param>
        void RaiseMessage(string messageText);

        /// <summary>
        /// Вывести предупреждение
        /// </summary>
        /// <param name="warningText">Текст сообщения</param>
        void RaiseWarning(string warningText);

        /// <summary>
        /// Вывести сообщение об ошибке
        /// </summary>
        /// <param name="errorText">Текст сообщения об ошибке</param>
        void RaiseError(string errorText);
        }
    }