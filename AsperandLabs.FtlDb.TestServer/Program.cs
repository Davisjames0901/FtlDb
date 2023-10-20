using System.Reflection;
using AsperandLabs.FtlDb.Engine;
using AsperandLabs.FtlDb.Engine.CodeGeneration;
using AsperandLabs.FtlDb.Engine.Config;
using AsperandLabs.FtlDb.Engine.Indexers;
using AsperandLabs.FtlDb.Engine.Row;
using AsperandLabs.FtlDb.Engine.Services;
using AsperandLabs.FtlDb.Engine.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(new TableConfig
{
    BaseDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DbFiles")
});
builder.Services.AddSingleton<RowClassGenerator>();
builder.Services.AddSingleton<TableStructureValidator>();
builder.Services.AddSingleton<TableService>();
builder.Services.AddSingleton<IndexerFactory>();
builder.Services.AddSingleton<DataService>();
builder.Services.AddTransient<Mount>();

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