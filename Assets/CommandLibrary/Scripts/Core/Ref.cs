using System;

namespace CommandLibrary
{

/// <summary>
/// The Ref class is used for binding properties/fields for functional tweening.
/// <code>
/// 	int x = 0;
/// 	Ref<int> xRef = new Ref<int>( () => x, val => {x = val } );
/// 	xRef.Value += 43;
/// 	Debug.Log(x); // Prints 43
/// </code>
/// </summary>
public sealed class Ref<T>
{
	#region Public properties
	public T Value
	{
		get { return _getter(); }
		set { _setter(value); }
	}
	#endregion
	
	#region Public methods
	/// <summary>
	/// Creates a Reference. A reference class encapsulates a getter and
	/// setter for modifying an external variable.
	/// </summary>
	public Ref(Func<T> getter, Action<T> setter)
	{
		_getter = getter;
		_setter = setter;
	}
	#endregion
	
	#region Private fields
	private readonly Func<T> _getter;
	private readonly Action<T> _setter;
	#endregion
}
	
}

