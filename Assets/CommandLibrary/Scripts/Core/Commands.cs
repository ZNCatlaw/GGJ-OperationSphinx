using System.Collections.Generic;

namespace CommandLibrary
{

public delegate void CommandDo();
public delegate bool CommandCondition();
public delegate bool CommandWhile(double elapsedTime);
public delegate void CommandDuration(double t);
public delegate bool CommandDelegate(ref double deltaTime);
public delegate IEnumerator<CommandDelegate> CommandCoroutine();

static public partial class Commands
{
	/// <summary>
	/// An <c>CommandDo</c> runs precisely once.
	/// </summary>
	/// <param name="command"> 
	/// The command to execute. Must be non-null.
	/// </param>
	/// <exception cref="System.ArgumentNullException"></exception>
	public static CommandDelegate Do(CommandDo command)
	{
		CheckArgumentNonNull(command);
		return delegate(ref double deltaTime) {
			command();
			return true;
		};
	}
	
	/// <summary>
	/// An <c>CommandWhile</c> runs until the command returns false.
	/// </summary>
	/// <param name="command"> 
	/// The command to execute. Must be non-null.
	/// </param>
	/// <exception cref="System.ArgumentNullException"></exception>
	public static CommandDelegate While(CommandWhile command)
	{
		CheckArgumentNonNull(command);
		double elapsedTime = 0.0;
		return Commands.Sequence(
			Commands.Do(() => elapsedTime = 0.0),
			delegate(ref double deltaTime) {
				elapsedTime += deltaTime;
				bool finished = !command(elapsedTime);
				if (!finished) { deltaTime = 0.0; }
				return finished;
			}
		);
	}
	
	/// <summary>
	/// An <c>CommandDuration</c> runs over a duration of time.
	/// </summary>
	/// <param name="command">
	/// The command to execute. Must be non-null.
	/// </param>
	/// <param name="duration">
	/// The duration of time, in seconds, to apply the command over. Must be greater than 0.
	/// </param>
	/// <param name="ease">
	/// An easing function to apply to the <c>t</c> parameter of an
	/// <c>CommandDuration</c> delegate. If null, linear easing is used.
	/// </param>
	/// <exception cref="System.ArgumentNullException"></exception>
	/// <exception cref="System.ArgumentOutOfRange"></exception>
	public static CommandDelegate Duration(CommandDuration command, double duration, CommandEase ease = null)
	{
		CheckArgumentNonNull(command);
		CheckDurationGreaterThanZero(duration);
		double elapsedTime = 0.0;
		return Commands.Sequence(
			Commands.Do( () => elapsedTime = 0.0),
			delegate(ref double deltaTime) {

				elapsedTime += deltaTime;
				deltaTime = 0.0;
				double t = (elapsedTime / duration);
				t = t < 0.0 ? 0.0 : (t > 1.0 ? 1.0 : t);
				if (ease != null) { t = ease(t); }
				command(t);
				bool finished = elapsedTime >= duration;
				if (finished) { deltaTime = elapsedTime - duration; }
				return finished;
			}
		);
	}
	

	/// <summary>
	/// A Wait command does nothing until duration has elapsed
	/// </summary>
	/// <param name="duration"> 
	/// The duration of time, in seconds, to wait. Must be greater than 0.
	/// </param>
	/// <exception cref="System.ArgumentOutOfRange"></exception>
	public static CommandDelegate WaitForSeconds(double duration)
	{
		CheckDurationGreaterThanZero(duration);
		double elapsedTime = 0.0;
		return Commands.Sequence(
			Commands.Do( () => elapsedTime = 0.0),
			delegate(ref double deltaTime) {
				elapsedTime += deltaTime;
				deltaTime = 0.0f;
				bool finished = elapsedTime >= duration;
				if (finished) { deltaTime = elapsedTime - duration; }
				return finished;
			}
		);
	}
	
	
	/// <summary>
	/// Waits a specified number of calls to update. This ignores time althogether.
	/// </summary>
	/// <param name="frameCount">
	/// The number of frames to wait. Must be > 0.
	/// </param>
	/// <exception cref="System.ArgumentOutOfRangeException"></exception>
	public static CommandDelegate WaitForFrames(int frameCount)
	{
		if (frameCount <= 0) { throw new System.ArgumentOutOfRangeException("frameCount",frameCount, "frameCount must be > 0."); }
		int counter = frameCount;
		return Commands.Sequence(
			Commands.Do(() => counter = frameCount),
			Commands.While((elapsedTime) => {
				if (counter > 0) { 
					--counter;
					return true;
				}
				return false;
			})
		);
		
	}
	
	/// <summary>
	/// A Parallel command executes several commands in parallel. It finishes
	/// when the last command has finished.
	/// </summary>
	/// <param name="command"> 
	/// The command to execute. Must be non-null.
	/// </param>
	/// <exception cref="System.ArgumentNullException"></exception>
	public static CommandDelegate Parallel(params CommandDelegate[] commands)
	{
		LinkedList<CommandDelegate> list = null;
		foreach (var command in commands) {
			CheckArgumentNonNull(command);
		}
		return Sequence(
			Commands.Do(() => {
				list = new LinkedList<CommandDelegate>(commands);
			}),
			delegate(ref double deltaTime) {
				bool finished = true;
				double smallestDeltaTime = deltaTime;
				var node = list.First;
			
				while (node != null) {
					var next  = node.Next;
				
					double deltaTimeCopy = deltaTime;
					bool thisFinished = node.Value(ref deltaTimeCopy);
				
					if (thisFinished) { list.Remove(node); } else { finished = false; } 
					smallestDeltaTime = System.Math.Min(deltaTimeCopy, smallestDeltaTime);
				
					node = next;
				}
			
				deltaTime = smallestDeltaTime;
			
				return finished;
		});
	}
	
	/// <summary>
	/// A Sequence command executes several commands sequentially.
	/// </summary>
	/// <param name="commands"> 
	/// A parameter list of commands to execute sequentially. All commands must be non-null.
	/// </param>
	/// <exception cref="System.ArgumentNullException"></exception>
	public static CommandDelegate Sequence(params CommandDelegate[] commands)
	{
		foreach (var command in commands) {
			CheckArgumentNonNull(command);
		}
		CommandQueue subQueue = null;
		return delegate(ref double  deltaTime) {
			if (subQueue == null) {
				// To make sequences repeatable, we set subQueue to null
				// after the sequence finishes, so when the sequence is
				// re-executed, we can recreate subQueue.
				subQueue = new CommandQueue();
				subQueue.Enqueue(commands);
			}
			bool finished = subQueue.Update(ref deltaTime);
			if (finished) { subQueue = null;}
			return finished;
		};
	}
	
	/// <summary>
	/// A  Queue command allows Commands to be nested recursively in queues. Queues
	/// are different to Sequences in that they are depletable, (so be careful if
	/// you are wrapping a queue in a Repeat command).
	/// </summary>
	/// <param name="queue"> 
	/// The queue to execute. Must be non-null.
	/// </param>
	/// <exception cref="System.ArgumentNullException"></exception>
	public static CommandDelegate Queue(CommandQueue queue)
	{
		CheckArgumentNonNull(queue, "queue");
		return delegate(ref double deltaTime) {
			return queue.Update(ref deltaTime);
		};
	}
	
	/// <summary>
	/// A Condition command allows branching behaviour. After a condition evaluates to <c>true</c>
	/// then onTrue will be evaluated until it finishes. Otherise onFalse will be evaluated, (if it 
	/// isn't null). When nested in a Repeat command, conditions will be re-evaluated once for every
	/// repeat.
	/// </summary>
	/// <param name="condition"> 
	/// The condition to evaluate. Must be non-null.
	/// </param>
	/// <param name="onTrue"> 
	/// The command to execute if condition evaluates to true. Must be non-null.
	/// </param>
	/// <param name="onFalse"> 
	/// The command to execute if condition evaluates to false.
	/// </param>
	/// <exception cref="System.ArgumentNullException"></exception>
	public static CommandDelegate Condition(CommandCondition condition, CommandDelegate onTrue, CommandDelegate onFalse = null)
	{
		CheckArgumentNonNull(condition, "condition");
		CheckArgumentNonNull(onTrue, "onTrue");
		CommandDelegate result = onFalse;
		return Sequence(
			Commands.Do( delegate() {
				result = onFalse;
				if (condition()) {
					result = onTrue;
				}
			}),
			delegate(ref double deltaTime) {
				if (result != null){
					return result(ref deltaTime);
				}
				return true;
			}
		);
	}
		
	// <summary>
	/// Require the specified condition to be true to continue executing the given command. 
	/// </summary>
	/// <param name='condition'>
	/// A condition which must remain true to continue executing the commands. Must be non-null.
	/// </param>
	/// <param name='commands'>
	/// A list of commands to be exexuted while condition is true. Must be non-null.
	/// </param>
	/// <remarks>
	/// The condition is only re-evaluated on new calls to Update, or after the child command finishes and restarts.
    /// This means that if the condition suddenly becomes false while the command is executing, it
    /// won't be immediately escaped.
	/// </remarks>
	/// <example>
	/// <code>
	/// 	CommandQueue queue = new CommandQueue();
	/// 	queue.Enqueue(
	/// 		Commands.Require( () => someObject != null,
	/// 			Commands.MoveTo(someObject, somePosition, someDuration)
	/// 		)
	/// 	);
	/// </code>
	/// </example>
	/// <exception cref="System.ArgumentNullException"></exception>
	public static CommandDelegate Require(CommandCondition condition,  params CommandDelegate[] commands)
	{
		CheckArgumentNonNull(condition, "condition");
			
		CommandDelegate sequence = Commands.Sequence(commands);
		
		return (ref double deltaTime) => condition() ? sequence(ref deltaTime) : true;
	}

	/// <summary>
	/// Loops over the specified commands, re-evaluating the condition at the start of every loop.
	/// </summary>
	/// <param name='condition'>
	/// A condition which must remain true to continue executing the commands. Must be non-null.
	/// </param>
	/// <param name='commands'>
	/// A list of commands to be exexuted while condition is true. Must be non-null.
	/// </param>
	/// <exception cref="System.ArgumentNullException"></exception>
	public static CommandDelegate While(CommandCondition condition, params CommandDelegate[] commands)
	{
		CheckArgumentNonNull(condition, "condition");
		CommandDelegate sequence = Commands.Sequence(commands);

		bool finished = true;
		return (ref double deltaTime) => {
			if (!finished) {
				finished = sequence(ref deltaTime);
			}
			while (finished) {
				if (!condition()) {
					return true;
				}
				finished = sequence(ref deltaTime);
			}

			return false;
		};
	}
	
	/// <summary>
	/// The Repeat command repeats a delegate a given number of times.
	/// </summary>
	/// <param name="repeatCount"> 
	/// The number of times to repeat the given command. Must be > 0.
	/// </param>
	/// <param name='commands'>
	/// The command sto repeat. All of the default commands, (except for Queue),
	/// are repeatable without side-effects. When writing your own Commands,
	/// be careful to make sure state inside the command can be reset. A simple
	/// way to do this is to wrap the command inside a Sequence.
	/// <code>
	/// 	int counter = 0;
	/// 	CommandDelegate someCommand = Commands.Sequence(
	/// 		Commands.Do(delegate() {
	/// 			// Reset state here. 
	/// 			counter = 0;
	/// 		}),
	/// 		Commands.While(delegate(double elapsedTime) {
	/// 			counter++;
	/// 			Debug.Log(counter);
	/// 			return (counter <= 5);
	/// 		})
	/// 	);
	/// </code>
	/// Must be non-null.
	/// </param>
	/// <exception cref="System.ArgumentNullException"></exception>
	/// <exception cref="System.ArgumentOutOfRangeException"></exception>
	public static CommandDelegate Repeat(int repeatCount, params CommandDelegate[] commands)
	{
		if (repeatCount <= 0) { throw new System.ArgumentOutOfRangeException("repeatCount",repeatCount, "repeatCount must be > 0."); }
		foreach (var command in commands) {
			CheckArgumentNonNull(command);
		}
		CommandQueue subQueue = new CommandQueue();
		subQueue.Enqueue(commands);
		int count  = repeatCount - 1;
		return delegate(ref double deltaTime) {
			bool finished = subQueue.Update(ref deltaTime);
			while (finished) {
				subQueue = new CommandQueue(); // Clears deltaTime state.
				if (count > 0) {
					subQueue.Enqueue(commands);
					--count;
					finished = subQueue.Update(ref deltaTime);
				} else {
					count = repeatCount;
					return true;
				}
			}
			return false;
		};
	}
	
	/// <summary>
	/// Repeats a command forever.
	/// </summary>
	/// <param name="commands"> 
	/// The commands to execute. Must be non-null.
	/// </param>
	/// <exception cref="System.ArgumentNullException"></exception>
	public static CommandDelegate RepeatForever(params CommandDelegate[] commands)
	{
		foreach (var command in commands) {
			CheckArgumentNonNull(command);
		}
		CommandQueue subQueue = new CommandQueue();
		subQueue.Enqueue(commands);
		return delegate(ref double deltaTime) {
			bool finished = subQueue.Update(ref deltaTime);
			while (finished) {
				subQueue = new CommandQueue();
				subQueue.Enqueue(commands);
				finished = subQueue.Update(ref deltaTime);
			}
			
			return false;
		};
	}
	
	/// <summary>
	/// Creates a command which runs a coroutine.
	/// </summary>
	/// <param name='command'>
	/// The command to generate the coroutine.
	/// </param>
	/// <remarks>
	/// The reason this method doesn't just except an IEnumerator is that
	/// IEnumerators created from continuations can't be reset, (a continuation is
	/// any method containing a yield statement, and returning an IEnumerator).  This means
	/// that coroutines would break when executed within a repeat command.
	/// By encapsulating the call to create the IEnumerator in a delegate, it is possible for a
	/// user to call the coroutine however they please, and for it to be repeatable.
	/// </remarks>
	/// <example>
	/// <code>
	/// 	private CommandQueue _queue = new CommandQueue();
	/// 
	///     IEnumerator<CommandDelegate> CoroutineMethod(int firstVal, int secondVal, int thirdVal)
	///     {
	///			Debug.Log(firstVal);
	/// 		yield return Commands.WaitForSeconds(1.0f); // You can return any CommandDelegate here.
	/// 		Debug.Log(secondVal);
	/// 		yield return null; // Wait a single frame.
	/// 		Debug.Log(thirdVal);
	/// 		yield break; // Force exits the coroutine.
	///     }
	/// 
	///     void Start() 
	/// 	{ 
	/// 		_queue.Enqueue(
	/// 			Commands.Coroutine( () => CoroutineCommand(1,2,3)
	/// 		);
	/// 	}
	/// 
	/// 	void Update()
	/// 	{
	/// 		_queue.Update(Time.deltaTime);
	/// 	}
	/// 	
	/// </code>
	/// </example>
	public static CommandDelegate Coroutine(CommandCoroutine command)
	{
		IEnumerator<CommandDelegate> coroutine = null;
		bool isEmpty = false;
		CommandDelegate currentCommand = null;
		
		System.Action setCurrentCommand = () => {
			currentCommand = coroutine.Current;
			if (currentCommand == null) {
				currentCommand = Commands.WaitForFrames(1);
			}
		};
		
		return Commands.Sequence(
			Commands.Do( () => {
				coroutine = command();
				isEmpty = !coroutine.MoveNext();
				setCurrentCommand();
			}),
			delegate (ref double deltaTime) {
				if (isEmpty) { return true; }
				bool finished = currentCommand(ref deltaTime);
				while (finished) {
					bool finishedCoroutine = !coroutine.MoveNext();

					if (finishedCoroutine) { 
						return true;
					}
				
					setCurrentCommand();
					finished = currentCommand(ref deltaTime);
				}
				return false;
			}
		);
	}
	
	/// <summary>
	/// Chooses a random child command to perform. Re-evaluated on repeat.
	/// </summary>
	/// <param name='commands'>
	/// A list of commands to choose from at random. Only one command will be performed.
	/// Null commands can be passed. At least one command must be specified.
	/// </param>
	/// <exception cref='System.ArgumentException'> </exception>
	public static CommandDelegate ChooseRandom(params CommandDelegate[] commands)
	{
		if (commands.Length == 0) {
			throw new System.ArgumentException("Must have at least one command parameter.", "commands");
		}
			
		System.Random random = new System.Random();
		CommandDelegate command = null;
		
		return Commands.Sequence(
			Commands.Do( () => {
				command = commands[random.Next(0, commands.Length)];
			}),
			delegate(ref double deltaTime) {
				if (command != null) {
					return command(ref deltaTime);
				}
				return true;
			}
		);
	}

	/// <summary>
	/// Runs the specified command on a new thread, only returning when the thread has
	/// finished. The Thread command should be used sparingly, as it instantiates a
	/// fresh thread each time the command is entered.
	/// </summary>
	/// <param name="command">
	///  The command to run on it's own thread. Do not use commands which call Unity methods,
	/// (this includes tweens on GameObjects).
	/// </param>
	/// <remarks>
	/// This will create a thread, but it can't force the thread to finish. This means that even
	/// if the Command is garbage collected, the thread may be left running. In Unity's editor,
	/// poorly written threads may be left running between play sessions, which can cause strange
	/// or unexpected behaviour. For this reason it is recommended  to avoid the Thread command 
	/// where possible. An example of a place where a thread might be required is to wrap blocking
	/// system methods, such as file io, or using system sockets.
	/// </remarks>
	public static CommandDelegate Thread(CommandDo command)
	{
		CheckArgumentNonNull (command);

		System.Threading.Thread thread = null;
		return Commands.Sequence (
			Commands.Do( () => {
				thread = new System.Threading.Thread( new System.Threading.ThreadStart(command));
				thread.Start();
			}),
			Commands.While( (t) => { 
				return thread.IsAlive;
			})
		);
	}
	
	private static void CheckArgumentNonNull(object obj, string argumentName = "command")
	{
		if (obj == null) { 
			throw new System.ArgumentNullException(argumentName); 
		}
	}
	
	private static void CheckDurationGreaterThanZero(double duration)
	{
		if (duration <= 0.0) {
			throw new System.ArgumentOutOfRangeException("duration", duration, "duration must be > 0");
		}
	}
	
}
	
}