using System.Net.Http.Json;
using System.Text;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using Microsoft.Maui.Storage;


namespace APIAsierSerrano
{
    public partial class MainPage : ContentPage
    {
        private List<Usuario> usuarios = new();

        public MainPage()
        {
            InitializeComponent();
        }

        private async void BtnConsulta_Clicked(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";

            try
            {
                using var client = new HttpClient();
                var url = "https://jsonplaceholder.typicode.com/users";

                var resultado = await client.GetAsync(url);

                if (!resultado.IsSuccessStatusCode)
                {
                    ErrorLabel.Text = "Error al obtener los datos.";
                    return;
                }

                usuarios = await resultado.Content.ReadFromJsonAsync<List<Usuario>>();

                UsuariosList.ItemsSource = usuarios;
            }
            catch
            {
                ErrorLabel.Text = "No se pudo realizar la consulta. Inténtalo más tarde.";
            }
        }

        private void BtnFiltrar_Clicked(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";

            if (usuarios.Count == 0)
            {
                ErrorLabel.Text = "Primero realiza la consulta.";
                return;
            }

            var letra = FiltroEntry.Text?.Trim().ToLower();

            if (string.IsNullOrEmpty(letra) || letra.Length != 1 || !char.IsLetter(letra[0]))
            {
                ErrorLabel.Text = "Introduce una única letra válida.";
                return;
            }

            var filtrados = usuarios
                .Where(u => u.name.ToLower().StartsWith(letra))
                .ToList();
   
            if (filtrados.Count == 0)
            {
                ErrorLabel.Text = "No hay usuarios que empiecen por esa letra.";
            }

            UsuariosList.ItemsSource = filtrados;
        }
        private async void BtnInforme_Clicked(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";

            if (UsuariosList.ItemsSource == null)
            {
                ErrorLabel.Text = "No hay datos para generar el informe.";
                return;
            }

            var lista = UsuariosList.ItemsSource.Cast<Usuario>().ToList();

            if (lista.Count == 0)
            {
                ErrorLabel.Text = "La lista está vacía.";
                return;
            }

            // Crear pdf
            PdfDocument doc = new PdfDocument();
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            var titleFont = new XFont("Arial", 20, XFontStyle.Bold);
            var textFont = new XFont("Arial", 12, XFontStyle.Regular);

            double y = 40;

            gfx.DrawString("INFORME DE USUARIOS", titleFont, XBrushes.Black, new XPoint(20, y));
            y += 40;

            gfx.DrawString($"Generado: {DateTime.Now}", textFont, XBrushes.Black, new XPoint(20, y));
            y += 20;

            gfx.DrawString($"Total de usuarios: {lista.Count}", textFont, XBrushes.Black, new XPoint(20, y));
            y += 30;

            foreach (var u in lista)
            {
                gfx.DrawString($"Nombre: {u.name}", textFont, XBrushes.Black, new XPoint(20, y));
                y += 20;

                gfx.DrawString($"Correo: {u.email}", textFont, XBrushes.Black, new XPoint(20, y));
                y += 30;

                // Nueva pag
                if (y > page.Height - 50)
                {
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }
            }

            // Guardar archivo
            var fileName = $"InformeUsuarios_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            using (var stream = File.Create(filePath))
            {
                doc.Save(stream);
            }

            await DisplayAlertAsync("Informe generado",
                $"El informe PDF se ha guardado en:\n{filePath}",
                "OK");
        }
}

public class Usuario
    {
        public string name { get; set; }
        public string email { get; set; }

    }
}
