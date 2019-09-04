using SOSIEL_CEMMA.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

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
            return CEMMAModel.Endowment * CEMMAModel.Externalities * Cost(IsContrib);
        }

        public double CalculateValue()
        {
            return CEMMAModel.Endowment * CEMMAModel.Externalities * Cost();
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
