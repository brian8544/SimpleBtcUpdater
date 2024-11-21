using System.Runtime.InteropServices;

class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static System.Timers.Timer timer;

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_MINIMIZE = 6;

    static async Task Main(string[] args)
    {
        DisplayWelcomeMessage();

        // Required to show '€', otherwise shown as '?'.
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Set up a timer to update the title every minute, which is the minimum interval supported by Coinbase.
        // Most cryptocurrency platforms update their data every minute, so this frequency is appropriate.
        timer = new System.Timers.Timer(60000);
        timer.Elapsed += async (sender, e) => await UpdateTitleWithBitcoinPrice();
        timer.Start();

        await UpdateTitleWithBitcoinPrice();

        // Keep the console alive.
        Console.ReadLine();
    }

    private static void DisplayWelcomeMessage()
    {
        Console.WriteLine("This console will update the value of Bitcoin in Euros in your taskbar.");
        Console.WriteLine("Keep this console open in your taskbar and it will update every minute.\n");
        Console.WriteLine("Not seeing Application titles in your taskbar? See: \nhttps://www.majorgeeks.com/content/page/how_to_show_app_names_in_taskbar_icons.html\n");

        Console.Write("Are you done monitoring? Press "); Console.ForegroundColor = ConsoleColor.Green; Console.Write("CTRL-C"); Console.ResetColor(); Console.WriteLine(" to exit this application.\n");

        Task.Delay(5000).Wait();

        // Minimize the console window after 5 seconds.
        MinimizeConsole();
    }

    private static void MinimizeConsole()
    {
        IntPtr hwnd = FindWindow(null, Console.Title);

        if (hwnd != IntPtr.Zero)
        {
            ShowWindow(hwnd, SW_MINIMIZE);
        }
    }

    private static async Task UpdateTitleWithBitcoinPrice()
    {
        string currentDateTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        try
        {
            string apiUrl = "https://min-api.cryptocompare.com/data/generateAvg?fsym=BTC&tsym=EUR&e=coinbase";
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);


                if (data != null && data.RAW != null)
                {
                    string price = data.RAW.PRICE.ToString();
                    Console.Title = $"BTC: €{price}";

                    Console.WriteLine($"[{currentDateTime}] | Bitcoin Price: €{price}.");
                }
                else
                {
                    Console.WriteLine($"U[{currentDateTime}] | Failed to fetch data. Status code: {response.StatusCode}.");
                }
            }
            else
            {
                Console.WriteLine($"[{currentDateTime}] | Error, status code: {response.StatusCode}.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"@[{currentDateTime}] | Error: {ex.Message}.");
        }
    }
}
