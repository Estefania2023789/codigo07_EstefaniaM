using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Protocolo;

namespace Cliente
{
    public partial class FrmValidador : Form
    {
        // Declaración de la variable para manejar la conexión TCP
        private TcpClient remoto;
        // Declara una variable para manejar el flujo de datos
        private NetworkStream flujo;
        // Instancia la clase Protocolo
        private Protocolo.Protocolo protocolo;

        public FrmValidador()
        {
            InitializeComponent();
            // Creacion de instancia de la clase Protocolo
            protocolo = new Protocolo.Protocolo();
        }

        private void FrmValidador_Load(object sender, EventArgs e)
        {
            try
            {
                remoto = new TcpClient("127.0.0.1", 8080);
                flujo = remoto.GetStream();
                MessageBox.Show("Conexión establecida con el servidor.", "Éxito");
            }
            catch (SocketException ex)
            {
                MessageBox.Show("No se puedo establecer conexión " + ex.Message,"ERROR");
            }
            finally
            {
                flujo?.Close();
                remoto?.Close();
            }
            
            // Desactiva los controles relacionados con la validación de placas
            panPlaca.Enabled = false;
            chkLunes.Enabled = false;
            chkMartes.Enabled = false;
            chkMiercoles.Enabled = false;
            chkJueves.Enabled = false;
            chkViernes.Enabled = false;
            chkDomingo.Enabled = false;
            chkSabado.Enabled = false;
        }

        private void btnIniciar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text;
            string contraseña = txtPassword.Text;
            Protocolo.Protocolo protocolo = new Protocolo.Protocolo();

            if (usuario == "" || contraseña == "")
            {
                MessageBox.Show("Se requiere el ingreso de usuario y contraseña","ADVERTENCIA");
                return;
            }
           
            // Crea un pedido con el comando respectivo
            Pedido pedido = protocolo.CrearPedido("INGRESO", new[] { usuario, contraseña });
            

            /*
            Pedido pedido = new Pedido
            {
                Comando = "INGRESO",
                Parametros = new[] { usuario, contraseña }
            };
            */


            // Se usa obtener la respuesta
            // Respuesta respuesta = Protocolo.Protocolo.HazOperacion(pedido);
            Respuesta respuesta = protocolo.HazOperacion(pedido);

            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            /*
            Respuesta respuesta = HazOperacion(pedido);
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }
            */

            // Verifica la respuesta del servidor. Si el acceso es concedido, habilita la siguiente sección de la interfaz
            if (respuesta.Estado == "OK" && respuesta.Mensaje == "ACCESO_CONCEDIDO")
            {
                panPlaca.Enabled = true;
                panLogin.Enabled = false;
                MessageBox.Show("Acceso concedido", "INFORMACIÓN");
                txtModelo.Focus();
            }
            else if (respuesta.Estado == "NOK" && respuesta.Mensaje == "ACCESO_NEGADO")
            {
                panPlaca.Enabled = false;
                panLogin.Enabled = true;
                MessageBox.Show("No se pudo ingresar, revise credenciales","ERROR");
                txtUsuario.Focus();
            }
        }




        /*
         
        private Respuesta HazOperacion(Pedido pedido)
        {
            if (flujo == null)
            {
                MessageBox.Show("No hay conexión", "ERROR");
                return null;
            }
            try
            {
                byte[] bufferTx = Encoding.UTF8.GetBytes(
                    pedido.Comando + " " + string.Join(" ", pedido.Parametros));

                flujo.Write(bufferTx, 0, bufferTx.Length);

                byte[] bufferRx = new byte[1024];

                int bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length);

                string mensaje = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);

                var partes = mensaje.Split(' ');

                return new Respuesta
                {
                    Estado = partes[0],
                    Mensaje = string.Join(" ", partes.Skip(1).ToArray())
                };
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Error al intentar transmitir " + ex.Message,
                    "ERROR");
            }
            finally
            {
                flujo?.Close();
                remoto?.Close();
            }
            return null;
        }

        */ 

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            string modelo = txtModelo.Text;
            string marca = txtMarca.Text;
            string placa = txtPlaca.Text;

            // Crea un pedido
            Pedido pedido = protocolo.CrearPedido("CALCULO", new[] { modelo, marca, placa });

            /*
            Pedido pedido = new Pedido
            {
                Comando = "CALCULO",
                Parametros = new[] { modelo, marca, placa }
            };
            */
            // Se realiza la operación y se obtiene la respuesta del servidor
            Respuesta respuesta = protocolo.HazOperacion(pedido);
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            if (respuesta.Estado == "NOK")
            {
                MessageBox.Show("Error en la solicitud.", "ERROR");
                chkLunes.Checked = false;
                chkMartes.Checked = false;
                chkMiercoles.Checked = false;
                chkJueves.Checked = false;
                chkViernes.Checked = false;
            }
            else
            {
                var partes = respuesta.Mensaje.Split(' ');
                MessageBox.Show("Se recibió: " + respuesta.Mensaje,"INFORMACIÓN");
                byte resultado = Byte.Parse(partes[1]);
                switch (resultado)
                {
                    case 0b00100000:
                        chkLunes.Checked = true;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 0b00010000:
                        chkMartes.Checked = true;
                        chkLunes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 0b00001000:
                        chkMiercoles.Checked = true;
                        chkLunes.Checked = false;
                        chkMartes.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 0b00000100:
                        chkJueves.Checked = true;
                        chkLunes.Checked = false;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 0b00000010:
                        chkViernes.Checked = true;
                        chkLunes.Checked = false;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        break;
                    default:
                        chkLunes.Checked = false;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                }
            }
        }

        private void btnNumConsultas_Click(object sender, EventArgs e)
        {
            String mensaje = "hola";

            Pedido pedido = protocolo.CrearPedido("CONTADOR", new[] { mensaje });
            /*
            Pedido pedido = new Pedido
            {
                Comando = "CONTADOR",
                Parametros = new[] { mensaje }
            };
            */

            Respuesta respuesta = protocolo.HazOperacion(pedido);
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            if (respuesta.Estado == "NOK")
            {
                MessageBox.Show("Error en la solicitud.", "ERROR");

            }
            else
            {
                var partes = respuesta.Mensaje.Split(' ');
                MessageBox.Show("El número de pedidos recibidos en este cliente es " + partes[0],"INFORMACIÓN");
            }
        }

        private void FrmValidador_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (flujo != null)
                flujo.Close();
            if (remoto != null)
                if (remoto.Connected)
                    remoto.Close();
        }
    }
}