# Ring
An unofficial [Ring](https://ring.com) library for [.NET](https://www.microsoft.com/net). **This library is in no way endorsed by Ring nor do I represent Ring in any form.**

Uses [.NET Standard](https://github.com/dotnet/standard) and [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json).

This library should work on all .NET platforms that support .NET Standard 1.4 including:

* [.NET Framework](https://github.com/microsoft/dotnet) 4.6.1 (including [ASP.NET](https://www.asp.net))
* [.NET Core](https://github.com/dotnet/core) 1.0 (including [ASP.NET Core](https://github.com/aspnet/Home))
* [Mono](https://github.com/mono/mono) 4.6
* [Xamarin.iOS](https://github.com/xamarin/xamarin-macios) 10.0
* [Xamarin.Mac](https://github.com/xamarin/xamarin-macios) 3.0
* [Xamarin.Android](https://github.com/xamarin/xamarin-android) 7.0
* [Universal Windows Platform](https://docs.microsoft.com/en-us/windows/uwp/index) 10.0

## Example Usage

```csharp
try
{
  // Connect to the Ring API using a Ring account username and password.
  var ring = new RingClient("username", "password");
  
  // Save ring.AuthToken for future connections instead of the username and password.
  // You can create a new connection using an auth token by calling new RingClient(authToken)
  
  // Get the list of the user's devices.
  var devices = await ring.GetDevicesAsync();
  
  // Check if any devices were returned.
  if (devices.Count > 0)
  {
    // Get the battery life of the first device.
    var batteryLife = devices[0].BatteryLife;
  }
  
  // Get the list of the user's active dings.
  var activeDings = await ring.GetActiveDingsAsync();
  
  // Get the list of the user's last 15 dings.
  var dings = await ring.GetDingsAsync(15);
  
  // Check if any dings were returned.
  if (dings.Count > 0)
  {
    // Get the URI of the recording of the most recent ding.
    var recordingUri = await ring.GetRecordingUriAsync(dings[0]);
    
    // Get the description of the Ring device that the most recent ding occurred from.
    var description = dings[0].Device.Description;
  }
}
catch (Exception ex)
{
  Console.WriteLine(ex.Message);
}
```

## Credits
This project was based off of [php-ring-api](https://github.com/jeroenmoors/php-ring-api) by [Jeroen Moors](https://github.com/jeroenmoors).
