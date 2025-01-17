using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Coreflux.Tools;
class Program
{
    public static void Main(string[] args)
    {
#if DEBUG
        string inputConfigFilePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "config.json");
#else
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: dotnet run <path>");
            return;
        }

        string inputConfigFilePath = args[0];
#endif
        if (!File.Exists(inputConfigFilePath))
        {
            Console.WriteLine("No file found in the directory");
            return;
        }

        try
        {
            string json = File.ReadAllText(inputConfigFilePath);
            JObject jsonObject = JObject.Parse(json);
            JObject newJson = Transform(jsonObject);

            string outputFilePath = Path.Combine(Path.GetDirectoryName(inputConfigFilePath)!, Path.GetFileNameWithoutExtension(inputConfigFilePath) + "_transformed.json");

            File.WriteAllText(outputFilePath, JsonConvert.SerializeObject(newJson, Formatting.Indented));
            Console.WriteLine($"Transformed JSON saved to: {outputFilePath}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
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
        jsonObject["SiemensParameters"]!["ConnectionType"] = (int)jsonObject["SiemensParameters"]!["ConnectionResource"]!;
        ((JObject)jsonObject["SiemensParameters"]!).Property("ConnectionResource")!.Remove();

        // Tags
        JArray old = (JArray)jsonObject["Tags"]!;
        JArray newTags = new JArray();

        foreach (var tag in old)
        {
            // REGEX for the variable!!
            var variableParameters = ParseVariable(tag["Variable"]!.ToString());

            var newTag = new JObject
            {
                ["Name"] = tag["Name"],
                ["Route"] = (int)tag["WriteDirection"]!, // it's the same 0 and 1
                ["MqttTopic"] = tag["MQTTTopic"],
                ["QualityOfService"] = (int)tag["MQTTQoS"]!,
                ["MqttRetain"] = (int)tag["MQTTRetain"]!,
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

    private static Dictionary<string, object> ParseVariable(string var)
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
                ["Type"] = match.Groups["Type"].Value switch{
                    "M" => 0,
                    "Q" => 1,
                    "I" => 2,
                    _ => throw new ArgumentException("Invalid type"),
                },
                ["DataBlockValue"] = 0,
                ["Byte"] = int.Parse(match.Groups["Byte"].Value),
                ["Bit"] = int.Parse(match.Groups["Bit"].Value),
                ["StringSize"] = match.Groups["StringSize"].Success ? int.Parse(match.Groups["StringSize"].Value) : 0
            };
        }

        throw new ArgumentException($"Invalid Variable format: {var}");
    }
}