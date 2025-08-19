using Newtonsoft.Json;

namespace Muhasebe.Business.Models
{
    public class GitHubRelease
    {
        [JsonProperty("tag_name")]
        public string TagName { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("assets")]
        public GitHubAsset[] Assets { get; set; }

        [JsonProperty("published_at")]
        public DateTime PublishedAt { get; set; }
    }
}
