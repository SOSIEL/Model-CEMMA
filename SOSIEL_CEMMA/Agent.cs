using System;
using System.Collections.Generic;
using System.Linq;
using SOSIEL.Entities;
using SOSIEL.Helpers;
using SOSIEL.Randoms;
using SOSIEL_CEMMA.Configuration;

namespace SOSIEL_CEMMA
{
    public sealed class Agent : SOSIEL.Entities.Agent
    {
        public AgentStateConfiguration AgentStateConfiguration { get; private set; }
        public CEMMAModel CEMMAConfiguration { get; private set; }

        // Agent's CEMMA data --------------------------------------------------
        //public String Name { get; set; }
        public bool Contrib { get; set; }
        public bool IsActive { get; set; }
        public Spot Spot { get; set; }

        /// <summary>The WellBeingAgent property represents the agents's resourses.</summary>
        /// <value>
        /// The WellBeingAgent property returns the value of the agents's resourses, 
        /// depending on the share/unshare state.
        /// </value>
        public double WellBeingAgent
        {
            get
            {
                return (Contrib ? 0 : CEMMAModel.Endowment) + Spot.CalculateValue(Contrib);
            }
        }

        /// <summary>The NeedToRemove property represents.</summary>
        /// <value>
        /// The NeedToRemove property returns TRUE if the agent has no resourses and must be removed.
        /// </value>
        public bool NeedToRemove
        {
            get
            {
                Console.WriteLine($"--------- Agent must be removed: {(WellBeingAgent - CEMMAModel.Disturbance) <= 0}");
                return (WellBeingAgent - CEMMAModel.Disturbance) <= 0;
            }
        }
        // END of the Agent's CEMMA data --------------------------------------------------

        public override SOSIEL.Entities.Agent Clone()
        {
            Agent agent = (Agent)base.Clone();

            return agent;
        }

        public override SOSIEL.Entities.Agent CreateChild(string gender)
        {
            Agent child = (Agent)base.CreateChild(gender);

            child[AlgorithmVariables.AgentIncome] = 0;
            child[AlgorithmVariables.AgentExpenses] = 0;
            child[AlgorithmVariables.AgentSavings] = 0;

            return child;
        }

        protected override SOSIEL.Entities.Agent CreateInstance()
        {
            return new Agent();
        }

        public void GenerateCustomParams()
        {
            
        }

        /// <summary>
        /// Creates agent instance based on agent prototype and agent configuration 
        /// </summary>
        /// <param name="agentConfiguration"></param>
        /// <param name="prototype"></param>
        /// <returns></returns>
        public static Agent CreateAgent(AgentStateConfiguration agentConfiguration, AgentPrototype prototype)
        {
            Agent agent = new Agent();

            agent.Prototype = prototype;
            agent.privateVariables = new Dictionary<string, dynamic>(agentConfiguration.PrivateVariables);

            agent.AssignedDecisionOptions = prototype.DecisionOptions.Where(r => agentConfiguration.AssignedDecisionOptions.Contains(r.Id)).ToList();
            agent.AssignedGoals = prototype.Goals.Where(g => agentConfiguration.AssignedGoals.Contains(g.Name)).ToList();

            agent.AssignedDecisionOptions.ForEach(kh => agent.DecisionOptionActivationFreshness.Add(kh, 1));

            //generate goal importance
            agentConfiguration.GoalsState.ForEach(kvp =>
            {
                var goalName = kvp.Key;
                var configuration = kvp.Value;

                var goal = agent.AssignedGoals.FirstOrDefault(g => g.Name == goalName);
                if (goal == null) return;

                double importance = configuration.Importance;

                if (configuration.Randomness)
                {
                    if (string.IsNullOrEmpty(configuration.BasedOn))
                    {
                        var from = (int)(configuration.RandomFrom * 10);
                        var to = (int)(configuration.RandomTo * 10);

                        importance = GenerateImportance(agent, configuration.RandomFrom, configuration.RandomTo);
                    }
                    else
                    {
                        var anotherGoalImportance = agent.InitialGoalStates[agent.AssignedGoals.FirstOrDefault(g => g.Name == configuration.BasedOn)]
                            .Importance;

                        importance = Math.Round(1 - anotherGoalImportance, 2);
                    }
                }

                GoalState goalState = new GoalState(configuration.Value, goal.FocalValue, importance);

                agent.InitialGoalStates.Add(goal, goalState);

                agent[string.Format("{0}_Importance", goal.Name)] = importance;
            });

            //initializes initial anticipated influence for each kh and goal assigned to the agent
            agent.AssignedDecisionOptions.ForEach(kh =>
            {
                Dictionary<string, double> source;

                if (kh.AutoGenerated && agent.Prototype.DoNothingAnticipatedInfluence != null)
                {
                    source = agent.Prototype.DoNothingAnticipatedInfluence;
                }
                else
                {
                    agentConfiguration.AnticipatedInfluenceState.TryGetValue(kh.Id, out source);
                }


                Dictionary<Goal, double> inner = new Dictionary<Goal, double>();

                agent.AssignedGoals.ForEach(g =>
                {
                    inner.Add(g, source != null && source.ContainsKey(g.Name) ? source[g.Name] : 0);
                });

                agent.AnticipationInfluence.Add(kh, inner);
            });


            InitializeDynamicVariables(agent);

            agent.AgentStateConfiguration = agentConfiguration;

            return agent;
        }

        private static void InitializeDynamicVariables(Agent agent)
        {
            agent[AlgorithmVariables.IsActive] = true;
        }

        private static double GenerateImportance(Agent agent, double min, double max)
        {
            double rand;

            if (agent.ContainsVariable(AlgorithmVariables.Mean) && agent.ContainsVariable(AlgorithmVariables.StdDev))
                rand = NormalDistributionRandom.GetInstance.Next(agent[AlgorithmVariables.Mean], agent[AlgorithmVariables.StdDev]);
            else
                rand = NormalDistributionRandom.GetInstance.Next();

            rand = Math.Round(rand, 1, MidpointRounding.AwayFromZero);

            if (rand < min || rand > max)
                return GenerateImportance(agent, min, max);

            return rand;
        }
    }
}
