using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Common.Helpers;

namespace MACS_Game
{

    public class SocialSpace
    {
        readonly Spot[,] _space;
        public int Rows { get; private set; }
        public int Cols { get; private set; }
        public SocialSpace(int size)
        {
            Rows = Cols = size;
            _space = new Spot[Rows, Cols];
            for (int r = 0; r < size; r++)
                for (int c = 0; c < size; c++)
                    _space[r, c] = new Spot() { Row = r, Col = c };
        }

        public SocialSpace(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            _space = new Spot[Rows, Cols];

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    _space[r, c] = new Spot() { Row = r, Col = c };
        }

        // return Empty position 
        // countAgent is the ActiveAgent.Count
        // rows * cols - CountAgent is the size of the returned collection
        public List<(int, int)> GetEmptyPositions(int CountAgent) // returns empty spots
        {
            List<(int, int)> result = new List<(int, int)>(Rows * Cols - CountAgent);
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    if (_space[r, c].Agent == null)
                        result.Add((r, c));

            return result;
        }

        public Agent this[int row, int col]
        {
            get
            {
                return _space[row, col].Agent;
            }
            set
            {
                // when we assign an agent a place, we recalculate the cost for all places in the pool

                // if there is an agent(it's remove) at this place we take it into account
                Agent oldAgent = _space[row, col]?.Agent;

                int rowNeighbourSpot, colNeighbourSpot;

                for (int r = row - 1; r <= row + 1; r++)
                {
                    
                    rowNeighbourSpot = r;

                    // if we go beyond the bounds of the array, take a spot on the other side
                    if (rowNeighbourSpot < 0)
                        rowNeighbourSpot = Rows - 1;
                    if (rowNeighbourSpot >= Rows)
                        rowNeighbourSpot = 0;

                    for (int c = col - 1; c <= col + 1; c++)
                    {
                        colNeighbourSpot = c;

                        // if we go beyond the bounds of the array, take a spot on the other side
                        if (colNeighbourSpot < 0)
                            colNeighbourSpot = Cols - 1;
                        if (colNeighbourSpot >= Cols)
                            colNeighbourSpot = 0;

                        if (oldAgent != null) // old agent exists
                        {
                            if (oldAgent.Contrib)
                                _space[rowNeighbourSpot, colNeighbourSpot].NeighNumOfContrib--;
                            else
                                _space[rowNeighbourSpot, colNeighbourSpot].NeighNumOfNonContrib--;
                        }

                        // if the new agent is zero, it is just the removal of the old agent
                        if (value != null) // new agent exists
                        {
                            
                            if (value.Contrib)
                                _space[rowNeighbourSpot, colNeighbourSpot].NeighNumOfContrib++;
                            else
                                _space[rowNeighbourSpot, colNeighbourSpot].NeighNumOfNonContrib++;
                        }
                    }
                }

                // break the bond of the old agent with the spot
                if (oldAgent != null)
                {
                    oldAgent.Spot = null;
                    _space[row, col].Agent = null;
                }

                // associate agent with spot
                if (value != null)
                {
                    _space[row, col].Agent = value;
                    value.Spot = _space[row, col];
                }
            }
        }

        // this function return better spot than old spot or null if old spot the best
        public Spot GetBestSpot(double CurrentCost, // The current spot value must be less than the one found.
            bool isContrib // when searching for a new place, you need to consider the type of agent
            )
        {

            Spot cs;
            List<Spot> bestSpots = null; // this list will save the best places and give a random place

            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                {
                    if (_space[r, c].Agent != null)
                        continue;

                    cs = _space[r, c];
                    if (cs.Cost(isContrib) > CurrentCost) // the Cost(bool IsContib) returns spot cost value if we will add agent with IsContrib(true or false) type
                    {
                        CurrentCost = cs.Cost(isContrib); // current cost will keep new cost
                        bestSpots = new List<Spot>(); // clear list
                    }

                    if (cs.Cost(isContrib) == CurrentCost && bestSpots != null) // add new spot in list
                        bestSpots.Add(cs);
                }

            if (bestSpots != null) // there are better spots
                return bestSpots.RandomizeOne();

            return null;
        }

    }

    public class Agent
    {
        // I do not comment on the parameters of the Agent, because it is clear
        public String Name { get; set; }
        public bool Contrib { get; set; }
        public bool IsActive { get; set; } 
        public Spot Spot { get; set; }

        public double WellBeingAgent
        {
            get
            {
                return (Contrib ? 0 : Configuration.Endowment) + Spot.CalculateValue();
            }
        }

        public bool NeedToRemove //  remove
        {
            get
            {
                return (WellBeingAgent - Configuration.Disturbance) <= 0;
            }
        }
    }

    public class Spot
    {
        public Agent Agent { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public int NeighNumOfContrib { get; set; }
        public int NeighNumOfNonContrib { get; set; }


        // I would like to draw attention to these three functions.
        // CalculateValue()
        // Cost()
        // Cost(bool IsContrib)

        // CalculateValue()
        /* 
         * this function return SpotValue. The SpotValue is WellBeingAgent
         * 
         * 
         * we can use this function to compare spot value, but it is not rational 
         * because 
         * Configuration.Externalities * Configuration.Endowment * (ContribCount)/(ContribCount + NonContribCount)
         * Configuration.Externalities * Configuration.Endowment is constant for all spots
         * only (ContribCount)/(ContribCount + NonContribCount) will be different. this specific value characterizes the amount of spots
         * Consequently if (ContribCount) / (ContribCount + NonContribCount) of spot A more than (ContribCount) / (ContribCount + NonContribCount) of spot B
         * then SpotValue of spot A more than SpotValue of spot B
         * 
         * if matrix size = 1000 we have 1000000 spots, better use (ContribCount) / (ContribCount + NonContribCount) than Configuration.Externalities * Configuration.Endowment * (ContribCount)/(ContribCount + NonContribCount)
        
            function Cost() returns (ContribCount) / (ContribCount + NonContribCount)
            This function is used to compare spot values.
             
            function Cost(bool IsContrib) returns (ContribCount) / (ContribCount + NonContribCount) if add agent IsContrib type
             

            !!!!!!!!!!!!!!!!!!!!!!
            if the logic of costing changes, then we simply change the body of the functions. Cost () and Cost (bool)
             
             */
        public double CalculateValue() // It's SpotValue 
        {
            return Configuration.Externalities * Configuration.Endowment * Cost();
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


    public class OutputStats
    {
        public int Iteration { get; set; }
        public double PropOfContribInPop { get; set; }
        public double PropOfContripInAvePool { get; set; }
        public double DisturbanceSize { get; set; }
    }

    public static class Configuration
    {
        public static double Disturbance { get; set; } = 1; // The disturbance amount.
        public static double DisturbanceIncrement { get; set; } = 0.5; // The disturbance increment.
        public static int Endowment { get; set; } = 10; // The amount of coins each agent is allocated at the beginning of each iteration.

        public static double Externalities { get; set; } = 15;

        public static int NumOfIterations { get; set; } = 100; // Number of iterations.
        public static int NumOfAgents { get; set; } = 100; // The number of agents in a simulation. Must be equal to or greater than ten.
        public static double PropOfContrib { get; set; } = 0.30; // The proportion of agents that are contributors.
        public static double VacantSpots { get; set; } = 0.3; //The number of vacant spots in the SocialSpace(See Subsec. 2.1).
        public static int SocialSpaceSize { get; set; } = (int)Math.Round(NumOfAgents / (1 - VacantSpots), 0); // The number of rows and columns in the SocialSpace (See Subsec. 2.1). When necessary, round up to the nearest integer.

    }
}
