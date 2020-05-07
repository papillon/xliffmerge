using F23.StringSimilarity;
using System;
using System.Collections.Generic;
using System.Xml;

namespace XliffMerge
{
    public class XliffMerge
    {
        private readonly XmlDocument _sourceDoc;
        private readonly XmlDocument _destDoc;
        private readonly XmlNamespaceManager _nsmgr;
        private readonly Dictionary<string, XmlNode> _translations;
        private readonly bool _replace;
        private readonly bool _verbose;
        private readonly int _fuzzy;

        public XliffMerge(XmlDocument sourceDoc, XmlDocument destDoc, bool replace, bool verbose, int fuzzy)
        {
            _sourceDoc = sourceDoc;
            _destDoc = destDoc;
            _translations = new Dictionary<string, XmlNode>();
            _replace = replace;
            _verbose = verbose;
            _fuzzy = fuzzy;

            _nsmgr = new XmlNamespaceManager(_sourceDoc.NameTable);
            _nsmgr.AddNamespace("xliff", "urn:oasis:names:tc:xliff:document:1.2");
        }

        public void Execute()
        {
            LoadSource();
            MergeIntoDestination();
        }

        private void LoadSource()
        {
            var root = _sourceDoc.DocumentElement;
            var nodes = root.SelectNodes("//xliff:trans-unit[xliff:target]", _nsmgr);
            foreach (XmlNode node in nodes)
                _translations.Add(node.Attributes["id"].Value, node);
        }

        private void MergeIntoDestination()
        {
            XmlNodeList nodes;
            var levenshtein = new NormalizedLevenshtein();
            var root = _destDoc.DocumentElement;

            if (_replace)
                nodes = root.SelectNodes("//xliff:trans-unit", _nsmgr);
            else
                nodes = root.SelectNodes("//xliff:trans-unit[not(xliff:target)]", _nsmgr);

            foreach (XmlNode node in nodes)
            {
                var id = node.Attributes["id"].Value;
                if (_translations.ContainsKey(id))
                {
                    var source = node.SelectSingleNode("xliff:source", _nsmgr);
                    var transSource = _translations[id].SelectSingleNode($"./xliff:source", _nsmgr);
                    var transTarget = _translations[id].SelectSingleNode($"./xliff:target", _nsmgr);

                    if (source.InnerText != transSource.InnerText)
                    {
                        var percentSimilar = Math.Round((1 - levenshtein.Distance(source.InnerText, transSource.InnerText)) * 100);
                        if (_verbose)
                        {
                            Console.WriteLine($"Sources mismatch in id='{id}' Similarity {percentSimilar}%.");
                            Console.WriteLine($" Source file='{transSource.InnerText}'");
                            Console.WriteLine($" Target file='{source.InnerText}'");
                        }
                        if (percentSimilar < _fuzzy)
                        {
                            if (_verbose)
                                Console.WriteLine($"Skipping");
                            continue;
                        }
                    }

                    if (_replace)
                    {
                        var oldTarget = node.SelectSingleNode("xliff:target", _nsmgr);
                        if (oldTarget != null)
                            node.RemoveChild(oldTarget);
                    }

                    if (source.NextSibling.Name != "#significant-whitespace")
                    {
                        XmlSignificantWhitespace sigws = _destDoc.CreateSignificantWhitespace("\n          ");
                        node.InsertAfter(sigws, source);
                    }
                    XmlNode target = _destDoc.ImportNode(transTarget, true);
                    node.InsertAfter(target, source.NextSibling);
                    if (target.NextSibling.Name != "#significant-whitespace")
                    {
                        XmlSignificantWhitespace sigws = _destDoc.CreateSignificantWhitespace("\n          ");
                        node.InsertAfter(sigws, target);
                    }

                }
            }

        }
    }
}
