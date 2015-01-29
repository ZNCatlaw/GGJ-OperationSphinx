using System;
using System.Collections.Generic;
using UnityEngine;


namespace CommandLibrary
{

public class CommandBehaviour : MonoBehaviour
{

	#region Public methods

	public CommandQueue Queue(params CommandDelegate[] commands)
	{
		var queue = new CommandQueue ();
		queue.Enqueue (commands);
		_queues.Add (queue);
		return queue;
	}

	public void RemoveQueue(CommandQueue queue)
	{
		if (queue == null) {
			throw new ArgumentNullException ("queue");
		}
		
		if (_queues.Contains (queue)) {
			_queues.Remove (queue);
		}
	}

	public void Schedule(params CommandDelegate[] commands)
	{
		_scheduler.Add(Commands.Sequence(commands));
	}

	#endregion

	#region MonoBehaviour events
	
	private void Update()
	{
		foreach (var queue in _queues) {
			queue.Update (Time.deltaTime);
		}

		_scheduler.Update (Time.deltaTime);
	}
	
	#endregion
	
	#region Private fields
	private HashSet<CommandQueue> _queues = new HashSet<CommandQueue>();
	private CommandScheduler _scheduler = new CommandScheduler();
	#endregion
}

public static class GameObjectExtensions
{
	#region Static methods
	public static CommandQueue Queue(this GameObject gm, params CommandDelegate[] commands)
	{
		return GetCommandBehaviour(gm).Queue(commands);
	}

	public static void RemoveQueue(this GameObject gm, CommandQueue queue)
	{
		GetCommandBehaviour(gm).RemoveQueue(queue);
	}

	public static void Schedule(this GameObject gm, params CommandDelegate[] commands)
	{
		GetCommandBehaviour(gm).Schedule(commands);
	}

	#endregion

	#region Private static methods
	private static CommandBehaviour GetCommandBehaviour(GameObject gm)
	{
		var behaviour = gm.GetComponent<CommandBehaviour>();
		if (behaviour == null) {
			behaviour = gm.AddComponent<CommandBehaviour> ();
		}
		return behaviour;
	}
	#endregion
}

}
