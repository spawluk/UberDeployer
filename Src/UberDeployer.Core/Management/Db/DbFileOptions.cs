using UberDeployer.Common.SyntaxSugar;

namespace UberDeployer.Core.Management.Db
{
  public class DbFileOptions
  {
    public DbFileOptions(string name, string fileName)
    {
      Guard.NotNullNorEmpty(name, "name");
      Guard.NotNullNorEmpty(fileName, "fileName");

      FileName = fileName;
      Name = name;
    }

    public string Name { get; private set; }

    public string FileName { get; private set; }
  }
}