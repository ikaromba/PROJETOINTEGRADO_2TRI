using System;
using System.IO.Ports;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    // Cliente HTTP estático para realizar as requisições para o servidor
    private static readonly HttpClient client = new HttpClient();

    // URL do seu servidor (Node.js ou Python/Flask)
    private static readonly string SERVER_URL = "http://localhost:3000/predict";

    // Buffer para guardar 3 leituras antes de enviar
    private static int countLeituras = 0;
    private static double somaLeituras = 0;

    static void Main(string[] args)
    {
        // i. Estabelecer comunicação com a Porta COM
        SerialPort portaSerial = new SerialPort();
        
        // AJUSTE AQUI: Altere para a porta que a sua STM32 estiver conectada (ex: COM3, COM4, COM5)
        portaSerial.PortName = "COM3"; 
        portaSerial.BaudRate = 115200; // Mesma velocidade configurada na STM32

        try
        {
            portaSerial.Open();
            Console.WriteLine($"Conectado com sucesso na porta {portaSerial.PortName}!");
            Console.WriteLine("Aguardando dados da STM32...");

            // ii. Receber continuamente as leituras provenientes do STM32
            while (true)
            {
                // Lê uma linha de texto enviada pela STM32 (ex: "4095")
                string linhaLida = portaSerial.ReadLine().Trim();

                // iii. Organizar e tratar as informações recebidas
                if (double.TryParse(linhaLida, out double valorAdc))
                {
                    somaLeituras += valorAdc;
                    countLeituras++;

                    Console.WriteLine($"Leitura [{countLeituras}/3]: {valorAdc}");

                    // Quando acumula 3 leituras, calcula a média
                    if (countLeituras == 3)
                    {
                        double mediaAdc = somaLeituras / 3.0;
                        Console.WriteLine($"---> Média calculada das 3 leituras: {mediaAdc:F2}");

                        // Dispara o envio de forma assíncrona
                        _ = EnviarParaServidorAsync(mediaAdc);

                        // Reseta o contador e a soma para o próximo ciclo
                        countLeituras = 0;
                        somaLeituras = 0;
                    }
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

    // iv. Converter os dados para JSON  &  v. Realizar requisições HTTP
    private static async Task EnviarParaServidorAsync(double mediaVal)
    {
        try
        {
            // Criando o objeto C#
            var dados = new 
            { 
                media_adc = mediaVal 
            };

            // Conversão para JSON
            string jsonBody = JsonSerializer.Serialize(dados);

            // Preparando o conteúdo da requisição HTTP POST
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // Realizando a requisição HTTP POST para o servidor
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