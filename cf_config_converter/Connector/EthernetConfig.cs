using System;
using Newtonsoft.Json;
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
        JObject newJson = Transform(JsonObject);

        // HUB v1.3-

        string outputFilePath = Path.Combine(Path.GetDirectoryName(path)!, "cf_config_converted", "coreflux_ethernetipmqtt_config.json");

        if (!Directory.Exists(Path.Combine(outputFilePath, "..")))
            Directory.CreateDirectory(Path.Combine(outputFilePath, ".."));

        File.WriteAllText(outputFilePath, JsonConvert.SerializeObject(newJson, Formatting.Indented));
        Console.WriteLine($"Transformed JSON saved to: {outputFilePath}.");

        //HUB v1.4+
        
        outputFilePath = Path.Combine(Path.GetDirectoryName(path)!, "cf_config_converted", "v1.4_coreflux_ethernetipmqtt_config.json");

        JObject config14 = new JObject
        {
            ["_comment"] = "WARNING: DON'T DELETE OR MODIFY CONNECTORTYPE FIELD",
            ["connectorType"] = "coreflux_ethernetipmqtt",
            ["config"] = newJson
        };       

        File.WriteAllText(outputFilePath, JsonConvert.SerializeObject(config14, Formatting.Indented));
        Console.WriteLine($"Transformed JSON saved to: {outputFilePath}.");
    }

    private JObject Transform(JObject jsonObject)
    {
        // MQTT parameters
        jsonObject["MqttParameters"] = new JObject();
        jsonObject["MqttParameters"]!["Port"] = (int)jsonObject["MQTTParameters"]!["Port"]!;
        jsonObject["MqttParameters"]!["Address"] = jsonObject["MQTTParameters"]!["Address"]!;
        jsonObject["MqttParameters"]!["IsAnonymous"] = jsonObject["MQTTParameters"]!["IsAnonymous"];
        jsonObject["MqttParameters"]!["Username"] = jsonObject["MQTTParameters"]!["Username"];
        jsonObject["MqttParameters"]!["Password"] = jsonObject["MQTTParameters"]!["Password"];
        jsonObject["MqttParameters"]!["WithTLS"] = jsonObject["MQTTParameters"]!["EnableTLS"];
        jsonObject["MqttParameters"]!["ClientId"] = Guid.NewGuid().ToString("n").Substring(0, 8);

        jsonObject.Remove("MQTTParameters");

        // ethernet ip
        jsonObject["EthernetIpParameters"] = new JObject();
        jsonObject["EthernetIpParameters"]!["Ip"] = jsonObject["EthernetIPParameters"]!["IP"];
        jsonObject["EthernetIpParameters"]!["PollingMs"] = jsonObject["EthernetIPParameters"]!["PollingMs"] != null ? jsonObject["EthernetIPParameters"]!["PollingMs"] : 500;
        jsonObject["EthernetIpParameters"]!["PortEthernet"] = jsonObject["EthernetIPParameters"]!["PortEthernet"] != null ? jsonObject["EthernetIPParameters"]!["PortEthernet"] : 44818;

        jsonObject.Remove("EthernetIPParameters");

        // tags
        JArray old = (JArray)jsonObject["Tags"]!;
        JArray newTags = new JArray();

        foreach (var tag in old)
        {
            var newTag = new JObject
            {
                ["Name"] = tag["Name"]!,
                // swap values
                ["Route"] = (int)tag["WriteDirection"]! switch
                {
                    0 => 1,
                    1 => 0,
                    2 => 0,
                    _ => throw new ArgumentException($"WriteDirection not valid on tag {(string)tag["Name"]!}")
                }, 
                ["MqttTopic"] = tag["MQTTTopic"],
                ["QualityOfService"] = (int)tag["MQTTQoS"]!,
                ["MqttRetain"] = (bool)tag["MQTTRetain"]!,
                ["Publish"] = 2,
                ["DataType"] = (int)tag["DataType"]!, //required
                ["ClassID"] = tag["ClassID"] != null ? (int)tag["ClassID"]! : 0,
                ["InstanceID"] = tag["InstanceID"] != null ? (int)tag["InstanceID"]! : 0,
                ["AttributeID"] = tag["AttributeID"] != null ? (int)tag["AttributeID"]! : 0,
                ["AttributeIndex"] = tag["AttributeIndex"] != null ? (int)tag["AttributeIndex"]! : 0 , //required
                ["EnableJoinAttribute"] = tag["EnableJoinAttribute"] != null ? (bool)tag["EnableJoinAttribute"]! : false,
                ["Length"] = tag["Length"] != null ? (int)tag["Length"]! : 1,
                ["Bit"] = tag["Bit"] != null ? (int)tag["Bit"]! : 0,
                ["JoinClassID"] = tag["JoinClassID"] != null ? (int)tag["JoinClassID"]! : 0,
                ["JoinInstanceID"] = tag["JoinInstanceID"] != null ? (int)tag["JoinInstanceID"]! : 0,
                ["JoinAttributeID"] = tag["JoinAttributeID"] != null ? (int)tag["JoinAttributeID"]! : 0,
                ["JoinIndex"] = tag["JoinIndex"] != null ? (int)tag["JoinIndex"]! : 0,
                ["JoinLength"] = tag["JoinLength"] != null ? (int)tag["JoinLength"]! : 1,
                ["ClearNull"] = tag["ClearNull"] != null ? (bool)tag["ClearNull"]! : false
            };
            newTags.Add(newTag);
        }

        jsonObject["Tags"] = newTags;
        return jsonObject;
    }
}
