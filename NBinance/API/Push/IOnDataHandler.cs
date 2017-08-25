namespace NBinance.API.Push
{
    public interface IOnDataHandler<T>
    {
        void HandleData(T obj);
    }
}