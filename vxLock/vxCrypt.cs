﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.IO;
using System.Security;

public class vxCrypt
{
    const int KEY_SIZE = 2048;
    const string pempubheader = "-----BEGIN PUBLIC KEY-----";
    const string pempubfooter = "-----END PUBLIC KEY-----";

    private const string public_key = "-----BEGIN PUBLIC KEY-----"
+ "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAnS9/s/Rz4ExWuWqctMvE"
+ "wrpaHPOCQJqIIEc0m4Ti4GNEbSuaYjPP7EEbbWXwN6tlyujrCycLPkBA7xydp8Z9"
+ "Ys3rVde5rGlVeIX/AoYruI3MlgLDSLV+gQgwqILWCt4qojsmHyS4XldblaI7LDus"
+ "GqdjlIwgLn55SKv+Dvc3JSdIAy2VrtnFEjWF16PAQ/dLgNXR4RIZ/4vzFkiUdZtz"
+ "c7R/pAVE0Ji5J3pPzlxbtLZW0pMb66OHWTdtGmfgie4tPjrRMW5YIBRUTUaTdEwF"
+ "9O8MX384Ci6Y4Fhl0fELwoq2A2PtfqVsO27LtsAJ1pehT4PSgt84Op/dFv7mWXEi"
+ "UwIDAQAB"
+ "-----END PUBLIC KEY-----";

    public RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(KEY_SIZE);
    private vxFunctions functions = new vxFunctions();
    private vxExportKeys ExpKeys = new vxExportKeys();
    private string Keys_xml = String.Empty;

    public vxCrypt()
    {
        RSA.PersistKeyInCsp = false;
        RSA.ExportParameters(false);
       // Keys_xml = _RSA.ToXmlString(true);
        //__ExportKeys();
        //Environment.Exit(0);
        GetEncryptionKey();
    }

    public vxCrypt(string privateKey)
    {
        RSA.PersistKeyInCsp = false;
        string publicKey = string.Empty;
        string privKey = string.Empty;

        DecodePEMKey(privateKey, out publicKey, out privKey);
    }


    protected void ExportKeys()
    {
        ExpKeys.ExportPublicKey(RSA, @"RSA_Keys.pub");
        ExpKeys.ExportPrivateKey(RSA, @"RSA_Keys.pem");
    }

    protected void GetEncryptionKey()
    {
        string publicKey = string.Empty;
        string privKey = string.Empty;

        try
        {
            DecodePEMKey(public_key, out publicKey, out privKey);
        }
        catch { Environment.Exit(1); }
    }



    public void DecodePEMKey(String pemstr, out string publicKey, out string privateKey)
    {
        byte[] pempublickey;
        byte[] pemprivatekey = new byte[] { 0 };

        publicKey = null;
        privateKey = null;

        if (pemstr.StartsWith(pempubheader))
        {
            //System.Diagnostics.Debug.Print("Trying to decode and parse a PEM public key ..");
            pempublickey = DecodeOpenSSLPublicKey(pemstr);
            if (pempublickey != null)
            {
                RSACryptoServiceProvider rsa = DecodeX509PublicKey(pempublickey);
                //PutFileBytes("rsapubkey.pem", pempublickey, pempublickey.Length) ;
                publicKey = rsa.ToXmlString(false);
                RSA = rsa;
            }
        }
        else if (pemstr.StartsWith("-----BEGIN RSA PRIVATE KEY-----"))
        {
           // System.Diagnostics.Debug.Print("Trying to decode and parse a PEM private key ..");
            pemprivatekey = DecodeOpenSSLPrivateKey(pemstr);
            if (pemprivatekey != null)
            {
                RSACryptoServiceProvider rsa = DecodeRSAPrivateKey(pemprivatekey);
                //PutFileBytes("rsapubkey.pem", pempublickey, pempublickey.Length) ;
                privateKey = rsa.ToXmlString(true);
                RSA = rsa;
            }

        }
    }


    //--------   Get the binary RSA PUBLIC key   --------
    protected byte[] DecodeOpenSSLPublicKey(String instr)
    {
        String pemstr = instr.Trim();
        byte[] binkey;
        if (!pemstr.StartsWith(pempubheader) || !pemstr.EndsWith(pempubfooter))
            return null;
        StringBuilder sb = new StringBuilder(pemstr);
        sb.Replace(pempubheader, "");  //remove headers/footers, if present
        sb.Replace(pempubfooter, "");

        String pubstr = sb.ToString().Trim();	//get string after removing leading/trailing whitespace

        try
        {
            binkey = Convert.FromBase64String(pubstr);
        }
        catch (System.FormatException)
        {		//if can't b64 decode, data is not valid
            return null;
        }
        return binkey;
    }

    //------- Parses binary asn.1 X509 SubjectPublicKeyInfo; returns RSACryptoServiceProvider ---
    public static RSACryptoServiceProvider DecodeX509PublicKey(byte[] x509key)
    {
        // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
        byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
        byte[] seq = new byte[15];
        // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
        MemoryStream mem = new MemoryStream(x509key);
        BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
        byte bt = 0;
        ushort twobytes = 0;

        try
        {

            twobytes = binr.ReadUInt16();
            if (twobytes == 0x8130)	//data read as little endian order (actual data order for Sequence is 30 81)
                binr.ReadByte();	//advance 1 byte
            else if (twobytes == 0x8230)
                binr.ReadInt16();	//advance 2 bytes
            else
                return null;

            seq = binr.ReadBytes(15);		//read the Sequence OID
            if (!CompareBytearrays(seq, SeqOID))	//make sure Sequence for OID is correct
                return null;

            twobytes = binr.ReadUInt16();
            if (twobytes == 0x8103)	//data read as little endian order (actual data order for Bit String is 03 81)
                binr.ReadByte();	//advance 1 byte
            else if (twobytes == 0x8203)
                binr.ReadInt16();	//advance 2 bytes
            else
                return null;

            bt = binr.ReadByte();
            if (bt != 0x00)		//expect null byte next
                return null;

            twobytes = binr.ReadUInt16();
            if (twobytes == 0x8130)	//data read as little endian order (actual data order for Sequence is 30 81)
                binr.ReadByte();	//advance 1 byte
            else if (twobytes == 0x8230)
                binr.ReadInt16();	//advance 2 bytes
            else
                return null;

            twobytes = binr.ReadUInt16();
            byte lowbyte = 0x00;
            byte highbyte = 0x00;

            if (twobytes == 0x8102)	//data read as little endian order (actual data order for Integer is 02 81)
                lowbyte = binr.ReadByte();	// read next bytes which is bytes in modulus
            else if (twobytes == 0x8202)
            {
                highbyte = binr.ReadByte();	//advance 2 bytes
                lowbyte = binr.ReadByte();
            }
            else
                return null;
            byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
            int modsize = BitConverter.ToInt32(modint, 0);

            byte firstbyte = binr.ReadByte();
            binr.BaseStream.Seek(-1, SeekOrigin.Current);

            if (firstbyte == 0x00)
            {	//if first byte (highest order) of modulus is zero, don't include it
                binr.ReadByte();	//skip this null byte
                modsize -= 1;	//reduce modulus buffer size by 1
            }

            byte[] modulus = binr.ReadBytes(modsize);	//read the modulus bytes

            if (binr.ReadByte() != 0x02)			//expect an Integer for the exponent data
                return null;
            int expbytes = (int)binr.ReadByte();		// should only need one byte for actual exponent data (for all useful values)
            byte[] exponent = binr.ReadBytes(expbytes);


            /*showBytes("\nExponent", exponent);
            showBytes("\nModulus", modulus);*/

            // ------- create RSACryptoServiceProvider instance and initialize with public key -----
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            RSAParameters RSAKeyInfo = new RSAParameters();
            RSAKeyInfo.Modulus = modulus;
            RSAKeyInfo.Exponent = exponent;
            RSA.ImportParameters(RSAKeyInfo);
            return RSA;
        }
        catch (Exception)
        {
            return null;
        }

        finally { binr.Close(); }

    }

    public byte[] DecodeOpenSSLPrivateKey(String instr)
    {
        const String pemprivheader = "-----BEGIN RSA PRIVATE KEY-----";
        const String pemprivfooter = "-----END RSA PRIVATE KEY-----";
        String pemstr = instr.Trim();
        byte[] binkey;
        if (!pemstr.StartsWith(pemprivheader) || !pemstr.EndsWith(pemprivfooter))
            return null;

        StringBuilder sb = new StringBuilder(pemstr);
        sb.Replace(pemprivheader, "");  //remove headers/footers, if present
        sb.Replace(pemprivfooter, "");

        String pvkstr = sb.ToString().Trim();	//get string after removing leading/trailing whitespace

        try
        {        // if there are no PEM encryption info lines, this is an UNencrypted PEM private key
            binkey = Convert.FromBase64String(pvkstr);
            return binkey;
        }
        catch (System.FormatException)
        {		
            //if can't b64 decode, it must be an encrypted private key 
        }
        return null;
    }

    public RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey)
    {
        byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

        // ---------  Set up stream to decode the asn.1 encoded RSA private key  ------
        MemoryStream mem = new MemoryStream(privkey);
        BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
        byte bt = 0;
        ushort twobytes = 0;
        int elems = 0;
        try
        {
            twobytes = binr.ReadUInt16();
            if (twobytes == 0x8130)	//data read as little endian order (actual data order for Sequence is 30 81)
                binr.ReadByte();	//advance 1 byte
            else if (twobytes == 0x8230)
                binr.ReadInt16();	//advance 2 bytes
            else
                return null;

            twobytes = binr.ReadUInt16();
            if (twobytes != 0x0102)	//version number
                return null;
            bt = binr.ReadByte();
            if (bt != 0x00)
                return null;


            //------  all private key components are Integer sequences ----
            elems = GetIntegerSize(binr);
            MODULUS = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            E = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            D = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            P = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            Q = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            DP = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            DQ = binr.ReadBytes(elems);

            elems = GetIntegerSize(binr);
            IQ = binr.ReadBytes(elems);

            // ------- create RSACryptoServiceProvider instance and initialize with public key -----
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            RSAParameters RSAparams = new RSAParameters();
            RSAparams.Modulus = MODULUS;
            RSAparams.Exponent = E;
            RSAparams.D = D;
            RSAparams.P = P;
            RSAparams.Q = Q;
            RSAparams.DP = DP;
            RSAparams.DQ = DQ;
            RSAparams.InverseQ = IQ;
            RSA.ImportParameters(RSAparams);
            return RSA;
        }
        catch (Exception)
        {
            return null;
        }
        finally { binr.Close(); }
    }

    private static int GetIntegerSize(BinaryReader binr)
    {
        byte bt = 0;
        byte lowbyte = 0x00;
        byte highbyte = 0x00;
        int count = 0;
        bt = binr.ReadByte();
        if (bt != 0x02)		//expect integer
            return 0;
        bt = binr.ReadByte();

        if (bt == 0x81)
            count = binr.ReadByte();	// data size in next byte
        else
            if (bt == 0x82)
            {
                highbyte = binr.ReadByte();	// data size in next 2 bytes
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;		// we already have the data size
            }

        while (binr.ReadByte() == 0x00)
        {	//remove high order zeros in data
            count -= 1;
        }
        binr.BaseStream.Seek(-1, SeekOrigin.Current);		//last ReadByte wasn't a removed zero, so back up a byte
        return count;
    }

    private static bool CompareBytearrays(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;
        int i = 0;
        foreach (byte c in a)
        {
            if (c != b[i])
                return false;
            i++;
        }
        return true;
    }
}
