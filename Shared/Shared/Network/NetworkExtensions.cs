using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web;

namespace Shared.Network
{
    public static class NetworkExtensions
    {
        public static string ToQueryString(this object obj, bool addInterrogation = true)
        {
            var content = addInterrogation ? "?" : string.Empty;
            foreach (var property in obj.GetType().GetRuntimeProperties())
            {
                var value = property.GetValue(obj);
                if (value is string valueString)
                    content += $"{property.Name}={HttpUtility.UrlEncode(valueString)}&";
            }
            return content;
        }

        public static string ToCustomString(this object obj, string separator = ",", bool addFieldName = false)
        {
            var content = string.Empty;
            foreach (var property in obj.GetType().GetRuntimeProperties())
            {
                var value = property.GetValue(obj);
                if (value is string valueString)
                {
                    if (addFieldName)
                        content += $"{property.Name}=";
                    content += $"{valueString}{separator}";
                }
            }
            return content;
        }

        public static MultipartFormDataContent ToMultipartFormDataContent(this object obj)
        {
            var content = new MultipartFormDataContent();

            // Caso 1: El objeto es directamente un IFormFile
            if (obj is IFormFile formFile)
            {
                AddFormFileContent(content, formFile, "file"); // "file" es el nombre del campo en el formulario
                return content;
            }

            // Caso 2: El objeto contiene propiedades de tipo IFormFile
            foreach (var property in obj.GetType().GetRuntimeProperties())
            {
                var value = property.GetValue(obj);

                if (value is string valueString)
                {
                    content.Add(new StringContent(valueString), property.Name);
                }
                else if (value is IFormFile file)
                {
                    AddFormFileContent(content, file, property.Name);
                }
                else if (value is byte[] fileBytes)
                {
                    if (fileBytes.Length > 0)
                    {
                        var fileContent = new ByteArrayContent(fileBytes);
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        content.Add(fileContent, property.Name, "upload.bin");
                    }
                }
                else if (value is Stream fileStream)
                {
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    content.Add(fileContent, property.Name, "upload.bin");
                }
                else if (value != null)
                {
                    content.Add(new StringContent(value.ToString()), property.Name);
                }
            }

            return content;
        }

        private static void AddFormFileContent(MultipartFormDataContent content, IFormFile formFile, string fieldName)
        {
            if (formFile.Length > 0)
            {
                var fileContent = new StreamContent(formFile.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(formFile.ContentType);

                // Usar el nombre real del archivo
                content.Add(fileContent, fieldName, formFile.FileName);
            }
        }

        public static FormUrlEncodedContent ToFormUrlEncodedContent(this object obj, bool onlyStrings, bool urlEncode)
        {
            var content = new List<KeyValuePair<string, string>>();
            foreach (var property in obj.GetType().GetProperties())
            {
                var value = property.GetValue(obj);
                if (onlyStrings && value is string || !onlyStrings)
                    content.Add(new KeyValuePair<string, string>(property.Name, urlEncode ? HttpUtility.UrlEncode(value.ToString()) : value.ToString()));
            }
            return new FormUrlEncodedContent(content);
        }
    }
}