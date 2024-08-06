using System.IO;
using System.Security.Cryptography;

public struct AES_ctx
{
	//    public int RoundKey[240];
	//    public int Iv[16];
};

namespace Gloki2._0.SystemBoards
{
	/*
  #if defined(AES256) && (AES256 == 1)
  #define AES_KEYLEN 32
  #define AES_keyExpSize 240
  #elif defined(AES192) && (AES192 == 1)
  #define AES_KEYLEN 24
  #define AES_keyExpSize 208
  #else
  #define AES_KEYLEN 16   // Key length in bytes
  #define AES_keyExpSize 176
  #endif
	  */

	class Crypto
	{
		byte[] Key = new byte[] { 0x2b, 0x7e, 0x15, 0x16, 0x28, 0xae, 0xd2, 0xa6, 0xab, 0xf7, 0x15, 0x88, 0x09, 0xcf, 0x4f, 0x3c };
		public int FrameCreate(byte[] dataIn, byte[] Frame, int Length)
		{
			int BlockLength, i, EncDataLengh;
			// byte[] Frame    = new byte[512];
			byte[] IV = new byte[16];
			byte[] ENCData = new byte[2000];
			//            byte[] Key      = new byte[] { 0x2b, 0x7e, 0x15, 0x16, 0x28, 0xae, 0xd2, 0xa6, 0xab, 0xf7, 0x15, 0x88, 0x09, 0xcf, 0x4f, 0x3c };

			for (i = 0; i < 16; i++)
			{
				IV[i] = (byte)i;
			}
			//           ENCData[0] = (byte)(Length << 8);
			ENCData[0] = (byte)(Length >> 8);
			ENCData[1] = (byte)Length;

			ENCData[2] = 0xC4; // Crypto check-> Encryption starts here
			ENCData[3] = 0xD7;
			for (i = 0; i < Length; i++)
			{
				ENCData[4 + i] = dataIn[i];
			}
			BlockLength = (i + 20) % 16;
			for (int j = 0; j < (16 - BlockLength); j++)
			{
				ENCData[4 + i] = 0xAA;
				i++;

			}
			EncDataLengh = i + 4;
			byte[] EnryptedData = Encrypt(ENCData, Key, IV, EncDataLengh);
			for (i = 0; i < 16; i++)
			{
				Frame[i] = (byte)IV[i];
			}
			for (int index = 0; index < EncDataLengh; index++)
			{
				Frame[i++] = EnryptedData[index];
			}
			return (16 + EncDataLengh);

			//           return Frame;
		}
		public int FrameExtract(byte[] DataIn, byte[] DataOut, int Length)
		{
			byte[] IV = new byte[16];
			byte[] EncData = new byte[(Length - 16)];
			int ReturnLength = 0;

			for (int i = 0; i < 16; i++)
			{
				IV[i] = DataIn[i];
			}
			for (int i = 0; i < (Length - 16); i++)
			{
				EncData[i] = DataIn[16 + i];
			}
			byte[] DecryptedData = Decrypt(EncData, Key, IV, (Length - 16));

			if ((DecryptedData[2] == 0xC4) && (DecryptedData[3] == 0xD7))// Crypto check-> Encryption starts here
			{
				ReturnLength = DecryptedData[0] << 8 | DecryptedData[1];
				for (int i = 0; i < ReturnLength; i++)
				{
					DataOut[i] = DecryptedData[4 + i];
				}
			}
			else
			{
				ReturnLength = 0;
			}

			return ReturnLength;

		} // END int FrameExtract( byte[] DataIn, byte[] DataOut, int Length )
		public byte[] Encrypt(byte[] data, byte[] key, byte[] iv, int Length)
		{
			using (var aes = Aes.Create())
			{
				aes.KeySize = 128;
				aes.BlockSize = 128;
				aes.Padding = PaddingMode.None;
				aes.Mode = CipherMode.CBC;
				//aes.Mode = CipherMode.ECB;

				aes.Key = key;
				aes.IV = iv;

				using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
				{
					return PerformCryptography(data, encryptor, Length);
				}
			}
		}

		public byte[] Decrypt(byte[] data, byte[] key, byte[] iv, int Length)
		{
			using (var aes = Aes.Create())
			{
				aes.KeySize = 128;
				aes.BlockSize = 128;
				aes.Padding = PaddingMode.None;
				aes.Mode = CipherMode.CBC;
				//aes.Mode = CipherMode.ECB;

				aes.Key = key;
				aes.IV = iv;

				using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
				{
					return PerformCryptography(data, decryptor, Length);
				}
			}
		}

		public byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform, int Length)
		{
			using (var ms = new MemoryStream())
			using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
			{
				//cryptoStream.Write(data, 0, data.Length);
				cryptoStream.Write(data, 0, Length);
				cryptoStream.FlushFinalBlock();

				return ms.ToArray();
			}
		}
	}
}
