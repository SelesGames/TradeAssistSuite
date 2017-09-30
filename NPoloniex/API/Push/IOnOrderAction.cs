namespace NPoloniex.API.Push
{
    public interface IOnOrderAction
    {
        void OnOrderEvent(Order order);
    }
}