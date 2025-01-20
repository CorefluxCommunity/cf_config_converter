using System;
using Newtonsoft.Json;
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
         JObject newJson = Transform(JsonObject);

        string outputFilePath = Path.Combine(Path.GetDirectoryName(path)!, "cf_config_converted", "coreflux_modbus2mqtt_config.json");

        if (!Directory.Exists(Path.Combine(outputFilePath, "..")))
            Directory.CreateDirectory(Path.Combine(outputFilePath, ".."));

        File.WriteAllText(outputFilePath, JsonConvert.SerializeObject(newJson, Formatting.Indented));
        Console.WriteLine($"Transformed JSON saved to: {outputFilePath}.");
    }

    private static JObject Transform(JObject jsonObject)
    {
        // MQTT Parameters
        // since the name is different for the new version
        jsonObject["MqttParameters"] = new JObject();

        jsonObject["MqttParameters"]!["Port"] = (int)jsonObject["MQTTParameters"]!["Port"]!;
        jsonObject["MqttParameters"]!["Address"] = jsonObject["MQTTParameters"]!["Address"]!;
        jsonObject["MqttParameters"]!["IsAnonymous"] = jsonObject["MQTTParameters"]!["IsAnonymous"];
        jsonObject["MqttParameters"]!["Username"] = jsonObject["MQTTParameters"]!["Username"];
        jsonObject["MqttParameters"]!["Password"] = jsonObject["MQTTParameters"]!["Password"];
        jsonObject["MqttParameters"]!["WithTLS"] = jsonObject["MQTTParameters"]!["EnableTLS"];
        jsonObject["SiemensParameters"]!["Debug"] = jsonObject["MQTTParameters"]!["EnableDebugTopic"];
        jsonObject["SiemensParameters"]!["DebugTopic"] = jsonObject["MQTTParameters"]!["DebugTopic"];
        jsonObject["MqttParameters"]!["ClientId"] = Guid.NewGuid().ToString("n").Substring(0, 8);

        jsonObject.Remove("MQTTParameters");

        // Siemens Parameters

        jsonObject["SiemensS7Parameters"] = new JObject();
        jsonObject["SiemensS7Parameters"]!["PLCRack"] = (int)jsonObject["SiemensParameters"]!["PLCRack"]!;
        jsonObject["SiemensS7Parameters"]!["PLCSlot"] = (int)jsonObject["SiemensParameters"]!["PLCSlot"]!;
        jsonObject["SiemensS7Parameters"]!["Retries"] = (int)jsonObject["SiemensParameters"]!["Retries"]!;
        jsonObject["SiemensS7Parameters"]!["RetryTimeInSeconds"] = (int)jsonObject["SiemensParameters"]!["RetryTimeInSeconds"]!;
        jsonObject["SiemensS7Parameters"]!["Debug"] = (bool)jsonObject["SiemensParameters"]!["Debug"]!;
        jsonObject["SiemensS7Parameters"]!["DebugTopic"] = jsonObject["SiemensParameters"]!["DebugTopic"]! != null ? jsonObject["SiemensParameters"]!["DebugTopic"]! : "";
        jsonObject["SiemensS7Parameters"]!["IP"] = jsonObject["SiemensParameters"]!["IP"]!;
        jsonObject["SiemensS7Parameters"]!["ConnectionType"] = (int)jsonObject["SiemensParameters"]!["ConnectionResource"]!;
        jsonObject["SiemensS7Parameters"]!["RefreshTimeInMs"] = (int)jsonObject["SiemensParameters"]!["RefreshTimeInMs"]!;
        jsonObject.Remove("SiemensParameters");

        // Tags
        JArray old = (JArray)jsonObject["Tags"]!;
        JArray newTags = new JArray();

        foreach (var tag in old)
        {
            var newTag = new JObject
            {
                ["Name"] = tag["Name"],
                ["Route"] = (int)tag["WriteDirection"]!, // it's the same 0 and 1
                ["MqttTopic"] = tag["MQTTTopic"],
                ["QualityOfService"] = (int)tag["MQTTQoS"]!,
                ["MqttRetain"] = (bool)tag["MQTTRetain"]!,
                 //publish
                ["Publish"] = 2
            };

            newTags.Add(newTag);
        }

        jsonObject["Tags"] = newTags;

        return jsonObject;
    }

}
