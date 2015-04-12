using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Serialization;
using MessageBox = System.Windows.MessageBox;

namespace ChatWheel
{
    public class Settings
    {
        public Settings()
        {
            PhrasesAmount = 5;
            HotKey = (int) Keys.Oemtilde;
            Phrases = new[]
            {
                new Phrase("First", "First"),
                new Phrase("Second", "Second"),
                new Phrase("Third", "Third"),
                new Phrase("Fourth", "Fourth"),
                new Phrase("Fifth", "Fifth"),
                new Phrase("Sixth", "Sixth"),
                new Phrase("Seventh", "Seventh"),
                new Phrase("Eighth", "Eighth")
            };
        }

        public int PhrasesAmount { get; set; }
        public int HotKey { get; set; }
        public Phrase[] Phrases { get; set; }

        public static Settings Deserialize()
        {
            if (!File.Exists("./heroeswheel.xml")) return null;
            try
            {
                var xmlSerializer = new XmlSerializer(typeof (Settings));
                using (var fs = File.OpenRead("./heroeswheel.xml"))
                {
                    return (Settings) xmlSerializer.Deserialize(fs);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("There was an error when saving settings.\n" + e.Message,
                    "Serialization error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }

        public static void Serialize(Settings settings)
        {
            try
            {
                var xmlSerializer = new XmlSerializer(typeof (Settings));
                using (var fs = File.OpenWrite("./heroeswheel.xml"))
                {
                    xmlSerializer.Serialize(fs, settings);
                    ;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("There was an error when saving settings.\n" + e.Message,
                    "Serialization error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public class Phrase
        {
            public Phrase()
            {
            }

            public Phrase(string shortPhrase, string fullPhrase)
            {
                ShortPhrase = shortPhrase;
                FullPhrase = fullPhrase;
            }

            public string FullPhrase { get; set; }
            public string ShortPhrase { get; set; }
        }
    }
}