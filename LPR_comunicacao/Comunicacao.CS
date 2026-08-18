using System;
using System.IO.Ports;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    private static readonly HttpClient client = new HttpClient();
    
   
    private static readonly string SERVER_URL = "http://127.0.0.1:8000/medicao";

    private static int countLeituras = 0;
    private static double somaLeituras = 0;

    static async Task Main(string[] args)
    {
        SerialPort portaSerial = new SerialPort
        {
            PortName = "COM3", // Ajuste para a sua porta COM do STM32
            BaudRate = 115200,
            ReadTimeout = 2000
        };

        try
        {
            portaSerial.Open();
            Console.WriteLine($"Conectado com sucesso na porta {portaSerial.PortName}!");
            Console.WriteLine("Aguardando pacotes da STM32...");

            while (true)
            {
                // Lê a linha enviada via USB do STM32
                string pacoteBruto = portaSerial.ReadLine().Trim();

                if (ValidarEExtrairProtocolo(pacoteBruto, out double valorAdc))
                {
                    somaLeituras += valorAdc;
                    countLeituras++;

                    Console.WriteLine($"[Protocolo OK] Leitura [{countLeituras}/3]: {valorAdc}");

                    if (countLeituras == 3)
                    {
                        double mediaAdc = somaLeituras / 3.0;
                        Console.WriteLine($"---> Média das 3 leituras: {mediaAdc:F2}");

                        // Envia a média para o servidor Flask
                        await EnviarParaServidorAsync(mediaAdc);

                        countLeituras = 0;
                        somaLeituras = 0;
                    }
                }
                else
                {
                    Console.WriteLine($"[Protocolo Inválido]: Pacote descartado -> {pacoteBruto}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro na comunicação serial: {ex.Message}");
        }
        finally
        {
            if (portaSerial.IsOpen)
                portaSerial.Close();
        }
    }

    private static bool ValidarEExtrairProtocolo(string pacote, out double valorExtracao)
    {
        valorExtracao = 0;
        string inicio = "('v')";
        string fim = "Bill#";

        if (pacote.StartsWith(inicio) && pacote.EndsWith(fim))
        {
            int inicioIndex = inicio.Length;
            int tamanhoConteudo = pacote.Length - inicio.Length - fim.Length;

            string conteudoValor = pacote.Substring(inicioIndex, tamanhoConteudo);

            if (double.TryParse(conteudoValor, out valorExtracao))
            {
                return true;
            }
        }

        return false;
    }

    private static async Task EnviarParaServidorAsync(double mediaVal)
    {
        try
        {
           
            var dados = new { valor_adc = (int)Math.Round(mediaVal) };
            string jsonBody = JsonSerializer.Serialize(dados);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(SERVER_URL, content);

            if (response.IsSuccessStatusCode)
            {
                string respostaServidor = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Servidor Respondeu]: {respostaServidor}");
            }
            else
            {
                Console.WriteLine($"[Erro HTTP]: Status {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Erro ao enviar HTTP]: {ex.Message}");
        }
    }
}