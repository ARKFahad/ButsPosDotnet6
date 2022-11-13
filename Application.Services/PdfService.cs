using Application.Core.Common;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.ReportModels;
using Application.ViewModel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PbApp.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PdfService : IPdfService
    {
         private readonly GHP_POS_DBContext _context;
        private readonly ICommonService _commonService;
        public PdfService(GHP_POS_DBContext context, ICommonService commonService)
        {
            _context = context;
            _commonService = commonService; 
        }
        protected void DrawHorizontalLineProtrait(PdfWriter pdfWriter)
        {
            PdfContentByte cb = pdfWriter.DirectContent;
            cb.MoveTo(30, 735);
            cb.LineTo(580, 735);
            cb.Stroke();
        }

        protected void DrawHorizontalLineLandscape(PdfWriter pdfWriter)
        {
            PdfContentByte cb = pdfWriter.DirectContent;
            cb.MoveTo(30, 485);
            cb.LineTo(810, 485);
            cb.Stroke();
        }
        public  byte[] AddPageNumbers(byte[] pdf)
        {
            MemoryStream ms = new MemoryStream();
            // we create a reader for a certain document
            PdfReader reader = new PdfReader(pdf);
            // we retrieve the total number of pages
            int n = reader.NumberOfPages;
            // we retrieve the size of the first page
            Rectangle psize = reader.GetPageSize(1);

            // step 1: creation of a document-object
            Document document = new Document(psize, 50, 50, 50, 50);
            // step 2: we create a writer that listens to the document
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            // step 3: we open the document

            document.Open();
            // step 4: we add content
            PdfContentByte cb = writer.DirectContent;

            int p = 0;
            // Console.WriteLine("There are " + n + " pages in the document.");
            for (int page = 1; page <= reader.NumberOfPages; page++)
            {
                document.NewPage();
                p++;

                PdfImportedPage importedPage = writer.GetImportedPage(reader, page);
                cb.AddTemplate(importedPage, 0, 0);

                BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

                cb.BeginText();
                cb.SetFontAndSize(bf, 10);

                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Checked By", 30, 30, 0);
                cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Approved By", 570, 30, 0);
                cb.EndText();

                cb.BeginText();
                cb.SetFontAndSize(bf, 10);

                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Printing Date : " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"), 30, 10, 0);
                cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Page " + p + " of " + n, 340, 10, 0);
                cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Powered By: BUTS", 570, 10, 0);
                cb.EndText();
            }
            // step 5: we close the document
            document.Close();
            return ms.ToArray();
        }
        protected virtual Font GetFont()
        {  
            var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "pdf", Path.GetFileName("RobotoRegular.ttf"));
            var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var font = new Font(baseFont, 10, Font.NORMAL);
            return font;
        }
        protected virtual PdfPCell GetPdfCell(object text, Font font)
        {
            return new PdfPCell(new Phrase(text.ToString(), font));
        }
        protected virtual void PrintReportHeader(PdfWriter pdfWriter, string reportHeader,Branch branch, Font headerTitleFont, Font addressFont, Font reportTitleFont, Document doc, DateTime? fromDate, DateTime? toDatde)
        {

            var font = GetFont();
            font.SetFamily("Times New Roman");
            font.Size = 10;
            font.SetStyle(Font.NORMAL);
            font.SetStyle(Font.BOLD);
            font.Color = BaseColor.Black;

            //header
            var numColumns = 2;
            var headerTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 70,30 } }
            };

            headerTable.SetWidths(widths[numColumns]);
            //SL
            var company = _context.Companies.First();

            var logo = Image.GetInstance(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", Path.GetFileName(company.LogoPath)));
            logo.ScaleToFit(100f, 500f);
            logo.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
            logo.IndentationLeft = 9f;
            logo.IndentationRight = 9f;
            logo.SpacingBefore = 5f;
            logo.SpacingAfter = 2f;
            //Item

            var logocell = new PdfPCell { Border = Rectangle.NO_BORDER };
            logocell.AddElement(logo);
            logocell.Colspan = 2;
            headerTable.AddCell(logocell);
            var cellCompanyName = GetPdfCell(company.Name, headerTitleFont);
            cellCompanyName.HorizontalAlignment = Element.ALIGN_CENTER;
            cellCompanyName.Phrase.Add(new Phrase(Environment.NewLine));
            cellCompanyName.Border = Rectangle.NO_BORDER;
            cellCompanyName.Colspan = 2;
            headerTable.AddCell(cellCompanyName);
           var cellReportName = GetPdfCell(reportHeader, headerTitleFont);
            cellReportName.HorizontalAlignment = Element.ALIGN_CENTER;
            cellReportName.Phrase.Add(new Phrase(Environment.NewLine));
            cellReportName.Phrase.Add(new Phrase(Environment.NewLine));
            cellReportName.Border = Rectangle.NO_BORDER;
            cellReportName.Colspan = 2;
            headerTable.AddCell(cellReportName);
            var cellBranchName = GetPdfCell("Branch Name: " + branch.Name, font);
            if (branch != null)
            {
                cellBranchName.HorizontalAlignment = Element.ALIGN_LEFT;
                cellBranchName.Border = Rectangle.NO_BORDER;
                headerTable.AddCell(cellBranchName);
            }
            else
            {
                cellBranchName = GetPdfCell("" , font);
                cellBranchName.HorizontalAlignment = Element.ALIGN_LEFT;
                cellBranchName.Border = Rectangle.NO_BORDER;
                headerTable.AddCell(cellBranchName);
            }
          var  cellFromDate = GetPdfCell("From: " + fromDate?.ToString("dd/MM/yyyy"), font);
            if (fromDate.HasValue)
            {
                cellFromDate.HorizontalAlignment = Element.ALIGN_RIGHT;
                cellFromDate.Phrase.Add(new Phrase(Environment.NewLine));
                cellFromDate.Border = Rectangle.NO_BORDER;
                headerTable.AddCell(cellFromDate);
            }
            else
            {
                cellFromDate = GetPdfCell("", font);
                cellFromDate.HorizontalAlignment = Element.ALIGN_RIGHT;
                cellFromDate.Border = Rectangle.NO_BORDER;
                headerTable.AddCell(cellFromDate);
            }
           var cellBranchCode = GetPdfCell("Branch Code : " + branch.Code, font);
            if (branch != null)
            {
                cellBranchCode = GetPdfCell("Branch Code : "+branch.Code , font);
                cellBranchCode.HorizontalAlignment = Element.ALIGN_LEFT;
                cellBranchCode.Border = Rectangle.NO_BORDER;
                headerTable.AddCell(cellBranchCode);
            }
            else
            {
                cellBranchCode = GetPdfCell("", font);
                cellBranchCode.HorizontalAlignment = Element.ALIGN_LEFT;
                cellBranchCode.Border = Rectangle.NO_BORDER;
                headerTable.AddCell(cellBranchCode);
            }
           var cellToDate = GetPdfCell("To: " + toDatde?.ToString("dd/MM/yyyy"), font);
            if (toDatde.HasValue)
            {
                cellToDate = GetPdfCell("To: " + toDatde?.ToString("dd/MM/yyyy"), font);
                cellToDate.HorizontalAlignment = Element.ALIGN_RIGHT;
                cellToDate.Phrase.Add(new Phrase(Environment.NewLine));
                cellToDate.Phrase.Add(new Phrase(Environment.NewLine));
                cellToDate.Border = Rectangle.NO_BORDER;
                headerTable.AddCell(cellToDate);
            }
            else
            {
                cellToDate = GetPdfCell("", font);
                cellToDate.HorizontalAlignment = Element.ALIGN_RIGHT;
                cellToDate.Border = Rectangle.NO_BORDER;
                headerTable.AddCell(cellToDate);
            }
            doc.Add(headerTable);
        }
        protected virtual void PrintSupplierLedgerReportHeader(PdfWriter pdfWriter, string reportHeader, Supplier supplier, Font headerTitleFont, Font addressFont, Font reportTitleFont, Document doc, DateTime? fromDate, DateTime? toDatde)
        {


            var font = GetFont();
            font.SetFamily("Times New Roman");
            font.Size = 10;
            font.SetStyle(Font.NORMAL);
            font.SetStyle(Font.BOLD);
            font.Color = BaseColor.Black;

            //header
            var numColumns = 2;
            var headerTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 70,30 } }
            };

            headerTable.SetWidths(widths[numColumns]);
            var company = _context.Companies.First();

            var logo = Image.GetInstance(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", Path.GetFileName(company.LogoPath)));
            logo.ScaleToFit(100f, 500f);
            logo.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
            logo.IndentationLeft = 9f;
            logo.IndentationRight = 9f;
            logo.SpacingBefore = 5f;
            logo.SpacingAfter = 2f;
            //Item

            var logocell = new PdfPCell { Border = Rectangle.NO_BORDER };
            logocell.AddElement(logo);
            logocell.Colspan = 2;
            headerTable.AddCell(logocell);



            var cellHeader = GetPdfCell(company.Name, headerTitleFont);


            cellHeader.HorizontalAlignment = Element.ALIGN_CENTER;
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));

            cellHeader.Border = Rectangle.NO_BORDER;

            cellHeader.Phrase.Add(new Phrase("Mobile: " + supplier.Contact, addressFont));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));

            cellHeader.Phrase.Add(new Phrase("Balance: " + supplier.Balance, reportTitleFont));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Colspan = 2;
            headerTable.AddCell(cellHeader);

            cellHeader = GetPdfCell(reportHeader, headerTitleFont);


            cellHeader.HorizontalAlignment = Element.ALIGN_CENTER;
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));

            cellHeader.Border = Rectangle.NO_BORDER;
            cellHeader.Colspan = 2;
            headerTable.AddCell(cellHeader);

            cellHeader = GetPdfCell("", font);


            cellHeader.HorizontalAlignment = Element.ALIGN_LEFT;


            cellHeader.Border = Rectangle.NO_BORDER;

            headerTable.AddCell(cellHeader);

            if (fromDate.HasValue)
            {
                cellHeader = GetPdfCell("From: " + fromDate?.ToString("dd/MM/yyyy"), font);


                cellHeader.HorizontalAlignment = Element.ALIGN_RIGHT;
                cellHeader.Phrase.Add(new Phrase(Environment.NewLine));


                cellHeader.Border = Rectangle.NO_BORDER;

                headerTable.AddCell(cellHeader);
            }
            else
            {
                cellHeader = GetPdfCell("", font);


                cellHeader.HorizontalAlignment = Element.ALIGN_LEFT;


                cellHeader.Border = Rectangle.NO_BORDER;

                headerTable.AddCell(cellHeader);
            }
            cellHeader = GetPdfCell("", font);


            cellHeader.HorizontalAlignment = Element.ALIGN_LEFT;


            cellHeader.Border = Rectangle.NO_BORDER;

            headerTable.AddCell(cellHeader);
            if (toDatde.HasValue)
            {
                cellHeader = GetPdfCell("To: " + toDatde?.ToString("dd/MM/yyyy"), font);


                cellHeader.HorizontalAlignment = Element.ALIGN_RIGHT;
                cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
                cellHeader.Phrase.Add(new Phrase(Environment.NewLine));

                cellHeader.Border = Rectangle.NO_BORDER;

                headerTable.AddCell(cellHeader);
            }

           

            doc.Add(headerTable);
        }
        protected virtual void PrintCustomerLedgerReportHeader(PdfWriter pdfWriter, string reportHeader, Customer customer, Font headerTitleFont, Font addressFont, Font reportTitleFont, Document doc, DateTime? fromDate, DateTime? toDatde)
        {

            var font = GetFont();
            font.SetFamily("Times New Roman");
            font.Size = 10;
            font.SetStyle(Font.NORMAL);
            font.SetStyle(Font.BOLD);
            font.Color = BaseColor.Black;

            //header
            var numColumns = 2;
            var headerTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 70,30 } }
            };

            headerTable.SetWidths(widths[numColumns]);

            var company = _context.Companies.First();
            var logo = Image.GetInstance(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", Path.GetFileName(company.LogoPath)));
            logo.ScaleToFit(100f, 500f);
            logo.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
            logo.IndentationLeft = 9f;
            logo.IndentationRight = 9f;
            logo.SpacingBefore = 5f;
            logo.SpacingAfter = 2f;
            //Item

            var logocell = new PdfPCell { Border = Rectangle.NO_BORDER };
            logocell.AddElement(logo);
            logocell.Colspan = 2;
            headerTable.AddCell(logocell);



            var cellHeader = GetPdfCell(company.Name, headerTitleFont);


            cellHeader.HorizontalAlignment = Element.ALIGN_CENTER;
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));

            cellHeader.Border = Rectangle.NO_BORDER;

            cellHeader.Phrase.Add(new Phrase("Mobile: " + customer.Contact, addressFont));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));

            cellHeader.Phrase.Add(new Phrase("Balance: " + customer.Balance, reportTitleFont));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Colspan = 2;
            headerTable.AddCell(cellHeader);

            cellHeader = GetPdfCell(reportHeader, headerTitleFont);


            cellHeader.HorizontalAlignment = Element.ALIGN_CENTER;
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));

            cellHeader.Border = Rectangle.NO_BORDER;
            cellHeader.Colspan = 2;
            headerTable.AddCell(cellHeader);

            cellHeader = GetPdfCell("", font);


            cellHeader.HorizontalAlignment = Element.ALIGN_LEFT;


            cellHeader.Border = Rectangle.NO_BORDER;

            headerTable.AddCell(cellHeader);

            if (fromDate.HasValue)
            {
                cellHeader = GetPdfCell("From: " + fromDate?.ToString("dd/MM/yyyy"), font);


                cellHeader.HorizontalAlignment = Element.ALIGN_RIGHT;
                cellHeader.Phrase.Add(new Phrase(Environment.NewLine));


                cellHeader.Border = Rectangle.NO_BORDER;

                headerTable.AddCell(cellHeader);
            }
            else
            {
                cellHeader = GetPdfCell("", font);


                cellHeader.HorizontalAlignment = Element.ALIGN_LEFT;


                cellHeader.Border = Rectangle.NO_BORDER;

                headerTable.AddCell(cellHeader);
            }
            cellHeader = GetPdfCell("", font);


            cellHeader.HorizontalAlignment = Element.ALIGN_LEFT;


            cellHeader.Border = Rectangle.NO_BORDER;

            headerTable.AddCell(cellHeader);
            if (toDatde.HasValue)
            {
                cellHeader = GetPdfCell("To: " + toDatde?.ToString("dd/MM/yyyy"), font);


                cellHeader.HorizontalAlignment = Element.ALIGN_RIGHT;
                cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
                cellHeader.Phrase.Add(new Phrase(Environment.NewLine));

                cellHeader.Border = Rectangle.NO_BORDER;

                headerTable.AddCell(cellHeader);
            }

            doc.Add(headerTable);
        }
        protected virtual void PrintReportHeader(PdfWriter pdfWriter, string reportHeader, Branch branch, Font headerTitleFont, Font addressFont, Font reportTitleFont, Document doc)
        {

            var headerTable = new PdfPTable(1)
            {
                // RunDirection = GetDirection(lang)
            };
            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;
            var company = _context.Companies.First();
            var cellHeader = GetPdfCell("", addressFont);
            cellHeader.Phrase.Add(new Phrase(company.Name, headerTitleFont));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.HorizontalAlignment = Element.ALIGN_CENTER;
            cellHeader.Border = Rectangle.NO_BORDER;
            if (branch != null)
            {
                cellHeader.Phrase.Add(new Phrase(branch.Name, headerTitleFont));
                cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
                cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
                cellHeader.Phrase.Add(new Phrase(branch.Address, addressFont));
                cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
                cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
                cellHeader.Phrase.Add(new Phrase("Mobile: " + branch.MobileNo, addressFont));
                cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            }
            //if (fromDate.HasValue && toDatde.HasValue)
            //{
            //    cellHeader.Phrase.Add(new Paragraph("From : " + fromDate.GetValueOrDefault().ToString("dd/MM/yyyy")) { Alignment = Element.ALIGN_LEFT });
            //    cellHeader.Phrase.Add(new Paragraph("    To : " + toDatde.GetValueOrDefault().ToString("dd/MM/yyyy")) { Alignment = Element.ALIGN_RIGHT });
            //}


            //cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            //cellHeader.Phrase.Add(new Phrase("Vat Reg: "+branch.VatRegNo, addressFont));
            //cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            if (branch == null)
            {
                cellHeader.Phrase.Add(new Phrase(reportHeader + "(All Branch)", reportTitleFont));
            }
            else if (branch != null)
            {
                cellHeader.Phrase.Add(new Phrase(reportHeader, reportTitleFont));
            }
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.HorizontalAlignment = Element.ALIGN_CENTER;
            cellHeader.Border = Rectangle.NO_BORDER;

            headerTable.AddCell(cellHeader);

            //var cellSpace = GetPdfCell("", headerTitleFont);
            //cellSpace.Border = Rectangle.NO_BORDER;

            //headerTable.AddCell(cellSpace);

            doc.Add(headerTable);
        }
        protected virtual PdfPTable PrintInvoiceHeader( string reportHeader, Branch branch, Font headerTitleFont, Font addressFont, Font reportTitleFont)
        {

            //header
            var headerTable = new PdfPTable(1)
            {
                WidthPercentage = 65f
                // RunDirection = GetDirection(lang)
            };
            headerTable.SetTotalWidth(new float[] {100 });
            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;
            var company = _context.Companies.First();
            var logo = Image.GetInstance(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", Path.GetFileName(company.LogoPath)));
            //  logo.Alignment = GetAlignment(lang, true);
            logo.ScaleToFit(120f, 60f);
            var cellHeaderLogo = new PdfPCell { Border = Rectangle.NO_BORDER };
            cellHeaderLogo.AddElement(logo);
            cellHeaderLogo.HorizontalAlignment = Element.ALIGN_CENTER;

            headerTable.AddCell(cellHeaderLogo);

            var cellHeader = GetPdfCell("", addressFont);
            cellHeader.Phrase.Add(new Phrase(branch.Name, headerTitleFont));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(branch.Address, reportTitleFont));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase("Mobile: " + branch.MobileNo, reportTitleFont));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));



            //cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            //cellHeader.Phrase.Add(new Phrase("Vat Reg: "+branch.VatRegNo, addressFont));
            //cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.HorizontalAlignment = Element.ALIGN_CENTER;
            cellHeader.Border = Rectangle.NO_BORDER;

            headerTable.AddCell(cellHeader);

            //var cellSpace = GetPdfCell("", headerTitleFont);
            //cellSpace.Border = Rectangle.NO_BORDER;

            //headerTable.AddCell(cellSpace);

            //doc.Add(headerTable);
            return headerTable;
        }
        public static float CalculatePdfPTableHeight(PdfPTable table)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.Tabloid);

                PdfWriter w = PdfWriter.GetInstance(doc, ms);
                   
                        doc.Open();

                        table.WriteSelectedRows(0, table.Rows.Count, 0, 0, w.DirectContent);

                        doc.Close();
                        return table.TotalHeight;
                    
                
            }
        }

        protected virtual void PrintSaleInvoice(Stream stream,PdfPTable headerTable, Font titleFont, Customer customer, Branch branch, Sale sale, List<SaleItemModel> saleItems, Font font, Font attributesFont)
        {
            
            var numColumns = 1;
            //print invoice info
            var invoiceInfoTable = new PdfPTable(numColumns)
            {
              //  RunDirection = GetDirection(lang),
                WidthPercentage =100f,
               
                
            };
            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 100 } }
            };
            invoiceInfoTable.SetTotalWidth(new float[] { 100 });

            invoiceInfoTable.SetWidths( widths[numColumns]);
            var cellInvoiceInfo = GetPdfCell("", font);
            cellInvoiceInfo.Phrase.Add(new Phrase(sale.CreatedOn.ToString("dd/MM/yyyy")+"   "+ sale.CreatedOn.ToString("hh:mm tt"), titleFont));
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

             cellInvoiceInfo = GetPdfCell("", font);
            cellInvoiceInfo.Phrase.Add(new Phrase("Bill No:" +sale.SaleNo, titleFont));
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            

            //cellInvoiceInfo = GetPdfCell("", font);
            //cellInvoiceInfo.Phrase.Add(new Phrase("Bill To: ", titleFont));
            //cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            //cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            //cellInvoiceInfo.Colspan = 2;
            //invoiceInfoTable.AddCell(cellInvoiceInfo);

            cellInvoiceInfo = GetPdfCell("", font);
            cellInvoiceInfo.Phrase.Add(new Phrase("Name: " + customer.Name, titleFont));
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            cellInvoiceInfo.Colspan = 2;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            //cellInvoiceInfo = GetPdfCell("", font);
            //cellInvoiceInfo.Phrase.Add(new Phrase("Address: " + customer.Address, titleFont));
            //cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            //cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            //cellInvoiceInfo.Colspan = 2;
            //invoiceInfoTable.AddCell(cellInvoiceInfo);

            cellInvoiceInfo = GetPdfCell("", font);
            cellInvoiceInfo.Phrase.Add(new Phrase("Contact: " + customer.Contact, titleFont));
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            cellInvoiceInfo.Colspan = 2;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            cellInvoiceInfo = GetPdfCell("", font);
            cellInvoiceInfo.Phrase.Add(new Phrase("Served By: " + _context.Users.Find(sale.CreatedBy)?.Username, titleFont));
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            
            //customer id & purchase date
            //cellInvoiceInfo = GetPdfCell("", font);
            //cellInvoiceInfo.Phrase.Add(new Phrase("Customer Id: " + sale.CustomerId, titleFont));
            //cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            //cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            //invoiceInfoTable.AddCell(cellInvoiceInfo);

            //cellInvoiceInfo = GetPdfCell("", font);
            //cellInvoiceInfo.Phrase.Add(new Phrase("Time: " + sale.CreatedOn.ToString("hh:mm tt"), titleFont));
            //cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_RIGHT;
            //cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            //invoiceInfoTable.AddCell(cellInvoiceInfo);

           

           


           

            


            cellInvoiceInfo.Phrase.Add(new Phrase(Environment.NewLine));
            cellInvoiceInfo.Phrase.Add(new Phrase(Environment.NewLine));

           

            //print purchase items
            var purchaseItemTable = new PdfPTable(4)
            {
               // RunDirection = GetDirection(lang),
                WidthPercentage =100f
            };
            purchaseItemTable.SetTotalWidth(new float[] { 47, 10, 20, 23 });

            var widthsOfPurchaseItemTable = new Dictionary<int, int[]>
            {
               // { 5, new[] { 5,39,13,13,30 } }
               { 4, new[] { 47,10,20,23 } }
            };
            purchaseItemTable.SetWidths(widthsOfPurchaseItemTable[4]);

            //var cellPurchaseItemTable = GetPdfCell("SL", font);
            //cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
            //purchaseItemTable.AddCell(cellPurchaseItemTable);

            var cellPurchaseItemTable = GetPdfCell("Description", titleFont);
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseItemTable.AddCell(cellPurchaseItemTable);

            cellPurchaseItemTable = GetPdfCell("Qty", titleFont);
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseItemTable.AddCell(cellPurchaseItemTable);

            cellPurchaseItemTable = GetPdfCell("MRP", titleFont);
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseItemTable.AddCell(cellPurchaseItemTable);

            cellPurchaseItemTable = GetPdfCell("Amount", titleFont);
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseItemTable.AddCell(cellPurchaseItemTable);



            var r = 0;
            foreach (var so in saleItems)
            {
                r++;

                //SL
                //cellPurchaseItemTable = GetPdfCell(r, font);
                //cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                //purchaseItemTable.AddCell(cellPurchaseItemTable);
                //ItemName
                cellPurchaseItemTable = GetPdfCell(so.Description, font);
                cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseItemTable.AddCell(cellPurchaseItemTable);

            
                //Quantity
                cellPurchaseItemTable = GetPdfCell(so.Quantity, font);
                cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseItemTable.AddCell(cellPurchaseItemTable);

                //UnitPrice
                var unitprice = so.Price;
                unitprice = Math.Round(unitprice, 2);
                cellPurchaseItemTable = GetPdfCell(unitprice, font);
                cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseItemTable.AddCell(cellPurchaseItemTable);

                //TotalPrice
                var TotalPrice = so.Amount;
                TotalPrice = Math.Round(TotalPrice, 2);
                cellPurchaseItemTable = GetPdfCell(TotalPrice, font);
                cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseItemTable.AddCell(cellPurchaseItemTable);


            }

            //
            cellPurchaseItemTable = GetPdfCell("", font);
            cellPurchaseItemTable.Border = Rectangle.NO_BORDER;
            cellPurchaseItemTable.Colspan = 2;
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseItemTable.AddCell(cellPurchaseItemTable);


            //Subtotal
            cellPurchaseItemTable = GetPdfCell("Subtotal ", titleFont);
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseItemTable.AddCell(cellPurchaseItemTable);

            //Subtotal
            cellPurchaseItemTable = GetPdfCell(sale.Subtotal, titleFont);
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseItemTable.AddCell(cellPurchaseItemTable);



            //
            cellPurchaseItemTable = GetPdfCell("", font);
            cellPurchaseItemTable.Border = Rectangle.NO_BORDER;
            cellPurchaseItemTable.Colspan = 2;
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseItemTable.AddCell(cellPurchaseItemTable);

            //Discount
            cellPurchaseItemTable = GetPdfCell("Discount ", titleFont);
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseItemTable.AddCell(cellPurchaseItemTable);

            //Discount
            cellPurchaseItemTable = GetPdfCell(sale.Discount, titleFont);
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseItemTable.AddCell(cellPurchaseItemTable);


            //
            //var redColorTextFont = FontFactory.GetFont("Times New Roman", 10, BaseColor.Red);
            cellPurchaseItemTable = GetPdfCell("", titleFont);
            cellPurchaseItemTable.Border = Rectangle.NO_BORDER;
            cellPurchaseItemTable.Colspan = 2;
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseItemTable.AddCell(cellPurchaseItemTable);

            //Grand Total
            cellPurchaseItemTable = GetPdfCell("Payable ", titleFont);
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseItemTable.AddCell(cellPurchaseItemTable);

            //Grand Total

            cellPurchaseItemTable = GetPdfCell(sale.PayableAmount, titleFont);
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseItemTable.AddCell(cellPurchaseItemTable);

            cellPurchaseItemTable = GetPdfCell("", titleFont);
            cellPurchaseItemTable.Border = Rectangle.NO_BORDER;
            cellPurchaseItemTable.Colspan = 2;
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseItemTable.AddCell(cellPurchaseItemTable);

            //Paid
            cellPurchaseItemTable = GetPdfCell("Cash ", titleFont);
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseItemTable.AddCell(cellPurchaseItemTable);

            

            cellPurchaseItemTable = GetPdfCell(sale.RecievedAmount, titleFont);
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseItemTable.AddCell(cellPurchaseItemTable);


            cellPurchaseItemTable = GetPdfCell("", font);
            cellPurchaseItemTable.Border = Rectangle.NO_BORDER;
            cellPurchaseItemTable.Colspan = 2;
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseItemTable.AddCell(cellPurchaseItemTable);

            //Due
            cellPurchaseItemTable = GetPdfCell("Change ", titleFont);
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseItemTable.AddCell(cellPurchaseItemTable);

            


            cellPurchaseItemTable = GetPdfCell(sale.ReturnAmount, titleFont);
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseItemTable.AddCell(cellPurchaseItemTable);
            



            var saleAccountInfoTable = new PdfPTable(2)
            {
         
                HorizontalAlignment = Element.ALIGN_LEFT,
                WidthPercentage = 50f
            };

            var widthsOfsaleAccountInfoTable = new Dictionary<int, int[]>
            {
               // { 5, new[] { 5,39,13,13,30 } }
               { 2, new[] { 50,50 } }
            };
            saleAccountInfoTable.SetWidths(widthsOfsaleAccountInfoTable[2]);
            saleAccountInfoTable.SetTotalWidth(new float[] { 50,50 });




            var cellsaleAccountInfoTable = GetPdfCell("", font);
          
         
            cellsaleAccountInfoTable.Phrase.Add(new Phrase(Environment.NewLine));
            cellsaleAccountInfoTable.Border = Rectangle.NO_BORDER;
            cellsaleAccountInfoTable.Phrase.Add(new Phrase(Environment.NewLine));
            cellsaleAccountInfoTable.HorizontalAlignment = Element.ALIGN_LEFT;

            cellsaleAccountInfoTable.Colspan = 2;
            saleAccountInfoTable.AddCell(cellsaleAccountInfoTable);

            //cellPurchaseItemTable.Phrase.Add(new Phrase(Environment.NewLine));
            cellsaleAccountInfoTable = GetPdfCell("", font);
            cellsaleAccountInfoTable = GetPdfCell("", font);
            cellsaleAccountInfoTable = GetPdfCell("Account Info", titleFont);
            cellsaleAccountInfoTable.Phrase.Add(new Phrase(Environment.NewLine));
            cellsaleAccountInfoTable.Phrase.Add(new Phrase("Total Due: " + sale.CustomerBalance, titleFont));
            cellsaleAccountInfoTable.Phrase.Add(new Phrase(Environment.NewLine));
            cellsaleAccountInfoTable.HorizontalAlignment = Element.ALIGN_LEFT;
            
            cellsaleAccountInfoTable.Colspan = 2;
            saleAccountInfoTable.AddCell(cellsaleAccountInfoTable);
            


            var footernumColumns = 1;
            //print invoice info
            var footerInfoTable = new PdfPTable(footernumColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f,


            };
            var footerwidths = new Dictionary<int, int[]>
            {
                { footernumColumns, new[] { 100 } }
            };
            footerInfoTable.SetTotalWidth(new float[] { 100 });

            footerInfoTable.SetWidths(widths[footernumColumns]);

            var cellfooterInfo = GetPdfCell("", font);
            cellfooterInfo.Phrase.Add(new Phrase(Environment.NewLine));
            cellfooterInfo.Phrase.Add(new Phrase(Environment.NewLine));
            cellfooterInfo.Phrase.Add(new Phrase("Sold goods are returnable with saleable condition" , titleFont));
            cellfooterInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellfooterInfo.Border = Rectangle.NO_BORDER;
            footerInfoTable.AddCell(cellfooterInfo);

             cellfooterInfo = GetPdfCell("", font);
            cellfooterInfo.Phrase.Add(new Phrase("Powered By: BUTS", titleFont));
            cellfooterInfo.HorizontalAlignment = Element.ALIGN_CENTER;
            cellfooterInfo.Border = Rectangle.NO_BORDER;
            footerInfoTable.AddCell(cellfooterInfo);



            var tableHeight= CalculatePdfPTableHeight(purchaseItemTable);
            var invoiceTableHeight = CalculatePdfPTableHeight(invoiceInfoTable);
            var headerTableHeight = CalculatePdfPTableHeight(headerTable);
            var accountinfoTableheight = CalculatePdfPTableHeight(saleAccountInfoTable);
            var footerinfoTableheight = CalculatePdfPTableHeight(footerInfoTable);

            Document doc = new Document(new iTextSharp.text.Rectangle(0, 0, 210, invoiceTableHeight+headerTableHeight+accountinfoTableheight+tableHeight+footerinfoTableheight),16.0f, 16.0f, 0, 0);
             PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //doc.SetPageSize( new iTextSharp.text.Rectangle(0, 0, 100, 150));
            doc.SetMargins(16.0f, 16.0f, 0, 0);
            doc.Add(headerTable);
            doc.Add(invoiceInfoTable);
            doc.Add(purchaseItemTable);
            doc.Add(saleAccountInfoTable);
            doc.Add(footerInfoTable);
            doc.Close();
           

           

        }
        public  byte[] AddFooterInfoForA5Page(byte[] pdf)
        {
            MemoryStream ms = new MemoryStream();
            // we create a reader for a certain document
            PdfReader reader = new PdfReader(pdf);
            // we retrieve the total number of pages
            int n = reader.NumberOfPages;
            // we retrieve the size of the first page
            Rectangle psize = reader.GetPageSize(1 );

            // step 1: creation of a document-object
            Document document = new Document(psize, 10, 10, 10, 10);
            // step 2: we create a writer that listens to the document
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            // step 3: we open the document

            document.Open();
            // step 4: we add content
            PdfContentByte cb = writer.DirectContent;

            int p = 0;
            // Console.WriteLine("There are " + n + " pages in the document.");
            for (int page = 1; page <= reader.NumberOfPages; page++)
            {
                document.NewPage();
                p++;

                PdfImportedPage importedPage = writer.GetImportedPage(reader, page);
                cb.AddTemplate(importedPage, 0, 0);

                BaseFont bf = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                BaseFont cf = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

               // cb.BeginText();
                //cb.SetFontAndSize(cf, 6);

                //cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Received By", 16, 30, 0);
                //cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Approved By", 404, 30, 0);
               // cb.EndText();

                cb.BeginText();
                cb.SetFontAndSize(bf, 9);


                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Sold goods are returnable with saleable condition",16,20,0);

               // cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Printing Date : " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"), 16, 10, 0);
                cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Powered By: BUTS", 130, 10, 0);
                cb.EndText();
            }
            // step 5: we close the document
            document.Close();
            return ms.ToArray();
        }
        public void PrintSaleInvoiceToPdf(Stream stream, Customer customer,Branch branch,Sale sale,  List<SaleItemModel> saleItems)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (sale == null)
                throw new ArgumentNullException(nameof(sale));

          //  var pageSize = PageSize.A6;

           // var doc = new Document();
      //      doc.SetMargins(16.0f, 16.0f, 0, 0);
           //  var doc = new Document(pageSize, 16.0f, 16.0f, 0, 0);
            //doc.SetPageSize(new iTextSharp.text.Rectangle(0, 0, 1, 1));

            //var pdfWriter = PdfWriter.GetInstance(doc, stream);
          //  doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 10;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 6;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 9;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //titleFont
            var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "pdf", Path.GetFileName("RobotoRegular.ttf"));
            var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var font = new Font(baseFont, 8, Font.NORMAL);

            var titleFont = font;
            titleFont.SetStyle(Font.BOLD);
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 8;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;
            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 8;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);
            

            //headers
           var headerTable= PrintInvoiceHeader( "Sale Invoice",branch, headerTitleFont, addressFont, reportTitleFont);
           
            // Report
            PrintSaleInvoice(stream,headerTable, titleFont,  customer,branch,sale,saleItems, defaultFont, attributesFont);



            //doc.Close();
        }
        protected virtual void PrintStockReport( Font titleFont, Document doc, IList<StockModel> stockReportLines, Font font, Font attributesFont, int supplierId = 0)
        {
          


            var numColumns = 5;
            var stockReportTable = new PdfPTable(numColumns)
            {
              //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 10,15,40,15,15 } }
            };

            stockReportTable.SetWidths( widths[numColumns]);

            //SL
            var cellStockReportItem = GetPdfCell("SL", font);
            cellStockReportItem.BackgroundColor = BaseColor.LightGray;
            cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            stockReportTable.AddCell(cellStockReportItem);
            //Item
           
            //Category
            cellStockReportItem = GetPdfCell("Supplier Code", font);
            cellStockReportItem.BackgroundColor = BaseColor.LightGray;
            cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            stockReportTable.AddCell(cellStockReportItem);
            //Brand
            cellStockReportItem = GetPdfCell("Product Details", font);
            cellStockReportItem.BackgroundColor = BaseColor.LightGray;
            cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            stockReportTable.AddCell(cellStockReportItem);

            //Stock Quantity
            cellStockReportItem = GetPdfCell("Qty.", font);
            cellStockReportItem.BackgroundColor = BaseColor.LightGray;
            cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            stockReportTable.AddCell(cellStockReportItem);
            //Stock Value
            cellStockReportItem = GetPdfCell("Stock Value", font);
            cellStockReportItem.BackgroundColor = BaseColor.LightGray;
            cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            stockReportTable.AddCell(cellStockReportItem);

            stockReportTable.HeaderRows = 1;
            var r = 0;
            foreach (var pl in stockReportLines)
            {
                r++;

                //SL
                cellStockReportItem = GetPdfCell(r, font);
                cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                stockReportTable.AddCell(cellStockReportItem);
                //ItemName
                

                //CategoryName
                cellStockReportItem = GetPdfCell(pl.Code, font);
                cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                stockReportTable.AddCell(cellStockReportItem);

                //BrandName
                cellStockReportItem = GetPdfCell(pl.ProductName+" " +pl.ProductPackSize+" "+pl.ProductCategory, font);
                cellStockReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                stockReportTable.AddCell(cellStockReportItem);


                //CurrentStock
                cellStockReportItem = GetPdfCell(pl.Quantity, font);
                cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                stockReportTable.AddCell(cellStockReportItem);

                //Price
                cellStockReportItem = GetPdfCell(Math.Round(pl.Value, 2), font);
                cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                stockReportTable.AddCell(cellStockReportItem);

            }
            //..................total..................................
            //SL
            cellStockReportItem = GetPdfCell("Total", font);
            cellStockReportItem.Colspan = 3;
            cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            stockReportTable.AddCell(cellStockReportItem);




            //CurrentStock
            cellStockReportItem = GetPdfCell(stockReportLines.Sum(x => x.Quantity), font);
            cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            stockReportTable.AddCell(cellStockReportItem);
            //CurrentStock
            cellStockReportItem = GetPdfCell(Math.Round(stockReportLines.Sum(x => x.Value), 2), font);
            cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            stockReportTable.AddCell(cellStockReportItem);

            ////Price
            //cellStockReportItem = GetPdfCell(Math.Ceiling(stockReportLines.Sum(x => x.PriceExcludeDiscount)), font);
            //cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            //stockReportTable.AddCell(cellStockReportItem);
            doc.Add(stockReportTable);
        }
        protected virtual void PrintOutOfStockReport(Font titleFont, Document doc, IList<StockModel> stockReportLines, Font font, Font attributesFont, int supplierId = 0)
        {



            var numColumns = 4;
            var stockReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 5,40,30,25 } }
            };

            stockReportTable.SetWidths(widths[numColumns]);

            //SL
            var cellStockReportItem = GetPdfCell("SL", font);
            cellStockReportItem.BackgroundColor = BaseColor.LightGray;
            cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            stockReportTable.AddCell(cellStockReportItem);
            //Item
            //cellStockReportItem = GetPdfCell("Branch", font);
            //cellStockReportItem.BackgroundColor = BaseColor.LightGray;
            //cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            //stockReportTable.AddCell(cellStockReportItem);
            //Category
            
            //Brand
            cellStockReportItem = GetPdfCell("Product", font);
            cellStockReportItem.BackgroundColor = BaseColor.LightGray;
            cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            stockReportTable.AddCell(cellStockReportItem);

            cellStockReportItem = GetPdfCell("Supplier", font);
            cellStockReportItem.BackgroundColor = BaseColor.LightGray;
            cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            stockReportTable.AddCell(cellStockReportItem);
            //Stock Quantity
            cellStockReportItem = GetPdfCell("Stock", font);
            cellStockReportItem.BackgroundColor = BaseColor.LightGray;
            cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            stockReportTable.AddCell(cellStockReportItem);
            //Stock Value
            //cellStockReportItem = GetPdfCell("Stock Value", font);
            //cellStockReportItem.BackgroundColor = BaseColor.LightGray;
            //cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            //stockReportTable.AddCell(cellStockReportItem);

            stockReportTable.HeaderRows = 1;
            var r = 0;
            foreach (var pl in stockReportLines)
            {
                r++;

                //SL
                cellStockReportItem = GetPdfCell(r, font);
                cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                stockReportTable.AddCell(cellStockReportItem);
                //ItemName
                //cellStockReportItem = GetPdfCell(pl.BranchName, font);
                //cellStockReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                //stockReportTable.AddCell(cellStockReportItem);

               

                //BrandName
                cellStockReportItem = GetPdfCell(pl.ProductName + " " + pl.ProductPackSize + " " + pl.ProductCategory, font);
                cellStockReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                stockReportTable.AddCell(cellStockReportItem);


                //CategoryName
                cellStockReportItem = GetPdfCell(pl.SupplierName, font);
                cellStockReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                stockReportTable.AddCell(cellStockReportItem);
                //CurrentStock
                cellStockReportItem = GetPdfCell("Out Of Stock", font);
                cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                stockReportTable.AddCell(cellStockReportItem);

                //Price
                //cellStockReportItem = GetPdfCell(Math.Round(pl.Value, 2), font);
                //cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                //stockReportTable.AddCell(cellStockReportItem);

            }
            //..................total..................................
            //SL
            //cellStockReportItem = GetPdfCell("Total", font);
            //cellStockReportItem.Colspan = 4;
            //cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            //stockReportTable.AddCell(cellStockReportItem);




            //CurrentStock
            //cellStockReportItem = GetPdfCell(stockReportLines.Sum(x => x.Quantity), font);
            //cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            //stockReportTable.AddCell(cellStockReportItem);
            //CurrentStock
            //cellStockReportItem = GetPdfCell(Math.Round(stockReportLines.Sum(x => x.Value), 2), font);
            //cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            //stockReportTable.AddCell(cellStockReportItem);

            ////Price
            //cellStockReportItem = GetPdfCell(Math.Ceiling(stockReportLines.Sum(x => x.PriceExcludeDiscount)), font);
            //cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            //stockReportTable.AddCell(cellStockReportItem);
            doc.Add(stockReportTable);
        }

        protected virtual void PrintProfitReport(Font titleFont, Document doc, IList<PLModel> profitReportLines, Font font, Font attributesFont)
        {



            var numColumns = 7;
            var profitReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 5,20,15,15,15,15,15 } }
            };

            profitReportTable.SetWidths(widths[numColumns]);

            //SL
            var cellProfitReportItem = GetPdfCell("SL", font);
            cellProfitReportItem.BackgroundColor = BaseColor.LightGray;
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            profitReportTable.AddCell(cellProfitReportItem);
            //Item
            cellProfitReportItem = GetPdfCell("Date", font);
            cellProfitReportItem.BackgroundColor = BaseColor.LightGray;
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            profitReportTable.AddCell(cellProfitReportItem);
            //Category
            cellProfitReportItem = GetPdfCell("Revenue", font);
            cellProfitReportItem.BackgroundColor = BaseColor.LightGray;
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            profitReportTable.AddCell(cellProfitReportItem);
            //Brand
            cellProfitReportItem = GetPdfCell("COGS", font);
            cellProfitReportItem.BackgroundColor = BaseColor.LightGray;
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            profitReportTable.AddCell(cellProfitReportItem);
            //Stock Quantity
            cellProfitReportItem = GetPdfCell("Expense", font);
            cellProfitReportItem.BackgroundColor = BaseColor.LightGray;
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            profitReportTable.AddCell(cellProfitReportItem);
            //Stock Quantity
            cellProfitReportItem = GetPdfCell("PL", font);
            cellProfitReportItem.BackgroundColor = BaseColor.LightGray;
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            profitReportTable.AddCell(cellProfitReportItem);

            //Stock Quantity
            cellProfitReportItem = GetPdfCell("Net PL", font);
            cellProfitReportItem.BackgroundColor = BaseColor.LightGray;
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            profitReportTable.AddCell(cellProfitReportItem);
            ////Stock Value
            //cellStockReportItem = GetPdfCell("Stock Value", font);
            //cellStockReportItem.BackgroundColor = BaseColor.LightGray;
            //cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            //stockReportTable.AddCell(cellStockReportItem);

            profitReportTable.HeaderRows = 1;
            var r = 0;
            foreach (var pl in profitReportLines)
            {
                r++;

                //SL
                cellProfitReportItem = GetPdfCell(r, font);
                cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                profitReportTable.AddCell(cellProfitReportItem);
                //ItemName
                cellProfitReportItem = GetPdfCell(pl.Date.ToString("dd/MM/yyyy"), font);
                cellProfitReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                profitReportTable.AddCell(cellProfitReportItem);

                //CategoryName
                cellProfitReportItem = GetPdfCell(pl.Revenue, font);
                cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                profitReportTable.AddCell(cellProfitReportItem);

                //BrandName
                cellProfitReportItem = GetPdfCell(pl.COGS, font);
                cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                profitReportTable.AddCell(cellProfitReportItem);

                //CurrentStock
                cellProfitReportItem = GetPdfCell(pl.Expens, font);
                cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                profitReportTable.AddCell(cellProfitReportItem);

                //CurrentStock
                cellProfitReportItem = GetPdfCell(pl.PLWithoutExpense, font);
                cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                profitReportTable.AddCell(cellProfitReportItem);
                //CurrentStock
                cellProfitReportItem = GetPdfCell(pl.PL, font);
                cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                profitReportTable.AddCell(cellProfitReportItem);

                ////Price
                //cellStockReportItem = GetPdfCell(Math.Round(pl.PriceExcludeDiscount, 2), font);
                //cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                //stockReportTable.AddCell(cellStockReportItem);

            }
            //..................total..................................
            //SL
            cellProfitReportItem = GetPdfCell("Total", font);
            cellProfitReportItem.Colspan = 2;
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            profitReportTable.AddCell(cellProfitReportItem);




            //CurrentStock
            cellProfitReportItem = GetPdfCell(profitReportLines.Sum(x => x.Revenue), font);
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            profitReportTable.AddCell(cellProfitReportItem);
            //CurrentStock
            cellProfitReportItem = GetPdfCell(profitReportLines.Sum(x => x.COGS), font);
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            profitReportTable.AddCell(cellProfitReportItem); //CurrentStock

            cellProfitReportItem = GetPdfCell(profitReportLines.Sum(x => x.Expens), font);
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            profitReportTable.AddCell(cellProfitReportItem); //CurrentStock
            cellProfitReportItem = GetPdfCell(profitReportLines.Sum(x => x.PLWithoutExpense), font);
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            profitReportTable.AddCell(cellProfitReportItem);

            cellProfitReportItem = GetPdfCell(profitReportLines.Sum(x => x.PL), font);
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            profitReportTable.AddCell(cellProfitReportItem);


            ////Price
            //cellStockReportItem = GetPdfCell(Math.Ceiling(stockReportLines.Sum(x => x.PriceExcludeDiscount)), font);
            //cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            //stockReportTable.AddCell(cellStockReportItem);
            doc.Add(profitReportTable);
        }

        protected virtual void PrintBalanceReport(Font titleFont, Document doc, IList<BalanceReportLine> balanceReportLines, Font font, Font attributesFont)
        {
            var numColumns = 8;
            var profitReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 5,20,12,12,12,12,12,15 } }
            };

            profitReportTable.SetWidths(widths[numColumns]);

            //SL
            var cellProfitReportItem = GetPdfCell("SL", font);
            cellProfitReportItem.BackgroundColor = BaseColor.LightGray;
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            profitReportTable.AddCell(cellProfitReportItem);
            //Item
            cellProfitReportItem = GetPdfCell("Date", font);
            cellProfitReportItem.BackgroundColor = BaseColor.LightGray;
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            profitReportTable.AddCell(cellProfitReportItem);
            //Category
            cellProfitReportItem = GetPdfCell("Cash Sales", font);
            cellProfitReportItem.BackgroundColor = BaseColor.LightGray;
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            profitReportTable.AddCell(cellProfitReportItem);
            //Brand
            cellProfitReportItem = GetPdfCell("Customer Due Collection", font);
            cellProfitReportItem.BackgroundColor = BaseColor.LightGray;
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            profitReportTable.AddCell(cellProfitReportItem);
            //Stock Quantity
            cellProfitReportItem = GetPdfCell("Expense", font);
            cellProfitReportItem.BackgroundColor = BaseColor.LightGray;
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            profitReportTable.AddCell(cellProfitReportItem);
            //Stock Quantity
            cellProfitReportItem = GetPdfCell("Sales Return", font);
            cellProfitReportItem.BackgroundColor = BaseColor.LightGray;
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            profitReportTable.AddCell(cellProfitReportItem);

            //Stock Quantity
            cellProfitReportItem = GetPdfCell("Supplier Payment", font);
            cellProfitReportItem.BackgroundColor = BaseColor.LightGray;
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            profitReportTable.AddCell(cellProfitReportItem);

            //Stock Quantity
            cellProfitReportItem = GetPdfCell("Balance", font);
            cellProfitReportItem.BackgroundColor = BaseColor.LightGray;
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            profitReportTable.AddCell(cellProfitReportItem);
            ////Stock Value
            //cellStockReportItem = GetPdfCell("Stock Value", font);
            //cellStockReportItem.BackgroundColor = BaseColor.LightGray;
            //cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            //stockReportTable.AddCell(cellStockReportItem);

            profitReportTable.HeaderRows = 1;
            var r = 0;
            foreach (var pl in balanceReportLines)
            {
                r++;

                //SL
                cellProfitReportItem = GetPdfCell(r, font);
                cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                profitReportTable.AddCell(cellProfitReportItem);
                //ItemName
                cellProfitReportItem = GetPdfCell(pl.Date.ToString("dd/MM/yyyy"), font);
                cellProfitReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                profitReportTable.AddCell(cellProfitReportItem);

                //CategoryName
                cellProfitReportItem = GetPdfCell(pl.CashSales, font);
                cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                profitReportTable.AddCell(cellProfitReportItem);

                //BrandName
                cellProfitReportItem = GetPdfCell(pl.CustomerDueCollection, font);
                cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                profitReportTable.AddCell(cellProfitReportItem);

                //CurrentStock
                cellProfitReportItem = GetPdfCell(pl.Expens, font);
                cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                profitReportTable.AddCell(cellProfitReportItem);

                //CurrentStock
                cellProfitReportItem = GetPdfCell(pl.SalesReturn, font);
                cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                profitReportTable.AddCell(cellProfitReportItem);
                //CurrentStock
                cellProfitReportItem = GetPdfCell(pl.SupplierPayment, font);
                cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                profitReportTable.AddCell(cellProfitReportItem);

                //CurrentStock
                cellProfitReportItem = GetPdfCell(pl.Balance, font);
                cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                profitReportTable.AddCell(cellProfitReportItem);

                ////Price
                //cellStockReportItem = GetPdfCell(Math.Round(pl.PriceExcludeDiscount, 2), font);
                //cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                //stockReportTable.AddCell(cellStockReportItem);

            }
            //..................total..................................
            //SL
            cellProfitReportItem = GetPdfCell("Total", font);
            cellProfitReportItem.Colspan = 2;
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            profitReportTable.AddCell(cellProfitReportItem);




            //CurrentStock
            cellProfitReportItem = GetPdfCell(balanceReportLines.Sum(x => x.CashSales), font);
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            profitReportTable.AddCell(cellProfitReportItem);
            //CurrentStock
            cellProfitReportItem = GetPdfCell(balanceReportLines.Sum(x => x.CustomerDueCollection), font);
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            profitReportTable.AddCell(cellProfitReportItem); //CurrentStock

            cellProfitReportItem = GetPdfCell(balanceReportLines.Sum(x => x.Expens), font);
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            profitReportTable.AddCell(cellProfitReportItem); //CurrentStock
            cellProfitReportItem = GetPdfCell(balanceReportLines.Sum(x => x.SalesReturn), font);
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            profitReportTable.AddCell(cellProfitReportItem);

            cellProfitReportItem = GetPdfCell(balanceReportLines.Sum(x => x.SupplierPayment), font);
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            profitReportTable.AddCell(cellProfitReportItem);

            cellProfitReportItem = GetPdfCell(balanceReportLines.Sum(x => x.Balance), font);
            cellProfitReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            profitReportTable.AddCell(cellProfitReportItem);


            ////Price
            //cellStockReportItem = GetPdfCell(Math.Ceiling(stockReportLines.Sum(x => x.PriceExcludeDiscount)), font);
            //cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            //stockReportTable.AddCell(cellStockReportItem);
            doc.Add(profitReportTable);
        }
        protected virtual void PrintSupplierLedgerReport(Font titleFont, Document doc, IList<SupplierLedgerReportLine> supplierLedgerReportLines,decimal? prevBalance, Font font, Font attributesFont)
        {



            var numColumns = 6;
            var supplierLedgerReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 5,25,15,15,20,20 } }
            };

            supplierLedgerReportTable.SetWidths(widths[numColumns]);

            //SL
            var cellSupplierLedgerReportItem = GetPdfCell("SL", font);
            cellSupplierLedgerReportItem.BackgroundColor = BaseColor.LightGray;
            cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);
            //Item
            cellSupplierLedgerReportItem = GetPdfCell("DATE", font);
            cellSupplierLedgerReportItem.BackgroundColor = BaseColor.LightGray;
            cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);
            //Category
            cellSupplierLedgerReportItem = GetPdfCell("REF NO", font);
            cellSupplierLedgerReportItem.BackgroundColor = BaseColor.LightGray;
            cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);
            //Brand
            cellSupplierLedgerReportItem = GetPdfCell("DEBIT", font);
            cellSupplierLedgerReportItem.BackgroundColor = BaseColor.LightGray;
            cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);
            //Stock Quantity
            cellSupplierLedgerReportItem = GetPdfCell("CREDIT", font);
            cellSupplierLedgerReportItem.BackgroundColor = BaseColor.LightGray;
            cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);
            //Stock Quantity
            cellSupplierLedgerReportItem = GetPdfCell("BALANCE", font);
            cellSupplierLedgerReportItem.BackgroundColor = BaseColor.LightGray;
            cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);
            ////Stock Value
            //cellStockReportItem = GetPdfCell("Stock Value", font);
            //cellStockReportItem.BackgroundColor = BaseColor.LightGray;
            //cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            //stockReportTable.AddCell(cellStockReportItem);

            supplierLedgerReportTable.HeaderRows = 1;
            var r = 0;
            //..................Previous Balance..................................
            //Previous Balance
            cellSupplierLedgerReportItem = GetPdfCell("Previous Balance", titleFont);
            cellSupplierLedgerReportItem.Colspan = 5;
            cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);

            //Balance
            cellSupplierLedgerReportItem = GetPdfCell(prevBalance, titleFont);
            cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);
            

            foreach (var sl in supplierLedgerReportLines)
            {
                r++;

                //SL
                cellSupplierLedgerReportItem = GetPdfCell(r, font);
                cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);
                //ItemName
                cellSupplierLedgerReportItem = GetPdfCell(sl.Date.ToString("dd/MM/yyyy"), font);
                cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);

                //CategoryName
                cellSupplierLedgerReportItem = GetPdfCell(sl.RefNo, font);
                cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);

                //BrandName
                cellSupplierLedgerReportItem = GetPdfCell(sl.Debit, font);
                cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);

                //CurrentStock
                cellSupplierLedgerReportItem = GetPdfCell(sl.Credit, font);
                cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);
                //CurrentStock
                cellSupplierLedgerReportItem = GetPdfCell(sl.Balance, font);
                cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);

                ////Price
                //cellStockReportItem = GetPdfCell(Math.Round(pl.PriceExcludeDiscount, 2), font);
                //cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                //stockReportTable.AddCell(cellStockReportItem);

            }
            //..................total..................................
            //SL
            cellSupplierLedgerReportItem = GetPdfCell("Total", font);
            cellSupplierLedgerReportItem.Colspan = 2;
            cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);




           

            ////Price
            //cellStockReportItem = GetPdfCell(Math.Ceiling(stockReportLines.Sum(x => x.PriceExcludeDiscount)), font);
            //cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            //stockReportTable.AddCell(cellStockReportItem);
            doc.Add(supplierLedgerReportTable);
        }
        protected virtual void PrintCustomerLedgerReport(Font titleFont, Document doc, IList<CustomerLedgerReportLine> customerLedgerReportLines,decimal? prevBalance, Font font, Font attributesFont)
        {



            var numColumns = 6;
            var supplierLedgerReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 5,25,15,15,20,20 } }
            };

            supplierLedgerReportTable.SetWidths(widths[numColumns]);

            //SL
            var cellSupplierLedgerReportItem = GetPdfCell("SL", font);
            cellSupplierLedgerReportItem.BackgroundColor = BaseColor.LightGray;
            cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);
            //Item
            cellSupplierLedgerReportItem = GetPdfCell("DATE", font);
            cellSupplierLedgerReportItem.BackgroundColor = BaseColor.LightGray;
            cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);
            //Category
            cellSupplierLedgerReportItem = GetPdfCell("REF NO", font);
            cellSupplierLedgerReportItem.BackgroundColor = BaseColor.LightGray;
            cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);
            //Brand
            cellSupplierLedgerReportItem = GetPdfCell("DEBIT", font);
            cellSupplierLedgerReportItem.BackgroundColor = BaseColor.LightGray;
            cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);
            //Stock Quantity
            cellSupplierLedgerReportItem = GetPdfCell("CREDIT", font);
            cellSupplierLedgerReportItem.BackgroundColor = BaseColor.LightGray;
            cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);
            //Stock Quantity
            cellSupplierLedgerReportItem = GetPdfCell("BALANCE", font);
            cellSupplierLedgerReportItem.BackgroundColor = BaseColor.LightGray;
            cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);
            ////Stock Value
            //cellStockReportItem = GetPdfCell("Stock Value", font);
            //cellStockReportItem.BackgroundColor = BaseColor.LightGray;
            //cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            //stockReportTable.AddCell(cellStockReportItem);

            supplierLedgerReportTable.HeaderRows = 1;
            var r = 0;
            //..................Previous Balance..................................
            //Previous Balance
            cellSupplierLedgerReportItem = GetPdfCell("Previous Balance", titleFont);
            cellSupplierLedgerReportItem.Colspan = 5;
            cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);

            //Balance
            cellSupplierLedgerReportItem = GetPdfCell(prevBalance, titleFont);
            cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);
            foreach (var sl in customerLedgerReportLines)
            {
                r++;

                //SL
                cellSupplierLedgerReportItem = GetPdfCell(r, font);
                cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);
                //ItemName
                cellSupplierLedgerReportItem = GetPdfCell(sl.Date.ToString("dd/MM/yyyy"), font);
                cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);

                //CategoryName
                cellSupplierLedgerReportItem = GetPdfCell(sl.RefNo, font);
                cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);

                //BrandName
                cellSupplierLedgerReportItem = GetPdfCell(sl.Debit, font);
                cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);

                //CurrentStock
                cellSupplierLedgerReportItem = GetPdfCell(sl.Credit, font);
                cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);
                //CurrentStock
                cellSupplierLedgerReportItem = GetPdfCell(sl.Balance, font);
                cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);

                ////Price
                //cellStockReportItem = GetPdfCell(Math.Round(pl.PriceExcludeDiscount, 2), font);
                //cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                //stockReportTable.AddCell(cellStockReportItem);

            }
            //..................total..................................
            //SL
            cellSupplierLedgerReportItem = GetPdfCell("Total", font);
            cellSupplierLedgerReportItem.Colspan = 2;
            cellSupplierLedgerReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            supplierLedgerReportTable.AddCell(cellSupplierLedgerReportItem);






            ////Price
            //cellStockReportItem = GetPdfCell(Math.Ceiling(stockReportLines.Sum(x => x.PriceExcludeDiscount)), font);
            //cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            //stockReportTable.AddCell(cellStockReportItem);
            doc.Add(supplierLedgerReportTable);
        }
        protected virtual void PrintExpenseReport(Font titleFont, Document doc, IList<ExpenseViewModel> expenseReportLines, Font font, Font attributesFont, int supplierId = 0)
        {



            var numColumns = 5;
            var expenseReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 7,13,20,35,25 } }
            };

            expenseReportTable.SetWidths(widths[numColumns]);

            //SL
            var cellExpenseReportItem = GetPdfCell("SL", font);
            cellExpenseReportItem.BackgroundColor = BaseColor.LightGray;
            cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            expenseReportTable.AddCell(cellExpenseReportItem);


            cellExpenseReportItem = GetPdfCell("Date", font);
            cellExpenseReportItem.BackgroundColor = BaseColor.LightGray;
            cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            expenseReportTable.AddCell(cellExpenseReportItem);
            //Item
            cellExpenseReportItem = GetPdfCell("Expense Type", font);
            cellExpenseReportItem.BackgroundColor = BaseColor.LightGray;
            cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            expenseReportTable.AddCell(cellExpenseReportItem);
            //Category
            //cellExpenseReportItem = GetPdfCell("Branch", font);
            //cellExpenseReportItem.BackgroundColor = BaseColor.LightGray;
            //cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            //expenseReportTable.AddCell(cellExpenseReportItem);
            //Brand
            cellExpenseReportItem = GetPdfCell("Description", font);
            cellExpenseReportItem.BackgroundColor = BaseColor.LightGray;
            cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            expenseReportTable.AddCell(cellExpenseReportItem);
            //Expense Quantity
            cellExpenseReportItem = GetPdfCell("Amount", font);
            cellExpenseReportItem.BackgroundColor = BaseColor.LightGray;
            cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            expenseReportTable.AddCell(cellExpenseReportItem);
            ////Expense Value
            //cellExpenseReportItem = GetPdfCell("Expense Value", font);
            //cellExpenseReportItem.BackgroundColor = BaseColor.LightGray;
            //cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            //expenseReportTable.AddCell(cellExpenseReportItem);

            expenseReportTable.HeaderRows = 1;
            var r = 0;
            foreach (var pl in expenseReportLines)
            {
                r++;

                //SL
                cellExpenseReportItem = GetPdfCell(r, font);
                cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                expenseReportTable.AddCell(cellExpenseReportItem);

                //Entry Date
                cellExpenseReportItem = GetPdfCell(pl.EntryDate.ToString("dd/MM/yyyy"), font);

                cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                expenseReportTable.AddCell(cellExpenseReportItem);



                //ItemName
                cellExpenseReportItem = GetPdfCell(pl.ExpenseType.Name, font);
                
                cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                expenseReportTable.AddCell(cellExpenseReportItem);

                //CategoryName
                //cellExpenseReportItem = GetPdfCell(pl.Branch, font);
                //cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                //expenseReportTable.AddCell(cellExpenseReportItem);

                //BrandName
                cellExpenseReportItem = GetPdfCell(pl.Remark, font);
                cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                expenseReportTable.AddCell(cellExpenseReportItem);

                //CurrentExpense
                cellExpenseReportItem = GetPdfCell(pl.Amount, font);
                cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                expenseReportTable.AddCell(cellExpenseReportItem);

                ////Price
                //cellExpenseReportItem = GetPdfCell(Math.Round(pl.PriceExcludeDiscount, 2), font);
                //cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                //expenseReportTable.AddCell(cellExpenseReportItem);

            }
            //..................total..................................
            //SL
            cellExpenseReportItem = GetPdfCell("Total", font);
            cellExpenseReportItem.Colspan = 4;
            cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            expenseReportTable.AddCell(cellExpenseReportItem);




            //CurrentExpense
            cellExpenseReportItem = GetPdfCell(expenseReportLines.Sum(x => x.Amount), font);
            cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            expenseReportTable.AddCell(cellExpenseReportItem);

            ////Price
            //cellExpenseReportItem = GetPdfCell(Math.Ceiling(expenseReportLines.Sum(x => x.PriceExcludeDiscount)), font);
            //cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            //expenseReportTable.AddCell(cellExpenseReportItem);
            doc.Add(expenseReportTable);
        }
        protected virtual void PrintPurchaseReport(Font titleFont, Document doc, IList<PurchaseViewModel> purchaseReportLines, Font font, Font attributesFont, int supplierId = 0)
        {



            var numColumns = 9;
            var purchaseReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 4,10,11,25,10,10,10,10,10 } }
            };

            purchaseReportTable.SetWidths(widths[numColumns]);

            //SL
            var cellPurchaseReportItem = GetPdfCell("SL", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell("Pur.No", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            cellPurchaseReportItem = GetPdfCell("Date", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Category
            cellPurchaseReportItem = GetPdfCell("Supplier Name", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Brand
            cellPurchaseReportItem = GetPdfCell("Ref.No", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Stock Quantity
            



            cellPurchaseReportItem = GetPdfCell("Amount", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            cellPurchaseReportItem = GetPdfCell("Discount", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            cellPurchaseReportItem = GetPdfCell("Ext.Cost", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            cellPurchaseReportItem = GetPdfCell("NetAmount", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            ////Stock Value
            //cellStockReportItem = GetPdfCell("Stock Value", font);
            //cellStockReportItem.BackgroundColor = BaseColor.LightGray;
            //cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            //stockReportTable.AddCell(cellStockReportItem);

            purchaseReportTable.HeaderRows = 1;
            var r = 0;
            foreach (var pl in purchaseReportLines)
            {
                r++;

                //SL
                cellPurchaseReportItem = GetPdfCell(r, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //ItemName
                cellPurchaseReportItem = GetPdfCell(pl.PurchaseNo, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);


                cellPurchaseReportItem = GetPdfCell(pl.PurchaseDate.ToString("dd/MM/yyyy"), font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                //CategoryName
                cellPurchaseReportItem = GetPdfCell(pl.SupplierName, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                //BrandName
                cellPurchaseReportItem = GetPdfCell(pl.ReferenceNo, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                //CurrentStock
               

               


                //cellPurchaseReportItem = GetPdfCell(pl.TotalValue, font);
                //cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                //purchaseReportTable.AddCell(cellPurchaseReportItem);

                //cellPurchaseReportItem = GetPdfCell(pl.TotalVat, font);
                //cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                //purchaseReportTable.AddCell(cellPurchaseReportItem);

                cellPurchaseReportItem = GetPdfCell(pl.Subtotal, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);


                cellPurchaseReportItem = GetPdfCell(pl.Discount, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);


                cellPurchaseReportItem = GetPdfCell(pl.AdditionalCost, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                cellPurchaseReportItem = GetPdfCell(pl.NetAmount, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //CurrentStock
                //cellPurchaseReportItem = GetPdfCell(pl.Subtotal, font);
                //cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                //purchaseReportTable.AddCell(cellPurchaseReportItem);

                ////Price
                //cellStockReportItem = GetPdfCell(Math.Round(pl.PriceExcludeDiscount, 2), font);
                //cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                //stockReportTable.AddCell(cellStockReportItem);

            }
            //..................total..................................
            //SL
            cellPurchaseReportItem = GetPdfCell("Total", font);
            cellPurchaseReportItem.Colspan = 5;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(purchaseReportLines.Sum(x => x.Subtotal), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(purchaseReportLines.Sum(x => x.Discount), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(purchaseReportLines.Sum(x => x.AdditionalCost), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(purchaseReportLines.Sum(x => x.NetAmount), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            ////Price
            //cellStockReportItem = GetPdfCell(Math.Ceiling(stockReportLines.Sum(x => x.PriceExcludeDiscount)), font);
            //cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            //stockReportTable.AddCell(cellStockReportItem);
            doc.Add(purchaseReportTable);
        }
        protected virtual void PrintBalanceSheetReport(Font titleFont, Document doc, IList<AccountHeadWithBalanceViewModel> balanceSheetReportLines, Font font, Font attributesFont, decimal netIncome = 0)
        {
            var redColorFont = GetFont();
            redColorFont.Color = new BaseColor(113, 18, 26, 255);
            var blueColorFont = GetFont();
            blueColorFont.Color = BaseColor.Blue; 
            var magentaColorFont = GetFont();
            magentaColorFont.Color = BaseColor.Magenta;
            var pinkColorFont = GetFont();
            pinkColorFont.Color = BaseColor.Pink;
            var blackColorFont = GetFont();
            blackColorFont.Color = BaseColor.Black;


            var numColumns = 6;
            var purchaseReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] {25,15,15,15,15,15 } }
            };

            purchaseReportTable.SetWidths(widths[numColumns]);

            //headline
            //SL
           
            var cellPurchaseReportItem = GetPdfCell("A/C Name", font);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell("", font);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);


            purchaseReportTable.HeaderRows = 1;
            foreach (var pl in balanceSheetReportLines)
            {
               

                
                //ItemName
               
                switch (pl.Level)
                {
                    case 1:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, blueColorFont);
                        break;
                    case 2:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, magentaColorFont);
                        break;
                    case 3:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, redColorFont);
                        break;
                    case 4:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, blackColorFont);
                        break;
                    case 5:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, blackColorFont);
                        break;
                    default:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, blackColorFont);
                        break;
                }
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);


                cellPurchaseReportItem = GetPdfCell(pl.Balance5==0M?"":pl.Balance5.ToString(), font);
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                //CategoryName
                cellPurchaseReportItem = GetPdfCell(pl.Balance4 == 0M ? "" : pl.Balance4.ToString(), font);
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                //BrandName
                cellPurchaseReportItem = GetPdfCell(pl.Balance3 == 0M ? "" : pl.Balance3.ToString(), font);
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

               

                cellPurchaseReportItem = GetPdfCell(pl.Balance2 == 0M ? "" : pl.Balance2.ToString(), font);
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);


                cellPurchaseReportItem = GetPdfCell(pl.Balance1 == 0M ? "" : pl.Balance1.ToString(), font);
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);


               
               
                

            }
            //..................total..................................
            //SL
            cellPurchaseReportItem = GetPdfCell("Net Income/Loss", font);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(netIncome, font);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            

            
            doc.Add(purchaseReportTable);
        }
        protected virtual void PrintIncomeStatementReport(Font titleFont, Document doc, IList<AccountHeadWithBalanceViewModel> balanceSheetReportLines, Font font, Font attributesFont, decimal netIncome = 0)
        {
            var redColorFont = GetFont();
            redColorFont.Color = new BaseColor(113, 18, 26, 255);
            var blueColorFont = GetFont();
            blueColorFont.Color = BaseColor.Blue;
            var magentaColorFont = GetFont();
            magentaColorFont.Color = BaseColor.Magenta;
            var pinkColorFont = GetFont();
            pinkColorFont.Color = BaseColor.Pink;
            var blackColorFont = GetFont();
            blackColorFont.Color = BaseColor.Black;


            var numColumns = 6;
            var purchaseReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] {25,15,15,15,15,15 } }
            };

            purchaseReportTable.SetWidths(widths[numColumns]);

            //headline
            //SL

            var cellPurchaseReportItem = GetPdfCell("A/C Name", font);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell("", font);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);


            purchaseReportTable.HeaderRows = 1;
            foreach (var pl in balanceSheetReportLines)
            {



                //ItemName

                switch (pl.Level)
                {
                    case 1:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, blueColorFont);
                        break;
                    case 2:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, magentaColorFont);
                        break;
                    case 3:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, redColorFont);
                        break;
                    case 4:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, blackColorFont);
                        break;
                    case 5:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, blackColorFont);
                        break;
                    default:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, blackColorFont);
                        break;
                }
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);


                cellPurchaseReportItem = GetPdfCell(pl.Balance5 == 0M ? "" : pl.Balance5.ToString(), font);
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                //CategoryName
                cellPurchaseReportItem = GetPdfCell(pl.Balance4 == 0M ? "" : pl.Balance4.ToString(), font);
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                //BrandName
                cellPurchaseReportItem = GetPdfCell(pl.Balance3 == 0M ? "" : pl.Balance3.ToString(), font);
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);



                cellPurchaseReportItem = GetPdfCell(pl.Balance2 == 0M ? "" : pl.Balance2.ToString(), font);
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);


                cellPurchaseReportItem = GetPdfCell(pl.Balance1 == 0M ? "" : pl.Balance1.ToString(), font);
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);


            }
            //..................total..................................
            //SL
            cellPurchaseReportItem = GetPdfCell("Net Income/Loss", font);
           // cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 3;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(netIncome, font);
        //    cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 3;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);




            doc.Add(purchaseReportTable);
        }
        protected virtual void PrintTrialBalanceReport(Font titleFont, Document doc, IList<TrialBalanceViewModel> trialBalanceReportLines, Font font, Font attributesFont, int supplierId = 0)
        {
            var redColorFont = GetFont();
            redColorFont.Color = new BaseColor(113, 18, 26, 255);
            var blueColorFont = GetFont();
            blueColorFont.Color = BaseColor.Blue;
            var magentaColorFont = GetFont();
            magentaColorFont.Color = BaseColor.Magenta;
            var pinkColorFont = GetFont();
            pinkColorFont.Color = BaseColor.Pink;
            var blackColorFont = GetFont();
            blackColorFont.Color = BaseColor.Black;


            var numColumns = 8;
            var purchaseReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 7,21,12,12,12,12,12,12 } }
            };

            purchaseReportTable.SetWidths(widths[numColumns]);

            //headline
            //SL
            var cellPurchaseReportItem = GetPdfCell("", font);
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell("Opening", font);
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);


            //Category
            cellPurchaseReportItem = GetPdfCell("Transaction for the period", font);
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Brand
            cellPurchaseReportItem = GetPdfCell("Balance", font);
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);


            //table header
            //SL
            cellPurchaseReportItem = GetPdfCell("A/C Code", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell("A/C Name", font);

            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);


            //Category
            cellPurchaseReportItem = GetPdfCell("Debit", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Brand
            cellPurchaseReportItem = GetPdfCell("Credit", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Stock Quantity




            cellPurchaseReportItem = GetPdfCell("Debit", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            cellPurchaseReportItem = GetPdfCell("Credit", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            cellPurchaseReportItem = GetPdfCell("Debit", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            cellPurchaseReportItem = GetPdfCell("Credit", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);


            purchaseReportTable.HeaderRows = 1;
            foreach (var pl in trialBalanceReportLines)
            {


                //SL
                cellPurchaseReportItem = GetPdfCell(pl.Code, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //ItemName

                switch (pl.Level)
                {
                    case 1:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, blueColorFont);
                        break;
                    case 2:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, magentaColorFont);
                        break;
                    case 3:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, redColorFont);
                        break;
                    case 4:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, blackColorFont);
                        break;
                    case 5:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, blackColorFont);
                        break;
                    default:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, blackColorFont);
                        break;
                }
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);


                cellPurchaseReportItem = GetPdfCell(pl.OpeningDebit, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                //CategoryName
                cellPurchaseReportItem = GetPdfCell(pl.OpeningCredit, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                //BrandName
                cellPurchaseReportItem = GetPdfCell(pl.ThisPeriodDebit, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);



                cellPurchaseReportItem = GetPdfCell(pl.ThisPeriodCredit, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);


                cellPurchaseReportItem = GetPdfCell(pl.BalanceDebit, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);


                cellPurchaseReportItem = GetPdfCell(pl.BalanceCredit, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);




            }
            //..................total..................................
            var levelOneTrialBalanceReportLines = trialBalanceReportLines.Where(x => x.Level == 1);
            //SL
            cellPurchaseReportItem = GetPdfCell("Total", font);
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(levelOneTrialBalanceReportLines.Where(x=>x.Level==1).Sum(x => x.OpeningDebit), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(levelOneTrialBalanceReportLines.Sum(x => x.OpeningCredit), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(levelOneTrialBalanceReportLines.Sum(x => x.ThisPeriodDebit), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(levelOneTrialBalanceReportLines.Sum(x => x.ThisPeriodCredit), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(levelOneTrialBalanceReportLines.Sum(x => x.BalanceDebit), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(levelOneTrialBalanceReportLines.Sum(x => x.BalanceCredit), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);


            doc.Add(purchaseReportTable);
        }

        protected virtual void PrintSubsidiaryLedgerReport(Font titleFont, Document doc, IList<SubsidiaryLedgerViewModel> subsidiaryLedgerReportLines, Font font, Font attributesFont, int supplierId = 0)
        {
            var redColorFont = GetFont();
            redColorFont.Color = new BaseColor(113, 18, 26, 255);
            redColorFont.Size = 12;
            var blueColorFont = GetFont();
            blueColorFont.Color = BaseColor.Blue;
            var magentaColorFont = GetFont();
            magentaColorFont.Color = BaseColor.Magenta;
            var pinkColorFont = GetFont();
            pinkColorFont.Color = BaseColor.Pink;
            var blackColorFont = GetFont();
            blackColorFont.Color = BaseColor.Black;


            var numColumns = 6;
            var purchaseReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] {15,25,15,15,15,15 } }
            };

            purchaseReportTable.SetWidths(widths[numColumns]);

            //headline
            //SL
            var cellPurchaseReportItem = GetPdfCell("", font);
            cellPurchaseReportItem.Colspan = 1;
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell("", font);
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);


            //Category
            cellPurchaseReportItem = GetPdfCell("Transaction for the period", font);
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Brand
            cellPurchaseReportItem = GetPdfCell("", font);
            cellPurchaseReportItem.Colspan = 1;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);


            //table header
            //SL
            cellPurchaseReportItem = GetPdfCell("VNumber", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell("Date", font);

            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);


            //Category
            cellPurchaseReportItem = GetPdfCell("Particular", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
           

            cellPurchaseReportItem = GetPdfCell("Debit", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            cellPurchaseReportItem = GetPdfCell("Credit", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            cellPurchaseReportItem = GetPdfCell("Balance", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

          

            purchaseReportTable.HeaderRows = 1;
            foreach (var pl in subsidiaryLedgerReportLines)
            {


                //SL
                cellPurchaseReportItem = GetPdfCell(pl.Vnumber, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //ItemName
                cellPurchaseReportItem = GetPdfCell(pl.VoucherDate.ToString("dd/MM/yyyy"), font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                //CategoryName
                cellPurchaseReportItem = GetPdfCell(pl.Particular, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                //BrandName
                cellPurchaseReportItem = GetPdfCell(pl.Debit, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);



                cellPurchaseReportItem = GetPdfCell(pl.Credit, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);


                cellPurchaseReportItem = GetPdfCell(pl.Balance, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);






            }
            //..................total..................................
            var totalDebit = subsidiaryLedgerReportLines.Sum(x => x.Debit);
            var totalCredit = subsidiaryLedgerReportLines.Sum(x => x.Credit);

            //SL
            cellPurchaseReportItem = GetPdfCell("Total Transaction", redColorFont);
            cellPurchaseReportItem.Colspan = 3;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

           
            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(totalDebit, redColorFont);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(totalCredit, redColorFont);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(Math.Abs(totalDebit-totalCredit) , redColorFont);
            cellPurchaseReportItem.Colspan = 1;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            


            doc.Add(purchaseReportTable);
        }


        protected virtual void PrintSaleReport(Font titleFont, Document doc, IList<SaleViewModel> saleReportLines, Font font, Font attributesFont, DateTime? fromDate, DateTime? toDate,Branch branch)
        {



            var numColumns = 10;
            var saleReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 5,10,15,13,12,9,8,10,11,7 } }
            };

            saleReportTable.SetWidths(widths[numColumns]);

            //SL
            var cellSaleReportItem = GetPdfCell("SL", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Item
            cellSaleReportItem = GetPdfCell("Invoice No.", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Category
            cellSaleReportItem = GetPdfCell("Customer Name", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Brand
            cellSaleReportItem = GetPdfCell("Branch Code", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);

            cellSaleReportItem = GetPdfCell("SaleDate", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Stock Quantity
            cellSaleReportItem = GetPdfCell("Subtotal", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);



            cellSaleReportItem = GetPdfCell("Discount", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);

           

            cellSaleReportItem = GetPdfCell("Payable", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);

            cellSaleReportItem = GetPdfCell("Paid", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);

            cellSaleReportItem = GetPdfCell("Due", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);


            saleReportTable.HeaderRows = 1;
            var r = 0;
            foreach (var pl in saleReportLines)
            {
                r++;

                //SL
                cellSaleReportItem = GetPdfCell(r, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                saleReportTable.AddCell(cellSaleReportItem);

                cellSaleReportItem = GetPdfCell(pl.SaleNo, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);
                //ItemName
                cellSaleReportItem = GetPdfCell(pl.CustomerName, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                saleReportTable.AddCell(cellSaleReportItem);

                //CategoryName
                cellSaleReportItem = GetPdfCell(pl.BranchCode, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                saleReportTable.AddCell(cellSaleReportItem);

                cellSaleReportItem = GetPdfCell(pl.SaleDate.ToString("dd/MM/yyyy"), font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);



                //BrandName
                cellSaleReportItem = GetPdfCell(pl.Subtotal, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                saleReportTable.AddCell(cellSaleReportItem);

                //CurrentStock
                cellSaleReportItem = GetPdfCell(pl.Discount, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);

              
                cellSaleReportItem = GetPdfCell(pl.PayableAmount, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);

                cellSaleReportItem = GetPdfCell(pl.PaidAmount, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);

                cellSaleReportItem = GetPdfCell(pl.DueAmount, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);

               

            }
            //..................total..................................
            //SL
            cellSaleReportItem = GetPdfCell("Total", font);
            cellSaleReportItem.Colspan = 5;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);

            //CurrentStock
            cellSaleReportItem = GetPdfCell(saleReportLines.Sum(x => x.Subtotal), font);
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);

            //CurrentStock
            cellSaleReportItem = GetPdfCell(saleReportLines.Sum(x => x.Discount), font);
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);

            //CurrentStock
            cellSaleReportItem = GetPdfCell(saleReportLines.Sum(x => x.PayableAmount), font);
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);

            cellSaleReportItem = GetPdfCell(saleReportLines.Sum(x => x.PaidAmount), font);
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);

            cellSaleReportItem = GetPdfCell(saleReportLines.Sum(x => x.DueAmount), font);
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);

            if(fromDate.HasValue && toDate.HasValue && branch!=null)
            {
                var saleReturnAmount = _commonService.GetSaleReturnTotal(fromDate.Value.Date, toDate.Value.Date, branch.Id);
                //..................total sale return..................................
                //SL
                cellSaleReportItem = GetPdfCell("Sale Return", font);
                cellSaleReportItem.Colspan = 7;
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);

                //CurrentStock
                cellSaleReportItem = GetPdfCell(saleReturnAmount, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);
                //CurrentStock
                cellSaleReportItem = GetPdfCell("", font);
                cellSaleReportItem.Colspan = 2;
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);

                //..................net sale..................................
                //SL
                cellSaleReportItem = GetPdfCell("Net Sale", font);
                cellSaleReportItem.Colspan = 7;
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);

                //CurrentStock
                cellSaleReportItem = GetPdfCell(saleReportLines.Sum(x => x.PayableAmount)-saleReturnAmount, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);
                //CurrentStock
                cellSaleReportItem = GetPdfCell("", font);
                cellSaleReportItem.Colspan = 2;
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);

            }



            doc.Add(saleReportTable);
        }

        protected virtual void PrintItemWiseSaleProfitLossReport(Font titleFont, Document doc, IList<ItemSaleProfitLossViewModel> saleReportLines, Font font, Font attributesFont, DateTime? fromDate, DateTime? toDate, Branch branch)
        {



            var numColumns = 9;
            var saleReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 6,18,16,10,10,10,10,10,10 } }
            };

            saleReportTable.SetWidths(widths[numColumns]);

            //SL
            var cellSaleReportItem = GetPdfCell("SL", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Item
            cellSaleReportItem = GetPdfCell("Product Name", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Category
            cellSaleReportItem = GetPdfCell("Supplier Name", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Brand
            cellSaleReportItem = GetPdfCell("Sale Quantity", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);

            cellSaleReportItem = GetPdfCell("Subtotal", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Stock Quantity
            cellSaleReportItem = GetPdfCell("Discount", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);

            cellSaleReportItem = GetPdfCell("Net Amount", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);



            cellSaleReportItem = GetPdfCell("COGS", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);

            cellSaleReportItem = GetPdfCell("PL", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);

            


            saleReportTable.HeaderRows = 1;
            var r = 0;
            foreach (var pl in saleReportLines)
            {
                r++;

                //SL
                cellSaleReportItem = GetPdfCell(r, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                saleReportTable.AddCell(cellSaleReportItem);

                cellSaleReportItem = GetPdfCell(pl.ProductName, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                saleReportTable.AddCell(cellSaleReportItem);
                //ItemName
                cellSaleReportItem = GetPdfCell(pl.SupplierName, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                saleReportTable.AddCell(cellSaleReportItem);

                //CategoryName
                cellSaleReportItem = GetPdfCell(pl.SaleQuantity, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);


                //BrandName
                cellSaleReportItem = GetPdfCell(Math.Round(pl.Subtotal,2), font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);

                //CurrentStock
                cellSaleReportItem = GetPdfCell(Math.Round(pl.Discount,2), font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);


                cellSaleReportItem = GetPdfCell(Math.Round(pl.NetAmount,2), font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);

                cellSaleReportItem = GetPdfCell(Math.Round(pl.Cogs,2), font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);

                cellSaleReportItem = GetPdfCell(Math.Round(pl.NetAmount-pl.Cogs,2), font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);



            }
            //..................total..................................
            var totalNetAmount = saleReportLines.Sum(x => x.NetAmount);
            var totalCogs = saleReportLines.Sum(x => x.Cogs);
            //SL
            cellSaleReportItem = GetPdfCell("Total", font);
            cellSaleReportItem.Colspan = 3;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);

            //CurrentStock
            cellSaleReportItem = GetPdfCell(saleReportLines.Sum(x => x.SaleQuantity), font);
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);

            //CurrentStock
            cellSaleReportItem = GetPdfCell(Math.Round(saleReportLines.Sum(x => x.Subtotal),2), font);
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);

            //CurrentStock
            cellSaleReportItem = GetPdfCell(Math.Round(saleReportLines.Sum(x => x.Discount),2), font);
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);

            cellSaleReportItem = GetPdfCell(Math.Round(totalNetAmount,2), font);
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);

            cellSaleReportItem = GetPdfCell(Math.Round(totalCogs,2), font);
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);

            cellSaleReportItem = GetPdfCell(Math.Round(totalNetAmount - totalCogs,2), font);
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);

            doc.Add(saleReportTable);
        }

        protected virtual void PrintItemWisePurchaseReport(Font titleFont, Document doc, IList<ItemPurchaseViewModel> saleReportLines, Font font, Font attributesFont, DateTime? fromDate, DateTime? toDate, Branch branch)
        {



            var numColumns = 8;
            var saleReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 8,20,16,10,10,12,12,12 } }
            };

            saleReportTable.SetWidths(widths[numColumns]);

            //SL
            var cellSaleReportItem = GetPdfCell("SL", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Item
            cellSaleReportItem = GetPdfCell("Product Name", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Category
            cellSaleReportItem = GetPdfCell("Supplier Name", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Brand
            cellSaleReportItem = GetPdfCell("Purchase Qty", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);

            //Brand
            cellSaleReportItem = GetPdfCell("Bonus Qty", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);

            cellSaleReportItem = GetPdfCell("Subtotal", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Stock Quantity
            cellSaleReportItem = GetPdfCell("Discount", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);

            cellSaleReportItem = GetPdfCell("Net Amount", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);

            saleReportTable.HeaderRows = 1;
            var r = 0;
            foreach (var pl in saleReportLines)
            {
                r++;

                //SL
                cellSaleReportItem = GetPdfCell(r, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                saleReportTable.AddCell(cellSaleReportItem);

                cellSaleReportItem = GetPdfCell(pl.ProductName, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                saleReportTable.AddCell(cellSaleReportItem);
                //ItemName
                cellSaleReportItem = GetPdfCell(pl.SupplierName, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                saleReportTable.AddCell(cellSaleReportItem);

                //CategoryName
                cellSaleReportItem = GetPdfCell(pl.PurchaseQuantity, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);

                //CategoryName
                cellSaleReportItem = GetPdfCell(pl.BonusQuantity, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);

                //BrandName
                cellSaleReportItem = GetPdfCell(pl.Subtotal, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);

                //CurrentStock
                cellSaleReportItem = GetPdfCell(Math.Round(pl.Discount,2), font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);


                cellSaleReportItem = GetPdfCell(pl.NetAmount, font);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);

               

            }
            //..................total..................................
          
            //SL
            cellSaleReportItem = GetPdfCell("Total", font);
            cellSaleReportItem.Colspan = 3;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);

            //CurrentStock
            cellSaleReportItem = GetPdfCell(saleReportLines.Sum(x => x.PurchaseQuantity), font);
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);

            //CurrentStock
            cellSaleReportItem = GetPdfCell(saleReportLines.Sum(x => x.BonusQuantity), font);
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);

            //CurrentStock
            cellSaleReportItem = GetPdfCell(saleReportLines.Sum(x => x.Subtotal), font);
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);

            //CurrentStock
            cellSaleReportItem = GetPdfCell(Math.Round(saleReportLines.Sum(x => x.Discount),2), font);
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);

            cellSaleReportItem = GetPdfCell(saleReportLines.Sum(x => x.NetAmount), font);
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);

           
            doc.Add(saleReportTable);
        }
        public virtual void PrintStockReportToPdf(Stream stream,Branch branch , IList<StockModel> stockReportLines, string reportName)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (stockReportLines == null)
                throw new ArgumentNullException(nameof(stockReportLines));

            var pageSize = PageSize.A4;

          

            var doc = new Document(pageSize,20,20,25,100);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            DateTime? fromDate = null;

            DateTime? toDate = null;
            //headers
            PrintReportHeader(pdfWriter,  reportName,branch, headerTitleFont, addressFont, reportTitleFont,doc,fromDate,toDate);
            //Line
           // DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintStockReport( reportTitleFont, doc, stockReportLines, defaultFont, attributesFont );

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = stockReportLines.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }
        public virtual void PrintOutOfStockReportToPdf(Stream stream, Branch branch, IList<StockModel> stockReportLines, string reportName)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (stockReportLines == null)
                throw new ArgumentNullException(nameof(stockReportLines));

            var pageSize = PageSize.A4;



            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            DateTime? fromDate = null;

            DateTime? toDate = null;
            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc,fromDate,toDate);
            //Line
            // DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintOutOfStockReport(reportTitleFont, doc, stockReportLines, defaultFont, attributesFont);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = stockReportLines.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }
        public virtual void PrintProfitReportToPdf(Stream stream, Branch branch, IList<PLModel> profitReportLines, string reportName,DateTime? fromDate,DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (profitReportLines == null)
                throw new ArgumentNullException(nameof(profitReportLines));

            var pageSize = PageSize.A4;



            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc,fromDate, toDate);
            //Line
            // DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintProfitReport(reportTitleFont, doc, profitReportLines, defaultFont, attributesFont);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = profitReportLines.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }

        public virtual void PrintBalanceReportToPdf(Stream stream, Branch branch, IList<BalanceReportLine> balanceReportLines, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (balanceReportLines == null)
                throw new ArgumentNullException(nameof(balanceReportLines));

            var pageSize = PageSize.A4;



            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            // DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintBalanceReport(reportTitleFont, doc, balanceReportLines, defaultFont, attributesFont);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = balanceReportLines.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }
        public virtual void PrintSupplierLedgerReportToPdf(Stream stream, Supplier supplier,decimal? prevBalance, IList<SupplierLedgerReportLine> supplierReportLines, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (supplierReportLines == null)
                throw new ArgumentNullException(nameof(supplierReportLines));

            var pageSize = PageSize.A4;



            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintSupplierLedgerReportHeader(pdfWriter, reportName, supplier, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            // DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintSupplierLedgerReport(reportTitleFont, doc, supplierReportLines, prevBalance, defaultFont, attributesFont);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = supplierReportLines.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }
        public virtual void PrintCustomerLedgerReportToPdf(Stream stream, Customer customer,decimal? prevBalance, IList<CustomerLedgerReportLine> customerReportLines, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (customerReportLines == null)
                throw new ArgumentNullException(nameof(customerReportLines));

            var pageSize = PageSize.A4;



            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintCustomerLedgerReportHeader(pdfWriter, reportName, customer, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            // DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintCustomerLedgerReport(reportTitleFont, doc, customerReportLines,prevBalance, defaultFont, attributesFont);

            var customerReportLineNumber = 20;
            var customerReportLineNumberCount = customerReportLines.Count;
            customerReportLineNumber++;

            if (customerReportLineNumber <= customerReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }
        public virtual void PrintPurchaseReportToPdf(Stream stream, Branch branch, IList<PurchaseViewModel> purchaseReportLines, string reportName,DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (purchaseReportLines == null)
                throw new ArgumentNullException(nameof(purchaseReportLines));

            var pageSize = PageSize.A4;



            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
          //  DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintPurchaseReport(reportTitleFont, doc, purchaseReportLines, defaultFont, attributesFont);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = purchaseReportLines.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }
        public virtual void PrintTrailBalanceReportToPdf(Stream stream, Branch branch, IList<TrialBalanceViewModel> trialBalanceReportLines, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (trialBalanceReportLines == null)
                throw new ArgumentNullException(nameof(trialBalanceReportLines));

            var pageSize = PageSize.A4;



            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.NORMAL);
            defaultFont.Size = 10;


            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintTrialBalanceReport(reportTitleFont, doc, trialBalanceReportLines, defaultFont, attributesFont);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = trialBalanceReportLines.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }
        public virtual void PrintBalanceSheetReportToPdf(Stream stream, Branch branch, IList<AccountHeadWithBalanceViewModel> balanceSheetReportLines, string reportName, DateTime? fromDate, DateTime? toDate,decimal netIncome)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (balanceSheetReportLines == null)
                throw new ArgumentNullException(nameof(balanceSheetReportLines));

            var pageSize = PageSize.A4;



            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintBalanceSheetReport(reportTitleFont, doc, balanceSheetReportLines, defaultFont, attributesFont,netIncome);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = balanceSheetReportLines.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }
        public virtual void PrintIncomeStatementReportToPdf(Stream stream, Branch branch, IList<AccountHeadWithBalanceViewModel> incomeStatementReportLines, string reportName, DateTime? fromDate, DateTime? toDate,decimal netIncome=0)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (incomeStatementReportLines == null)
                throw new ArgumentNullException(nameof(incomeStatementReportLines));

            var pageSize = PageSize.A4;



            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintIncomeStatementReport(reportTitleFont, doc, incomeStatementReportLines, defaultFont, attributesFont,netIncome);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = incomeStatementReportLines.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }
        public virtual void PrintExpenseReportToPdf(Stream stream, Branch branch, IList<ExpenseViewModel> expenseReportLines, string reportName, DateTime? fromDate, DateTime? toDatde)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (expenseReportLines == null)
                throw new ArgumentNullException(nameof(expenseReportLines));

            var pageSize = PageSize.A4;



            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc,  fromDate, toDatde);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintExpenseReport(reportTitleFont, doc, expenseReportLines, defaultFont, attributesFont);

            var expenseReportLineNumber = 20;
            var expenseReportLineNumberCount = expenseReportLines.Count;
            expenseReportLineNumber++;

            if (expenseReportLineNumber <= expenseReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }
        public virtual void PrintSaleReportToPdf(Stream stream, Branch branch, IList<SaleViewModel> saleReportLines, string reportName,DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (saleReportLines == null)
                throw new ArgumentNullException(nameof(saleReportLines));

            var pageSize = PageSize.A4;



            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintSaleReport(reportTitleFont, doc, saleReportLines, defaultFont, attributesFont,fromDate,toDate,branch);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = saleReportLines.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }
        public virtual void PrintItemWiseSaleProfitLossReportToPdf(Stream stream, Branch branch, IList<ItemSaleProfitLossViewModel> saleReportLines, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (saleReportLines == null)
                throw new ArgumentNullException(nameof(saleReportLines));

            var pageSize = PageSize.A4;



            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintItemWiseSaleProfitLossReport(reportTitleFont, doc, saleReportLines, defaultFont, attributesFont, fromDate, toDate, branch);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = saleReportLines.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }

        public virtual void PrintItemWisePurchaseReportToPdf(Stream stream, Branch branch, IList<ItemPurchaseViewModel> saleReportLines, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (saleReportLines == null)
                throw new ArgumentNullException(nameof(saleReportLines));

            var pageSize = PageSize.A4;



            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintItemWisePurchaseReport(reportTitleFont, doc, saleReportLines, defaultFont, attributesFont, fromDate, toDate, branch);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = saleReportLines.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }

        public void PrintSupplierBalanceReportToPdf(Stream stream, List<SupplierViewModel> suppliers, string message)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (suppliers == null)
                throw new ArgumentNullException(nameof(suppliers));

            var pageSize = PageSize.A4;



            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintSupplierBalanceReportHeader(pdfWriter, "Supplier Balance Report",  headerTitleFont, addressFont, reportTitleFont, doc);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintSupplierBalanceReport(reportTitleFont, doc, suppliers, defaultFont, attributesFont);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = suppliers.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }

        private void PrintSupplierBalanceReport(Font reportTitleFont, Document doc, List<SupplierViewModel> suppliers, Font defaultFont, Font attributesFont)
        {
            var numColumns = 4;
            var supplierBalanceReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 10,25,40,25 } }
            };

            supplierBalanceReportTable.SetWidths(widths[numColumns]);

            //SL
            var cellSupplierBalanceReportItem = GetPdfCell("SL", defaultFont);
            cellSupplierBalanceReportItem.BackgroundColor = BaseColor.LightGray;
            cellSupplierBalanceReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierBalanceReportTable.AddCell(cellSupplierBalanceReportItem);
            //Item
            cellSupplierBalanceReportItem = GetPdfCell("Supplier Code", defaultFont);
            cellSupplierBalanceReportItem.BackgroundColor = BaseColor.LightGray;
            cellSupplierBalanceReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierBalanceReportTable.AddCell(cellSupplierBalanceReportItem);

            cellSupplierBalanceReportItem = GetPdfCell("Supplier Name", defaultFont);
            cellSupplierBalanceReportItem.BackgroundColor = BaseColor.LightGray;
            cellSupplierBalanceReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierBalanceReportTable.AddCell(cellSupplierBalanceReportItem);
            //Expense Quantity
            cellSupplierBalanceReportItem = GetPdfCell("Supplier Balance", defaultFont);
            cellSupplierBalanceReportItem.BackgroundColor = BaseColor.LightGray;
            cellSupplierBalanceReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierBalanceReportTable.AddCell(cellSupplierBalanceReportItem);
          

            supplierBalanceReportTable.HeaderRows = 1;
            var r = 0;
            foreach (var sl in suppliers)
            {
                r++;

                //SL
                cellSupplierBalanceReportItem = GetPdfCell(r, defaultFont);
                cellSupplierBalanceReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                supplierBalanceReportTable.AddCell(cellSupplierBalanceReportItem);
                //ItemName
                cellSupplierBalanceReportItem = GetPdfCell(sl.Code, defaultFont);
                cellSupplierBalanceReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                supplierBalanceReportTable.AddCell(cellSupplierBalanceReportItem);

                //CategoryName
                //cellExpenseReportItem = GetPdfCell(pl.Branch, font);
                //cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                //expenseReportTable.AddCell(cellExpenseReportItem);

                //BrandName
                cellSupplierBalanceReportItem = GetPdfCell(sl.Name, defaultFont);
                cellSupplierBalanceReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                supplierBalanceReportTable.AddCell(cellSupplierBalanceReportItem);

                //CurrentExpense
                cellSupplierBalanceReportItem = GetPdfCell(sl.Balance, defaultFont);
                cellSupplierBalanceReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                supplierBalanceReportTable.AddCell(cellSupplierBalanceReportItem);

                
            }
            //..................total..................................
            //SL
            cellSupplierBalanceReportItem = GetPdfCell("Total Balance", defaultFont);
            cellSupplierBalanceReportItem.Colspan = 3;
            cellSupplierBalanceReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            supplierBalanceReportTable.AddCell(cellSupplierBalanceReportItem);




            //CurrentExpense
            cellSupplierBalanceReportItem = GetPdfCell(suppliers.Sum(x => x.Balance), defaultFont);
            cellSupplierBalanceReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            supplierBalanceReportTable.AddCell(cellSupplierBalanceReportItem);

            
            doc.Add(supplierBalanceReportTable);
        }

        private void PrintSupplierBalanceReportHeader(PdfWriter pdfWriter, string reportHeader, Font headerTitleFont, Font addressFont, Font reportTitleFont, Document doc)
        {



            var font = GetFont();
            font.SetFamily("Times New Roman");
            font.Size = 10;
            font.SetStyle(Font.NORMAL);
            font.SetStyle(Font.BOLD);
            font.Color = BaseColor.Black;

            //header
            var numColumns = 2;
            var headerTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 70,30 } }
            };

            headerTable.SetWidths(widths[numColumns]);


            //SL


            var logo = Image.GetInstance(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", Path.GetFileName("logo.jpg")));
            logo.ScaleToFit(100f, 500f);
            logo.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
            logo.IndentationLeft = 9f;
            logo.IndentationRight = 9f;
            logo.SpacingBefore = 5f;
            logo.SpacingAfter = 2f;
            //Item

            var logocell = new PdfPCell { Border = Rectangle.NO_BORDER };
            logocell.AddElement(logo);
            logocell.Colspan = 2;
            headerTable.AddCell(logocell);

            var cellPurchaseReportItem = GetPdfCell("GREENHILL VET PHARMA", headerTitleFont);


            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            cellPurchaseReportItem.Phrase.Add(new Phrase(Environment.NewLine));

            cellPurchaseReportItem.Border = Rectangle.NO_BORDER;
            cellPurchaseReportItem.Colspan = 2;
            headerTable.AddCell(cellPurchaseReportItem);

            cellPurchaseReportItem = GetPdfCell(reportHeader, headerTitleFont);


            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            cellPurchaseReportItem.Phrase.Add(new Phrase(Environment.NewLine));
            cellPurchaseReportItem.Phrase.Add(new Phrase(Environment.NewLine));

            cellPurchaseReportItem.Border = Rectangle.NO_BORDER;
            cellPurchaseReportItem.Colspan = 2;
            headerTable.AddCell(cellPurchaseReportItem);

            doc.Add(headerTable);
        }

        public void PrintCustomerBalanceReportToPdf(Stream stream, List<CustomerViewModel> customers, string message)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (customers == null)
                throw new ArgumentNullException(nameof(customers));

            var pageSize = PageSize.A4;



            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintcustomerBalanceReportHeader(pdfWriter, "Customer Balance Report", headerTitleFont, addressFont, reportTitleFont, doc);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintcustomerBalanceReport(reportTitleFont, doc, customers, defaultFont, attributesFont);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = customers.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }
        private void PrintcustomerBalanceReportHeader(PdfWriter pdfWriter, string reportHeader, Font headerTitleFont, Font addressFont, Font reportTitleFont, Document doc)
        {



            var font = GetFont();
            font.SetFamily("Times New Roman");
            font.Size = 10;
            font.SetStyle(Font.NORMAL);
            font.SetStyle(Font.BOLD);
            font.Color = BaseColor.Black;

            //header
            var numColumns = 2;
            var headerTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 70,30 } }
            };

            headerTable.SetWidths(widths[numColumns]);


            //SL


            var logo = Image.GetInstance(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", Path.GetFileName("logo.jpg")));
            logo.ScaleToFit(100f, 500f);
            logo.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
            logo.IndentationLeft = 9f;
            logo.IndentationRight = 9f;
            logo.SpacingBefore = 5f;
            logo.SpacingAfter = 2f;
            //Item

            var logocell = new PdfPCell { Border = Rectangle.NO_BORDER };
            logocell.AddElement(logo);
            logocell.Colspan = 2;
            headerTable.AddCell(logocell);

            var cellPurchaseReportItem = GetPdfCell("GREENHILL VET PHARMA", headerTitleFont);


            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            cellPurchaseReportItem.Phrase.Add(new Phrase(Environment.NewLine));

            cellPurchaseReportItem.Border = Rectangle.NO_BORDER;
            cellPurchaseReportItem.Colspan = 2;
            headerTable.AddCell(cellPurchaseReportItem);

            cellPurchaseReportItem = GetPdfCell(reportHeader, headerTitleFont);


            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            cellPurchaseReportItem.Phrase.Add(new Phrase(Environment.NewLine));
            cellPurchaseReportItem.Phrase.Add(new Phrase(Environment.NewLine));

            cellPurchaseReportItem.Border = Rectangle.NO_BORDER;
            cellPurchaseReportItem.Colspan = 2;
            headerTable.AddCell(cellPurchaseReportItem);

            doc.Add(headerTable);
        }
        private void PrintcustomerBalanceReport(Font reportTitleFont, Document doc, List<CustomerViewModel> customers, Font defaultFont, Font attributesFont)
        {
            var numColumns = 4;
            var customerBalanceReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 10,25,40,25 } }
            };

            customerBalanceReportTable.SetWidths(widths[numColumns]);

            //SL
            var cellcustomerBalanceReportItem = GetPdfCell("SL", defaultFont);
            cellcustomerBalanceReportItem.BackgroundColor = BaseColor.LightGray;
            cellcustomerBalanceReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            customerBalanceReportTable.AddCell(cellcustomerBalanceReportItem);
            //Item
            cellcustomerBalanceReportItem = GetPdfCell("customer Code", defaultFont);
            cellcustomerBalanceReportItem.BackgroundColor = BaseColor.LightGray;
            cellcustomerBalanceReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            customerBalanceReportTable.AddCell(cellcustomerBalanceReportItem);

            cellcustomerBalanceReportItem = GetPdfCell("customer Name", defaultFont);
            cellcustomerBalanceReportItem.BackgroundColor = BaseColor.LightGray;
            cellcustomerBalanceReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            customerBalanceReportTable.AddCell(cellcustomerBalanceReportItem);

            //Expense Quantity
            cellcustomerBalanceReportItem = GetPdfCell("customer Balance", defaultFont);
            cellcustomerBalanceReportItem.BackgroundColor = BaseColor.LightGray;
            cellcustomerBalanceReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            customerBalanceReportTable.AddCell(cellcustomerBalanceReportItem);


            customerBalanceReportTable.HeaderRows = 1;
            var r = 0;
            foreach (var sl in customers)
            {
                r++;

                //SL
                cellcustomerBalanceReportItem = GetPdfCell(r, defaultFont);
                cellcustomerBalanceReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                customerBalanceReportTable.AddCell(cellcustomerBalanceReportItem);
                //ItemName
                cellcustomerBalanceReportItem = GetPdfCell(sl.Code, defaultFont);
                cellcustomerBalanceReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                customerBalanceReportTable.AddCell(cellcustomerBalanceReportItem);

                //CategoryName
                //cellExpenseReportItem = GetPdfCell(pl.Branch, font);
                //cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                //expenseReportTable.AddCell(cellExpenseReportItem);

                //BrandName
                cellcustomerBalanceReportItem = GetPdfCell(sl.Name, defaultFont);
                cellcustomerBalanceReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                customerBalanceReportTable.AddCell(cellcustomerBalanceReportItem);

                //CurrentExpense
                cellcustomerBalanceReportItem = GetPdfCell(sl.Balance, defaultFont);
                cellcustomerBalanceReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                customerBalanceReportTable.AddCell(cellcustomerBalanceReportItem);


            }
            //..................total..................................
            //SL
            cellcustomerBalanceReportItem = GetPdfCell("Total Balance", defaultFont);
            cellcustomerBalanceReportItem.Colspan = 3;
            cellcustomerBalanceReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            customerBalanceReportTable.AddCell(cellcustomerBalanceReportItem);




            //CurrentExpense
            cellcustomerBalanceReportItem = GetPdfCell(customers.Sum(x => x.Balance), defaultFont);
            cellcustomerBalanceReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            customerBalanceReportTable.AddCell(cellcustomerBalanceReportItem);


            doc.Add(customerBalanceReportTable);
        }

        public void PrintSupplierPaymentReportToPdf(Stream stream, List<SupplierPaymentViewModel> supplierpayments, string message,Branch branch,DateTime? fromDate, DateTime?toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (supplierpayments == null)
                throw new ArgumentNullException(nameof(supplierpayments));

            var pageSize = PageSize.A4;



            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintReportHeader(pdfWriter, "Supplier Payment Report",branch, headerTitleFont, addressFont, reportTitleFont, doc,fromDate,toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintSupplierPaymentReport(reportTitleFont, doc, supplierpayments, defaultFont, attributesFont);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = supplierpayments.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }
        public void PrintCustomerPaymentReportToPdf(Stream stream, List<CustomerPaymentViewModel> customerpayments, string message, Branch branch, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (customerpayments == null)
                throw new ArgumentNullException(nameof(customerpayments));

            var pageSize = PageSize.A4;



            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintReportHeader(pdfWriter, "Customer Payment Report", branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintCustomerPaymentReport(reportTitleFont, doc, customerpayments, defaultFont, attributesFont);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = customerpayments.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }

        private void PrintCustomerPaymentReport(Font reportTitleFont, Document doc, List<CustomerPaymentViewModel> customerpayments, Font defaultFont, Font attributesFont)
        {
            var numColumns = 4;
            var customerPaymentReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 10,25,40,25 } }
            };

            customerPaymentReportTable.SetWidths(widths[numColumns]);

            //SL
            var cellcustomerPaymentReportItem = GetPdfCell("SL", defaultFont);
            cellcustomerPaymentReportItem.BackgroundColor = BaseColor.LightGray;
            cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            customerPaymentReportTable.AddCell(cellcustomerPaymentReportItem);
            //Item
            cellcustomerPaymentReportItem = GetPdfCell("Bill-No", defaultFont);
            cellcustomerPaymentReportItem.BackgroundColor = BaseColor.LightGray;
            cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            customerPaymentReportTable.AddCell(cellcustomerPaymentReportItem);

            cellcustomerPaymentReportItem = GetPdfCell("Customer Name", defaultFont);
            cellcustomerPaymentReportItem.BackgroundColor = BaseColor.LightGray;
            cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            customerPaymentReportTable.AddCell(cellcustomerPaymentReportItem);
            //Expense Quantity
            cellcustomerPaymentReportItem = GetPdfCell("Customer Payment", defaultFont);
            cellcustomerPaymentReportItem.BackgroundColor = BaseColor.LightGray;
            cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            customerPaymentReportTable.AddCell(cellcustomerPaymentReportItem);


            customerPaymentReportTable.HeaderRows = 1;
            var r = 0;
            foreach (var sl in customerpayments)
            {
                r++;

                //SL
                cellcustomerPaymentReportItem = GetPdfCell(r, defaultFont);
                cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                customerPaymentReportTable.AddCell(cellcustomerPaymentReportItem);
                //ItemName
                cellcustomerPaymentReportItem = GetPdfCell(sl.BillNo, defaultFont);
                cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                customerPaymentReportTable.AddCell(cellcustomerPaymentReportItem);

                //CategoryName
                //cellExpenseReportItem = GetPdfCell(pl.Branch, font);
                //cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                //expenseReportTable.AddCell(cellExpenseReportItem);

                //BrandName
                cellcustomerPaymentReportItem = GetPdfCell(sl.Customer.Name, defaultFont);
                cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
               customerPaymentReportTable.AddCell(cellcustomerPaymentReportItem);

                //CurrentExpense
                cellcustomerPaymentReportItem = GetPdfCell(sl.PaymentAmount, defaultFont);
                cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                customerPaymentReportTable.AddCell(cellcustomerPaymentReportItem);


            }
            //..................total..................................
            //SL
            cellcustomerPaymentReportItem = GetPdfCell("Total Payment", defaultFont);
            cellcustomerPaymentReportItem.Colspan = 3;
            cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            customerPaymentReportTable.AddCell(cellcustomerPaymentReportItem);




            //CurrentExpense
            cellcustomerPaymentReportItem = GetPdfCell(customerpayments.Sum(x => x.CustomerBalance), defaultFont);
            cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            customerPaymentReportTable.AddCell(cellcustomerPaymentReportItem);


            doc.Add(customerPaymentReportTable);
        }

        private void PrintSupplierPaymentReport(Font reportTitleFont, Document doc, List<SupplierPaymentViewModel> supplierpayments, Font defaultFont, Font attributesFont)
        {
            var numColumns = 5;
            var supplierPaymentReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 10,25,30,15,20 } }
            };

            supplierPaymentReportTable.SetWidths(widths[numColumns]);

            //SL
            var cellsupplierPaymentReportItem = GetPdfCell("SL", defaultFont);
            cellsupplierPaymentReportItem.BackgroundColor = BaseColor.LightGray;
            cellsupplierPaymentReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierPaymentReportTable.AddCell(cellsupplierPaymentReportItem);
            //Item
            cellsupplierPaymentReportItem = GetPdfCell("Bill-No", defaultFont);
            cellsupplierPaymentReportItem.BackgroundColor = BaseColor.LightGray;
            cellsupplierPaymentReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierPaymentReportTable.AddCell(cellsupplierPaymentReportItem);

            cellsupplierPaymentReportItem = GetPdfCell("Supplier Name", defaultFont);
            cellsupplierPaymentReportItem.BackgroundColor = BaseColor.LightGray;
            cellsupplierPaymentReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierPaymentReportTable.AddCell(cellsupplierPaymentReportItem);

            cellsupplierPaymentReportItem = GetPdfCell("Payment Method Name", defaultFont);
            cellsupplierPaymentReportItem.BackgroundColor = BaseColor.LightGray;
            cellsupplierPaymentReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierPaymentReportTable.AddCell(cellsupplierPaymentReportItem);

            //cellsupplierPaymentReportItem = GetPdfCell("Remark", defaultFont);
            //cellsupplierPaymentReportItem.BackgroundColor = BaseColor.LightGray;
            //cellsupplierPaymentReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            //supplierPaymentReportTable.AddCell(cellsupplierPaymentReportItem);
            //Expense Quantity
            cellsupplierPaymentReportItem = GetPdfCell("Supplier Payment", defaultFont);
            cellsupplierPaymentReportItem.BackgroundColor = BaseColor.LightGray;
            cellsupplierPaymentReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierPaymentReportTable.AddCell(cellsupplierPaymentReportItem);


            supplierPaymentReportTable.HeaderRows = 1;
            var r = 0;
            foreach (var sl in supplierpayments)
            {
                r++;

                //SL
                cellsupplierPaymentReportItem = GetPdfCell(r, defaultFont);
                cellsupplierPaymentReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                supplierPaymentReportTable.AddCell(cellsupplierPaymentReportItem);
                //ItemName
                cellsupplierPaymentReportItem = GetPdfCell(sl.BillNo, defaultFont);
                cellsupplierPaymentReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                supplierPaymentReportTable.AddCell(cellsupplierPaymentReportItem);

                //CategoryName
                //cellExpenseReportItem = GetPdfCell(pl.Branch, font);
                //cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                //expenseReportTable.AddCell(cellExpenseReportItem);

                //BrandName
                cellsupplierPaymentReportItem = GetPdfCell(sl.Supplier.Name, defaultFont);
                cellsupplierPaymentReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                supplierPaymentReportTable.AddCell(cellsupplierPaymentReportItem);

                cellsupplierPaymentReportItem = GetPdfCell(sl.PaymentMethod.Name, defaultFont);
                cellsupplierPaymentReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                supplierPaymentReportTable.AddCell(cellsupplierPaymentReportItem);

                //cellsupplierPaymentReportItem = GetPdfCell(sl.Remark, defaultFont);
                //cellsupplierPaymentReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                //supplierPaymentReportTable.AddCell(cellsupplierPaymentReportItem);

                //CurrentExpense
                cellsupplierPaymentReportItem = GetPdfCell(sl.PaymentAmount, defaultFont);
                cellsupplierPaymentReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                supplierPaymentReportTable.AddCell(cellsupplierPaymentReportItem);


            }
            //..................total..................................
            //SL
            cellsupplierPaymentReportItem = GetPdfCell("Total Payment", defaultFont);
            cellsupplierPaymentReportItem.Colspan = 4;
            cellsupplierPaymentReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            supplierPaymentReportTable.AddCell(cellsupplierPaymentReportItem);




            //CurrentExpense
            cellsupplierPaymentReportItem = GetPdfCell(supplierpayments.Sum(x => x.PaymentAmount), defaultFont);
            cellsupplierPaymentReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            supplierPaymentReportTable.AddCell(cellsupplierPaymentReportItem);


            doc.Add(supplierPaymentReportTable);
        }

        public void PrintSaleReturnReportToPdf(MemoryStream stream, List<SaleReturnViewModel> item1, string v, Branch branch, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (item1 == null)
                throw new ArgumentNullException(nameof(item1));

            var pageSize = PageSize.A4;



            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);
            PrintReportHeader(pdfWriter, "Sale Return Report", branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            PrintSaleReturnReport(reportTitleFont, doc, item1, defaultFont, attributesFont);

            var salereturnReportLineNumber = 20;
            var saleReturnReportLineNumberCount = item1.Count;
            salereturnReportLineNumber++;

            if (salereturnReportLineNumber <= saleReturnReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }

        private void PrintSaleReturnReport(Font reportTitleFont, Document doc, List<SaleReturnViewModel> item1, Font defaultFont, Font attributesFont)
        {
            var numColumns = 6;
            var saleReturnReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 10,20,10,25,15,20 } }
            };

            saleReturnReportTable.SetWidths(widths[numColumns]);

            var cellsalereturnReportItem = GetPdfCell("SL", defaultFont);
            cellsalereturnReportItem.BackgroundColor = BaseColor.LightGray;
            cellsalereturnReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReturnReportTable.AddCell(cellsalereturnReportItem);

            cellsalereturnReportItem = GetPdfCell("Date", defaultFont);
            cellsalereturnReportItem.BackgroundColor = BaseColor.LightGray;
            cellsalereturnReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReturnReportTable.AddCell(cellsalereturnReportItem);

            cellsalereturnReportItem = GetPdfCell("Invoice No.", defaultFont);
            cellsalereturnReportItem.BackgroundColor = BaseColor.LightGray;
            cellsalereturnReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReturnReportTable.AddCell(cellsalereturnReportItem);

            cellsalereturnReportItem = GetPdfCell("Customer Name", defaultFont);
            cellsalereturnReportItem.BackgroundColor = BaseColor.LightGray;
            cellsalereturnReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReturnReportTable.AddCell(cellsalereturnReportItem);

            cellsalereturnReportItem = GetPdfCell("Qty", defaultFont);
            cellsalereturnReportItem.BackgroundColor = BaseColor.LightGray;
            cellsalereturnReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReturnReportTable.AddCell(cellsalereturnReportItem);

            cellsalereturnReportItem = GetPdfCell("Amount", defaultFont);
            cellsalereturnReportItem.BackgroundColor = BaseColor.LightGray;
            cellsalereturnReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReturnReportTable.AddCell(cellsalereturnReportItem);

            

            saleReturnReportTable.HeaderRows = 1;

            var r = 0;
            foreach (var sl in item1)
            {
                r++;
                cellsalereturnReportItem = GetPdfCell(r, defaultFont);
               // cellsalereturnReportItem.BackgroundColor = BaseColor.LightGray;
                cellsalereturnReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                saleReturnReportTable.AddCell(cellsalereturnReportItem);

                cellsalereturnReportItem = GetPdfCell(sl.SaleReturnDate.ToString("dd/MM/yy"), defaultFont);
               // cellsalereturnReportItem.BackgroundColor = BaseColor.LightGray;
                cellsalereturnReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                saleReturnReportTable.AddCell(cellsalereturnReportItem);

                cellsalereturnReportItem = GetPdfCell(sl.SaleNo, defaultFont);
               // cellsalereturnReportItem.BackgroundColor = BaseColor.LightGray;
                cellsalereturnReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                saleReturnReportTable.AddCell(cellsalereturnReportItem);

                cellsalereturnReportItem = GetPdfCell(sl.CustomerName, defaultFont);
                // cellsalereturnReportItem.BackgroundColor = BaseColor.LightGray;
                cellsalereturnReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                saleReturnReportTable.AddCell(cellsalereturnReportItem);

                cellsalereturnReportItem = GetPdfCell(sl.TotalItem, defaultFont);
               // cellsalereturnReportItem.BackgroundColor = BaseColor.LightGray;
                cellsalereturnReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                saleReturnReportTable.AddCell(cellsalereturnReportItem);

                cellsalereturnReportItem = GetPdfCell(sl.Subtotal, defaultFont);
              //  cellsalereturnReportItem.BackgroundColor = BaseColor.LightGray;
                cellsalereturnReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                saleReturnReportTable.AddCell(cellsalereturnReportItem);

            }

            cellsalereturnReportItem = GetPdfCell("Total", defaultFont);
            cellsalereturnReportItem.Colspan = 4;
            cellsalereturnReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReturnReportTable.AddCell(cellsalereturnReportItem);

            cellsalereturnReportItem = GetPdfCell(item1.Sum(x => x.TotalItem), defaultFont);
            cellsalereturnReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReturnReportTable.AddCell(cellsalereturnReportItem);

            cellsalereturnReportItem = GetPdfCell(item1.Sum(x => x.Subtotal), defaultFont);
            cellsalereturnReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReturnReportTable.AddCell(cellsalereturnReportItem);


            doc.Add(saleReturnReportTable);
        }

        public void PrintSubsidiaryLedgerReportToPdf(MemoryStream stream, Branch? branch, IList<SubsidiaryLedgerViewModel> subsidiaryLedgerReportLines, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (subsidiaryLedgerReportLines == null)
                throw new ArgumentNullException(nameof(subsidiaryLedgerReportLines));

            var pageSize = PageSize.A4;



            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.NORMAL);
            defaultFont.Size = 12;
            addressFont.Color = BaseColor.Red;

            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintSubsidiaryLedgerReport(reportTitleFont, doc, subsidiaryLedgerReportLines, defaultFont, attributesFont);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = subsidiaryLedgerReportLines.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }
    }
}
