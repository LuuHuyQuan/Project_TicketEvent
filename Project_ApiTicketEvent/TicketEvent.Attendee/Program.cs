using Data;
using Repositories.Implementations;
using Repositories.Interfaces;
using Services.Implementations;
using Services.Interfaces;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container

builder.Services.AddControllers();
builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();

builder.Services.AddScoped<IDanhMucSuKienRepository, DanhMucSuKienRepository>();
builder.Services.AddScoped<IDanhMucSuKienService, DanhMucSuKienService>();
builder.Services.AddScoped<IDiaDiemReponsitory, DiaDiemReponsitory>();
builder.Services.AddScoped<IDiaDiemService, DiaDiemService>();
builder.Services.AddScoped<ISuKienRepository, SuKienRepository>();
builder.Services.AddScoped<IDonHangRepository, DonHangRepository>();
builder.Services.AddScoped<ILoaiVeRepository, LoaiVeRepository>();
builder.Services.AddScoped<IThanhToanRepository, ThanhToanRepository>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
