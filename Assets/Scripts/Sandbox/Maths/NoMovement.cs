namespace Glitchers.EcoKnow.Sandbox
{
    /// <summary>
    /// A movement model that leaves populations in place; useful for models with no dispersal.
    /// </summary>
    public class NoMovement : IMovementModel
    {
        public string Name() => "No Movement";

        public void Move(EntityManager entityManager, int[,,] entityLookupTable)
        {
            /* intentionally does nothing */
        }
    }
}
