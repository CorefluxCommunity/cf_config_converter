using System;
using Newtonsoft.Json.Linq;

namespace Coreflux.Tools.Connector;

public abstract class BaseConfig
{
    public JObject JsonObject { get; }

    protected BaseConfig(JObject config)
    {
        JsonObject = config;
    }

    public abstract void Process(string path);
}
