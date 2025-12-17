using Newtonsoft.Json.Linq;
using NSwag;
using System.Reflection;
using Xunit.Sdk;

namespace DataApi.ContractTests
{

    public class EndpointDataAttribute : DataAttribute
    {
        // Cache the document once across all tests using this attribute
        private static readonly Lazy<OpenApiDocument> Doc = new Lazy<OpenApiDocument>(() =>
        {
            var baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL")
                ?? throw new InvalidOperationException("API_BASE_URL is not set.");
            using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
            var json = client.GetStringAsync("data/swagger/v1/swagger.json").Result;
            return OpenApiDocument.FromJsonAsync(json).Result;
        });

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var doc = Doc.Value;

            foreach (var path in doc.Paths)
            {
                foreach (var op in path.Value)
                {
                    // op.Key is "get", "post", etc. If you only want GETs, filter here.
                    yield return new object[] { path.Key, op.Key, op.Value };
                }
            }
        }
    }

    public class ContractTests
    {
        private readonly HttpClient _client;

        public ContractTests()
        {
            var baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL")
                ?? throw new InvalidOperationException("API_BASE_URL is not set.");
            _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        }

        [Theory]
        [EndpointData]
        public async Task Endpoint_ReturnsValidSchema(string path, string verb, OpenApiOperation operation)
        {
            // Substitute placeholders with sample values
            var substitutedPath = path
                .Replace("{locationIdentifier}", "123")
                .Replace("{dataType}", "speed");

            var requestUrl = "/data" + substitutedPath;

            // Add query params if required
            if (path.Contains("GetData"))
            {
                requestUrl += "?start=2025-01-01&end=2025-01-02";
            }

            var res = verb switch
            {
                "get" => await _client.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead),
                //"post" => await _client.PostAsync(requestUrl, new StringContent("{}")),
                _ => throw new NotSupportedException($"Verb {verb} not supported")
            };

            res.EnsureSuccessStatusCode();

            if (operation.Responses.TryGetValue("200", out var okResponse))
            {
                if (okResponse.Content.TryGetValue("application/json", out var mediaType))
                {
                    var schema = mediaType.Schema;
                    var json = await res.Content.ReadAsStringAsync();
                    var token = JToken.Parse(json);
                    var errors = schema.Validate(token);
                    Assert.True(errors.Count == 0, $"Schema validation failed: {string.Join(", ", errors)}");
                }

                if (okResponse.Content.TryGetValue("application/x-ndjson", out var ndjsonMediaType))
                {
                    var schema = ndjsonMediaType.Schema;
                    using var stream = await res.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream);

                    string? line;
                    int lineNum = 0;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        lineNum++;
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var token = JToken.Parse(line);
                        var errors = schema.Validate(token);
                        Assert.True(errors.Count == 0,
                            $"Schema validation failed at line {lineNum}: {string.Join(", ", errors)}");
                    }
                }
            }
        }


        //[Theory]
        //[EndpointData]
        //public async Task LocationIdentifier_NotFound_Returns404(string path, string verb, OpenApiOperation operation)
        //{
        //    var substitutedPath = path.Replace("{locationIdentifier}", "slinky");
        //    var requestUrl = "/data" + substitutedPath;

        //    var res = await _client.GetAsync(requestUrl);
        //    Assert.Equal(System.Net.HttpStatusCode.NotFound, res.StatusCode);
        //}

        //[Theory]
        //[EndpointData]
        //public async Task StartGreaterThanEnd_Returns400(string path, string verb, OpenApiOperation operation)
        //{
        //    if (!path.Contains("GetData")) return;

        //    var substitutedPath = path.Replace("{locationIdentifier}", "123")
        //                              .Replace("{dataType}", "speed");
        //    var requestUrl = "/data" + substitutedPath + "?start=2025-12-31&end=2025-01-01";

        //    var res = await _client.GetAsync(requestUrl);
        //    Assert.Equal(System.Net.HttpStatusCode.BadRequest, res.StatusCode);
        //}

        //[Theory]
        //[EndpointData]
        //public async Task StartEndNullOrDefault_Returns400(string path, string verb, OpenApiOperation operation)
        //{
        //    if (!path.Contains("GetData")) return;

        //    var substitutedPath = path.Replace("{locationIdentifier}", "123")
        //                              .Replace("{dataType}", "speed");
        //    var requestUrl = "/data" + substitutedPath + "?start=&end=";

        //    var res = await _client.GetAsync(requestUrl);
        //    Assert.Equal(System.Net.HttpStatusCode.BadRequest, res.StatusCode);
        //}

        //[Theory]
        //[EndpointData]
        //public async Task InvalidDataType_Returns400(string path, string verb, OpenApiOperation operation)
        //{
        //    var substitutedPath = path.Replace("{locationIdentifier}", "123")
        //                              .Replace("{dataType}", "slinky");
        //    var requestUrl = "/data" + substitutedPath;

        //    var res = await _client.GetAsync(requestUrl);
        //    Assert.Equal(System.Net.HttpStatusCode.BadRequest, res.StatusCode);
        //}
    }
}
