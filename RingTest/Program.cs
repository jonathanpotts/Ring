using System;
using System.Threading.Tasks;
using Ring;

namespace RingTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Connect to the Ring API using a Ring account username and password.
                var ring = await RingClient.CreateAsync("user@example.com", "password");

                // Save the AuthToken for future connections instead of the Ring account username and password.
                // You can create a new connection using an auth token by calling: RingClient.CreateAsync(authToken)
                // The auth token may expire, but you can ask the user for their username and password at that point.
                var authToken = ring.AuthToken;

                // Get the list of the user's devices.
                var devices = await ring.GetDevicesAsync();

                // Check if any devices were returned.
                if (devices.Count > 0)
                {
                    // Get the battery life of the first device.
                    var batteryLife = devices[0].BatteryLife;

                    var deviceDescription = devices[0].Description;

                    Console.WriteLine($"{deviceDescription} battery life: {batteryLife}");
                }

                // Get the list of the user's active dings.
                var activeDings = await ring.GetActiveDingsAsync();

                // Check if there are any active dings.
                if (activeDings.Count > 0)
                {
                    // Get the type of the first active ding.
                    var activeDingType = activeDings[0].Type;

                    Console.WriteLine($"Active ding: {activeDingType}");
                }

                // Get the list of the user's last 15 dings.
                var dings = await ring.GetDingsAsync(15);

                // Check if any dings were returned.
                if (dings.Count > 0)
                {
                    // Get the URI of the recording of the most recent ding.
                    var recordingUri = await ring.GetRecordingUriAsync(dings[0]);

                    // Get the description of the Ring device that the most recent ding occurred from.
                    var dingDeviceDescription = dings[0].Device.Description;

                    Console.WriteLine($"Ding from {dingDeviceDescription}: {recordingUri}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
