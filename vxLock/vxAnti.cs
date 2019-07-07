using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

class vxAnti
{
    [DllImport("kernel32.dll")]
    static extern bool IsDebuggerPresent();

    [DllImport("kernel32.dll")]
    static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    private bool antidebug = true;
    private bool antisandbox = true;

    /// <summary>
    /// Procedure wich task is to detect any unwanted programs set by author.
    /// </summary>
    public vxAnti()
    {
        if (antidebug) AntiDebug();
        if (antisandbox) DetectSandboxie();
    }

    protected void DetectSandboxie()
    {
        if (GetModuleHandle("SbieDll.dll").ToInt32() != 0) Environment.Exit(102);     
    }

    protected void AntiDebug()
    {
        if (IsDebuggerPresent()) Environment.Exit(101);
        if (System.Diagnostics.Debugger.IsAttached) Environment.Exit(101);
    }


    /*
     * [ Error Codes ]
     * 101 = Debugger detected
     * 102 = Sandboxie detected
     * */
}
