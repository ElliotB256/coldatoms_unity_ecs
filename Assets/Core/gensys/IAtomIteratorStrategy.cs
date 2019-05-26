using Unity.Entities;

public interface IAtomIteratorStrategy<TIterator, TAtomComponent>
    where TIterator : struct, IComponentData
    where TAtomComponent : struct, IComponentData
{
    void Execute(TIterator iterator, ref TAtomComponent atom);
}