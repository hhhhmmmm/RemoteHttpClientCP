using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using RemoteHttpClient.Helpers;

namespace RemoteHttpClient.Config
	{
	/// <summary>
	/// Класс для чтения настроек конфигурации библиотеки OfdProxy
	/// </summary>
	public class ConfigFileSettingsBase
		{
		/// <summary>
		/// Канал сообщений
		/// </summary>
		private IMessageChannel m_messageChannel;

		/// <summary>
		/// Имя файла конфигурации сборки, например OfdProxy.dll.config
		/// </summary>
		private string m_AssemblyConfigFileName;

		/// <summary>
		/// Полное имя файла конфигурации сборки, например D:\ГИС ЖКХ\CSharp\GosUslugi\GosUslugiApp\bin\Debug\OfdProxy.dll.config
		/// </summary>
		private string m_FullAssemblyConfigFileName;

		/// <summary>
		/// Имя файла сборки, например OfdProxy.dll
		/// </summary>
		private string m_AssemblyFileName;

		/// <summary>
		/// Путь к файлу сборки, например D:\ГИС ЖКХ\CSharp\GosUslugi\GosUslugiApp\bin\Debug
		/// </summary>
		private string m_AssemblyDirectoryName;

		/// <summary>
		/// Конфигурация прочитанная из файла конфигурации
		/// </summary>
		private Configuration m_Configuration;

		/// <summary>
		/// Секция  &lt;appSettings&gt; файла конфигурации
		/// </summary>
		private AppSettingsSection m_AppSettings;

		#region Конструкторы

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="messageChannel">канал сообщений</param>
		public ConfigFileSettingsBase(IMessageChannel messageChannel)
			{
			m_messageChannel = messageChannel;
			GetAssemblyNames();
			}

		#endregion Конструкторы

		#region Вспомогательные функции

		/// <summary>
		/// Разобрать названия частей сборки
		/// </summary>
		private void GetAssemblyNames()
			{
			var ExecutingAssembly = Assembly.GetExecutingAssembly();
			if (ExecutingAssembly == null)
				{
				throw new InvalidOperationException("Ошибка определения того, где мы находимся");
				}

			var AssemblyLocation = new Uri(ExecutingAssembly.GetName().CodeBase).LocalPath;
			// string AssemblyLocation = ExecutingAssembly.Location;

			m_AssemblyFileName = Path.GetFileName(AssemblyLocation);
			m_AssemblyDirectoryName = Path.GetDirectoryName(AssemblyLocation);

			if (string.IsNullOrEmpty(m_AssemblyDirectoryName))
				{
				throw new InvalidOperationException("Ошибка определения m_AssemblyDirectoryName");
				}

			m_AssemblyConfigFileName = m_AssemblyFileName + ".config";
			if (string.IsNullOrEmpty(m_AssemblyConfigFileName))
				{
				throw new InvalidOperationException();
				}

			m_FullAssemblyConfigFileName = Path.Combine(m_AssemblyDirectoryName, m_AssemblyConfigFileName);
			}

		/// <summary>
		/// Показать Message с ошибкой
		/// </summary>
		/// <param name="Text">Текст сообщения</param>
		internal void RaiseError(string Text)
			{
			m_messageChannel?.RaiseError(Text);
			}

		/// <summary>
		/// Прочитать значение типа bool из настроек конфигурации
		/// </summary>
		/// <param name="key">Имя параметра в файле конфигурации</param>
		/// <returns>true если параметр найден и его значение равно 'true','yes','да' или '1'</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public bool GetAppConfigBool(string key)
			{
			string s;

			s = GetAppConfigString(key);

			if (string.IsNullOrEmpty(s))
				{
				return false;
				}

			if (
				s.Equals("true", StringComparison.OrdinalIgnoreCase) ||
				s.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
				s.Equals("да", StringComparison.OrdinalIgnoreCase) ||
				s.Equals("1", StringComparison.OrdinalIgnoreCase)
				)
				{
				return true;
				}

			return false;
			}

		/// <summary>
		/// Прочитать значение типа bool из настроек конфигурации.
		/// Если не указано (или не запрещено), то по умолчанию считать как true
		/// </summary>
		/// <param name="key">Имя параметра в файле конфигурации</param>
		/// <returns>true если значение не равно 'false','no','нет' или '0'</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public bool GetAppConfigBoolDefaultTrue(string key)
			{
			string s;

			s = GetAppConfigString(key);

			if (string.IsNullOrEmpty(s))
				{
				return true;
				}

			if (
				s.Equals("false", StringComparison.OrdinalIgnoreCase) ||
				s.Equals("no", StringComparison.OrdinalIgnoreCase) ||
				s.Equals("нет", StringComparison.OrdinalIgnoreCase) ||
				s.Equals("0", StringComparison.OrdinalIgnoreCase)
				)
				{
				return false;
				}

			return true;
			}

		/// <summary>
		/// Прочитать значение типа int из настроек конфигурации
		/// </summary>
		/// <param name="key">Имя параметра в файле конфигурации</param>
		/// <param name="Value">Результат</param>
		/// <returns>true если параметр найден и его значение удалось преобразовать в int</returns>
		public bool GetAppConfigInt(string key, out int Value)
			{
			string s;
			Value = 0;

			s = GetAppConfigString(key);

			if (string.IsNullOrEmpty(s))
				{
				return false;
				}

			int result;
			if (int.TryParse(s, out result))
				{
				Value = int.Parse(s, CultureInfo.CurrentCulture);
				return true;
				}

			return false;
			}

		/// <summary>
		/// Прочитать значение типа string из настроек конфигурации
		/// </summary>
		/// <param name="key">Имя параметра в файле конфигурации</param>
		/// <returns>строка если параметр найден или пустая строка если не найден</returns>
		public string GetAppConfigString(string key)
			{
			string s;

			if (AppSettings == null)
				{
				return null;
				// throw new InvalidOperationException("Ошибка открытия файла конфигурации - нет секции <appSettings>");
				}

			var kv = AppSettings.Settings;

			if (kv == null)
				{
				return string.Empty;
				}

			if (kv.Count == 0)
				{
				return string.Empty;
				}

			var bFound = false;

			for (int i = 0; i < kv.AllKeys.Length; i++)
				{
				if (kv.AllKeys[i] == key)
					{
					bFound = true;
					break;
					}
				}

			if (!bFound)
				{
				return string.Empty;
				}

			s = kv[key].Value;

			if (s == null)
				{
				s = string.Empty;
				}

			return s;
			}

		/// <summary>
		/// Разобрать файл конфигурации
		/// </summary>
		/// <returns>true если все хорошо</returns>
		private bool LoadConfigFile()
			{
			var ConfigurationFileMap = new ExeConfigurationFileMap();
			ConfigurationFileMap.ExeConfigFilename = FullAssemblyConfigFileName; //
																				 //AssemblyConfigFileName;

			m_Configuration = ConfigurationManager.OpenMappedExeConfiguration(ConfigurationFileMap, ConfigurationUserLevel.None);
			if (m_Configuration == null)
				{
				var sb = new StringBuilder();
				sb.AppendFormat("Ошибка открытия файла конфигурации {0}", FullAssemblyConfigFileName);
				RaiseError(sb.ToString());
				return false;
				}

			m_AppSettings = m_Configuration.AppSettings;
			if (m_AppSettings == null)
				{
				var sb = new StringBuilder();
				sb.AppendFormat("Ошибка открытия файла конфигурации {0} - нет секции <appSettings>", FullAssemblyConfigFileName);
				RaiseError(sb.ToString());
				return false;
				}
			return true;
			}

		#endregion Вспомогательные функции

		/// <summary>
		/// Проверить, что файл существует, без учета регистра названия файла
		/// </summary>
		/// <param name="fileName">Название файла</param>
		/// <returns></returns>
		public static bool FileExistsCaseInsensitive(string fileName)
			{
			var directory = Path.GetDirectoryName(fileName);
			if (string.IsNullOrEmpty(directory))
				{
				return false;
				}

			var upperFileName = Path.GetFileName(fileName)?.ToUpper();

			foreach (string filePath in Directory.GetFiles(directory))
				{
				if (Path.GetFileName(filePath).ToUpper() == upperFileName)
					{
					return true;
					}
				}

			return false;
			}

		/// <summary>
		/// Файл конфигурации инициализирован
		/// </summary>
		public bool IsInitialized
			{
			get;
			private set;
			}

		/// <summary>
		/// Загрузить файл конфигурации
		/// </summary>
		/// <returns>true если файл конфигурации существует и успешно загружен</returns>
		public bool Init()
			{
			if (IsInitialized)
				{
				return true;
				}
			bool bres;

			//			if (!FileExistsCaseInsensitive(FullAssemblyConfigFileName))
			if (!File.Exists(FullAssemblyConfigFileName))
				{
				var sb = new StringBuilder();
				sb.AppendFormat("Файл {0} не найден", FullAssemblyConfigFileName);
				RaiseError(sb.ToString());
				return false;
				}

			bres = LoadConfigFile();
			if (bres)
				{
				IsInitialized = true;
				}
			return bres;
			}

		#region Свойства

		/// <summary>
		/// Конфигурация прочитанная из файла конфигурации
		/// </summary>
		public Configuration Configuration
			{
			get
				{
				return m_Configuration;
				}
			}

		/// <summary>
		/// Секция  appSettings файла конфигурации
		/// </summary>
		public AppSettingsSection AppSettings
			{
			get
				{
				return m_AppSettings;
				}
			}

		/// <summary>
		/// Имя файла конфигурации библиотеки
		/// </summary>
		public string AssemblyConfigFileName
			{
			get
				{
				return m_AssemblyConfigFileName;
				}
			}

		/// <summary>
		/// Имя файла сборки
		/// </summary>
		public string AssemblyFileName
			{
			get
				{
				return m_AssemblyFileName;
				}
			}

		/// <summary>
		/// Полное имя файла конфигурации сборки
		/// </summary>
		public string FullAssemblyConfigFileName
			{
			get
				{
				return m_FullAssemblyConfigFileName;
				}
			}

		/// <summary>
		/// Путь к файлу сборки
		/// </summary>
		public string AssemblyDirectoryName
			{
			get
				{
				return m_AssemblyDirectoryName;
				}
			}

		#endregion Свойства
		}
	}