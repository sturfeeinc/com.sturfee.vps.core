using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.Core
{
	public static class TaskExtensions
	{
		public static IEnumerator AsCoroutine(this Task task)
		{
			while (!task.IsCompleted)
			{
				yield return null;
			}
		}

		public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
		{
			var tcs = new TaskCompletionSource<object>();
			asyncOp.completed += obj => { tcs.SetResult(null); };
			return ((Task)tcs.Task).GetAwaiter();
		}

		public static Task CancelOnFaulted(this Task task, CancellationTokenSource cts)
		{
			if(cts != null)
				task.ContinueWith(task => cts.Cancel(), cts.Token, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
			return task;
		}

		public static Task<T> CancelOnFaulted<T>(this Task<T> task, CancellationTokenSource cts)
		{
			if (cts != null)
				task.ContinueWith(task => cts.Cancel(), cts.Token, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
			return task;
		}
	}
}
