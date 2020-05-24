namespace Common
{
    public interface IVisitable
    {
        void Accept(IVisitor pVisitor);
    }
}
