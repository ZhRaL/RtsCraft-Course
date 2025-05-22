namespace UI
{
    public interface IUIElement<T>
    {
        void EnableFor(T item);
        void Disable();
    }

    public interface IUIElement<T1, T2>
    {
        void EnableFor(T1 item1, T2 callBack);
        void Disable();
        
    }
}