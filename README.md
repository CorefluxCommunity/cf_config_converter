# Coreflux Configuration Conversion Tool

### Description

The Coreflux Configuration Conversion Tool allows for an easily conversion of old versions of connectors' configurations into the new updated configuration parameters.

### Prerequisites
- The older configuration's file path;
- Coreflux HUB v1.3 or HUBLESS;

### Setup instructions
1. Go to the *Releases* tab;
2. Download the zip file for your OS;
3. Extract the contents;
4. Open the command line in the directory of the file.

### Running the Tool
On windows, run this script:
```
cf_config_converter.exe <configuration_file_path>
```

On linux, run this script:
```
./cf_config_converter <configuration_file_path>
```
where configuration_file_path is the path for the configuration you want to convert.

### Output
#### Success
If the configuration is completed with success, the following message will appear in the console:
```
Transformed JSON saved to: <path_1.3>
Transformed JSON saved to: <path_1.4>
```
A folder will be created in the configuration file path directory indicated with the name `cf_config_converted` and, inside, two configuration files:
- `coreflux_<assetname>_config.json`: To be used on the Coreflux HUB v1.3 and HUBLESS;
- `v1.4_coreflux_<assetname>_config.json`: To be used on the Coreflux HUB v1.4;

 #### Failure
 If something goes wrong, the following message will appear in the console:
 ```
Error: <error_information>
```

### Available Connector Converters:

- Siemens S7 Connector;
- EthernetIp Connector;
- Modbus Connector;
