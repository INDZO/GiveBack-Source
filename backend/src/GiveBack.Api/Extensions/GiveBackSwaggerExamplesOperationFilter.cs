using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GiveBack.Api.Extensions;

public sealed class GiveBackSwaggerExamplesOperationFilter : IOperationFilter
{
    private static readonly Dictionary<string, string> RequestExamples = new(StringComparer.OrdinalIgnoreCase)
    {
        ["POST /api/v{version}/opportunities"] = """
            {
              "causeId": 1,
              "title": "Food bank shift",
              "description": "Help sort and deliver donated food.",
              "requiredVolunteersCount": 5,
              "startAt": "2026-06-20T09:00:00Z",
              "endAt": "2026-06-20T13:00:00Z",
              "periodDescription": null,
              "location": "Belgrade",
              "format": 1,
              "durationHours": 4,
              "type": 1,
              "visibility": 1,
              "isTeamBased": false,
              "isUrgent": false,
              "skillIds": [3]
            }
            """,
        ["PUT /api/v{version}/opportunities/{id}"] = """
            {
              "causeId": 1,
              "title": "Updated food bank shift",
              "description": "Help sort and deliver donated food.",
              "requiredVolunteersCount": 6,
              "startAt": "2026-06-20T09:00:00Z",
              "endAt": "2026-06-20T13:00:00Z",
              "periodDescription": null,
              "location": "Belgrade",
              "format": 1,
              "durationHours": 4,
              "type": 1,
              "visibility": 1,
              "isTeamBased": false,
              "isUrgent": false,
              "skillIds": [3]
            }
            """,
        ["POST /api/v{version}/opportunities/{id}/apply"] = """
            {
              "message": "I am available and have previous experience."
            }
            """,
        ["POST /api/v{version}/opportunities/{id}/apply-team"] = """
            {
              "teamId": 1,
              "message": "Our confirmed members are ready to help."
            }
            """,
        ["POST /api/v{version}/hour-logs"] = """
            {
              "applicationId": 1,
              "workDate": "2026-06-20T00:00:00Z",
              "submittedHours": 4,
              "description": "Sorted and packed donated food."
            }
            """,
        ["PATCH /api/v{version}/hour-logs/{id}/correct"] = """
            {
              "approvedHours": 3.5
            }
            """,
        ["POST /api/v{version}/certificates/generate"] = """
            {
              "periodFrom": "2026-01-01T00:00:00Z",
              "periodTo": "2026-12-31T00:00:00Z"
            }
            """,
        ["POST /api/v{version}/opportunities/{id}/invite"] = """
            {
              "volunteerId": 1
            }
            """,
        ["PATCH /api/v{version}/admin/organizations/{id}/reject"] = """
            {
              "reason": "Registration data could not be verified."
            }
            """
    };

    private static readonly Dictionary<string, string> ResponseExamples = new(StringComparer.OrdinalIgnoreCase)
    {
        ["PATCH /api/v{version}/applications/{id}/accept"] = """{ "status": "accepted" }""",
        ["PATCH /api/v{version}/applications/{id}/reject"] = """{ "status": "rejected" }""",
        ["PATCH /api/v{version}/hour-logs/{id}/approve"] = """{ "status": "approved" }""",
        ["PATCH /api/v{version}/opportunities/invitations/{id}/accept"] = """{ "status": "accepted" }""",
        ["PATCH /api/v{version}/opportunities/invitations/{id}/decline"] = """{ "status": "declined" }""",
        ["PATCH /api/v{version}/admin/organizations/{id}/verify"] = """{ "isVerified": true }""",
        ["GET /api/v{version}/admin/statistics"] = """{ "usersCount": 10, "volunteersCount": 6, "organizationsCount": 3, "totalVerifiedHours": 42 }"""
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var rawKey = $"{context.ApiDescription.HttpMethod} /{context.ApiDescription.RelativePath}".Split('?')[0];
        var key = Regex.Replace(rawKey, @"/v\d+(\.\d+)?(?=/)", "/v{version}");
        if (RequestExamples.TryGetValue(key, out var requestJson) &&
            operation.RequestBody is { Content: not null } requestBody &&
            requestBody.Content.TryGetValue("application/json", out var requestMediaType))
        {
            requestMediaType.Example = JsonNode.Parse(requestJson);
        }

        if (ResponseExamples.TryGetValue(key, out var responseJson) &&
            operation.Responses is not null &&
            operation.Responses.TryGetValue("200", out var response) &&
            response is OpenApiResponse concreteResponse)
        {
            concreteResponse.Content ??= new Dictionary<string, OpenApiMediaType>();
            if (!concreteResponse.Content.TryGetValue("application/json", out var responseMediaType))
            {
                responseMediaType = new OpenApiMediaType();
                concreteResponse.Content["application/json"] = responseMediaType;
            }
            responseMediaType.Example = JsonNode.Parse(responseJson);
        }
    }
}
