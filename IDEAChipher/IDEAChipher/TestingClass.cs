using System;
using NUnit.Framework;

namespace IDEAChipher
{
	[TestFixture]
	public class TestingClass
	{
		public TestingClass()
		{
		}

		[Test]
		public void XorOfTwoShortNumbers()
		{
			ushort expected = 10;
			ushort actual;

			IdeaChipher ic = new IdeaChipher();
			ushort l = 255;
			ushort r = 245;
			actual = ic.XOR(l, r);

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void XorOfTwoShortNumbersOnBorder()
		{
			ushort expected = 0;
			ushort actual;

			IdeaChipher ic = new IdeaChipher();
			ushort l = 65535;
			ushort r = 65535;
			actual = ic.XOR(l, r);

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void SumOfTwoShortNumbersGettingMod()
		{
			ushort expected = 4;
			ushort actual;

			IdeaChipher ic = new IdeaChipher();
			ushort l = 65535;
			ushort r = 5;
			actual = ic.Summ(l, r);

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void MulTwoZeroesGetting1()
		{
			IdeaChipher ic = new IdeaChipher();

			ushort expected = 1;
			ushort l = 0;
			ushort r = 0;

			ushort actual = ic.Mul(l, r);

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void ShiftToFourBitesIsFirstNumberCorrect()
		{
			IdeaChipher ic = new IdeaChipher();
			ushort[] shifted = ic.BinaryShift(new ushort[] { 1, 2 }, 4);

			ushort expected = 16;
			ushort actual = shifted[0];

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void ShiftToFourBitesIsSecondNumberCorrect()
		{
			IdeaChipher ic = new IdeaChipher();
			ushort[] shifted = ic.BinaryShift(new ushort[] { 1, 2 }, 4);

			ushort expected = 32;
			ushort actual = shifted[1];

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void MultiplexialInvertionOf128Returns65025()
		{
			IdeaChipher ic = new IdeaChipher();
			ushort expected = 65025;
			ushort parametr = 128;
			ushort actual = ic.MultiplexialInvertion(parametr);

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void MultiplexialInvertionOf7Returns18725()
		{
			IdeaChipher ic = new IdeaChipher();
			ushort expected = 18725;
			ushort parametr = 7;
			ushort actual = ic.MultiplexialInvertion(parametr);

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void IsInversionKeysCorrectUserKey12345678()
		{
			string key = new string(new char[] { '\x0001', '\x0002', '\x0003', '\x0004', '\x0005', '\x0006', '\x0007', '\x0008' });
			IdeaChipher ic = new IdeaChipher(key);
			ushort[,] decKeys = new ushort[9, 6];
			decKeys[0, 0] = '\xfe01';
			decKeys[0, 1] = '\xff40';
			decKeys[0, 2] = '\xff00';
			decKeys[0, 3] = '\x659a';
			decKeys[0, 4] = '\xc000';
			decKeys[0, 5] = '\xe001';

			decKeys[1, 0] = '\xfffd';
			decKeys[1, 1] = '\x8000';
			decKeys[1, 2] = '\xa000';
			decKeys[1, 3] = '\xcccc';
			decKeys[1, 4] = '\x0000';
			decKeys[1, 5] = '\x2000';

			decKeys[2, 0] = '\xa556';
			decKeys[2, 1] = '\xffb0';
			decKeys[2, 2] = '\xffc0';
			decKeys[2, 3] = '\x52ab';
			decKeys[2, 4] = '\x0010';
			decKeys[2, 5] = '\x0020';

			decKeys[3, 0] = '\x554b';
			decKeys[3, 1] = '\xff90';
			decKeys[3, 2] = '\xe000';
			decKeys[3, 3] = '\xfe01';
			decKeys[3, 4] = '\x0800';
			decKeys[3, 5] = '\x1000';

			decKeys[4, 0] = '\x332d';
			decKeys[4, 1] = '\xc800';
			decKeys[4, 2] = '\xd000';
			decKeys[4, 3] = '\xfffd';
			decKeys[4, 4] = '\x0008';
			decKeys[4, 5] = '\x000c';

			decKeys[5, 0] = '\x4aab';
			decKeys[5, 1] = '\xffe0';
			decKeys[5, 2] = '\xffe4';
			decKeys[5, 3] = '\xc001';
			decKeys[5, 4] = '\x0010';
			decKeys[5, 5] = '\x0014';

			decKeys[6, 0] = '\xaa96';
			decKeys[6, 1] = '\xf000';
			decKeys[6, 2] = '\xf200';
			decKeys[6, 3] = '\xff81';
			decKeys[6, 4] = '\x0800';
			decKeys[6, 5] = '\x0a00';

			decKeys[7, 0] = '\x4925';
			decKeys[7, 1] = '\xfc00';
			decKeys[7, 2] = '\xfff8';
			decKeys[7, 3] = '\x552b';
			decKeys[7, 4] = '\x0005';
			decKeys[7, 5] = '\x0006';

			decKeys[8, 0] = '\x0001';
			decKeys[8, 1] = '\xfffe';
			decKeys[8, 2] = '\xfffd';
			decKeys[8, 3] = '\xc001';

			CollectionAssert.AreEqual(decKeys, ic.decKeys);
		}


	}
}
