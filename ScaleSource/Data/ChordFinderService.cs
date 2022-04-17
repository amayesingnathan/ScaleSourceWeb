using System.Net.Http.Headers;
using ChordCanvas;

#pragma warning disable 8619

namespace ScaleSourceWeb.Data
{
    class ChordFinderService
    {

        static HttpClient sClient = new HttpClient();
        static bool sInitialised = false;
        static DateTime? sLastChordTStamp = null;

        public static void Init()
        {
            // Initialise HTTP Client on first call.
            if (!sInitialised)
            {
                sClient.BaseAddress = new Uri("https://api.uberchord.com/v1/chords/");
                sClient.DefaultRequestHeaders.Accept.Clear();
                sClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                sInitialised = true;
            }
        }
        private static async Task<Chord?> GetChordAsync(string path)
        {
            Chord? chord = null;
            HttpResponseMessage response = await sClient.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<List<Chord>>();
                chord = result.FirstOrDefault();
                if (chord is not null)
                    chord.ChordName = chord.ChordName.Replace(",", "");
            }

            sLastChordTStamp = DateTime.Now;
            return chord;
        }

        public static async Task<Chord?> FindChord(string chordName)
        {
            // Initialise HTTP Client on first call.
            if (!sInitialised)
            {
                sClient.BaseAddress = new Uri("https://api.uberchord.com/v1/chords/");
                sClient.DefaultRequestHeaders.Accept.Clear();
                sClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                sInitialised = true;
            }

            if (sLastChordTStamp is null)
                sLastChordTStamp = DateTime.Now;

            try
            {
                return await GetChordAsync(chordName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        private static IEnumerable<Chord> PruneNullChords(IEnumerable<Chord?> Progression)
        {
            return Progression.Where(i => i != null);
        }

        public static IEnumerable<Chord> GetProgression(IEnumerable<string> ChordNames)
        {
            var ChordProgression = ChordNames.Select(name => FindChord(name).Result);

            var FinalChordProgression = PruneNullChords(ChordProgression);

            return FinalChordProgression;
        }
    }
}
