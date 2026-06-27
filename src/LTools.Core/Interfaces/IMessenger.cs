namespace LTools.Core.Interfaces;

public interface IMessenger
{
    void Send<T>(T message) where T : class;
    void Subscribe<T>(Action<T> handler) where T : class;
    void Unsubscribe<T>(Action<T> handler) where T : class;
}
