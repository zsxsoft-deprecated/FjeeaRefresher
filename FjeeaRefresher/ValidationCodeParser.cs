using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace FjeeaRefresher
{
    class ValidationCodeParser
    {

        public static Dictionary<int, List<string>> studiedStore = new Dictionary<int, List<string>>();


        /// <summary>
        /// Initializes the store.
        /// </summary>
        /// <param name="storePath">The store path.</param>
        public static void InitStore(string storePath) => Enumerable.Range(0, 9).Select(i =>studiedStore[i] = Directory.GetFiles($"{storePath}\\{i}").Select(str => BitmapToString(new Bitmap(str))).ToList());
        /// <summary>
        /// Initializes the store from ResourceSet
        /// </summary>
        /// <param name="dictionaryStore">The dictionary store.</param>
        public static void InitStore(System.Resources.ResourceSet dictionaryStore)
        {
            Enumerable.Range(0,10).ToList().ForEach(i=> studiedStore[i] = new List<string>());
            foreach (DictionaryEntry item in dictionaryStore)
            {
                var resourceKey = item.Key.ToString();
                if (resourceKey[0] != '_') continue;
                var splittedKey = resourceKey.Split('_');
                var storeKey = Convert.ToInt32(splittedKey[1]);
                studiedStore[storeKey].Add(BitmapToString(((Bitmap)item.Value)));
            }
        }

        /// <summary>
        /// Gets the verify code.
        /// </summary>
        /// <param name="codeImagePath">The code image path.</param>
        /// <returns></returns>
        public static string GetVerifyCode(string codeImagePath) => GetVerifyCode(new Bitmap(codeImagePath));

        /// <summary>
        /// Gets the verify code.
        /// </summary>
        /// <param name="codeImage">The code image.</param>
        /// <returns></returns>
        public static string GetVerifyCode(Bitmap codeImage)
        {
            var mokeLikedStrings = DivideImage(RemoveNoise(Binarizate(codeImage))).Select(bitmap =>
            {
                var bitmapString = BitmapToString(bitmap);
                var minimumSimilarDegree = 32767;
                var mokeLikedNumber = 0;
                Enumerable.Range(0, 10).ToList().ForEach(i =>
                {
                    studiedStore[i].ForEach(item =>
                    {
                        var degree = CalcSimilarDegree(bitmapString, item);
                        if (degree < minimumSimilarDegree)
                        {
                            minimumSimilarDegree = degree;
                            mokeLikedNumber = i;
                        }
                    });
                });
                return mokeLikedNumber.ToString();
            });
            return string.Join("",mokeLikedStrings);
        }
        /// <summary>
        /// Clone a new Bitmap Object
        /// </summary>
        /// <param name="B">The Bitmap Object</param>
        /// <returns></returns>
        public static Bitmap Clone(Bitmap B) => B.Clone(new Rectangle(0, 0, B.Width, B.Height), B.PixelFormat);
        /// <summary>
        /// Binarizate the specified original bitmap.
        /// </summary>
        /// <param name="originalBitmap">The original bitmap object.</param>
        /// <see cref="http://www.scantips.com/lumin.html"/>
        /// <see cref="​http://stackoverflow.com/questions/596216/formula-to-determine-brightness-of-rgb-color"/>
        /// <returns></returns>
        public static Bitmap Binarizate(Bitmap originalBitmap)
        {
            var newBitmap = Clone(originalBitmap);
            for (int i = 0; i < newBitmap.Width; i++)
            {
                for (int j = 0; j < newBitmap.Height; j++)
                {
                    var color = newBitmap.GetPixel(i, j);
                    var gray = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                    var newColor = Color.FromArgb(gray, gray, gray);
                    var bppColor = newColor.B < 127 ? Color.Black : Color.White;
                    newBitmap.SetPixel(i, j, bppColor);
                }
            }
            return newBitmap;
        }


        /// <summary>
        /// Divides image into pieces
        /// </summary>
        /// <param name="row">The count of rows</param>
        /// <param name="column">The count of columns</param>
        /// <see cref="http://www.cnblogs.com/yuanbao/archive/2007/09/25/905322.html"/>
        public static Bitmap[] DivideImage(Bitmap bitmap, int row = 6, int column = 1)
        {
            var averageWidth = bitmap.Width / row;
            var averageHeight = bitmap.Height / column;
            var picArray = new Bitmap[row * column];

            for (int i = 0; i < column; i++)
            {
                for (int j = 0; j < row; j++)
                {
                    var cloneRect = new Rectangle(j * averageWidth, i * averageHeight, averageWidth, averageHeight);
                    picArray[i * row + j] = bitmap.Clone(cloneRect, bitmap.PixelFormat);//复制小块图
                }
            }
            return picArray;
        }


        /// <summary>
        /// Converts Bitmap to String
        /// </summary>
        /// <param name="bitmap">bitmap</param>
        /// <returns></returns>
        public static string BitmapToString(Bitmap bitmap)
        {
            var pixels = Enumerable.Range(0, bitmap.Width).Select(i => Enumerable.Range(0, bitmap.Height).Select(j =>
                   {
                       var color = bitmap.GetPixel(i, j);
                       return color.R < 127 ? "1" : "0";
                   }));
            return string.Join("", pixels);
        }

        /// <summary>
        /// Calculates the similar degree.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public static int CalcSimilarDegree(string a, string b)
        {
            var shortString = a.Length <= b.Length ? a : b;
            return Enumerable.Range(0, shortString.Length).Sum(i => a[i] != b[i] ? 1 : 0);
        }
        /// <summary>
        /// Removes the noise.
        /// </summary>
        /// <param name="dgGrayValue">The dg gray value.</param>
        /// <param name="MaxNearPoints">The maximum near points.</param>
        /// <returns></returns>
        public static Bitmap RemoveNoise(Bitmap originalBitmap, int dgGrayValue = 128, int MaxNearPoints = 3)
        {
            int nearDots = 0;
            var newBitmap = Clone(originalBitmap);
            for (int i = 0; i < newBitmap.Width; i++)
                for (int j = 0; j < newBitmap.Height; j++)
                {
                    var piexl = newBitmap.GetPixel(i, j);
                    if (piexl.R < dgGrayValue)
                    {
                        nearDots = 0;
                        //判断周围8个点是否全为空
                        if (i == 0 || i == newBitmap.Width - 1 || j == 0 || j == newBitmap.Height - 1)  //边框全去掉
                        {
                            newBitmap.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                        }
                        else
                        {
                            for (int k = i - 1; k <= i + 1; k++)
                                for (int l = j - 1; l <= j + 1; l++)
                                    if (i != j&& newBitmap.GetPixel(k, l).R < dgGrayValue) { nearDots++; }
                        }

                        if (nearDots < MaxNearPoints)
                            newBitmap.SetPixel(i, j, Color.FromArgb(255, 255, 255));   //去掉单点 && 粗细小3邻边点
                    }
                    else  //背景
                        newBitmap.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                }
            return newBitmap;

        }
    }
}
