using Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MACS_Game
{
    class Program
    {
        static void Main(string[] args)
        {
            // For writing to html
            ExcelWrite exw = new ExcelWrite();

            // For writing to Excel
            FileWrite wf = new FileWrite(exw.FolderName);

            // This collection is for Agents
            List<Agent> ActiveAgents = new List<Agent>(Configuration.NumOfAgents);
            List<OutputStats> OutputStats = new List<OutputStats>(); // collects statistics

            // agent naming counter
            int agentName = 1;
            // Contribution count
            int contribCount = (int)(Configuration.NumOfAgents * Configuration.PropOfContrib);

            (int row, int col) currentPosition;

            // initialize agents (Name and Contribution)
            for (int i = 0; i < Configuration.NumOfAgents; i++)
                ActiveAgents.Add(new Agent() { Name = agentName++.ToString(), Contrib = i < contribCount, IsActive = true });

            // Initialize SocialSpace
            SocialSpace socialSpace = new SocialSpace(Configuration.SocialSpaceSize);

            #region Add Agents in SosialSpace 
            Random random = new Random();

            List<int> indexePlaces = Enumerable.Range(0, Configuration.SocialSpaceSize * Configuration.SocialSpaceSize).ToList();

            int positionItem;
            foreach (Agent agent in ActiveAgents)
            {
                positionItem = random.Next(0, indexePlaces.Count);
                socialSpace[indexePlaces[positionItem] / Configuration.SocialSpaceSize, indexePlaces[positionItem] % Configuration.SocialSpaceSize] = agent;

                indexePlaces.RemoveAt(positionItem);
                if (indexePlaces.Count == 0)
                    break;
            }
            #endregion

            // This collection will contain not active agents
            List<Agent> NotActive = new List<Agent>();

            
            bool isMove = false; // If no movement
            double CurrentCost; 
            Spot BestSpot;

            //wf.Add("<h3>RANDOM LOCATION</h3>");
            //wf.Add(sosialspace);

            DateTime start = DateTime.Now; // start date
            for (int i = 0; i < Configuration.NumOfIterations; i++)
            {
                wf.Add($"<h3>BEGIN ITERATION - {i + 1}</h3>");
                wf.Add("<br />---------------------------------------------------------------------------------------------------------------");
                isMove = false;
                ActiveAgents = ActiveAgents.OrderBy(e => random.Next()).ToList();
                foreach (Agent agent in ActiveAgents)
                {
                    CurrentCost = agent.Spot.Cost();
                    currentPosition = (agent.Spot.Row, agent.Spot.Col);

                    socialSpace[currentPosition.row, currentPosition.col] = null; // remove agent from socialspace for recalculation

                    BestSpot = socialSpace.GetBestSpot(CurrentCost, agent.Contrib); // get the best spot
                    if (BestSpot != null) // There is a better place
                    {
                        wf.Add($"Agent - {agent.Name}{(agent.Contrib?"(Contrib)":"")} from [{currentPosition.row},{currentPosition.col}] to [{BestSpot.Row},{BestSpot.Col}]");
                        socialSpace[BestSpot.Row, BestSpot.Col] = agent;
                        isMove = true;
                    }
                    else
                    {
                        socialSpace[currentPosition.row, currentPosition.col] = agent; // return to the old spot
                        wf.Add($"<h3 style='color:red'>Agent - {agent.Name}{(agent.Contrib ? "(Contrib)" : "")} (position[{currentPosition.row},{currentPosition.col}]) didn't move</h3><br />");
                    }

                    //if (BestSpot != null)
                        //wf.Add(socialSpace);
                }

                // Add OutputRelations and OutputAgentType for this iteration
                exw.OutputRelations(i + 1, socialSpace);
                exw.OutputAgentType(i + 1, ActiveAgents.OrderBy(e => Convert.ToInt32(e.Name)));


                // Add OutputStats

                // number of active agents
                int CountAllAgent = ActiveAgents.Count;
                // count of contrib active agent
                int ContribActive = ActiveAgents.Count(e => e.Contrib);

                // Add OutputStats of this iteration to OutputStats set
                OutputStats.Add(new OutputStats
                {
                    Iteration = i + 1,
                    PropOfContribInPop = Math.Round((double)ContribActive / CountAllAgent, 2),
                    PropOfContripInAvePool = Math.Round(ActiveAgents.Sum(e => e.Spot.Cost()) / CountAllAgent, 2),
                    DisturbanceSize = Configuration.Disturbance
                });

                wf.Add($"<h3>END ITERATION - {i + 1}</h3>");

                #region delete inactive clients
                wf.Add($"Disturbance- {Configuration.Disturbance}");
                wf.Add("Check WellBeingAgent");
                int el = ActiveAgents.Count;

                ActiveAgents.ForEach(e => {
                    if (e.NeedToRemove)
                        e.IsActive = false; });

                while (--el >= 0)
                    if (!ActiveAgents[el].IsActive)
                    {
                        // add line to html file
                        wf.Add($"Agent - {ActiveAgents[el].Name}{(ActiveAgents[el].Contrib ? "(Contrib)" : "")} died (position [{ActiveAgents[el].Spot.Row},{ActiveAgents[el].Spot.Col}])");
                        socialSpace[ActiveAgents[el].Spot.Row, ActiveAgents[el].Spot.Col] = null; // remove agent from socialspace

                        // NotActive is used nowhere, but you may need it later
                        NotActive.Add(ActiveAgents[el]); // dd inactive to set
                        ActiveAgents.RemoveAt(el);
                    }

                wf.Add("<br />"); // add line to html file
                #endregion


                #region add agents if < NumOfAgents
                if (Configuration.NumOfAgents > ActiveAgents.Count && ActiveAgents.Count > 0) // add agents if some died but if everyone died, do not add
                {
                    wf.Add("Add new agents");

                    // number of active agents after remove
                    CountAllAgent = ActiveAgents.Count;
                    // count of contrib active agent after remove
                    ContribActive = ActiveAgents.Count(e => e.Contrib);

                    // get empty positions for new agents
                    List<(int row, int col)> EmptyPosition = socialSpace.GetEmptyPositions(Configuration.NumOfAgents - ActiveAgents.Count);

                    #region Temp variables for action
                    int IndexSpot; // random spot in the social space for a new agent
                    int AgentIndex; // random position of the agent from the set of agents
                    Agent newAgent; // there will be a new agent
                    (int row, int col) position; // random spot position 
                    #endregion



                    while (ActiveAgents.Count < Configuration.NumOfAgents && EmptyPosition.Count > 0)
                    {
                        AgentIndex = random.Next(CountAllAgent + 1); // get random position of the agent from the set of agents

                        // if AgentIndex < ContribActive new Agent will be Contrib
                        newAgent = new Agent() { Name = agentName++.ToString(), Contrib = AgentIndex <= ContribActive, IsActive = true };

                        // Add
                        // get random spot in the social space for a new agent
                        IndexSpot = random.Next(EmptyPosition.Count + 1);
                        
                        position = EmptyPosition.ElementAt(IndexSpot); // get a row and column from a random spot in the socialspace for a new agent
                        socialSpace[position.row, position.col] = newAgent; // add new agent to socialspace
                        EmptyPosition.RemoveAt(IndexSpot); // remove an occupied position from the empty set
                        ActiveAgents.Add(newAgent); // add new agent to ActiveAgents set


                        // add line to html file
                        wf.Add($"Add Agent - {newAgent.Name}{(newAgent.Contrib ? "(Contrib)" : "")} to [{position.row},{position.col}]"); 
                    }

                    wf.Add("<br />");
                }



                #endregion

                if (!isMove || ActiveAgents.Count == 0) // nobody moved or everybody died
                    break;

                Configuration.Disturbance += Configuration.DisturbanceIncrement; // increase Disturbance
            }

            exw.OutputStats(OutputStats);
            Console.WriteLine($"spent time - {DateTime.Now - start}"); // Show the time spent
            //wf.Add(socialspace);
            
            wf.Close(); // close FileStream
        }
    }
}
