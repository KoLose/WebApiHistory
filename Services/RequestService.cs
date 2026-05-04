using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using WebApi.Interfaces;
using WebApi.Models;
using WebApi.Requests;

namespace WebApi.Services;

public class RequestService: IRequestService
{
    private readonly Supabase.Client _supabaseClient;

    private readonly string _folderId;
    private readonly string _apiKey;
    
    private const string ModelName = "gemma-3-27b-it/latest";

    public RequestService(Supabase.Client supabaseClient, string folderId, string apiKey)
    {
        _supabaseClient = supabaseClient;
        _folderId = folderId;
        _apiKey = apiKey;
    }

    public async Task<IActionResult> GetAndSaveImageAsync(CreateNewRequest newRequest)
    {
        await _supabaseClient.InitializeAsync();
        
        try
        {
            if (newRequest != null)
            {
                using var stream = newRequest.FileUser.OpenReadStream();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var fileBytes = ms.ToArray();

                string textRequest = $"data:image/jpeg;base64,{Convert.ToBase64String(fileBytes)}";
                string prompt =
                    @"Распознай таблицу на изображении.
                    Верни результат в виде текста, где столбцы разделены символом ; (точка с запятой), а строки переносом строки.
                    Первая строка — заголовки.
                    Никакого лишнего текста, только данные.
                    Пример:
                    Дата;Сумма;Описание
                    01.01.2024;1000;Зарплата
                    02.01.2024;500;Подарок";

                string answer = await SendRequest(textRequest, prompt);

                Console.WriteLine(answer);
        //        byte[] excelBytes;

         //       excelBytes = CreateExcel(answer);
                
                   var ext = Path.GetExtension(newRequest.FileUser.FileName);
                    
                    var imgPath = $"image/{Guid.NewGuid()}{ext}";
                    await _supabaseClient.Storage.From("storage").Upload(fileBytes, imgPath);
                    var imageUrl = $"https://bccvmwlqehhsbldanwao.supabase.co/storage/v1/object/public/storage/{imgPath}";

                    Console.WriteLine(imageUrl);
                    
                    /*             var excelPath = $"excel/{Guid.NewGuid()}.xlsx";
                                await _supabaseClient.Storage.From("storage").Upload(excelBytes, excelPath);
                                var excelUrl =
                                    $"https://bccvmwlqehhsbldanwao.supabase.co/storage/v1/object/public/storage/{excelPath}";

                                Console.WriteLine(excelUrl);
                       */   
                    var request = new Request()
                    {
                        ImageUrl = imageUrl,
                        ExcelUrl = "хихи",
                        UserId = newRequest.UserId
                    };
                    
                    await _supabaseClient.From<Request>().Insert(request);
                    
                    return new OkObjectResult(new
                        {
                            status = true,
                        }
                    );
                }
            
            return new BadRequestObjectResult(new
                {
                    status = false,
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

    public async Task<string> SendRequest(string textRequest, string prompt)
    {
        var endpoint = "https://ai.api.cloud.yandex.net/v1/chat/completions";
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
                            image_url = new { url = textRequest }
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

    public byte[] CreateExcel(string text)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Data");
        
        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        for (int row = 0; row < lines.Length; row++)
        {
            // Разбиваем каждую строку по точке с запятой
            var columns = lines[row].Split(';');
        
            for (int col = 0; col < columns.Length; col++)
            {
                var cell = worksheet.Cell(row + 1, col + 1);
                cell.Value = columns[col].Trim(); // Убираем пробелы
            
                if (row == 0) 
                    cell.Style.Font.Bold = true; // Заголовки жирные
            }
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}