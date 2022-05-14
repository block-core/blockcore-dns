namespace Blockcore.Dns
{
    public class AgentSettings
    {
        public string DidKey { get; set; }

        public int IntervalMin { get; set; } = 5;

        public string[] Hosts { get; set; }

    }
}
