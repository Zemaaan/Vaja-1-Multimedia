using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace FELICS
{
	internal class Program
	{
		static byte[][] TestnaMatrika =
		{
			// Citati z indeksima (stupac, redak)

			new byte[] {23, 21, 21, 23, 23, 21, 23, 23},
			new byte[] {24, 22, 22, 20, 24, 22, 20, 24},
			new byte[] {23, 22, 22, 19, 23, 22, 19, 23},
			new byte[] {26, 25, 21, 19, 22, 21, 19, 22},
			new byte[] {23, 21, 21, 23, 23, 21, 23, 23},
			new byte[] {24, 22, 22, 20, 24, 22, 20, 24},
			new byte[] {23, 22, 22, 19, 23, 22, 19, 23},
			new byte[] {26, 25, 21, 19, 22, 21, 19, 22},
		};

		public static (ushort, byte, int, int) PreberiHeader()
		{
			ushort Visina;
			byte Prvi;
			int Zadnji;
			int n;

			using (BinaryReader reader = new BinaryReader(File.Open("D:\\FELICS\\FELICS\\Datoteka\\BinarnaDatoteka.bin", FileMode.Open)))
			{
				// Read an integer from the file
				Visina = reader.ReadUInt16();
				Prvi = reader.ReadByte();
				Zadnji = reader.ReadInt32();
				n = reader.ReadInt32();
			}

			return (Visina, Prvi, Zadnji, n);
		}

		public static void Main(string[] args)
		{
			Bitmap bmp = new Bitmap("E:\\FELICS\\FELICS\\Datoteka\\eStudij\\Baboon.bmp");
			FileStream fs = new FileStream(@"E:\FELICS\FELICS\Datoteka\BinarnaDatotekaZakodirana.bin", FileMode.Create);
			fs.Close();

			ushort height = (ushort) bmp.Height;

			List<int> E = Predict(bmp);
			List<int> C = new List<int>(E.Count);
			List<int> N = new List<int>(E.Count);

			for (int i = 0; i < E.Count; i++)
			{
				C.Add(-255);
				N.Add(-255);
			}

			N[0] = E[0];

			for (int i = 1; i < C.Count; i++)
			{
				if (C[i] >= 0)
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


			SetHeader(height, (byte) C[0], (byte) C[C.Count - 1], C.Count);

			string binarnadatoteka = @"E:\FELICS\FELICS\Datoteka\BinarnaDatotekaZakodirana.bin";
			string ZacetniBinarniTok = "";

			IC(ZacetniBinarniTok, C, 0, C.Count - 1);
			
			// Dekompresija spodaj
			// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		}
		
		static byte[,] ConvertImageToMatrix(string imagePath)
		{
			Bitmap bitmap = new Bitmap(imagePath);

			int width = bitmap.Width;
			int height = bitmap.Height;

			byte[,] matrix = new byte[width, height];

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					Color pixelColor = bitmap.GetPixel(x, y);
					// Assuming grayscale image, you may need to modify this based on your image format
					byte pixelValue = (byte)((pixelColor.R + pixelColor.G + pixelColor.B) / 3);
					matrix[x, y] = pixelValue;
				}
			}

			return matrix;
		}

		public static void SetHeader(ushort Visina, byte Prvi, byte Zadnji, int Dolzina, string pot = "E:\\FELICS\\FELICS\\Datoteka\\BinarnaDatoteka.bin")
		{
			try
			{
				Stream stream = new FileStream(pot, FileMode.Append);
				BinaryWriter B = new BinaryWriter(stream);

				B.Write(Visina);
				B.Flush();

				B.Write(Prvi);
				B.Flush();

				B.Write(Zadnji);
				B.Flush();

				B.Write(Dolzina);
				B.Flush();

				B.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
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
						// TODO: Implementirati a, b, c in x (Xp) piksle

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
		}


		public static void IC(string BinarniTok, List<int> C, int L, int H)
		{
			if (H - L > 1)
			{
				if (C[H] != C[L])
				{
					int m = (int) Math.Floor(0.5 * (H + L));
					int g = (int) Math.Ceiling(Math.Log(C[H] - C[L] + 1, 2));

					BinarniTok = Encode(BinarniTok, g, C[m] - C[L]);

					if (L < m)
					{
						IC(BinarniTok, C, L, (int) m);
					}

					if (m < H)
					{
						IC(BinarniTok, C, m, H);
					}
				}
			}
			// return List<BitArray>;
		}
		static string OstanekOdZadnjeIteracije = "";
		private static string Encode(string BitniNiz, int SteviloBitov, int SteviloZaKodirati)
		{
			if (SteviloBitov + OstanekOdZadnjeIteracije.Length > 8)
			{
				string PrvihOsem = OstanekOdZadnjeIteracije.Substring(0, 8);
				
				using (FileStream fileStream = new FileStream(@"E:\FELICS\FELICS\Datoteka\BinarnaDatotekaZakodirana.bin", FileMode.Append))
				{
					Console.WriteLine("Zapisujem {0} v datoteko", SteviloZaKodirati);
					Console.WriteLine("Zapisujem {0} v datoteko", PrvihOsem);
					fileStream.WriteByte((byte)SteviloZaKodirati);
				}
				
				OstanekOdZadnjeIteracije = OstanekOdZadnjeIteracije.Substring(8);
			}
			else
			{   // v binarni tok samo ci ima mesta v bufferu?
				string BinarnaStevilkaZaZapis = Convert.ToString(SteviloZaKodirati, 2);
				OstanekOdZadnjeIteracije += BinarnaStevilkaZaZapis;
			}
			return "BitniNiz";
		}

		public static void DeCompress(string LokacijaDatoteke)
		{
			int DatotekaVisina = -1;
			int DatotekaPrviC = -1;
			int DatotekaZadnjiC = -1;
			int DatotekaDolzina = -1;

			using (FileStream fileStream = new FileStream(LokacijaDatoteke, FileMode.Open))
			{
				using (BinaryReader reader = new BinaryReader(fileStream))
				{
					DatotekaVisina = reader.ReadInt16();
					DatotekaPrviC = reader.ReadByte();
					DatotekaZadnjiC = reader.ReadInt32();
					DatotekaDolzina = reader.ReadInt32();

					// Console.WriteLine($"Value 1: {DatotekaVisina}");
					// Console.WriteLine($"Value 2: {DatotekaPrviC}");
					// Console.WriteLine($"Value 3: {DatotekaZadnjiC}");
					// Console.WriteLine($"Value 3: {DatotekaDolzina}");
				}
			}

			int y = DatotekaDolzina / DatotekaVisina;
			List<int> C = new List<int>(DatotekaDolzina);
			C[0] = DatotekaVisina;
			C[1] = DatotekaDolzina;
			C[2] = DatotekaPrviC;
			C[3] = DatotekaZadnjiC;
		}

		public static void DeIC(string SeznamBitov, List<int> C, int L, int H)
		{
			if (H - L > 1)
			{
				if (C[L] == C[H])
				{
					for (int i = L + H + 1; i < H - 1; i++)
					{
						C[i] = C[L];
					}
				}
			}
			else
			{
				int m = (int) Math.Floor(0.5 * (H + L));
				int g = (int) Math.Ceiling(Math.Log(C[H] - C[L] + 1));

				string BitiZaBOdelavo = SeznamBitov.Substring(0, g);
				SeznamBitov = SeznamBitov.Substring(g, SeznamBitov.Length - 1);
				BitiZaBOdelavo.PadLeft(8, '0');
				int DekodiraniBiti = Convert.ToInt32(BitiZaBOdelavo, 2);

				C[m] = C[L] + DekodiraniBiti;
				if (L < m)
				{
					DeIC(SeznamBitov, C, L, m);
				}

				if (m < H)
				{
					DeIC(SeznamBitov, C, L, m);
				}
			}
		}

		public static void InversePrediction(List<int> C)
		{
			int DatotekaVisina = C[0];
			int DatotekaDolzina = C[1];
			int DatotekaPrviC = C[2];
			int DatotekaZadnjiC = C[3];
			// sirina v prosojnici je visina v kodu
			// x v prosojnicam je y v kodu
			int SirinaSlike = DatotekaDolzina / DatotekaVisina;
			List<int> E = new List<int>(DatotekaDolzina);

			int[,] P = new int[DatotekaVisina, SirinaSlike];

			for (int y = 0; y < DatotekaVisina; y++)
			{
				for (int x = 0; x < SirinaSlike; x++)
				{
					if (y == 0 && x == 0)
					{
						P[0, 0] = E[0];
					}

					if (x == 0)
					{
						P[y, 0] = P[y - 1, 0] - E[x * DatotekaVisina + y];
					}

					if (y == 0)
					{
						P[0, x] = P[0, x - 1] - E[x * DatotekaVisina + y];
					}
					else
					{
						if (P[y - 1, x - 1] >= Math.Max(P[y - 1, x], P[y, x - 1]))
						{
							P[y, x] = Math.Min(P[y - 1, x], P[y, x - 1]) - E[x * DatotekaVisina + y];
						}

						if (P[y - 1, x - 1] <= Math.Min(P[y - 1, x], P[y, x - 1]))
						{
							P[y, x] = Math.Max(P[y - 1, x], P[y, x - 1]) - E[x * DatotekaVisina + y];
						}
						else
						{
							P[y, x] = P[y - 1, x] + P[y, x - 1] - P[y - 1, x - 1] - E[x * DatotekaVisina + y];
						}
					}
				}
			}
		}
	}
}