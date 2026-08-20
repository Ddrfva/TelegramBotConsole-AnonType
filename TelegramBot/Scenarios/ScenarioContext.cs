using System;
using System.Collections.Generic;
using TelegramBot_31.Scenarios;

namespace TelegramBot_31.Scenarios
{
    public class ScenarioContext
    {
        public ScenarioType CurrentScenario { get; set; }
        public string? CurrentStep { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime CreatedAt { get; } = DateTime.UtcNow;

        public ScenarioContext(ScenarioType scenario)
        {
            CurrentScenario = scenario;
        }
    }
}