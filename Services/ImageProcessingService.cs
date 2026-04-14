using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using System.Text.Json;
using ClosedXML.Excel;
using System.Data;

namespace WebApi.Services;

public class ImageProcessingService
{
    private readonly Supabase.Client _supabaseClient;
    private readonly YandexGeminiService _yandexService;

    public ImageProcessingService(Supabase.Client supabaseClient, YandexGeminiService yandexService)
    {
        _supabaseClient = supabaseClient;
        _yandexService = yandexService;
    }

    public async Task<IActionResult> ProcessAndSaveImageAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return new BadRequestObjectResult(new { status = false, message = "Файл не передан" });

            await _supabaseClient.InitializeAsync();

            // 1. Читаем файл
            using var stream = file.OpenReadStream();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            // 2. Сохраняем картинку в Storage
            var ext = Path.GetExtension(file.FileName);
            var imgPath = $"images/{Guid.NewGuid()}{ext}";
            await _supabaseClient.Storage.From("Storage").Upload(fileBytes, imgPath);
            var imageUrl = $"https://bccvmwlqehhsbldanwao.supabase.co/storage/v1/object/public/Storage/{imgPath}";

            // 3. Отправляем в Яндекс
            string base64WithPrefix = $"data:image/jpeg;base64,{Convert.ToBase64String(fileBytes)}";
            string prompt = @"Распознай таблицу на изображении. 
            Верни результат СТРОГО в формате JSON массива объектов. 
            Каждый объект — это строка таблицы. 
            Ключи объекта — это названия колонок (например, ""Год"", ""Шелк"", ""Цена""). Значения — данные из ячеек. 
            Если заголовки отсутствуют или неразборчивы, используй ""Col1"", ""Col2"" и т.д. 
            Не используй числа в качестве ключей. 
            Пример: [{""Год"": ""1855"", ""Данные"": ""2545""}, {""Год"": ""1856"", ""Данные"": ""2459""}]. 
            Никакого лишнего текста, только JSON.";
            
            string aiResponse = await _yandexService.RecognizeTextAsync(base64WithPrefix, prompt);
            
            // Очистка ответа от markdown (```json ... ```)
            string cleanJson = System.Text.RegularExpressions.Regex.Replace(aiResponse, @"^```json\s*|\s*```$", "", System.Text.RegularExpressions.RegexOptions.Multiline).Trim();

            // 4. ГЕНЕРАЦИЯ EXCEL ИЗ JSON
            byte[] excelBytes;
            try 
            {
                excelBytes = GenerateExcelFromJson(cleanJson);
            }
            catch (Exception ex)
            {
                // Если JSON кривой, создаем Excel с одной ячейкой "Ошибка распознавания"
                excelBytes = CreateErrorExcel($"Ошибка парсинга JSON: {ex.Message}\n\nRaw Data:\n{cleanJson}");
            }

            // 5. Сохраняем Excel в Storage
            var excelPath = $"excels/{Guid.NewGuid()}.xlsx";
            await _supabaseClient.Storage.From("Storage").Upload(excelBytes, excelPath);
            var excelUrl = $"https://bccvmwlqehhsbldanwao.supabase.co/storage/v1/object/public/Storage/{excelPath}";

            // 6. Запись в БД
            Random rnd = new Random();
            int id = rnd.Next(0, 1_000_000);
                
            var image = new Image
            {
                Id = id,
                ImageUrl = imageUrl,
                ExcelUrl = excelUrl // Теперь тут ссылка на реальный файл!
            };

            await _supabaseClient.From<Image>().Insert(image);

            return new OkObjectResult(new { status = true, excel_url = excelUrl });
        }
        catch (Exception e)
        {   
            return new ObjectResult(new 
            { 
                status = false,
                error = e.Message, 
                inner = e.InnerException?.Message
            }) 
            { 
                StatusCode = 500 
            };
        }
    }

    // Helper: Создает Excel из JSON строки
    private byte[] GenerateExcelFromJson(string json)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Data");

        // Пробуем распарсить как массив объектов
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        
        // Если это массив объектов [{"col": "val"}, ...]
        if (json.StartsWith("["))
        {
            var data = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json, options);
            
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
             // Если просто объект или текст
             worksheet.Cell(1, 1).Value = "Raw Response";
             worksheet.Cell(2, 1).Value = json;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // Helper: Создает Excel с ошибкой
    private byte[] CreateErrorExcel(string errorText)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Error");
        worksheet.Cell(1, 1).Value = errorText;
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}