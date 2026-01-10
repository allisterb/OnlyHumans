using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace OnlyHumans.ChatCompletion;

internal static class FunctionGemmaTemplate
{
    public static string FormatPrompt(ChatHistory history, IEnumerable<KernelFunction>? functions)
    {
        var sb = new StringBuilder();
        
        var systemMessage = history.FirstOrDefault(m => m.Role == AuthorRole.System || m.Role == AuthorRole.Developer);
        bool hasTools = functions != null && functions.Any();

        if (systemMessage != null || hasTools)
        {
            sb.Append("<start_of_turn>developer\n");
            
            if (systemMessage != null && !string.IsNullOrEmpty(systemMessage.Content))
            {
                sb.Append(systemMessage.Content.Trim());
            }

            if (hasTools)
            {
                foreach (var func in functions!)
                {
                    sb.Append("\n<start_function_declaration>");
                    sb.Append(FormatFunctionDeclaration(func));
                    sb.Append("<end_function_declaration>");
                }
            }
            
            sb.Append("\n<end_of_turn>\n");
        }

        foreach (var message in history)
        {
            if (message == systemMessage) continue;

            if (message.Role == AuthorRole.Tool)
            {
                foreach (var item in message.Items.OfType<FunctionResultContent>())
                {
                    sb.Append($"<start_function_response>response:{item.FunctionName}");
                    sb.Append(FormatArgument(item.Result));
                    sb.Append("<end_function_response>");
                }
                
                if (!message.Items.Any() && !string.IsNullOrEmpty(message.Content))
                {
                     // Skip legacy text-only tool messages to avoid confusion
                }
                continue;
            }

            string role = message.Role == AuthorRole.Assistant ? "model" : "user";
            sb.Append($"<start_of_turn>{role}\n");

            if (message.Role == AuthorRole.User)
            {
                if (!string.IsNullOrEmpty(message.Content))
                {
                    sb.Append(message.Content.Trim());
                }
            }
            else if (message.Role == AuthorRole.Assistant)
            {
                var textContent = string.Join("", message.Items.OfType<TextContent>().Select(t => t.Text));
                if (!string.IsNullOrEmpty(textContent))
                {
                    sb.Append(textContent.Trim());
                }

                foreach (var call in message.Items.OfType<FunctionCallContent>())
                {
                    sb.Append($"<start_function_call>call:{call.FunctionName}{{");
                    if (call.Arguments != null)
                    {
                        var args = new List<string>();
                        foreach (var arg in call.Arguments)
                        {
                            args.Add($"{arg.Key}:{FormatArgument(arg.Value)}");
                        }
                        sb.Append(string.Join(",", args));
                    }
                    sb.Append("}}<end_function_call>");
                }
            }
            
            sb.Append("\n<end_of_turn>\n");
        }
        
        //sb.Append("<start_of_turn>model\n");
        return sb.ToString();
    }

    private static string FormatFunctionDeclaration(KernelFunction func)
    {
        var sb = new StringBuilder();
        var meta = func.Metadata;
        
        sb.Append($"declaration:{meta.Name}");
        sb.Append($"{{description:<escape>{meta.Description}<escape>");
        
        if (meta.Parameters != null && meta.Parameters.Any())
        {
            sb.Append(",parameters:{{properties:{");
            
            var props = new List<string>();
            var required = new List<string>();

            foreach (var param in meta.Parameters)
            {
                var pSb = new StringBuilder();
                pSb.Append($"{param.Name}:{{description:<escape>{param.Description}<escape>");
                
                string type = MapType(param.ParameterType);
                pSb.Append($",type:<escape>{type}<escape>}}");
                
                props.Add(pSb.ToString());
                
                if (param.IsRequired)
                {
                    required.Add($"<escape>{param.Name}<escape>");
                }
            }
            
            sb.Append(string.Join(",", props));
            sb.Append("}");

            if (required.Any())
            {
                sb.Append($",required:[{string.Join(",", required)}]");
            }
            
            sb.Append(",type:<escape>OBJECT<escape>}}");
        }
        
        sb.Append("}");
        return sb.ToString();
    }
    
    private static string MapType(Type? type)
    {
        if (type == null) return "STRING";
        if (type == typeof(int) || type == typeof(long) || type == typeof(double) || type == typeof(float) || type == typeof(decimal)) return "NUMBER";
        if (type == typeof(bool)) return "BOOLEAN";
        if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))) return "ARRAY";
        return "STRING";
    }

    private static string FormatArgument(object? value)
    {
        if (value == null) return "null";
        if (value is string s) return $"<escape>{s}<escape>";
        if (value is bool b) return b ? "true" : "false";
        if (value is int || value is long || value is double || value is float || value is decimal) return value.ToString();
        return $"<escape>{value}<escape>";
    }

    public static List<FunctionCallContent> ParseFunctionCalls(string text)
    {
        var list = new List<FunctionCallContent>();
        int idx = 0;
        while ((idx = text.IndexOf("<start_function_call>", idx)) != -1)
        {
            int start = idx + "<start_function_call>".Length;
            int end = text.IndexOf("<end_function_call>", start);
            if (end == -1) break;
            
            string callStr = text.Substring(start, end - start);
            if (callStr.StartsWith("call:"))
            {
                int braceOpen = callStr.IndexOf('{');
                if (braceOpen != -1)
                {
                    string name = callStr.Substring(5, braceOpen - 5);
                    string argsStr = callStr.Substring(braceOpen);
                    
                    var args = ParseGemmaArgs(argsStr);
                    list.Add(new FunctionCallContent(name, arguments:args));
                }
            }
            idx = end + "<end_function_call>".Length;
        }
        return list;
    }

    public static string RemoveFunctionCalls(string text)
    {
        var regex = new Regex(@"<start_function_call>.*?<end_function_call>", RegexOptions.Singleline);
        return regex.Replace(text, "").Trim();
    }

    private static KernelArguments ParseGemmaArgs(string jsonLike)
    {
        var args = new KernelArguments();
        if (jsonLike.StartsWith("{") && jsonLike.EndsWith("}"))
        {
            jsonLike = jsonLike.Substring(1, jsonLike.Length - 2);
        }
        
        if (string.IsNullOrWhiteSpace(jsonLike)) return args;

        int pos = 0;
        while (pos < jsonLike.Length)
        {
            int colon = jsonLike.IndexOf(':', pos);
            if (colon == -1) break;
            
            string key = jsonLike.Substring(pos, colon - pos).Trim();
            pos = colon + 1;
            
            if (pos >= jsonLike.Length) break;

            object? value = null;
            if (jsonLike.Substring(pos).StartsWith("<escape>"))
            {
                int vStart = pos + "<escape>".Length;
                int vEnd = jsonLike.IndexOf("<escape>", vStart);
                if (vEnd == -1) break;
                value = jsonLike.Substring(vStart, vEnd - vStart);
                pos = vEnd + "<escape>".Length;
            }
            else
            {
                int comma = jsonLike.IndexOf(',', pos);
                if (comma == -1) comma = jsonLike.Length;
                
                string valStr = jsonLike.Substring(pos, comma - pos).Trim();
                if (valStr == "true") value = true;
                else if (valStr == "false") value = false;
                else if (double.TryParse(valStr, out double d)) value = d;
                else value = valStr;
                
                pos = comma;
            }

            args[key] = value;
            
            if (pos < jsonLike.Length && jsonLike[pos] == ',') pos++;
        }
        
        return args;
    }
}
