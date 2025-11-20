namespace OnlyHumans.Acp;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public class Client : Runtime, IClient
{
    public Client(string name, string version, int protocolVersion, ClientCapabilities capabilities)
    {
        this.name = name;
        this.version = version; 
        this.protocolVersion = protocolVersion;
        this.clientInfo = new Implementation()
        {
            Name = name,
            Version = version,
        };
        this.capabilities = capabilities;
    }

    #region IClient virtual method stubs
    public virtual Task SessionUpdateAsync(SessionNotification notification)
        => throw new NotImplementedException();

    public virtual Task<Result<RequestPermissionResponse>> RequestPermissionAsync(RequestPermissionRequest request)
        => throw new NotImplementedException();

    public virtual Task<Result<CreateTerminalResponse>> CreateTerminalAsync(CreateTerminalRequest request)
        => throw new NotImplementedException();

    public virtual Task<Result<KillTerminalCommandResponse>> KillTerminalCommandAsync(KillTerminalCommandRequest request)
        => throw new NotImplementedException();

    public virtual Task<Result<ReleaseTerminalResponse>> ReleaseTerminalAsync(ReleaseTerminalRequest request)
        => throw new NotImplementedException();

    public virtual Task<Result<TerminalOutputResponse>> TerminalOutputAsync(TerminalOutputRequest request)
        => throw new NotImplementedException();

    public virtual Task<Result<WaitForTerminalExitResponse>> WaitForTerminalExitAsync(WaitForTerminalExitRequest request)
        => throw new NotImplementedException();

    public virtual Task<Result<ReadTextFileResponse>> ReadTextFileAsync(ReadTextFileRequest request)
        => throw new NotImplementedException();

    public virtual Task<Result<WriteTextFileResponse>> WriteTextFileAsync(WriteTextFileRequest request)
        => throw new NotImplementedException();

    public virtual Task<Result<Dictionary<string, object>>> ExtMethodAsync(string method, Dictionary<string, object>? parameters = null)
        => throw new NotImplementedException();

    public virtual Task ExtNotificationAsync(string notification, Dictionary<string, object>? parameters = null)
        => throw new NotImplementedException();
    #endregion

    
    #region Fields
    public readonly string name;
    public readonly string version;
    public readonly int protocolVersion;
    public readonly Implementation clientInfo;
    public readonly ClientCapabilities capabilities;
    #endregion
}

public partial record ClientCapabilities
{
    public static ClientCapabilities Default { get; } = new ClientCapabilities()
    {
        Fs = new FileSystemCapability()
        {
            ReadTextFile = true,
            WriteTextFile = true
        },
        Terminal = true,
    };
}


