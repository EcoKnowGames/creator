using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.Grid
{
    public class CellEntity
    {
        private string _id;
        public string ID => _id;
        private int _population;
        public int Population { get => _population; set { _population = value; } }

        public CellEntity(string id, int population)
        {
            _id = id;
            _population = population;
        }
    }
}
