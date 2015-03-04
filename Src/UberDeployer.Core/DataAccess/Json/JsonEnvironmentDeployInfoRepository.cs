using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Domain;

namespace UberDeployer.Core.DataAccess.Json
{
  public class JsonEnvironmentDeployInfoRepository : IEnvironmentDeployInfoRepository
  {
    private readonly string _configurationFilesDirPath;
    private Dictionary<string, EnvironmentDeployInfo> _environmentDeployInfosByName;

    public JsonEnvironmentDeployInfoRepository(string configurationFilesDirPath)
    {
      _configurationFilesDirPath = configurationFilesDirPath;
    }

    public EnvironmentDeployInfo FindByName(string environmentName)
    {
      Guard.NotNullNorEmpty(environmentName, "environmentName");

      LoadJsonFilesIfNeeded();

      EnvironmentDeployInfo environmentDeployInfo;

      if(_environmentDeployInfosByName.TryGetValue(environmentName, out environmentDeployInfo))
      {
        return environmentDeployInfo;
      }

      return null;
    }

    private void LoadJsonFilesIfNeeded()
    {
      if (_environmentDeployInfosByName != null)
      {
        return;
      }

      _environmentDeployInfosByName = new Dictionary<string, EnvironmentDeployInfo>();

      var jsonSerializer = new JsonSerializer();      

      foreach (string jsonFilePath in Directory.GetFiles(_configurationFilesDirPath, "*.json", SearchOption.TopDirectoryOnly))
      {
        EnvironmentDeployInfoJson environmentDeployInfoJson = null;

        using (FileStream fileStream = File.OpenRead(jsonFilePath))
        using(var streamReader = new StreamReader(fileStream))
        using (var jsonReader = new JsonTextReader(streamReader))
        {
          environmentDeployInfoJson = jsonSerializer.Deserialize<EnvironmentDeployInfoJson>(jsonReader);
        }

        if (environmentDeployInfoJson != null)
        {
          EnvironmentDeployInfo environmentDeployInfo = ConvertToDomain(environmentDeployInfoJson);

          _environmentDeployInfosByName.Add(environmentDeployInfo.TargetEnvironment, environmentDeployInfo);
        }
      }
    }

    private static EnvironmentDeployInfo ConvertToDomain(EnvironmentDeployInfoJson environmentDeployInfoJson)
    {
      return new EnvironmentDeployInfo(
        environmentDeployInfoJson.TargetEnvironment, 
        environmentDeployInfoJson.ProjectsToDeploy);
    }

    public class EnvironmentDeployInfoJson
    {
      public string TargetEnvironment { get; set; }

      public List<string> ProjectsToDeploy { get; set; } 
    }
  }
}
