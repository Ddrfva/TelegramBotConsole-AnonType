using System.Collections.Generic;

namespace TelegramBot_27_2.Scenarios
{
    public class ScenarioContext
    {
        public ScenarioType CurrentScenario { get; set; }
        public string? CurrentStep { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();

        public ScenarioContext(ScenarioType scenario)
        {
            CurrentScenario = scenario;
        }
    }
}