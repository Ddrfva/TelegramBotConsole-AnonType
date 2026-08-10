using System.Collections.Generic;

namespace TelegramBot_29.TelegramBot.Scenarios
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