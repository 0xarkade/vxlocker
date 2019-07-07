using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Management;
using System.Net;

public class vxFunctions
{
    private const string alphanum = "9876054231abcdefghijklmqUVWrstuvwxAEFnopGHIJKLyzMNOPQRBCDSTXYZ!@#$%^&*()-_=+\"|/][~`";

    #region RandomString_functions
    public string GenRandomString(byte len)
    {
        string result = String.Empty;

        Random rnd = new Random((int)DateTime.Now.Ticks);

        for (int i = 0; i < len; ++i)
        {
            result += alphanum.Substring(rnd.Next(0, alphanum.Length), 1);
        }

        return result;
    }


    /// <summary>
    /// Generates Random String with Secure Algorithm
    /// </summary>
    /// <param name="len">Length of String</param>
    /// <returns>returns randomly generated crypto string with given length</returns>
    public string GetRNGStr(byte len)
    {
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        byte[] rng_array = new byte[2048];
        byte[] output_rng = new byte[len];
        rng.GetNonZeroBytes(rng_array);
        byte c = 0;

        foreach (byte rng_b in rng_array)
        {
            char ch = Convert.ToChar(Encoding.ASCII.GetString(new[] { rng_b }));
            if (Char.IsLetterOrDigit(ch) || Char.IsSymbol(ch))
            {
                if (c <= len - 1)
                {
                     output_rng[c] = rng_b;
                     c++;
                }
                else break;         
            }
        }
        return Encoding.ASCII.GetString(output_rng);
    }
    #endregion

    #region StringHashes
    public static string GetMD5Hash(string input)
    {
        byte[] data = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(input));

        StringBuilder sBuilder = new StringBuilder();

        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
        // Return the hexadecimal string.
        return sBuilder.ToString();
    }


    public string GetSHA1Hash(string input)
    {
        byte[] data = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(input));

        StringBuilder sBuilder = new StringBuilder();

        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
        // Return the hexadecimal string.
        return sBuilder.ToString();
    }
    #endregion

    #region Base64
    public string Base64Encode(string plainText)
    {
        return System.Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
    }

    public string Base64Decode(string plainText)
    {
        return Encoding.UTF8.GetString(System.Convert.FromBase64String(plainText));
    }
    #endregion

    #region CheckSum

    public class CheckSums
    {
        public String MD5CheckSum(string input_file)
        {
            if (!File.Exists(@input_file)) return String.Empty;
            MD5 md5 = MD5.Create();
            StringBuilder sb = new StringBuilder();

            try
            {
                FileStream input_f = File.OpenRead(input_file);
                byte[] out_bytes = md5.ComputeHash(input_f);

                foreach (byte o in out_bytes)
                {
                    sb.Append(o.ToString("x2"));
                }
                md5.Clear();
                input_f.Close();
            }
            catch { return String.Empty; }

            return sb.ToString();
        }

        public String SHA1CheckSum(string input_file)
        {
            if (!File.Exists(@input_file)) return String.Empty;
            SHA1 sha1 = SHA1.Create();
            StringBuilder sb = new StringBuilder();

            try
            {
                FileStream input_f = File.OpenRead(input_file);
                byte[] out_bytes = sha1.ComputeHash(input_f);

                foreach (byte o in out_bytes)
                {
                    sb.Append(o.ToString("x2"));
                }
                sha1.Clear();
                input_f.Close();
            }
            catch { return String.Empty; }

            return sb.ToString();
        }

        public String SHA256CheckSum(string input_file)
        {
            if (!File.Exists(@input_file)) return String.Empty;
            SHA256 sha = SHA256.Create();
            StringBuilder sb = new StringBuilder();

            try
            {
                FileStream input_f = File.OpenRead(input_file);
                byte[] out_bytes = sha.ComputeHash(input_f);

                foreach (byte o in out_bytes)
                {
                    sb.Append(o.ToString("x2"));
                }
                sha.Clear();
                input_f.Close();
            }
            catch { return String.Empty; }

            return sb.ToString();
        }

        public String SHA512CheckSum(string input_file)
        {
            if (!File.Exists(@input_file)) return String.Empty;
            SHA512 sha = SHA512.Create();
            StringBuilder sb = new StringBuilder();

            try
            {
                FileStream input_f = File.OpenRead(input_file);
                byte[] out_bytes = sha.ComputeHash(input_f);

                foreach (byte o in out_bytes)
                {
                    sb.Append(o.ToString("x2"));
                }
                sha.Clear();
                input_f.Close();
            }
            catch { return String.Empty; }

            return sb.ToString();
        }
    }

    #endregion


    public static string GetMACAddress1()
    {
        ManagementObjectSearcher objMOS = new ManagementObjectSearcher("Select * FROM Win32_NetworkAdapterConfiguration");
        ManagementObjectCollection objMOC = objMOS.Get();
        string macAddress = String.Empty;
        foreach (ManagementObject objMO in objMOC)
        {
            object tempMacAddrObj = objMO["MacAddress"];

            if (tempMacAddrObj == null) //Skip objects without a MACAddress
            {
                continue;
            }
            if (macAddress == String.Empty) // only return MAC Address from first card that has a MAC Address
            {
                macAddress = tempMacAddrObj.ToString();
            }
            objMO.Dispose();
        }
        macAddress = macAddress.Replace(":", "");
        return macAddress;
    }

    public static String GetOS()
    {
        string result = "Unknown";
        try
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            foreach (ManagementObject os in searcher.Get())
            {
                result = os["Caption"].ToString();
                break;
            }
        }
        catch
        { }

        return result;
    }

    public static String GetIP()
    {
        String myIp = "localhost";
        try
        {
            myIp = new WebClient().DownloadString(@"http://icanhazip.com").Trim();
        }
        catch { }

        return myIp;
    }

    public static String GenKey()
    {
        return "127AudghjAQWrtgLVGQWref%^32qqdfs";
    }


}
