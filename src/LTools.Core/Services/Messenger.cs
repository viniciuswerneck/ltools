using LTools.Core.Interfaces;

namespace LTools.Core.Services;

public class Messenger : IMessenger
{
    private readonly Dictionary<Type, List<WeakReference>> _handlers = [];

    public void Send<T>(T message) where T : class
    {
        if (!_handlers.TryGetValue(typeof(T), out var handlers))
            return;

        var alive = new List<WeakReference>();

        foreach (var weakRef in handlers)
        {
            if (weakRef.Target is Action<T> handler)
            {
                handler(message);
                alive.Add(weakRef);
            }
        }

        if (alive.Count < handlers.Count)
            _handlers[typeof(T)] = alive;
    }

    public void Subscribe<T>(Action<T> handler) where T : class
    {
        var type = typeof(T);
        if (!_handlers.ContainsKey(type))
            _handlers[type] = [];
        _handlers[type].Add(new WeakReference(handler));
    }

    public void Unsubscribe<T>(Action<T> handler) where T : class
    {
        if (!_handlers.TryGetValue(typeof(T), out var handlers))
            return;

        handlers.RemoveAll(w => w.Target is Action<T> h && h == handler);
    }
}
