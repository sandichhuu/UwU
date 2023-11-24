namespace UwU.Core
{
    public interface ILogger
    {
        void Trace(object message);

        void Warn(object message);

        void Error(object message);
    }
}