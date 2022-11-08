using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exeRarUnpack
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string path = args[0];
            string sevenzippath = Environment.CurrentDirectory + "\\7z.exe";

            Console.WriteLine("Searching exe....");
            string[] exefiles = Directory.GetFiles(path, "*.exe", SearchOption.AllDirectories);
            Console.WriteLine("found {0} exe files.",exefiles.Length);

            for (int i = 0; i < exefiles.Length; i++)
            {
                string exefile = exefiles[i];
                Console.WriteLine("Processing [{0}/{1}] {2}...", i+1, exefiles.Length, exefile);
                using (BinaryReader rd = new BinaryReader(File.OpenRead(exefile)))
                {
                    rd.BaseStream.Seek(0x4f260, SeekOrigin.Begin);
                    if (rd.ReadBytes(3).SequenceEqual(new byte[] { 0x52, 0x61, 0x72 }))
                    {
                        string outname = Path.GetDirectoryName(exefile) + "\\" + Path.GetFileNameWithoutExtension(exefile) + ".rar";
                        Console.WriteLine("Out file: {0}", outname);
                        using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(outname)))
                        {
                            
                            rd.BaseStream.Seek(-3, SeekOrigin.Current);

                            while (rd.BaseStream.Position < rd.BaseStream.Length)
                            {
                                byte[] buffer = new byte[8192];
                                rd.Read(buffer, 0, buffer.Length);
                                writer.Write(buffer, 0, buffer.Length);
                            }

                        }
                        
                        Console.WriteLine("Rar convert completed.");
                        
                        Process szip = new Process();
                        szip.StartInfo.FileName = sevenzippath;
                        szip.StartInfo.Arguments = "e \""+outname + "\" -scsUTF-8 -o\"" + Path.GetDirectoryName(exefile) + "\" -spf";
                        Console.WriteLine("Execute {0} {1}",szip.StartInfo.FileName,szip.StartInfo.Arguments);
                        szip.Start();
                        szip.WaitForExit();
                        //Console.WriteLine("Delete {0}", exefile);
                        //File.Delete(exefile);
                    }
                    else
                    {
                        Console.WriteLine("Error: {0} is not a Rar included exe file. skipping",exefile);
                    }
                }

            }

            Console.WriteLine("Everything end.");
            //0x4F260

        }
    }
}
