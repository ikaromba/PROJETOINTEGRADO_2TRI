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

    // URL do servidor Python/Flask
    private static readonly string SERVER_URL = "http://localhost:5000/predict";

    // Buffer para guardar 3 leituras antes de enviar
    private static int countLeituras = 0;
    private static double somaLeituras = 0;

    static void Main(string[] args)
    {
        // i. Estabelecer comunicação com a Porta COM
        SerialPort portaSerial = new SerialPort();
        
        portaSerial.PortName = "COM3"; // Ajuste para a porta COM conectada à STM32
        portaSerial.BaudRate = 115200;

        try
        {
            portaSerial.Open();
            Console.WriteLine($"Conectado com sucesso na porta {portaSerial.PortName}!");
            Console.WriteLine("Aguardando pacotes da STM32 (Protocolo Proprietário)...");

            // ii. Receber continuamente as leituras provenientes do STM32
            while (true)
            {
                // Lê uma linha vinda da Serial (Exemplo esperado: "$ADC;3850#")
                string pacoteBruto = portaSerial.ReadLine().Trim();

                // iii. Organizar e tratar as informações recebidas via Protocolo Proprietário
                if (ValidarEExtrairProtocolo(pacoteBruto, out double valorAdc))
                {
                    somaLeituras += valorAdc;
                    countLeituras++;

                    Console.WriteLine($"[Protocolo OK] Leitura [{countLeituras}/3]: {valorAdc}");

                    // Quando acumula 3 leituras, calcula a média
                    if (countLeituras == 3)
                    {
                        double mediaAdc = somaLeituras / 3.0;
                        Console.WriteLine($"---> Média das 3 leituras: {mediaAdc:F2}");

                        // Dispara o envio de forma assíncrona
                        _ = EnviarParaServidorAsync(mediaAdc);

                        // Reseta o contador e a soma
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

    /// <summary>
    /// Valida se o pacote recebido atende ao protocolo proprietário ($ADC;VALOR#)
    /// e extrai o valor numérico.
    /// </summary>
    private static bool ValidarEExtrairProtocolo(string pacote, out double valorExtracao)
    {
        valorExtracao = 0;

        // 1. Verifica se inicia com '$ADC;' e termina com '#'
        if (pacote.StartsWith("('v')") && pacote.EndsWith("Bill#"))
        {
            
            string conteudoValor = pacote.Replace("('v')", "").Replace("Bill#", "");

            
            if (double.TryParse(conteudoValor, out valorExtracao))
            {
                return true; // Pacote válido e convertido com sucesso!
            }
        }

        return false; // Pacote fora do padrão do protocolo
    }

    // iv. Converter os dados para JSON  &  v. Realizar requisições HTTP
    private static async Task EnviarParaServidorAsync(double mediaVal)
    {
        try
        {
            var dados = new 
            { 
                media_adc = mediaVal 
            };

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