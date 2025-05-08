using System;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

namespace Client_StudioMedico
{
    internal class ClientMedico
    {
        private const string ServerIp = "127.0.0.1";
        private const int Port = 12345;
        private const string ServerName = "c_stumed.local";

        static void Main(string[] args)
        {
            TcpClient client = new TcpClient(ServerIp, Port);
            using SslStream stream = new SslStream(client.GetStream(),
            false,
            new RemoteCertificateValidationCallback(ValidateServerCertificate),
            null
               );

            Console.WriteLine("Inserisci il tuo ID medico:");
                string medicoId = Console.ReadLine();
                Console.WriteLine("inserisci la tua password:");
                string password = Console.ReadLine();
            stream.AuthenticateAsClient(ServerName, null, SslProtocols.Tls12 |SslProtocols.Tls13, true);
            Console.WriteLine($"Connessione sicura stabilita:{ stream.IsAuthenticated}");
            while (true)
                {
                    string request = $"0|{medicoId}|{password}";
                    byte[] requestBytes = Encoding.UTF8.GetBytes(request);
                    stream.Write(requestBytes, 0, requestBytes.Length);

                    byte[] buffer = new byte[2048];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    if (response == "OK")
                    {
                        break;
                    }
                    Console.WriteLine("ID o password errati. Riprova.");
                    Console.WriteLine("Inserisci il tuo ID medico:");
                    medicoId = Console.ReadLine();
                    Console.WriteLine("inserisci la tua password:");
                    password = Console.ReadLine();
                }

            while (true)
            {
                Console.WriteLine("Scegli un'opzione:");
                Console.WriteLine("1. Visualizza appuntamenti della giornata");
                Console.WriteLine("2. Mostra la storia clinica di un paziente");
                Console.WriteLine("3. Registra una visita");
                Console.WriteLine("4. Inserire un certificato di malattia");
                Console.WriteLine("5. Esci");
                string choice = Console.ReadLine();

                if (choice == "5")
                {
                    break;
                }

                string request = "";

                switch (choice)
                {
                    case "1":
                        request = $"1|{medicoId}";
                        break;
                    case "2":
                        Console.WriteLine("Inserisci il codice del paziente:");
                        string pazienteId = Console.ReadLine();
                        request = $"2|{pazienteId}";
                        break;
                    case "3":
                        Console.WriteLine("Inserisci codice paziente:");
                        string pId = Console.ReadLine();
                        Console.WriteLine("Data (YYYY-MM-DD):");
                        string data = Console.ReadLine();
                        Console.WriteLine("Ora (HH:MM):");
                        string ora = Console.ReadLine();
                        Console.WriteLine("Motivo della visita:");
                        string motivo = Console.ReadLine();
                        Console.WriteLine("Diagnosi:");
                        string diagnosi = Console.ReadLine();
                        Console.WriteLine("Prescrizioni:");
                        string prescrizioni = Console.ReadLine();
                        request = $"3|{data}|{ora}|{motivo}|{diagnosi}|{prescrizioni}|{medicoId}|{pId}";
                        break;
                    case "4":
                        Console.WriteLine("Inserisci codice paziente:");
                        string certPazienteId = Console.ReadLine();
                        Console.WriteLine("Data (YYYY-MM-DD):");
                        string certData = Console.ReadLine();
                        Console.WriteLine("Diagnosi:");
                        string certDiagnosi = Console.ReadLine();
                        Console.WriteLine("Numero di giorni di malattia:");
                        string giorni = Console.ReadLine();
                        request = $"4|{certData}|{certDiagnosi}|{giorni}|{medicoId}|{certPazienteId}";
                        break;
                    default:
                        Console.WriteLine("Scelta non valida.");
                        continue;
                }

                byte[] requestBytes = Encoding.UTF8.GetBytes(request);
                stream.Write(requestBytes, 0, requestBytes.Length);

                byte[] buffer = new byte[2048];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Risposta dal server: " + response);
            }

            client.Close();
        }
        static string CalcolaSHA256(string testo)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(testo);
                byte[] hashBytes = sha256.ComputeHash(bytes);

                // Converti l'hash in una stringa esadecimale
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                    sb.Append(b.ToString("x2"));

                return sb.ToString();
            }
        }
        public static bool ValidateServerCertificate(object sender,X509Certificate certificate,X509Chain chain,SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            Console.WriteLine($"Errore certificato: {sslPolicyErrors}");
            
            return true;
        }
    }
}
