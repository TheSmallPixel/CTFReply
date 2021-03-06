﻿using IronBarCode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
namespace CTFReply
{
    class Program
    {
        [DllImport("user32.dll")]
        internal static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        internal static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        internal static extern bool SetClipboardData(uint uFormat, IntPtr data);

        [STAThread]
        public static void Main(String[] args)
        {//++++++++++[

            var wPath = Console.ReadLine();
            var cd = Console.ReadLine();
            int count = int.Parse(Console.ReadLine());
            var gen = new List<Generate>();
            gen.Add(new Java());
            gen.Add(new Java2());
            gen.Add(new Py());
            gen.Add(new JS());
            gen.Add(new JS2());
            gen.Add(new Py2());
            gen.Add(new Java3());
            gen.Add(new JS3());
            gen.Add(new Py3());
            gen.Add(new Php());
            gen.Add(new Php2());
            gen.Add(new Shell());
            gen.Add(new Shell2());
            gen.Add(new JS4());
            gen.Add(new Shell3());
            gen.Add(new Shell4());
            gen.Add(new Php3());
            gen.Add(new Java4());
            while (true)
            {
                string qr = System.IO.Directory.GetFiles(cd, "*.png").FirstOrDefault();
                string zip = System.IO.Directory.GetFiles(cd, "*.zip").FirstOrDefault();
                if (string.IsNullOrWhiteSpace(qr) || string.IsNullOrWhiteSpace(zip)) break;


                Regex regex = new Regex("\\\"+[\\w]+\\\"");
                Regex rp3 = new Regex("(?<=param2 = )+[\\d]+");
                Regex rp3v = new Regex("(?<=param2=)+[\\d]+");
                BarcodeResult Result = BarcodeReader.QuicklyReadOneBarcode(qr);
                if (Result == null) break;

                if (regex.Matches(Result.Text).Count == 0)
                {

                    var list = new List<DataPoint>();
                    var pointer = 0;
                    var outp = "";
                    for (int i = 0; i < Result.Text.Length && i >= 0; i++)
                    {
                        var c = Result.Text[i];
                        if (c == '.')
                        {
                            outp += (char)list.FirstOrDefault(x => x.index == pointer && x.operation == 'i').data;
                        }
                        if (c == '>') { pointer++; }
                        if (c == '<') { pointer--; }
                        if (c == '+')
                        {
                            if (list.Exists(x => x.index == pointer))
                            {
                                list.FirstOrDefault(x => x.index == pointer).data++;
                            }
                            else
                            {
                                list.Add(new DataPoint() { index = pointer, data = 1, operation = 'i' });
                            }
                        }
                        if (c == '-')
                        {
                            if (list.Exists(x => x.index == pointer))
                            {
                                list.FirstOrDefault(x => x.index == pointer).data--;
                            }
                            else
                            {
                                list.Add(new DataPoint() { index = pointer, data = -1, operation = 'i' });
                            }
                        }
                        if (c == '[')
                        {
                            if (!list.Exists(x => x.index == pointer && x.operation == 'f'))
                                list.Add(new DataPoint() { realIndex = i, index = pointer, data = list.Where(x => x.operation == 'i').FirstOrDefault(x => x.index == pointer).data, operation = 'f' });
                        }
                        if (c == ']')
                        {
                            var finit = list.Last(x => x.index < i && x.operation == 'f');

                            if (list.FirstOrDefault(x => x.index == finit.index).data > 0)
                            {

                                i = finit.realIndex;

                            }
                        }
                        
                    }
                    var pass = outp;
                    cd = wPath + "\\" + count;
                    DirectoryInfo di = Directory.CreateDirectory(cd);
                    using (Ionic.Zip.ZipFile zipF = Ionic.Zip.ZipFile.Read(zip))
                    {
                        zipF.Name = "File" + count;
                        var found = false;
                        try
                        {
                            zipF.Password = pass;
                            zipF.ExtractAll(cd, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                            found = true;
                            Console.Write("[V]");
                        }
                        catch (Exception e)
                        {
                            Console.Write("[X]");
                        }

                        Console.WriteLine();
                        if (!found)
                        {
                            Console.WriteLine("Generatore fallito. " + pass);
                            Console.WriteLine(Result.Text);
                            var ptr = Marshal.StringToHGlobalUni(Result.Text);
                            SetClipboardData(13, ptr);
                            CloseClipboard();
                            Marshal.FreeHGlobal(ptr);
                            try
                            {
                                Console.WriteLine("Inserire la password: ");
                                pass = Console.ReadLine();
                                zipF.Password = pass;
                                zipF.ExtractAll(cd, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                                found = true;
                                Console.WriteLine("[V] script:  Umano");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Errore critico...");
                                break;
                            }

                        }

                    }
                    count++;
                }
                else
                {


                    var p1 = regex.Matches(Result.Text)[0].Value.Trim('"');
                    var p2 = regex.Matches(Result.Text)[1].Value.Trim('"');
                    var p3 = 0;
                    var p3m = rp3.Matches(Result.Text);
                    var p3v = rp3v.Matches(Result.Text);
                    if (p3m.Count > 0) p3 = int.Parse(p3m[0].Value);
                    if (p3v.Count > 0 && p3 == 0) p3 = int.Parse(p3v[0].Value);
                    Console.WriteLine($"Data:\np1: {p1}\np2: {p2}\np3: {p3}");
                    var pass = string.Empty;
                    cd = wPath + "\\" + count;
                    DirectoryInfo di = Directory.CreateDirectory(cd);
                    using (Ionic.Zip.ZipFile zipF = Ionic.Zip.ZipFile.Read(zip))
                    {
                        zipF.Name = "File" + count;
                        var genList = gen.GetEnumerator();
                        var found = false;
                        while (genList.MoveNext())
                        {
                            try
                            {
                                pass = genList.Current.run(p1, p2, p3);
                                zipF.Password = pass;
                                zipF.ExtractAll(cd, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                                found = true;
                                Console.Write("[V]");
                                break;
                            }
                            catch (Exception e)
                            {
                                Console.Write("[X]");
                            }
                        }
                        Console.WriteLine();
                        if (!found)
                        {
                            Console.WriteLine("Generatore fallito. " + pass);
                            Console.WriteLine(Result.Text);
                            var ptr = Marshal.StringToHGlobalUni(Result.Text);
                            SetClipboardData(13, ptr);
                            CloseClipboard();
                            Marshal.FreeHGlobal(ptr);
                            try
                            {
                                Console.WriteLine("Inserire la password: ");
                                pass = Console.ReadLine();
                                zipF.Password = pass;
                                zipF.ExtractAll(cd, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                                found = true;
                                Console.WriteLine("[V] script:  Umano");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Errore critico...");
                                break;
                            }

                        }

                    }
                    Console.WriteLine("==========[DONE]=============");

                    count++;
                }
            }
            Console.ReadLine();
        }
        public class DataPoint
        {
            public int index;
            public char operation;
            public int data;
            public int realIndex;
        }
        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
        public interface Generate
        {
            string run(string param0, string param1, int param2);
        }
        public class Java2 : Generate
        {
            public string run(string param0, string param1, int param2)
            {
                string output = "";
                for (int i = 0, j = 0; i < param0.Length && j < param1.Length; i++, j++)
                {
                    char c0 = param0[i];
                    char c1 = param1[j];
                    param2 -= i;
                    if (param2 < 0)
                    {
                        param2 *= -1;
                    }
                    if (param2 % 2 == 0)
                    {
                        output += (c1);
                        output = Reverse(output);
                        output += param0[(param2 % param0.Length)];
                    }
                    else
                    {
                        output += (c0);
                        output = Reverse(output);
                        output += param1[(param2 % param1.Length)];
                    }
                    output = Reverse(output);
                }

                return new String(output.Take(32).ToArray());
            }
        }
        public class Java : Generate
        {
            public string run(string param0, string param1, int param2)
            {
                string output = "";
                for (int i = 0, j = 0; i < param0.Length && j < param1.Length; i++, j++)
                {
                    char c0 = param0[(i)];
                    char c1 = param1[(j)];
                    if (i % 2 == 0)
                    {
                        output += (c0) + (c1);
                    }
                    else
                    {
                        output += (c1) + (c0);
                    }
                    output = Reverse(output);
                    if (Char.IsDigit(c0) || Char.IsDigit(c1))
                    {
                        param2 -= i;
                        if (Char.IsDigit(c0))
                        {
                            param2 -= i * (int)Char.GetNumericValue(c0);
                        }
                        if (Char.IsDigit(c1))
                        {
                            param2 -= i * (int)Char.GetNumericValue(c1);
                        }
                        if (param2 < 0)
                        {
                            param2 *= -1;
                        }
                        String x = param2.ToString();
                        output += x[param2 % x.Length];
                        output = Reverse(output);
                    }
                }
                return new String(output.Take(32).ToArray());
            }
        }
        public class Py : Generate
        {
            public string run(string param0, string param1, int param2)
            {
                bool o = false;
                string output = string.Empty;
                param1 = Reverse(param1);
                foreach (var c in Zip(param0, param1))
                {
                    if (char.IsUpper(c.Key))
                        output += c.Key;
                    else if (char.IsLower(c.Key))
                        output += c.Value;
                    else if (char.IsDigit(c.Key))
                    {
                        if (!o)
                        {
                            output += param2.ToString();
                            o = true;
                        }
                        else
                        {
                            output += c.Key + "" + c.Value;
                            output = Reverse(output);
                        }
                    }
                    else
                    {
                        output += "42";
                    }
                }
                return new String(output.Take(32).ToArray());
            }
        }
        static IEnumerable<KeyValuePair<T1, T2>> Zip<T1, T2>(IEnumerable<T1> a, IEnumerable<T2> b)
        {
            var enumeratorA = a.GetEnumerator();
            var enumeratorB = b.GetEnumerator();
            while (enumeratorA.MoveNext())
            {
                if (!enumeratorB.MoveNext())
                {
                    yield break;
                }
                yield return new KeyValuePair<T1, T2>
                (
                    enumeratorA.Current,
                    enumeratorB.Current
                );
            }
        }
        public class JS : Generate
        {
            public string run(string param0, string param1, int param2)
            {
                var output = "";
                var helper = "";
                for (int i = 0, j = 0; i < param0.Length && j < param1.Length; i++, j++)
                {
                    if ((i + param2) % 2 == 0)
                    {
                        helper += param0[i] + param1[j];
                    }
                    else
                    {
                        helper += param1[j] + param0[i];
                    }
                    if ((j * param2 - i) % 3 == 0)
                    {
                        helper += Convert.ToString(param2, 16);
                    }
                    helper = Reverse(helper);
                }
                var y = 0;
                for (var p = 0; p < 32; p++)
                {
                    y = (y + param2 * p) % helper.Length;
                    output += helper[y];
                    output = Reverse(output);
                }
                return output.Substring(0, param0.Length);
            }
        }
        public class JS2 : Generate
        {
            public string run(string param0, string param1, int param2)
            {
                var output = "";
                var helper = "";

                for (int i = 0, j = 0; i < param0.Length && j < param1.Length; i++, j++)
                {
                    if (((i + param2) % 2) != 0)
                    {
                        helper += String.Concat(param0[i], param1[j]);
                    }
                    else
                    {
                        helper += String.Concat(param1[j], param0[i]);
                    }

                    if ((j * param2 - i) % 3 == 0)
                    {
                        helper += Convert.ToString(param2, 16);
                    }
                    helper = Reverse(helper);
                }
                var y = 0;
                for (var p = 0; p < 32; p++)
                {
                    y = (y + param2 * p) % helper.Length;
                    output += helper[y];
                    output = Reverse(output);
                }
                output = output.Substring(0, param0.Length);
                return output;
            }
        }
        public class Py2 : Generate
        {
            public string run(string param0, string param1, int param2)
            {
                var output = "";
                var roll = "WerenostrangerstoloveYouknowtherulesandsodoIAfullcommitmentswhatImthinkingofYouwouldntgetthisfromanyotherguyIjustwannatellyouhowImfeelingGottamakeyouunderstandNevergonnagiveyouupNevergonnaletyoudownNevergonnarunaroundanddesertyouNevergonnamakeyoucryNevergonnasaygoodbyeNevergonnatellalieandhurtyou";

                var get_index = new Func<int, int>((i) => Convert.ToInt32(i + param2) % roll.Length);

                foreach (var els in Zip(param0, param1))
                {
                    if ((int)els.Key == (int)els.Value)
                    {
                        output += roll[get_index((int)els.Key + (int)els.Value)];
                        if (char.IsDigit(els.Key))
                        {
                            output += els.Key;
                        }
                    }
                    else if ((int)els.Key > (int)els.Value)
                    {
                        output += roll[get_index((int)els.Key)];
                        if (char.IsDigit(els.Key))
                        {
                            output += els.Key;
                        }
                    }
                    else
                    {
                        output += roll[get_index((int)els.Value)];
                        if (char.IsDigit(els.Value))
                        {
                            output += els.Value;
                        }
                    }
                    output = Reverse(output);
                }
                output = output.Substring(0, 32);
                return output;
            }
        }
        public class Java3 : Generate
        {
            public string run(string param0, string param1, int param2)
            {
                string output = "";
                for (int i = 0, j = 0; i < param0.Length && j < param1.Length; i++, j++)
                {
                    char c0 = param0[i];
                    char c1 = param1[j];
                    if (i % 2 == 0)
                    {
                        output = string.Concat(output, c0, c1);
                    }
                    else
                    {
                        output = string.Concat(output, c1, c0);
                    }
                    output = Reverse(output);
                    if (Char.IsDigit(c0) || Char.IsDigit(c1))
                    {
                        param2 -= i;
                        if (Char.IsDigit(c0))
                        {
                            param2 -= i * (int)Char.GetNumericValue(c0);
                        }
                        if (Char.IsDigit(c1))
                        {
                            param2 -= i * (int)Char.GetNumericValue(c1);
                        }
                        if (param2 < 0)
                        {
                            param2 *= -1;
                        }
                        String x = param2.ToString();
                        output = string.Concat(output, x[param2 % x.Length]);
                        output = Reverse(output);
                    }
                }
                return output.Substring(0, 32);
            }
        }
        public class JS3 : Generate
        {
            public string run(string param0, string param1, int param2)
            {
                var output = "";
                for (int i = 0, j = 0; i < param0.Length && j < param1.Length; i++, j++)
                {
                    var c = ((int)param0[i]);
                    if (c % 2 != 0)
                    {
                        output += param0[i];
                    }
                    else
                    {
                        output += param1[j];
                    }
                    if (c < 42)
                    {
                        output += param2.ToString();
                    }
                    else
                    {
                        output = Reverse(output);
                    }
                }
                output = output.Substring(0, param0.Length);
                return output;
            }
        }
        public class Py3 : Generate
        {
            public string run(string param0, string param1, int param2)
            {
                var o = false;
                var output = "";
                var i = 0;
                foreach (var iter in Zip(param0, param1))
                {
                    if (i % 2 == 0)
                    {
                        var cc = iter.Key + "" + iter.Value;

                        if (int.TryParse(cc, out int aaaa) && !o)
                        {
                            o = true;
                            var meh = param0 + param2.ToString() + param1;
                            for (int j = 0; j < (i * param2); j++)
                            {
                                var x = 13;
                                if (j % 2 == 0)
                                    x = 77;
                                output += meh[(j + x) % (meh.Length)];
                                output = Reverse(output);
                            }
                        }
                        else
                        {
                            output += cc;
                            output = Reverse(output);
                        }
                    }
                    else
                    {
                        if (iter.Key == iter.Value)
                        {
                            output += iter.Value + "" + iter.Key;
                        }
                        else if (iter.Key > iter.Value)
                        {
                            output += iter.Key;
                        }
                        else
                        {
                            output += iter.Value;
                        }
                    }
                    i += 1;
                }
                return output.Substring(0, 32);
            }
        }
        public class Php : Generate
        {
            public string run(string param0, string param1, int param2)
            {
                //var param0 = "zot4j5vZfWun2xGAGY3rINDl7uHH3J59";
                //var param1 = "QTbqtvWSaVREphR4T5bojUcLVUmL7HIT";
                //var param2 = 1333;
                var output = "";
                var l = param2;

                if (l < param0.Length && l < param1.Length)
                {
                    l = param0.Length + param1.Length;
                }

                for (var i = 0; i < l; i++)
                {
                    if (i >= param0.Length && i >= param1.Length)
                    {
                        if ((i % 2) == 1)
                        {
                            output += param0[i % param0.Length];
                        }
                        else
                        {
                            output += param1[i % param1.Length];
                        }
                        if (output.Length > 0 && char.IsUpper(output[0]))
                        {
                            param0 = Reverse(param1);
                            param1 = Reverse(param0);
                        }
                    }
                    else
                    {
                        if (char.IsUpper(param0[0]))
                        {
                            output += param0[i];
                        }
                        else
                        {
                            output += param1[i];
                        }
                    }

                }
                output = output.Substring(0, 32);
                return output;
            }
        }
        public class Php2 : Generate
        {
            public string run(string param0, string param1, int param2)
            {
                //var param0 = "GzZq2gLr9quleVn5oB7IRDbc2749pABg";
                //var param1 = "P6aHWqbvW6OlilNfqy1Rsa8YYoLpvX0Q";
                //var param2 = 1084;
                var output = "";

                for (int i = 0, j = 0; i < param0.Length && j < param1.Length; i++, j++)
                {
                    var c0 = param0[i];
                    var c1 = param1[j];

                    if (i % 2 == 0)
                    {
                        output += String.Concat(c1, c0);
                    }
                    else
                    {
                        output += String.Concat(c0, c1);
                    }
                }

                for (var k = 0; k < output.Length; k++)
                {
                    int d, s;
                    if (k % 2 == 0)
                    {
                        s = k * param0.Length + param2 * 3;
                        d = 1 + param2 + k;
                    }
                    else
                    {
                        s = k * 2 * param2;
                        d = (15 + param1.Length) * k;
                    }
                    s = s % output.Length;
                    d = d % output.Length;
                    var x = output[d];
                    var char_array = output.ToCharArray();
                    char_array[d] = char_array[s];
                    char_array[s] = x;
                    output = new string(char_array);
                    output = Reverse(output);
                }
                output = output.Substring(0, 32);
                return output;
            }
        }
        public class Shell : Generate
        {
            public string run(string param0, string param1, int param2)
            {
                var output = "";
                for (int i = 0, j = 0; i < param0.Length && j < param1.Length; i++, j++)
                {
                    var c0 = param0.Substring(i, 1).FirstOrDefault();
                    var c1 = param1.Substring(j, 1).FirstOrDefault();

                    if (char.IsNumber(c0))
                    {
                        param2 += (int.Parse(c0.ToString()));
                    }
                    if (char.IsNumber(c1))
                    {
                        param2 += (int.Parse(c1.ToString()));
                    }
                    if (char.IsLetter(c0))
                    {
                        output += c0;//mm,...
                    }
                    if (char.IsLetter(c1))
                    {
                        output += c1;//mm,...
                    }
                    if (char.IsLetter(c0) && char.IsLetter(c1))
                    {
                        //output = Reverse(output);
                        var reverse = "";
                        for (int k = output.Length - 1; k >= 0; k--)
                        {
                            reverse = reverse + output.Substring(k, 1);
                        }
                        output = reverse;
                    }
                }
                output = param2.ToString() + output;
                return output.Substring(0, 32);
            }
        }
        public class Shell2 : Generate
        {
            public string run(string param0, string param1, int param2)
            {
                var output = "";
                for (int h = 0; h < param2; h += 25)
                {
                    for (int i = 0, j = 0; i < param0.Length || j < param1.Length; i++, j++)
                    {
                        var c0 = param0.Substring(i, 1).FirstOrDefault();
                        var c1 = param1.Substring(j, 1).FirstOrDefault();
                        if (char.IsNumber(c0) && char.IsNumber(c1))
                        {
                            output += (param2 * (int.Parse(c0.ToString()) + int.Parse(c1.ToString()) + h));
                        }
                        else
                        {
                            var x = h + i;
                            if (x % 2 != 0)
                            {
                                output += c0;
                            }
                            else
                            {
                                output += c1;
                            }
                        }
                    }
                    output = Reverse(output);
                }
                return output.Substring(0, 32);
            }
        }
        public class JS4 : Generate
        {
            public string run(string param0, string param1, int param2)
            {
                var output = "";
                for (int i = 0, j = param1.Length - 1; i < param0.Length || j >= 0; i++, j--)
                {
                    var c0 = param0[i];
                    var c1 = param1[j];
                    if (output.IndexOf(c0) != 0)
                    {
                        if (i % 2 != 0)
                        {
                            output += c1;
                        }
                        else
                        {
                            output += c0;
                        }
                    }
                    if (c0 > c1)
                    {
                        output += c0;
                    }
                    else if (c0 < c1)
                    {
                        output = c1 + "" + output;
                    }
                    else
                    {
                        output = Reverse(output);
                        output += c1;
                    }
                }
                return Reverse(output).Substring(0, param0.Length);
            }
        }
        public class Shell3 : Generate
        {
            public string run(string param0, string param1, int param2)
            {
                var output = "";
                var f = false;
                for (int i = 0, j = 0; i < param0.Length || j < param1.Length; i++, j++)
                {
                    var c0 = param0.Substring(i, 1).FirstOrDefault();
                    var c1 = param1.Substring(j, 1).FirstOrDefault();
                    if (char.IsNumber(c0) && char.IsNumber(c1))
                    {
                        param2 = param2 - i - (c0 * c1);
                        if (param2 < 1)
                        {
                            param2 *= -1;
                            f = !f;
                        }
                        output += param2;
                    }
                    else
                    {
                        if (f && char.IsUpper(c0))
                        {
                            output += c0;
                        }
                        else
                        {
                            output += c1 + "" + c0;
                        }
                    }
                    if (true)
                    {
                        output = Reverse(output);
                    }

                }
                return output.Substring(0, 32);
            }
        }
        public class Shell4 : Generate
        {
            public string run(string param0, string param1, int param2)
            {
                var output = "";
                var f = false;
                for (int i = 0, j = 0; i < param0.Length || j < param1.Length; i++, j++)
                {
                    var c0 = param0.Substring(i, 1).FirstOrDefault();
                    var c1 = param1.Substring(j, 1).FirstOrDefault();
                    if (char.IsNumber(c0) && char.IsNumber(c1))
                    {
                        param2 = param2 - i - (int.Parse(c0.ToString()) * int.Parse(c1.ToString()));
                        if (param2 < 1)
                        {
                            param2 *= -1;
                            f = !f;
                        }
                        output += param2;
                    }
                    else
                    {
                        if (f && char.IsUpper(c0))
                        {
                            output += c0;
                        }
                        else
                        {
                            output += c1 + "" + c0;
                        }
                    }
                    if (true)
                    {
                        output = Reverse(output);
                    }

                }
                return output.Substring(0, 32);
            }
        }
        public class Php3 : Generate
        {
            public string run(string param0, string param1, int param2)
            {
                var output = "";
                var l = new string[] {"z",
                                        "0",
                                        "T",
                                        "r",
                                        "F",
                                        "1",
                                        "s",
                                        "3",
                                        "G",
                                        "n"};

                for (int i = 0, j = 0; i < param0.Length || j < param1.Length; i++, j++)
                {
                    var c0 = param0[i];
                    var c1 = param1[j];
                    if (char.IsNumber(c0) && char.IsNumber(c1))
                    {
                        output += c0;
                        output = Reverse(output);
                        output += c1;
                    }
                    else
                    {
                        if (char.IsNumber(c0))
                        {
                            output += l[(int.Parse(c0.ToString()) + param2) % l.Count()];
                        }
                        else
                        {
                            output = Reverse(output);
                            output += c0;
                        }
                        output += c1;
                    }
                }

                return output.Substring(0, 32);
            }
        }
        public class Java4 : Generate
        {
            public string run(string param0, string param1, int param2)
            {
                var output = "";
                var pp = param0 + param1;
                for (int i = 0, j = 0; i < param0.Length && j < param1.Length; i++, j++)
                {
                    var c0 = param0[i];
                    var c1 = param1[j];
                    if (char.IsDigit(c0) && char.IsDigit(c1))
                    {
                        param2 += int.Parse(c0.ToString()) * int.Parse(c1.ToString());
                        output += (pp[param2 % pp.Length]);
                        output += (c0);
                        output = Reverse(output);
                        output += (c1);
                    }
                    else
                    {
                        if (char.IsDigit(c0))
                        {
                            param2 += int.Parse(c0.ToString());
                            output += (param1[param2 % param1.Length]);
                            output = Reverse(output);
                        }
                        output += (c0);
                        output = Reverse(output);
                        if (char.IsDigit(c1))
                        {
                            param2 += int.Parse(c1.ToString());
                            output += (param0[param2 % param0.Length]);
                            output = Reverse(output);
                        }
                        output += (c0);
                        output = Reverse(output);
                    }

                }
                return output.Substring(0, 32);
            }
        }
    }

    //public class Codding100
    //{
    //    public void Start()
    //    {

    //        var text = RemoveSpecialCharacters(
    //            File.ReadAllText("C:/Users/loren/Downloads/coding-100/The Time Machine by H. G. Wells.txt").ToLower())
    //            .Split(' ').GroupBy(x => x).ToArray();
    //        var dic = RemoveSpecialCharacters(File.ReadAllText("C:/Users/loren/Downloads/coding-100/words.txt").ToLower());
    //        var dicA = dic.Split(
    //            new[] { "\n" },
    //            StringSplitOptions.None).Where(x => x.Length > 3);
    //        foreach (var f in text)
    //        {
    //            if (dicA.Where(x => f.Contains(x)).Any(x => CalculateSimilarity(x, f..) > 0.80d))
    //            {
    //                Console.WriteLine(f);
    //            }
    //            else
    //            {

    //            }
    //        }

    //    }
    //    public static string RemoveSpecialCharacters(string input)
    //    {
    //        Regex r = new Regex("[0-9a-zA-Z]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    //        return r.Replace(input, String.Empty);
    //    }
    //    int ComputeLevenshteinDistance(string source, string target)
    //    {
    //        if ((source == null) || (target == null)) return 0;
    //        if ((source.Length == 0) || (target.Length == 0)) return 0;
    //        if (source == target) return source.Length;

    //        int sourceWordCount = source.Length;
    //        int targetWordCount = target.Length;

    //        // Step 1
    //        if (sourceWordCount == 0)
    //            return targetWordCount;

    //        if (targetWordCount == 0)
    //            return sourceWordCount;

    //        int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

    //        // Step 2
    //        for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
    //        for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

    //        for (int i = 1; i <= sourceWordCount; i++)
    //        {
    //            for (int j = 1; j <= targetWordCount; j++)
    //            {
    //                // Step 3
    //                int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

    //                // Step 4
    //                distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
    //            }
    //        }

    //        return distance[sourceWordCount, targetWordCount];
    //    }
    //    double CalculateSimilarity(string source, string target)
    //    {
    //        if ((source == null) || (target == null)) return 0.0;
    //        if ((source.Length == 0) || (target.Length == 0)) return 0.0;
    //        if (source == target) return 1.0;

    //        int stepsToSame = ComputeLevenshteinDistance(source, target);
    //        return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
    //    }
    //}

}
