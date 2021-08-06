using System;
using System.Diagnostics;
using System.Threading;

namespace RemoteHttpClient.Http.Performance
	{
	/// <summary>
	/// Точный счетчик производительности
	/// </summary>
	public sealed class HighPerformanceCounter
		{
		#region Константы

		/// <summary>
		/// определяет количество DateTime тактов в 1 миллисекунду
		/// </summary>
		private const long TicksPerMillisecond = 10000;

		/// <summary>
		/// определяет количество DateTime тактов в 1 секунду
		/// </summary>
		private const long TicksPerSecond = TicksPerMillisecond * 1000;

		#endregion Константы

		/// <summary>
		/// Счетчик
		/// </summary>
		private readonly Stopwatch _stopWatch = new Stopwatch();

		/// <summary>
		/// Локер
		/// </summary>
		private readonly object Locker = new object();

		/// <summary>
		/// Имя группы счетчика
		/// </summary>
		private string m_CounterGroupName;

		/// <summary>
		/// Время начала работы
		/// </summary>
		private long _startTicks;

		/// <summary>
		/// Время конца работы
		/// </summary>
		private long _stopTicks;

		/// <summary>
		/// Общее количество вызовов
		/// </summary>
		private int _CallCount;

		#region Конструкторы

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="counterName">Имя счетчика</param>
		public HighPerformanceCounter(string counterName) : this(counterName, null)
			{
			}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="counterName">Имя счетчика</param>
		/// <param name="counterGroupName">Имя группы счетчика</param>
		public HighPerformanceCounter(string counterName, string counterGroupName) : this()
			{
			CounterName = counterName;
			CounterGroupName = counterGroupName;
			}

		/// <summary>
		/// Конструктор
		/// </summary>
		public HighPerformanceCounter()
			{
			_startTicks = 0;
			_stopTicks = 0;
			_CallCount = 0;
			Frequency = Stopwatch.Frequency;
			}

		#endregion Конструкторы

		#region Свойства

		/// <summary>
		/// Таймер высокого разрешения
		/// </summary>
		public static bool IsHighResolution
			{
			get
				{
				return Stopwatch.IsHighResolution;
				}
			}

		/// <summary>
		/// Точность счетчика
		/// </summary>
		public static long PerformanceFrequency
			{
			get
				{
				return Stopwatch.Frequency;
				}
			}

		/// <summary>
		/// Имя счетчика
		/// </summary>
		public string CounterName
			{
			get;
			private set;
			}

		/// <summary>
		/// Имя группы счетчика
		/// </summary>
		public string CounterGroupName
			{
			get
				{
				return m_CounterGroupName;
				}

			set
				{
				if (string.IsNullOrEmpty(m_CounterGroupName))
					{
					m_CounterGroupName = value;
					return;
					}
				throw new InvalidOperationException($"CounterGroupName уже присвоено: {CounterGroupName}");
				}
			}

		/// <summary>
		/// Счетчик запущен
		/// </summary>
		public bool Started
			{
			get;
			private set;
			}

		/// <summary>
		/// Общее количество вызовов
		/// </summary>
		public int CallCount
			{
			get
				{
				return _CallCount;
				}
			}

		/// <summary>
		/// Общее время работы
		/// </summary>
		public long TotalTicks
			{
			get;
			private set;
			}

		/// <summary>
		/// Точность счетчика
		/// </summary>
		public long Frequency
			{
			get;
			}

		/// <summary>
		/// Общее время работы в виде интервала времени
		/// </summary>
		/// <returns>Общее время работы в виде интервала времени</returns>
		public TimeSpan ElapsedTime
			{
			get
				{
				return ComputeElapsedTime(TotalTicks, Frequency);
				}
			}

		/// <summary>
		/// Среднее время работы = Общее время работы / количество вызовов
		/// </summary>
		/// <returns>Общее время работы в виде интервала времени</returns>
		public TimeSpan AvgTime
			{
			get
				{
				return ComputeAvgTime(TotalTicks, Frequency, CallCount);
				}
			}

		#endregion Свойства

		/// <summary>
		/// Посчитать среднее время выполнения
		/// </summary>
		/// <param name="TotalTicks">Количество тиков</param>
		/// <param name="Freq">Частота счетчика</param>
		/// <param name="CallCount">Общее количество вызовов</param>
		/// <returns></returns>
		public static TimeSpan ComputeAvgTime(long TotalTicks, long Freq, int CallCount)
			{
			if (CallCount == 0)
				{
				return TimeSpan.Zero;
				}

			if (TotalTicks == 0)
				{
				return TimeSpan.Zero;
				}

			if (Freq == 0)
				{
				return TimeSpan.Zero;
				}

			var Tns = (TotalTicks * TicksPerSecond) / (Freq * CallCount); // в 100 нс
			var ts = TimeSpan.FromTicks(Tns);
			return ts;
			}

		/// <summary>
		/// Посчитать общее время выполнения
		/// </summary>
		/// <param name="TotalTicks">Количество тиков</param>
		/// <param name="Freq">Частота счетчика</param>
		/// <returns></returns>
		public static TimeSpan ComputeElapsedTime(long TotalTicks, long Freq)
			{
			if (TotalTicks == 0)
				{
				return TimeSpan.Zero;
				}

			if (Freq == 0)
				{
				return TimeSpan.Zero;
				}

			var Tns = (TotalTicks * TicksPerSecond) / Freq; // в 100 нс
			var ts = TimeSpan.FromTicks(Tns);
			return ts;
			}

		/// <summary>
		/// Запустить таймер
		/// </summary>
		public void Start()
			{
			lock (Locker)
				{
				if (Started)
					{
					throw new InvalidOperationException($"Счетчик '{CounterName}' уже запущен");
					}

				_startTicks = _stopWatch.ElapsedTicks;
				_stopWatch.Start();
				Started = true;
				}
			}

		/// <summary>
		/// Остановить таймер
		/// </summary>
		public void Stop()
			{
			lock (Locker)
				{
				if (!Started)
					{
					throw new InvalidOperationException($"Счетчик '{CounterName}' не был запущен");
					}

				_stopWatch.Stop();

				_stopTicks = _stopWatch.ElapsedTicks;

				var time = _stopTicks - _startTicks;
				Interlocked.Increment(ref _CallCount);
				TotalTicks += time;
				Started = false;
				}
			}

		/// <summary>
		/// Время последнего выполнения
		/// </summary>
		public TimeSpan LastTimeSpanInterval
			{
			get
				{
				lock (Locker)
					{
					if (_stopTicks == 0 || _startTicks == 0)
						{
						return TimeSpan.Zero;
						}
					var time = _stopTicks - _startTicks;
					return ComputeElapsedTime(time, Frequency);
					}
				}
			}

		#region Перегруженные методы

		/// <summary>
		/// ToString
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			{
			var ts = ElapsedTime;
			var s = $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds:D3}";
			return s;
			}

		#endregion Перегруженные методы
		}
	}

// https://habrahabr.ru/post/226279/