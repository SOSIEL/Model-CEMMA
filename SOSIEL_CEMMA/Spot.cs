using SOSIEL_CEMMA.Configuration;

namespace SOSIEL_CEMMA
{
    public class Spot : SOSIEL.Entities.Site
    {

        public Agent Agent { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public int NeighNumOfContrib { get; set; }
        public int NeighNumOfNonContrib { get; set; }

        /// <summary>
        /// Calculates the SpotValue.
        /// </summary>
        /// <returns>SpotValue. The SpotValue is WellBeingAgent.</returns>
        public double CalculateValue(bool IsContrib) 
        {
            return Algorithm.GameConfiguration.Endowment* Algorithm.GameConfiguration.Externalities* Cost(IsContrib);
        }

        public double CalculateValue()
        {
            return Algorithm.GameConfiguration.Endowment * Algorithm.GameConfiguration.Externalities * Cost();
        }

        public double Cost()
        {
            if (NeighNumOfContrib == 0)
                return 0;

            return (double)NeighNumOfContrib / (NeighNumOfContrib + NeighNumOfNonContrib);
        }

        public double Cost(bool IsContrib)
        {
            if (NeighNumOfContrib == 0 && !IsContrib)
                return 0;

            return (double)(NeighNumOfContrib + (IsContrib ? 1 : 0)) / (NeighNumOfContrib + NeighNumOfNonContrib + 1);
        }
    }
}
