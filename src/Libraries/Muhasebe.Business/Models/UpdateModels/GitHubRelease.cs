using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Muhasebe.Business.Models.UpdateModels
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

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }
    }
}
