# Coreflux Configuration Converter Tool

Used old versions of Coreflux connectors that have now changed and your configuration is no longer accepted?

The Coreflux Configuration Converter Tool now allows you to turn your old configuration into the new configuration, only by running the tool as:

```
cf_config_converter.exe <configuration_file_path>
```

A folder will be created in the file path with the name `cf_config_converted` and, inside, two configuration files: <br>
- `coreflux_<assetname>_config.json`: To be used on the Coreflux HUB v1.3 and HUBLESS;
- `v1.4_coreflux_<assetname>_config.json`: To be used on the Coreflux HUB v1.4 and HUBLESS;

### Available Connector Converters:

- Siemens S7 Connector;
- EthernetIp Connector;
- Modbus Connector;