namespace SOSIEL.Algorithm
{
    public interface IAlgorithm
    {
        string Name { get; }

        string Run();
    }
    
    /*public interface IAlgorithm<TData>
    {
        string Name { get; }

        void Initialize(TData data);

        TData Run(TData data);
    }*/
}
