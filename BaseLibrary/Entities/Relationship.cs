using System.Text.Json.Serialization;

namespace BaseLibrary.Entities
{
    public class Relationship
    {
        /// <summary>
        /// Relationship : One to Many
        /// </summary>
        [JsonIgnore]
        public List<Employye>? Employyes { get; set; }
    }
}
