using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Volo.Docs.Documents;

public class DocumentParams
{
    [JsonPropertyName("Parameters")]
    public List<DocumentParameter> Parameters { get; set; } = new();
    
    
    public class DocumentParameter
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }
    
        [JsonPropertyName("values")]
        public List<Dictionary<string, string>> Values { get; set; } 
    }
}