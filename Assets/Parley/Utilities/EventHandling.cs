// ============================================================================
//  Parley - Dialogue System
//  Copyright (c) 2026 KodeFlow Studios. All rights reserved.
// ----------------------------------------------------------------------------
//  File:    EventHandling.cs
//  Summary: Events you can both subscribe to (classic C# style) AND await
//           (async/await style). The trick is exposing a GetAwaiter() so
//           the compiler lets you write `await myEvent;` — when the event
//           next fires, execution continues. One-shot per await.
// ============================================================================

using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KodeFlowStudios.Parley.EventHandling
{
	internal class AwaitableEvent
	{
		private event Action InternalEvent;

		public void Invoke() => InternalEvent?.Invoke();

		public static AwaitableEvent operator +(AwaitableEvent e, Action handler)
		{
			e.InternalEvent += handler;
			return e;
		}

		public static AwaitableEvent operator -(AwaitableEvent e, Action handler)
		{
			e.InternalEvent -= handler;
			return e;
		}

		public TaskAwaiter GetAwaiter()
		{
			var tcs = new TaskCompletionSource<object>();
			Action handler = null;
			handler = () =>
			{
				InternalEvent -= handler;
				tcs.SetResult(null);
			};
			InternalEvent += handler;
			return ((Task)tcs.Task).GetAwaiter();
		}
	}

	public class AwaitableEvent<T>
	{
		private event Action<T> InternalEvent;

		public void Invoke(T value) => InternalEvent?.Invoke(value);

		public static AwaitableEvent<T> operator +(AwaitableEvent<T> e, Action<T> handler)
		{
			e.InternalEvent += handler;
			return e;
		}

		public static AwaitableEvent<T> operator -(AwaitableEvent<T> e, Action<T> handler)
		{
			e.InternalEvent -= handler;
			return e;
		}

		public TaskAwaiter<T> GetAwaiter()
		{
			var tcs = new TaskCompletionSource<T>();
			Action<T> handler = null;
			handler = (value) =>
			{
				InternalEvent -= handler;
				tcs.SetResult(value);
			};
			InternalEvent += handler;
			return tcs.Task.GetAwaiter();
		}
	}
}
