using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ResultManager.Processor.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TranscriptGeneration.Models.Entities;

namespace ResultManager.Processor.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    var configBuilder = new ConfigurationBuilder().AddJsonFile("appsetting.json").Build();
        //    var configsession = configBuilder.GetSection("AppSettings");

        //    var connectionstring = configsession.GetSection("connectionstring").Value.Trim();


        //    optionsBuilder.UseSqlServer(connectionstring);
        //}

        public DbSet<StudentResultDetails> StudentResultDetail { get; set; }
        //public DbSet<StudentResult> StudentResults { get; set; }
       // public DbSet<ResultProcessingTransactionLog> ResultProcessingTransactionLog { get; set;}
        public DbSet<StudentResultCummulative> StudentResultCummulative { get; set; }
        
        public DbSet<StudentResultFinalSummary> StudentResultFinalSummary { get; set; }
        
        public DbSet<StudentResultSessionSummary> StudentResultSessionSummary { get; set; }
        public DbSet<StudentResultRawDetail> StudentResultRawDetail { get; set; }

        public DbSet<StudentResultSemesterSummary> StudentResultSemesterSummary { get; set; }
        public DbSet<StudentInfo> StudentInfo { get; set; }
        public DbSet<Sessions> Sessions { get; set; }

        // Add Faculties, and Programs

    }
}
