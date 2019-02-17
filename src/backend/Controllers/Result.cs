namespace backend.Controllers
{
    public abstract class PunchClockResult<T> { }

    public sealed class Success<T> : PunchClockResult<T>
    {
        public Success(T result)
        {
            Result = result;
        }
        public T Result { get; }
    }

    public sealed class Err<T> : PunchClockResult<T>
    {
        public Err(string msg)
        {
            Error = msg;
        }
        public string Error { get; }
    }
}