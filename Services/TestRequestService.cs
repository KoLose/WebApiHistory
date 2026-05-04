using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using WebApi.Interfaces;
using WebApi.Models;
using WebApi.Models.Response;
using WebApi.Requests;

namespace WebApi.Services;

public class TestRequestService : ITestRequestService
{
    private readonly Supabase.Client _supabaseClient;

    private readonly string _folderId;
    private readonly string _apiKey;

    private const string ModelName = "gemma-3-27b-it/latest";

    private string prompt =
        @"Распознай таблицу на изображении.
        Верни результат в виде текста, где столбцы разделены символом ; (точка с запятой), а строки переносом строки.
        Первая строка — заголовки.
        Никакого лишнего текста, только данные.
        Пример:
        Дата;Сумма;Описание
        01.01.2024;1000;Зарплата
        02.01.2024;500;Подарок";

    private string endpoint = "https://ai.api.cloud.yandex.net/v1/chat/completions";

    public TestRequestService(Supabase.Client supabaseClient, string folderId, string apiKey)
    {
        _supabaseClient = supabaseClient;
        _folderId = folderId;
        _apiKey = apiKey;
    }
    
    public async Task<IActionResult> PostRequestAsync(CreateNewRequest request)
    {
        try
        {
            byte[] userFileByte = await ReadImageFile(request.FileUser); // Call method for reading user image
            
            string aiAnswer = await SendRequetAI(userFileByte); // Call method for send request to AI
            
            byte[] aiAnswerByte = CreateExcel(aiAnswer); // Call method for creating excel file
            
            var userFileExtenstion = Path.GetExtension(request.FileUser.FileName);
            string userFileUrl = await CreatePathStorage(userFileByte, "image", userFileExtenstion);
            string aiAnswerlUrl = await CreatePathStorage(aiAnswerByte, "excel", ".xlsx");

            await SaveRequestDB(userFileUrl, aiAnswerlUrl, request.UserId); // Call method for creating row in request table

            return new OkObjectResult(new
            {
                status = true
            });
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(new
                {
                    status = false,
                    error = e.Message
                }
            );
        }
    }

    private async Task<byte[]> ReadImageFile(IFormFile imageFile)
    {
        using var stream = imageFile.OpenReadStream();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        
        return ms.ToArray();
    }

    private async Task<string> SendRequetAI(byte[] userFileByte)
    {
        
        string base64Image = Convert.ToBase64String(userFileByte);
        
        var requestBody = new
        {
            model = $"gpt://{_folderId}/{ModelName}",
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = prompt },
                        new
                        {
                            type = "image_url",
                            image_url = new { url = $"data:image/jpeg;base64,{base64Image}" } 
                        }
                    }
                }
            },
            temperature = 0,
            max_tokens = 1000
        };

        var json = JsonSerializer.Serialize(requestBody);

        using var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        try
        {
            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return responseBody;
            }

            var jsonDoc = JsonDocument.Parse(responseBody);
            string content = jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";

            string answer = Regex.Replace(content, @"^```json\s*|\s*```$", "", RegexOptions.Multiline).Trim();

            return answer;
        }

        catch (Exception e)
        {
            return $"EXCEPTION: {e.Message}";
        }
    }

    private byte[] CreateExcel(string aiAnswer)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Data");

        var lines = aiAnswer.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        for (int row = 0; row < lines.Length; row++)
        {
            var columns = lines[row].Split(';');

            for (int col = 0; col < columns.Length; col++)
            {
                var cell = worksheet.Cell(row + 1, col + 1);
                cell.Value = columns[col].Trim();

                if (row == 0)
                    cell.Style.Font.Bold = true;
            }
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private async Task<string> CreatePathStorage(byte[] file, string folder, string extension)
    {
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = $"{folder}/{fileName}";
        
        await _supabaseClient.Storage.From("storage").Upload(file, filePath);

        return $"https://usumwizzaswjmiucesxo.supabase.co/storage/v1/object/public/storage/{filePath}";
    }

    private async Task SaveRequestDB(string userFileUrl, string aiAnswerUrl, Guid userId)
    {
        var request = new Request
        {
            Request_id = Guid.NewGuid(),
            ImageUrl = userFileUrl,
            ExcelUrl = aiAnswerUrl,
            UserId = userId
        };

        await _supabaseClient.From<Request>().Insert(request);
    }
    
    public async Task<IActionResult> GetRequestsAsync(Guid userId)
    {
        try
        {
            await _supabaseClient.InitializeAsync();

            var response = await _supabaseClient.From<WebApi.Models.Request>()
                .Where(r => r.UserId == userId)
                .Get();

            var requests = response.Models.Select(r => new ImageResponse
            {
                UserId = r.UserId,
                ImageUrl = r.ImageUrl,
                ExcelUrl = r.ExcelUrl
            }).ToList();

            return new OkObjectResult(new
            {
                data = new
                {
                    image = requests
                },
                status = true
            });
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(new
            {
                status = false,
                error = e.Message
            });
        }
    }
}