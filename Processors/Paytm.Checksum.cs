using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

public class PayTMChecksum
{
	public static string generateSignature(Dictionary<string, string> input, string key)
	{
		return generateSignature(getStringByParams(input), key);
	}

	public static Dictionary<string,string> parseValues(JObject jobject,bool includechild)
	{
        Dictionary<string, string> lst = new Dictionary<string, string>();
        foreach (var jelm in jobject)
        {
            if (jelm.Value.GetType() == typeof(JValue))
            {
                lst.Add(jelm.Key, jelm.Value == null ? "": jelm.Value.ToString());
            }
            else if(jelm.Value.GetType() == typeof(JObject) && includechild)
            {
				foreach(var kv in parseValues(jelm.Value as JObject, includechild))
				{
                    lst.Add(kv.Key, kv.Value == null ? "" : kv.Value.ToString());
                }
            }
        }
		return lst;
    }
    public static string generateJSONSignature(string input,bool includechild, string key)
    {
        return generateSignature(parseValues(JObject.Parse(input), includechild), key);
    }
    public static string generateSignature(string input, string key)
	{
		try
		{
            validateGenerateCheckSumInput(key);
			StringBuilder stringBuilder = new StringBuilder(input);
			stringBuilder.Append("|");
			string text = generateRandomString(4);
			stringBuilder.Append(text);
			string hashedString = getHashedString(stringBuilder.ToString());
			hashedString += text;
			return encrypt(hashedString, key);
		}
		catch (Exception ex)
		{
			ShowException(ex);
			return null;
		}
	}

	public static bool verifySignature(Dictionary<string, string> input, string key, string CheckSum)
	{
		return verifySignature(getStringByParams(input), key, CheckSum);
	}

	public static bool verifySignature(string input, string key, string CheckSum)
	{
		try
		{
			validateVerifyCheckSumInput(CheckSum, key);
			string text = decrypt(CheckSum, key);
			if (text == null || text.Length < 4)
			{
				return false;
			}
			string text2 = text.Substring(text.Length - 4, 4);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(input);
			stringBuilder.Append("|");
			stringBuilder.Append(text2);
			return text.Equals(getHashedString(stringBuilder.ToString()) + text2);
		}
		catch (Exception ex)
		{
			ShowException(ex);
			return false;
		}
	}

	public static string encrypt(string input, string key)
	{
		try
		{
			byte[] bytes = Encoding.ASCII.GetBytes(input);
			MemoryStream memoryStream = new MemoryStream();
			Rijndael rijndael = Rijndael.Create();
			rijndael.Key = Encoding.ASCII.GetBytes(key);
			rijndael.IV = new byte[16]
			{
				64, 64, 64, 64, 38, 38, 38, 38, 35, 35,
				35, 35, 36, 36, 36, 36
			};
			rijndael.Mode = CipherMode.CBC;
			rijndael.Padding = PaddingMode.PKCS7;
			CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write);
			cryptoStream.Write(bytes, 0, bytes.Length);
			cryptoStream.Close();
			return Convert.ToBase64String(memoryStream.ToArray());
		}
		catch (Exception ex)
		{
			ShowException(ex);
			return null;
		}
	}

	public static string decrypt(string input, string key)
	{
		try
		{
			byte[] array = Convert.FromBase64String(input);
			MemoryStream memoryStream = new MemoryStream();
			Rijndael rijndael = Rijndael.Create();
			rijndael.Key = Encoding.ASCII.GetBytes(key);
			rijndael.IV = new byte[16]
			{
				64, 64, 64, 64, 38, 38, 38, 38, 35, 35,
				35, 35, 36, 36, 36, 36
			};
			rijndael.Mode = CipherMode.CBC;
			rijndael.Padding = PaddingMode.PKCS7;
			CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndael.CreateDecryptor(), CryptoStreamMode.Write);
			cryptoStream.Write(array, 0, array.Length);
			cryptoStream.Close();
			return Encoding.ASCII.GetString(memoryStream.ToArray());
		}
		catch (Exception ex)
		{
			ShowException(ex);
			return null;
		}
	}

	private static void validateGenerateCheckSumInput(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("Parameter cannot be null", "Specified key");
		}
	}

	private static void validateVerifyCheckSumInput(string checkSum, string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("Parameter cannot be null", "Specified key");
		}
		if (checkSum == null)
		{
			throw new ArgumentNullException("Parameter cannot be null", "Specified checkSum");
		}
	}

	private static string getStringByParams(Dictionary<string, string> parameters)
	{
		if (parameters == null)
		{
			return "";
		}
		SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>(parameters, StringComparer.Ordinal);
		StringBuilder stringBuilder = new StringBuilder("");
		foreach (KeyValuePair<string, string> item in sortedDictionary)
		{
			string text = item.Value;
			if (text == null)
			{
				text = "";
			}
			stringBuilder.Append(text).Append("|");
		}
		return stringBuilder.ToString().Substring(0, stringBuilder.Length - 1);
	}

	private static string generateRandomString(int length)
	{
		if (length <= 0)
		{
			return "";
		}
		Random random = new Random((int)DateTime.Now.Ticks);
		StringBuilder stringBuilder = new StringBuilder("");
		for (int i = 0; i < length; i++)
		{
			int startIndex = random.Next("@#!abcdefghijklmonpqrstuvwxyz#@01234567890123456789#@ABCDEFGHIJKLMNOPQRSTUVWXYZ#@".Length);
			stringBuilder.Append("@#!abcdefghijklmonpqrstuvwxyz#@01234567890123456789#@ABCDEFGHIJKLMNOPQRSTUVWXYZ#@".Substring(startIndex, 1));
		}
		return stringBuilder.ToString();
	}

	private static string getHashedString(string inputValue)
	{
		byte[] bytes = Encoding.ASCII.GetBytes(inputValue);
		SHA256Managed sHA256Managed = new SHA256Managed();
		byte[] array = sHA256Managed.ComputeHash(bytes);
		return BitConverter.ToString(array).Replace("-", "").ToLower();
	}

	private static void ShowException(Exception ex)
	{
		Console.WriteLine("Message : " + ex.Message + Environment.NewLine + "StackTrace : " + ex.StackTrace);
	}
}
