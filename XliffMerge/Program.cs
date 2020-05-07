using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Xml;

namespace XliffMerge
{
    class Program
    {
        public class Options
        {
            [Option('s', "source", HelpText = "Filename where translations are read from.", Required = true)]
            public string SourceFilename { get; set; }

            [Option('t', "target", HelpText = "Filename where translations are written to.", Required = true)]
            public string TargetFilename { get; set; }

            [Option('r', "replace", Required = false, HelpText = "Replace existing translations in destination.")]
            public bool Replace { get; set; }

            [Option('f', "fuzzy", Required = false, Default = 100, HelpText = "Source strings must match by this percentage (100 = totally equal)")]
            public int Fuzzy { get; set; }

            [Option('v', "verbose", Required = false, HelpText = "Show potential issues.")]
            public bool Verbose { get; set; }

            [Usage(ApplicationAlias = "XliffMerge")]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    return new List<Example>() {
                        new Example("Merge translations from source to target file", new Options { SourceFilename = "Base Application.de-DE.xlf", TargetFilename = "Base Application.g.xlf" }),
                        new Example("Merge and replace existing translations", new Options { Replace = true, SourceFilename = "source.xlf", TargetFilename = "target.xlf" }),
                        new Example("Be tolerant if sources only match by 95% or more", new Options { Replace = true, SourceFilename = "source.xlf", TargetFilename = "target.xlf", Fuzzy = 95, Verbose = true })
                    };
                }
            }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                XmlDocument sourceDoc = new XmlDocument();
                XmlDocument destDoc = new XmlDocument();

                Console.WriteLine($"Reading {o.SourceFilename}");
                sourceDoc.Load(o.SourceFilename);

                Console.WriteLine($"Reading {o.TargetFilename}");
                destDoc.Load(o.TargetFilename);

                Console.WriteLine($"Projecting translations into target");
                var xliffMerge = new XliffMerge(sourceDoc, destDoc, o.Replace, o.Verbose, o.Fuzzy);
                xliffMerge.Execute();

                XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
                var xmlWriter = XmlWriter.Create(o.TargetFilename, settings);
                var xmlTextWriter = new XmlWriterDecorator(xmlWriter);
                Console.WriteLine($"Writing {o.TargetFilename}");
                destDoc.WriteTo(xmlTextWriter);
                xmlWriter.Close();
            });
        }
    }
}
