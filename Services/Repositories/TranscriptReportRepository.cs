
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Writers;
using Microsoft.SqlServer.Server;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Tables;
using MigraDocCore.Rendering;

//using MigraDoc.DocumentObjectModel;
//using MigraDoc.DocumentObjectModel.Shapes;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Utils;
//using PdfSharp.Pdf;
using ResultManager.Processor.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces.Companding;

////using MigraDoc.Rendering;
//using PdfSharp.Drawing;
//using PdfSharp.Fonts;

using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using TranscriptGeneration.Models.Dto;
using TranscriptGeneration.Models.Entities;
using TranscriptGeneration.Services.Interface;
using static MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes.ImageSource;
using static System.Net.Mime.MediaTypeNames;
using Color = MigraDocCore.DocumentObjectModel.Color;
using Document = MigraDocCore.DocumentObjectModel.Document;
using Image = SixLabors.ImageSharp.Image;

namespace TranscriptGeneration.Services.Repositories
{
    public class TranscriptReportRepository : ITranscriptReport
    {
        private readonly ApplicationDbContext _context;
        public TranscriptReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<GeneralResponse> GetFinalGPACGPAsync(string studentId, int DepartmentId)
        {
            var cgpa = await _context.StudentResultFinalSummary.FirstOrDefaultAsync(sr => sr.MatricNumber == studentId);

            if (cgpa != null)
            {
                return new GeneralResponse { StatusCode = 200, Message = "GPA retrieved successfully.", Data = cgpa.CGPA };
            }
            else
            {
                return new GeneralResponse { StatusCode = 404, Message = "GPA not found for the specified student and semester." };
            }
        }

        public async Task<GeneralResponse> GetSemesterGPAsync(string studentId, int SessionId, int DepartmentId, int SemesterId)
        {
            var gpa = await _context.StudentResultSemesterSummary.FirstOrDefaultAsync(sr => sr.MatricNumber == studentId && sr.SessionId == SessionId && sr.DepartmentId == DepartmentId && sr.SemesterId == SemesterId);

            if (gpa != null)
            {
                return new GeneralResponse { StatusCode = 200, Message = "GPA retrieved successfully.", Data = gpa.GPA };
            }
            else
            {
                return new GeneralResponse { StatusCode = 404, Message = "GPA not found for the specified student and semester." };
            }
        }

        public async Task<GeneralResponse> GetSemesterResultAsync(string studentId, int SessionId, int DepartmentId, int SemesterId)
        {




            var lstOfResults = await _context.StudentResultRawDetail
                .Where(sr => sr.MatricNumber == studentId
                && sr.SessionId == SessionId &&
                sr.DepartmentId == DepartmentId
                && sr.SemesterId == SemesterId)
                .Select(sr => new
                {
                    sr.CourseCode,
                    sr.CourseTitle,
                    sr.Units,
                    sr.Grade,
                    sr.GradePoints,
                    sr.Score

                })
                .OrderBy(sr => sr.CourseCode)
                .ToListAsync();

            if (lstOfResults.Count > 0)
            {

                TranscriptModuleDto trans = new TranscriptModuleDto();


                switch (SemesterId)
                {
                    case 1:
                        trans.Title = "First Semester";
                        break;
                    case 2:
                        trans.Title = "Second Semester";
                        break;
                    case 3:
                        trans.Title = "Third Semester";
                        break;
                    case 4:
                        trans.Title = "Fourth Semester";
                        break;
                }

                var ssion = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionId == SessionId);
                if (ssion != null)
                {
                    trans.Title = $"{trans.Title} {ssion.SessionName} ";
                }
                else
                {
                    trans.Title = $"{trans.Title} - session: {SessionId} ";
                }

                trans.Header = new ResultRecord { SN = "SN", CourseCode = "CourseCode", CourseTitle = "CourseTitle", Unit = "Unit", Score = "Score", Grade = "Grade", GradePoint = "GradePoint" };
                trans.Body = new List<ResultRecord>();
                int sn = 1;
                List<string> list = new List<string>();
                foreach (var item in lstOfResults)
                {
                    ResultRecord resultRecord = new ResultRecord();
                    resultRecord.SN = sn.ToString("00");
                    resultRecord.CourseCode = item.CourseCode;
                    resultRecord.CourseTitle = item.CourseTitle.ToUpper();
                    resultRecord.Unit = item.Units.ToString();
                    resultRecord.Grade = item.Grade;
                    resultRecord.GradePoint = item.GradePoints.ToString();
                    resultRecord.Score = item.Score.ToString();
                    int i = 0;
                    foreach (var body in trans.Body)
                    {
                        if (body.CourseCode == resultRecord.CourseCode)
                        {
                            i++;
                        }


                    }

                    if (i == 0)
                    {
                        trans.Body.Add(resultRecord);
                    }



                    sn++;
                }
                var gpa = await _context.StudentResultSemesterSummary
                    .Where(sr => sr.MatricNumber == studentId && sr.SessionId == SessionId && sr.DepartmentId == DepartmentId && sr.SemesterId == SemesterId)
                    .Select(sr => sr.GPA)
                    .FirstOrDefaultAsync();
                if (gpa != null)
                {
                    trans.Footer = $"GPA: {gpa}";
                }



                return new GeneralResponse { StatusCode = 200, Message = $"Session {SessionId} and Semester {SemesterId} for Student {studentId} retrieved successfully.", Data = trans };


            }
            {
                return new GeneralResponse { StatusCode = 404, Message = $"No Result exists for the specified student in session {SessionId} and semester {SessionId}" };
            }
        }




        public async Task<GeneralResponse> GetSessionCGPAsync(string studentId, int SessionId, int DepartmentId)
        {
            var cgpa = await _context.StudentResultSessionSummary.FirstOrDefaultAsync(sr => sr.MatricNumber == studentId && sr.SessionId == SessionId && sr.DepartmentId == DepartmentId);

            if (cgpa != null)
            {
                return new GeneralResponse { StatusCode = 200, Message = "GPA retrieved successfully.", Data = cgpa.CGPA };
            }
            else
            {
                return new GeneralResponse { StatusCode = 404, Message = "GPA not found for the specified student and semester." };
            }
        }

        public async Task<GeneralResponse> GetTranscriptReportAsync(string studentId)
        {

            //BuildDocument("l");

            InstitutionInfo studentTransctript = new InstitutionInfo();
            var periodInformation = await _context.StudentResultSemesterSummary
               .Where(sr => sr.MatricNumber == studentId)
               .Select(sr => new
               {
                   sr.SessionId,
                   sr.DepartmentId,
                   sr.SemesterId,

               })
               .OrderBy(sr => sr.SessionId)
               .ThenBy(sr => sr.SemesterId)
               .ToListAsync();
            if (periodInformation != null)
            {
                if (periodInformation.Count > 0)
                {


                    studentTransctript.SerialNumber = GetOrderNumberByCustomerId(10);
                    studentTransctript.UniversityName = "James Hope University";
                    studentTransctript.UniversityAddress = "123 University Lane, Lagos, Nigeria";
                    studentTransctript.UniversityOffice = "Office of the Registrar";
                    studentTransctript.Signature = "Registrar Signature Placeholder";
                    studentTransctript.UniversityLogo = "https://example.com/logo.png"; // Placeholder for logo URL

                    studentTransctript.StudentInfo = await _context.StudentInfo.FirstOrDefaultAsync(s => s.MatricNumber == studentId);


                    studentTransctript.SemesterData = new List<TranscriptModuleDto>();
                    foreach (var period in periodInformation)
                    {
                        var result = await GetSemesterResultAsync(studentId, period.SessionId, period.DepartmentId, period.SemesterId);
                        if (result != null)
                        {
                            studentTransctript.SemesterData.Add((TranscriptModuleDto)result.Data);
                        }


                    }
                    var cgpa = await _context.StudentResultFinalSummary
                        .Where(sr => sr.MatricNumber == studentId)
                        .Select(sr => sr.CGPA)
                        .FirstOrDefaultAsync();
                    if (cgpa != null)
                    {
                        studentTransctript.FinalGPA = cgpa.ToString();

                        var byteFile = BuildDocument(studentTransctript);

                        if (byteFile == null || byteFile.Length == 0)
                        {

                            return new GeneralResponse { StatusCode = 500, Message = "Error generating transcript PDF." };
                        }
                        else
                        {
                            string base64 = Convert.ToBase64String(byteFile);
                            return new GeneralResponse { StatusCode = 200, Message = base64, Data = studentTransctript };
                        }
                        // return new GeneralResponse { StatusCode = 200, Message = "Transcript retrieved successfully.", Data = studentTransctript };
                    }
                    else
                    {
                        studentTransctript.FinalGPA = "N/A"; // or some default value

                    }

                }


            }

            return new GeneralResponse { StatusCode = 404, Message = "No results found for the specified student." };
        }
        public string GetOrderNumberByCustomerId(int n)
        {
            Guid guid = Guid.NewGuid();

            char[] ch = guid.ToString().ToCharArray();
            string output = string.Concat(ch.Where(Char.IsDigit));

            if (output.Length < n)
            {
                guid = Guid.NewGuid();
                ch = guid.ToString().ToCharArray();

                output += string.Concat(ch.Where(Char.IsDigit));
            }
            output = output.Substring(0, n);

            return output;

        }



        private byte[] BuildDocument(InstitutionInfo studentTranascript)
        {


            // Create a new PDF document




            // PdfSharp.Fonts.GlobalFontSettings.FontResolver =  CustomFontResolver.Instance;


            //MigraDocCore
            // Create the document
            var document = new Document();

            // Set default font
            document.Styles["Normal"].Font.Name = "Courier New";
            document.Styles["Normal"].Font.Size = 11;



            var section = document.AddSection();

            section.PageSetup.TopMargin = Unit.FromCentimeter(0.2);    // 1 cm
            section.PageSetup.BottomMargin = Unit.FromCentimeter(0.5); // 1 cm
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1.0);   // 1.5 cm
            section.PageSetup.RightMargin = Unit.FromCentimeter(0.5);  // 1.5 cm


            //  AddWatermark(section, "NOT OFFICIAL");
            // AddDiagonalWatermark(section, "NOT OFFICIAL");


            var imagePath = Path.Combine(Environment.CurrentDirectory, "jhu.png");
            var signaturePath = Path.Combine(Environment.CurrentDirectory, "registrar.png");


            // === Header Info ===
            var paragraph = section.AddParagraph();
            //paragraph.AddLineBreak();
            paragraph.AddLineBreak();
            paragraph.AddLineBreak();

            paragraph.Format.Font.Color = Color.Parse("0xFF8B0000");
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.Format.SpaceBefore = "0.5cm";
            paragraph.Format.SpaceAfter = "0.5cm";

            // University Name
            paragraph = section.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.Format.SpaceBefore = "2cm";
            paragraph.Format.SpaceAfter = 0;
            paragraph.Format.LineSpacing = Unit.FromPoint(10); // Optional: tighter line spacing
            paragraph.AddFormattedText(studentTranascript.UniversityName, TextFormat.Bold).Font.Size = 22;

            // University Office
            paragraph = section.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.Format.SpaceBefore = 0;
            paragraph.Format.SpaceAfter = 0;
            paragraph.Format.LineSpacing = Unit.FromPoint(10);
            var officeText = paragraph.AddFormattedText(studentTranascript.UniversityOffice, TextFormat.Bold);
            officeText.Font.Size = 12;
            officeText.Font.Italic = true;

            // Transcript Title
            paragraph = section.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.Format.SpaceBefore = 0;
            paragraph.Format.SpaceAfter = 0;
            paragraph.Format.LineSpacing = Unit.FromPoint(10);
            paragraph.AddFormattedText("Student Academic Transcript", TextFormat.Bold).Font.Size = 12;


            var spacer = section.AddParagraph();
            spacer.Format.SpaceAfter = "0.01cm";



            // Student Table Info

            // Create Student Info Table
            var studenttable = section.AddTable();

            // Center the table horizontally
            studenttable.Rows.LeftIndent = 0;
            studenttable.Format.Alignment = ParagraphAlignment.Center;
            studenttable.TopPadding = 2;
            studenttable.BottomPadding = 2;

            // Set border width
            studenttable.Borders.Width = 0.5;

            // Define columns
            studenttable.AddColumn("7.5cm");
            studenttable.AddColumn("11cm");

            // === Header Row ===
            var row = studenttable.AddRow();
            row.Format.Alignment = ParagraphAlignment.Left;
            row.HeadingFormat = true;

            row.Cells[0].AddParagraph("Academic Record of").Format.Font.Bold = true;
            row.Cells[1].AddParagraph($"{studentTranascript.StudentInfo.Surname} {studentTranascript.StudentInfo.OtherNames}");

            // Row 2
            row = studenttable.AddRow();
            row.Format.Alignment = ParagraphAlignment.Left;
            row.Cells[0].AddParagraph("Matriculation Number").Format.Font.Bold = true;
            row.Cells[1].AddParagraph(studentTranascript.StudentInfo.MatricNumber);
            row.Shading.Color = Color.Parse("0x20D3D3D3");

            var adSess = _context.Sessions.FirstOrDefaultAsync(s => s.SessionId.ToString() == studentTranascript.StudentInfo.AdmittedSessionId);
            if (adSess != null)
            {
                // Row 3
                row = studenttable.AddRow();
                row.Format.Alignment = ParagraphAlignment.Left;
                row.Cells[0].AddParagraph("Admission Session").Format.Font.Bold = true;
                row.Cells[1].AddParagraph(adSess.Result.SessionName);
            }



            // Row 4
            var faculty = _context.Faculty.FirstOrDefaultAsync(s => s.FacultyId == studentTranascript.StudentInfo.FacultyId);
            if (faculty != null)
            {


                row = studenttable.AddRow();
                row.Format.Alignment = ParagraphAlignment.Left;
                row.Cells[0].AddParagraph("Faculty").Format.Font.Bold = true;
                row.Cells[1].AddParagraph(faculty.Result.FacultyName);
                row.Shading.Color = Color.Parse("0x20D3D3D3");
            }

            var programme = _context.Programmes.FirstOrDefaultAsync(s => s.ProgrammeId == studentTranascript.StudentInfo.ProgrammeId);

            if (programme != null)
            {
                row = studenttable.AddRow();
                row.Format.Alignment = ParagraphAlignment.Left;
                row.Cells[0].AddParagraph("Program").Format.Font.Bold = true;
                row.Cells[1].AddParagraph(programme.Result.ProgrammeName);
            }

            // Row 6
            row = studenttable.AddRow();
            row.Format.Alignment = ParagraphAlignment.Left;
            row.Cells[0].AddParagraph("Level").Format.Font.Bold = true;
            row.Cells[1].AddParagraph("800");



            spacer = section.AddParagraph();
            spacer.Format.SpaceAfter = "0.001cm";


            if (studentTranascript.SemesterData != null)
            {
                int tableCount = 0;

                foreach (var data in studentTranascript.SemesterData)
                {
                    spacer = section.AddParagraph();
                    spacer.Format.SpaceAfter = "0.01cm";

                    var header = data.Header;


                    var subhead = section.AddParagraph();
                    subhead.Format.Alignment = ParagraphAlignment.Center;
                    subhead.Format.Font.Size = 10;
                    subhead.AddFormattedText(data.Title, TextFormat.Bold);
                    spacer = section.AddParagraph();
                    spacer.Format.SpaceAfter = "0.0005cm";

                    // === Table Creation ===
                    var table = section.AddTable();
                    table.Format.Alignment = ParagraphAlignment.Center;
                    table.Borders.Width = 0.5;


                    // Define columns
                    table.AddColumn("1cm");  // SN
                    table.AddColumn("3cm");    // Course Code
                    table.AddColumn("7cm");  // Title
                    table.AddColumn("1.5cm");    // Unit
                    table.AddColumn("1.5cm");    // Score
                    table.AddColumn("1.5cm");    // Grade                    
                    table.AddColumn("3cm");    // Grade Point
                                               //  table.Shading.Color = new Color(23, 5, 109, 0.3);

                    // ParagraphFormat paragraphFormat = new ParagraphFormat();
                    // === Header Row ===
                    row = table.AddRow();
                    row.Shading.Color = Color.Parse("0x80D3D3D3"); // Colors.LightGray;
                    row.HeadingFormat = true;
                    row.Format.Font.Bold = true;


                    row.Cells[0].AddParagraph(header.SN);
                    row.Cells[1].AddParagraph(header.CourseCode.ToUpper());
                    row.Cells[2].AddParagraph(header.CourseTitle);
                    row.Cells[2].Format.Alignment = ParagraphAlignment.Left;

                    row.Cells[3].AddParagraph(header.Unit);
                    row.Cells[4].AddParagraph(header.Score);
                    row.Cells[5].AddParagraph(header.Grade);

                    row.Cells[6].AddParagraph(header.GradePoint);


                    int rowIndex = 1;
                    var white = Color.Parse("0x00FFFFFF");
                    var lightGray = Color.Parse("0x25D3D3D3");
                    foreach (var item in data.Body)
                    {

                        // === Sample Row 1 ===
                        row = table.AddRow();
                        //row.HeadingFormat = true;

                        row.Shading.Color = (rowIndex % 2 == 0) ? lightGray : white;
                        //row.Shading.Color = Color.Parse("0x10D3D3D3"); // Colors.LightGray;                    


                        row.Cells[0].AddParagraph(item.SN); //.Format.Alignment = ParagraphAlignment.Center;

                        string courseCode = item.CourseCode.Trim().Length > 0 ? item.CourseCode.Trim().ToUpper() : "";

                        row.Cells[1].AddParagraph(courseCode).Format.Alignment = ParagraphAlignment.Left; ;
                        row.Cells[2].AddParagraph(item.CourseTitle).Format.Alignment = ParagraphAlignment.Left;

                        row.Cells[3].AddParagraph(item.Unit); //.Format.Alignment = ParagraphAlignment.Center;

                        string scoreStr = item.Score ?? "";
                        if (scoreStr.Length > 0)
                        {
                            double scoreVal = double.Parse(scoreStr);
                            scoreStr = scoreVal % 1 == 0 ? ((int)scoreVal).ToString() : scoreVal.ToString();
                        }


                        row.Cells[4].AddParagraph(scoreStr); //.Format.Alignment = ParagraphAlignment.Center;
                        row.Cells[5].AddParagraph(item.Grade); //.Format.Alignment = ParagraphAlignment.Center;                        

                        string gpStr = item.GradePoint ?? "";
                        if (gpStr.Length > 0)
                        {
                            double sVal = double.Parse(gpStr);
                            gpStr = sVal % 1 == 0 ? ((int)sVal).ToString() : sVal.ToString();
                        }

                        row.Cells[6].AddParagraph(gpStr); //.Format.Alignment = ParagraphAlignment.Center;

                        rowIndex++;
                    }

                    spacer = section.AddParagraph();
                    spacer.Format.SpaceAfter = "0.005cm";

                    var subfooter = section.AddParagraph();
                    subfooter.Format.Alignment = ParagraphAlignment.Left;
                    subfooter.Format.Font.Size = 10;
                    subfooter.AddFormattedText($"{data.Footer}      ", TextFormat.Bold);

                    tableCount++;

                    if (tableCount % 2 == 0 && tableCount < studentTranascript.SemesterData.Count)
                    {
                        section.AddPageBreak();
                    }
                }


                // === Add Final CGPA Paragraph ===
                var finalCgpaParagraph = section.AddParagraph();
                finalCgpaParagraph.Format.SpaceBefore = "0.5cm";
                finalCgpaParagraph.Format.SpaceAfter = "1cm";
                finalCgpaParagraph.Format.Alignment = ParagraphAlignment.Left;
                finalCgpaParagraph.Format.Font.Size = 12;
                finalCgpaParagraph.AddFormattedText($"Final CGPA: {studentTranascript.FinalGPA}", TextFormat.Bold);

                // === Add Signature Line at Bottom-Left ===
                var signatureParagraph = section.AddParagraph();
                signatureParagraph.Format.SpaceBefore = "3cm";  // Push signature towards bottom
                signatureParagraph.Format.Alignment = ParagraphAlignment.Left;
                signatureParagraph.Format.Font.Size = 12;
                signatureParagraph.Format.SpaceAfter = "14cm";

                signatureParagraph.AddLineBreak();

                // === Create Summary Table ===
                var summaryTable = section.AddTable();
                summaryTable.Format.Font.Size = 6;
                summaryTable.Format.Alignment = ParagraphAlignment.Center;

                // Remove all borders
                summaryTable.Borders.Width = 0;
                summaryTable.Borders.Visible = false;

                // Define columns
                //summaryTable.AddColumn(Unit.FromCentimeter(5)); // Course Code Key
                //summaryTable.AddColumn(Unit.FromCentimeter(5)); // % Pass
                summaryTable.AddColumn(Unit.FromCentimeter(5)); // Grade
                summaryTable.AddColumn(Unit.FromCentimeter(5)); // Grade Level
                summaryTable.AddColumn(Unit.FromCentimeter(5)); // CGPA
                summaryTable.AddColumn(Unit.FromCentimeter(5)); // Classification



                // === Add Header Row ===
                var hdRow = summaryTable.AddRow();
                hdRow.Borders.Visible = false;
                //hdRow.Cells[0].AddParagraph("Course Code Key").Format.Alignment = ParagraphAlignment.Left;
                //hdRow.Cells[1].AddParagraph("% Pass").Format.Alignment = ParagraphAlignment.Left;
                hdRow.Cells[0].AddParagraph("Grade").Format.Alignment = ParagraphAlignment.Left;
                hdRow.Cells[0].Format.Font.Bold = true;

                hdRow.Cells[1].AddParagraph("Grade Level").Format.Alignment = ParagraphAlignment.Left;
                hdRow.Cells[1].Format.Font.Bold = true;

                hdRow.Cells[2].AddParagraph("CGPA").Format.Alignment = ParagraphAlignment.Left;
                hdRow.Cells[2].Format.Font.Bold = true;

                hdRow.Cells[3].AddParagraph("Classification").Format.Alignment = ParagraphAlignment.Left;
                hdRow.Cells[3].Format.Font.Bold = true;

                // === Add Data Rows ===
                string[,] gradeRows = {
                                         {"A", "70.0 and Above", "4.50-5.00", "Distinction" },
                                         {"B", "60.0-69.9", "4.00-4.49", "Pass" },
                                         {"C", "55.0-59.9", "3.50-3.99", "Pass"},
                                         {"C-", "50.0-54.9", "3.00-3.49", "Pass"},
                                         {"F", "0-4.9", "0-2.99", "Fail"}
                                       };

                for (int i = 0; i < gradeRows.GetLength(0); i++)
                {
                    var rowx = summaryTable.AddRow();
                    rowx.Borders.Visible = false;
                    for (int j = 0; j < gradeRows.GetLength(1); j++)
                    {
                        rowx.Cells[j].AddParagraph(gradeRows[i, j]).Format.Alignment = ParagraphAlignment.Left;
                    }
                }




                var footer = section.Footers.Primary.AddParagraph();

                // Align page numbers to the center (or right, if you prefer)
                footer.Format.Alignment = ParagraphAlignment.Right;
                footer.Format.Font.Size = 10;
                footer.Format.Font.Color = MigraDocCore.DocumentObjectModel.Colors.Gray;

                // Add "Page X of Y"
                footer.AddText("Page ");
                footer.AddPageField();      // current page number
                footer.AddText(" of ");
                footer.AddNumPagesField();



                //paragraph.Format.Alignment = ParagraphAlignment.Center;

                var pdfRenderer = new MigraDocCore.Rendering.PdfDocumentRenderer();
                pdfRenderer.Document = document;
                pdfRenderer.RenderDocument();



                // Render the document

                string filePath = Path.Combine(Environment.CurrentDirectory, $"{studentTranascript.StudentInfo.MatricNumber}-temp.pdf");
                pdfRenderer.PdfDocument.Save(filePath);

                // Open an existing PDF document.
                PdfDocument pdfDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Modify);
                int pageIndex = 0;

                XGraphics gfx = null;

                foreach (PdfPage pg in pdfDocument.Pages)
                {


                    // Get the first page of the document.
                    //PdfPage page = pdfDocument.Pages[0];





                    // Load the image to be added.
                    switch (pageIndex)
                    {
                        case 0:
                            // Create a graphics object to draw on the page.
                            gfx = XGraphics.FromPdfPage(pg);

                            using (var img = Image.Load(imagePath))
                            using (var ms = new MemoryStream())
                            {
                                // Save the image as PNG into a memory stream
                                img.Save(ms, new PngEncoder());
                                ms.Position = 0;

                                // PdfSharpCore expects a Func<Stream>, so wrap the stream
                                var xImage = XImage.FromStream(() => new MemoryStream(ms.ToArray()));

                                //220;

                                double width = 52;
                                double height = 52;
                                double y = 20;
                                double x = (pg.Width.Point - img.Width) / 2 + width;


                                // Draw the image on the page.
                                gfx.DrawImage(xImage, x, y, width, height);

                                var cx = pg.Width / 2;
                                var cy = pg.Height / 2;

                                // Font & semi-transparent brush
                                var font = new XFont("Courier New", 48, XFontStyle.Bold);
                                var brush = new XSolidBrush(XColor.FromArgb(64, 0, 0, 0)); // ~25% opaque black

                                // Move origin to center, rotate, then draw centered text
                                gfx.TranslateTransform(cx, cy);
                                gfx.RotateTransform(-45); // diagonal
                                var rect = new XRect(-pg.Width, -60, pg.Width * 2, 120);
                                gfx.DrawString("Not Official", font, brush, rect, XStringFormats.Center);
                                pageIndex++;

                            }
                            break;
                        default:

                            gfx = XGraphics.FromPdfPage(pg);

                            using (var img = Image.Load(signaturePath))
                            using (var ms = new MemoryStream())
                            {
                                // Save the image as PNG into a memory stream
                                img.Save(ms, new PngEncoder());
                                ms.Position = 0;



                                // PdfSharpCore expects a Func<Stream>, so wrap the stream
                                var sImage = XImage.FromStream(() => new MemoryStream(ms.ToArray()));

                                //220;
                                // Suppose after writing the underline, you know the Y-position:
                                double underlineY = 100; // Example Y position in points

                                // Place signature image just above the line
                                double imageHeight = 25;
                                double imageWidth = 50;
                                double imageX = 100; // left margin
                                double imageY = underlineY - imageHeight - 5;


                                var font = new XFont("Courier New", 12, XFontStyle.Bold);
                                var brush = new XSolidBrush(XColor.FromArgb(64, 0, 0, 0)); // ~25% opaque black


                                double currentY = 650; // after your CGPA text

                                // Place image
                                gfx.DrawImage(sImage, imageX, currentY, imageWidth, imageHeight);
                                currentY += imageHeight + 5;

                                // Underline
                                gfx.DrawString("____________________", font, XBrushes.Black, new XPoint(imageX - 40, currentY + 2));
                                currentY += 10;

                                // Label
                                gfx.DrawString("Registrar Signature", font, XBrushes.Black, new XPoint(imageX - 40, currentY + 8));



                                var cx = pg.Width / 2;
                                var cy = pg.Height / 2;

                                // Font & semi-transparent brush
                                font = new XFont("Courier New", 48, XFontStyle.Bold);
                                brush = new XSolidBrush(XColor.FromArgb(64, 0, 0, 0)); // ~25% opaque black

                                // Move origin to center, rotate, then draw centered text
                                gfx.TranslateTransform(cx, cy);
                                gfx.RotateTransform(-45); // diagonal
                                var rect = new XRect(-pg.Width, -60, pg.Width * 2, 120);
                                gfx.DrawString("Not Official", font, brush, rect, XStringFormats.Center);
                                pageIndex++;

                                break;

                            }

                            // Use xImage in your PDF here
                    }



                    // PdfSharpCore.Drawing.XImage image = XImage.FromFile(imagePath);

                    // Define the position and size of the image.



                }

                if (System.IO.Directory.Exists("TranscriptDr") != true)
                {
                    System.IO.Directory.CreateDirectory("TranscriptDr");
                }
                // Save the changes to the PDF.


                string transcriptPath = Path.Combine(Environment.CurrentDirectory, $"TranscriptDr/{studentTranascript.StudentInfo.MatricNumber}-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.pdf");




                // pdfDocument.Save($"TranscriptDr/{studentTranascript.StudentInfo.MatricNumber}-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.pdf");
                pdfDocument.Save(transcriptPath);

                pdfDocument.Close();

                byte[] fileBytes = File.ReadAllBytes(transcriptPath);

                // string base64 = Convert.ToBase64String(fileBytes);

                return fileBytes;


            }
            byte[] fl = Array.Empty<byte>();

            return fl;
        }

        private void BuildDocument(string val)
        {
            // Create a new PDF document




            // PdfSharp.Fonts.GlobalFontSettings.FontResolver =  CustomFontResolver.Instance;


            //MigraDocCore
            // Create the document
            var document = new Document();

            // Set default font
            document.Styles["Normal"].Font.Name = "Courier New";
            document.Styles["Normal"].Font.Size = 11;


            var section = document.AddSection();







            AddWatermark(section, "NOT OFFICIAL");
            // AddDiagonalWatermark(section, "NOT OFFICIAL");


            var imagePath = Path.Combine(Environment.CurrentDirectory, "jhu.png");





            // === Header Info ===
            var paragraph = section.AddParagraph();
            paragraph.AddLineBreak();
            paragraph.AddLineBreak();
            paragraph.AddLineBreak();

            paragraph.Format.Font.Color = Color.Parse("0xFF8B0000");
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.Format.SpaceAfter = 20;


            paragraph.AddFormattedText("James Hope University", TextFormat.Bold).Font.Size = 22;
            paragraph.AddLineBreak();


            paragraph = section.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.Format.Font.Size = 12;
            paragraph.AddFormattedText("Office of the Registrar", TextFormat.Bold).Font.Italic = true;
            paragraph.AddLineBreak();


            paragraph = section.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.Format.Font.Size = 12;
            paragraph.AddFormattedText("Student Academic Transcript", TextFormat.Bold);


            var spacer = section.AddParagraph();
            spacer.Format.SpaceAfter = "0.2cm";



            // Student Table Info

            // Craete Student Info Table
            var studenttable = section.AddTable();
            studenttable.Format.Alignment = ParagraphAlignment.Center;
            studenttable.Borders.Width = 0.5;


            // Define columns
            studenttable.AddColumn("5.5cm");  // 
            studenttable.AddColumn("9cm");    // Course Code


            // ParagraphFormat paragraphFormat = new ParagraphFormat();
            // === Header Row ===
            var row = studenttable.AddRow();
            row.Format.Alignment = ParagraphAlignment.Left;
            // row.Shading.Color = Color.Parse("0x10D3D3D3"); // Colors.LightGray;
            row.HeadingFormat = true;
            //row.Format.Font.Bold = true;


            row.Cells[0].AddParagraph("Academic Record of");
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[1].AddParagraph("Sensi Wu");



            row = studenttable.AddRow();
            row.Format.Alignment = ParagraphAlignment.Left;
            row.Cells[0].AddParagraph("Matriculation Number");
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[1].AddParagraph("MBA230012");
            row.Shading.Color = Color.Parse("0x10D3D3D3");

            row = studenttable.AddRow();
            row.Format.Alignment = ParagraphAlignment.Left;
            row.Cells[0].AddParagraph("Admission Session");
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[1].AddParagraph("2023-2024");


            row = studenttable.AddRow();
            row.Format.Alignment = ParagraphAlignment.Left;
            row.Cells[0].AddParagraph("Faculty");
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[1].AddParagraph("School of Business");
            row.Shading.Color = Color.Parse("0x10D3D3D3");


            row = studenttable.AddRow();
            row.Format.Alignment = ParagraphAlignment.Left;
            row.Cells[0].AddParagraph("Program");
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[1].AddParagraph("Masters of Business Administration");


            row = studenttable.AddRow();
            row.Format.Alignment = ParagraphAlignment.Left;
            row.Cells[0].AddParagraph("Session");
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[1].AddParagraph("2024-2025");
            row.Shading.Color = Color.Parse("0x10D3D3D3");



            row = studenttable.AddRow();
            row.Format.Alignment = ParagraphAlignment.Left;
            row.Cells[0].AddParagraph("Level");
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[1].AddParagraph("800");




            spacer = section.AddParagraph();
            spacer.Format.SpaceAfter = "0.3cm";


            // === Table Creation ===
            var table = section.AddTable();
            table.Format.Alignment = ParagraphAlignment.Center;
            table.Borders.Width = 0.5;


            // Define columns
            table.AddColumn("1cm");  // SN
            table.AddColumn("3cm");    // Course Code
            table.AddColumn("7cm");  // Title
            table.AddColumn("1.5cm");    // Grade
            table.AddColumn("1cm");    // Unit
            table.AddColumn("3.5cm");    // Grade Point
                                         //  table.Shading.Color = new Color(23, 5, 109, 0.3);

            // ParagraphFormat paragraphFormat = new ParagraphFormat();
            // === Header Row ===
            row = table.AddRow();
            row.Shading.Color = Color.Parse("0x10D3D3D3"); // Colors.LightGray;
            row.HeadingFormat = true;
            row.Format.Font.Bold = true;


            row.Cells[0].AddParagraph("SN");
            row.Cells[1].AddParagraph("Course Code");
            row.Cells[2].AddParagraph("Title");
            row.Cells[2].Format.Alignment = ParagraphAlignment.Left;

            row.Cells[3].AddParagraph("Grade");
            row.Cells[4].AddParagraph("Unit");
            row.Cells[5].AddParagraph("Grade Point");

            // === Sample Row 1 ===
            row = table.AddRow();
            row.Cells[0].AddParagraph("1"); //.Format.Alignment = ParagraphAlignment.Center;

            row.Cells[1].AddParagraph("MTH402"); //.Format.Alignment = ParagraphAlignment.Left;
            row.Cells[2].AddParagraph("General Topology II"); //.Format.Alignment = ParagraphAlignment.Left;
            row.Cells[2].Format.Alignment = ParagraphAlignment.Left;

            row.Cells[3].AddParagraph("A"); //.Format.Alignment = ParagraphAlignment.Center;
            row.Cells[4].AddParagraph("3"); //.Format.Alignment = ParagraphAlignment.Center;
            row.Cells[5].AddParagraph("15"); //.Format.Alignment = ParagraphAlignment.Center;

            // === Sample Row 2 ===
            row = table.AddRow();
            row.Cells[0].AddParagraph("2"); //.Format.Alignment = ParagraphAlignment.Center;
            row.Cells[1].AddParagraph("MTH403"); //.Format.Alignment = ParagraphAlignment.Left;

            row.Cells[2].AddParagraph("Functional Analysis II");
            row.Cells[2].Format.Alignment = ParagraphAlignment.Left;

            row.Cells[3].AddParagraph("A"); //.Format.Alignment = ParagraphAlignment.Center;
            row.Cells[4].AddParagraph("3"); //.Format.Alignment = ParagraphAlignment.Center;
            row.Cells[5].AddParagraph("15"); //.Format.Alignment = ParagraphAlignment.Center;

            // === Semester GPA ===
            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = 10;
            paragraph.AddText("Semester GPA: 5.0");
            paragraph.AddLineBreak();

            // === Footer ===
            //paragraph = section.Footers.Primary.AddParagraph();

            //paragraph.AddText("Final CGPA: 5.0");


            var footer = section.Footers.Primary.AddParagraph();

            // Align page numbers to the center (or right, if you prefer)
            footer.Format.Alignment = ParagraphAlignment.Right;
            footer.Format.Font.Size = 10;
            footer.Format.Font.Color = MigraDocCore.DocumentObjectModel.Colors.Gray;

            // Add "Page X of Y"
            footer.AddText("Page ");
            footer.AddPageField();      // current page number
            footer.AddText(" of ");
            footer.AddNumPagesField();




            //paragraph.Format.Alignment = ParagraphAlignment.Center;

            var pdfRenderer = new MigraDocCore.Rendering.PdfDocumentRenderer();
            pdfRenderer.Document = document;
            pdfRenderer.RenderDocument();



            // Render the document

            string filePath = Path.Combine(Environment.CurrentDirectory, "Transcript1.pdf");
            pdfRenderer.PdfDocument.Save(filePath);

            // Open an existing PDF document.
            PdfDocument pdfDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Modify);

            // Get the first page of the document.
            PdfPage page = pdfDocument.Pages[0];

            // Create a graphics object to draw on the page.
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Load the image to be added.


            using (var img = Image.Load(imagePath))
            using (var ms = new MemoryStream())
            {
                // Save the image as PNG into a memory stream
                img.Save(ms, new PngEncoder());
                ms.Position = 0;

                // PdfSharpCore expects a Func<Stream>, so wrap the stream
                var xImage = XImage.FromStream(() => new MemoryStream(ms.ToArray()));

                //220;

                double width = 52;
                double height = 52;
                double y = 40;
                double x = (page.Width.Point - img.Width) / 2 + width;


                // Draw the image on the page.
                gfx.DrawImage(xImage, x, y, width, height);

                var cx = page.Width / 2;
                var cy = page.Height / 2;

                // Font & semi-transparent brush
                var font = new XFont("Courier New", 60, XFontStyle.Bold);
                var brush = new XSolidBrush(XColor.FromArgb(64, 0, 0, 0)); // ~25% opaque black

                // Move origin to center, rotate, then draw centered text
                gfx.TranslateTransform(cx, cy);
                gfx.RotateTransform(-45); // diagonal
                var rect = new XRect(-page.Width, -60, page.Width * 2, 120);
                gfx.DrawString("Not Official", font, brush, rect, XStringFormats.Center);

                // Use xImage in your PDF here
            }


            // PdfSharpCore.Drawing.XImage image = XImage.FromFile(imagePath);

            // Define the position and size of the image.


            // Save the changes to the PDF.
            pdfDocument.Save("output.pdf");
            pdfDocument.Close();

            // === Save PDF ===

        }

        public static void AddDiagonalWatermark(Section section, string text)
        {
            // Create a text frame
            var watermark = section.AddTextFrame();

            // Set size of the watermark area
            watermark.Width = "14cm";
            watermark.Height = "1cm";

            // Position watermark in the center of the page
            watermark.Left = ShapePosition.Center;
            watermark.Top = ShapePosition.Center;

            // Align relative to page
            watermark.RelativeHorizontal = RelativeHorizontal.Page;
            watermark.RelativeVertical = RelativeVertical.Page;

            // No text wrapping
            watermark.WrapFormat.Style = WrapStyle.None;

            // Rotate the text frame for diagonal effect (45 degrees)
            watermark.Orientation = TextOrientation.Upward;

            // Add text
            var paragraph = watermark.AddParagraph(text);
            paragraph.Format.Font.Size = 20;
            paragraph.Format.Font.Color = Color.Parse("0x40D3D3D3"); // Light gray
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Alignment = ParagraphAlignment.Center;
        }

        void AddDiagonalWatermark(string inputPath, string outputPath, string text)
        {
            // Open the rendered PDF
            using var pdf = PdfReader.Open(inputPath, PdfDocumentOpenMode.Modify);

            foreach (PdfPage page in pdf.Pages)
            {
                // Prepend = draw behind existing content. Use Append to draw on top.
                using var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Prepend);
                gfx.Save();

                // Center of the page
                var cx = page.Width / 2;
                var cy = page.Height / 2;

                // Font & semi-transparent brush
                var font = new XFont("Courier New", 60, XFontStyle.Bold);
                var brush = new XSolidBrush(XColor.FromArgb(64, 0, 0, 0)); // ~25% opaque black

                // Move origin to center, rotate, then draw centered text
                gfx.TranslateTransform(cx, cy);
                gfx.RotateTransform(-45); // diagonal
                var rect = new XRect(-page.Width, -60, page.Width * 2, 120);
                gfx.DrawString(text, font, brush, rect, XStringFormats.Center);

                gfx.Restore();
            }

            pdf.Save(outputPath);
        }

        public static void AddWatermark(Section section, string text)
        {
            // Create a text frame that spans the page
            var watermark = section.AddTextFrame();

            // Position in the middle of the page
            watermark.Width = "20cm";
            watermark.Height = "5cm";
            watermark.Left = ShapePosition.Center;
            watermark.Top = "10cm";

            // Rotate the text (for diagonal watermark)
            watermark.RelativeHorizontal = RelativeHorizontal.Page;
            watermark.RelativeVertical = RelativeVertical.Page;
            watermark.WrapFormat.Style = WrapStyle.None;

            // Add the paragraph with the watermark text
            var paragraph = watermark.AddParagraph(text);
            paragraph.Format.Font.Size = 60;
            paragraph.Format.Font.Color = Color.Parse("0x40D3D3D3"); // Light gray
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Alignment = ParagraphAlignment.Center;
        }

        public static void DefineStyles(Document document)
        {

            Style style = document.Styles["Normal"];
            style.Font.Name = "Arial";
            style.Font.Size = 14;

            // Create a font

            style = document.Styles["Title"];
            style.Font.Size = 20;
            style.Font.Bold = true;

            style = document.Styles["DateField"];
            style.Font.Size = 10;


            style = document.Styles["Text"];
            style.Font.Bold = true;

            style = document.Styles["Footer"];
            style.Font.Size = 8;

            style = document.Styles["Heading1"];
            style.Font.Size = 30;
        }

        public Task<GeneralResponse> GetTranscriptReportAsync(int SessionId, int DepartmentId, int SemesterId)
        {
            throw new NotImplementedException();
        }

        public Task<GeneralResponse> CreatePdfFromHTML(string html, int paperSize)
        {
            var ms = new MemoryStream();
            var pdfBytes = new byte[0];

            switch (paperSize)
            {
                case 3:

                    var pdf3 = PdfGenerator.GeneratePdf(html, PdfSharp.PageSize.A3, 12);


                    ms.Position = 0;

                    pdf3.Save(ms);
                    pdfBytes = ms.ToArray();

                    break;

                case 4:
                    var pdf4 = PdfGenerator.GeneratePdf(html, PdfSharp.PageSize.A4, 10);
                    ms.Position = 0;
                    pdf4.Save(ms);
                    pdfBytes = ms.ToArray();
                    break;

                default:


                    var pdf31 = PdfGenerator.GeneratePdf(html, PdfSharp.PageSize.A3, 12);


                    ms.Position = 0;

                    pdf31.Save(ms);
                    pdfBytes = ms.ToArray();


                    break;
            }


            var response = new GeneralResponse { StatusCode = 200, Message = "PDF generated successfully", Data = pdfBytes };
            return Task.FromResult(response);

        }

        public async Task<GeneralResponse> GetDepartmentResultAsync(DepartmentResultDto model)
        {

            var cmulative = await _context.StudentResultCummulative
            .Where(sr => sr.DepartmentId == model.DepartmentId
              && sr.SessionId == model.SessionId
              && sr.SemesterId == model.SemesterId
              && sr.ClassCode == model.ClassCode)
        .Join(
        _context.StudentResultFinalSummary,
        sr => sr.MatricNumber,
        fs => fs.MatricNumber,
        (sr, fs) => new
        {

            Name = fs.Name,
            sr.MatricNumber,
            sr.TCR,
            sr.TCE,
            sr.TGP,
            sr.GPA,
            sr.PTCR,
            sr.PTCE,
            sr.PTGP,
            sr.PGPA,
            sr.CTCR,
            sr.CTCE,
            sr.CTGP,
            sr.CGPA,
            fs.ClassOfDegree,
            sr.Remarks
        }
    )
    .OrderBy(x => x.MatricNumber)
    .ToListAsync();


            if (cmulative != null)
            {
                var results = await _context.StudentResultRawDetail
                                    .Where(c => c.DepartmentId == model.DepartmentId
                                    && c.SessionId == model.SessionId
                                    && c.SemesterId == model.SemesterId
                                    && c.ClassCode == model.ClassCode)
                                    .Select(sr => new
                                    {
                                        sr.MatricNumber,
                                        sr.CourseCode,
                                        sr.Units,
                                        sr.Score,
                                        sr.Grade
                                    })
                                    .OrderBy(x => x.MatricNumber)                                                                                                                                                   //   })
                                                                                                                                                           .OrderBy(x => x.MatricNumber)
                                    .ToListAsync();



                var courseCodes = results
                                    .Select(r => r.CourseCode)
                                    .Distinct()
                                    .OrderBy(c => c)
                                    .ToList();




                // Create the document
                var document = new Document();

                // Set default font
                document.Styles["Normal"].Font.Name = "Courier New";
                document.Styles["Normal"].Font.Size = 8;

                document.DefaultPageSetup.PageFormat = PageFormat.A3; // This changes the paper size to the desired A3 sheet



                var section = document.AddSection();



                section.PageSetup.PageFormat = PageFormat.A3;

                section.PageSetup.Orientation = Orientation.Landscape;


                var width = section.PageSetup.PageWidth;
                var height = section.PageSetup.PageHeight;


                section.PageSetup.TopMargin = Unit.FromCentimeter(0.8);
                section.PageSetup.BottomMargin = Unit.FromCentimeter(0.2);
                section.PageSetup.LeftMargin = Unit.FromCentimeter(0.8);
                section.PageSetup.RightMargin = Unit.FromCentimeter(0.8);





                var imagePath = Path.Combine(Environment.CurrentDirectory, "jhu.png");
                var signaturePath = Path.Combine(Environment.CurrentDirectory, "registrar.png");


                // === Header Info ===
                var paragraph = section.AddParagraph();
                paragraph.AddLineBreak();


                //paragraph.Format.Font.Color = Color.Parse("0xFF8B0000");
                //paragraph.Format.Alignment = ParagraphAlignment.Center;
                //paragraph.Format.SpaceBefore = "0.5cm";
                //paragraph.Format.SpaceAfter = "0.5cm";

                // University Name
                paragraph = section.AddParagraph();
                paragraph.Format.Alignment = ParagraphAlignment.Center;
                paragraph.Format.SpaceBefore = "2cm";
                paragraph.Format.SpaceAfter = 0;
                paragraph.Format.LineSpacing = Unit.FromPoint(10); // Optional: tighter line spacing
                paragraph.Format.Font.Name = "Candara";
                //paragraph.Format.Font.Color = Color.Parse("0xFF8B0000");
                //paragraph.Format.Font.Color = Color.Parse("0xFFA51C30"); // Harvard Crimson
                paragraph.Format.Font.Color = Color.Parse("0xFF682C2C"); // Texas University (Dark Maron)

                paragraph.AddFormattedText("James Hope University", TextFormat.Bold).Font.Size = 22;


                // Address
                paragraph = section.AddParagraph();
                paragraph.Format.Alignment = ParagraphAlignment.Center;
                paragraph.Format.SpaceBefore = "0.002cm";
                paragraph.Format.SpaceAfter = 0;
                paragraph.Format.LineSpacing = Unit.FromPoint(3); // Optional: tighter line spacing
                paragraph.AddFormattedText("Chevron Junction Lekki Epe Expressway, Lagos State Nigeria", TextFormat.Bold).Font.Size = 10;

                // School Name
                paragraph = section.AddParagraph();
                paragraph.Format.Alignment = ParagraphAlignment.Center;
                paragraph.Format.SpaceBefore = "0.2cm";
                paragraph.Format.SpaceAfter = 0;
                paragraph.Format.LineSpacing = Unit.FromPoint(3); // Optional: tighter line spacing
                paragraph.AddFormattedText("James Hope Business School", TextFormat.Bold).Font.Size = 12;

                // Program
                paragraph = section.AddParagraph();
                paragraph.Format.Alignment = ParagraphAlignment.Center;
                paragraph.Format.SpaceBefore = "0.2cm";
                paragraph.Format.SpaceAfter = 0;
                paragraph.Format.LineSpacing = Unit.FromPoint(3); // Optional: tighter line spacing
                paragraph.AddFormattedText("Executive MBA", TextFormat.Bold).Font.Size = 12;


                var ssion = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionId == model.SessionId);
                if (ssion != null)
                {
                    string semester = model.SemesterId == 1 ? "First Semester" : "Second Semester";

                    paragraph = section.AddParagraph();
                    paragraph.Format.Alignment = ParagraphAlignment.Right;
                    //paragraph.Format.LeftIndent = "3cm";       // Indent in from the left
                    paragraph.Format.RightIndent = "1.0cm";    // Optional: more space from the right edge
                    paragraph.Format.SpaceBefore = "0cm";
                    paragraph.Format.SpaceAfter = 0;
                    paragraph.Format.LineSpacing = Unit.FromPoint(0);
                    paragraph.AddFormattedText($"{ssion.SessionName} - {semester}", TextFormat.Bold).Font.Size = 10;
                }



                string[] ccodes = courseCodes.ToArray();

                // === Table Creation ===
                var table = section.AddTable();
                table.Format.Font.Name = "Courier New";
                table.Format.Font.Size = 8;
                table.Format.Alignment = ParagraphAlignment.Center;
                table.Borders.Width = 0.5;



                // Define columns
                table.AddColumn("0.8cm"); // SR
                table.AddColumn("5cm");  //Name
                table.AddColumn("3cm");    // Course Code

                int ct = courseCodes.Count + 13;
                double cwt = 26.2 / double.Parse(ct.ToString());

                for (int i = 1; i < ct; i++)
                {

                    table.AddColumn($"{cwt}cm");  // Title
                }


                table.AddColumn("6cm");     // Remark

                // ParagraphFormat paragraphFormat = new ParagraphFormat();

                //===First Header Row===
                var row = table.AddRow();
                row.Shading.Color = Color.Parse("0x80D3D3D3"); // Colors.LightGray;
                row.HeadingFormat = true;
                row.Format.Font.Bold = true;
                row.Format.Font.Size = 10;

                int cellIndex = 0;
                int lenScores = ccodes.Length;

                row.Cells[cellIndex].MergeRight = 2;
                row.Cells[cellIndex].AddParagraph("Student Info");

                cellIndex = cellIndex + 3;

                row.Cells[3].MergeRight = lenScores - 1; //The 
                row.Cells[3].AddParagraph("Detailed Scoresheet");

                cellIndex = cellIndex + lenScores;
                //Current
                row.Cells[cellIndex].MergeRight = 3;
                row.Cells[cellIndex].AddParagraph("Previous");

                cellIndex = cellIndex + 3 + 1; // PROMOTE FORWARD 1 CELL

                //Previous 
                row.Cells[cellIndex].MergeRight = 3;
                row.Cells[cellIndex].AddParagraph("Current");

                cellIndex = cellIndex + 3 + 1; // PROMOTE FORWARD 1 CELL

                //Cummulative 
                row.Cells[cellIndex].MergeRight = 3;
                row.Cells[cellIndex].AddParagraph("Cummulative");


                // === Other Header Rows ===
                row = table.AddRow();
                row.Shading.Color = Color.Parse("0x80D3D3D3"); // Colors.LightGray;
                row.HeadingFormat = true;
                row.Format.Font.Bold = true;
                row.Format.Font.Size = 10;

                int rowIndex = 0;
                row.Cells[rowIndex].AddParagraph("SN");
                rowIndex = rowIndex + 1;

                row.Cells[rowIndex].AddParagraph("Full Name");
                row.Cells[rowIndex].Format.Alignment = ParagraphAlignment.Left;
                rowIndex = rowIndex + 1;

                row.Cells[rowIndex].AddParagraph("MatricNo");
                row.Cells[rowIndex].Format.Alignment = ParagraphAlignment.Left;

                rowIndex = rowIndex + 1;




                for (int i = 0; i < ccodes.Length; i++)
                {
                    //row.Cells[rowIndex].AddParagraph(ccodes[i]);

                    string courseCd = ccodes[i].ToUpper();
                    var para = row.Cells[rowIndex].AddParagraph();
                    para.Format.Alignment = ParagraphAlignment.Center;
                    para.AddFormattedText(ccodes[i], TextFormat.Bold);
                    para.AddLineBreak();

                    var unt = results
                                  .Where(r => r.CourseCode == ccodes[i])
                                  .Select(r => r.Units)
                                  .FirstOrDefault();

                    para.AddFormattedText(unt.ToString(), TextFormat.Italic);



                    row.Cells[rowIndex].Format.Font.Bold = true;
                    row.Cells[rowIndex].Format.Font.Size = 7;

                    rowIndex = rowIndex + 1;
                }



                //sr.PTCR,
                row.Cells[rowIndex].AddParagraph("PTCR");
                rowIndex = rowIndex + 1;
                //sr.PTCE,
                row.Cells[rowIndex].AddParagraph("PTCE");
                rowIndex = rowIndex + 1;
                // sr.PTGP,
                row.Cells[rowIndex].AddParagraph("PTGP");
                rowIndex = rowIndex + 1;
                // sr.PGPA,
                row.Cells[rowIndex].AddParagraph("PGPA");
                rowIndex = rowIndex + 1;
                row.Cells[rowIndex].AddParagraph("TCR");
                rowIndex = rowIndex + 1;

                row.Cells[rowIndex].AddParagraph("TCE");
                rowIndex = rowIndex + 1;
                // sr.TGP,
                row.Cells[rowIndex].AddParagraph("TGP");
                rowIndex = rowIndex + 1;
                //sr.GPA,
                row.Cells[rowIndex].AddParagraph("GPA");
                rowIndex = rowIndex + 1;
                //sr.CTCR,
                row.Cells[rowIndex].AddParagraph("CTCR");
                rowIndex = rowIndex + 1;
                //sr.CTCE,
                row.Cells[rowIndex].AddParagraph("CTCE");
                rowIndex = rowIndex + 1;
                //sr.CTGP,
                row.Cells[rowIndex].AddParagraph("CTGP");
                rowIndex = rowIndex + 1;
                //sr.CGPA,
                row.Cells[rowIndex].AddParagraph("CGPA");
                //rowIndex = rowIndex + 1;
                rowIndex = rowIndex + 1;
                row.Cells[rowIndex].AddParagraph("Remarks");
                row.Cells[rowIndex].Format.Font.Bold = true;
                row.Cells[rowIndex].Format.Font.Size = 8;
                row.Cells[rowIndex].Format.Alignment = ParagraphAlignment.Left;

                int rowIndex1 = 1;
                var white = Color.Parse("0x00FFFFFF");
                var lightGray = Color.Parse("0x25D3D3D3");
                foreach (var item in cmulative)
                {

                    // === Sample Row 1 ===
                    row = table.AddRow();
                    //row.HeadingFormat = true;

                    row.Shading.Color = (rowIndex1 % 2 == 0) ? lightGray : white;
                    //row.Shading.Color = Color.Parse("0x10D3D3D3"); // Colors.LightGray;                    

                    int k = 0;
                    row.Cells[k].AddParagraph(rowIndex1.ToString()); //.Format.Alignment = ParagraphAlignment.Center;
                    k++;
                    row.Cells[k].AddParagraph(item.Name).Format.Alignment = ParagraphAlignment.Left;

                    k++;
                    row.Cells[2].AddParagraph(item.MatricNumber).Format.Alignment = ParagraphAlignment.Left;
                    k++;

                    for (int i = 0; i < courseCodes.Count; i++)
                    {


                        var grade = results
                                  .Where(r => r.MatricNumber == item.MatricNumber && r.CourseCode == ccodes[i])
                                  .Select(r => r.Grade)
                                  .FirstOrDefault();
                        if (grade == null)
                        {
                            grade = "";
                        }

                        var score = results
                                 .Where(r => r.MatricNumber == item.MatricNumber && r.CourseCode == ccodes[i])
                                 .Select(r => r.Score)
                                 .FirstOrDefault();

                        var scr = score == null ? "" : score.ToString();

                        if (scr == "0")
                        {
                            scr = string.Empty;
                        }

                        if (scr.EndsWith(".00") == true)
                        {
                            scr = scr.Split('.')[0];
                        }

                        //  row.Cells[k].AddParagraph($"{score}{grade}").Format.Alignment = ParagraphAlignment.Center;

                        var parag = row.Cells[k].AddParagraph();
                        parag.Format.Alignment = ParagraphAlignment.Center;

                        if (scr == "F")
                        {
                            var text = parag.AddFormattedText(scr, TextFormat.Bold);
                            text.Font.Color = Color.Parse("#FF0000"); // or Colors.Red
                        }
                        else
                        {
                            parag.AddFormattedText(scr, TextFormat.Bold);
                        }
                        parag.AddLineBreak();
                        parag.AddFormattedText(grade, TextFormat.Bold);


                        k++;

                    }
                    // string formattedTG = item.TCR % 1 == 0 ? item.TCR.ToString("0") : item.TCR.ToString("0.00");


                    row.Cells[k].AddParagraph(item.PTCR % 1 == 0 ? item.PTCR.ToString("0") : item.PTCR.ToString("0.##")); //.Format.Alignment = ParagraphAlignment.Center;
                    k++;
                    row.Cells[k].AddParagraph(item.PTCE % 1 == 0 ? item.PTCE.ToString("0") : item.PTCE.ToString("0.##")); //.Format.Alignment = ParagraphAlignment.Center;
                    k++;
                    row.Cells[k].AddParagraph(item.PTGP % 1 == 0 ? item.PTGP.ToString("0") : item.PTGP.ToString("0.##")); //.Format.Alignment = ParagraphAlignment.Center;
                    k++;
                    row.Cells[k].AddParagraph(item.PGPA % 1 == 0 ? item.PGPA.ToString("0") : item.PGPA.ToString("0.##")).Format.Font.Bold = true; //.Format.Alignment = ParagraphAlignment.Center;
                    k++;
                    row.Cells[k].AddParagraph(item.TCR % 1 == 0 ? item.TCR.ToString("0") : item.TCR.ToString("0.##")); //.Format.Alignment = ParagraphAlignment.Center;

                    k++;
                    row.Cells[k].AddParagraph(item.TCE % 1 == 0 ? item.TCE.ToString("0") : item.TCE.ToString("0.##")); //.Format.Alignment = ParagraphAlignment.Center;
                    k++;
                    row.Cells[k].AddParagraph(item.TGP % 1 == 0 ? item.TGP.ToString("0") : item.TGP.ToString("0.##")); //.Format.Alignment = ParagraphAlignment.Center;
                    k++;
                    row.Cells[k].AddParagraph(item.GPA % 1 == 0 ? item.GPA.ToString("0") : item.GPA.ToString("0.##")).Format.Font.Bold = true; //.Format.Alignment = ParagraphAlignment.Center;
                    k++;
                    row.Cells[k].AddParagraph(item.CTCR % 1 == 0 ? item.CTCR.ToString("0") : item.CTCR.ToString("0.##")); //.Format.Alignment = ParagraphAlignment.Center;
                    k++;
                    row.Cells[k].AddParagraph(item.CTCE % 1 == 0 ? item.CTCE.ToString("0") : item.CTCE.ToString("0.##")); //.Format.Alignment = ParagraphAlignment.Center;
                    k++;
                    row.Cells[k].AddParagraph(item.CTGP % 1 == 0 ? item.CTGP.ToString("0") : item.CTGP.ToString("0.##")); //.Format.Alignment = ParagraphAlignment.Center;
                    k++;
                    row.Cells[k].AddParagraph(item.CGPA % 1 == 0 ? item.CGPA.ToString("0") : item.CGPA.ToString("0.##")).Format.Font.Bold = true; //.Format.Alignment = ParagraphAlignment.Center;
                    k++;

                    //string cls = item.ClassOfDegree == null ? "" : item.ClassOfDegree;                      
                    //if (item.Remarks.ToLower() != "pass")
                    //{
                    //    cls = cls + item.Remarks; 
                    //}

                    row.Cells[k].AddParagraph(item.Remarks).Format.Alignment = ParagraphAlignment.Left;

                    rowIndex1 = rowIndex1 + 1;
                }

                var spacer = section.AddParagraph();
                spacer.Format.SpaceAfter = "0.005cm";

                var subfooter = section.AddParagraph();
                subfooter.Format.Alignment = ParagraphAlignment.Left;
                subfooter.Format.Font.Size = 10;
                //  subfooter.AddFormattedText($"{"Hello"}      ", TextFormat.Bold);

                //tableCount++;

                //if (tableCount % 2 == 0 && tableCount < studentTranascript.SemesterData.Count)
                //{
                //    section.AddPageBreak();
                //}



                // === Add Final CGPA Paragraph ===
                var finalCgpaParagraph = section.AddParagraph();
                finalCgpaParagraph.Format.SpaceBefore = "0.5cm";
                finalCgpaParagraph.Format.SpaceAfter = "1cm";
                finalCgpaParagraph.Format.Alignment = ParagraphAlignment.Left;
                finalCgpaParagraph.Format.Font.Size = 12;
                //   finalCgpaParagraph.AddFormattedText($"Final CGPA: {studentTranascript.FinalGPA}", TextFormat.Bold);

                // === Add Signature Line at Bottom-Left ===
                var signatureParagraph = section.AddParagraph();
                signatureParagraph.Format.SpaceBefore = "3cm";  // Push signature towards bottom
                signatureParagraph.Format.Alignment = ParagraphAlignment.Left;
                signatureParagraph.Format.Font.Size = 12;

                var signatureImage = section.AddParagraph();
                //     signatureImage.AddImage("signature.png").Width = "3cm";

                //// Draw the signature line
                //signatureParagraph.AddLineBreak();
                //signatureParagraph.AddText("_____________________");

                //// Add "Registrar Signature" text under the line
                //signatureParagraph.AddLineBreak();
                //signatureParagraph.AddText("Registrar Signature");

                // === Signature Table for Dean and Registrar ===
                var signatureTable = section.AddTable();

                // Define two columns: Left and Right
                signatureTable.AddColumn(Unit.FromCentimeter(30));   // Left column for HOD
                signatureTable.AddColumn(Unit.FromCentimeter(8));   // Right column for Registrar

                // Add a row
                var sigRow = signatureTable.AddRow();

                // === Left Cell: HOD Signature ===
                var hodPara = sigRow.Cells[0].AddParagraph();
                hodPara.AddText("___________________________");
                hodPara.AddLineBreak();
                hodPara.AddText("H.O.D's Signature");
                hodPara.Format.Alignment = ParagraphAlignment.Left;
                hodPara.Format.SpaceBefore = "2cm"; // spacing from previous content

                // === Right Cell: Dean Signature ===
                var deanPara = sigRow.Cells[1].AddParagraph();
                deanPara.AddText("___________________________");
                deanPara.AddLineBreak();
                deanPara.AddText("Dean's Signature");
                deanPara.Format.Alignment = ParagraphAlignment.Right;
                deanPara.Format.RightIndent = "1.0cm";
                deanPara.Format.SpaceBefore = "2cm"; // matching spacing

                // Add a row for Vice Chancelor to sign
                sigRow = signatureTable.AddRow();
                sigRow.Cells[0].MergeRight = 1;
                // === Left Cell: Dean Signature ===
                var vcPara = sigRow.Cells[0].AddParagraph();
                vcPara.AddText("_________________________________");
                vcPara.AddLineBreak();
                vcPara.AddText("Vice Chancelor's Signature");
                vcPara.Format.Alignment = ParagraphAlignment.Center;
                vcPara.Format.SpaceBefore = "2cm"; // spacing from previous content


                // === Header Info ===
                var blankParagraph = section.AddParagraph();
                blankParagraph.AddLineBreak();
                blankParagraph.AddLineBreak();


                // === Create Summary Table ===
                var summaryTable = section.AddTable();
                summaryTable.Format.Font.Size = 6;
                summaryTable.Format.Alignment = ParagraphAlignment.Center;

                // Remove all borders
                summaryTable.Borders.Width = 0;
                summaryTable.Borders.Visible = false;

                // Define columns
                summaryTable.AddColumn(Unit.FromCentimeter(5)); // Course Code Key
                summaryTable.AddColumn(Unit.FromCentimeter(5)); // % Pass
                summaryTable.AddColumn(Unit.FromCentimeter(5)); // Grade
                summaryTable.AddColumn(Unit.FromCentimeter(5)); // Grade Level
                summaryTable.AddColumn(Unit.FromCentimeter(5)); // CGPA
                summaryTable.AddColumn(Unit.FromCentimeter(5)); // Classification

                // === Add Header Row ===
                var hdRow = summaryTable.AddRow();
                hdRow.Borders.Visible = false;
                hdRow.Cells[0].AddParagraph("Course Code Key").Format.Alignment = ParagraphAlignment.Left;
                hdRow.Cells[1].AddParagraph("% Pass").Format.Alignment = ParagraphAlignment.Left;
                hdRow.Cells[2].AddParagraph("Grade").Format.Alignment = ParagraphAlignment.Left;
                hdRow.Cells[3].AddParagraph("Grade Level").Format.Alignment = ParagraphAlignment.Left;
                hdRow.Cells[4].AddParagraph("CGPA").Format.Alignment = ParagraphAlignment.Left;
                hdRow.Cells[5].AddParagraph("Classification").Format.Alignment = ParagraphAlignment.Left;

                // === Add Data Rows ===
                string[,] gradeRows = {
                                         { "", "", "A", "70.0 and Above", "4.50-5.00", "Distinction" },
                                         { "", "", "B", "60.0-69.9", "4.00-4.49", "Pass" },
                                         { "", "", "C", "55.0-59.9", "3.50-3.99", "Pass" },
                                         { "", "", "C-", "50.0-54.9", "3.00-3.49", "Pass" },
                                         { "", "", "F", "0-4.9", "0-2.99", "Fail" }
                                       };

                for (int i = 0; i < gradeRows.GetLength(0); i++)
                {
                    var rowx = summaryTable.AddRow();
                    rowx.Borders.Visible = false;
                    for (int j = 0; j < gradeRows.GetLength(1); j++)
                    {
                        rowx.Cells[j].AddParagraph(gradeRows[i, j]).Format.Alignment = ParagraphAlignment.Left;
                    }
                }



                var footer = section.Footers.Primary.AddParagraph();

                // Align page numbers to the center (or right, if you prefer)
                footer.Format.Alignment = ParagraphAlignment.Right;
                footer.Format.Font.Size = 10;
                footer.Format.Font.Color = MigraDocCore.DocumentObjectModel.Colors.Gray;

                // Add "Page X of Y"
                footer.AddText("Page ");
                footer.AddPageField();      // current page number
                footer.AddText(" of ");
                footer.AddNumPagesField();



                //paragraph.Format.Alignment = ParagraphAlignment.Center;

                var pdfRenderer = new MigraDocCore.Rendering.PdfDocumentRenderer(true);
                pdfRenderer.Document = document;
                pdfRenderer.RenderDocument();


                // Render the document

                string filePath = Path.Combine(Environment.CurrentDirectory, $"TranscriptDr/{"Department"}{model.DepartmentId}{"Result"}-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.pdf");
                pdfRenderer.PdfDocument.Save(filePath);


                byte[] fileBytes = File.ReadAllBytes(filePath);

                string base64 = Convert.ToBase64String(fileBytes);

                // -------


                // Open an existing PDF document.
                PdfDocument pdfDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Modify);

                PdfPage page = pdfDocument.Pages[0];
                // Create a graphics object to draw on the page.
                XGraphics gfx = XGraphics.FromPdfPage(page);

                using (var img = Image.Load(imagePath))
                using (var ms = new MemoryStream())
                {
                    // Save the image as PNG into a memory stream
                    img.Save(ms, new PngEncoder());
                    ms.Position = 0;

                    // PdfSharpCore expects a Func<Stream>, so wrap the stream
                    var xImage = XImage.FromStream(() => new MemoryStream(ms.ToArray()));

                    //220;

                    double ImageWidth = 64;
                    double ImageHeight = 64;
                    double y = 20;
                    double x = (page.Width.Point + ImageWidth + section.PageSetup.RightMargin + section.PageSetup.LeftMargin - img.Width) / 2; // + width;


                    // Draw the image on the page.
                    gfx.DrawImage(xImage, x, y, ImageWidth, ImageHeight);

                    string outputFilePath = Path.Combine(Environment.CurrentDirectory, $"TranscriptDr/{"OutputDepartment"}{model.DepartmentId}{"Result"}-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.pdf");


                    pdfDocument.Save(outputFilePath);

                    // Return the PDF as a byte array in the response

                }



                return new GeneralResponse { StatusCode = 200, Message = base64, Data = fileBytes };

            }

            return new GeneralResponse { StatusCode = 200, Message = "No results found for the specified student.", Data = cmulative };

        }
        /// <summary>
        /// //
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>

        public async Task<GeneralResponse> GetStudentCertificateAsync(StudentCertificateDto model)
        {
            // 1. Create the PDF document
            var document = new Document();
            var section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.TopMargin = 0;
            section.PageSetup.BottomMargin = 0;
            section.PageSetup.LeftMargin = 0;
            section.PageSetup.RightMargin = 0;

            // 2. Add certificate title
            var paragraph = section.AddParagraph("James Hope University");
            paragraph.Format.Font.Size = 28;
            paragraph.Format.Font.Name = "Cambria";
            paragraph.Format.Font.Color = Color.Parse("0xFF682C2C");
            paragraph.Format.Font.Bold = true;
            paragraph.Format.SpaceBefore = "5cm";
            paragraph.Format.SpaceAfter = "1cm";
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            // 3. Render document to PDF
            var renderer = new PdfDocumentRenderer(unicode: true);
            renderer.Document = document;
            renderer.RenderDocument();
            var pdf = renderer.PdfDocument;

            // 4. Set up graphics for drawing
            var page = pdf.Pages[0];
            var gfx = XGraphics.FromPdfPage(page);

            // Insert Logo

            var imagePath = Path.Combine(Environment.CurrentDirectory, "jhu.png");
            var signaturePath = Path.Combine(Environment.CurrentDirectory, "registrar.png");

            using (var img = Image.Load(imagePath))
            using (var ms = new MemoryStream())
            {
                // Save the image as PNG into a memory stream
                img.Save(ms, new PngEncoder());
                ms.Position = 0;

                // PdfSharpCore expects a Func<Stream>, so wrap the stream
                var xImage = XImage.FromStream(() => new MemoryStream(ms.ToArray()));

                //220;

                double ImageWidth = 70;
                double ImageHeight = 70;
                double y = 60;
                double x = (page.Width.Point + 1.5 * ImageWidth + section.PageSetup.RightMargin + section.PageSetup.LeftMargin - img.Width) / 2; // + width;


                // Draw the image on the page.
                gfx.DrawImage(xImage, x, y, ImageWidth, ImageHeight);         
          
                ms.Dispose();

            }

            //insert line

            // Insert Logo

            var linePath = Path.Combine(Environment.CurrentDirectory, "jhu-line2.png");
             

            using (var imgline = Image.Load(linePath))
            using (var linems = new MemoryStream())
            {
                // Save the image as PNG into a memory stream
                imgline.Save(linems, new PngEncoder());
                linems.Position = 0;

                // PdfSharpCore expects a Func<Stream>, so wrap the stream
                var lineImage = XImage.FromStream(() => new MemoryStream(linems.ToArray()));

                //220;

                double lineWidth = 90;
                double lineHeight = 10;
                double y = 40;
                double x = (page.Width.Point + lineWidth + section.PageSetup.RightMargin + section.PageSetup.LeftMargin - imgline.Width) / 2; // + width;


                // Draw the image on the page.
                gfx.DrawImage(lineImage, x, y, lineWidth, lineHeight);

                linems.Dispose();

            }





            // 

            double pageWidth = XUnit.FromMillimeter(210);  // A4 width
            double pageHeight = XUnit.FromMillimeter(297); // A4 height

            // Insert watermark image

            if (File.Exists(linePath))
            {
                XImage watermark = XImage.FromFile(linePath);

                 
               

                double centerX = (pageWidth - 240) / 2;
                double centerY = (pageHeight) / 4.5;

                gfx.DrawImage(watermark, centerX, centerY, 240, 10);
            }


            // === Styles and dimensions ===
            double outerBorderThickness = 2;
            double innerBorderThickness = 4;
            double innerMargin = 10;
            double cornerCircleRadius = XUnit.FromMillimeter(10);  // ~6mm circle radius
            double cornerSquareSize = XUnit.FromMillimeter(6);    // 6mm square

            var outerPen = new XPen(XColor.FromArgb(255, 128, 0, 0), outerBorderThickness); // Maroon
            var innerPen = new XPen(XColor.FromArgb(255, 104, 44, 44), innerBorderThickness); // Dark red
            var circlePen = new XPen(XColors.Maroon, 2); // Thin black outline for circles
            var circleFill = XBrushes.White;              // White background
            var squareFill = XBrushes.White;

            // === 5. Draw outer rectangle and corner circles ===
            DrawRectangleWithCornerOutlines(
                gfx,
                outerPen,
                circlePen,
                circleFill,
                outerBorderThickness / 2,
                outerBorderThickness / 2,
                pageWidth - outerBorderThickness,
                pageHeight - outerBorderThickness,
                cornerCircleRadius
            );

            // === 6. Draw inner rectangle ===
            double innerX = innerMargin;
            double innerY = innerMargin;
            double innerWidth = pageWidth - 2 * innerMargin;
            double innerHeight = pageHeight - 2 * innerMargin;

            gfx.DrawRectangle(innerPen, innerX, innerY, innerWidth, innerHeight);

            // === 7. Draw white squares at inner corners ===
            DrawInnerCornerSquares(gfx, squareFill, innerX, innerY, innerWidth, innerHeight, cornerSquareSize);

            // === 8. Save and return PDF ===
            string fileName = $"Certificate_{model.MatricNumber}_{DateTime.Now:yyyyMMddHHmmssfff}.pdf";
            string filePath = Path.Combine(Environment.CurrentDirectory, "TranscriptDr", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            //Secure the PDF

            //pdf.SecuritySettings.UserPassword = "viewonly";
            //pdf.SecuritySettings.OwnerPassword = "admin123";

            //// Restrict capabilities
            //pdf.SecuritySettings.PermitPrint = false;
            //pdf.SecuritySettings.PermitModifyDocument = false;
            ////pdf.SecuritySettings.PermitCopyContent = false;
            //pdf.SecuritySettings.PermitAnnotations = false;

            //Embed some info

            pdf.Info.Author = "James Hope University";
            pdf.Info.Subject = "JHU School of Business";
            pdf.Info.Keywords = "Certificate, Graduation, Authenticated";
            pdf.Info.CreationDate = DateTime.Now;
             
            // Insert a QR Code


            pdf.Save(filePath);

            byte[] fileBytes = File.ReadAllBytes(filePath);
            string base64 = Convert.ToBase64String(fileBytes);

            return new GeneralResponse
            {
                StatusCode = 200,
                Message = base64,
                Data = fileBytes
            };
        }

        private void DrawRectangleWithCornerOutlines( XGraphics gfx, XPen rectanglePen, XPen circlePen,  XBrush circleFill, double x, double y, double width, double height, double cornerCircleRadius)
        {
            // 1. Draw the outer rectangle
            gfx.DrawRectangle(rectanglePen, x, y, width, height);

            // 2. Define corner centers
            var corners = new (double cx, double cy)[]
            {
                (x, y),                       // Top-left
                (x + width, y),              // Top-right
                (x + width, y + height),     // Bottom-right
                (x, y + height)              // Bottom-left
            };

            double diameter = cornerCircleRadius * 2;

            // 3. Draw circle at each corner
            foreach (var (cx, cy) in corners)
            {
                double circleX = cx - cornerCircleRadius;
                double circleY = cy - cornerCircleRadius;

                gfx.DrawEllipse(circleFill, circleX, circleY, diameter, diameter); // Fill
                gfx.DrawEllipse(circlePen, circleX, circleY, diameter, diameter); // Outline
            }
        }


        private void DrawInnerCornerSquares( XGraphics gfx, XBrush fill, double x, double y, double width, double height, double squareSize)
        {
            double s = squareSize;

            double offset = XUnit.FromMillimeter(1); // Inset the square inward

            var corners = new (double sx, double sy)[]
            {
                    (x + offset, y + offset),                                 // Top-left
                    (x + width - s - offset, y + offset),                     // Top-right
                    (x + width - s - offset, y + height - s - offset),        // Bottom-right
                    (x + offset, y + height - s - offset)                     // Bottom-left
            };

            foreach (var (sx, sy) in corners)
            {
                gfx.DrawRectangle(fill, sx, sy, s, s);
            }
        }


        //    public async Task<GeneralResponse> GetStudentCertificateAsync(StudentCertificateDto model)
        //    {
        //        // 1. Create PDF document
        //        var document = new Document();
        //        var section = document.AddSection();
        //        section.PageSetup.PageFormat = PageFormat.A4;
        //        section.PageSetup.TopMargin = 0;
        //        section.PageSetup.BottomMargin = 0;
        //        section.PageSetup.LeftMargin = 0;
        //        section.PageSetup.RightMargin = 0;

        //        // 2. Add certificate title
        //        var paragraph = section.AddParagraph("Certificate of Graduation");
        //        paragraph.Format.Font.Size = 24;
        //        paragraph.Format.Font.Bold = true;
        //        paragraph.Format.SpaceBefore = "8cm";
        //        paragraph.Format.SpaceAfter = "1cm";

        //        // 3. Render PDF
        //        var renderer = new PdfDocumentRenderer(unicode: true);
        //        renderer.Document = document;
        //        renderer.RenderDocument();
        //        var pdf = renderer.PdfDocument;

        //        // 4. Prepare drawing surface
        //        var page = pdf.Pages[0];
        //        var gfx = XGraphics.FromPdfPage(page);

        //        double pageWidth = XUnit.FromMillimeter(210);  // A4 width
        //        double pageHeight = XUnit.FromMillimeter(297); // A4 height

        //        // === Drawing parameters ===
        //        double outerBorderThickness = 2;
        //        double innerBorderThickness = 4;
        //        double cornerCircleRadius = 12;  // radius for corner-covering circles
        //        double innerMargin = 10;         // margin from outer border to inner rectangle

        //        var outerPen = new XPen(XColor.FromArgb(255, 128, 0, 0), outerBorderThickness); // Maroon
        //        var innerPen = new XPen(XColor.FromArgb(255, 104, 44, 44), innerBorderThickness); // Dark red

        //        // === Step 1: Outer rectangle with circular caps ===
        //        DrawRectangleWithCornerCircles(
        //            gfx,
        //            outerPen,
        //            XBrushes.White, // Fill color to "hide" corners
        //            outerBorderThickness / 2,
        //            outerBorderThickness / 2,
        //            pageWidth - outerBorderThickness,
        //            pageHeight - outerBorderThickness,
        //            cornerCircleRadius
        //        );

        //        // === Step 2: Inner rectangle ===
        //        gfx.DrawRectangle(
        //            innerPen,
        //            innerMargin,
        //            innerMargin,
        //            pageWidth - 2 * innerMargin,
        //            pageHeight - 2 * innerMargin
        //        );

        //        // === Step 3: Save and return PDF ===
        //        string fileName = $"Certificate_{model.MatricNumber}_{DateTime.Now:yyyyMMddHHmmssfff}.pdf";
        //        string filePath = Path.Combine(Environment.CurrentDirectory, "TranscriptDr", fileName);
        //        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        //        pdf.Save(filePath);

        //        byte[] fileBytes = File.ReadAllBytes(filePath);
        //        string base64 = Convert.ToBase64String(fileBytes);

        //        return new GeneralResponse
        //        {
        //            StatusCode = 200,
        //            Message = base64,
        //            Data = fileBytes
        //        };
        //    }
        //    private void DrawRectangleWithCornerCircles(
        //XGraphics gfx,
        //XPen borderPen,
        //XBrush cornerFill,
        //double x,
        //double y,
        //double width,
        //double height,
        //double cornerCircleRadius)
        //    {
        //        // 1. Draw the full outer rectangle
        //        gfx.DrawRectangle(borderPen, x, y, width, height);

        //        // 2. Coordinates for corners
        //        var corners = new (double cx, double cy)[]
        //        {
        //    (x, y),                          // Top-left
        //    (x + width, y),                 // Top-right
        //    (x + width, y + height),        // Bottom-right
        //    (x, y + height),                // Bottom-left
        //        };

        //        // 3. Draw white-filled circles centered on each corner
        //        foreach (var (cx, cy) in corners)
        //        {
        //            double circleX = cx - cornerCircleRadius;
        //            double circleY = cy - cornerCircleRadius;
        //            double diameter = cornerCircleRadius * 2;

        //            gfx.DrawEllipse(cornerFill, circleX, circleY, diameter, diameter);
        //        }
        //    }


        //private void DrawRectangleWithCircularCutouts(
        //  XGraphics gfx,
        //  XPen pen,
        //  XBrush fill, 
        //  double x,
        //  double y,
        //  double width,
        //  double height,
        //  double radius)
        //      {
        //          var path = new XGraphicsPath();

        //          // Rectangle edges (adjusted to leave room for arcs)
        //          double left = x + radius;
        //          double top = y + radius;
        //          double right = x + width - radius;
        //          double bottom = y + height - radius;

        //          path.StartFigure();

        //          // Top edge
        //          path.AddLine(left, y, right, y);
        //          path.AddArc(x + width - 2 * radius, y, 2 * radius, 2 * radius, 270, -90); // Top-right

        //          // Right edge
        //          path.AddLine(x + width, top, x + width, bottom);
        //          path.AddArc(x + width - 2 * radius, y + height - 2 * radius, 2 * radius, 2 * radius, 0, -90); // Bottom-right

        //          // Bottom edge
        //          path.AddLine(right, y + height, left, y + height);
        //          path.AddArc(x, y + height - 2 * radius, 2 * radius, 2 * radius, 90, -90); // Bottom-left

        //          // Left edge
        //          path.AddLine(x, bottom, x, top);
        //          path.AddArc(x, y, 2 * radius, 2 * radius, 180, -90); // Top-left

        //          path.CloseFigure();

        //          // gfx.DrawPath(pen, fill, path);
        //      }


        //public async Task<GeneralResponse> GetStudentCertificateAsync(StudentCertificateDto model)
        //{
        //    //We want to create a certificate with border
        //    var document = new Document();
        //    var section = document.AddSection();

        //    //Seet all margins to zero
        //    section.PageSetup.PageFormat = PageFormat.A4;
        //    section.PageSetup.TopMargin = 0;
        //    section.PageSetup.BottomMargin = 0;
        //    section.PageSetup.LeftMargin = 0;
        //    section.PageSetup.RightMargin = 0;

        //    var paragraph = section.AddParagraph("Certificate of graduation");
        //    paragraph.Format.Font.Size = 24;
        //    paragraph.Format.Font.Bold = true;

        //    paragraph.Format.SpaceBefore = "8cm";
        //    paragraph.Format.SpaceAfter = "1cm";

        //    var renderer = new PdfDocumentRenderer(unicode: true);


        //    renderer.Document = document;
        //    renderer.RenderDocument();
        //    var pdf = renderer.PdfDocument;

        //    var page = pdf.Pages[0];
        //    var gfx = XGraphics.FromPdfPage(page);

        //    double pageWidth = XUnit.FromMillimeter(210);
        //    double pageHeight = XUnit.FromMillimeter(297);

        //    double outThickness = 2;

        //    double inThickness = 2;

        //    var maroon = new XPen(XColor.FromArgb(255, 128, 0, 0), outThickness);

        //    gfx.DrawRectangle(maroon, outThickness / 2, outThickness / 2, pageWidth - outThickness, pageHeight - outThickness);


        //    //var darkRed = new XPen(XColor.FromArgb(255, 139, 0, 0), inThickness);
        //    double innerMargin = 7;// from ourterMargin // outThickness + (inThickness / 2);

        //    double innewThickness = 4;// outThickness + inThickness;
        //    double cornerRadius = 3;

        //    var innerPen = new XPen(XColor.FromArgb(255, 104, 44, 44), innewThickness); // Dark red color

        //    DrawInsetRoundedRectangle(gfx, innerPen, innerMargin,innerMargin, pageWidth - (2 * innerMargin), pageHeight - (2 * innerMargin), cornerRadius);


        //  string   filePath = Path.Combine(Environment.CurrentDirectory, $"TranscriptDr/{"Certificate"}{model.MatricNumber}-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.pdf");


        //    pdf.Save(filePath);


        //    byte[] fileBytes = File.ReadAllBytes(filePath);

        //    string base64 = Convert.ToBase64String(fileBytes);
        //    return  new GeneralResponse { StatusCode = 200, Message =  base64, Data = fileBytes };
        //}

        //private void DrawInsetRoundedRectangle(XGraphics gfx, XPen pen, XBrush fill, double x, double y, double width, double height, double radius)
        //{
        //    var path = new XGraphicsPath();

        //    // Calculate adjusted rectangle size
        //    double left = x + radius;
        //    double top = y + radius;
        //    double right = x + width - radius;
        //    double bottom = y + height - radius;

        //    // Start path at top-left after arc
        //    path.StartFigure();

        //    // Top edge (left to right)
        //    path.AddLine(left, y, right, y);

        //    // Top-right arc (concave)
        //    path.AddArc(x + width - 2 * radius, y, 2 * radius, 2 * radius, 270, -90);

        //    // Right edge (top to bottom)
        //    path.AddLine(x + width, top, x + width, bottom);

        //    // Bottom-right arc
        //    path.AddArc(x + width - 2 * radius, y + height - 2 * radius, 2 * radius, 2 * radius, 0, -90);

        //    // Bottom edge (right to left)
        //    path.AddLine(right, y + height, left, y + height);

        //    // Bottom-left arc
        //    path.AddArc(x, y + height - 2 * radius, 2 * radius, 2 * radius, 90, -90);

        //    // Left edge (bottom to top)
        //    path.AddLine(x, bottom, x, top);

        //    // Top-left arc
        //    path.AddArc(x, y, 2 * radius, 2 * radius, 180, -90);

        //    // Close path
        //    path.CloseFigure();

        //    // Fill and draw
        //    gfx.DrawPath(pen, fill, path);
        //}


        //private void DrawInsetRoundedRectangle(XGraphics gfx, XPen pen, double x, double y, double width, double height, double radius)
        //{
        //    var path = new XGraphicsPath();

        //    // Start drawing a new figure
        //    path.StartFigure();

        //    // Top edge
        //    path.AddLine(x + radius, y, x + width - radius, y);
        //    path.AddArc(x + width - 2 * radius, y, 2 * radius, 2 * radius, 270, -90); // Top-right concave

        //    // Right edge
        //    path.AddLine(x + width, y + radius, x + width, y + height - radius);
        //    path.AddArc(x + width - 2 * radius, y + height - 2 * radius, 2 * radius, 2 * radius, 0, -90); // Bottom-right concave

        //    // Bottom edge
        //    path.AddLine(x + width - radius, y + height, x + radius, y + height);
        //    path.AddArc(x, y + height - 2 * radius, 2 * radius, 2 * radius, 90, -90); // Bottom-left concave

        //    // Left edge
        //    path.AddLine(x, y + height - radius, x, y + radius);
        //    path.AddArc(x, y, 2 * radius, 2 * radius, 180, -90); // Top-left concave

        //    // Close the path
        //    path.CloseFigure();

        //    // Draw the path
        //    gfx.DrawPath(pen, path);
        //}

        //private void DrawRoundedRectangle(XGraphics gfx, XPen innerPen, double x, double y, double width, double height, double radius)
        //{
        //     var path = new XGraphicsPath();
        //    path.AddArc(x, y, radius * 2, radius * 2, 180, 90); // Top-left corner
        //    path.AddLine(x + radius, y, x + width - radius, y); // Top-edge

        //    path.AddArc(x + width - radius * 2, y, radius * 2, radius * 2, 270, 90); // Top-right corner
        //    path.AddLine(x + width, y + radius, x + width, y + height - radius); // Right-edge

        //    path.AddArc(x + width - radius * 2, y + height - radius * 2, radius * 2, radius * 2, 0, 90); // Bottom-right corner
        //    path.AddLine(x + width - radius, y + height, x + radius, y + height); // Bottom-edge

        //    path.AddArc(x, y + height - radius * 2, radius * 2, radius * 2, 90, 90); // Bottom-left corner
        //    path.AddLine(x, y + height - radius, x, y + radius); // Left-edge

        //    //Close and draw
        //    path.CloseFigure();

        //    gfx.DrawPath(innerPen, path);       



        //}
        public Task<GeneralResponse> GetManagementReportAsync(DepartmentResultDto model)
        {
            throw new NotImplementedException();
        }
    }
}
