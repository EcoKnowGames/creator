using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glitchers.EcoKnow.Sandbox
{
    public class WinCondition
    {
        public string title { get; protected set; }
        public string description { get; protected set; }

        protected int entityIndex;

        protected float lowerLimit;
        protected float upperLimit;

        public int requiredRounds { get; protected set; }
        private int _consecutiveSuccesses = 0;
        public int ConsecutiveSuccesses => _consecutiveSuccesses;

        private bool _completed;
        public bool Completed => _completed;

        public void Init(WinConditionRecord record)
        {
            //Setup from record

            title = record.Title;
            description = record.Description;
            entityIndex = record.EntityIndex;

            lowerLimit = record.LowerLimit;
            upperLimit = record.UpperLimit;

            requiredRounds = record.RequiredRounds <= 0 ? 1 : record.RequiredRounds;
        }

        public void OnNewRound()
        {
            bool success = HasConditionBeenMet();
            if (success)
            {
                _consecutiveSuccesses += 1;
                if (_consecutiveSuccesses >= requiredRounds)
                {
                    SetComplete();
                }
            }
            else
            {
                _consecutiveSuccesses = 0;
                SetIncomplete();
            }
        }

        protected bool HasConditionBeenMet()
        {
            EntityManager entityManager = SandboxManager.Instance.EntityManager;
            if (entityManager != null)
            {

                Entity entityType = entityManager.GetEntityType(entityIndex);
                if (entityType != null)
                {
                    int totalPopulation = entityManager.GetTotalPopulationOfEntityType(entityIndex);
                    if ((upperLimit > 0) && (upperLimit >= lowerLimit))
                    {
                        if (totalPopulation >= lowerLimit && totalPopulation <= upperLimit)
                        {
                            return true;
                        }
                    }
                    else //If the scenario designer has not set an upper limit or if the upper limit is smaller than the lower limit, we should not consider it
                    {
                        if (totalPopulation >= lowerLimit)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        protected void SetComplete()
        {
            if (_completed)
            {
                return;
            }

            _completed = true;
        }

        protected void SetIncomplete()
        {
            if (!_completed)
            {
                return;
            }

            _completed = false;
        }

    }
}
