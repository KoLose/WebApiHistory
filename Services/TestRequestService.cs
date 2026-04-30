using Microsoft.AspNetCore.Mvc;
using WebApi.Interfaces;
using WebApi.Models;
using WebApi.Requests;

namespace WebApi.Services;

public class TestRequestService: ITestRequestService
{
    
    private readonly Supabase.Client _supabaseClient;

    private readonly string _folderId;
    private readonly string _apiKey;
    
    private const string ModelName = "gemma-3-27b-it/latest";

    public TestRequestService(Supabase.Client supabaseClient, string folderId, string apiKey)
    {
        _supabaseClient = supabaseClient;
        _folderId = folderId;
        _apiKey = apiKey;
    }

    public async Task<IActionResult> GetAndSaveImageAsync(CreateNewRequest newRequest)
    {
        await _supabaseClient.InitializeAsync();

        string imageUrl = "hahaha";
        string excelUrl = "hihihi";

        try
        {
            var request = new Request()
            {
                ImageUrl = imageUrl,
                ExcelUrl = excelUrl,
                UserId = newRequest.UserId
            };
            await _supabaseClient.From<Request>().Insert(request);

            return new OkObjectResult(new
                {
                    status = true,
                }
            );
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(new
                {
                    status = false, 
                    error = e.Message,
                    inner = e.InnerException?.Message
                }
            );
        }
    }
}