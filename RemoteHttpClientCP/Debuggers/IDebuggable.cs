
namespace RemoteHttpClient.Debuggers
	{
	/// <summary>
	/// Есть способности к взаимодействию с IDebugger
	/// </summary>
	public interface IDebuggable
		{
		/// <summary>
		/// Отладчик присоединен, идет какая-то отладка
		/// </summary>
		bool IsDebugging
			{
			get;
			}

		/// <summary>
		/// Интерфейс отладчика
		/// </summary>
		IDebugger Debugger
			{
			get;
			}

		/// <summary>
		/// Установить интерфейс отладчика
		/// </summary>
		/// <param name="debugger">интерфейс отладчика</param>
		void SetIDebugger(IDebugger debugger);

		/// <summary>
		/// Присоединить отладчик к объекту, если получится
		/// </summary>
		void AttachDebugger(object o);


		}
	}
