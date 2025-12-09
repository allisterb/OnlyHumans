@echo off
dotnet tool restore
dotnet nswag jsonschema2csclient /input:schema\schema.json /output:Schema.gen.cs /GenerateNativeRecords:true /Name:Schema /Namespace:OnlyHumans.Acp