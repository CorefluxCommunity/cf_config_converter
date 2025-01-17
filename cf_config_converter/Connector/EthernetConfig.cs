using System;
using Newtonsoft.Json.Linq;

namespace Coreflux.Tools.Connector;

public class EthernetConfig : BaseConfig
{
    public JObject JsonObject { get; set; }
    public EthernetConfig(JObject config) : base(config)
    {
        JsonObject = config;
    }

    public override void Process(string path)
    {
        throw new NotImplementedException();
    }
}
