using System;
using System.ComponentModel;

namespace Protoplasm.ComponentModel
{
    public class EventObserver<TSender, TEventArgs> 
    {
        static EventObserver()
        {
            BeforeEvent += x => { };
            AfterEvent += x => { };
        }

        public static event EventWithSenderHandler<TSender, TEventArgs> BeforeEvent;
        public static event EventWithSenderHandler<TSender, TEventArgs> AfterEvent;

        protected static Func<TSender, TEventArgs, IEventArgsWithSender<TSender, TEventArgs>> _getEventArgsWithSender =
            (sender, e) => new EventArgsWithSender<TSender, TEventArgs>(sender, e);

        public static IEventArgsWithSender<TSender, TEventArgs> Before(TSender sender, TEventArgs args)
        {
            var e = _getEventArgsWithSender(sender, args);
            Before(e);
            return e;
        }

        public static void Before<T>(T e)
            where T : IEventArgsWithSender<TSender, TEventArgs>
        {
            var handler = BeforeEvent;
            if (handler != null)
                handler(e);
        }

        public static IEventArgsWithSender<TSender, TEventArgs> After(TSender sender, TEventArgs args)
        {
            var e = _getEventArgsWithSender(sender, args);
            After(e);
            return e; 
        }

        public static void After<T>(T e)
            where T : IEventArgsWithSender<TSender, TEventArgs>
        {
            var handler = AfterEvent;
            if (handler != null)
                handler(e);
        }
    }


    public delegate void EventWithSenderHandler<in TSender, in TEventArgs>(IEventArgsWithSender<TSender, TEventArgs> e);

    public interface IEventArgsWithSender<out TSender, out TEventArgs>
    {
        TEventArgs Args { get; }
        string ID { get; }
        TSender Sender { get; }
        object Tag { get; set; }
    }

    public class EventArgsWithSender<TSender, TEventArgs> : IEventArgsWithSender<TSender, TEventArgs>
    {
        private readonly TSender _sender;
        private TEventArgs _args;

        public object Tag { get; set; }

        protected internal EventArgsWithSender(TSender sender, TEventArgs args)
        {
            ID = Guid.NewGuid().ToString();
            _sender = sender;
            _args = args;
        }

        public TEventArgs Args
        {
            get { return _args; }
        }

        public string ID { get; protected set; }

        public TSender Sender
        {
            get { return _sender; }
        }
    }

    public class Test
    {
        void tmp()
        {
            EventObserver<object, PropertyChangedEventArgs>.Before(null, new PropertyChangedEventArgs(""));
        }
    }

    //================================================

    public class PropertyChangedObserver : EventObserver<object, PropertyChangedEventArgs>
    {
        static PropertyChangedObserver()
        {
            _getEventArgsWithSender = (sender, e) => new PropertyChangedEventArgsWithSender(sender, e);
        }

        public static PropertyChangedEventArgsWithSender Before(object sender, string propertyName)
        {
            var e = new PropertyChangedEventArgsWithSender(sender, propertyName);
            Before(e);
            return e;
        }
    }

    public class PropertyChangedEventArgsWithSender : PropertyChangedEventArgs, IEventArgsWithSender<object, PropertyChangedEventArgs>
    {
        private readonly object _sender;

        public PropertyChangedEventArgsWithSender(object sender, PropertyChangedEventArgs args)
            : this(sender, args.PropertyName)
        {
            
        }

        public PropertyChangedEventArgsWithSender(object sender, string propertyName)
            : base(propertyName)
        {
            ID = Guid.NewGuid().ToString();
            _sender = sender;
        }

        public PropertyChangedEventArgs Args
        {
            get { return this; }
        }

        public string ID { get; protected set; }

        public object Tag { get; set; }

        public object Sender
        {
            get { return _sender; }
        }

        public void After()
        {
            PropertyChangedObserver.After(this);
        }
    }

    //================================================

    //public delegate void PropertyChangedEventWithSenderHandler(PropertyChangedEventArgsWithSender1 e);

    //public static class PropertyChangedObserver1
    //{
    //    static PropertyChangedObserver1()
    //    {
    //        BeforePropertyChanged += x => { };
    //        AfterPropertyChanged += x => { };
    //    }

    //    public static event PropertyChangedEventWithSenderHandler BeforePropertyChanged;
    //    public static event PropertyChangedEventWithSenderHandler AfterPropertyChanged;

    //    public static PropertyChangedEventArgsWithSender1 Before(object sender, string propertyName)
    //    {
    //        var e = new PropertyChangedEventArgsWithSender1(sender, propertyName);
    //        Before(sender, e);
    //        return e;
    //    }

    //    public static PropertyChangedEventArgsWithSender1 Before(object sender, PropertyChangedEventArgs e)
    //    {
    //        var ea = (e as PropertyChangedEventArgsWithSender1) ?? (new PropertyChangedEventArgsWithSender1(sender, e.PropertyName));
    //        Before(ea);
    //        return ea;
    //    }

    //    public static void Before(PropertyChangedEventArgsWithSender1 e)
    //    {
    //        BeforePropertyChanged(e);
    //    }

    //    public static void After(object sender, PropertyChangedEventArgs e)
    //    {
    //        var ea = (e as PropertyChangedEventArgsWithSender1) ?? (new PropertyChangedEventArgsWithSender1(sender, e.PropertyName));
    //        After(ea);
    //    }

    //    public static void After(PropertyChangedEventArgsWithSender1 e)
    //    {
    //        AfterPropertyChanged(e);
    //    }
    //}

    //public class PropertyChangedEventArgsWithSender1 : PropertyChangedEventArgs
    //{
    //    private readonly object _sender;

    //    public PropertyChangedEventArgsWithSender1(object sender, string propertyName) : base(propertyName)
    //    {
    //        ID = Guid.NewGuid();
    //        _sender = sender;
    //    }

    //    public Guid ID { get; private set; }

    //    public object Sender
    //    {
    //        get { return _sender; }
    //    }

    //    public void After()
    //    {
    //        PropertyChangedObserver1.After(this);
    //    }
    //}
}