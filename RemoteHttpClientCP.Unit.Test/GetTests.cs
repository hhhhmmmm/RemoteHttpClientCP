using System.Net;
using System.Threading.Tasks;

using NUnit.Framework;

using RemoteHttpClient.Http;
using RemoteHttpClient.RemoteClientConsumers;

namespace RemoteHttpClient.Unit.Test
    {
    [TestFixture]
    [Description("Запросы типа GET")]
    [Category("Запросы типа GET")]
    public class GetTests : MessageHandlerBase
        {
        /// <summary>
        /// Инициализация
        /// </summary>
        [SetUp]
        public void Init()
            {
            RemoteClientFactory.CollectPerformanceStatistics = true;
            ClientFactory = RemoteClientFactory.GetInstance();
            Assert.IsNotNull(ClientFactory);
            }

        [Test]
        [TestCase("http://www.google.com", TestName = "20. Вызов GET http://www_google_com")]
        [TestCase("http://www.inary.ru", TestName = "20. Вызов GET http://www_inary_ru")]
        [TestCase("http://www.yandex.ru", TestName = "20. Вызов GET http://www_yandex_ru")]
        [TestCase("http://gkh.inary.ru/index.php/kontakty", TestName = "20. Вызов GET http://gkh_inary_ru/index_php/kontakty")]
        public async Task GetSite(string url)
            {
            RemoteClient remoteclient = null;
            try
                {
                using (var consumer = new ProxyTestIrccImplementation(url))
                    {
                    remoteclient = ClientFactory.GetStandaloneInstance(url, null);
                    Assert.IsNotNull(remoteclient);
                    var bres = remoteclient.Initialize();
                    Assert.IsTrue(bres);
                    var uid = consumer as IRemoteClientDataUid;
                    var result = await remoteclient.GetAsync(consumer, uid, WantedResponseType.String, null);
                    Assert.IsTrue(result);
                    var count = remoteclient.CookieContainer.Count;
                    }
                }
            finally
                {
                remoteclient?.Dispose();
                }
            }

        [Test]
        [TestCase("http://www.google.com", TestName = "25. IrccConsumer  Вызов GET http://www_google_com")]
        [TestCase("http://www.inary.ru", TestName = "25. IrccConsumer Вызов GET http://www_inary_ru")]
        [TestCase("http://www.yandex.ru", TestName = "25. IrccConsumer Вызов GET http://www_yandex_ru")]
        [TestCase("http://gkh.inary.ru/index.php/kontakty", TestName = "25. IrccConsumer Вызов GET http://gkh_inary_ru/index_php/kontakty")]
        public async Task GetSiteIrccConsumer(string url)
            {
            RemoteClient remoteclient = null;
            try
                {
                using (var irccConsumer = new IrccConsumer(url))
                    {
                    var iCookieHelper = irccConsumer as ICookieHelper;
                    Assert.IsNotNull(iCookieHelper.CookieContainer);

                    var result = await irccConsumer.GetAsync();
                    Assert.IsTrue(result);
                    Assert.IsNotNull(irccConsumer.StringResult);
                    Assert.IsTrue(irccConsumer.IsSuccessStatusCode);
                    Assert.AreEqual(irccConsumer.StatusCode, HttpStatusCode.OK);
                    }
                }
            finally
                {
                remoteclient?.Dispose();
                }
            }

        [Test]
        [TestCase("http://downloads.inary.ru/free_downloads/tmp/test38580.bin", true, WantedResponseType.ByteArray, TestName = "30. Вызов GET(ByteArray) для файла test38580.bin")]
        [TestCase("http://downloads.inary.ru/free_downloads/tmp/test38580.bin", true, WantedResponseType.Stream, TestName = "31. Вызов GET(Stream) для файла test38580.bin")]
        [TestCase("http://downloads.inary.ru/free_downloads/tmp/test2304000.bin", true, WantedResponseType.ByteArray, TestName = "32. Вызов GET(ByteArray) для файла test2304000.bin")]
        [TestCase("http://downloads.inary.ru/free_downloads/tmp/test2304000.bin", true, WantedResponseType.Stream, TestName = "33. Вызов GET(Stream) для файла test2304000.bin")]
        [TestCase("http://dd.inary.ru/free_downloads/tmp/test38580.bin", false, WantedResponseType.ByteArray, TestName = "34. -- Ошибочный вызов GET(ByteArray) для файла test38580.bin")]
        public async Task GetFile(string url, bool wantedResult, WantedResponseType wantedResponseType)
            {
            RemoteClient remoteclient = null;
            try
                {
                using (var consumer = new ProxyTestIrccImplementation(url))
                    {
                    remoteclient = ClientFactory.GetStandaloneInstance(url, null);
                    remoteclient.DefaultAccept = string.Empty;
                    Assert.IsNotNull(remoteclient);
                    var bres = remoteclient.Initialize();
                    Assert.IsTrue(bres);
                    var uid = consumer as IRemoteClientDataUid;
                    var result = await remoteclient.GetAsync(consumer, uid, wantedResponseType, null);
                    Assert.AreEqual(wantedResult, result);
                    }
                }
            finally
                {
                remoteclient?.Dispose();
                }
            }
        }
    }