using System.Speech.Synthesis;

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

            synthesizer.Speak(text);
        }
    }
}
