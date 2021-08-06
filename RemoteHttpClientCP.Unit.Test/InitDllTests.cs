using NUnit.Framework;
using RemoteHttpClient;

namespace RemoteHttpClient.Unit.Test
	{
	[TestFixture]
	[Description("Инициализация библиотеки")]
	[Category("Инициализация библиотеки")]
	public class InitDllTests : MessageHandlerBase
		{
		/// <summary>
		/// Инициализация
		/// </summary>
		[SetUp]
		public void Init()
			{
			}

		[Test]
		[TestCase(TestName = "10. Библиотека инициализирована")]
		public void RemoteHttpClientIsInitialized()
			{
			Assert.IsTrue(RemoteHttpClientGlobals.IsInitialized);
			}
		}
	}