using JetBrains.Annotations;

namespace Nez.AI.BehaviorTree.Actions;

/// <summary>
/// A simple task which will output the specified text and return success. It can be used for debugging.
/// </summary>
[PublicAPI]
public class LogAction<T>(string text) : Behavior<T>
{
	/// <summary>
	/// Is this text an error
	/// </summary>
	public bool IsError { get; init; }

	/// <summary>
	/// Text to log
	/// </summary>
	public string Text { get; init; } = text;


	public override TaskStatus Update(T context)
    {
        if (IsError)
            Debug.Error(Text);
        else
            Debug.Log(Text);

        return TaskStatus.Success;
    }
}