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

public class RequestService : IRequestService
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
                using var stream = newRequest.File.OpenReadStream();
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
                byte[] excelBytes;

                excelBytes = CreateExcelFromJson(answer);

                var ext = Path.GetExtension(newRequest.File.FileName);

                var imgPath = $"image/{Guid.NewGuid()}{ext}";
                await _supabaseClient.Storage.From("storage").Upload(fileBytes, imgPath);
                var imageUrl = $"https://bccvmwlqehhsbldanwao.supabase.co/storage/v1/object/public/storage/{imgPath}";

                Console.WriteLine(imageUrl);
                Console.WriteLine(excelBytes);
                
                var excelPath = $"excel/{Guid.NewGuid()}.xlsx";
                await _supabaseClient.Storage.From("storage").Upload(excelBytes, excelPath);
                var excelUrl =
                    $"https://bccvmwlqehhsbldanwao.supabase.co/storage/v1/object/public/storage/{excelPath}";

                Console.WriteLine(excelUrl);

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

    private byte[] CreateExcelFromJson(string json)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Data");

        // Очищаем от markdown, если ИИ его добавил
        string cleanJson = System.Text.RegularExpressions.Regex.Replace(json, @"^```\w*\s*|\s*```$", "",
            System.Text.RegularExpressions.RegexOptions.Multiline).Trim();

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Парсим как массив объектов
            if (cleanJson.StartsWith("["))
            {
                var data = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(cleanJson, options);

                if (data != null && data.Count > 0)
                {
                    // Заголовки
                    var headers = data[0].Keys.ToList();
                    for (int i = 0; i < headers.Count; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = headers[i];
                        worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    }

                    // Данные
                    for (int row = 0; row < data.Count; row++)
                    {
                        for (int col = 0; col < headers.Count; col++)
                        {
                            var key = headers[col];
                            var value = data[row][key]?.ToString() ?? "";
                            worksheet.Cell(row + 2, col + 1).Value = value;
                        }
                    }
                }
            }
            else
            {
                // Если пришел не массив, а один объект или ошибка
                worksheet.Cell(1, 1).Value = "Raw Response";
                worksheet.Cell(2, 1).Value = cleanJson;
            }
        }
        catch (Exception ex)
        {
            // Если JSON битый, пишем ошибку в Excel, чтобы не ронять весь запрос
            worksheet.Cell(1, 1).Value = "JSON Parse Error";
            worksheet.Cell(2, 1).Value = ex.Message;
            worksheet.Cell(3, 1).Value = cleanJson;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}