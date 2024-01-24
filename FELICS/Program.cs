using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace FELICS
{
	internal class Program
	{
		private static string BinarniTok = "";
		
		// Novi DeIC
		static byte[][] TestnaMatrika =
		{
			// Citati z indeksima (stupac, redak)

			new byte[] {23, 21, 21, 23, 23},
			new byte[] {24, 22, 22, 20, 24},
			new byte[] {23, 22, 22, 19, 23},
			new byte[] {26, 25, 21, 19, 22},
		};

		private static bool Test = true;

		public static void Main(string[] args)
		{ 
			File.Delete("E:\\FELICS\\FELICS\\Datoteka\\BinarnaDatotekaZakodirana.bin");
			File.Delete("E:\\FELICS\\FELICS\\Datoteka\\BinarnaDatoteka.bin");
			
			Bitmap bmp;
			
			if (Test) bmp = PretvoriMatrikoVBitmap(TestnaMatrika);
			else bmp = new Bitmap("E:\\FELICS\\FELICS\\Datoteka\\eStudij\\Mosaic.bmp");


			FileStream fs = new FileStream(@"E:\FELICS\FELICS\Datoteka\BinarnaDatotekaZakodirana.bin", FileMode.Create);
			fs.Close();

			string tok = Compress(bmp, bmp.Height, bmp.Width);
			Console.WriteLine();
			// Dekompresija spodaj
			// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
			DeCompress();
		}

		public static Bitmap PretvoriMatrikoVBitmap(byte[][] VhodnaMatrika)
		{
			Bitmap IzhodnaVrednost = new Bitmap(5, 4);

			for (int Vrstica = 0; Vrstica < 4; Vrstica++)
			{
				for (int Stolpec = 0; Stolpec < 5; Stolpec++)
				{
					IzhodnaVrednost.SetPixel(Stolpec, Vrstica, Color.FromArgb(VhodnaMatrika[Vrstica][Stolpec]));
				}
			}
			return IzhodnaVrednost;
		}

		public static string Compress(Bitmap P, int X, int Y)
		{
			List<int> E = Predict(P);
			List<int> C = new List<int>(X * Y);
			List<int> N = new List<int>(X * Y);

			for (int i = 0; i < E.Count; i++) C.Add(-255);
			for (int i = 0; i < E.Count; i++) N.Add(-255);

			N[0] = E[0];

			for (int i = 1; i < C.Count; i++)
			{
				if (E[i] >= 0)
				{
					N[i] = 2 * E[i];
				}
				else
				{
					N[i] = 2 * Math.Abs(E[i]) - 1;
				}
			}

			C[0] = N[0];

			for (int i = 1; i < C.Count; i++)
			{
				C[i] = C[i - 1] + N[i];
			}

			string binarnadatoteka = @"E:\FELICS\FELICS\Datoteka\BinarnaDatotekaZakodirana.bin";

			SetHeader((ushort) P.Height, (byte) C[0], C[C.Count - 1], C.Count);
			BinarniTok += Convert.ToString(P.Height, 2).PadLeft(16, '0');
			BinarniTok += Convert.ToString(C[0], 2).PadLeft(8, '0');
			BinarniTok += Convert.ToString(C[C.Count - 1], 2).PadLeft(32, '0');
			BinarniTok += Convert.ToString(C.Count, 2).PadLeft(32, '0');
			// BinarniTok += "---";
			IC(C, 0, C.Count - 1);
			Console.WriteLine();
			// BinarniTok.Insert(Mesto)
			Console.WriteLine(BinarniTok.GetHashCode());
			return BinarniTok;
		}


		public static string SetHeader(ushort Visina, byte Prvi, int Zadnji, int Dolzina, string pot = "E:\\FELICS\\FELICS\\Datoteka\\BinarnaDatotekaZakodirana.bin")
		{
			// string Izhod = "";
			// Izhod += Convert.ToString(Visina, 2);
			// Izhod += Convert.ToString(Prvi, 2);
			// Izhod += Convert.ToString(Zadnji, 2);
			// Izhod += Convert.ToString(Dolzina, 2);
			try
			{
				Stream stream = new FileStream(pot, FileMode.Append);
				BinaryWriter B = new BinaryWriter(stream);

				B.Write(Visina);

				B.Write(Prvi);

				B.Write(Zadnji);

				B.Write(Dolzina);

				B.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
			// return Izhod;
			return "";
		}


		public static List<int> Predict(Bitmap image)
		{
			short height = (short) image.Height;
			short width = (short) image.Width;
			int Counter = 1;

			List<int> E = new List<int>(height * width);

			for (int i = 0; i < height * width; i++) E.Add(-255);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if (y == 0 && x == 0)
					{
						E[x * height + y] = image.GetPixel(0, 0).B;
					}
					else if (x == 0)
					{
						E[x * height + y] = image.GetPixel(0, y - 1).B - image.GetPixel(0, y).B; // Popravljeno
					}
					else if (y == 0)
					{
						E[x * height + y] = image.GetPixel(x - 1, 0).B - image.GetPixel(x, 0).B;
					}
					else
					{
						byte PikselC = image.GetPixel(x - 1, y - 1).B;
						byte PikselB = image.GetPixel(x, y - 1).B;
						byte PikselA = image.GetPixel(x - 1, y).B; // Above left
						byte PikselX = image.GetPixel(x, y).B;

						if (PikselC >= Math.Max(PikselA, PikselB))
						{
							E[x * height + y] = Math.Min(PikselA, PikselB) - PikselX;
						}
						else if (PikselC <= Math.Min(PikselA, PikselB))
						{
							E[x * height + y] = Math.Max(PikselA, PikselB) - PikselX;
						}
						else
						{
							E[x * height + y] = (PikselA + PikselB - PikselC) - PikselX;
						}
					}

					Counter++;
				}
			}

			return E;
			Console.WriteLine();
		}

		public static string IC(List<int> C, int L, int H)
		{
			if (H - L > 1)
			{
				if (C[H] != C[L])
				{
					int m = (int) Math.Floor(0.5 * (H + L));
					int g = (int) Math.Ceiling(Math.Log(C[H] - C[L] + 1, 2));

					Encode(g, C[m] - C[L]);

					if (L < m)
					{
						IC(C, L, m);
					}

					if (m < H) 
					{
						IC(C, m, H);
					}
				}
			}
			return BinarniTok;
		}

		private static bool BitniSeznamNeankrat = false;
		static string BitniNizZaDodati = "";

		private static string Encode(int SteviloBitov, int SteviloZaKodirati)
		{
			Console.WriteLine("Kodiram {0} kot {1} z g={2}", SteviloZaKodirati, Convert.ToString(SteviloZaKodirati, 2).PadLeft(SteviloBitov, '0'), SteviloBitov);
			string Dodatek = Convert.ToString(SteviloZaKodirati, 2).PadLeft(SteviloBitov, '0');
			BinarniTok += Dodatek;
			return "";
		}

		public static int[,] DeCompress(string LokacijaDatoteke = "E:\\FELICS\\FELICS\\Datoteka\\BinarnaDatotekaZakodirana.bin")
		{
			Int16 DatotekaVisina = Convert.ToInt16(BinarniTok.Substring(0, 16), 2);
			BinarniTok = BinarniTok.Substring(16);
			
			byte DatotekaPrviC = Convert.ToByte(BinarniTok.Substring(0, 8), 2);
			BinarniTok = BinarniTok.Substring(8);

			int DatotekaZadnjiC = Convert.ToInt32(BinarniTok.Substring(0, 32), 2);
			BinarniTok = BinarniTok.Substring(32);

			int SteviloVsehElementov = Convert.ToInt32(BinarniTok.Substring(0, 32), 2);
			BinarniTok = BinarniTok.Substring(32);

			int Y = SteviloVsehElementov / DatotekaVisina;
			
			List<int> C = new List<int>();
			List<int> E = new List<int>();
			List<int> N = new List<int>();

			for (int i = 0; i < SteviloVsehElementov; i++) E.Add(0);
			for (int i = 0; i < SteviloVsehElementov; i++) N.Add(0);
			for (int i = 0; i < SteviloVsehElementov; i++) C.Add(0);

			C[0] = DatotekaPrviC;
			C[C.Count - 1] = DatotekaZadnjiC;

			C = DeIC(C, 0, SteviloVsehElementov - 1);

			N[0] = C[0];

			for (int i = 1; i < SteviloVsehElementov - 1; i++)
			{
				N[i] = C[i] - C[i - 1];
			}

			E[0] = N[0];
			
			for (int i = 1; i < SteviloVsehElementov - 1; i++)
			{
				if (N[i] % 2 == 0) E[i] = N[i] / 2;
				else E[i] = -(N[i] + 1) / 2;
			}

			int[,] P = InversePrediction(E, DatotekaVisina, Y);
			Console.WriteLine(); // Napaka v konstrukciji seznamov
			return P;
		}

		private static int ZacetnoSteviloBitov = 12;
		
		public static List<int> DeIC(List<int> C, int L, int H)
		{
			
			if (H - L > 1)
			{
				if (C[L] == C[H])
				{
					for (int i = L + 1; i < H - 1; i++)
					{
						C[i] = C[L];
					}
				}
				else
				{ // 8, 15 za testno matriko
					int m = (int)Math.Floor(0.5 * (H + L));
					int g = (int)Math.Ceiling(Math.Log(C[H] - C[L] + 1, 2));
					string PrvihGBitov = "";
					
					PrvihGBitov = BinarniTok.Substring(0, g);
					Console.WriteLine("Prenajšel {1} v obliki {0}", PrvihGBitov, Convert.ToInt16(PrvihGBitov, 2));
					BinarniTok = BinarniTok.Substring(g);

					byte SteviloDekodirano = Convert.ToByte(PrvihGBitov, 2);
					C[m] = C[L] + SteviloDekodirano;
					if (L < m)
					{
						DeIC(C, L, m);
					}

					if (m < H)
					{
						DeIC(C, m, H);
					}
				}
			}
			return C;
		}

		public static int[,] InversePrediction(List<int> E, int VisinaX, int SirinaY)
		{
			int[,] P = new int[VisinaX, SirinaY];

			for (int x = 0; x < VisinaX; x++)
			{
				for (int y = 0; y < SirinaY; y++)
				{
					int Konstanta = E[y * VisinaX + x];
					if (x == 0 && y == 0)
					{
						P[0, 0] = E[0];
					}
					else if (y == 0)
					{
						P[x, 0] = P[x - 1, 0] - Konstanta;
					}
					else if (x == 0)
					{
						P[0, y] = P[0, y - 1] - Konstanta;
					}
					else
					{
						if (P[x - 1, y - 1] >= Math.Max(P[x - 1, y], P[x, y - 1]))
						{
							P[x, y] = Math.Min(P[x - 1, y], P[x, y - 1]) - Konstanta;
						}
						else if (P[x - 1, y - 1] <= Math.Min(P[x - 1, y], P[x, y - 1]))
						{
							P[x, y] = Math.Max(P[x - 1, y], P[x, y - 1]) - Konstanta;
						}
						else
						{
							P[x, y] = P[x - 1, y] + P[x, y - 1] - P[x - 1, y - 1] - Konstanta;
						}
					}
				}
			}
			return P;
		}
	}
}