using System.Xml;
using UnityEngine;

public static class XMLDocumentCreater
{
    public static XmlDocument CreateXmlDocument(string fileName)
    {
        TextAsset xmlTextAsset = Resources.Load<TextAsset>($"Sprites/{fileName}");
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xmlTextAsset.text);
        return xmlDocument;
    }
}