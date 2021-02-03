using IIscAppLoaderInterfaces;
using log4net;

namespace SampleApp
{
  internal static class SdCardUsageExample
  {
    private static readonly ILog Logger = LogManager.GetLogger(typeof (IscApp));

    public static void UseSdCard(IIscAppConnector appHost)
    {
      // The property IsSdCardPresent indicates if any SD card is plugged into the device.
      // If the app runs on the device, the presence of the physical SD card is checked.
      // If the app runs on the developing machine, the presence state is simulated according to the value
      // of ConfigSDCardPresentDevelopment in IscAppConnectorHost.exe.config.
      if (appHost.IsSdCardPresent)
      {
        // GetSdCardPath() gets the path to the SD card as string.
        // If the app runs on the device, the path to the physical SD card is being retrieved.
        // If the app runs on the developing machine, the path according to ConfigSDCardPathDevelopment in
        // IscAppConnectorHost.exe.config is being returned.
        var sdCardPath = appHost.SdCardPath;
        // Write the path on the bus (The communication object with the ID 46 is a 14 bytes object (string)).
        appHost.WriteValue(46, sdCardPath);
        return;
      }

      appHost.WriteValue(46, "No SD card!");
    }
  }
}