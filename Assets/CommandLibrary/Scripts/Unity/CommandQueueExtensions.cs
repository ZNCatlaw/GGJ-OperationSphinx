using System.Collections;
using UnityEngine;

namespace CommandLibrary
{

public static class CommandQueueExtensions
{
	/// <summary>
	/// A helper method for polling a <c>CommandQueue</c> inside a Unity coroutine. Waits
	/// till the queue has finished.
	/// </summary>
	/// <returns>
	/// An <c>IEnumerator</c>, to be used inside a Unity coroutine. See example.
	/// </returns>
	/// <param name='shouldUpdateDeltaTime'>
	/// Whether the method should call the <c>CommandQueue</c>'s <c>Update</c> method. If this
	/// isn't set to <c>true</c>, it is expected the queue's <c>Update</c> method is being called
	/// regularly somewhere  else. Otherwise this method will hang if used inside a coroutine.
	/// </param>
	/// <code>
	/// 	// Inside a unity coroutine
	/// 	CommandQueue queue = new CommandQueue();
	/// 	queue.Enqueue( ... );
	/// 	StartCoroutine(queue.WaitTillFinished())
	/// </code>
	public static IEnumerator WaitTillFinished(this CommandQueue queue, bool shouldUpdateDeltaTime = true)
	{
		bool finished = false;
		while (!finished) {
			finished = queue.Update(Time.deltaTime);
			yield return null;
		}
	}
}

}

