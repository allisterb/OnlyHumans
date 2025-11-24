namespace OnlyHumans.Acp;

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Nerdbank.Streams;

public class Terminal : Runtime
{
    #region Constructors
    public Terminal(Session session, CreateTerminalRequest request, string terminalid)
    {
        this.session = session;
        this.request = request;
        this.sessionid = request.SessionId;
        this.terminalid = terminalid;
        ProcessStartInfo psi = new ProcessStartInfo()
        {
            FileName = request.Command,
            Arguments = request.Args.JoinWith(" "),
            WorkingDirectory = request.Cwd,
        };
        foreach (var env in request.Env)
        {
            psi.Environment[env.Name] = env.Value;
        }
        process = new Process()
        {
            StartInfo = psi,
            EnableRaisingEvents = true,
        };
        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            startException = ex;
            return;
        }
        var istream = new MonitoringStream(process.StandardOutput.BaseStream);
        istream.DidReadAny += Istream_DidReadAny;
        var ostream = new MonitoringStream(process.StandardInput.BaseStream);
        initialized = true;
    }

    private void Istream_DidReadAny(object sender, ReadOnlySpan<byte> span)
    {
        //request.
    }
    #endregion

    #region Fields
    public readonly Session session;
    public readonly CreateTerminalRequest request;
    public readonly string sessionid;
    public readonly string terminalid;
    public readonly Process process;
    public readonly bool initialized = false;   
    public Exception? startException = null;
    #endregion
}

