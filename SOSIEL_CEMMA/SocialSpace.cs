using System;
using System.Collections.Generic;
using SOSIEL_CEMMA.Helpers;


namespace SOSIEL_CEMMA
{
    public class SocialSpace
    {
        readonly Spot[,] _space;
        public int Rows { get; private set; }
        public int Cols { get; private set; }
        public int Size { get; }
        public SocialSpace(int size)
        {
            Size = Rows = Cols = size;
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
                                bool isContrib) // when searching for a new place, you need to consider the type of agent
        {

            Spot cs;
            List<Spot> bestSpots = null; // this list will save the best places and give a random place

            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                {
                    if (_space[r, c].Agent != null)
                        continue;

                    cs = _space[r, c];
                    // the Cost(bool IsContrib) returns spot cost value if we will add agent with IsContrib(true or false) type
                    if (cs.Cost(isContrib) > CurrentCost) 
                    {
                        CurrentCost = cs.Cost(isContrib); // current cost will keep new cost
                        bestSpots = new List<Spot>(); // clear list
                    }

                    // add new spot in list
                    if (cs.Cost(isContrib) == CurrentCost && bestSpots != null) 
                        bestSpots.Add(cs);
                }

            if (bestSpots != null && bestSpots.Count > 0)
            {
                // there are better spots
                return bestSpots.RandomizeOne();
            }
            return null;
        }
    }
}
