using System;
using System.Net;
using System.Text;

namespace RemoteHttpClient.Http
    {
    /// <summary>
    /// Вспомогательный интерфейс для работы с куками
    /// </summary>
    public interface ICookieHelper
        {
        #region Свойства

        /// <summary>
        /// Контейнер куков
        /// </summary>
        CookieContainer CookieContainer
            {
            get;
            }

        /// <summary>
        /// Контейнер куков в виде массива
        /// </summary>
        Cookie[] CookiesArray
            {
            get;
            }

        /// <summary>
        /// Кука аутентификации FormsAuthentication
        /// ".ASPXAUTH"
        /// </summary>
        Cookie FormsAuthenticationCookie
            {
            get;
            }

        #endregion Свойства

        #region Методы

        /// <summary>
        /// Установить контейнер с куками
        /// </summary>
        /// <param name="cookieContainer">Контейнер с куками</param>
        void SetCookieContainer(CookieContainer cookieContainer);

        /// <summary>
        /// Создать и установить контейнер для куков
        /// </summary>
        void CreateAndSetCookieContainer();

        /// <summary>
        /// Получить коллекцию всех куков
        /// </summary>
        /// <returns>Коллекция всех куков</returns>
        CookieCollection GetAllCookiesCollection();

        /// <summary>
        /// Добавить куку
        /// </summary>
        /// <param name="name">название куки</param>
        /// <param name="value">значение куки</param>
        void AddCookie(string name, string value);

        /// <summary>
        /// Создать и добавить куку из существующей
        /// Берется только название и значение куки
        /// </summary>
        /// <param name="cookie">кука</param>
        void AddCookieFromExisting(Cookie cookie);

        /// <summary>
        /// Добавить все куки из существующего контейнера куков
        /// </summary>
        /// <param name="cookieContainer">Контейнер куков</param>
        void AddAllCookiesFromCookieContainer(CookieContainer cookieContainer);

        #endregion Методы
        }
    }