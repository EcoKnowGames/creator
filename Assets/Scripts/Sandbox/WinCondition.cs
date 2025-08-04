using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glitchers.EcoKnow.Sandbox
{
    public class WinCondition
    {
        public enum Result { NOT_STARTED, IN_RANGE, STREAK, GRACE, FAILED };
        private List<Result> _resultCache;
        public List<Result> Results => _resultCache;

        public string title { get; protected set; }
        public string description { get; protected set; }

        protected int entityIndex;
        public int EntityIndex => entityIndex;

        protected float lowerLimit;
        protected float upperLimit;

        private int _graceRemaining = 1;

        public int requiredRounds { get; protected set; }
        private int _consecutiveSuccesses = 0;
        public int ConsecutiveSuccesses => _consecutiveSuccesses;

        private bool _completed;
        public bool Completed => IsCurrentlyComplete();

        public void Init(WinConditionRecord record)
        {
            //Setup from record

            title = record.Title;
            description = record.Description;
            entityIndex = record.EntityIndex;

            lowerLimit = record.LowerLimit;
            upperLimit = record.UpperLimit;

            requiredRounds = record.RequiredRounds <= 0 ? 1 : record.RequiredRounds;
            _graceRemaining = 1;

            _resultCache = new List<Result>();
        }

        public void OnNewRound()
        {
            if (GetPreviousResult() == Result.FAILED)
            {
                //No longer tracked
            }
            else
            {
                bool success = HasConditionBeenMet();
                if (success)
                {
                    OnSuccessfulRound();
                }
                else
                {
                    OnFailedRound();
                }
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

        private void OnSuccessfulRound()
        {
            if (IsCurrentlyComplete())
            {
                PushResult(Result.STREAK);
            }
            else
            {
                PushResult(Result.IN_RANGE);
            }
        }

        private void OnFailedRound()
        {
            if (GetPreviousResult() == Result.NOT_STARTED)
            {
                PushResult(Result.NOT_STARTED);
            }
            else
            {
                Result failResult = CanEnterGracePeriod() ? Result.GRACE : Result.FAILED;
                PushResult(failResult);
            }
        }


        #region Result
        private bool CanEnterGracePeriod()
        {
            return _graceRemaining > 0;
        }

        private bool IsCurrentlyComplete()
        {
            //Too early to start a STREAK
            int startIndex = _resultCache.Count - requiredRounds;
            if (startIndex < 0)
            {
                return false;
            }

            //If recentresults are all IN-RANGE or STREAK, we streak. otherwise we are in range
            Result[] recentResults = _resultCache.GetRange(startIndex, requiredRounds).ToArray();
            return recentResults.All(x => x == Result.IN_RANGE || x == Result.STREAK);
        }
        private void PushResult(Result result)
        {
            if (_resultCache == null)
            {
                _resultCache = new List<Result>();
            }

            if (result == Result.GRACE)
            {
                _graceRemaining -= 1;
            }

            _resultCache.Add(result);
        }

        private Result GetPreviousResult()
        {
            if ((_resultCache == null) || (_resultCache.Count == 0))
            {
                return Result.NOT_STARTED;
            }

            return _resultCache.Last();
        }

        public Dictionary<int, string> GetRoundResults()
        {
            Dictionary<int, string> results = new Dictionary<int, string>();
            if (_resultCache != null)
            {
                for (int i = 0; i < _resultCache.Count; i++)
                {
                    //Match round number
                    results.Add(i + 1, _resultCache[i].ToString());
                }
            }

            return results;
        }
        #endregion
    }
}
