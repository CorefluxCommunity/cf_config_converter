using System.Text.RegularExpressions;
using Coreflux.Tools.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Coreflux.Tools;
class Program
{
    public static void Main(string[] args)
    {
#if DEBUG
        string inputConfigFilePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "configs7old.json");
#else
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: cf_config_converter.exe <path>");
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

            BaseConfig config = Caracterize(jsonObject);
            config.Process(inputConfigFilePath);
            
            return;            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static BaseConfig Caracterize(JObject jsonObject)
    {
        if (jsonObject["SiemensParameters"] != null)
            return new S7Config(jsonObject);

        else if (jsonObject["ModbusParameters"] != null)
            return new ModbusConfig(jsonObject);

        else if (jsonObject["EthernetIPParameters"] != null)
            return new EthernetConfig(jsonObject);

        else
            throw new ArgumentException("Unrecognized configuration type.");
    }
}