using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

using NUnit.Framework;

using RemoteHttpClient.Http;

namespace RemoteHttpClient.Unit.Test
    {
    [TestFixture]
    [Description("Тесты прокси")]
    [Category("Тесты прокси")]
    public class ProxyTests : MessageHandlerBase
        {
        /// <summary>
        /// Инициализация
        /// </summary>
        [SetUp]
        public void Init()
            {
            base.ClientFactory = RemoteClientFactory.GetInstance();
            Assert.IsNotNull(ClientFactory);
            }

        [Test]
        [TestCase(TestName = "30. ProxyTest - настройки читаются")]
        public void IStringRccImplementationTest()
            {
            var proxySettings = RemoteHttpClientGlobals.DllConfiguration.ProxySettings;
            Assert.IsNotNull(proxySettings);
            }

        [Test]
        /// <param name="useProxy">Использовать прокси</param>
        /// <param name="scheme"></param>
        /// <param name="serverIPAddress">IP адрес прокси-сервера</param>
        /// <param name="serverPort">Порт прокси-сервера</param>
        /// <param name="useCustomNetworkCredential">Использовать имя пользователя и пароль </param>
        /// <param name="networkCredentialUserName">Имя пользователя прокси</param>
        /// <param name="networkCredentialPassword">Пароль пользователя прокси</param>
        /// <param name="networkCredentialDomain">Домен пользователя прокси</param>
        /// <param name="bypassProxyOnLocal">Не использовать прокси для локальных адресов</param>
        /// <param name="bypassList">Список адресов для которых прокси не используется</param>
        /// <param name="wantedStatusCode">Ожидаемый код ответа</param>
        [TestCase(true, "192.168.200.1", 3128, true, "tecmint", "12345", "", false, null, HttpStatusCode.OK, TestName = "ProxyTest standalone - Вызов GET 192.168.200.1")]
        [TestCase(true, "192.168.200.1", 3128, true, "tecmint", "123456", "", false, null, HttpStatusCode.ProxyAuthenticationRequired, TestName = "ProxyTest standalone - Вызов GET 192.168.200.1 неверный пароль")]
        [TestCase(true, "192.0.2.13", 3128, true, "tecmint", "12345", "", false, null, HttpStatusCode.OK, TestName = "ProxyTest standalone - Вызов GET 192.0.2.13")]
        [TestCase(false, "192.0.2.13", 3128, true, "tecmint", "12345", "", false, null, HttpStatusCode.OK, TestName = "ProxyTest standalone - Вызов GET 192.0.2.13 без прокси")]
        [TestCase(true, "173.249.35.163", 10010, false, "", "", "", false, null, HttpStatusCode.OK, TestName = "ProxyTest standalone - 173.249.35.163 10010")]
        [TestCase(true, "51.158.98.121", 8811, false, "", "", "", false, null, HttpStatusCode.OK, TestName = "ProxyTest standalone - 51.158.98.121")]
        [TestCase(true, "173.249.35.163", 1448, false, "", "", "", false, null, HttpStatusCode.OK, TestName = "ProxyTest standalone - 173.249.35.163 1448")]
        [TestCase(true, "182.52.31.58", 8080, false, "", "", "", false, null, HttpStatusCode.OK, TestName = "ProxyTest standalone - 182.52.31.58")]
        [TestCase(true, "157.230.45.141", 8080, false, "", "", "", false, null, HttpStatusCode.OK, TestName = "ProxyTest standalone - 157.230.45.141")]
        [TestCase(true, "45.248.94.60", 40886, false, "elite", "proxy", "", false, null, HttpStatusCode.OK, TestName = "ProxyTest standalone - 45.248.94.60")]
        [Category("ProxyTest standalone")]
        public async Task GetUsingProxyGoogleStandalone
            (
            bool useProxy,
            string serverIPAddress,
            int serverPort,
            bool useCustomNetworkCredential,
            string networkCredentialUserName,
            string networkCredentialPassword,
            string networkCredentialDomain,
            bool bypassProxyOnLocal,
            string[] bypassList,
            int wantedStatusCode
            )
            {
            // https://www.sslproxies.org/
            RemoteClient remoteclient = null;
            ProxyTestIrccImplementation consumer = null;
            try
                {
                const string url = "http://www.google.com";
                consumer = new ProxyTestIrccImplementation(url);

                var ps = HttpProxySettings.Create(
                    useProxy,
                    serverIPAddress,
                    serverPort,
                    useCustomNetworkCredential,
                    networkCredentialUserName,
                    networkCredentialPassword,
                    networkCredentialDomain,
                    bypassProxyOnLocal,
                    bypassList
                    );

                var iwebproxy = ps.CreateWebProxy();

                remoteclient = ClientFactory.GetStandaloneInstance(url, iwebproxy);
                var bres = remoteclient.Initialize();
                Assert.IsTrue(bres);

                var uid = consumer as IRemoteClientDataUid;
                var result = await remoteclient.GetAsync(consumer, uid, WantedResponseType.String, null);
                Assert.IsTrue(result);

                Assert.AreEqual(wantedStatusCode, (int)consumer.StatusCode);
                Debug.WriteLine(consumer.StringResult);
                }
            finally
                {
                remoteclient?.Dispose();
                consumer?.Dispose();
                }
            }

        [Test]
        /// <param name="useProxy">Использовать прокси</param>
        /// <param name="scheme"></param>
        /// <param name="serverIPAddress">IP адрес прокси-сервера</param>
        /// <param name="serverPort">Порт прокси-сервера</param>
        /// <param name="useCustomNetworkCredential">Использовать имя пользователя и пароль </param>
        /// <param name="networkCredentialUserName">Имя пользователя прокси</param>
        /// <param name="networkCredentialPassword">Пароль пользователя прокси</param>
        /// <param name="networkCredentialDomain">Домен пользователя прокси</param>
        /// <param name="bypassProxyOnLocal">Не использовать прокси для локальных адресов</param>
        /// <param name="bypassList">Список адресов для которых прокси не используется</param>
        /// <param name="wantedStatusCode">Ожидаемый код ответа</param>
        [Category("ProxyTest с прокси")]
        [TestCase(true, "192.168.200.1", 3128, true, "tecmint", "12345", "", false, null, HttpStatusCode.OK, TestName = "ProxyTest - Вызов GET 192.168.200.1")]
        [TestCase(true, "192.168.200.1", 3128, true, "tecmint", "123456", "", false, null, HttpStatusCode.ProxyAuthenticationRequired, TestName = "ProxyTest - Вызов GET 192.168.200.1 неверный пароль")]
        [TestCase(true, "192.0.2.13", 3128, true, "tecmint", "12345", "", false, null, HttpStatusCode.OK, TestName = "ProxyTest - Вызов GET 192.0.2.13")]
        [TestCase(false, "192.0.2.13", 3128, true, "tecmint", "12345", "", false, null, HttpStatusCode.OK, TestName = "ProxyTest - Вызов GET 192.0.2.13 без прокси")]
        [TestCase(true, "173.249.35.163", 10010, false, "", "", "", false, null, HttpStatusCode.OK, TestName = "ProxyTest - 173.249.35.163 10010")]
        [TestCase(true, "51.158.98.121", 8811, false, "", "", "", false, null, HttpStatusCode.OK, TestName = "ProxyTest - 51.158.98.121")]
        [TestCase(true, "173.249.35.163", 1448, false, "", "", "", false, null, HttpStatusCode.OK, TestName = "ProxyTest - 173.249.35.163 1448")]
        [TestCase(true, "182.52.31.58", 8080, false, "", "", "", false, null, HttpStatusCode.OK, TestName = "ProxyTest - 182.52.31.58")]
        [TestCase(true, "157.230.45.141", 8080, false, "", "", "", false, null, HttpStatusCode.OK, TestName = "ProxyTest - 157.230.45.141")]
        [TestCase(true, "45.248.94.60", 40886, false, "elite", "proxy", "", false, null, HttpStatusCode.OK, TestName = "ProxyTest - 45.248.94.60")]
        public async Task GetUsingProxyGoogleCache
            (
            bool useProxy,
            string serverIPAddress,
            int serverPort,
            bool useCustomNetworkCredential,
            string networkCredentialUserName,
            string networkCredentialPassword,
            string networkCredentialDomain,
            bool bypassProxyOnLocal,
            string[] bypassList,
            int wantedStatusCode
            )
            {
            // https://www.sslproxies.org/
            RemoteClient remoteclient = null;
            ProxyTestIrccImplementation consumer = null;
            try
                {
                const string url = "http://www.google.com";
                consumer = new ProxyTestIrccImplementation(url);

                var ps = HttpProxySettings.Create(
                    useProxy,
                    serverIPAddress,
                    serverPort,
                    useCustomNetworkCredential,
                    networkCredentialUserName,
                    networkCredentialPassword,
                    networkCredentialDomain,
                    bypassProxyOnLocal,
                    bypassList
                    );

                var iwebproxy = ps.CreateWebProxy();

                Assert.IsNotNull(ClientFactory);
                remoteclient = ClientFactory.GetInstance(url, iwebproxy);
                var bres = remoteclient.Initialize();
                Assert.IsTrue(bres);

                var uid = consumer as IRemoteClientDataUid;
                var result = await remoteclient.GetAsync(consumer, uid, WantedResponseType.String, null);
                Assert.IsTrue(result);

                Assert.AreEqual(wantedStatusCode, (int)consumer.StatusCode);
                Debug.WriteLine(consumer.StringResult);
                }
            finally
                {
                remoteclient?.Dispose();
                consumer?.Dispose();
                }
            }

        [Test]
        [TestCase(TestName = "ProxyTest - разные экземпляры с прокси и без него")]
        public void GetUsingProxyGoogleCache()
            {
            // https://www.sslproxies.org/
            RemoteClient remoteclient11 = null;
            RemoteClient remoteclient12 = null;

            RemoteClient remoteclient2 = null;

            try
                {
                bool bres;
                const string url = "http://www.google.com";

                var iwebproxy = RemoteHttpClientGlobals.HttpProxy;
                Assert.IsNotNull(iwebproxy, "Прокси отключен в файле конфигурации");
                Assert.IsNotNull(ClientFactory);

                remoteclient11 = ClientFactory.GetInstance(url, iwebproxy);
                bres = remoteclient11.Initialize();
                Assert.IsTrue(bres);

                remoteclient2 = ClientFactory.GetInstance(url, null);
                bres = remoteclient2.Initialize();
                Assert.IsTrue(bres);

                Assert.AreNotEqual(remoteclient11, remoteclient2);

                remoteclient12 = ClientFactory.GetInstance(url, iwebproxy);
                bres = remoteclient12.Initialize();
                Assert.IsTrue(bres);

                Assert.AreEqual(remoteclient11, remoteclient12);
                }
            finally
                {
                remoteclient11?.Dispose();
                remoteclient12?.Dispose();
                remoteclient2?.Dispose();
                }
            }
        }
    }