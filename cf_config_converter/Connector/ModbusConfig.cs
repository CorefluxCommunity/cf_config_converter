using System;
using Coreflux.Tools.Connector;
using Newtonsoft.Json.Linq;

namespace Coreflux.Tools.Connector;

public class ModbusConfig : BaseConfig
{
    public JObject JsonObject { get; set; }

    public ModbusConfig(JObject config) : base(config){
        JsonObject = config;
    }
    public override void Process(string path)
    {
        throw new NotImplementedException();
    }
}
