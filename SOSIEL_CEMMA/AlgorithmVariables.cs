using SOSIEL.Helpers;

namespace SOSIEL_CEMMA
{
    /// <summary>
    /// Contains variable names used in code.
    /// </summary>
    public class AlgorithmVariables: SosielVariables
    {
        public const string AgentIncome = "Income";
        public const string AgentExpenses = "Expenses";
        public const string AgentSavings = "Savings";

        public const string HouseholdIncome = "HouseholdIncome";
        public const string HouseholdExpenses = "HouseholdExpenses";
        public const string HouseholdSavings = "HouseholdSavings";


        public const string Mean = "Mean";
        public const string StdDev = "StdDev";

        // CEMMA variables
        public const string AgentPersona = "AgentPersona";
        public const string CurrentSpotValue = "CurrentSpotValue";
        public const string BestSpotValue = "BestSpotValue";
        public const string Move = "Move";
        public const string ExtractedResource = "ExtractedResource";
        public const string SharedResourceValue = "SharedResourceValue";
    }
}
