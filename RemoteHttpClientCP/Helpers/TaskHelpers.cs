using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteHttpClient.Helpers
    {
    /// <summary>
    /// Вспомогательный класс для задач
    /// </summary>
    public static class TaskHelpers
        {
        /// <summary>
        /// Завершенная задача типа bool
        /// </summary>
        public static readonly Task<bool> CompletedTaskBoolTrue = Task.FromResult<bool>(true);

        /// <summary>
        /// Неуспешно завершенная задача
        /// </summary>
        public static readonly Task<bool> CompletedTaskBoolFalse = Task.FromResult<bool>(false);

		/// <summary>
		/// Concurrently Executes async actions for each item of IEnumerable(T)
		/// </summary>
		/// <typeparam name="T">Type of IEnumerable</typeparam>
		/// <param name="enumerable">instance of IEnumerable(T)</param>
		/// <param name="action">an async Action to execute</param>
		/// <param name="maxDegreeOfParallelism">Optional, An integer that represents the maximum degree of parallelism,
		/// Must be grater than 0</param>
		/// <returns>A Task representing an async operation</returns>
		/// <exception cref="ArgumentOutOfRangeException">If the maxActionsToRunInParallel is less than 1</exception>
		/// <example>
		/// Sample Usage:
		/// await enumerable.ForEachAsyncConcurrent(
		/// async item =>
		///     {
		///     await SomeAsyncMethod(item);
		///     },
		///  5);
		/// </example>
		public static async Task ForEachAsyncConcurrentAsync<T>(
            this IEnumerable<T> enumerable,
            Func<T, Task> action,
            int? maxDegreeOfParallelism = null)
            {
            if (action == null)
                {
                throw new ArgumentNullException(nameof(action));
                }

            if (maxDegreeOfParallelism.HasValue)
                {
                using (var semaphoreSlim = new SemaphoreSlim(
                    maxDegreeOfParallelism.Value, maxDegreeOfParallelism.Value))
                    {
                    var tasksWithThrottler = new List<Task>();

                    foreach (var item in enumerable)
                        {
                        // Increment the number of currently running tasks and wait if they are more than limit.
                        await semaphoreSlim.WaitAsync();

                        tasksWithThrottler.Add(Task.Run(async () =>
                        {
                            await action(item).ContinueWith(res =>
                            {
                                // action is completed, so decrement the number of currently running tasks
                                semaphoreSlim.Release();
                            });
                        }));
                        }

                    // Wait for all tasks to complete.
                    await Task.WhenAll(tasksWithThrottler.ToArray());
                    }
                }
            else
                {
                await Task.WhenAll(enumerable.Select(item => action(item)));
                }
            }

		/// <summary>
		/// Для каждой задачи типа T из списка выполнить действие
		/// расспаллелив выполнение на maxDegreeOfParallelism 
		/// </summary>
		/// <typeparam name="T">Тип</typeparam>
		/// <param name="enumerable">Перечисление</param>
		/// <param name="action">Действие</param>
		/// <returns></returns>
		public static Task ForEachAsyncConcurrentAutoAsync<T>(
                   this IEnumerable<T> enumerable,
                   Func<T, Task> action
                   )
            {
			var maxDegreeOfParallelism = GetMaxDegreeOfParallelism();
			return ForEachAsyncConcurrentAsync<T>(enumerable, action, maxDegreeOfParallelism);
            }

		/// <summary>
		/// Получить макс. кол-во параллельных потоков
		/// </summary>
		/// <returns></returns>
		public static int GetMaxDegreeOfParallelism()
			{
			var maxDegreeOfParallelism = Environment.ProcessorCount;
			//maxDegreeOfParallelism = 200;
			return maxDegreeOfParallelism;
			}

		}
    }