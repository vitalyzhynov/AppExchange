using System;
using System.Text;

namespace IDEAChipher
{
	class KaracybuMethod
	{

		public int adPlusbc105 = 0; 
		public int adPlusbc72 = 0; 
		public int adPlusbc12 = 0;

		public string MultiLenMul(string X, string Y)
		{
			string result = string.Empty;

			if ((X != null) && (Y != null)) {
				//parcing the parameters
				string a, b, c, d;

				int powerForX;
				int powerForY;
				if ((IsPowerOfTwo (X, 0, out powerForX)) & (IsPowerOfTwo (Y, 0, out powerForY))) {
					//Console.WriteLine ("true " + powerForX + " " + powerForY);
					if (X.Length == Y.Length) {
						a = X.Substring (0, (X.Length) / 2);
						b = X.Substring ((X.Length) / 2);
						c = Y.Substring (0, (Y.Length) / 2);
						d = Y.Substring ((Y.Length) / 2);
						//Console.WriteLine (a);
						//Console.WriteLine (b);
						//Console.WriteLine (c);
						//Console.WriteLine (d);

						//Simplelest case (Last one or two cases)
						if ((X.Length == 2) && (Y.Length == 2)) {
							//xy = 10^n * ac + 10^(n/2)*(ad + bc) + bd
							try {
								int a_ = Convert.ToInt32 (a);
								int b_ = Convert.ToInt32 (b);
								int c_ = Convert.ToInt32 (c);
								int d_ = Convert.ToInt32 (d);
								int ac = a_ * c_; 
								int bd = b_ * d_;
								//ad + bc = (a + b)*(c + d) - ac - bd;
								int aPlusB = (a_ + b_);
								int cPlusD = (c_ + d_);
								int adPlusbc;
								int xy;

								if ((aPlusB > 9) || (cPlusD > 9)) {
									adPlusbc = Convert.ToInt32 (MultiLenMul (aPlusB.ToString (), cPlusD.ToString ())) - ac - bd;
								} else {
									adPlusbc = aPlusB * cPlusD - ac - bd;
								}
								//Console.WriteLine (adPlusbc);
								if(adPlusbc == 105){
									adPlusbc105++;
								} else if(adPlusbc == 72){
									adPlusbc72++;
								} else if(adPlusbc == 12){
									adPlusbc12++;
								}

								xy = 100 * ac + 10 * adPlusbc + bd;
								result = xy.ToString ();

							} catch (InvalidCastException iex) {
								Console.WriteLine (iex.Message);
							} catch (Exception ex) {
								Console.WriteLine (ex.Message);
							}
						} else {
							string ac = MultiLenMul (a, c);
							string bd = MultiLenMul (b, d);
							//ad+bc
							string aPlusB = LongAdd (a, b);
							string cPlusD = LongAdd (c, d);
							string aPlusBCPlusD = MultiLenMul(aPlusB, cPlusD);
							string acPlusBD = LongAdd (ac, bd);
							string adPlusbc = LongSub(aPlusBCPlusD, acPlusBD);
							//Console.WriteLine (adPlusbc);
							string adPlusbc1 = RemoveExcessNoughts(new StringBuilder(adPlusbc)).ToString();
							if(adPlusbc1.Equals("105")){
								adPlusbc105++;
							} else if(adPlusbc1.Equals("72")){
								adPlusbc72++;
							} else if(adPlusbc1.Equals("12")){
								adPlusbc12++;
							}
							//xy = 
							result = LongAdd(LongAdd(ac + new string('0', X.Length), adPlusbc + new string('0', X.Length / 2)), bd);
						}

					} else { 
						if (X.Length > Y.Length) {
							Y = AddNoughts (Y, X.Length);
						} else if (Y.Length > X.Length) {
							X = AddNoughts (X, Y.Length);
						}
						result = MultiLenMul (X, Y);
					}

				} else {
					if (powerForX > powerForY) {
						X = AddNoughts (X, powerForX);
						Y = AddNoughts (Y, powerForX);
					} else {
						X = AddNoughts (X, powerForY);
						Y = AddNoughts (Y, powerForY);
					}

					result = MultiLenMul (X, Y);
				}					
			}

			//result = RemoveExcessNoughts (new StringBuilder (result)).ToString();
			return result;
		}

		//exponent starts from 0
		public bool IsPowerOfTwo(string number, int exponent, out int power)
		{
			bool result = false;

			//initialization
			power = (int)Math.Pow (2, exponent);
			//int digitCount = (int)Math.Log10 (power) + 1;

			if (number.Length > power) {
				result = IsPowerOfTwo (number, exponent + 1, out power);
			} else if (number.Length == power) {
				result = true;
			} else {
				result = false;
			}

			return result;
		}

		public string AddNoughts(string inputNumber, int newLen)
		{
			string result = string.Empty;

			if (newLen < 0) {
				throw new ArgumentException ("new Len for number must be a positive!");
			}

			int countOfNoughts = newLen - inputNumber.Length;

			if (countOfNoughts > 0) {
				string noughts = new string ('0', countOfNoughts);
				result = noughts + inputNumber;
			} else {
				result = inputNumber;
			}

			return result;
		}

		public string LongAdd(string first, string second)
		{
			string result = string.Empty;
			int maxCount;
			int minCount;
			int maxInd;
			string minus = "";

			if (first.Equals ("")) {
				if (second.Equals ("")) {
					first = "0";
					second = "0";
				} else {
					first = new string ('0', second.Length);
				}	
			} else if (second.Equals ("")) {
				second = new string ('0', first.Length);
			}

			if ((first [0] == '-') && (second [0] != '-')) {
				first = first.Substring (1);
				result = LongSub (second, first);
				return result;
			} else if ((second [0] == '-') && (first [0] != '-')) {
				second = second.Substring (1);
				result = LongSub (first, second);
				return result;
			} else if ((first [0] == '-') && (second [0] == '-')) {
				first = first.Substring (1);
				second = second.Substring (1);
				minus = "-";
			}

			if (first.Length > second.Length) {
				maxCount = first.Length;
				minCount = second.Length;
				maxInd = 1;
			} else {
				maxCount = second.Length;
				minCount = first.Length;
				maxInd = 2;
			}

			StringBuilder builder = new StringBuilder (maxCount + 1);

			int localSum = 0;
			int memory = 0;
			int a, b;
			int i = 0;
			for (i = 0; i < minCount; i++) {
				a = Convert.ToInt32 ((first[first.Length - i - 1]).ToString());
				b = Convert.ToInt32 ((second[second.Length - i - 1]).ToString());
				localSum = a + b + memory;
				memory = 0;
					
				if (localSum > 9) {
					memory++;
					localSum = localSum - 10;
				}

				builder.Insert (0, localSum.ToString ());
				localSum = 0;
			}

			//localSum += memory;
			//memory = 0;
			//i -= 1;
			while (i < maxCount) {
				if (maxInd == 1) {
					a = Convert.ToInt32 ((first[first.Length - i - 1]).ToString());
					localSum = a + memory;
					memory = 0;
				} else if (maxInd == 2) {
					b = Convert.ToInt32 ((second[second.Length - i - 1]).ToString());
					localSum = b + memory;
					memory = 0;
				}
				 
				if (localSum > 9) {
					memory++;
					localSum = localSum - 10;
				}

				builder.Insert (0, localSum.ToString());
				localSum = 0;
				i++;
			}

			if (memory != 0) {
				builder.Insert (0, memory.ToString ());
			}

			result = minus + builder.ToString ();

			return result;
		}

		public string LongSub(string first, string second)
		{
			string result = string.Empty;
			StringBuilder bigger;
			string smaller;
			string minus = "";
			int a, b;

			if ((first [0] == '-') && (second [0] != '-')) {
				first = first.Substring (1);
				result = "-" + LongAdd (first, second);
				return result;
			} else if ((second [0] == '-') && (first [0] != '-')) {
				second = second.Substring (1);
				result = LongAdd (first, second);
				return result;
			} else if ((first [0] == '-') && (second [0] == '-')) {
				first = first.Substring (1);
				second = second.Substring (1);
				result = LongSub (second, first);
				return result;
			}

			if (LongGreaterThan(first, second)){
				bigger = new StringBuilder (first, first.Length + 1);
				smaller = second;
			} else {
				bigger = new StringBuilder (second, second.Length + 1);
				smaller = first;
				minus = "-";
			}

			StringBuilder builder = new StringBuilder (bigger.Length);
			int localSum = 0;
			int i;
			for (i = 0; i < smaller.Length; i++) {
				a = Convert.ToInt32 ((bigger[bigger.Length - i - 1]).ToString());
				b = Convert.ToInt32 ((smaller[smaller.Length - i - 1]).ToString());
				localSum = a - b;

				if (localSum < 0) {
					//call the ten from elder rank
					//Changes the in string begger
					localSum += GetAdditionalTen (bigger.Length - i - 2, bigger);

				}

				builder.Insert (0, Convert.ToString (localSum));
				localSum = 0;
			}

			string buf = (bigger.ToString ()).Substring (0, bigger.Length - i);
			builder.Insert (0, buf);
			builder = RemoveExcessNoughts (builder);

			result = minus + builder.ToString ();

			return result;
		}

		public int GetAdditionalTen(int i, StringBuilder bigger)
		{
			int result;

			if ((i >= 0) && (i < bigger.Length)) {
				int currentElement = Convert.ToInt32 (bigger [i].ToString());
				if (currentElement != 0) {
					result = 10;
				} else {
					result = GetAdditionalTen (i - 1, bigger);
					currentElement = 10;
				}

				if (result != 0) {
					bigger [i] = Convert.ToChar ((currentElement - 1).ToString ());
				}
			} else {
				result = 0;
			}

			return result;
		}

		public StringBuilder RemoveExcessNoughts(StringBuilder str)
		{
			StringBuilder result = new StringBuilder (str.Length);

			bool mozhna = true;
			for (int i = 0; i < str.Length; i++) {
				if (mozhna) {
					if (str [i] == '0') {
						continue;
					} else {
						mozhna = false;
						result.Append (str [i]);
					}
				} else {
					result.Append (str [i]);
				}
			}

			return result;
		}

		public bool LongGreaterThan(string first, string second)
		{
			Boolean result = false;

			if (first.Length > second.Length) {
				result = true;
			} else if (second.Length > first.Length) {
				result = false;
			} else {
				int f;
				int s;
				for (int i = 0; i < first.Length; i++) {
					f = Convert.ToInt32 (first [i].ToString());
					s = Convert.ToInt32 (second [i].ToString());
					if (f > s) {
						result = true;
						break;
					} else if (s > f) {
						result = false;
						break;
					} else {
						//continue;
					}
				}
			}

			return result;
		}

	}
}
