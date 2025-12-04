using System.Net.Http.Json;
using SubrogationDemandManagement.Domain.Models;

namespace SubrogationDemandManagement.UI.Services;

public class SubrogationApiClient
{
    private readonly HttpClient _httpClient;

    public SubrogationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Cases
    public async Task<List<SubrogationCase>> GetCasesAsync(Guid tenantId, int skip = 0, int take = 50)
    {
        return await _httpClient.GetFromJsonAsync<List<SubrogationCase>>(
            $"api/SubrogationCases?tenantId={tenantId}&skip={skip}&take={take}") ?? new List<SubrogationCase>();
    }

    public async Task<SubrogationCase?> GetCaseAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<SubrogationCase>($"api/SubrogationCases/{id}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<SubrogationCase> CreateCaseAsync(SubrogationCase subrogationCase)
    {
        var response = await _httpClient.PostAsJsonAsync("api/SubrogationCases", subrogationCase);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SubrogationCase>() 
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    // Packages
    public async Task<List<DemandPackage>> GetPackagesAsync(Guid caseId)
    {
        return await _httpClient.GetFromJsonAsync<List<DemandPackage>>(
            $"api/DemandPackages?caseId={caseId}") ?? new List<DemandPackage>();
    }

    public async Task<DemandPackage> CreatePackageAsync(DemandPackage package)
    {
        var response = await _httpClient.PostAsJsonAsync("api/DemandPackages", package);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DemandPackage>()
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task GeneratePackageAsync(Guid packageId)
    {
        var response = await _httpClient.PostAsync($"api/DemandPackages/{packageId}/generate", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendPackageAsync(Guid packageId, List<string> recipients)
    {
        var request = new { Recipients = recipients };
        var response = await _httpClient.PostAsJsonAsync($"api/DemandPackages/{packageId}/send", request);
        response.EnsureSuccessStatusCode();
    }
}
