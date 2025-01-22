using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Coreflux.Tools.Connector;

public class ModbusConfig : BaseConfig
{
    public JObject JsonObject { get; set; }

    public ModbusConfig(JObject config) : base(config)
    {
        JsonObject = config;
    }
    public override void Process(string path)
    {
        JObject newJson = Transform(JsonObject);

        // HUB v1.3-

        string outputFilePath = Path.Combine(Path.GetDirectoryName(path)!, "cf_config_converted", "coreflux_modbus2mqtt_config.json");

        if (!Directory.Exists(Path.Combine(outputFilePath, "..")))
            Directory.CreateDirectory(Path.Combine(outputFilePath, ".."));

        File.WriteAllText(outputFilePath, JsonConvert.SerializeObject(newJson, Formatting.Indented));
        Console.WriteLine($"Transformed JSON saved to: {outputFilePath}.");

        // HUB v1.4+

        outputFilePath = Path.Combine(Path.GetDirectoryName(path)!, "cf_config_converted", "v1.4_coreflux_modbus2mqtt_config.json");

        JObject config14 = new JObject
        {
            ["_comment"] = "WARNING: DON'T DELETE OR MODIFY CONNECTORTYPE FIELD",
            ["connectorType"] = "coreflux_modbus2mqtt",
            ["config"] = newJson
        };

        File.WriteAllText(outputFilePath, JsonConvert.SerializeObject(config14, Formatting.Indented));
        Console.WriteLine($"Transformed JSON saved to: {outputFilePath}.");
    }

    private static JObject Transform(JObject jsonObject)
    {
        // MQTT Parameters
        // there should be 2 options, the old old and the not so old

        if (jsonObject["MqttParameters"] == null) // old old
        {
            jsonObject["MqttParameters"] = new JObject();
            jsonObject["MqttParameters"]!["Port"] = (int)jsonObject["MQTTParameters"]!["Port"]!;
            jsonObject["MqttParameters"]!["Address"] = jsonObject["MQTTParameters"]!["Address"]!;
            jsonObject["MqttParameters"]!["IsAnonymous"] = jsonObject["MQTTParameters"]!["IsAnonymous"];
            jsonObject["MqttParameters"]!["Username"] = jsonObject["MQTTParameters"]!["Username"];
            jsonObject["MqttParameters"]!["Password"] = jsonObject["MQTTParameters"]!["Password"];
            jsonObject["MqttParameters"]!["WithTLS"] = jsonObject["MQTTParameters"]!["EnableTLS"];
            jsonObject["MqttParameters"]!["ClientId"] = Guid.NewGuid().ToString("n").Substring(0, 8);
            jsonObject.Remove("MQTTParameters");

            //ModbusParameters don't change

            // Tags
            JArray old = (JArray)jsonObject["Tags"]!;
            JArray newTags = new JArray();

            foreach (var tag in old)
            {
                var newTag = new JObject
                {
                    ["Name"] = tag["Name"],
                    ["Route"] = (int)tag["WriteDirection"]! switch
                    {
                        0 => 1,
                        1 => 0,
                        _ => throw new ArgumentException($"WriteDirection not valid on tag {(string)tag["Name"]!}"),
                    },
                    ["MqttTopic"] = tag["MQTTTopic"],
                    ["QualityOfService"] = (int)tag["MQTTQoS"]!,
                    ["MqttRetain"] = (bool)tag["MQTTRetain"]!,
                    ["Publish"] = 2,
                    ["ModbusMemoryArea"] = tag["ModbusMemoryArea"]!,
                    ["ModbusMemoryAddress"] = (int)tag["ModbusMemoryAddress"]!,
                    ["ModbusDataType"] = tag["ModbusDataType"],
                    ["ModbusRegisterBit"] = tag["ModbusRegisterBit"] != null ? (int)tag["ModbusRegisterBit"]! : 0,
                    ["RegisterByte"] = tag["RegisterByte"] != null ? (int)tag["RegisterByte"]! : 0,
                    ["SwapByte"] = tag["SwapByte"] != null ? (bool)tag["SwapByte"]! : false,
                    ["SwapWord"] = tag["SwapWord"] != null ? (bool)tag["SwapWord"]! : false
                };

                newTags.Add(newTag);
            }

            jsonObject["Tags"] = newTags;

            return jsonObject;
        }
        else
        {
            // Tags
            JArray old = (JArray)jsonObject["Tags"]!;
            JArray newTags = new JArray();

            foreach (var tag in old)
            {
                var newTag = new JObject
                {
                    ["Name"] = tag["Name"],
                    ["Route"] = (int)tag["Route"]!, // it's the same 0 and 1
                    ["MqttTopic"] = tag["MqttTopic"],
                    ["QualityOfService"] = (int)tag["MqttQos"]!,
                    ["MqttRetain"] = (bool)tag["MqttRetain"]!,
                    //publish
                    ["Publish"] = tag["Publish"],
                    ["PublishCycle"] = tag["PublishCycle"] != null ? (int)tag["PublishCycle"]! : 0,
                    ["ModbusMemoryArea"] = tag["ModbusMemoryArea"]!,
                    ["ModbusMemoryAddress"] = (int)tag["ModbusMemoryAddress"]!,
                    ["ModbusDataType"] = tag["ModbusDataType"],
                    ["ModbusRegisterBit"] = tag["ModbusRegisterBit"] != null ? (int)tag["ModbusRegisterBit"]! : 0,
                    ["RegisterByte"] = tag["RegisterByte"] != null ? (int)tag["RegisterByte"]! : 0,
                    ["SwapByte"] = tag["SwapByte"] != null ? (bool)tag["SwapByte"]! : false,
                    ["SwapWord"] = tag["SwapWord"] != null ? (bool)tag["SwapWord"]! : false
                };

                newTags.Add(newTag);
            }

            jsonObject["Tags"] = newTags;

            return jsonObject;
        }


    }

}
