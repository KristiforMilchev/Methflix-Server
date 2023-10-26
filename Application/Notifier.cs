namespace Application;

public static class Notifier
{
    record ActionCall
    {
        public Action<object> Action { get; set; }
        public string Name { get; set; }
    }

    

    static List<ActionCall> _callbacks = new();

    public static void Subscribe(string name,Action<object> action)
    {
        _callbacks.Add(new ActionCall
        {
            Action = action,
            Name = name
        });
    }

    public static void Call(string name, object data)
    {
        _callbacks.FirstOrDefault(x => x.Name == name)?.Action.Invoke(data);
    }

    public static void Dispose(string action)
    {
        var exists = _callbacks.FirstOrDefault(x => x.Name == action);
        if(exists == null) return;
        
        _callbacks.Remove(exists);
    }
}
