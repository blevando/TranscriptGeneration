
using Microsoft.EntityFrameworkCore;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
 
using PdfSharpCore.Fonts;
using PdfSharpCore.Utils;
using ResultManager.Processor.Data;
using SixLabors.ImageSharp.PixelFormats;
 
using TranscriptGeneration.Mangers;
using TranscriptGeneration.Services.Interface;
using TranscriptGeneration.Services.Repositories;



namespace TranscriptGeneration
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Register the image source implementation for MigraDocCore
            ImageSource.ImageSourceImpl = new ImageSharpImageSource<Rgba32>();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // Inside Main or before building the app
            //GlobalFontSettings.FontResolver = (IFontResolver)CustomFontResolver.Instance;

            var configuration = builder.Configuration;
            var connectionstrings = builder.Configuration.GetConnectionString("connectionstring"); 

            builder.Services.AddDbContext<ApplicationDbContext>(a => a.UseSqlServer(connectionstrings));



            // ✅ CORS Policy: Allow both React (5173) and CRA/Next (3000)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://72.55.189.248:6007", "http://72.55.189.248:6008")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); // For JWT or cookies
                });
            });

            //builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("connectionstring")));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<TranscriptManager>();
            builder.Services.AddScoped<ITranscriptReport, TranscriptReportRepository>();

         //   MigraDoc.DocumentObjectModel.IO.ImageSource.ImageSourceImpl = new MigraDoc.Rendering.ImageSharp.ImageSharpImageSource();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseCors("AllowLocalFrontend"); // ✅ use the updated CORS policy here

            app.UseDeveloperExceptionPage(); // only for debugging


            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
