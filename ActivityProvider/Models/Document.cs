using ActivityProvider.Models.Atores;

namespace ActivityProvider.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string ActivityId { get; set; }
        public string Text { get; set; }
        public string Instructions { get; set; }
        public string LanguageFrom { get; set; }
        public string LanguageTo { get; set; }
        public List<ActorProcess> Processes { get; set; }
    }
}
