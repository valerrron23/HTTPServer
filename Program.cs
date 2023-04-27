using System.Net;
using System.Text;

internal class Program
{
    struct Param
    {
        public string key { get; set; }

        public string value { get; set; }

        public Param(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }

    private static void Main(string[] args)
    {
        HttpListener server = new HttpListener();
        // установка адресов прослушки
        server.Prefixes.Add("http://127.0.0.1:8888/connection/");
        server.Start(); // начинаем прослушивать входящие подключения

        while (true)
        {

            // получаем контекст
            var context = server.GetContextAsync();

            var response = context.Result.Response;

            var request = context.Result.Request;

            string url = request.RawUrl;

            List<Param> httpParams = new List<Param>();

            // Console.WriteLine($"адрес приложения: {request.LocalEndPoint}");
            // Console.WriteLine($"адрес клиента: {request.RemoteEndPoint}");
            Console.WriteLine(request.RawUrl);

            if (url.Contains("?"))
            {
                String tmpUrl = url.Remove(0, 1);

                String[] tmpStr = tmpUrl.Split("&");

                foreach (var str in tmpStr)
                {
                    string[] tmp = str.Split("=");
                    httpParams.Add(new Param(key: tmp[0], value: tmp[1]));
                }

                url = url.Remove(url.IndexOf('?'));

                httpParams.ForEach(item =>
                {
                    Console.WriteLine(item.key + ":" + item.value);
                });
            }

            Console.WriteLine(url);
            // Console.WriteLine($"Запрошен адрес: {request.Url}");
            // Console.WriteLine("Заголовки запроса:");

            /* foreach (string item in response.Headers.Keys)
             {
                 Console.WriteLine($"{item}:{response.Headers[item]}");
             }*/

            // отправляемый в ответ код htmlвозвращает
            string responseText = "";

            string path = @"l:\\db\404.html";

            if (url == "/connection/")
                path = @"l:\\db\index.html";

            if (url == "/connection/main")
                path = @"l:\\db\main.html";

            if (url == "/connection/about")
                path = @"l:\\db\about.html";

            using (StreamReader sr = new StreamReader(path))
            {
                responseText = sr.ReadToEnd();

                if (responseText.Contains("{{age}}"))
                {
                    string age = (httpParams.Find(item => item.key == "age")).value;
                    responseText = responseText.Replace("{{age}}", age);
                }

                if (responseText.Contains("{{email}}"))
                {
                    string email = (httpParams.Find(item => item.key == "email")).value;
                    responseText = responseText.Replace("{{email}}", email);
                }
            }

            byte[] buffer = Encoding.UTF8.GetBytes(responseText);
            // получаем поток ответа и пишем в него ответ
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            // отправляем данные
            output.WriteAsync(buffer, 0, buffer.Length);
            output.FlushAsync();
            output.Close();

            Console.WriteLine("Запрос обработан");
        }

        server.Stop();
    }
}