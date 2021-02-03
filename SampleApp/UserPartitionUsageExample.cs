using System;
using System.IO;
using IIscAppLoaderInterfaces;
using log4net;

namespace SampleApp
{
  internal static class UserPartitionUsageExample
  {
    private static readonly ILog Logger = LogManager.GetLogger(typeof (IscApp));

    public static void CreateFileOnUserPartition(IIscAppConnector appHost)
    {
      // The method GetConfigRootPath() retrieves the path of the user partition, where you can create a file. 
      var configRootPath = appHost.ConfigRootPath;
      // combine a full path of the file you want ot create.
      var configfile = Path.Combine(configRootPath, "configfile.xml");

      // Check if the file exists and delete it if so.
      if (File.Exists(configfile))
      {
        File.Delete(configfile);
      }

      try
      {
        // Create the file
        using (var fileStream = File.Create(configfile))
        {
          // Here you can write the content of the file before you close the file stream
        }

        // Check if the file has been successfully create
        if (File.Exists(configfile))
        {
          // Send a message on the bus. The communication object with the ID 46 is a 14 bytes object (string)
          appHost.WriteValue(46, "FileCreated");
        }
      }
      catch (Exception ex)
      {
        appHost.WriteValue(21, "NoFileCreated");
        Logger.Error("Could not create file: " + ex);
      }
    }
  }
}