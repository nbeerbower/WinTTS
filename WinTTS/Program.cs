using System.Speech.Synthesis;
using CSCore.Codecs.WAV;
using CSCore.SoundOut;
using CSCore.CoreAudioAPI;

class Program
{
    static void Main(string[] args)
    {
        using (SpeechSynthesizer synthesizer = new SpeechSynthesizer())
        {
            // Iterate available voices
            Console.WriteLine("Available Voices:");
            string selectedVoice = "";
            foreach (InstalledVoice voice in synthesizer.GetInstalledVoices())
            {
                Console.WriteLine($"- {voice.VoiceInfo.Name}");
                selectedVoice = voice.VoiceInfo.Name;
            }
            synthesizer.SelectVoice(selectedVoice);
            Console.WriteLine("Using voice: " + selectedVoice);

            // Get user input for text to be spoken
            Console.Write("Enter text to be spoken: ");
            string text = Console.ReadLine();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                synthesizer.SetOutputToWaveStream(memoryStream);
                synthesizer.Speak(text);

                // Reset memory stream position
                memoryStream.Position = 0;

                // List available audio devices
                Console.WriteLine("Available Audio Devices:");
                var deviceEnumerator = new MMDeviceEnumerator();
                var devices = deviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);
                int deviceIndex = 0;
                foreach (var device in devices)
                {
                    Console.WriteLine($"{deviceIndex++}. {device.FriendlyName}");
                }

                // Prompt user for device selection
                Console.Write("Select an audio device by index: ");
                if (!int.TryParse(Console.ReadLine(), out int selectedIndex) || selectedIndex < 0 || selectedIndex >= devices.Count)
                {
                    Console.WriteLine("Invalid selection. Exiting...");
                    return;
                }

                // Play the memory stream on the selected audio device using CSCore
                using (var waveSource = new WaveFileReader(memoryStream))
                {
                    using (var soundOut = new WasapiOut())
                    {
                        soundOut.Device = devices[selectedIndex];
                        soundOut.Initialize(waveSource);
                        soundOut.Play();

                        // Use an event to wait for the audio to finish playing
                        bool playbackFinished = false;
                        soundOut.Stopped += (s, e) => playbackFinished = true;

                        while (!playbackFinished)
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
            }
        }
    }
}
