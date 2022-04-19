namespace ChordCanvas
{
    public class Chord
    {
        public enum Fingers
        {
            NoFinger = -1,
            Thumb,
            Index,
            Middle,
            Ring,
            Little
        }

        public string Strings { get; set; } = "";
        public IEnumerable<int> FretList => Strings.Split(" ").Select(
            stringName => (stringName.Equals("X") ? -1 : Int32.Parse(stringName)));
        public string Fingering { get; set; } = "";
        public IEnumerable<Fingers> FingeringList => Fingering.Split(" ").Select(
            fingerName => (Fingers)(fingerName.Equals("X") ? -1 : Int32.Parse(fingerName)));
        public string ChordName { get; set; } = "";
        public string EnharmonicChordName { get; set; } = "";
        public string VoicingID { get; set; } = "";
        public string Tones { get; set; } = "";

    }
}
