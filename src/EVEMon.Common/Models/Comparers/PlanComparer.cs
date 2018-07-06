using System;
using System.Collections.Generic;
using EVEMon.Common.Enumerations;

namespace EVEMon.Common.Models.Comparers
{
    /// <summary>
    /// Implements a plan comparer.
    /// </summary>
    public sealed class PlanComparer : Comparer<Plan>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sort"></param>
        public PlanComparer(PlanSort sort)
        {
            Sort = sort;
        }

        /// <summary>
        /// Sort order (ascending, descending).
        /// </summary>
        public SortOrder Order { get; set; }

        /// <summary>
        /// Sort criteria.
        /// </summary>
        public PlanSort Sort { get; set; }

        /// <summary>
        /// Comparison function.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override int Compare(Plan x, Plan y)
        {
            // Swap variables for descending order
            if (Order == SortOrder.Descending)
            {
                Plan tmp = y;
                y = x;
                x = tmp;
            }

            // Compare plans
            switch (Sort)
            {
                case PlanSort.Description:
                    if (x != null && y != null)
                        return string.Compare(x.Description, y.Description,
                            StringComparison.CurrentCulture);
                    break;
                case PlanSort.Name:
                    if (x != null && y != null)
                        return string.Compare(x.Name, y.Name, StringComparison.CurrentCulture);
                    break;
                case PlanSort.Time:
                    if (x != null && y != null)
                    {
                        TimeSpan xtime = x.TotalTrainingTime;
                        TimeSpan ytime = y.TotalTrainingTime;
                        return TimeSpan.Compare(xtime, ytime);
                    }
                    break;
                case PlanSort.SkillsCount:
                    if (x != null && y != null)
                        return x.UniqueSkillsCount - y.UniqueSkillsCount;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return 0;
        }
    }
}
