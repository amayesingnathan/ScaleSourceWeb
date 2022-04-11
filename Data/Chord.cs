namespace ScaleSourceWeb.Data
{
    enum Fret
    {
        Muted = -1,
        Open,
        First,
        Second,
        Third,
        Fourth,
        Fifth
    } 

    class Chord
    {
        public string? Strings { get; set; }
        public IEnumerable<Fret>? FretList => Strings?.Split(" ").Select(
            stringName => (Fret)(stringName.Equals("X") ? -1 : Int32.Parse(stringName)));
        public string? Fingering { get; set; }
        public IEnumerable<string>? FingeringList => Fingering?.Split(" ");
        public string? ChordName { get; set; }
        public string? EnharmonicChordName { get; set; }
        public string? VoicingID { get; set; }
        public string? Tones { get; set; }

    }
}
