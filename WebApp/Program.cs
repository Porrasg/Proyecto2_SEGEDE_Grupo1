var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configurar la canalización de solicitudes HTTP (pipeline).
//if (!app.Environment.IsDevelopment())
//{
app.UseExceptionHandler("/Error");
// El valor predeterminado de HSTS es de 30 días. Es posible que desees cambiar esto para escenarios de producción, consulta https://aka.ms/aspnetcore-hsts.
app.UseHsts();
//}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
