using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Coreflux.Tools.Connector;

public class S7Config : BaseConfig
{
    public JObject JsonObject { get; set; }

    public S7Config(JObject config) : base(config)
    {
        JsonObject = config;
    }

    public override void Process(string path)
    {
        JObject newJson = Transform(JsonObject);

        string outputFilePath = Path.Combine(Path.GetDirectoryName(path)!, "cf_config_converted", "coreflux_s72mqtt_config.json");

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
            // REGEX for the variable!!
            var variableParameters = ParseVariable(tag["Variable"]!.ToString(), (string)tag["Name"]!);

            var newTag = new JObject
            {
                ["Name"] = tag["Name"],
                ["Route"] = (int)tag["WriteDirection"]!, // it's the same 0 and 1
                ["MqttTopic"] = tag["MQTTTopic"],
                ["QualityOfService"] = (int)tag["MQTTQoS"]!,
                ["MqttRetain"] = (bool)tag["MQTTRetain"]!,
                ["DataType"] = (int)tag["VariableType"]!, // the old variabletype is now DataType
                ["Behaviour"] = (int)tag["Behaviour"]!,
                //variable
                ["VariableType"] = (int)variableParameters["Type"],
                ["DataBlockValue"] = (int)variableParameters["DataBlockValue"],
                ["Byte"] = (int)variableParameters["Byte"],
                ["Bit"] = (int)variableParameters["Bit"],
                ["StringSize"] = (int)variableParameters["StringSize"],

                //publish
                ["Publish"] = 2
            };

            newTags.Add(newTag);
        }

        jsonObject["Tags"] = newTags;

        return jsonObject;
    }

    private static Dictionary<string, object> ParseVariable(string var, string name)
    {
        string dataBlockPattern = @"DB(?<DataBlockValue>\d+)\.(DBX|DBB|DBW|DBD)(?<Byte>\d+)(?:\.(?<Bit>\d+))?(?: STRING (?<StringSize>\d+))?";
        string qimPattern = @"(?<Type>[QIM])(?<Bit>\d+)\.(?<Byte>\d+)(?: STRING (?<StringSize>\d+))?";

        Match match = Regex.Match(var, dataBlockPattern);
        if (match.Success)
        {
            return new Dictionary<string, object>
            {
                ["Type"] = 3,
                ["DataBlockValue"] = int.Parse(match.Groups["DataBlockValue"].Value),
                ["Byte"] = int.Parse(match.Groups["Byte"].Value),
                ["Bit"] = match.Groups["Bit"].Success ? int.Parse(match.Groups["Bit"].Value) : 0,
                ["StringSize"] = match.Groups["StringSize"].Success ? int.Parse(match.Groups["StringSize"].Value) : 0
            };
        }

        match = Regex.Match(var, qimPattern);
        if (match.Success)
        {
            return new Dictionary<string, object>
            {
                ["Type"] = match.Groups["Type"].Value switch
                {
                    "M" => 0,
                    "Q" => 1,
                    "I" => 2,
                    _ => throw new ArgumentException($"Invalid type on tag {name}"),
                },
                ["DataBlockValue"] = 0,
                ["Byte"] = int.Parse(match.Groups["Byte"].Value),
                ["Bit"] = int.Parse(match.Groups["Bit"].Value),
                ["StringSize"] = match.Groups["StringSize"].Success ? int.Parse(match.Groups["StringSize"].Value) : 0
            };
        }

        throw new ArgumentException($"Invalid Variable format: {var}, on tag: {name}");
    }
}
