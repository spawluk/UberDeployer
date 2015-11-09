using System.Collections.Generic;
using System.Xml.Serialization;

namespace UberDeployer.Core.DataAccess.Xml.ProjectInfos
{
  [XmlInclude(typeof(NtServiceProjectInfoXml))]
  [XmlInclude(typeof(WebAppProjectInfoXml))]
  [XmlInclude(typeof(SchedulerAppProjectInfoXml))]
  [XmlInclude(typeof(TerminalAppProjectInfoXml))]
  [XmlInclude(typeof(DbProjectInfoXml))]
  [XmlInclude(typeof(UberDeployerAgentProjectInfoXml))]
  [XmlInclude(typeof(ExtensionProjectInfoXml))]
  [XmlInclude(typeof(PowerShellScriptProjectInfoXml))]
  public class ProjectInfosXml
  {
    public List<ProjectInfoXml> ProjectInfos { get; set; }
  }
}
