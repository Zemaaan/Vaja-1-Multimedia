using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace FELICS
{
	internal class Program
	{
		static byte[][] TestnaMatrika = new byte[][]
		{
			new byte[] {23, 21, 21, 23, 23},
			new byte[] {24, 22, 22, 20, 24},
			new byte[] {23, 22, 22, 19, 23},
			new byte[] {26, 25, 21, 19, 22},
		};
		
		public static void Main(string[] args)
		{
			int SteviloVrstic = 4;
			int SteviloStolpcev = 5;
			Bitmap TestniBitmap = new Bitmap(SteviloStolpcev, SteviloVrstic);

			for (int StevilkaVrstice = 0; StevilkaVrstice < 4; StevilkaVrstice++)
			{
				for (int StevilkaStolpca = 0; StevilkaStolpca < 5; StevilkaStolpca++)
				{
					TestniBitmap.SetPixel(StevilkaStolpca, StevilkaVrstice, Color.FromArgb(TestnaMatrika[StevilkaVrstice][StevilkaStolpca], TestnaMatrika[StevilkaVrstice][StevilkaStolpca], TestnaMatrika[StevilkaVrstice][StevilkaStolpca]));
					// Console.WriteLine("{0}, {1}, {2}", StevilkaVrstice, StevilkaStolpca, TestniBitmap.GetPixel(StevilkaStolpca, StevilkaVrstice));
				}
			}

			// Console.WriteLine(TestniBitmap.GetPixel(1, 0).B.ToString()); // Prvi parameter je stolpec, drugi je vrstica
			// Console.WriteLine(TestnaMatrika[1][0]); // Prvi parameter je stolpec, drugi je vrstica
			// Environment.Exit(0);
			Compress(TestniBitmap, true);
		}

		public static void Compress(Bitmap image, bool Test)
		{
			int VisinaYOS = image.Height;
			int SirinaXOS = image.Width;

			List<int> E = new List<int>(VisinaYOS * SirinaXOS);
			List<int> C = new List<int>(VisinaYOS * SirinaXOS);
			List<int> N = new List<int>(VisinaYOS * SirinaXOS);
			
			for (int i = 0; i < VisinaYOS * SirinaXOS; i++)
			{
				E.Add(-255);
				C.Add(-255);
				N.Add(-255);
			}
			for (int y = 0; y < VisinaYOS; y++)
			{
				for (int x = 0; x < SirinaXOS; x++)
				{
					if (x == 0 && y == 0)
					{
						E[y * SirinaXOS + x] = image.GetPixel(0,0).B;
					}
					else if (y == 0)
					{
						E[y * SirinaXOS + x] = image.GetPixel(x-1, 0).B - image.GetPixel(x, 0 ).B; // Popravljeno
					}
					else if (x == 0)
					{
						E[y * SirinaXOS + x] = image.GetPixel(0,y-1).B - image.GetPixel(0,y).B;
					}
					else
					{ // TODO: Implementirati a, b, c in x (Xp) piksle
						
						byte PikselC = image.GetPixel(x - 1, y - 1).B;
						byte PikselB = image.GetPixel(x, y - 1).B;
						byte PikselA = image.GetPixel(x - 1, y).B; // Above left
						byte PikselX = image.GetPixel(x, y).B;
						
						if (PikselC >= Math.Max(PikselA, PikselB))
						{
							E[y * SirinaXOS + x] = Math.Min(PikselA, PikselB) - PikselX;
						}
						else if(PikselC <= Math.Min(PikselA, PikselB))
						{
							E[y * SirinaXOS + x] = Math.Max(PikselA, PikselB) - PikselX;
						}
						else
						{
							E[y * SirinaXOS + x] = (PikselA + PikselB - PikselC) - PikselX;
						}
					}
					
				}
			}

			int n = VisinaYOS * SirinaXOS;
			N[0] = E[0];

			for (int i = 0; i < n; i++)
			{
				if (E[i] >= 0)
				{
					N[i] = 2 * E[i];
				}
				else
				{
					N[i] = Math.Abs(2 * E[i]);
				}
			}

			C[0] = N[0];
			for (int i = 1; i < n; i++)
			{
				C[i] = C[i - 1] + N[i];
			}

			int[] SeznamHeaderVrednosti = {VisinaYOS, C[0], C[C.Count - 1], n};
			BitArray B = new BitArray(88);
			int PozicijaKodiranjaBitnegaSeznama = 0;


			if (Test)
			{
				for (int i = 0; i < E.Count; i++) {Console.WriteLine(E[i]);}
				Console.WriteLine("-----------------");
				for (int i = 0; i < N.Count; i++) {Console.WriteLine(N[i]);}
				Console.WriteLine("-----------------");
				for (int i = 0; i < C.Count; i++) {Console.WriteLine(C[i]);}
				Console.WriteLine("-----------------");
				// for (int i = 0; i < image.Width; i++)
				// {
				// 	for (int j = 0; j < image.Height; j++)
				// 	{
				// 		Console.WriteLine("{0}, {1}, {2}, {3}, {4}", i, j, image.GetPixel(i, j).B, TestnaMatrika[j][i], image.GetPixel(i, j).B==TestnaMatrika[j][i]);
				// 	}
				// }
			}
			
			foreach (int HeaderValue in SeznamHeaderVrednosti)
			{
				/*Console.WriteLine(HeaderValue);
				Console.WriteLine(Convert.ToString(HeaderValue, 2));
				Console.WriteLine("-------------------------------------------------------");*/
				string BinarnaReprezentacijaString = Convert.ToString(HeaderValue, 2);

				for (int Indeks = 0; Indeks < BinarnaReprezentacijaString.Length; Indeks++)
				{
					if (BinarnaReprezentacijaString[Indeks] == '0') B[PozicijaKodiranjaBitnegaSeznama] = false;
					else B[PozicijaKodiranjaBitnegaSeznama] = true;
					PozicijaKodiranjaBitnegaSeznama += 1;
				}
			}
		}
		}
	}
