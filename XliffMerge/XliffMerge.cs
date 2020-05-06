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

        public XliffMerge(XmlDocument sourceDoc, XmlDocument destDoc, bool replace)
        {
            _sourceDoc = sourceDoc;
            _destDoc = destDoc;
            _translations = new Dictionary<string, XmlNode>();
            _replace = replace;

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
            {
                var target = node.SelectSingleNode("xliff:target", _nsmgr);
                if (target != null)
                    _translations.Add(node.Attributes["id"].Value, target);
            }
        }

        private void MergeIntoDestination()
        {
            XmlNodeList nodes;
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
                    XmlNode target = _destDoc.ImportNode(_translations[id], true);
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
