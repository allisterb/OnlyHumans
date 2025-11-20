namespace OnlyHumans.Acp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class Client : Runtime
{
    public Client(string name, string version, int protocolVersion, ClientCapabilities capabilities)
    {
        this.name = name;
        this.version = version; 
        this.protocolVersion = protocolVersion;
        this.capabilities = capabilities;
    }

    #region Fields
    public readonly string name;
    public readonly string version;
    public readonly int protocolVersion;
    public readonly ClientCapabilities capabilities;
    #endregion
}

