@echo off
dotnet tool restore
dotnet nswag jsonschema2csclient /input:schema\schema.json /output:Schema.cs /GenerateNativeRecords:true /Name:Schema /Namespace:OnlyHumans.Acp /JsonSerializerSettingsTransformationMethod:Transform