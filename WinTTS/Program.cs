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

            string text;
            if (args.Length > 0)
            {
                // Get CLI argument
                text = args[0];
            }
            else
            {
                // Get user input
                Console.Write("Enter text to be spoken: ");
                text = Console.ReadLine();
            }

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

                // Select index from CLI or user input
                int selectedIndex;
                if (args.Length > 1 && int.TryParse(args[1], out selectedIndex) && selectedIndex >= 0 && selectedIndex < devices.Count)
                {
                    Console.WriteLine($"Selected audio device: {devices[selectedIndex].FriendlyName}");
                }
                else
                {
                    Console.Write("Select an audio device by index: ");
                    if (!int.TryParse(Console.ReadLine(), out selectedIndex) || selectedIndex < 0 || selectedIndex >= devices.Count)
                    {
                        Console.WriteLine("Invalid selection. Exiting...");
                        return;
                    }
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
