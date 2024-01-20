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
			
			Compress(bmp, bmp.Height, bmp.Width);

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
			string ZacetniBinarniTok = "";
			
			SetHeader((ushort)P.Height, (byte)C[0], C[C.Count - 1], C.Count);
			ZacetniBinarniTok = IC(ZacetniBinarniTok, C, 0, C.Count - 1);
			return ZacetniBinarniTok;
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
		}

		public static string IC(string BinarniTok, List<int> C, int L, int H)
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
						IC(BinarniTok, C, L, m);
					}

					if (m < H)
					{
						IC(BinarniTok, C, m, H);
					}
				}
			}
			return BinarniTok;
		}
		
		static string BitniNizZaDodati = "";
		private static string Encode(string BitniNiz, int SteviloBitov, int SteviloZaKodirati)
		{
			// if (SteviloZaKodirati > 255)
			// {
			// 	Console.WriteLine("Napaka - večje od 255 - {0}", SteviloZaKodirati);
			// 	Console.ReadKey();
			// }
			if (BitniNizZaDodati.Length >= 8) // ce je trenutno 8 ali vec bitov, potem prebermo prvih osem bitov
			{
				string PrvihOsem = BitniNizZaDodati.Substring(0, 8); // En byte, zapisan v oblikis stringa
				byte ŠtevilkaZaZapis = Convert.ToByte(PrvihOsem, 2);
				
				using (FileStream fileStream = new FileStream(@"E:\FELICS\FELICS\Datoteka\BinarnaDatotekaZakodirana.bin", FileMode.Append))
				{
					fileStream.WriteByte(ŠtevilkaZaZapis);
					fileStream.Flush();
					BitniNiz += PrvihOsem;
					Console.WriteLine("Zapisujem {0} kot {1}", ŠtevilkaZaZapis, PrvihOsem);
				}
				BitniNizZaDodati = BitniNizZaDodati.Substring(8); // Odstranimo prvih 8
				BitniNizZaDodati += Convert.ToString(SteviloZaKodirati, 2); // Dodamo naslednjo število v buffer
			}
			else{
				Console.WriteLine("Dodajam število {0} na seznam v obliki {1} z parametrom SteviloBitov/g: {2}", SteviloZaKodirati, Convert.ToString(SteviloZaKodirati, 2), SteviloBitov);
				BitniNizZaDodati += Convert.ToString(SteviloZaKodirati, 2); // Dodamo naslednjo število v buffer
			}
			return BitniNiz;
		}

		public static void DeCompress(string LokacijaDatoteke = "E:\\FELICS\\FELICS\\Datoteka\\BinarnaDatotekaZakodirana.bin")
		{
			Int16 DatotekaVisina = 0;
			byte DatotekaPrviC = 0;
			int DatotekaZadnjiC = -1;
			int SteviloVsehElementov = -1;

			string BinarniTok = "";
			using (BinaryReader reader = new BinaryReader(File.Open(LokacijaDatoteke, FileMode.Open)))
			{
				try
				{
					DatotekaVisina = reader.ReadInt16();
					Console.WriteLine($"DatotekaVisina Value: {DatotekaVisina}");

					DatotekaPrviC = reader.ReadByte();
					Console.WriteLine($"DatotekaPrviC Value: {DatotekaPrviC}");

					DatotekaZadnjiC = reader.ReadInt32();
					Console.WriteLine($"DatotekaZadnjiC Value: {DatotekaZadnjiC}");

					SteviloVsehElementov = reader.ReadInt32();
					Console.WriteLine($"DatotekaDolzina Value: {SteviloVsehElementov}");
					
					while (reader.BaseStream.Position < reader.BaseStream.Length)
					{
						byte currentByte = reader.ReadByte();
						BinarniTok += Convert.ToString(currentByte, 2);
					}
				}
				catch (EndOfStreamException e)
				{
					Console.WriteLine("Error reading from the file: " + e.Message);
				}
			}

			int X = SteviloVsehElementov / DatotekaVisina;
			List<int> C = new List<int>();
			List<int> E = new List<int>();
			List<int> N = new List<int>();
			
			for(int i = 0; i < SteviloVsehElementov; i++) E.Add(-255);
			for(int i = 0; i < SteviloVsehElementov; i++) N.Add(-255);
			for(int i = 0; i < SteviloVsehElementov; i++) C.Add(-255);
			
			C[0] = DatotekaPrviC;
			C[C.Count - 1] = DatotekaZadnjiC;

				C = DeIC(BinarniTok, C, 0, SteviloVsehElementov - 1);
			N[0] = C[0];
			for (int i = 1; i < SteviloVsehElementov; i++)
			{
				N[i] = C[i] - C[i - 1];
			}

			E[0] = N[0];
			for (int i = 1; i < SteviloVsehElementov; i++)
			{
				if (N[i] % 2 == 0) E[i] = N[i] / 2;
				else E[i] = -(N[i] + 1) / 2;
			}

			int[,] P = InversePrediction(E, DatotekaVisina, X);
			Console.WriteLine();
		}

		public static List<int> DeIC(string SeznamBitov, List<int> C, int L, int H)
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
			return C;
		}

		public static int[,] InversePrediction(List<int> E, int VisinaX, int SirinaY)
		{
			
			// TODO: implementirati po uputama
			// TODO: Ce bi funkcija uporabljala sistem [stolpec, vrstica], potem bi zamenjali formule?
			// sirina v prosojnici je visina v kodu
			
			int[,] P = new int[VisinaX, SirinaY];

			for (int x = 0; x < VisinaX; x++)
			{
				for (int y = 0; y < SirinaY; y++)
				{
					if (x == 0 && y == 0)
					{
						P[0,0] = E[0];
					}

					else if (y == 0)
					{
						P[x, 0] = P[x - 1, 0] - E[y * VisinaX + x];
					}

					else if (x == 0)
					{
						P[0, y] = P[0, y-1] - E[y * VisinaX + x];
					}
					else
					{
						if (P[x - 1, y - 1] >= Math.Max(P[x-1, y], P[x, y-1]))
						{
							P[x, y] = Math.Min(P[x - 1, y], P[x, y - 1]) - E[y * VisinaX + x];
						}
						else if (P[x - 1, y - 1] >= Math.Min(P[x-1, y], P[x, y-1]))
						{
							P[x, y] = Math.Max(P[x - 1, y], P[x, y - 1]) - E[y * VisinaX + x];
						}
						else
						{
							P[x, y] = P[x - 1, y] + P[x, y - 1] - P[x - 1, y - 1] - E[y * VisinaX + x];
						}
					}
				}
			}
			Console.WriteLine();
			return P;
		}
	}
}