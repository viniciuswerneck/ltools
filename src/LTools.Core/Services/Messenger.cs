using LTools.Core.Interfaces;

namespace LTools.Core.Services;

public class Messenger : IMessenger
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = [];

    public void Send<T>(T message) where T : class
    {
        if (_handlers.TryGetValue(typeof(T), out var handlers))
        {
            foreach (var handler in handlers.Cast<Action<T>>())
                handler(message);
        }
    }

    public void Subscribe<T>(Action<T> handler) where T : class
    {
        var type = typeof(T);
        if (!_handlers.ContainsKey(type))
            _handlers[type] = [];
        _handlers[type].Add(handler);
    }

    public void Unsubscribe<T>(Action<T> handler) where T : class
    {
        var type = typeof(T);
        if (_handlers.TryGetValue(type, out var handlers))
            handlers.Remove(handler);
    }
}
