namespace Dacodelaac.Events
{
    public interface IEventListener
    {
        void OnEventRaised(IEvent e);
    }
    
    public interface IEventListener<in TType>
    {
        void OnEventRaised(TType data);
    }
}