using Syncfusion.Blazor.Buttons;

namespace ScaleSourceWeb.Data
{
    public enum ChordEntryStage
    {
        Note,
        Accidental,
        Scale,
        Display
    }

    class EntryButton
    {
        public static readonly string[] NoteNames = { "A", "B", "C", "D", "E", "F", "G" };
        public static readonly string[] AccidentalNames = { "Natural", "Sharp", "Flat" };
        public static readonly string[] ScaleNames = { "Major", "Minor" };

        public SfButton? Button { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string APIName => ToAPIName(Name);
        public ChordEntryStage Stage { get; set; }

        public EntryButton(int i, ChordEntryStage stage)
        {
            Index = i;
            Stage = stage;
            switch (stage)
            {
                case ChordEntryStage.Note:
                    Name = NoteNames[i];
                    break;

                case ChordEntryStage.Accidental:
                    Name = AccidentalNames[i];
                    break;

                case ChordEntryStage.Scale:
                    Name = ScaleNames[i];
                    break;

                default:
                    Name = "";
                    break;
            }
        }

        private string ToAPIName(string name)
        {
            if (Stage == ChordEntryStage.Note) return name;

            switch (name)
            {
                case "Natural":
                    return "";

                case "Sharp":
                    return "#";

                case "Flat":
                    return "b";

                case "Major":
                    return "";

                case "Minor":
                    return "m";

                default:
                    return "";
            }
        }
    }
}
