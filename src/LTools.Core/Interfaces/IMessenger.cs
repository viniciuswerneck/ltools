namespace LTools.Core.Interfaces;

/// <summary>Weak-reference messaging bus for decoupled communication between components.</summary>
public interface IMessenger
{
    /// <summary>Sends a message of type T to all subscribed handlers.</summary>
    void Send<T>(T message) where T : class;
    /// <summary>Subscribes a handler to receive messages of type T.</summary>
    void Subscribe<T>(Action<T> handler) where T : class;
    /// <summary>Unsubscribes a previously registered handler.</summary>
    void Unsubscribe<T>(Action<T> handler) where T : class;
}
