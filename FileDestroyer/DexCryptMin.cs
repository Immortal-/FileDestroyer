/*
*   Copyright (C) 2011, Dextrey (0xDEADDEAD)
* LINK: http://www.hackforums.net/showthread.php?tid=1481595
*/
using System;
using System.Collections.Generic;
using System.Text;

class DexCryptMin
{

    public static string Encrypt(string plain, string key)
    {
        return Convert.ToBase64String(Encrypt(UnicodeEncoding.UTF8.GetBytes(plain), UnicodeEncoding.UTF8.GetBytes(key)));
    }
    public static string Decrypt(string plain, string key)
    {
        return UnicodeEncoding.UTF8.GetString(Decrypt(Convert.FromBase64String(plain), UnicodeEncoding.UTF8.GetBytes(key)));
    }

    public static byte[] Encrypt(byte[] plain, byte[] key)
    {
        byte[] expandedKey = ExpandKey(key, plain.Length);
        byte[] wholeState = plain;
        byte magic = (byte)new Random().Next(byte.MaxValue);
        Array.Resize(ref wholeState, plain.Length + 1);
        wholeState[wholeState.Length - 1] = magic;

        for (int i = 0; i < wholeState.Length - 1; i++)
        {
            wholeState[i] = (byte)(wholeState[i] ^ expandedKey[i] ^ magic);
        }

        return wholeState;
    }

    public static byte[] Decrypt(byte[] plain, byte[] key)
    {
        byte[] expandedKey = ExpandKey(key, plain.Length);
        byte[] wholeState = plain;
        byte magic = plain[plain.Length - 1];
        Array.Resize(ref wholeState, wholeState.Length - 1);

        for (int i = 0; i < wholeState.Length; i++)
        {
            wholeState[i] = (byte)(wholeState[i] ^ magic ^ expandedKey[i]);
        }

        return wholeState;
    }
    /// <summary>
    /// Performs DexCryptMin key expansion algorithm on variable length input key
    /// </summary>
    /// <param name="key">Input key</param>
    /// <param name="length">Count of output keystream bytes</param>
    /// <returns>Expanded keystream</returns>
    private static byte[] ExpandKey(byte[] key, int length)
    {
        if (key.Length >= length) return key;
        byte[] rconst = BitConverter.GetBytes(Math.Round(Math.PI, 3));
        byte[] result = new byte[length];
        Buffer.BlockCopy(key, 0, result, 0, key.Length);
        // init round (fill all remaining bytes)
        for (int i = key.Length; i < length; i++)
        {
            // x[i] = ((k[i - len(k)) % len(k)] + x[i - 1]) % 256 
            result[i] = (byte)((key[(i - key.Length) % key.Length] ^ (result[i - 1])) % 256);
        }
        // main rounds (process all bytes)
        for (int round = 0; round < 2; round++)
        {
            result[0] = (byte)(result[0] ^ rconst[round]);
            for (int i = 1; i < result.Length; i++)
            {
                // x[i] = ((x[i] ^ (rcon[r] << (i % 3))) ^ x [i - 1]) % 256
                result[i] = (byte)(((result[i] ^ (byte)(rconst[round] << (i % 4))) ^ result[i - 1]) % 256);
            }
        }
        return result;
    }
}