using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SOSIEL.Algorithm;
using SOSIEL.Configuration;
using SOSIEL.Entities;
using SOSIEL.Exceptions;
using SOSIEL.Helpers;
using Newtonsoft.Json;
using SOSIEL.Processes;
using SOSIEL.Randoms;
using SOSIEL_CEMMA.Configuration;
using SOSIEL_CEMMA.Helpers;
using SOSIEL_CEMMA.Output;

namespace SOSIEL_CEMMA
{
    public sealed class Algorithm : SosielAlgorithm<Spot>, IAlgorithm
    {
        public string Name { get { return "SOSIEL_CEMMA"; } }

        public static CEMMAModel GameConfiguration;
        string _gameComfigurationFileName = "game.configuration.json";
        // NOTE: for changing game settings to "Greedy Agent" - pay attention to regions (I), (II) and (III) below

        private SocialSpace socialSpace;

        string _outputFolder;

        private List<OutputStats> outputStats = new List<OutputStats>(); // collects statistics
        // For writing to Excel
        private static ExcelWrite exw;
        // For writing to html
        private FileWrite wf;

        ConfigurationModel _configuration;
        private AgentPrototype prototype;
        private int TotalNumberOfAgents = 0;
        private int generalAgentIndex = 1;

        bool isAnyMove = false; // no movement

        public static ProcessesConfiguration GetProcessConfiguration()
        {
            return new ProcessesConfiguration
            {
                ActionTakingEnabled = true,
                AnticipatoryLearningEnabled = false, //
                DecisionOptionSelectionEnabled = true,
                DecisionOptionSelectionPart2Enabled = false,
                SocialLearningEnabled = false,
                CounterfactualThinkingEnabled = false,
                InnovationEnabled = false,
                ReproductionEnabled = false,
                AgentRandomizationEnabled = true,
                AgentsDeactivationEnabled = false, //
                AlgorithmStopIfAllAgentsSelectDoNothing = true
            };
        }

        public Algorithm(ConfigurationModel configuration) : base(1, GetProcessConfiguration())
        {
            _configuration = configuration;

            _outputFolder = @"Output/";

            if (Directory.Exists(_outputFolder) == false)
                Directory.CreateDirectory(_outputFolder);

            exw = new ExcelWrite(String.Concat(_outputFolder, "stat"));
            wf = new FileWrite(_outputFolder);
        }

        public string Run()
        {
            Initialize();

            var sites = new Spot[] { };

            foreach (int iteration in Enumerable.Range(1, _configuration.AlgorithmConfiguration.NumberOfIterations))
            {
                if (algorithmStoppage)
                {
                    break;
                }
                RunSosiel(sites);
            }

            wf.Close();

            return _outputFolder;

        }

        /// <summary>
        /// Executes algorithm initialization
        /// </summary>
        public void Initialize()
        {
            string _gameConfigurationFilePath = Path.Combine(Directory.GetCurrentDirectory(), _gameComfigurationFileName);
            Console.WriteLine($"Reading game configuration from {_gameConfigurationFilePath}");
            string game_config_json = File.ReadAllText(_gameConfigurationFilePath);
            GameConfiguration = JsonConvert.DeserializeObject<CEMMAModel>(game_config_json);
            // Console.ReadKey();

            InitializeAgents();
            InitializeSocialSpace();
            InitializeProbabilities();

            if (_configuration.AlgorithmConfiguration.UseDimographicProcesses)
            {
                UseDemographic();
            }

            AfterInitialization();
            Console.WriteLine("Initialization complete....");
        }

        protected override void UseDemographic()
        {
            base.UseDemographic();

            demographic = new Demographic<Spot>(_configuration.AlgorithmConfiguration.DemographicConfiguration,
                probabilities.GetProbabilityTable<int>(AlgorithmProbabilityTables.BirthProbabilityTable),
                probabilities.GetProbabilityTable<int>(AlgorithmProbabilityTables.DeathProbabilityTable));
        }

        /// <inheritdoc />
        protected override void InitializeAgents()
        {
            var agents = new List<IAgent>();

            Dictionary<string, AgentPrototype> agentPrototypes = _configuration.AgentConfiguration;

            if (agentPrototypes.Count == 0)
            {
                throw new SosielAlgorithmException("Agent prototypes were not defined. See configuration file");
            }

            InitialStateConfiguration initialState = _configuration.InitialState;

            var networks = new Dictionary<string, List<SOSIEL.Entities.Agent>>();


            //create agents, groupby is used for saving agents numeration, e.g. CEA1, FE1, HM1. HM2 etc
            initialState.AgentsState.GroupBy(state => state.PrototypeOfAgent).ForEach((agentStateGroup) =>
            {
                prototype = agentPrototypes[agentStateGroup.Key];

                var mentalProto = prototype.MentalProto;

                agentStateGroup.ForEach((agentState) =>
                {
                    for (int i = 0; i < agentState.NumberOfAgents; i++)
                    {
                        Agent agent = Agent.CreateAgent(agentState, prototype);
                        agent.SetId(generalAgentIndex);
                        agent.IsActive = true;

                        agentState.PrivateVariables.ForEach((privateVariable) => {
                            if (privateVariable.Value == "Sharer")
                            {
                                agent.Contrib = true;
                            }
                            else agent.Contrib = false;
                        });
                        agents.Add(agent);

                        generalAgentIndex++;
                    }
                });
            });

            agentList = new AgentList(agents, agentPrototypes.Select(kvp => kvp.Value).ToList());
            Console.WriteLine("---Agents are initialized");
        }

        protected void InitializeSocialSpace()
        {
            _configuration.InitialState.AgentsState.GroupBy(number => number.NumberOfAgents).ForEach((agentStateGroup) =>
            {
                agentStateGroup.ForEach((agentState) =>
                {
                    TotalNumberOfAgents = TotalNumberOfAgents + agentState.NumberOfAgents;
                });
            });
            var socialSpaceSize = (int)System.Math.Round(TotalNumberOfAgents / (1 - CEMMAModel.VacantSpots), 0);
            socialSpace = new SocialSpace(socialSpaceSize);
            Console.WriteLine($"---SocialSpace is constructed. Size: {socialSpaceSize}");
        }

        private void InitializeProbabilities()
        {
            var probabilitiesList = new Probabilities();

            foreach (var probabilityElementConfiguration in _configuration.AlgorithmConfiguration.ProbabilitiesConfiguration)
            {
                var variableType = VariableTypeHelper.ConvertStringToType(probabilityElementConfiguration.VariableType);
                var parseTableMethod = ReflectionHelper.GetGenerecMethod(variableType, typeof(ProbabilityTableParser), "Parse");

                dynamic table = parseTableMethod.Invoke(null, new object[] { probabilityElementConfiguration.FilePath, probabilityElementConfiguration.WithHeader });

                var addToListMethod =
                    ReflectionHelper.GetGenerecMethod(variableType, typeof(Probabilities), "AddProbabilityTable");

                addToListMethod.Invoke(probabilitiesList, new object[] { probabilityElementConfiguration.Variable, table });
            }

            probabilities = probabilitiesList;
            Console.WriteLine("---Probabilities initialized");
        }

        protected override void AfterInitialization()
        {
            base.AfterInitialization();
        }

        /// <inheritdoc />
        protected override Dictionary<IAgent, AgentState<Spot>> InitializeFirstIterationState()
        {
            var states = new Dictionary<IAgent, AgentState<Spot>>();

            Random random = new Random();
            List<int> indexePlaces = Enumerable.Range(0, socialSpace.Size * socialSpace.Size).ToList();
            int positionItem;

            agentList.Agents.ForEach(agent =>
            {
                //creates empty agent state
                AgentState<Spot> agentState = AgentState<Spot>.Create(agent.Prototype.IsSiteOriented);

                // ---
                positionItem = random.Next(0, indexePlaces.Count);
                socialSpace[indexePlaces[positionItem] / socialSpace.Size, indexePlaces[positionItem] % socialSpace.Size] = (Agent)agent;
                indexePlaces.RemoveAt(positionItem);
                if (indexePlaces.Count == 0) throw new Exception("Social Space is overcrowded!");
                // ---

                //copy generated goal importance
                agent.InitialGoalStates.ForEach(kvp =>
                {
                    var goalState = kvp.Value;
                    goalState.Value = agent[kvp.Key.ReferenceVariable];

                    agentState.GoalsState[kvp.Key] = goalState;
                });

                states.Add(agent, agentState);
            });

            return states;
        }

        protected override void Maintenance()
        {
            base.Maintenance();

            // increase Disturbance
            CEMMAModel.Disturbance += CEMMAModel.DisturbanceIncrement;
            if (algorithmStoppage)
            {
                wf.Close();
                return;
            }

        }


        protected override void BeforeActionSelection(IAgent _agent, Spot site)
        {

            Random random = new Random();
            var agent = (Agent)_agent;
            site = agent.Spot;
            bool isContrib = (bool)agent.Contrib;
            double CurrentCost = site.Cost(isContrib);

            // comment the following region for Greedy Agent settings of the game (do not forget to uncomment regions (II) and (III))
            /**/
            #region (I) - simple configuration of the game
            agent.WellBeingAgent = (agent.Contrib ? 0 : CEMMAModel.Endowment)
                 + site.CalculateValue(agent.Contrib);
            Console.WriteLine($"--- Agent {agent.Id} Current Resource - {agent.WellBeingAgent}");
            #endregion
            /**/

            // remove agent from socialspace for recalculation

            Console.WriteLine($"--- Agent <{agent.Id}> {(isContrib ? "{Sharer}" : "{NonSharer}")}");

            Spot BestSpot = socialSpace.GetBestSpot(CurrentCost, isContrib);
            agent[AlgorithmVariables.CurrentSpotValue] = site.Cost(isContrib);


            if (BestSpot != null)
            {
                agent.TargetSpot = BestSpot;
                Console.WriteLine($"--- Best spot = {BestSpot.Col} x {BestSpot.Row}");
                agent[AlgorithmVariables.BestSpotValue] = BestSpot.Cost(isContrib);

            }
            else
            {
                agent[AlgorithmVariables.BestSpotValue] = site.Cost(isContrib);
                Console.WriteLine($"--- No Spot better than current - {site.Cost(isContrib)}");
                Console.WriteLine($"--- Current cost - {CurrentCost}");
            }
            Console.WriteLine();
            //wf.Add(socialSpace);
        }

        protected override void AfterActionTaking(IAgent _agent, Spot site)
        {
            var agent = (Agent)_agent;
            site = agent.Spot;
            (int row, int col) currentPosition = (site.Row, site.Col);

            bool isMove = agent[AlgorithmVariables.Move];

            if (isMove && agent.TargetSpot != null && socialSpace[agent.TargetSpot.Row, agent.TargetSpot.Col] == null)
            {
                socialSpace[currentPosition.row, currentPosition.col] = null;
                socialSpace[agent.TargetSpot.Row, agent.TargetSpot.Col] = agent;
                Console.WriteLine($"Agent <{agent.Id}> {(agent.Contrib ? "{Sharer}" : "{NonSharer}")} moved from [{currentPosition.row},{currentPosition.col}] to [{agent.TargetSpot.Row},{agent.TargetSpot.Col}]");
                wf.Add($"Agent &lt;<b>{agent.Id}</b>&gt; {(agent.Contrib ? "<i>Sharer</i>" : "NonSharer")} <span style='color:green'>moved</span> from [{currentPosition.row},{currentPosition.col}] to [{agent.TargetSpot.Row},{agent.TargetSpot.Col}]. Resource in possesion: {agent.WellBeingAgent}<br />");
                isAnyMove = true;
                agent[AlgorithmVariables.Move] = false;
                agent.TargetSpot = null;
            }
            else
            {
                socialSpace[currentPosition.row, currentPosition.col] = agent; // return to the old spot
                Console.WriteLine($"Agent <{agent.Id}> {(agent.Contrib ? "{Sharer}" : "{NonSharer}")} (position[{currentPosition.row},{currentPosition.col}]) didn't move");
                wf.Add($"Agent &lt;<b>{agent.Id}</b>&gt; {(agent.Contrib ? "<i>Sharer</i>" : "NonSharer")} <span style='color:red'>stayed</span> at position [{currentPosition.row},{currentPosition.col}]. Resource in possesion: {agent.WellBeingAgent}<br />");
            }
        }

        protected override void PreIterationCalculations(int iteration)
        {
            isAnyMove = false;
        }

        protected override void PreIterationStatistic(int iteration)
        {
            Console.WriteLine();
            Console.WriteLine((string)"Starting iteration {0}", (object)iteration);
            Console.WriteLine();
            wf.Add("<br /><br />");
            wf.Add($"<u>Iteration <b>{iteration}</b></u>");
            wf.Add($" <br /> &nbsp; &nbsp; &nbsp; &nbsp;Disturbance rate: {CEMMAModel.Disturbance}");
            wf.Add("<br />");
        }

        protected override void PostIterationCalculations(int iteration)
        {
            // uncomment the following region for Greedy Agent settings of the game (do not forget to comment region (I) and uncomment region (III))
            /*
            #region (II) - Greedy Agent configuration of the game 
            Console.WriteLine();
            foreach (Agent agent in agentList.Agents)
            {
                agent.WellBeingAgent = agent.WellBeingAgent - (agent.Contrib ? GameConfiguration.Endowment: 0) 
                    + agent.Spot.CalculateValue(agent.Contrib);
                Console.WriteLine($"--- Agent {agent.Id} Current Resource - {agent.WellBeingAgent}");
            }
            #endregion 
            */

            Console.WriteLine();
            Console.WriteLine($"--- Endowment: {CEMMAModel.Endowment} Disturbance: {CEMMAModel.Disturbance}");
            Console.WriteLine($"--- Movements: {isAnyMove}");
            Console.WriteLine();

            foreach (Agent a in agentList.Agents)
            {
                a.WellBeingAgent = a.WellBeingAgent - CEMMAModel.Disturbance;
                a[AlgorithmVariables.Move] = false;
            }


            #region remove inactive agents
            List<Agent> activeAgents = agentList.Agents.ConvertAll(a => (Agent)a);

            int el = agentList.Agents.Count;

            activeAgents.ForEach(e => {
                if (e.WellBeingAgent <= 0)
                {
                    e.IsActive = false;
                    Console.WriteLine($"--- Agent {e.Id} Current Resource - {e.WellBeingAgent}");
                }
            });
            wf.Add("<br />");
            while (--el >= 0) // ---------
                if (!activeAgents[el].IsActive)
                {
                    Console.WriteLine($"Agent - {activeAgents[el].Id} {(activeAgents[el].Contrib ? "{Sharer}" : "{NonSharer}")} died (position [{activeAgents[el].Spot.Row},{activeAgents[el].Spot.Col}]). Resource in possesion: {activeAgents[el].WellBeingAgent}");
                    // add line to html file
                    wf.Add($"Agent &lt;<b>{activeAgents[el].Id}</b>&gt; { (activeAgents[el].Contrib ? "<i>Sharer</i>" : "NonSharer")} died (at position [{ activeAgents[el].Spot.Row},{ activeAgents[el].Spot.Col}]). Resource in possesion: {activeAgents[el].WellBeingAgent}<br />");
                    socialSpace[activeAgents[el].Spot.Row, activeAgents[el].Spot.Col] = null; // remove agent from socialspace
                    activeAgents.RemoveAt(el);
                    agentList.Agents.RemoveAt(el);
                }

            wf.Add("<br />"); // add line to html file
            #endregion

            // uncomment the following region for Greedy Agent settings of the game (do not forget to comment region (I) and uncomment region (II)) 
            /*
            #region (III) - Greedy Agent configuration of the game
            if (!isAnyMove || activeAgents.Count == 0 && iteration > 1) // nobody moved or everybody died; condition (iteration > 1) is for ignoring the first iteration so that agents who did not find the group in the 1st iteration do not die
            #endregion
            */
            if (!isAnyMove || activeAgents.Count == 0) // nobody moved or everybody died
            {
                algorithmStoppage = true;
                Console.WriteLine();
                Console.WriteLine($"AlgorithmStoppage: {algorithmStoppage}");
                return;
            }

            if (TotalNumberOfAgents > agentList.Agents.Count && agentList.Agents.Count > 0)
            {
                wf.Add(" &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;Add new agents");
                Console.WriteLine();
                Console.WriteLine("   ---   Add new agents");

                // number of active agents after remove
                int countActiveAgents = agentList.Agents.Count;

                // get empty positions for new agents
                List<(int row, int col)> EmptyPosition = socialSpace.GetEmptyPositions(TotalNumberOfAgents - countActiveAgents);

                #region Temp variables for action
                int IndexSpot; // random spot in the social space for a new agent
                int AgentIndex; // random position of the agent from the set of agents
                Agent newAgent; // there will be a new agent
                (int row, int col) position; // random spot position 
                #endregion



                /**
                 * NC: If the number of agents on the landscape is less than the initial number of agents, 
                 * the missing agents are replaced using the current distribution of agents 
                 * and are then randomly distributed on the landscape in unoccupied spots. 
                 */
                int countAgentsBeforeAdd = agentList.Agents.Count;
                int countSharers = 0;
                foreach (Agent a in agentList.Agents)
                {
                    if (a.Contrib) countSharers++;
                }
                double SharersToAgentsBeforeAdd = (double)countSharers / countAgentsBeforeAdd;
                double cds = (TotalNumberOfAgents - agentList.Agents.Count) * SharersToAgentsBeforeAdd;
                int cdsInt = (int)Math.Round(cds, MidpointRounding.AwayFromZero);
                while (agentList.Agents.Count < TotalNumberOfAgents && EmptyPosition.Count > 0)
                {
                    // get random position of the agent from the set of agents
                    Random random = new Random();
                    AgentIndex = generalAgentIndex++;
                    newAgent = Agent.CreateAgent(_configuration.InitialState.AgentsState.First(), prototype);
                    newAgent.SetId(AgentIndex);

                    //set the newAgent.Contrib as Sharer or NonSharer using the current distribution of agents;
                    //newAgent.Contrib = random.NextDouble() < (double)countSharers / countAgentsBeforeAdd;
                    newAgent.Contrib = (cdsInt > 0) ? true : false;
                    cdsInt--;
                    newAgent.IsActive = true;
                    AgentState<Spot> agentState = AgentState<Spot>.Create(newAgent.Prototype.IsSiteOriented);

                    //copy generated goal importance
                    newAgent.InitialGoalStates.ForEach(kvp =>
                    {
                        var goalState = kvp.Value;
                        goalState.Value = newAgent[kvp.Key.ReferenceVariable];

                        agentState.GoalsState[kvp.Key] = goalState;
                    });
                    iterations.Last.Value.Add(newAgent, agentState);

                    // get random spot in the social space for a new agent
                    IndexSpot = random.Next(EmptyPosition.Count + 1);
                    position = EmptyPosition.ElementAt(IndexSpot); // get a row and column from a random spot in the socialspace for a new agent
                    socialSpace[position.row, position.col] = newAgent; // add new agent to socialspace
                    EmptyPosition.RemoveAt(IndexSpot); // remove an occupied position from the empty set
                    agentList.Agents.Add(newAgent); // add new agent to ActiveAgents set

                    Console.WriteLine($"Add Agent <{newAgent.Id}> {(newAgent.Contrib ? "{Sharer}" : "{NonSharer}")} to [{position.row},{position.col}]");

                    // add line to html file
                    wf.Add($"Add Agent &lt;{newAgent.Id}&gt; {(newAgent.Contrib ? "<i>Sharer</i>" : "NonSharer")} to [{position.row},{position.col}]");
                }

                wf.Add("<br />"); // add line to html file
            }
        }

        /// <inheritdoc />
        protected override void PostIterationStatistic(int iteration)
        {
            // Add OutputStats

            // number of active agents
            int countActiveAgents = agentList.Agents.Count;
            int countSharers = 0;
            foreach (Agent a in agentList.Agents) { if (a.Contrib) countSharers++; }

            // Add OutputStats of this iteration to OutputStats set
            double sum = 0;
            if (agentList.Agents != null && agentList.Agents.Count > 0)
            {
                foreach (Agent a in agentList.Agents)
                {
                    sum = sum + a.Spot.Cost();
                }
            }

            // Add OutputRelations and OutputAgentType for this iteration
            exw.OutputRelations(iteration, socialSpace);
            exw.OutputAgentType(iteration, agentList.Agents.ConvertAll(a => (Agent)a).OrderBy(e => e.Id));

            outputStats.Add(new OutputStats
            {
                Iteration = iteration,
                PropOfContribInPop = Math.Round((double)countSharers / countActiveAgents, 2),
                PropOfContripInAvePool = Math.Round(sum / countActiveAgents, 2),
                DisturbanceSize = CEMMAModel.Disturbance
            });

            exw.OutputStats(outputStats);
            Console.WriteLine("-------------------------------------------------------------");
        }

    }
}