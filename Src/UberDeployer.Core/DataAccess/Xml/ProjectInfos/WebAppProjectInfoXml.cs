namespace UberDeployer.Core.DataAccess.Xml.ProjectInfos
{
  public class WebAppProjectInfoXml : ProjectInfoXml
  {
    public string AppPoolId { get; set; }

    public string WebSiteName { get; set; }

    public string WebAppDirName { get; set; }

    public string WebAppName { get; set; }
  }
}
