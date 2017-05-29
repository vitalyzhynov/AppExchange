using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Text;

namespace IDEAChipher
{
	public class IdeaChipher
	{
		private int blockSize = 16;
		private byte countOfKeyShiftingBits = 25;
		private byte countOfRounds = 8;
		//52 = 8 x 6 + 4 
		//private int countOfKeys = 52;
		//array of 16-bit internal keys (two last values are empty!)
		ushort[,] keys = new ushort[9,6];
		public ushort[,] decKeys = new ushort[9, 6];
		uint module = 65536;

		public IdeaChipher()
		{
		}

		public IdeaChipher(string userKey)
		{
			InitKeys(userKey);
		}

		public string Encrypt(string text)
		{
			string result = string.Empty;
			result =  Encrypt(text, keys, true);
			return result;
		}

		private string Encrypt(string text, ushort[,] specificKeys, bool autocomplete)
		{
			string result = string.Empty;

			//Cutting text to 64-bit blocks  
			if (text.Length == blockSize / 4)
			{
				result = IdeaProcessing(text, specificKeys);
			}
			if(text.Length == 0)
			{
				throw new Exception("Text is empty!");
			}
			else
			{
				StringBuilder builder;
				if (text.Length % (blockSize / 4) != 0)
				{
                    if (autocomplete)
                    {
                        byte necessaryCharsCount = (byte)(blockSize / 4 - text.Length % (blockSize / 4));
                        text = Autocomplete(text, necessaryCharsCount);
                    }
                    else
                    {
                        throw new Exception("Text is demaged! Please, try to check the count of characters!");
                    }
				}

				int count = text.Length / (blockSize / 4);
				builder = new StringBuilder(text.Length);
				string[] bufStr = new string[count];

				int j = 0;
				bufStr[j] += text[0];
				for (int i = 1; i < text.Length; i++)
				{
					bufStr[j] += text[i];

					if ((i + 1) % (blockSize / 4) == 0)
						j++;
				}

				for (int i = 0; i < count; i++)
				{
					string buf = IdeaProcessing(bufStr[i], specificKeys);
					builder.Append(buf);
				}

				result = builder.ToString();
			}

			return result;
		}

		public string Decrypt(string text)
		{
            if (SurrogatePairsDetected(text))
            {
                //cut the additional parts of surrogete pairs
                text = DeleteAdditionalPartsFromSurrogetePairs(text);
            }
            //throws the exception if len of the message is not 64-bit compatible
            return Encrypt(text, decKeys, false);
		}

        public bool SurrogatePairsDetected(string str)
        {
            bool res = false;

            for (int i = 0; i < str.Length; i++)
            {
                if ((str[i] >= '\xD800') && (str[i] <= '\uDFFF'))
                {
                    res = true;
                    break;
                }
            }

            return res;
        }

		private string Autocomplete(string text, byte count)
		{
			string additionals = new string(' ', count);
			return text + additionals;
		}

		//block - 64-bit string
		public string IdeaProcessing(string block, ushort[,] specificKeys)
		{
			string result = string.Empty;

			if(block != null)
			{
				if(block.Length != blockSize / 4)
				{
					throw new Exception(string.Format("Incorrect size of block! must be {0}. IdeaProcessing method", blockSize / 4));
				}

				ushort[] subBlocks = GetDigitsFromString(block);

				for (byte i = 0; i < countOfRounds; i++)
				{
					IdeaRound1(ref subBlocks[0], ref subBlocks[1], ref subBlocks[2], ref subBlocks[3], i, specificKeys);
				}

				ushort buf = subBlocks[2];
				subBlocks[2] = subBlocks[1];
				subBlocks[1] = buf;

				ConclusionTransformation(ref subBlocks[0], ref subBlocks[1], ref subBlocks[2], ref subBlocks[3], specificKeys);

				result = subBlocks.ToLiteralString();
			}
			else
			{
				throw new NullReferenceException("Block is empty! IdeaProcessing method");	
			}

			return result;
		}

		public void ConclusionTransformation(ref ushort d1, ref ushort d2, ref ushort d3, ref ushort d4, ushort[,] specificKeys)
		{
			d1 = Mul(d1, specificKeys[8, 0]);
			d2 = Summ(d2, specificKeys[8, 1]);
			d3 = Summ(d3, specificKeys[8, 2]);
			d4 = Mul(d4, specificKeys[8, 3]);
		}

		public void IdeaRound1(ref ushort d1, ref ushort d2, ref ushort d3, ref ushort d4, byte i, ushort[,] sk)
		{
			if (keys == null)
			{
				throw new Exception("You must to init the key firstly before the algorithm starts!");
			}

			ushort a = Mul(d1, sk[i, 0]);
			ushort b = Summ(d2, sk[i, 1]);
			ushort c = Summ(d3, sk[i, 2]);
			ushort d = Mul(d4, sk[i, 3]);

			ushort e = XOR(a, c);
			ushort f = XOR(b, d);

			d1 = XOR(a, Mul(Summ(f, Mul(e, sk[i, 4])), sk[i, 5]));
			d2 = XOR(c, Mul(Summ(f, Mul(e, sk[i, 4])), sk[i, 5]));
			d3 = XOR(b, Summ(Mul(e, sk[i, 4]), Mul(Summ(f, Mul(e, sk[i, 4])), sk[i, 5])));
			d4 = XOR(d, Summ(Mul(e, sk[i, 4]), Mul(Summ(f, Mul(e, sk[i, 4])), sk[i, 5])));
		}

		public void IdeaRound(ref ushort d1, ref ushort d2, ref ushort d3, ref ushort d4, byte roundIndex, ushort[,] spcificKeys)
		{
			if(keys == null)
			{
				throw new Exception("You must to init the key firstly before the algorithm starts!");
			}

			ushort a = Mul(d1, spcificKeys[roundIndex, 0]);
			ushort b = Summ(d2, spcificKeys[roundIndex, 1]);
			ushort c = Summ(d3, spcificKeys[roundIndex, 2]);
			ushort d = Mul(d4, spcificKeys[roundIndex, 3]);

			ushort bufBlue1 = XOR(a, c);
			ushort bufBlue2 = XOR(b, d);
			ushort bufRed1 = Mul(keys[roundIndex, 4], bufBlue1);
			ushort bufGreen1 = Summ(bufRed1, bufBlue2);
			ushort bufRed2 = Mul(bufGreen1, keys[roundIndex, 5]);
			ushort bufGreen2 = Summ(bufRed1, bufRed2);

			d1 = XOR(a, bufRed2);
			d2 = XOR(bufRed2, c);
			d3 = XOR(b, bufGreen2);
			d4 = XOR(bufGreen2, d);
		}

		public ushort MultiplexialInvertion(ushort x)
		{
			ushort result;

			uint t0, t1;
			uint q, y;

			if (x <= 1)
				return x;
			
			t1 = (module + 1) / x;
			y = (module + 1) % x;

			if (y == 1)
			{
				/* 0 and 1 are self-inverse */
				/* Since x >= 2, this fits into 16 bits */
				uint buf = (1 - t1) & (module - 1);
				result = Convert.ToUInt16(buf);
				return result;
			}
			t0 = 1;
			do
			{
				q = x / y;
				x = (ushort)(x % y);
				t0 += q * t1;
				if (x == 1)
				{
					result = (ushort)t0;
					return result;
				}
				q = y / x;
				y = y % x;
				t1 += q * t0;
			} while (y != 1);

			uint buf1 = (1 - t1) & (module - 1);
			result = Convert.ToUInt16(buf1);

			return result;
		}

		public ushort AdditionalInversion(ushort x)
		{
			ushort result;

			if (x != 0)
			{
				uint resultBuf = module - x;
				result = Convert.ToUInt16(resultBuf);
			}
			else
			{
				throw new Exception("Argument for AdditionalInversion method has empty value!");
			}

			return result;
		}

		//two 16-bit numbers
		public ushort XOR(ushort firstN, ushort secondN)
		{
			ushort result;

			result = Convert.ToUInt16(firstN ^ secondN);

			return result;
		}

		//two 16-bit numbers
		public ushort Summ(ushort firstN, ushort secondN)
		{
			ushort result;

			int algebraSum = firstN + secondN;
			/*65546 = pow(2, 16)*/
			result = Convert.ToUInt16(algebraSum % 65536);

			return result;
		}

		//two 16-bit numbers
		public ushort Mul(ushort firstN, ushort secondN)
		{
			ushort result;
			uint l, r;

			l = firstN;
			r = secondN;

			if (l == 0)
			{ l = module; }
			if(r == 0)
			{ r = module; }

			if ((l == module) && (r == module))
			{
				ulong ll = l;
				ulong lr = r;
				//ulong longProduct = ll * lr;
				ulong longProduct = 4294967296; // to avoid expensive multiplication
				result = Convert.ToUInt16(longProduct % (module + 1));
			}
			else
			{
				uint product = l * r;
                //was chanched (was if( product) ..)
                uint restOfTheDivision = product % (module + 1);

                if (restOfTheDivision != module)
				{
					result = Convert.ToUInt16(restOfTheDivision);
				}
				else
				{
					result = 0;
				}
			}

			return result;
		}

		//userkey - 128-bit key 8 chars
		public void InitKeys(string userKey)
		{
			if(userKey != null)
			{
				if (userKey.Length == 8)
				{
					ushort[] digitUserKey = GetDigitsFromString(userKey);
					byte fIndex = 0;
					byte sIndex = 0;
					//ushort[] eightKeys = GetEightKeys(digitUserKey);
					AddRangeOfKeys(ref fIndex, ref sIndex, digitUserKey);

					//Just repeating
					for (int i = 0; i < 6; i++)
					{
						//digitUserKey = digitUserKey << 25;
						digitUserKey = BinaryShift(digitUserKey, countOfKeyShiftingBits);
						AddRangeOfKeys(ref fIndex, ref sIndex, digitUserKey);
					}

					InitDecKeys();
 				}
				else
				{
					throw new Exception("Lenth of th user key must be 128 bit!");
				}
			}
			else
			{
				throw new NullReferenceException("user key has null value in InitKeys method");
			}
		}

		private void InitDecKeys()
		{
			decKeys[0, 0] = MultiplexialInvertion(keys[8, 0]);
			decKeys[0, 1] = AdditionalInversion(keys[8, 1]);
			decKeys[0, 2] = AdditionalInversion(keys[8, 2]);
			decKeys[0, 3] = MultiplexialInvertion(keys[8, 3]);
			decKeys[0, 4] = keys[7, 4];
			decKeys[0, 5] = keys[7, 5];

			decKeys[1, 0] = MultiplexialInvertion(keys[7, 0]);
			decKeys[1, 1] = AdditionalInversion(keys[7, 2]);
			decKeys[1, 2] = AdditionalInversion(keys[7, 1]);
			decKeys[1, 3] = MultiplexialInvertion(keys[7, 3]);
			decKeys[1, 4] = keys[6, 4];
			decKeys[1, 5] = keys[6, 5];

			decKeys[2, 0] = MultiplexialInvertion(keys[6, 0]);
			decKeys[2, 1] = AdditionalInversion(keys[6, 2]);
			decKeys[2, 2] = AdditionalInversion(keys[6, 1]);
			decKeys[2, 3] = MultiplexialInvertion(keys[6, 3]);
			decKeys[2, 4] = keys[5, 4];
			decKeys[2, 5] = keys[5, 5];

			decKeys[3, 0] = MultiplexialInvertion(keys[5, 0]);
			decKeys[3, 1] = AdditionalInversion(keys[5, 2]);
			decKeys[3, 2] = AdditionalInversion(keys[5, 1]);
			decKeys[3, 3] = MultiplexialInvertion(keys[5, 3]);
			decKeys[3, 4] = keys[4, 4];
			decKeys[3, 5] = keys[4, 5];

			decKeys[4, 0] = MultiplexialInvertion(keys[4, 0]);
			decKeys[4, 1] = AdditionalInversion(keys[4, 2]);
			decKeys[4, 2] = AdditionalInversion(keys[4, 1]);
			decKeys[4, 3] = MultiplexialInvertion(keys[4, 3]);
			decKeys[4, 4] = keys[3, 4];
			decKeys[4, 5] = keys[3, 5];

			decKeys[5, 0] = MultiplexialInvertion(keys[3, 0]);
			decKeys[5, 1] = AdditionalInversion(keys[3, 2]);
			decKeys[5, 2] = AdditionalInversion(keys[3, 1]);
			decKeys[5, 3] = MultiplexialInvertion(keys[3, 3]);
			decKeys[5, 4] = keys[2, 4];
			decKeys[5, 5] = keys[2, 5];

			decKeys[6, 0] = MultiplexialInvertion(keys[2, 0]);
			decKeys[6, 1] = AdditionalInversion(keys[2, 2]);
			decKeys[6, 2] = AdditionalInversion(keys[2, 1]);
			decKeys[6, 3] = MultiplexialInvertion(keys[2, 3]);
			decKeys[6, 4] = keys[1, 4];
			decKeys[6, 5] = keys[1, 5];

			decKeys[7, 0] = MultiplexialInvertion(keys[1, 0]);
			decKeys[7, 1] = AdditionalInversion(keys[1, 2]);
			decKeys[7, 2] = AdditionalInversion(keys[1, 1]);
			decKeys[7, 3] = MultiplexialInvertion(keys[1, 3]);
			decKeys[7, 4] = keys[0, 4];
			decKeys[7, 5] = keys[0, 5];

			decKeys[8, 0] = MultiplexialInvertion(keys[0, 0]);
			decKeys[8, 1] = AdditionalInversion(keys[0, 1]);
			decKeys[8, 2] = AdditionalInversion(keys[0, 2]);
			decKeys[8, 3] = MultiplexialInvertion(keys[0, 3]);
		}

        public string DeleteAdditionalPartsFromSurrogetePairs(string text)
        {
            StringBuilder builder = new StringBuilder(text.Length);

            //must be at decrypt method and used to restore the original encrypted message if decryption wasn't successful
            List<int> indexesOfPropableErrorRemovingLeftBorder = new List<int>();

            for (int i = 0; i < text.Length; i++)
            {
                //left border
                if (text[i] == '\uD800')
                {
                    if (i + 1 < text.Length)
                    {
                        if (text[i + 1] == '\xDC00')
                        {
                            indexesOfPropableErrorRemovingLeftBorder.Add(i);

                            //ignoring the left border symbol
                            continue;
                        }
                        else if ((text[i + 1] > '\xDC00') && (text[i + 1] <= '\xDFFF'))
                        {
                            //ignoring the left border symbol
                            continue;
                        }
                    }
                }
                //right border
                else if (text[i] == '\uDC00')
                {
                    if ((i - 1) >= 0)
                    {
                        if ((text[i - 1] >= '\xD800') && (text[i - 1] <= '\xDBFF'))
                        {
                            //ignoring the right border symbol
                            continue;
                        }
                    }
                }

                builder.Append(text[i]);
            }

            return builder.ToString();
        }

        private ushort[] GetDigitsFromString(string userKey)
		{
			ushort[] result = new ushort[userKey.Length];

			for (int i = 0; i < userKey.Length; i++)
			{
				result[i] = Convert.ToUInt16(userKey[i]);
			}

			return result;
		}

		private void AddRangeOfKeys(ref byte fIndex, ref byte sIndex, ushort[] newKeys)
		{
			for (int i = 0; i < newKeys.Length; i++)
			{
				if (fIndex == 8)
				{
					//When the 53-s element want to be placed too - we stop it
					if(sIndex > 3)
					{
						return;
					}
				}
				if (sIndex > 5)
				{
					sIndex = 0;
					fIndex++;
				}
				try
				{
					keys[fIndex, sIndex] = newKeys[i];
					sIndex++;
				}
				catch(Exception ex)
				{
					Console.WriteLine("keys[{0} ; {1}] - such element wasn't found! Index is out of range!" + ex.Message, fIndex, sIndex);
				}
			}
		}

		//For key generation
		public ushort[] BinaryShift(ushort[] digitUserKey, ushort shiftingNumber)
		{
			ushort[] shiftedKey = new ushort[digitUserKey.Length];
			List<byte> oldBytes = new List<byte>(digitUserKey.Length * blockSize);
			List<byte> newBytes = new List<byte>(digitUserKey.Length * blockSize);

			for (int i = 0; i < digitUserKey.Length; i++)
			{
				string s = Convert.ToString(digitUserKey[i], 2);
				byte[] bits = s.PadLeft(blockSize, '0') // Add 0's from left
				                 .Select(c => byte.Parse(c.ToString())) // convert each char to int
			 					 .ToArray(); // Convert IEnumerable from select to Array
				//BitArray b = new BitArray(digitUserKey[i]);
				//byte[] bits = b.Cast<byte>().ToArray();
				oldBytes.AddRange(bits);
			}

			for (int i = shiftingNumber; i < oldBytes.Count; i++)
			{ 
				newBytes.Add(oldBytes[i]);
			}

			for (int i = 0; i < shiftingNumber; i++)
			{
				newBytes.Add(oldBytes[i]);
			}

			//Reverse is nesessary because array.Copy() will produce diametral array of bites 
			for (int k = 0; k < shiftedKey.Length; k++)
			{
				Boolean[] bites = new Boolean[blockSize];

				for (int j = blockSize - 1; j > -1; j--)
				{
					if (newBytes[k * blockSize + j] == 1)
					{
						bites[blockSize - j - 1] = true;
					}
					else
					{
						bites[blockSize - j - 1] = false;
					}
				}

				BitArray b = new BitArray(bites);
				shiftedKey[k] = GetUshortFromBitArray(b);
			}

			return shiftedKey;
		}

		private ushort GetUshortFromBitArray(BitArray bitArray)
		{
			if (bitArray.Length > blockSize)
				throw new ArgumentException(string.Format("Argument length shall be at most {0} bits.", blockSize));

			int[] array = new int[1];
			bitArray.CopyTo(array, 0);
			return (ushort)array[0];
		}
	}
}
