namespace Glitchers.EcoKnow.Sandbox
{
    /// <summary>
    /// Shared movement/dispersal abstraction so calculators can compose and reuse
    /// movement independently of their population maths (Issue #57).
    /// </summary>
    public interface IMovementModel
    {
        string Name();
        void Move(EntityManager entityManager, int[,,] entityLookupTable);
    }
}
