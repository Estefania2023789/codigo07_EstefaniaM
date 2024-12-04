using System.Linq;
using System;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text.RegularExpressions;



namespace Protocolo
{
    // Clase Protocolo
    public class Protocolo
    {
        // Variables
        private static Dictionary<string, int> listadoClientes = new Dictionary<string, int>();
        private NetworkStream flujo;
        private TcpClient remoto;

        // Metodo para crear un pedido combinando comando y parámetros
        public Pedido CrearPedido(string comando, string[] parametros)
        {
            // Combina el comando y los parámetros en un solo string
            string mensaje = comando + " " + string.Join(" ", parametros);
            return Pedido.Procesar(mensaje);
        }

        // Metodo para crear un pedido a partir de un mensaje
        public Pedido CrearPedido1(string mensaje)
        {
            return Pedido.Procesar(mensaje);
        }

        // Metodo para crear una respuesta
        public Respuesta CrearRespuesta(string estado, string mensaje)
        {
            return new Respuesta
            {
                Estado = estado,
                Mensaje = mensaje
            };
        }

        public string FormatearRespuesta(Respuesta respuesta)
        {
            return respuesta.ToString();
        }

        public string FormatearPedido(Pedido pedido)
        {
            return pedido.ToString();
        }

        
        // Metodo estático que resuelve un pedido dependiendo de su comando
        public static Respuesta ResolverPedido(Pedido pedido, string direccionCliente)
        {
            Respuesta respuesta = new Respuesta
            { Estado = "NOK", Mensaje = "Comando no reconocido" };

            switch (pedido.Comando)
            {
                case "INGRESO":
                    if (pedido.Parametros.Length == 2 && pedido.Parametros[0] == "root" && pedido.Parametros[1] == "admin20")
                    {
                        respuesta = new Random().Next(2) == 0
                            ? new Respuesta
                            {
                                Estado = "OK",
                                Mensaje = "ACCESO_CONCEDIDO"
                            }
                            : new Respuesta
                            {
                                Estado = "NOK",
                                Mensaje = "ACCESO_NEGADO"
                            };
                    }
                    else
                    {
                        respuesta.Mensaje = "ACCESO_NEGADO";
                    }
                    break;

                case "CALCULO":
                    if (pedido.Parametros.Length == 3)
                    {
                        string modelo = pedido.Parametros[0];
                        string marca = pedido.Parametros[1];
                        string placa = pedido.Parametros[2];
                        if (ValidarPlaca(placa))
                        {
                            byte indicadorDia = ObtenerIndicadorDia(placa);
                            respuesta = new Respuesta
                            {
                                Estado = "OK",
                                Mensaje = $"{placa} {indicadorDia}"
                            };
                            ContadorCliente(direccionCliente);
                        }
                        else
                        {
                            respuesta.Mensaje = "Placa no válida";
                        }
                    }
                    break;

                case "CONTADOR":
                    // Si el cliente ya tiene solicitudes anterriores se responde con el número de solicitudes
                    if (listadoClientes.ContainsKey(direccionCliente))
                    {
                        respuesta = new Respuesta
                        {
                            Estado = "OK",
                            Mensaje = listadoClientes[direccionCliente].ToString()
                        };
                    }
                    else
                    {
                        respuesta.Mensaje = "No hay solicitudes previas";
                    }
                    break;
            }

            return respuesta;
        }

        // Metodo para realizar una operación sobre un pedido
        public Respuesta HazOperacion(Pedido pedido)
        {
            // Verifica si la conexión esta activa
            if (flujo == null || remoto == null)
            {
                // Si flujo o remoto son nulos, intenta establecer la conexión
                Console.WriteLine("No hay conexión");
                return null;
            }

            try
            {
                // Si no hay conexión activa, establece una nueva conexión al servidor TCP
                if (remoto == null)
                {
                    remoto = new TcpClient("127.0.0.1", 8080); 
                }

                // Configurar el flujo de red (NetworkStream)
                flujo = remoto.GetStream();
                // Convierte el pedido a un arreglo de bytes
                byte[] bufferTx = Encoding.UTF8.GetBytes(pedido.Comando + " " + string.Join(" ", pedido.Parametros));
                // Envía el pedido al servidor
                flujo.Write(bufferTx, 0, bufferTx.Length);
                // Prepara un buffer para la respuesta del servidor
                byte[] bufferRx = new byte[1024];
                // Lee la respuesta del servidor
                int bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length);
                // Convierte la respuesta en un string
                string mensaje = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);
                // Divide la respuesta en partes
                var partes = mensaje.Split(' ');

                // Retorna una respuesta con el estado y mensaje recibido
                return new Respuesta
                {
                    Estado = partes[0],
                    Mensaje = string.Join(" ", partes.Skip(1).ToArray())
                };
            }
            catch (SocketException ex)
            {
                // MessageBox.Show("Error al intentar transmitir " + ex.Message, "ERROR");
                Console.WriteLine("Error al intentar transmitir " + ex.Message, "ERROR");

            }
            finally
            {
                // Cierra el flujo y la conexión, si están abiertas
                flujo?.Close();
                remoto?.Close();
            }
            return null;
        }

        public static bool ValidarPlaca(string placa)
        {
            return Regex.IsMatch(placa, @"^[A-Z]{3}[0-9]{4}$");
        }

        // Metodo estático que obtiene un indicador del día de la semana basado en la placa
        public static byte ObtenerIndicadorDia(string placa)
        {
            int ultimoDigito = int.Parse(placa.Substring(6, 1));
            switch (ultimoDigito)
            {
                case 1:
                case 2:
                    return 0b00100000; // Lunes
                case 3:
                case 4:
                    return 0b00010000; // Martes
                case 5:
                case 6:
                    return 0b00001000; // Miércoles
                case 7:
                case 8:
                    return 0b00000100; // Jueves
                case 9:
                case 0:
                    return 0b00000010; // Viernes
                default:
                    return 0;
            }
        }

        // Metodo estático para contar las solicitudes de un cliente específico
        public static void ContadorCliente(string direccionCliente)
        {
            if (listadoClientes.ContainsKey(direccionCliente))
            {
                listadoClientes[direccionCliente]++;
            }
            else
            {
                listadoClientes[direccionCliente] = 1;
            }
        }



    }

    // Clase de PEDIDO y RESPUESTA
    public class Pedido
    {
        public string Comando { get; set; }
        public string[] Parametros { get; set; }

        public static Pedido Procesar(string mensaje)
        {
            var partes = mensaje.Split(' ');
            return new Pedido
            {
                Comando = partes[0].ToUpper(),
                Parametros = partes.Skip(1).ToArray()
            };
        }

        public override string ToString()
        {
            return $"{Comando} {string.Join(" ", Parametros)}";
        }
    }

    public class Respuesta
    {
        public string Estado { get; set; }
        public string Mensaje { get; set; }

        public override string ToString()
        {
            return $"{Estado} {Mensaje}";
        }
    }

    


}