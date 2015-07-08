using System.IO;
using System.Xml.Serialization;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Configuration;

namespace UberDeployer.Core.DataAccess.Xml
{
  public class XmlApplicationConfigurationRepository : IApplicationConfigurationRepository
  {
    private readonly string _xmlFilePath;

    public XmlApplicationConfigurationRepository(string xmlFilePath)
    {
      Guard.NotNullNorEmpty(xmlFilePath, "xmlFilePath");

      _xmlFilePath = xmlFilePath;
    }

    public IApplicationConfiguration LoadConfiguration()
    {
      XmlSerializer xmlSerializer = new XmlSerializer(typeof(ApplicationConfigurationXml));

      ApplicationConfigurationXml xmlConfiguration;

      using (var fs = File.OpenRead(_xmlFilePath))
      {
        xmlConfiguration = (ApplicationConfigurationXml)xmlSerializer.Deserialize(fs);
      }

      return xmlConfiguration;
    }
  }
}
