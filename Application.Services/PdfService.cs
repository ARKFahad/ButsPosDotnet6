using Application.Core.Common;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.ReportModels;
using Application.ViewModel;
using iTextSharp.text;
using iTextSharp.text.pdf;

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

        protected void DrawHorizontalLineProtrait(PdfWriter writer)
        {
            // Get the PdfContentByte from the PdfWriter
            PdfContentByte contentByte = writer.DirectContent;

            // Set the starting point of the line
            float startX = 30;
            float startY = 735;

            // Set the ending point of the line
            float endX = 580;
            float endY = 735;

            // Move to the starting point
            contentByte.MoveTo(startX, startY);

            // Draw a line to the ending point
            contentByte.LineTo(endX, endY);

            // Stroke the line (draw it)
            contentByte.Stroke();
        }

        protected void DrawHorizontalLineLandscape(PdfWriter writer)
        {
            // Get the PdfContentByte from the PdfWriter
            PdfContentByte contentByte = writer.DirectContent;

            // Set the starting point of the line
            float startX = 30;
            float startY = 485;

            // Set the ending point of the line
            float endX = 810;
            float endY = 485;

            // Move to the starting point
            contentByte.MoveTo(startX, startY);

            // Draw a line to the ending point
            contentByte.LineTo(endX, endY);

            // Stroke the line (draw it)
            contentByte.Stroke();
        }

        public byte[] AddPageNumbers(byte[] originalPdf)
        {
            MemoryStream modifiedPdfStream = new MemoryStream();
            // Create a reader for the original document
            PdfReader originalReader = new PdfReader(originalPdf);

            // Retrieve the total number of pages in the original document
            int totalPages = originalReader.NumberOfPages;

            // Retrieve the size of the first page
            Rectangle pageSize = originalReader.GetPageSize(1);

            // Step 1: Creation of a document object for the modified PDF
            Document modifiedDocument = new Document(pageSize, 50, 50, 50, 50);

            // Step 2: Create a writer that listens to the modified document
            PdfWriter writer = PdfWriter.GetInstance(modifiedDocument, modifiedPdfStream);

            // Step 3: Open the modified document
            modifiedDocument.Open();

            // Step 4: Add content to the modified document
            PdfContentByte contentByte = writer.DirectContent;

            int currentPageNumber = 0;

            for (int page = 1; page <= originalReader.NumberOfPages; page++)
            {
                modifiedDocument.NewPage();
                currentPageNumber++;

                PdfImportedPage importedPage = writer.GetImportedPage(originalReader, page);
                contentByte.AddTemplate(importedPage, 0, 0);

                BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

                contentByte.BeginText();
                contentByte.SetFontAndSize(baseFont, 10);

                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Checked By", 30, 30, 0);
                contentByte.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Approved By", 570, 30, 0);
                contentByte.EndText();

                contentByte.BeginText();
                contentByte.SetFontAndSize(baseFont, 10);

                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Printing Date : " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"), 30, 10, 0);
                contentByte.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Page " + currentPageNumber + " of " + totalPages, 340, 10, 0);
                contentByte.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Powered By: BUTS", 570, 10, 0);
                contentByte.EndText();
            }

            // Step 5: Close the modified document
            modifiedDocument.Close();

            return modifiedPdfStream.ToArray();
        }

        protected virtual Font GetFont()
        {
            //var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "pdf", Path.GetFileName("RobotoRegular.ttf"));
            // Set the path to the Roboto Regular font file
            var fontFilePath = Path.Combine(Directory.GetCurrentDirectory(), "pdf", "RobotoRegular.ttf");

            // Create a BaseFont using the specified font file
            var baseFont = BaseFont.CreateFont(fontFilePath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            // Create a Font object with the specified BaseFont and size
            var font = new Font(baseFont, 10, Font.NORMAL);

            return font;
        }

        protected virtual PdfPCell GetPdfCell(object content, Font cellFont)
        {
            // Create a PdfPCell with the specified content and font
            return new PdfPCell(new Phrase(content.ToString(), cellFont));
        }

        //Common Report Header With FromDate and ToDate
        protected virtual void PrintReportHeader(PdfWriter pdfWriter, string reportHeader, Branch branch, Font headerTitleFont, Font addressFont, Font reportTitleFont, Document doc, DateTime? fromDate, DateTime? toDatde)
        {

            var font = GetFont();
            font.SetFamily("Times New Roman");
            font.Size = 8;
            font.SetStyle(Font.NORMAL);
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

            var cellReportName = GetPdfCell(reportHeader, reportTitleFont);
            cellReportName.HorizontalAlignment = Element.ALIGN_CENTER;
            cellReportName.Phrase.Add(new Phrase(Environment.NewLine));
            cellReportName.Phrase.Add(new Phrase(Environment.NewLine));
            cellReportName.Border = Rectangle.NO_BORDER;
            cellReportName.Colspan = 2;
            headerTable.AddCell(cellReportName);

            var cellBranchName = GetPdfCell("Branch Name: " + branch?.Name, font);
            if (branch != null)
            {
                cellBranchName.HorizontalAlignment = Element.ALIGN_LEFT;
                cellBranchName.Border = Rectangle.NO_BORDER;
                headerTable.AddCell(cellBranchName);
            }
            else
            {
                cellBranchName = GetPdfCell("", font);
                cellBranchName.HorizontalAlignment = Element.ALIGN_LEFT;
                cellBranchName.Border = Rectangle.NO_BORDER;
                headerTable.AddCell(cellBranchName);
            }

            var cellFromDate = GetPdfCell("From: " + fromDate?.ToString("dd/MM/yyyy"), font);
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

            var cellBranchCode = GetPdfCell("Branch Code : " + branch?.Code, font);
            if (branch != null)
            {
                cellBranchCode = GetPdfCell("Branch Code : " + branch.Code, font);
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


        //Common Report Header Without FromDate and ToDate
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


        public byte[] AddFooterInfoForA5Page(byte[] pdf)
        {
            MemoryStream ms = new MemoryStream();
            // we create a reader for a certain document
            PdfReader reader = new PdfReader(pdf);
            // we retrieve the total number of pages
            int n = reader.NumberOfPages;
            // we retrieve the size of the first page
            Rectangle psize = reader.GetPageSize(1);

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


                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Sold goods are returnable with saleable condition", 16, 20, 0);

                // cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Printing Date : " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"), 16, 10, 0);
                cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Powered By: BUTS", 130, 10, 0);
                cb.EndText();
            }
            // step 5: we close the document
            document.Close();
            return ms.ToArray();
        }

        //
        protected virtual PdfPTable PrintInvoiceHeader(string reportHeader, Branch branch, Font headerTitleFont, Font addressFont, Font reportTitleFont)
        {

            //header
            var headerTable = new PdfPTable(1)
            {
                WidthPercentage = 65f
                // RunDirection = GetDirection(lang)
            };
            headerTable.SetTotalWidth(new float[] { 100 });
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


        //Sale Invoice
        public void PrintSaleInvoiceToPdf(Stream stream, Customer customer, Branch branch, Sale sale, List<SaleItemModel> saleItems)
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
            var headerTable = PrintInvoiceHeader("Sale Invoice", branch, headerTitleFont, addressFont, reportTitleFont);
            //Data
            PrintSaleInvoice(stream, headerTable, titleFont, customer, branch, sale, saleItems, defaultFont, attributesFont);
            //doc.Close();
        }


        protected virtual void PrintSaleInvoice(Stream stream, PdfPTable headerTable, Font titleFont, Customer customer, Branch branch, Sale sale, List<SaleItemModel> saleItems, Font font, Font attributesFont)
        {

            var numColumns = 1;
            //print invoice info
            var invoiceInfoTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f,


            };
            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 100 } }
            };
            invoiceInfoTable.SetTotalWidth(new float[] { 100 });

            invoiceInfoTable.SetWidths(widths[numColumns]);
            var cellInvoiceInfo = GetPdfCell("", font);
            cellInvoiceInfo.Phrase.Add(new Phrase(sale.CreatedOn.ToString("dd/MM/yyyy") + "   " + sale.CreatedOn.ToString("hh:mm tt"), titleFont));
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            cellInvoiceInfo = GetPdfCell("", font);
            cellInvoiceInfo.Phrase.Add(new Phrase("Bill No:" + sale.SaleNo, titleFont));
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
                WidthPercentage = 100f
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
            saleAccountInfoTable.SetTotalWidth(new float[] { 50, 50 });

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
            cellfooterInfo.Phrase.Add(new Phrase("Sold goods are returnable with saleable condition", titleFont));
            cellfooterInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellfooterInfo.Border = Rectangle.NO_BORDER;
            footerInfoTable.AddCell(cellfooterInfo);

            cellfooterInfo = GetPdfCell("", font);
            cellfooterInfo.Phrase.Add(new Phrase("Powered By: BUTS", titleFont));
            cellfooterInfo.HorizontalAlignment = Element.ALIGN_CENTER;
            cellfooterInfo.Border = Rectangle.NO_BORDER;
            footerInfoTable.AddCell(cellfooterInfo);

            var tableHeight = CalculatePdfPTableHeight(purchaseItemTable);
            var invoiceTableHeight = CalculatePdfPTableHeight(invoiceInfoTable);
            var headerTableHeight = CalculatePdfPTableHeight(headerTable);
            var accountinfoTableheight = CalculatePdfPTableHeight(saleAccountInfoTable);
            var footerinfoTableheight = CalculatePdfPTableHeight(footerInfoTable);

            Document doc = new Document(new iTextSharp.text.Rectangle(0, 0, 210, invoiceTableHeight + headerTableHeight + accountinfoTableheight + tableHeight + footerinfoTableheight), 16.0f, 16.0f, 0, 0);
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


        //Stock
        public virtual void PrintStockReportToPdf(Stream stream, Branch branch, IList<StockModel> stockReportLines, string reportName)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (stockReportLines == null)
                throw new ArgumentNullException(nameof(stockReportLines));

            var pageSize = PageSize.A4;
            var doc = new Document(pageSize, 20, 20, 25, 100);
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
            defaultFont.Size = 8;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            DateTime? fromDate = null;

            DateTime? toDate = null;
            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            // DrawHorizontalLineProtrait(pdfWriter);
            //Data
            PrintStockReport(reportTitleFont, doc, stockReportLines, defaultFont, attributesFont);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = stockReportLines.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }

        protected virtual void PrintStockReport(Font titleFont, Document doc, IList<StockModel> stockReportLines, Font font, Font attributesFont, int supplierId = 0)
        {
            var numColumns = 5;
            var stockReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 10,30,35,10,15 } }
            };

            stockReportTable.SetWidths(widths[numColumns]);

            //SL
            var cellStockReportItem = GetPdfCell("SL", font);
            cellStockReportItem.BackgroundColor = BaseColor.LightGray;
            cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            stockReportTable.AddCell(cellStockReportItem);

            //Brand
            cellStockReportItem = GetPdfCell("Product Details", font);
            cellStockReportItem.BackgroundColor = BaseColor.LightGray;
            cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            stockReportTable.AddCell(cellStockReportItem);

            //SupplierName
            cellStockReportItem = GetPdfCell("Supplier", font);
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

                //Product Details
                cellStockReportItem = GetPdfCell(pl.ProductName + " " + pl.ProductPackSize + " " + pl.ProductCategory, font);
                cellStockReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                stockReportTable.AddCell(cellStockReportItem);

                //Supplier Name
                cellStockReportItem = GetPdfCell(pl.SupplierName ?? " ", font);
                cellStockReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                stockReportTable.AddCell(cellStockReportItem);

                //Quantity
                cellStockReportItem = GetPdfCell(pl.Quantity.ToString("#,##0"), font);
                cellStockReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                stockReportTable.AddCell(cellStockReportItem);

                //Stock Value
                cellStockReportItem = GetPdfCell(pl.Value.ToString("#,##0.00"), font);
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

            doc.Add(stockReportTable);
        }


        //Out Of Stock
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
            defaultFont.Size = 8;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            DateTime? fromDate = null;

            DateTime? toDate = null;
            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            // DrawHorizontalLineProtrait(pdfWriter);
            //Data
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
                { numColumns, new[] { 8,40,37,15 } }
            };

            stockReportTable.SetWidths(widths[numColumns]);

            //SL
            var cellStockReportItem = GetPdfCell("SL", font);
            cellStockReportItem.BackgroundColor = BaseColor.LightGray;
            cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            stockReportTable.AddCell(cellStockReportItem);

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

            stockReportTable.HeaderRows = 1;
            var r = 0;
            foreach (var pl in stockReportLines)
            {
                r++;

                //SL
                cellStockReportItem = GetPdfCell(r, font);
                cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                stockReportTable.AddCell(cellStockReportItem);

                //Product Details
                cellStockReportItem = GetPdfCell(pl.ProductName + " " + pl.ProductPackSize + " " + pl.ProductCategory, font);
                cellStockReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                stockReportTable.AddCell(cellStockReportItem);

                //SupplierName
                cellStockReportItem = GetPdfCell(pl.SupplierName, font);
                cellStockReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                stockReportTable.AddCell(cellStockReportItem);

                //CurrentStock
                cellStockReportItem = GetPdfCell("Out Of Stock", font);
                cellStockReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                stockReportTable.AddCell(cellStockReportItem);

            }

            doc.Add(stockReportTable);
        }


        //Profit
        public virtual void PrintProfitReportToPdf(Stream stream, Branch branch, IList<PLModel> profitReportLines, string reportName, DateTime? fromDate, DateTime? toDate)
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
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            // DrawHorizontalLineProtrait(pdfWriter);
            //Data
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


        //Balance
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
            //Data
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


        //Supplier Ledger
        public virtual void PrintSupplierLedgerReportToPdf(Stream stream, Supplier supplier, decimal? prevBalance, IList<SupplierLedgerReportLine> supplierReportLines, string reportName, DateTime? fromDate, DateTime? toDate)
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
            //Supplier Ledger
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

        protected virtual void PrintSupplierLedgerReport(Font titleFont, Document doc, IList<SupplierLedgerReportLine> supplierLedgerReportLines, decimal? prevBalance, Font font, Font attributesFont)
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


        //Customer Ledger
        public virtual void PrintCustomerLedgerReportToPdf(Stream stream, Customer customer, decimal? prevBalance, IList<CustomerLedgerReportLine> customerReportLines, string reportName, DateTime? fromDate, DateTime? toDate)
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
            //Data
            PrintCustomerLedgerReport(reportTitleFont, doc, customerReportLines, prevBalance, defaultFont, attributesFont);

            var customerReportLineNumber = 20;
            var customerReportLineNumberCount = customerReportLines.Count;
            customerReportLineNumber++;

            if (customerReportLineNumber <= customerReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
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

        protected virtual void PrintCustomerLedgerReport(Font titleFont, Document doc, IList<CustomerLedgerReportLine> customerLedgerReportLines, decimal? prevBalance, Font font, Font attributesFont)
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


        //Trail Balance
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
            defaultFont.Size = 8;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.NORMAL);


            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Data
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

        protected virtual void PrintTrialBalanceReport(Font titleFont, Document doc, IList<TrialBalanceViewModel> trialBalanceReportLines, Font font, Font attributesFont, int supplierId = 0)
        {
            var redColorFont = GetFont();
            redColorFont.Color = new BaseColor(113, 18, 26, 255);
            redColorFont.Size = 8;

            var blueColorFont = GetFont();
            blueColorFont.Color = BaseColor.Blue;
            blueColorFont.Size = 8;

            var magentaColorFont = GetFont();
            magentaColorFont.Color = BaseColor.Magenta;
            magentaColorFont.Size = 8;

            var pinkColorFont = GetFont();
            pinkColorFont.Color = BaseColor.Pink;
            pinkColorFont.Size = 8;

            var blackColorFont = GetFont();
            blackColorFont.Color = BaseColor.Black;
            blackColorFont.Size = 8;


            var numColumns = 8;
            var purchaseReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 7,23,11,11,12,12,12,12 } }
            };

            purchaseReportTable.SetWidths(widths[numColumns]);

            //headline
            //
            var cellPurchaseReportItem = GetPdfCell("", font);
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            /*Opening*/
            cellPurchaseReportItem = GetPdfCell("Opening", font);
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            /*Transaction for the period*/
            cellPurchaseReportItem = GetPdfCell("Transaction for the period", font);
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            /*Balance*/
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

            /*Opening*/
            //Debit
            cellPurchaseReportItem = GetPdfCell("Debit", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Credit
            cellPurchaseReportItem = GetPdfCell("Credit", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);


            /*Transaction for the period*/
            //Debit
            cellPurchaseReportItem = GetPdfCell("Debit", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Credit
            cellPurchaseReportItem = GetPdfCell("Credit", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);


            /*Balance*/
            //Debit
            cellPurchaseReportItem = GetPdfCell("Debit", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Credit
            cellPurchaseReportItem = GetPdfCell("Credit", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);


            purchaseReportTable.HeaderRows = 1;
            foreach (var item in trialBalanceReportLines)
            {
                if (item.BalanceCredit == 0M && item.BalanceDebit == 0M) continue;

                //Code
                cellPurchaseReportItem = GetPdfCell(item.Code, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                //A/C Name
                switch (item.Level)
                {
                    case 1:
                        cellPurchaseReportItem = GetPdfCell(item.Name, blueColorFont);
                        break;
                    case 2:
                        cellPurchaseReportItem = GetPdfCell(item.Name, magentaColorFont);
                        break;
                    case 3:
                        cellPurchaseReportItem = GetPdfCell(item.Name, redColorFont);
                        break;
                    case 4:
                        cellPurchaseReportItem = GetPdfCell(item.Name, blackColorFont);
                        break;
                    case 5:
                        cellPurchaseReportItem = GetPdfCell(item.Name, blackColorFont);
                        break;
                    default:
                        cellPurchaseReportItem = GetPdfCell(item.Name, blackColorFont);
                        break;
                }
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                //OpeningDebit
                cellPurchaseReportItem = GetPdfCell(item.OpeningDebit.ToString("#,##0.00"), font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //OpeningCredit
                cellPurchaseReportItem = GetPdfCell(item.OpeningCredit.ToString("#,##0.00"), font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                //ThisPeriodDebit
                cellPurchaseReportItem = GetPdfCell(item.ThisPeriodDebit.ToString("#,##0.00"), font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //ThisPeriodCredit
                cellPurchaseReportItem = GetPdfCell(item.ThisPeriodCredit.ToString("#,##0.00"), font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                //BalanceDebit
                cellPurchaseReportItem = GetPdfCell(item.BalanceDebit.ToString("#,##0.00"), font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //BalanceCredit
                cellPurchaseReportItem = GetPdfCell(item.BalanceCredit.ToString("#,##0.00"), font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }

            //..................total..................................
            var levelOneTrialBalanceReportLines = trialBalanceReportLines.Where(x => x.Level == 1);

            //Total
            cellPurchaseReportItem = GetPdfCell("Total", font);
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //Total OpeningDebit
            cellPurchaseReportItem = GetPdfCell(levelOneTrialBalanceReportLines.Where(x => x.Level == 1).Sum(x => x.OpeningDebit).ToString("#,##0.00"), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //Total OpeningCredit
            cellPurchaseReportItem = GetPdfCell(levelOneTrialBalanceReportLines.Sum(x => x.OpeningCredit).ToString("#,##0.00"), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //Total ThisPeriodDebit
            cellPurchaseReportItem = GetPdfCell(levelOneTrialBalanceReportLines.Sum(x => x.ThisPeriodDebit).ToString("#,##0.00"), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //total ThisPeriodCredit
            cellPurchaseReportItem = GetPdfCell(levelOneTrialBalanceReportLines.Sum(x => x.ThisPeriodCredit).ToString("#,##0.00"), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //Total BalanceDebit
            cellPurchaseReportItem = GetPdfCell(levelOneTrialBalanceReportLines.Sum(x => x.BalanceDebit).ToString("#,##0.00"), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //Total BalanceCredit
            cellPurchaseReportItem = GetPdfCell(levelOneTrialBalanceReportLines.Sum(x => x.BalanceCredit).ToString("#,##0.00"), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            doc.Add(purchaseReportTable);
        }


        //Balance Sheet
        public virtual void PrintBalanceSheetReportToPdf(Stream stream, Branch branch, IList<AccountHeadWithBalanceViewModel> balanceSheetReportLines, string reportName, DateTime? fromDate, DateTime? toDate, decimal netIncome)
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
            //Data
            PrintBalanceSheetReport(reportTitleFont, doc, balanceSheetReportLines, defaultFont, attributesFont, netIncome);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = balanceSheetReportLines.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
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
            //A/C Name
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
            foreach (var item in balanceSheetReportLines)
            {
                if (item.Balance1 == 0M && item.Balance2 == 0M && item.Balance3 == 0M && item.Balance4 == 0M && item.Balance5 == 0M) continue;

                //ItemName
                switch (item.Level)
                {
                    case 1:
                        cellPurchaseReportItem = GetPdfCell(item.Name, blueColorFont);
                        break;
                    case 2:
                        cellPurchaseReportItem = GetPdfCell(item.Name, magentaColorFont);
                        break;
                    case 3:
                        cellPurchaseReportItem = GetPdfCell(item.Name, redColorFont);
                        break;
                    case 4:
                        cellPurchaseReportItem = GetPdfCell(item.Name, blackColorFont);
                        break;
                    case 5:
                        cellPurchaseReportItem = GetPdfCell(item.Name, blackColorFont);
                        break;
                    default:
                        cellPurchaseReportItem = GetPdfCell(item.Name, blackColorFont);
                        break;
                }

                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //Balance5
                cellPurchaseReportItem = GetPdfCell(item.Balance5 == 0M ? "" : item.Balance5.ToString("#,##0.00"), font);
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //Balance4
                cellPurchaseReportItem = GetPdfCell(item.Balance4 == 0M ? "" : item.Balance4.ToString("#,##0.00"), font);
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //Balance3
                cellPurchaseReportItem = GetPdfCell(item.Balance3 == 0M ? "" : item.Balance3.ToString("#,##0.00"), font);
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //Balance2
                cellPurchaseReportItem = GetPdfCell(item.Balance2 == 0M ? "" : item.Balance2.ToString("#,##0.00"), font);
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //Balance1
                cellPurchaseReportItem = GetPdfCell(item.Balance1 == 0M ? "" : item.Balance1.ToString("#,##0.00"), font);
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }

            //..................total..................................
            //Net Income/Loss
            cellPurchaseReportItem = GetPdfCell("Net Income/Loss", font);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //netIncome
            cellPurchaseReportItem = GetPdfCell(netIncome, font);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            doc.Add(purchaseReportTable);
        }


        //Income Statement
        public virtual void PrintIncomeStatementReportToPdf(Stream stream, Branch branch, IList<AccountHeadWithBalanceViewModel> incomeStatementReportLines, string reportName, DateTime? fromDate, DateTime? toDate, decimal netIncome = 0)
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
            //Data
            PrintIncomeStatementReport(reportTitleFont, doc, incomeStatementReportLines, defaultFont, attributesFont, netIncome);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = incomeStatementReportLines.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }

        protected virtual void PrintIncomeStatementReport(Font titleFont, Document doc, IList<AccountHeadWithBalanceViewModel> incomeStatementReportLines, Font font, Font attributesFont, decimal netIncome = 0)
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
            //A/C Name
            var cellPurchaseReportItem = GetPdfCell("A/C Name", font);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            // Empty Cell
            cellPurchaseReportItem = GetPdfCell("", font);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            purchaseReportTable.HeaderRows = 1;

            foreach (var item in incomeStatementReportLines)
            {
                if (item.Balance1 == 0M && item.Balance2 == 0M && item.Balance3 == 0M && item.Balance4 == 0M && item.Balance5 == 0M) continue;

                //ItemName
                switch (item.Level)
                {
                    case 1:
                        cellPurchaseReportItem = GetPdfCell(item.Name, blueColorFont);
                        break;
                    case 2:
                        cellPurchaseReportItem = GetPdfCell(item.Name, magentaColorFont);
                        break;
                    case 3:
                        cellPurchaseReportItem = GetPdfCell(item.Name, redColorFont);
                        break;
                    case 4:
                        cellPurchaseReportItem = GetPdfCell(item.Name, blackColorFont);
                        break;
                    case 5:
                        cellPurchaseReportItem = GetPdfCell(item.Name, blackColorFont);
                        break;
                    default:
                        cellPurchaseReportItem = GetPdfCell(item.Name, blackColorFont);
                        break;
                }

                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                //Balance5
                cellPurchaseReportItem = GetPdfCell(item.Balance5 == 0M ? "" : item.Balance5.ToString("#,##0.00"), font);
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //Balance4
                cellPurchaseReportItem = GetPdfCell(item.Balance4 == 0M ? "" : item.Balance4.ToString("#,##0.00"), font);
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //Balance3
                cellPurchaseReportItem = GetPdfCell(item.Balance3 == 0M ? "" : item.Balance3.ToString("#,##0.00"), font);
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //Balance2
                cellPurchaseReportItem = GetPdfCell(item.Balance2 == 0M ? "" : item.Balance2.ToString("#,##0.00"), font);
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //Balance1
                cellPurchaseReportItem = GetPdfCell(item.Balance1 == 0M ? "" : item.Balance1.ToString("#,##0.00"), font);
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }

            //..................total..................................
            //Net Income/Loss
            cellPurchaseReportItem = GetPdfCell("Net Income/Loss", font);
            // cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 3;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //netIncome
            cellPurchaseReportItem = GetPdfCell(netIncome, font);
            //    cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 3;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            doc.Add(purchaseReportTable);
        }


        //Expense
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
            defaultFont.Size = 8;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDatde);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Data
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

                //BrandName
                cellExpenseReportItem = GetPdfCell(pl.Remark, font);
                cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                expenseReportTable.AddCell(cellExpenseReportItem);

                //CurrentExpense
                cellExpenseReportItem = GetPdfCell(pl.Amount.ToString("#,##0.00"), font);
                cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                expenseReportTable.AddCell(cellExpenseReportItem);

            }
            //..................total..................................
            //SL
            cellExpenseReportItem = GetPdfCell("Total", font);
            cellExpenseReportItem.Colspan = 4;
            cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            expenseReportTable.AddCell(cellExpenseReportItem);

            //CurrentExpense
            cellExpenseReportItem = GetPdfCell(expenseReportLines.Sum(x => x.Amount).ToString("#,##0.00")
                , font);
            cellExpenseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            expenseReportTable.AddCell(cellExpenseReportItem);

            doc.Add(expenseReportTable);
        }


        //Sale
        public virtual void PrintSaleReportToPdf(Stream stream, Branch branch, IList<SaleViewModel> saleReportLines, string reportName, DateTime? fromDate, DateTime? toDate)
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
            defaultFont.Size = 8;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Data
            PrintSaleReportTestingPurpose(reportTitleFont, doc, saleReportLines, defaultFont, attributesFont, fromDate, toDate, branch);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = saleReportLines.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }

        protected virtual void PrintSaleReportTestingPurpose(Font titleFont, Document doc, IList<SaleViewModel> saleReportLines, Font font, Font attributesFont, DateTime? fromDate, DateTime? toDate, Branch branch)
        {
            //sale table
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

            saleReportTable.AddCell(new PdfPCell(new Phrase("SL", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReportTable.AddCell(new PdfPCell(new Phrase("Invoice No.", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReportTable.AddCell(new PdfPCell(new Phrase("Customer Name", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReportTable.AddCell(new PdfPCell(new Phrase("Branch Code", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReportTable.AddCell(new PdfPCell(new Phrase("Sale Date", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReportTable.AddCell(new PdfPCell(new Phrase("Subtotal", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReportTable.AddCell(new PdfPCell(new Phrase("Discount", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReportTable.AddCell(new PdfPCell(new Phrase("Payable", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReportTable.AddCell(new PdfPCell(new Phrase("Paid", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReportTable.AddCell(new PdfPCell(new Phrase("Due", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });

            //SL
            var cellSaleReportItem = GetPdfCell("SL", font);

            saleReportTable.HeaderRows = 1;
            var sl = 0;
            foreach (var pl in saleReportLines)
            {
                sl++;

                saleReportTable.AddCell(new PdfPCell(new Phrase(sl.ToString(), font)) { HorizontalAlignment = Element.ALIGN_CENTER });
                saleReportTable.AddCell(new PdfPCell(new Phrase(pl.SaleNo, font)) { HorizontalAlignment = Element.ALIGN_CENTER });
                saleReportTable.AddCell(new PdfPCell(new Phrase(pl.CustomerName, font)) { HorizontalAlignment = Element.ALIGN_LEFT });
                saleReportTable.AddCell(new PdfPCell(new Phrase(pl.BranchCode, font)) { HorizontalAlignment = Element.ALIGN_CENTER });
                saleReportTable.AddCell(new PdfPCell(new Phrase(pl.SaleDate.ToString("dd/MM/yyyy"), font)) { HorizontalAlignment = Element.ALIGN_CENTER });
                saleReportTable.AddCell(new PdfPCell(new Phrase(pl.Subtotal.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                saleReportTable.AddCell(new PdfPCell(new Phrase(pl.Discount.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                saleReportTable.AddCell(new PdfPCell(new Phrase(pl.PayableAmount.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                saleReportTable.AddCell(new PdfPCell(new Phrase(pl.PaidAmount.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                saleReportTable.AddCell(new PdfPCell(new Phrase(pl.DueAmount.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });

                //sale item table.............................................
                var numColumnsOfSaleItemTable = 2;
                var saleItemReportTable = new PdfPTable(numColumnsOfSaleItemTable)
                {
                    //  RunDirection = GetDirection(lang),
                    WidthPercentage = 100f
                };

                var widthsSaleItem = new Dictionary<int, int[]> { { numColumnsOfSaleItemTable, new[] { 70, 30 } } };

                saleItemReportTable.SetWidths(widthsSaleItem[numColumnsOfSaleItemTable]);

                saleItemReportTable.AddCell(new PdfPCell(new Phrase("Item Name", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
                saleItemReportTable.AddCell(new PdfPCell(new Phrase("Amount", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });

                foreach (var item in pl.SaleItems)
                {
                    saleItemReportTable.AddCell(new PdfPCell(new Phrase(item?.ProductName, font)) { HorizontalAlignment = Element.ALIGN_LEFT });
                    saleItemReportTable.AddCell(new PdfPCell(new Phrase(item?.Amount.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                }

                //saleReportTable.AddCell(new PdfPCell(new Phrase("Items", font)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                //saleItemReportTable.AddCell(new PdfPCell(new Phrase("", font)) { Colspan = 8, HorizontalAlignment = Element.ALIGN_CENTER });

                //Adding table to the cell
                cellSaleReportItem = GetPdfCell("Items", font);
                cellSaleReportItem.Colspan = 2;
                saleReportTable.AddCell(cellSaleReportItem);

                cellSaleReportItem = GetPdfCell("", font);
                cellSaleReportItem.Colspan = 8;
                cellSaleReportItem.AddElement(saleItemReportTable);
                saleReportTable.AddCell(cellSaleReportItem);
            }

            saleReportTable.AddCell(new PdfPCell(new Phrase("Total", font)) { Colspan = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
            saleReportTable.AddCell(new PdfPCell(new Phrase(saleReportLines.Sum(x => x.Subtotal).ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            saleReportTable.AddCell(new PdfPCell(new Phrase(saleReportLines.Sum(x => x.Discount).ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            saleReportTable.AddCell(new PdfPCell(new Phrase(saleReportLines.Sum(x => x.PayableAmount).ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            saleReportTable.AddCell(new PdfPCell(new Phrase(saleReportLines.Sum(x => x.PaidAmount).ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            saleReportTable.AddCell(new PdfPCell(new Phrase(saleReportLines.Sum(x => x.DueAmount).ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });

            //..................total..................................

            if (fromDate.HasValue && toDate.HasValue && branch != null)
            {
                var saleReturnAmount = _commonService.GetSaleReturnTotal(fromDate.Value.Date, toDate.Value.Date, branch.Id);

                //..................total sale return..................................
                saleReportTable.AddCell(new PdfPCell(new Phrase("Sale Return", font)) { Colspan = 7, HorizontalAlignment = Element.ALIGN_RIGHT });
                saleReportTable.AddCell(new PdfPCell(new Phrase(saleReturnAmount.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                saleReportTable.AddCell(new PdfPCell(new Phrase("", font)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_RIGHT });

                //..................net sale..................................
                saleReportTable.AddCell(new PdfPCell(new Phrase("Net Sale", font)) { Colspan = 7, HorizontalAlignment = Element.ALIGN_RIGHT });
                saleReportTable.AddCell(new PdfPCell(new Phrase((saleReportLines.Sum(x => x.PayableAmount) - saleReturnAmount).ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                saleReportTable.AddCell(new PdfPCell(new Phrase("", font)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            doc.Add(saleReportTable);
        }

        protected virtual void PrintSaleReport(Font titleFont, Document doc, IList<SaleViewModel> saleReportLines, Font font, Font attributesFont, DateTime? fromDate, DateTime? toDate, Branch branch)
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
            //Invoice No.
            cellSaleReportItem = GetPdfCell("Invoice No.", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Customer Name
            cellSaleReportItem = GetPdfCell("Customer Name", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Branch Code
            cellSaleReportItem = GetPdfCell("Branch Code", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Sale Date
            cellSaleReportItem = GetPdfCell("SaleDate", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Subtotal
            cellSaleReportItem = GetPdfCell("Subtotal", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Discount
            cellSaleReportItem = GetPdfCell("Discount", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Payable
            cellSaleReportItem = GetPdfCell("Payable", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Paid
            cellSaleReportItem = GetPdfCell("Paid", font);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Due
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

            if (fromDate.HasValue && toDate.HasValue && branch != null)
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
                cellSaleReportItem = GetPdfCell(saleReportLines.Sum(x => x.PayableAmount) - saleReturnAmount, font);
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


        //Item Wise Sale Profit Loss
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
            reportTitleFont.SetStyle(Font.NORMAL);
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
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Data
            PrintItemWiseSaleProfitLossDataTable(reportTitleFont, doc, saleReportLines, defaultFont, attributesFont, fromDate, toDate, branch);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = saleReportLines.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }

        protected virtual void PrintItemWiseSaleProfitLossDataTable(Font titleFont, Document doc, IList<ItemSaleProfitLossViewModel> saleReportLines, Font font, Font attributesFont, DateTime? fromDate, DateTime? toDate, Branch branch)
        {
            PdfPTable itemWiseSaleProfitLossReportTable = new(9);
            itemWiseSaleProfitLossReportTable.WidthPercentage = 100;
            float[] widths = new float[] { 6, 19, 22, 7, 10, 8, 10, 10, 10 };
            itemWiseSaleProfitLossReportTable.SetWidths(widths);

            itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase("SL", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase("Product Name", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase("Supplier Name", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase("Sale Quantity", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase("Subtotal", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase("Discount", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase("Net Amount", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase("COGS", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase("PL", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });

            itemWiseSaleProfitLossReportTable.HeaderRows = 1;
            var r = 0;
            foreach (var pl in saleReportLines)
            {
                r++;

                itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase(r.ToString(), font)) { HorizontalAlignment = Element.ALIGN_CENTER });
                itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase(pl.ProductName, font)) { HorizontalAlignment = Element.ALIGN_LEFT });
                itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase(pl.SupplierName, font)) { HorizontalAlignment = Element.ALIGN_LEFT });
                itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase(pl.SaleQuantity.ToString("#,##0"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase(pl.Subtotal.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase(pl.Discount.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase(pl.NetAmount.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase(pl.Cogs.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase((pl.NetAmount - pl.Cogs).ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            //..................total..................................
            itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase("Total", font)) { Colspan = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase(saleReportLines.Sum(x => x.SaleQuantity).ToString("#,##0"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase(saleReportLines.Sum(x => x.Subtotal).ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase(saleReportLines.Sum(x => x.Discount).ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase(saleReportLines.Sum(x => x.NetAmount).ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase(saleReportLines.Sum(x => x.Cogs).ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            itemWiseSaleProfitLossReportTable.AddCell(new PdfPCell(new Phrase(saleReportLines.Sum(x => x.NetAmount - x.Cogs).ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });

            doc.Add(itemWiseSaleProfitLossReportTable);
        }


        //Item Wise Purchase
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
            defaultFont.Size = 8;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Data
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
                { numColumns, new[] { 6,24,24,8,8,11,8,11 } }
            };

            saleReportTable.SetWidths(widths[numColumns]);

            saleReportTable.AddCell(new PdfPCell(new Phrase("SL", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReportTable.AddCell(new PdfPCell(new Phrase("Product Name", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReportTable.AddCell(new PdfPCell(new Phrase("Supplier Name", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReportTable.AddCell(new PdfPCell(new Phrase("Purchase Qty", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReportTable.AddCell(new PdfPCell(new Phrase("Bonus Qty", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReportTable.AddCell(new PdfPCell(new Phrase("Subtotal", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReportTable.AddCell(new PdfPCell(new Phrase("Discount", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReportTable.AddCell(new PdfPCell(new Phrase("Net Amount", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });

            saleReportTable.HeaderRows = 1;
            var r = 0;
            foreach (var pl in saleReportLines)
            {
                r++;

                saleReportTable.AddCell(new PdfPCell(new Phrase(r.ToString(), font)) { HorizontalAlignment = Element.ALIGN_CENTER });
                saleReportTable.AddCell(new PdfPCell(new Phrase(pl.ProductName, font)) { HorizontalAlignment = Element.ALIGN_LEFT });
                saleReportTable.AddCell(new PdfPCell(new Phrase(pl.SupplierName, font)) { HorizontalAlignment = Element.ALIGN_LEFT });
                saleReportTable.AddCell(new PdfPCell(new Phrase(pl.PurchaseQuantity.ToString("#,##0"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                saleReportTable.AddCell(new PdfPCell(new Phrase(pl.BonusQuantity.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                saleReportTable.AddCell(new PdfPCell(new Phrase(pl.Subtotal.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                saleReportTable.AddCell(new PdfPCell(new Phrase(pl.Discount.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                saleReportTable.AddCell(new PdfPCell(new Phrase(pl.NetAmount.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            //..................total..................................
            saleReportTable.AddCell(new PdfPCell(new Phrase("Total", font)) { Colspan = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            saleReportTable.AddCell(new PdfPCell(new Phrase(saleReportLines.Sum(x => x.PurchaseQuantity).ToString("#,##0"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            saleReportTable.AddCell(new PdfPCell(new Phrase(saleReportLines.Sum(x => x.BonusQuantity).ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            saleReportTable.AddCell(new PdfPCell(new Phrase(saleReportLines.Sum(x => x.Subtotal).ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            saleReportTable.AddCell(new PdfPCell(new Phrase(saleReportLines.Sum(x => x.Discount).ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            saleReportTable.AddCell(new PdfPCell(new Phrase(saleReportLines.Sum(x => x.NetAmount).ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });

            doc.Add(saleReportTable);
        }


        //Supplier Balance
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
            PrintSupplierBalanceReportHeader(pdfWriter, "Supplier Balance Report", headerTitleFont, addressFont, reportTitleFont, doc);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Data
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


        //Customer Balance
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
            //Data
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


        //Supplier Payment
        public void PrintSupplierPaymentReportToPdf(Stream stream, List<SupplierPaymentViewModel> supplierpayments, string message, Branch branch, DateTime? fromDate, DateTime? toDate)
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
            defaultFont.Size = 8;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            //headers
            PrintReportHeader(pdfWriter, "Supplier Payment Report", branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Data
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

        private void PrintSupplierPaymentReport(Font reportTitleFont, Document doc, List<SupplierPaymentViewModel> supplierpayments, Font defaultFont, Font attributesFont)
        {
            var numColumns = 6;
            var supplierPaymentReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 8,12,12,30,18,20 } }
            };

            supplierPaymentReportTable.SetWidths(widths[numColumns]);

            supplierPaymentReportTable.AddCell(new PdfPCell(new Phrase("SL", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            supplierPaymentReportTable.AddCell(new PdfPCell(new Phrase("Bill-No", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            supplierPaymentReportTable.AddCell(new PdfPCell(new Phrase("Payment Date", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            supplierPaymentReportTable.AddCell(new PdfPCell(new Phrase("Supplier Name", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            supplierPaymentReportTable.AddCell(new PdfPCell(new Phrase("Payment Method Name", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            supplierPaymentReportTable.AddCell(new PdfPCell(new Phrase("Supplier Payment", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });

            supplierPaymentReportTable.HeaderRows = 1;

            var r = 0;
            foreach (var sl in supplierpayments)
            {
                r++;

                supplierPaymentReportTable.AddCell(new PdfPCell(new Phrase(r.ToString(), defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                supplierPaymentReportTable.AddCell(new PdfPCell(new Phrase(sl.BillNo, defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                supplierPaymentReportTable.AddCell(new PdfPCell(new Phrase(sl.PaymentDate.ToString("dd/MM/yy"), defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                supplierPaymentReportTable.AddCell(new PdfPCell(new Phrase(sl.Supplier?.Name, defaultFont)) { HorizontalAlignment = Element.ALIGN_LEFT });
                supplierPaymentReportTable.AddCell(new PdfPCell(new Phrase(sl.PaymentMethod?.Name, defaultFont)) { HorizontalAlignment = Element.ALIGN_LEFT });
                supplierPaymentReportTable.AddCell(new PdfPCell(new Phrase(sl.PaymentAmount.ToString("#,##0.00"), defaultFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            }
            //..................total..................................
            supplierPaymentReportTable.AddCell(new PdfPCell(new Phrase("Total Payment", defaultFont)) { Colspan = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
            supplierPaymentReportTable.AddCell(new PdfPCell(new Phrase(supplierpayments.Sum(x => x.PaymentAmount).ToString("#,##0.00"), defaultFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });

            doc.Add(supplierPaymentReportTable);
        }


        //Customer Payment
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
            defaultFont.Size = 8;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            //headers
            PrintReportHeader(pdfWriter, "Customer Payment Report", branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Data
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

            customerPaymentReportTable.AddCell(new PdfPCell(new Phrase("SL", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            customerPaymentReportTable.AddCell(new PdfPCell(new Phrase("Bill-No", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            customerPaymentReportTable.AddCell(new PdfPCell(new Phrase("Customer Name", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            customerPaymentReportTable.AddCell(new PdfPCell(new Phrase("Customer Payment", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });

            customerPaymentReportTable.HeaderRows = 1;

            var sl = 0;
            foreach (var item in customerpayments)
            {
                sl++;

                customerPaymentReportTable.AddCell(new PdfPCell(new Phrase(sl.ToString(), defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                customerPaymentReportTable.AddCell(new PdfPCell(new Phrase(item.BillNo, defaultFont)) { HorizontalAlignment = Element.ALIGN_LEFT });
                customerPaymentReportTable.AddCell(new PdfPCell(new Phrase(item.Customer?.Name, defaultFont)) { HorizontalAlignment = Element.ALIGN_LEFT });
                customerPaymentReportTable.AddCell(new PdfPCell(new Phrase(item.PaymentAmount.ToString("#,##0.00"), defaultFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            //..................total..................................
            customerPaymentReportTable.AddCell(new PdfPCell(new Phrase("Total Payment", defaultFont)) { Colspan = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            customerPaymentReportTable.AddCell(new PdfPCell(new Phrase(customerpayments.Sum(x => x.CustomerBalance).ToString("#,##0.00"), defaultFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });

            doc.Add(customerPaymentReportTable);
        }


        //Sale Return
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
            defaultFont.Size = 8;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            //Header
            PrintReportHeader(pdfWriter, "Sale Return Report", branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Data
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
                { numColumns, new[] { 10,12,20,25,13,20 } }
            };

            saleReturnReportTable.SetWidths(widths[numColumns]);

            saleReturnReportTable.AddCell(new PdfPCell(new Phrase("SL", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReturnReportTable.AddCell(new PdfPCell(new Phrase("Date", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReturnReportTable.AddCell(new PdfPCell(new Phrase("Invoice No.", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReturnReportTable.AddCell(new PdfPCell(new Phrase("Customer Name", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReturnReportTable.AddCell(new PdfPCell(new Phrase("Qty", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            saleReturnReportTable.AddCell(new PdfPCell(new Phrase("Amount", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });

            saleReturnReportTable.HeaderRows = 1;

            var r = 0;
            foreach (var sl in item1)
            {
                r++;

                saleReturnReportTable.AddCell(new PdfPCell(new Phrase(r.ToString(), defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                saleReturnReportTable.AddCell(new PdfPCell(new Phrase(sl.SaleReturnDate.ToString("dd/MM/yy"), defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                saleReturnReportTable.AddCell(new PdfPCell(new Phrase(sl.SaleNo, defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                saleReturnReportTable.AddCell(new PdfPCell(new Phrase(sl.CustomerName, defaultFont)) { HorizontalAlignment = Element.ALIGN_LEFT });
                saleReturnReportTable.AddCell(new PdfPCell(new Phrase(sl.TotalItem.ToString("#,##0"), defaultFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                saleReturnReportTable.AddCell(new PdfPCell(new Phrase(sl.Subtotal.ToString("#,##0.00"), defaultFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            saleReturnReportTable.AddCell(new PdfPCell(new Phrase("Total", defaultFont)) { Colspan = 4, HorizontalAlignment = Element.ALIGN_RIGHT });
            saleReturnReportTable.AddCell(new PdfPCell(new Phrase(item1.Sum(x => x.TotalItem).ToString("#,##0"), defaultFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            saleReturnReportTable.AddCell(new PdfPCell(new Phrase(item1.Sum(x => x.Subtotal).ToString("#,##0.00"), defaultFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });

            doc.Add(saleReturnReportTable);
        }


        //Subsidiary Ledger
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
            defaultFont.Size = 8;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.NORMAL);

            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Data
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

        protected virtual void PrintSubsidiaryLedgerReport(Font titleFont, Document doc, IList<SubsidiaryLedgerViewModel> subsidiaryLedgerReportLines, Font font, Font attributesFont, int supplierId = 0)
        {
            var redColorFont = GetFont();
            redColorFont.SetStyle(Font.BOLD);
            redColorFont.Color = new BaseColor(113, 18, 26, 255);
            redColorFont.Size = 8;

            var numColumns = 6;
            var purchaseReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] {12,12,28,16,16,16 } }
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
            //Transaction for the period
            cellPurchaseReportItem = GetPdfCell("Transaction for the period", font);
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //
            cellPurchaseReportItem = GetPdfCell("", font);
            cellPurchaseReportItem.Colspan = 1;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //VNumber
            cellPurchaseReportItem = GetPdfCell("VNumber", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Date
            cellPurchaseReportItem = GetPdfCell("Date", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Particular
            cellPurchaseReportItem = GetPdfCell("Particular", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Debit
            cellPurchaseReportItem = GetPdfCell("Debit", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Credit
            cellPurchaseReportItem = GetPdfCell("Credit", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Balance
            cellPurchaseReportItem = GetPdfCell("Balance", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            purchaseReportTable.HeaderRows = 1;

            foreach (var pl in subsidiaryLedgerReportLines)
            {
                //Vnumber
                cellPurchaseReportItem = GetPdfCell(pl.Vnumber, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //VoucherDate
                cellPurchaseReportItem = GetPdfCell(pl.VoucherDate.ToString("dd/MM/yyyy"), font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //Particular
                cellPurchaseReportItem = GetPdfCell(pl.Particular, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //Debit
                cellPurchaseReportItem = GetPdfCell(pl.Debit.ToString("#,##0.00"), font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //Credit
                cellPurchaseReportItem = GetPdfCell(pl.Credit.ToString("#,##0.00"), font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //Balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance.ToString("#,##0.00"), font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }

            //..................total..................................
            var totalDebit = subsidiaryLedgerReportLines.Sum(x => x.Debit);
            var totalCredit = subsidiaryLedgerReportLines.Sum(x => x.Credit);

            //Total Transaction
            cellPurchaseReportItem = GetPdfCell("Total Transaction", redColorFont);
            cellPurchaseReportItem.Colspan = 3;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Total Debit
            cellPurchaseReportItem = GetPdfCell(totalDebit.ToString("#,##0.00"), redColorFont);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Total Credit
            cellPurchaseReportItem = GetPdfCell(totalCredit.ToString("#,##0.00"), redColorFont);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //TotalDebit - TotalCredit
            cellPurchaseReportItem = GetPdfCell(Math.Abs(totalDebit - totalCredit).ToString("#,##0.00"), redColorFont);
            cellPurchaseReportItem.Colspan = 1;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            doc.Add(purchaseReportTable);
        }


        //Purchase Invoice
        public void PrintPurchaseInvoiceToPdf(MemoryStream stream, Supplier? supplier, Branch? branch, Purchase purchase, List<PurchaseItemModel> purchaseItems)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (purchase == null)
                throw new ArgumentNullException(nameof(purchase));

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

            var titleFont = GetFont();
            titleFont.SetFamily("Times New Roman");
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;
            titleFont.Size = 8;

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
            var headerTable = PrintInvoiceHeader("Purchase Invoice", branch, headerTitleFont, addressFont, reportTitleFont);

            // Report
            PrintPurchaseInvoice(stream, headerTable, titleFont, supplier, branch, purchase, purchaseItems, defaultFont, attributesFont);

            //doc.Close();
        }

        protected virtual void PrintPurchaseInvoice(Stream stream, PdfPTable headerTable, Font titleFont, Supplier supplier, Branch branch, Purchase purchase, List<PurchaseItemModel> purchaseItems, Font font, Font attributesFont)
        {

            var numColumns = 1;
            //print invoice info
            var invoiceInfoTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f,
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 100 } }
            };

            invoiceInfoTable.SetTotalWidth(new float[] { 100 });

            invoiceInfoTable.SetWidths(widths[numColumns]);

            invoiceInfoTable.AddCell(new PdfPCell(new Phrase(purchase.CreatedOn.ToString("dd/MM/yyyy") + "   " + purchase.CreatedOn.ToString("hh:mm tt"), titleFont)) { HorizontalAlignment = Element.ALIGN_LEFT, Border = Rectangle.NO_BORDER });
            invoiceInfoTable.AddCell(new PdfPCell(new Phrase("Bill No:" + purchase.PurchaseNo, titleFont)) { HorizontalAlignment = Element.ALIGN_LEFT, Border = Rectangle.NO_BORDER });
            invoiceInfoTable.AddCell(new PdfPCell(new Phrase("Name: " + supplier.Name, titleFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_LEFT, Border = Rectangle.NO_BORDER });
            invoiceInfoTable.AddCell(new PdfPCell(new Phrase("Contact: " + supplier.Contact, titleFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_LEFT, Border = Rectangle.NO_BORDER });
            invoiceInfoTable.AddCell(new PdfPCell(new Phrase("Served By: " + _context.Users.Find(purchase.CreatedBy)?.Username, titleFont)) { HorizontalAlignment = Element.ALIGN_LEFT, Border = Rectangle.NO_BORDER });
            invoiceInfoTable.AddCell(new PdfPCell(new Phrase(Environment.NewLine, titleFont)) { HorizontalAlignment = Element.ALIGN_LEFT, Border = Rectangle.NO_BORDER });

            //print purchase items
            var purchaseItemTable = new PdfPTable(5)
            {
                // RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            purchaseItemTable.SetTotalWidth(new float[] { 33, 12, 12, 20, 23 });

            var widthsOfPurchaseItemTable = new Dictionary<int, int[]>
            {
               // { 5, new[] { 5,39,13,13,30 } }
               { 5, new[] { 33,12,12,20,23 } }
            };

            purchaseItemTable.SetWidths(widthsOfPurchaseItemTable[5]);

            purchaseItemTable.AddCell(new PdfPCell(new Phrase("Description", titleFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
            purchaseItemTable.AddCell(new PdfPCell(new Phrase("Qty", titleFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
            purchaseItemTable.AddCell(new PdfPCell(new Phrase("BQty", titleFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
            purchaseItemTable.AddCell(new PdfPCell(new Phrase("Price", titleFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
            purchaseItemTable.AddCell(new PdfPCell(new Phrase("Amount", titleFont)) { HorizontalAlignment = Element.ALIGN_CENTER });

            var sl = 0;
            foreach (var po in purchaseItems)
            {
                sl++;

                purchaseItemTable.AddCell(new PdfPCell(new Phrase(po.Description, font)) { HorizontalAlignment = Element.ALIGN_LEFT });
                purchaseItemTable.AddCell(new PdfPCell(new Phrase(po.Quantity.ToString("#,##0"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                purchaseItemTable.AddCell(new PdfPCell(new Phrase(po.BonusQuantity.ToString("#,##0"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                purchaseItemTable.AddCell(new PdfPCell(new Phrase(po.Price.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                purchaseItemTable.AddCell(new PdfPCell(new Phrase(po.Amount.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            //Subtotal
            purchaseItemTable.AddCell(new PdfPCell(new Phrase("", titleFont)) { Colspan = 3, HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.NO_BORDER });
            purchaseItemTable.AddCell(new PdfPCell(new Phrase("Subtotal", titleFont)) { HorizontalAlignment = Element.ALIGN_LEFT });
            purchaseItemTable.AddCell(new PdfPCell(new Phrase(purchase.Subtotal.ToString("#,##0.00"), titleFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            //Discount
            purchaseItemTable.AddCell(new PdfPCell(new Phrase("", font)) { Colspan = 3, HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.NO_BORDER });
            purchaseItemTable.AddCell(new PdfPCell(new Phrase("Discount", titleFont)) { HorizontalAlignment = Element.ALIGN_LEFT });
            purchaseItemTable.AddCell(new PdfPCell(new Phrase(purchase.Discount.ToString("#,##0.00"), titleFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            //Net Amount
            purchaseItemTable.AddCell(new PdfPCell(new Phrase("", font)) { Colspan = 3, HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.NO_BORDER });
            purchaseItemTable.AddCell(new PdfPCell(new Phrase("Net Amount", titleFont)) { HorizontalAlignment = Element.ALIGN_LEFT });
            purchaseItemTable.AddCell(new PdfPCell(new Phrase(purchase.NetAmount.ToString("#,##0.00"), titleFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });


            var tableHeight = CalculatePdfPTableHeight(purchaseItemTable);
            var invoiceTableHeight = CalculatePdfPTableHeight(invoiceInfoTable);
            var headerTableHeight = CalculatePdfPTableHeight(headerTable);

            Document doc = new Document(new iTextSharp.text.Rectangle(0, 0, 210, invoiceTableHeight + headerTableHeight + tableHeight), 16.0f, 16.0f, 0, 0);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //doc.SetPageSize( new iTextSharp.text.Rectangle(0, 0, 100, 150));
            doc.SetMargins(16.0f, 16.0f, 0, 0);
            doc.Add(headerTable);
            doc.Add(invoiceInfoTable);
            doc.Add(purchaseItemTable);
            doc.Close();
        }


        //Supplier Payment Voucher
        public void PrintSupplierPaymentVoucherToPdf(MemoryStream stream, Supplier? supplier, Branch? branch, SupplierPayment supplierPayment, PaymentMethod? paymentMethod)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (supplierPayment == null)
                throw new ArgumentNullException(nameof(supplierPayment));

            var pageSize = PageSize.A5;
            var doc = new Document(pageSize);
            //var doc = new Document(new iTextSharp.text.Rectangle(410f, 288f).Rotate());
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
            defaultFont.Size = 8;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            //headers
            PrintReportHeader(pdfWriter, "Credit Voucher", branch, headerTitleFont, addressFont, reportTitleFont, doc);
            //Line
            //DrawHorizontalLineProtrait(pdfWriter);
            //Data
            PrintSupplierPaymentVoucherReport(stream, supplier, branch, supplierPayment, paymentMethod, reportTitleFont, doc, defaultFont, attributesFont);
            doc.Close();
        }

        protected virtual void PrintSupplierPaymentVoucherReport(Stream stream, Supplier supplier, Branch branch, SupplierPayment supplierPayment, PaymentMethod paymentMethod, Font reportTitleFont, Document doc, Font defaultFont, Font attributesFont)
        {
            var font8Bold = GetFont();
            font8Bold.Size = 8;
            font8Bold.SetStyle(Font.BOLD);

            //Horizontal Line
            //Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.Black, Element.ALIGN_LEFT, 1)));

            var numColumns = 4;
            var supplierPaymentVoucherReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 10,40,25,25 } }
            };

            supplierPaymentVoucherReportTable.SetWidths(widths[numColumns]);

            var cellsupplierPaymentVoucherReportItem = GetPdfCell("Voucher No :     " + supplierPayment.BillNo, defaultFont);
            cellsupplierPaymentVoucherReportItem.Border = Rectangle.NO_BORDER;
            cellsupplierPaymentVoucherReportItem.Colspan = 2;
            cellsupplierPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            supplierPaymentVoucherReportTable.AddCell(cellsupplierPaymentVoucherReportItem);

            //Item
            cellsupplierPaymentVoucherReportItem = GetPdfCell("Date :     " + supplierPayment.PaymentDate.ToString("dd/MM/yyyy"), defaultFont);
            cellsupplierPaymentVoucherReportItem.Border = Rectangle.NO_BORDER;
            cellsupplierPaymentVoucherReportItem.Colspan = 2;
            cellsupplierPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            supplierPaymentVoucherReportTable.AddCell(cellsupplierPaymentVoucherReportItem);

            cellsupplierPaymentVoucherReportItem = GetPdfCell("Cost Center :     " + branch.Name, defaultFont);
            cellsupplierPaymentVoucherReportItem.Border = Rectangle.NO_BORDER;
            cellsupplierPaymentVoucherReportItem.Colspan = 4;
            cellsupplierPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            supplierPaymentVoucherReportTable.AddCell(cellsupplierPaymentVoucherReportItem);

            cellsupplierPaymentVoucherReportItem = GetPdfCell("", defaultFont);
            cellsupplierPaymentVoucherReportItem.Phrase.Add(new Phrase(Environment.NewLine));
            cellsupplierPaymentVoucherReportItem.Colspan = 4;
            cellsupplierPaymentVoucherReportItem.Border = Rectangle.NO_BORDER;
            supplierPaymentVoucherReportTable.AddCell(cellsupplierPaymentVoucherReportItem);

            //SL
            cellsupplierPaymentVoucherReportItem = GetPdfCell("SL", defaultFont);
            cellsupplierPaymentVoucherReportItem.BackgroundColor = BaseColor.LightGray;
            cellsupplierPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierPaymentVoucherReportTable.AddCell(cellsupplierPaymentVoucherReportItem);
            //Item
            cellsupplierPaymentVoucherReportItem = GetPdfCell("Account Name/Description", defaultFont);
            cellsupplierPaymentVoucherReportItem.BackgroundColor = BaseColor.LightGray;
            cellsupplierPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierPaymentVoucherReportTable.AddCell(cellsupplierPaymentVoucherReportItem);

            cellsupplierPaymentVoucherReportItem = GetPdfCell("DR", defaultFont);
            cellsupplierPaymentVoucherReportItem.BackgroundColor = BaseColor.LightGray;
            cellsupplierPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierPaymentVoucherReportTable.AddCell(cellsupplierPaymentVoucherReportItem);

            cellsupplierPaymentVoucherReportItem = GetPdfCell("CR", defaultFont);
            cellsupplierPaymentVoucherReportItem.BackgroundColor = BaseColor.LightGray;
            cellsupplierPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierPaymentVoucherReportTable.AddCell(cellsupplierPaymentVoucherReportItem);

            //SL: 1
            cellsupplierPaymentVoucherReportItem = GetPdfCell("1", defaultFont);
            //cellsupplierPaymentVoucherReportItem.Colspan = 4;
            cellsupplierPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierPaymentVoucherReportTable.AddCell(cellsupplierPaymentVoucherReportItem);

            cellsupplierPaymentVoucherReportItem = GetPdfCell(supplier.Name, defaultFont);
            cellsupplierPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            supplierPaymentVoucherReportTable.AddCell(cellsupplierPaymentVoucherReportItem);

            cellsupplierPaymentVoucherReportItem = GetPdfCell(Math.Round(supplierPayment.PaymentAmount, 2), defaultFont);
            cellsupplierPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            supplierPaymentVoucherReportTable.AddCell(cellsupplierPaymentVoucherReportItem);

            cellsupplierPaymentVoucherReportItem = GetPdfCell("0.00", defaultFont);
            cellsupplierPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            supplierPaymentVoucherReportTable.AddCell(cellsupplierPaymentVoucherReportItem);

            //SL: 2
            cellsupplierPaymentVoucherReportItem = GetPdfCell("2", defaultFont);

            cellsupplierPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            supplierPaymentVoucherReportTable.AddCell(cellsupplierPaymentVoucherReportItem);

            cellsupplierPaymentVoucherReportItem = GetPdfCell(paymentMethod.Name.ToUpper() == "CASH" ? "Cash In Hand" : paymentMethod.Name, defaultFont);
            cellsupplierPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            supplierPaymentVoucherReportTable.AddCell(cellsupplierPaymentVoucherReportItem);

            cellsupplierPaymentVoucherReportItem = GetPdfCell("0.00", defaultFont);
            cellsupplierPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            supplierPaymentVoucherReportTable.AddCell(cellsupplierPaymentVoucherReportItem);

            cellsupplierPaymentVoucherReportItem = GetPdfCell(Math.Round(supplierPayment.PaymentAmount, 2), defaultFont);
            cellsupplierPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            supplierPaymentVoucherReportTable.AddCell(cellsupplierPaymentVoucherReportItem);

            //Total
            cellsupplierPaymentVoucherReportItem = GetPdfCell("Total", defaultFont);
            cellsupplierPaymentVoucherReportItem.Colspan = 2;
            cellsupplierPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            supplierPaymentVoucherReportTable.AddCell(cellsupplierPaymentVoucherReportItem);

            cellsupplierPaymentVoucherReportItem = GetPdfCell(Math.Round(supplierPayment.PaymentAmount, 2), defaultFont);
            cellsupplierPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            supplierPaymentVoucherReportTable.AddCell(cellsupplierPaymentVoucherReportItem);

            cellsupplierPaymentVoucherReportItem = GetPdfCell(Math.Round(supplierPayment.PaymentAmount, 2), defaultFont);
            cellsupplierPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            supplierPaymentVoucherReportTable.AddCell(cellsupplierPaymentVoucherReportItem);

            var numColumn = 2;
            var supplierPaymentVoucherReportTableInfo = new PdfPTable(numColumn)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var width = new Dictionary<int, int[]>
            {
                { numColumn, new[] { 30,70 } }
            };

            supplierPaymentVoucherReportTableInfo.SetWidths(width[numColumn]);

            var cellsupplierPaymentVoucherReportItemInfo = GetPdfCell("", defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.Phrase.Add(new Phrase(Environment.NewLine));
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            supplierPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell("", defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.Phrase.Add(new Phrase(Environment.NewLine));
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            supplierPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell("Amount In Words :", defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            supplierPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell(NumberToWords.ConvertAmount(supplierPayment.PaymentAmount), font8Bold);
            cellsupplierPaymentVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            supplierPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell("Narration :", defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            supplierPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell(paymentMethod.Name + " Payment To " + supplier.Name + " Tk- " + supplierPayment.PaymentAmount, defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            supplierPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell("Payment Mode :", defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            supplierPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell(paymentMethod.Name + " - - " + supplierPayment.PaymentDate.ToString("dd/MM/yyyy"), defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            supplierPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell("", defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.Phrase.Add(new Phrase(Environment.NewLine));
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            supplierPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell("", defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.Phrase.Add(new Phrase(Environment.NewLine));
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            supplierPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell("Checked By :", defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            supplierPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell("Approved By :", defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_RIGHT;
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            supplierPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            doc.Add(supplierPaymentVoucherReportTable);
            doc.Add(supplierPaymentVoucherReportTableInfo);
        }


        //Customer Payment Voucher
        public void PrintCustomerPaymentVoucherToPdf(MemoryStream stream, Customer? customer, Branch? branch, CustomerPayment customerPayment, PaymentMethod? paymentMethod)
        {
            //throw new NotImplementedException();
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (customerPayment == null)
                throw new ArgumentNullException(nameof(customerPayment));

            var pageSize = PageSize.A5;
            var doc = new Document(pageSize);
            //var doc = new Document(new iTextSharp.text.Rectangle(410f, 288f).Rotate());
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
            defaultFont.Size = 8;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintReportHeader(pdfWriter, "Debit Voucher", branch, headerTitleFont, addressFont, reportTitleFont, doc);
            //Line
            //DrawHorizontalLineProtrait(pdfWriter);
            //Data
            PrintCustomerPaymentVoucherReport(stream, customer, branch, customerPayment, paymentMethod, reportTitleFont, doc, defaultFont, attributesFont);

            doc.Close();
        }

        protected virtual void PrintCustomerPaymentVoucherReport(Stream stream, Customer customer, Branch branch, CustomerPayment customerPayment, PaymentMethod paymentMethod, Font reportTitleFont, Document doc, Font defaultFont, Font attributesFont)
        {
            var font8Bold = GetFont();
            font8Bold.Size = 8;
            font8Bold.SetStyle(Font.BOLD);

            //Horizontal Line
            //Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.Black, Element.ALIGN_LEFT, 1)));

            var numColumns = 4;
            var customerPaymentVoucherReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 10,40,25,25 } }
            };

            customerPaymentVoucherReportTable.SetWidths(widths[numColumns]);

            var cellcustomerPaymentVoucherReportItem = GetPdfCell("Voucher No :     " + customerPayment.BillNo, defaultFont);
            cellcustomerPaymentVoucherReportItem.Border = Rectangle.NO_BORDER;
            cellcustomerPaymentVoucherReportItem.Colspan = 2;
            cellcustomerPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            customerPaymentVoucherReportTable.AddCell(cellcustomerPaymentVoucherReportItem);

            //Item
            cellcustomerPaymentVoucherReportItem = GetPdfCell("Date :     " + customerPayment.PaymentDate.ToString("dd/MM/yyyy"), defaultFont);
            cellcustomerPaymentVoucherReportItem.Border = Rectangle.NO_BORDER;
            cellcustomerPaymentVoucherReportItem.Colspan = 2;
            cellcustomerPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            customerPaymentVoucherReportTable.AddCell(cellcustomerPaymentVoucherReportItem);

            cellcustomerPaymentVoucherReportItem = GetPdfCell("Cost Center :     " + branch.Name, defaultFont);
            cellcustomerPaymentVoucherReportItem.Border = Rectangle.NO_BORDER;
            cellcustomerPaymentVoucherReportItem.Colspan = 4;
            cellcustomerPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            customerPaymentVoucherReportTable.AddCell(cellcustomerPaymentVoucherReportItem);

            cellcustomerPaymentVoucherReportItem = GetPdfCell("", defaultFont);
            cellcustomerPaymentVoucherReportItem.Phrase.Add(new Phrase(Environment.NewLine));
            cellcustomerPaymentVoucherReportItem.Colspan = 4;
            cellcustomerPaymentVoucherReportItem.Border = Rectangle.NO_BORDER;
            customerPaymentVoucherReportTable.AddCell(cellcustomerPaymentVoucherReportItem);

            //SL
            cellcustomerPaymentVoucherReportItem = GetPdfCell("SL", defaultFont);
            cellcustomerPaymentVoucherReportItem.BackgroundColor = BaseColor.LightGray;
            cellcustomerPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            customerPaymentVoucherReportTable.AddCell(cellcustomerPaymentVoucherReportItem);
            //Item
            cellcustomerPaymentVoucherReportItem = GetPdfCell("Account Name/Description", defaultFont);
            cellcustomerPaymentVoucherReportItem.BackgroundColor = BaseColor.LightGray;
            cellcustomerPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            customerPaymentVoucherReportTable.AddCell(cellcustomerPaymentVoucherReportItem);

            cellcustomerPaymentVoucherReportItem = GetPdfCell("DR", defaultFont);
            cellcustomerPaymentVoucherReportItem.BackgroundColor = BaseColor.LightGray;
            cellcustomerPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            customerPaymentVoucherReportTable.AddCell(cellcustomerPaymentVoucherReportItem);

            cellcustomerPaymentVoucherReportItem = GetPdfCell("CR", defaultFont);
            cellcustomerPaymentVoucherReportItem.BackgroundColor = BaseColor.LightGray;
            cellcustomerPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            customerPaymentVoucherReportTable.AddCell(cellcustomerPaymentVoucherReportItem);

            //SL: 1
            cellcustomerPaymentVoucherReportItem = GetPdfCell("1", defaultFont);
            //cellsupplierPaymentVoucherReportItem.Colspan = 4;
            cellcustomerPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            customerPaymentVoucherReportTable.AddCell(cellcustomerPaymentVoucherReportItem);

            cellcustomerPaymentVoucherReportItem = GetPdfCell(customer.Name, defaultFont);
            cellcustomerPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            customerPaymentVoucherReportTable.AddCell(cellcustomerPaymentVoucherReportItem);

            cellcustomerPaymentVoucherReportItem = GetPdfCell("0.00", defaultFont);
            cellcustomerPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            customerPaymentVoucherReportTable.AddCell(cellcustomerPaymentVoucherReportItem);

            cellcustomerPaymentVoucherReportItem = GetPdfCell(customerPayment.PaymentAmount.ToString("#,##0.00"), defaultFont);
            cellcustomerPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            customerPaymentVoucherReportTable.AddCell(cellcustomerPaymentVoucherReportItem);

            //SL: 2
            cellcustomerPaymentVoucherReportItem = GetPdfCell("2", defaultFont);
            cellcustomerPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            customerPaymentVoucherReportTable.AddCell(cellcustomerPaymentVoucherReportItem);

            cellcustomerPaymentVoucherReportItem = GetPdfCell(paymentMethod.Name.ToUpper() == "CASH" ? "Cash In Hand" : paymentMethod.Name, defaultFont);
            cellcustomerPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            customerPaymentVoucherReportTable.AddCell(cellcustomerPaymentVoucherReportItem);

            cellcustomerPaymentVoucherReportItem = GetPdfCell(customerPayment.PaymentAmount.ToString("#,##0.00"), defaultFont);
            cellcustomerPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            customerPaymentVoucherReportTable.AddCell(cellcustomerPaymentVoucherReportItem);

            cellcustomerPaymentVoucherReportItem = GetPdfCell("0.00", defaultFont);
            cellcustomerPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            customerPaymentVoucherReportTable.AddCell(cellcustomerPaymentVoucherReportItem);

            //Total
            cellcustomerPaymentVoucherReportItem = GetPdfCell("Total", defaultFont);
            cellcustomerPaymentVoucherReportItem.Colspan = 2;
            cellcustomerPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            customerPaymentVoucherReportTable.AddCell(cellcustomerPaymentVoucherReportItem);

            cellcustomerPaymentVoucherReportItem = GetPdfCell(customerPayment.PaymentAmount.ToString("#,##0.00"), defaultFont);
            cellcustomerPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            customerPaymentVoucherReportTable.AddCell(cellcustomerPaymentVoucherReportItem);

            cellcustomerPaymentVoucherReportItem = GetPdfCell(customerPayment.PaymentAmount.ToString("#,##0.00"), defaultFont);
            cellcustomerPaymentVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            customerPaymentVoucherReportTable.AddCell(cellcustomerPaymentVoucherReportItem);

            var numColumn = 2;
            var customerPaymentVoucherReportTableInfo = new PdfPTable(numColumn)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var width = new Dictionary<int, int[]>
            {
                { numColumn, new[] { 30,70 } }
            };

            customerPaymentVoucherReportTableInfo.SetWidths(width[numColumn]);

            var cellsupplierPaymentVoucherReportItemInfo = GetPdfCell("", defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.Phrase.Add(new Phrase(Environment.NewLine));
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            customerPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell("", defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.Phrase.Add(new Phrase(Environment.NewLine));
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            customerPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell("Amount In Words :", defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            customerPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell(NumberToWords.ConvertAmount(customerPayment.PaymentAmount), font8Bold);
            cellsupplierPaymentVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            customerPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell("Narration :", defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            customerPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell(paymentMethod.Name + " Receive From " + customer.Name + " Tk- " + customerPayment.PaymentAmount, defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            customerPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell("Payment Mode :", defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            customerPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell(paymentMethod.Name + " - - " + customerPayment.PaymentDate.ToString("dd/MM/yyyy"), defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            customerPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell("", defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.Phrase.Add(new Phrase(Environment.NewLine));
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            customerPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell("", defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.Phrase.Add(new Phrase(Environment.NewLine));
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            customerPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell("Checked By :", defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            customerPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            cellsupplierPaymentVoucherReportItemInfo = GetPdfCell("Approved By :", defaultFont);
            cellsupplierPaymentVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_RIGHT;
            cellsupplierPaymentVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            customerPaymentVoucherReportTableInfo.AddCell(cellsupplierPaymentVoucherReportItemInfo);

            doc.Add(customerPaymentVoucherReportTable);
            doc.Add(customerPaymentVoucherReportTableInfo);
        }


        //Expense Voucher
        public void PrintExpenseVoucherToPdf(MemoryStream stream, ExpenseType? expenseType, Branch? branch, Expense expense, PaymentMethod? paymentMethod)
        {
            //throw new NotImplementedException();
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (expense == null)
                throw new ArgumentNullException(nameof(expense));

            var pageSize = PageSize.A5;
            var doc = new Document(pageSize);
            //var doc = new Document(new iTextSharp.text.Rectangle(410f, 288f).Rotate());
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
            defaultFont.Size = 8;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintReportHeader(pdfWriter, "Debit Voucher", branch, headerTitleFont, addressFont, reportTitleFont, doc);
            //Line
            //DrawHorizontalLineProtrait(pdfWriter);
            //Data
            PrintExpenseVoucherReport(stream, expenseType, branch, expense, paymentMethod, reportTitleFont, doc, defaultFont, attributesFont);
            doc.Close();
        }

        protected virtual void PrintExpenseVoucherReport(Stream stream, ExpenseType expenseType, Branch branch, Expense expense, PaymentMethod paymentMethod, Font reportTitleFont, Document doc, Font defaultFont, Font attributesFont)
        {

            var font8Bold = GetFont();
            font8Bold.Size = 8;
            font8Bold.SetStyle(Font.BOLD);

            //Horizontal Line
            //Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.Black, Element.ALIGN_LEFT, 1)));

            var numColumns = 4;
            var expenseVoucherReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 10,40,25,25 } }
            };

            expenseVoucherReportTable.SetWidths(widths[numColumns]);

            var cellExpenseVoucherReportItem = GetPdfCell("Voucher No :     " + expense.ExpenseNo, defaultFont);
            cellExpenseVoucherReportItem.Border = Rectangle.NO_BORDER;
            cellExpenseVoucherReportItem.Colspan = 2;
            cellExpenseVoucherReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            expenseVoucherReportTable.AddCell(cellExpenseVoucherReportItem);

            //Item
            cellExpenseVoucherReportItem = GetPdfCell("Date :     " + expense.EntryDate.ToString("dd/MM/yyyy"), defaultFont);
            cellExpenseVoucherReportItem.Border = Rectangle.NO_BORDER;
            cellExpenseVoucherReportItem.Colspan = 2;
            cellExpenseVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            expenseVoucherReportTable.AddCell(cellExpenseVoucherReportItem);

            cellExpenseVoucherReportItem = GetPdfCell("Cost Center :     " + branch.Name, defaultFont);
            cellExpenseVoucherReportItem.Border = Rectangle.NO_BORDER;
            cellExpenseVoucherReportItem.Colspan = 4;
            cellExpenseVoucherReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            expenseVoucherReportTable.AddCell(cellExpenseVoucherReportItem);

            cellExpenseVoucherReportItem = GetPdfCell("", defaultFont);
            cellExpenseVoucherReportItem.Phrase.Add(new Phrase(Environment.NewLine));
            cellExpenseVoucherReportItem.Colspan = 4;
            cellExpenseVoucherReportItem.Border = Rectangle.NO_BORDER;
            expenseVoucherReportTable.AddCell(cellExpenseVoucherReportItem);

            //SL
            cellExpenseVoucherReportItem = GetPdfCell("SL", defaultFont);
            cellExpenseVoucherReportItem.BackgroundColor = BaseColor.LightGray;
            cellExpenseVoucherReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            expenseVoucherReportTable.AddCell(cellExpenseVoucherReportItem);

            //Item
            cellExpenseVoucherReportItem = GetPdfCell("Account Name/Description", defaultFont);
            cellExpenseVoucherReportItem.BackgroundColor = BaseColor.LightGray;
            cellExpenseVoucherReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            expenseVoucherReportTable.AddCell(cellExpenseVoucherReportItem);

            cellExpenseVoucherReportItem = GetPdfCell("DR", defaultFont);
            cellExpenseVoucherReportItem.BackgroundColor = BaseColor.LightGray;
            cellExpenseVoucherReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            expenseVoucherReportTable.AddCell(cellExpenseVoucherReportItem);

            cellExpenseVoucherReportItem = GetPdfCell("CR", defaultFont);
            cellExpenseVoucherReportItem.BackgroundColor = BaseColor.LightGray;
            cellExpenseVoucherReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            expenseVoucherReportTable.AddCell(cellExpenseVoucherReportItem);

            //SL: 1
            cellExpenseVoucherReportItem = GetPdfCell("1", defaultFont);
            //cellsupplierPaymentVoucherReportItem.Colspan = 4;
            cellExpenseVoucherReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            expenseVoucherReportTable.AddCell(cellExpenseVoucherReportItem);

            cellExpenseVoucherReportItem = GetPdfCell(expenseType.Name, defaultFont);
            cellExpenseVoucherReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            expenseVoucherReportTable.AddCell(cellExpenseVoucherReportItem);

            cellExpenseVoucherReportItem = GetPdfCell(expense.Amount.ToString("#,##0.00"), defaultFont);
            cellExpenseVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            expenseVoucherReportTable.AddCell(cellExpenseVoucherReportItem);

            cellExpenseVoucherReportItem = GetPdfCell("0.00", defaultFont);
            cellExpenseVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            expenseVoucherReportTable.AddCell(cellExpenseVoucherReportItem);

            //SL: 2
            cellExpenseVoucherReportItem = GetPdfCell("2", defaultFont);
            cellExpenseVoucherReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            expenseVoucherReportTable.AddCell(cellExpenseVoucherReportItem);

            cellExpenseVoucherReportItem = GetPdfCell(paymentMethod.Name.ToUpper() == "CASH" ? "Cash In Hand" : paymentMethod.Name, defaultFont);
            cellExpenseVoucherReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            expenseVoucherReportTable.AddCell(cellExpenseVoucherReportItem);

            cellExpenseVoucherReportItem = GetPdfCell("0.00", defaultFont);
            cellExpenseVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            expenseVoucherReportTable.AddCell(cellExpenseVoucherReportItem);

            cellExpenseVoucherReportItem = GetPdfCell(expense.Amount.ToString("#,##0.00"), defaultFont);
            cellExpenseVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            expenseVoucherReportTable.AddCell(cellExpenseVoucherReportItem);

            //Total
            cellExpenseVoucherReportItem = GetPdfCell("Total", defaultFont);
            cellExpenseVoucherReportItem.Colspan = 2;
            cellExpenseVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            expenseVoucherReportTable.AddCell(cellExpenseVoucherReportItem);

            cellExpenseVoucherReportItem = GetPdfCell(expense.Amount.ToString("#,##0.00"), defaultFont);
            cellExpenseVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            expenseVoucherReportTable.AddCell(cellExpenseVoucherReportItem);

            cellExpenseVoucherReportItem = GetPdfCell(expense.Amount.ToString("#,##0.00"), defaultFont);
            cellExpenseVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            expenseVoucherReportTable.AddCell(cellExpenseVoucherReportItem);

            var numColumn = 2;
            var expenseVoucherReportTableInfo = new PdfPTable(numColumn)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var width = new Dictionary<int, int[]>
            {
                { numColumn, new[] { 30,70 } }
            };

            expenseVoucherReportTableInfo.SetWidths(width[numColumn]);

            var cellExpenseVoucherReportItemInfo = GetPdfCell("", defaultFont);
            cellExpenseVoucherReportItemInfo.Phrase.Add(new Phrase(Environment.NewLine));
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            expenseVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell("", defaultFont);
            cellExpenseVoucherReportItemInfo.Phrase.Add(new Phrase(Environment.NewLine));
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            expenseVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell("Amount In Words :", defaultFont);
            cellExpenseVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            expenseVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell(NumberToWords.ConvertAmount(expense.Amount), font8Bold);
            cellExpenseVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            expenseVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell("Narration :", defaultFont);
            cellExpenseVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            expenseVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell(paymentMethod.Name + " Paid For " + expenseType.Name + " Tk- " + expense.Amount + "  " + expense.Remark, defaultFont);
            cellExpenseVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            expenseVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell("Payment Mode :", defaultFont);
            cellExpenseVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            expenseVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell(paymentMethod.Name + " - - " + expense.EntryDate.ToString("dd/MM/yyyy"), defaultFont);
            cellExpenseVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            expenseVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell("", defaultFont);
            cellExpenseVoucherReportItemInfo.Phrase.Add(new Phrase(Environment.NewLine));
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            expenseVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell("", defaultFont);
            cellExpenseVoucherReportItemInfo.Phrase.Add(new Phrase(Environment.NewLine));
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            expenseVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell("Checked By :", defaultFont);
            cellExpenseVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            expenseVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell("Approved By :", defaultFont);
            cellExpenseVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_RIGHT;
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            expenseVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            doc.Add(expenseVoucherReportTable);
            doc.Add(expenseVoucherReportTableInfo);
        }


        //Pay Receive Voucher
        public void PrintPayReceiveVoucherToPdf(MemoryStream stream, Branch? branch, PayReceive payReceive, AccountHead? accountHead, PaymentMethod? paymentMethod)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (payReceive == null)
                throw new ArgumentNullException(nameof(payReceive));

            var pageSize = PageSize.A5;



            var doc = new Document(pageSize);
            //var doc = new Document(new iTextSharp.text.Rectangle(410f, 288f).Rotate());

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
            defaultFont.Size = 8;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintReportHeader(pdfWriter, payReceive.VoucherType == 1 ? "Credit Voucher" : "Debit Voucher", branch, headerTitleFont, addressFont, reportTitleFont, doc);
            //Line
            //DrawHorizontalLineProtrait(pdfWriter);
            //Data
            PrintPayReceiveVoucherReport(stream, branch, accountHead, payReceive, paymentMethod, reportTitleFont, doc, defaultFont, attributesFont);
            doc.Close();
        }

        protected virtual void PrintPayReceiveVoucherReport(MemoryStream stream, Branch branch, AccountHead accountHead, PayReceive payReceive, PaymentMethod paymentMethod, Font reportTitleFont, Document doc, Font defaultFont, Font attributesFont)
        {
            var numColumns = 4;
            var payReceiveVoucherReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 10,50,20,20 } }
            };

            payReceiveVoucherReportTable.SetWidths(widths[numColumns]);

            var cellPayReceiveVoucherReportItem = GetPdfCell("Voucher No :     " + payReceive.Vnumber, defaultFont);
            cellPayReceiveVoucherReportItem.Border = Rectangle.NO_BORDER;
            cellPayReceiveVoucherReportItem.Colspan = 2;
            cellPayReceiveVoucherReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            payReceiveVoucherReportTable.AddCell(cellPayReceiveVoucherReportItem);

            //Item
            cellPayReceiveVoucherReportItem = GetPdfCell("Date :     " + payReceive.VoucherDate.ToString("dd/MM/yyyy"), defaultFont);
            cellPayReceiveVoucherReportItem.Border = Rectangle.NO_BORDER;
            cellPayReceiveVoucherReportItem.Colspan = 2;
            cellPayReceiveVoucherReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            payReceiveVoucherReportTable.AddCell(cellPayReceiveVoucherReportItem);

            cellPayReceiveVoucherReportItem = GetPdfCell("Cost Center :     " + branch.Name, defaultFont);
            cellPayReceiveVoucherReportItem.Border = Rectangle.NO_BORDER;
            cellPayReceiveVoucherReportItem.Colspan = 4;
            cellPayReceiveVoucherReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            payReceiveVoucherReportTable.AddCell(cellPayReceiveVoucherReportItem);

            cellPayReceiveVoucherReportItem = GetPdfCell("", defaultFont);
            cellPayReceiveVoucherReportItem.Phrase.Add(new Phrase(Environment.NewLine));
            cellPayReceiveVoucherReportItem.Colspan = 4;
            cellPayReceiveVoucherReportItem.Border = Rectangle.NO_BORDER;
            payReceiveVoucherReportTable.AddCell(cellPayReceiveVoucherReportItem);

            payReceiveVoucherReportTable.AddCell(new PdfPCell(new Phrase("SL", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            payReceiveVoucherReportTable.AddCell(new PdfPCell(new Phrase("Account Name/Description", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            payReceiveVoucherReportTable.AddCell(new PdfPCell(new Phrase("DR", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            payReceiveVoucherReportTable.AddCell(new PdfPCell(new Phrase("CR", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            //SL: 1
            payReceiveVoucherReportTable.AddCell(new PdfPCell(new Phrase("1", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
            payReceiveVoucherReportTable.AddCell(new PdfPCell(new Phrase(accountHead.Name, defaultFont)) { HorizontalAlignment = Element.ALIGN_LEFT });
            payReceiveVoucherReportTable.AddCell(new PdfPCell(new Phrase(payReceive.VoucherType == 1 ? payReceive.Amount.ToString("#,##0.00") : "0.00", defaultFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            payReceiveVoucherReportTable.AddCell(new PdfPCell(new Phrase(payReceive.VoucherType == 1 ? "0.00" : payReceive.Amount.ToString("#,##0.00"), defaultFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            //SL: 2
            payReceiveVoucherReportTable.AddCell(new PdfPCell(new Phrase("2", defaultFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
            payReceiveVoucherReportTable.AddCell(new PdfPCell(new Phrase(paymentMethod.Name.ToUpper() == "CASH" ? "Cash In Hand" : paymentMethod.Name, defaultFont)) { HorizontalAlignment = Element.ALIGN_LEFT, });
            payReceiveVoucherReportTable.AddCell(new PdfPCell(new Phrase(payReceive.VoucherType == 1 ? "0.00" : payReceive.Amount.ToString("#,##0.00"), defaultFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, });
            payReceiveVoucherReportTable.AddCell(new PdfPCell(new Phrase(payReceive.VoucherType == 1 ? payReceive.Amount.ToString("#,##0.00") : "0.00", defaultFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, });

            //Total
            payReceiveVoucherReportTable.AddCell(new PdfPCell(new Phrase("Total", defaultFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_RIGHT, });
            payReceiveVoucherReportTable.AddCell(new PdfPCell(new Phrase(payReceive.Amount.ToString("#,##0.00"), defaultFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, });
            payReceiveVoucherReportTable.AddCell(new PdfPCell(new Phrase(payReceive.Amount.ToString("#,##0.00"), defaultFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, });

            var numColumn = 2;
            var payReceiveVoucherReportTableInfo = new PdfPTable(numColumn)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var width = new Dictionary<int, int[]>
            {
                { numColumn, new[] { 30,70 } }
            };

            payReceiveVoucherReportTableInfo.SetWidths(width[numColumn]);

            var cellExpenseVoucherReportItemInfo = GetPdfCell("", defaultFont);
            cellExpenseVoucherReportItemInfo.Phrase.Add(new Phrase(Environment.NewLine));
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            payReceiveVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell("", defaultFont);
            cellExpenseVoucherReportItemInfo.Phrase.Add(new Phrase(Environment.NewLine));
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            payReceiveVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell("Amount In Words :", defaultFont);
            cellExpenseVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            payReceiveVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell(NumberToWords.ConvertAmount(payReceive.Amount), reportTitleFont);
            cellExpenseVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            payReceiveVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell("Narration :", defaultFont);
            cellExpenseVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            payReceiveVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell(paymentMethod.Name + (payReceive.VoucherType == 1 ? " Paid To " : " Receive From ") + accountHead.Name + " Tk- " + payReceive.Amount + "  " + payReceive.Description, defaultFont);
            cellExpenseVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            payReceiveVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell("Payment Mode :", defaultFont);
            cellExpenseVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            payReceiveVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell(paymentMethod.Name + " - - " + payReceive.VoucherDate.ToString("dd/MM/yyyy"), defaultFont);
            cellExpenseVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            payReceiveVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell("", defaultFont);
            cellExpenseVoucherReportItemInfo.Phrase.Add(new Phrase(Environment.NewLine));
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            payReceiveVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell("", defaultFont);
            cellExpenseVoucherReportItemInfo.Phrase.Add(new Phrase(Environment.NewLine));
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            payReceiveVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell("Checked By :", defaultFont);
            cellExpenseVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            payReceiveVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            cellExpenseVoucherReportItemInfo = GetPdfCell("Approved By :", defaultFont);
            cellExpenseVoucherReportItemInfo.HorizontalAlignment = Element.ALIGN_RIGHT;
            cellExpenseVoucherReportItemInfo.Border = Rectangle.NO_BORDER;
            payReceiveVoucherReportTableInfo.AddCell(cellExpenseVoucherReportItemInfo);

            doc.Add(payReceiveVoucherReportTable);
            doc.Add(payReceiveVoucherReportTableInfo);
        }


        //Purchase Item
        public virtual void PrintPurchaseItemReportToPdf(MemoryStream stream, Branch? branch, List<PurchaseItemViewModel> purchaseItemViewModels, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (purchaseItemViewModels == null)
                throw new ArgumentNullException(nameof(purchaseItemViewModels));

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
            defaultFont.Size = 8;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Data
            PrintPurchaseItemReport(reportTitleFont, doc, purchaseItemViewModels, defaultFont, attributesFont);

            var purchaseItemLineNumber = 20;
            var purchaseItemLineNumberCount = purchaseItemViewModels.Count;
            purchaseItemLineNumber++;

            if (purchaseItemLineNumber <= purchaseItemLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }

        protected virtual void PrintPurchaseItemReport(Font titleFont, Document doc, List<PurchaseItemViewModel> purchaseItemViewModels, Font font, Font attributesFont, int supplierId = 0)
        {
            var numColumns = 9;
            var purchaseItemReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 6,10,24,10,12,9,7,12,10 } }
            };

            purchaseItemReportTable.SetWidths(widths[numColumns]);

            var cellPurchaseItemReportItem = GetPdfCell("SL", font);
            cellPurchaseItemReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseItemReportTable.AddCell(cellPurchaseItemReportItem);

            cellPurchaseItemReportItem = GetPdfCell("Purchase No", font);
            cellPurchaseItemReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseItemReportTable.AddCell(cellPurchaseItemReportItem);

            cellPurchaseItemReportItem = GetPdfCell("Product Name", font);
            cellPurchaseItemReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseItemReportTable.AddCell(cellPurchaseItemReportItem);

            cellPurchaseItemReportItem = GetPdfCell("Expire Date", font);
            cellPurchaseItemReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseItemReportTable.AddCell(cellPurchaseItemReportItem);

            cellPurchaseItemReportItem = GetPdfCell("Purchase Price", font);
            cellPurchaseItemReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseItemReportTable.AddCell(cellPurchaseItemReportItem);

            cellPurchaseItemReportItem = GetPdfCell("Purchase Qty", font);
            cellPurchaseItemReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseItemReportTable.AddCell(cellPurchaseItemReportItem);

            cellPurchaseItemReportItem = GetPdfCell("Bonus Qty", font);
            cellPurchaseItemReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseItemReportTable.AddCell(cellPurchaseItemReportItem);

            cellPurchaseItemReportItem = GetPdfCell("Amount", font);
            cellPurchaseItemReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseItemReportTable.AddCell(cellPurchaseItemReportItem);

            cellPurchaseItemReportItem = GetPdfCell("Created On", font);
            cellPurchaseItemReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseItemReportTable.AddCell(cellPurchaseItemReportItem);

            purchaseItemReportTable.HeaderRows = 1;
            var r = 0;
            foreach (var pil in purchaseItemViewModels)
            {
                //pil = purchase item list
                r++;

                //Sl
                cellPurchaseItemReportItem = GetPdfCell(r, font);
                cellPurchaseItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                purchaseItemReportTable.AddCell(cellPurchaseItemReportItem);
                //ItemName
                cellPurchaseItemReportItem = GetPdfCell(pil.PurchaseNo, font);
                cellPurchaseItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                purchaseItemReportTable.AddCell(cellPurchaseItemReportItem);

                cellPurchaseItemReportItem = GetPdfCell(pil.Option, font);
                cellPurchaseItemReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseItemReportTable.AddCell(cellPurchaseItemReportItem);

                cellPurchaseItemReportItem = GetPdfCell(pil.ExpireDate?.ToString("dd/MM/yyyy"), font);
                cellPurchaseItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                purchaseItemReportTable.AddCell(cellPurchaseItemReportItem);

                cellPurchaseItemReportItem = GetPdfCell(pil.PurchasePrice.ToString("#,##0.00"), font);
                cellPurchaseItemReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseItemReportTable.AddCell(cellPurchaseItemReportItem);

                cellPurchaseItemReportItem = GetPdfCell(pil.PurchaseQuantity.ToString("#,##0"), font);
                cellPurchaseItemReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseItemReportTable.AddCell(cellPurchaseItemReportItem);

                cellPurchaseItemReportItem = GetPdfCell(pil.BonusQuantity.ToString("#,##0"), font);
                cellPurchaseItemReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseItemReportTable.AddCell(cellPurchaseItemReportItem);

                cellPurchaseItemReportItem = GetPdfCell(pil.Amount.ToString("#,##0.00"), font);
                cellPurchaseItemReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseItemReportTable.AddCell(cellPurchaseItemReportItem);

                cellPurchaseItemReportItem = GetPdfCell(pil.CreatedOn.ToString("dd/MM/yyyy"), font);
                cellPurchaseItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                purchaseItemReportTable.AddCell(cellPurchaseItemReportItem);

            }

            doc.Add(purchaseItemReportTable);
        }


        //Sale Item
        public virtual void PrintSaleItemReportToPdf(MemoryStream stream, Branch? branch, List<SaleItemViewModel> saleItemViewModels, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (saleItemViewModels == null)
                throw new ArgumentNullException(nameof(saleItemViewModels));

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
            defaultFont.Size = 8;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Data
            PrintSaleItemReport(reportTitleFont, doc, saleItemViewModels, defaultFont, attributesFont);

            var saleItemLineNumber = 20;
            var saleItemLineNumberCount = saleItemViewModels.Count;
            saleItemLineNumber++;

            if (saleItemLineNumber <= saleItemLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }

        protected virtual void PrintSaleItemReport(Font titleFont, Document doc, List<SaleItemViewModel> saleItemViewModels, Font font, Font attributesFont, int supplierId = 0)
        {
            var numColumns = 8;
            var saleItemReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 8,10,28,10,10,12,12,10 } }
            };

            saleItemReportTable.SetWidths(widths[numColumns]);

            var cellSaleItemReportItem = GetPdfCell("SL", font);
            cellSaleItemReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleItemReportTable.AddCell(cellSaleItemReportItem);

            cellSaleItemReportItem = GetPdfCell("Sale No", font);
            cellSaleItemReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleItemReportTable.AddCell(cellSaleItemReportItem);

            cellSaleItemReportItem = GetPdfCell("Product Name", font);
            cellSaleItemReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleItemReportTable.AddCell(cellSaleItemReportItem);

            cellSaleItemReportItem = GetPdfCell("Price", font);
            cellSaleItemReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleItemReportTable.AddCell(cellSaleItemReportItem);

            cellSaleItemReportItem = GetPdfCell("Quantity", font);
            cellSaleItemReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleItemReportTable.AddCell(cellSaleItemReportItem);

            cellSaleItemReportItem = GetPdfCell("Amount", font);
            cellSaleItemReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleItemReportTable.AddCell(cellSaleItemReportItem);

            cellSaleItemReportItem = GetPdfCell("COGS", font);
            cellSaleItemReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleItemReportTable.AddCell(cellSaleItemReportItem);

            cellSaleItemReportItem = GetPdfCell("Created On", font);
            cellSaleItemReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleItemReportTable.AddCell(cellSaleItemReportItem);

            saleItemReportTable.HeaderRows = 1;

            var r = 0;
            foreach (var sil in saleItemViewModels)
            {
                r++;

                //Sl
                cellSaleItemReportItem = GetPdfCell(r, font);
                cellSaleItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                saleItemReportTable.AddCell(cellSaleItemReportItem);
                //SaleNo
                cellSaleItemReportItem = GetPdfCell(sil.SaleNo, font);
                cellSaleItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                saleItemReportTable.AddCell(cellSaleItemReportItem);

                cellSaleItemReportItem = GetPdfCell(sil.Option, font);
                cellSaleItemReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                saleItemReportTable.AddCell(cellSaleItemReportItem);

                cellSaleItemReportItem = GetPdfCell(sil.Price.ToString("#,##0.00"), font);
                cellSaleItemReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleItemReportTable.AddCell(cellSaleItemReportItem);

                cellSaleItemReportItem = GetPdfCell(sil.Quantity.ToString("#,##0"), font);
                cellSaleItemReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleItemReportTable.AddCell(cellSaleItemReportItem);

                cellSaleItemReportItem = GetPdfCell(sil.Amount.ToString("#,##0.00"), font);
                cellSaleItemReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleItemReportTable.AddCell(cellSaleItemReportItem);

                cellSaleItemReportItem = GetPdfCell((sil.ItemCogs * sil.Quantity).ToString("#,##0.00"), font);
                cellSaleItemReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleItemReportTable.AddCell(cellSaleItemReportItem);

                cellSaleItemReportItem = GetPdfCell(sil.CreatedOn.ToString("dd/MM/yyyy"), font);
                cellSaleItemReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                saleItemReportTable.AddCell(cellSaleItemReportItem);

            }
            doc.Add(saleItemReportTable);
        }


        //Transfer Detail
        public void PrintTransferDetailReportToPdf(MemoryStream stream, Branch? sourceBranch, Branch? destinationBranch, List<TransferDetailViewModel> transferDetailViewModels, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (transferDetailViewModels == null)
                throw new ArgumentNullException(nameof(transferDetailViewModels));

            var pageSize = PageSize.A4;
            var doc = new Document(pageSize);
            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //headerTitleFont
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
            PrintTransferDetailReportHeader(pdfWriter, reportName, sourceBranch, destinationBranch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Data
            PrintTransferDetailReport(reportTitleFont, doc, transferDetailViewModels, defaultFont, attributesFont);

            var transferDetailReportLineNumber = 20;
            var transferDetailReportLineNumberCount = transferDetailViewModels.Count;
            transferDetailReportLineNumber++;

            if (transferDetailReportLineNumber <= transferDetailReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }

        protected virtual void PrintTransferDetailReportHeader(PdfWriter pdfWriter, string reportHeader, Branch sourceBranch, Branch destinationBranch, Font headerTitleFont, Font addressFont, Font reportTitleFont, Document doc, DateTime? fromDate, DateTime? toDatde)
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

            //logo
            var logocell = new PdfPCell { Border = Rectangle.NO_BORDER };
            logocell.AddElement(logo);
            logocell.Colspan = 2;
            headerTable.AddCell(logocell);

            //Company Name
            var cellCompanyName = GetPdfCell(company.Name, headerTitleFont);
            cellCompanyName.HorizontalAlignment = Element.ALIGN_CENTER;
            cellCompanyName.Phrase.Add(new Phrase(Environment.NewLine));
            cellCompanyName.Border = Rectangle.NO_BORDER;
            cellCompanyName.Colspan = 2;
            headerTable.AddCell(cellCompanyName);

            //Report Name
            var cellReportName = GetPdfCell(reportHeader, headerTitleFont);
            cellReportName.HorizontalAlignment = Element.ALIGN_CENTER;
            cellReportName.Phrase.Add(new Phrase(Environment.NewLine));
            cellReportName.Phrase.Add(new Phrase(Environment.NewLine));
            cellReportName.Border = Rectangle.NO_BORDER;
            cellReportName.Colspan = 2;
            headerTable.AddCell(cellReportName);

            //Source Branch Name
            var cellSourceBranchName = GetPdfCell("Source Branch Name: " + sourceBranch?.Name, font);
            if (sourceBranch != null)
            {
                cellSourceBranchName.HorizontalAlignment = Element.ALIGN_LEFT;
                cellSourceBranchName.Border = Rectangle.NO_BORDER;
                headerTable.AddCell(cellSourceBranchName);
            }
            else
            {
                cellSourceBranchName = GetPdfCell("", font);
                cellSourceBranchName.HorizontalAlignment = Element.ALIGN_LEFT;
                cellSourceBranchName.Border = Rectangle.NO_BORDER;
                headerTable.AddCell(cellSourceBranchName);
            }

            //From Date
            var cellFromDate = GetPdfCell("From: " + fromDate?.ToString("dd/MM/yyyy"), font);
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

            //Source Branch Name
            var cellDestinationBranchName = GetPdfCell("Destination Branch Name: " + destinationBranch?.Name, font);
            if (destinationBranch != null)
            {
                cellDestinationBranchName.HorizontalAlignment = Element.ALIGN_LEFT;
                cellDestinationBranchName.Border = Rectangle.NO_BORDER;
                headerTable.AddCell(cellDestinationBranchName);
            }
            else
            {
                cellDestinationBranchName = GetPdfCell("", font);
                cellDestinationBranchName.HorizontalAlignment = Element.ALIGN_LEFT;
                cellDestinationBranchName.Border = Rectangle.NO_BORDER;
                headerTable.AddCell(cellDestinationBranchName);
            }

            //To Date
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

        private void PrintTransferDetailReport(Font reportTitleFont, Document doc, List<TransferDetailViewModel> transferDetailViewModels, Font defaultFont, Font attributesFont)
        {
            var numColumns = 5;
            var transferDetailReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 10,15,40,20,15 } }
            };

            transferDetailReportTable.SetWidths(widths[numColumns]);

            //SL
            var cellTransferDetailReportItem = GetPdfCell("SL", defaultFont);
            cellTransferDetailReportItem.BackgroundColor = BaseColor.LightGray;
            cellTransferDetailReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            transferDetailReportTable.AddCell(cellTransferDetailReportItem);
            //Transfer No
            cellTransferDetailReportItem = GetPdfCell("Transfer No", defaultFont);
            cellTransferDetailReportItem.BackgroundColor = BaseColor.LightGray;
            cellTransferDetailReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            transferDetailReportTable.AddCell(cellTransferDetailReportItem);
            //Product Name
            cellTransferDetailReportItem = GetPdfCell("Product Name", defaultFont);
            cellTransferDetailReportItem.BackgroundColor = BaseColor.LightGray;
            cellTransferDetailReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            transferDetailReportTable.AddCell(cellTransferDetailReportItem);
            //Transfer Quantity
            cellTransferDetailReportItem = GetPdfCell("Transfer Qty", defaultFont);
            cellTransferDetailReportItem.BackgroundColor = BaseColor.LightGray;
            cellTransferDetailReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            transferDetailReportTable.AddCell(cellTransferDetailReportItem);
            //Amount
            cellTransferDetailReportItem = GetPdfCell("Amount", defaultFont);
            cellTransferDetailReportItem.BackgroundColor = BaseColor.LightGray;
            cellTransferDetailReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            transferDetailReportTable.AddCell(cellTransferDetailReportItem);


            transferDetailReportTable.HeaderRows = 1;
            var r = 0;
            foreach (var transferDetailObj in transferDetailViewModels)
            {
                r++;

                //SL
                cellTransferDetailReportItem = GetPdfCell(r, defaultFont);
                cellTransferDetailReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                transferDetailReportTable.AddCell(cellTransferDetailReportItem);
                //Transfer Name
                cellTransferDetailReportItem = GetPdfCell(transferDetailObj.TransferNo, defaultFont);
                cellTransferDetailReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                transferDetailReportTable.AddCell(cellTransferDetailReportItem);

                //Product Name
                cellTransferDetailReportItem = GetPdfCell(transferDetailObj.ProductName, defaultFont);
                cellTransferDetailReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                transferDetailReportTable.AddCell(cellTransferDetailReportItem);

                //Transfer Quantity
                cellTransferDetailReportItem = GetPdfCell(transferDetailObj.TransferQty, defaultFont);
                cellTransferDetailReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                transferDetailReportTable.AddCell(cellTransferDetailReportItem);
                //Amount
                cellTransferDetailReportItem = GetPdfCell(transferDetailObj.Amount, defaultFont);
                cellTransferDetailReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                transferDetailReportTable.AddCell(cellTransferDetailReportItem);
            }
            //Grand Total
            cellTransferDetailReportItem = GetPdfCell("GrandTotal", defaultFont);
            cellTransferDetailReportItem.Colspan = 3;
            cellTransferDetailReportItem.BackgroundColor = BaseColor.LightGray;
            cellTransferDetailReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            transferDetailReportTable.AddCell(cellTransferDetailReportItem);

            //Total Transfer Quantity
            cellTransferDetailReportItem = GetPdfCell(transferDetailViewModels.Sum(x => x.TransferQty), defaultFont);
            cellTransferDetailReportItem.BackgroundColor = BaseColor.LightGray;
            cellTransferDetailReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            transferDetailReportTable.AddCell(cellTransferDetailReportItem);

            cellTransferDetailReportItem = GetPdfCell(transferDetailViewModels.Sum(x => x.Amount), defaultFont);
            cellTransferDetailReportItem.BackgroundColor = BaseColor.LightGray;
            cellTransferDetailReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            transferDetailReportTable.AddCell(cellTransferDetailReportItem);



            doc.Add(transferDetailReportTable);
        }


        //Purchase
        public virtual void PrintPurchaseReportToPdf(MemoryStream stream, Branch? branch, List<PurchaseViewModel> purchaseViewModels, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (purchaseViewModels == null)
                throw new ArgumentNullException(nameof(purchaseViewModels));

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
            addressFont.Color = BaseColor.DarkGray;

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
            defaultFont.Size = 8;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Data
            PrintPurchaseReport(reportTitleFont, doc, purchaseViewModels, defaultFont, addressFont);

            var purchaseReportLineNumber = 20;
            var purchaseReportLineNumberCount = purchaseViewModels.Count;
            purchaseReportLineNumber++;

            if (purchaseReportLineNumber <= purchaseReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }

        protected virtual void PrintPurchaseReport(Font reportTitleFont, Document doc, List<PurchaseViewModel> purchaseViewModels, Font font, Font addressFont, int supplierId = 0)
        {
            //Test Font
            var testFont = GetFont();
            testFont.SetFamily("Times New Roman");
            testFont.Size = 9;
            testFont.SetStyle(Font.NORMAL);
            testFont.Color = BaseColor.Black;


            var numColumns = 9;
            var purchaseReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 4,10,14,22,10,11,9,9,11 } }
            };

            purchaseReportTable.SetWidths(widths[numColumns]);

            purchaseReportTable.AddCell(new PdfPCell(new Phrase("SL", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            purchaseReportTable.AddCell(new PdfPCell(new Phrase("Purchase No", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            purchaseReportTable.AddCell(new PdfPCell(new Phrase("Date", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            purchaseReportTable.AddCell(new PdfPCell(new Phrase("Supplier Name", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            purchaseReportTable.AddCell(new PdfPCell(new Phrase("Ref. No", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            purchaseReportTable.AddCell(new PdfPCell(new Phrase("Amount", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            purchaseReportTable.AddCell(new PdfPCell(new Phrase("Discount", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            purchaseReportTable.AddCell(new PdfPCell(new Phrase("Ext. Cost", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            purchaseReportTable.AddCell(new PdfPCell(new Phrase("Net Amount", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });

            purchaseReportTable.HeaderRows = 1;
            var sl = 0;
            foreach (var item in purchaseViewModels)
            {
                sl++;

                purchaseReportTable.AddCell(new PdfPCell(new Phrase(sl.ToString(), font)) { HorizontalAlignment = Element.ALIGN_CENTER });
                purchaseReportTable.AddCell(new PdfPCell(new Phrase(item.PurchaseNo, font)) { HorizontalAlignment = Element.ALIGN_LEFT });
                purchaseReportTable.AddCell(new PdfPCell(new Phrase(item.PurchaseDate.ToString("dd/MM/yyyy"), font)) { HorizontalAlignment = Element.ALIGN_CENTER });
                purchaseReportTable.AddCell(new PdfPCell(new Phrase(item.SupplierName, font)) { HorizontalAlignment = Element.ALIGN_LEFT });
                purchaseReportTable.AddCell(new PdfPCell(new Phrase(item.ReferenceNo, font)) { HorizontalAlignment = Element.ALIGN_LEFT });
                purchaseReportTable.AddCell(new PdfPCell(new Phrase(item.Subtotal.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                purchaseReportTable.AddCell(new PdfPCell(new Phrase(item.Discount.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                purchaseReportTable.AddCell(new PdfPCell(new Phrase(item.AdditionalCost.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                purchaseReportTable.AddCell(new PdfPCell(new Phrase(item.NetAmount.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });

                if (item.PurchaseItems.Count() > 0)
                {
                    purchaseReportTable.AddCell(new PdfPCell(new Phrase("Items", font)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.LEFT_BORDER });
                    purchaseReportTable.AddCell(new PdfPCell(new Phrase("Product", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
                    purchaseReportTable.AddCell(new PdfPCell(new Phrase("Exp. Date", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
                    purchaseReportTable.AddCell(new PdfPCell(new Phrase("Purchase Qty", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
                    purchaseReportTable.AddCell(new PdfPCell(new Phrase("Return Qty", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
                    purchaseReportTable.AddCell(new PdfPCell(new Phrase("Bonus Qty", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
                    purchaseReportTable.AddCell(new PdfPCell(new Phrase("Purchase Price", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
                    purchaseReportTable.AddCell(new PdfPCell(new Phrase("Amount", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });

                    foreach (var miniItem in item.PurchaseItems)
                    {
                        purchaseReportTable.AddCell(new PdfPCell(new Phrase("", font)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_LEFT, Border = Rectangle.LEFT_BORDER });
                        purchaseReportTable.AddCell(new PdfPCell(new Phrase(miniItem.Option, font)) { HorizontalAlignment = Element.ALIGN_LEFT });
                        purchaseReportTable.AddCell(new PdfPCell(new Phrase(miniItem.ExpireDate?.ToString("dd/MM/yyyy"), font)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        purchaseReportTable.AddCell(new PdfPCell(new Phrase(miniItem.PurchaseQuantity.ToString("#,##0"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                        purchaseReportTable.AddCell(new PdfPCell(new Phrase(miniItem.ReturnQuantity.ToString("#,##0"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                        purchaseReportTable.AddCell(new PdfPCell(new Phrase(miniItem.BonusQuantity.ToString("#,##0"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                        purchaseReportTable.AddCell(new PdfPCell(new Phrase(miniItem.PurchasePrice.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                        purchaseReportTable.AddCell(new PdfPCell(new Phrase(miniItem.Amount.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                    }

                }

            }

            purchaseReportTable.AddCell(new PdfPCell(new Phrase("Total", testFont)) { Colspan = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
            purchaseReportTable.AddCell(new PdfPCell(new Phrase(purchaseViewModels.Sum(x => x.Subtotal).ToString("#,##0.00"), testFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            purchaseReportTable.AddCell(new PdfPCell(new Phrase(purchaseViewModels.Sum(x => x.Discount).ToString("#,##0.00"), testFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            purchaseReportTable.AddCell(new PdfPCell(new Phrase(purchaseViewModels.Sum(x => x.AdditionalCost).ToString("#,##0.00"), testFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            purchaseReportTable.AddCell(new PdfPCell(new Phrase(purchaseViewModels.Sum(x => x.NetAmount).ToString("#,##0.00"), testFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });

            doc.Add(purchaseReportTable);
        }


        //Day Wise Sale
        public virtual void PrintDayWiseSaleReportToPdf(MemoryStream stream, Branch? branch, List<SalesData> summaryData, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (summaryData == null)
                throw new ArgumentNullException(nameof(summaryData));

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
            defaultFont.Size = 8;

            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //DayWiseSaleReport
            PrintDayWiseSaleReport(reportTitleFont, doc, summaryData, defaultFont, fromDate, toDate, branch);

            var summaryDataLineNumber = 20;
            var summaryDataLineNumberCount = summaryData.Count;
            summaryDataLineNumber++;

            if (summaryDataLineNumber <= summaryDataLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }

        protected virtual void PrintDayWiseSaleReport(Font reportTitleFont, Document doc, List<SalesData> summaryData, Font defaultFont, DateTime? fromDate, DateTime? toDate, Branch branch)
        {
            var numColumns = 7;
            var saleReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 10,15,15,15,15,15, 15} }
            };

            saleReportTable.SetWidths(widths[numColumns]);

            //SL
            var cellSaleReportItem = GetPdfCell("SL", defaultFont);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Sale Date
            cellSaleReportItem = GetPdfCell("Sale Date", defaultFont);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Payable
            cellSaleReportItem = GetPdfCell("Payable", defaultFont);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Paid
            cellSaleReportItem = GetPdfCell("Paid", defaultFont);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Due
            cellSaleReportItem = GetPdfCell("Due", defaultFont);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Due
            cellSaleReportItem = GetPdfCell("Return Amount", defaultFont);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);
            //Due
            cellSaleReportItem = GetPdfCell("Net Sale", defaultFont);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            saleReportTable.AddCell(cellSaleReportItem);

            saleReportTable.HeaderRows = 1;
            var r = 0;
            foreach (var pl in summaryData)
            {
                r++;

                //SL
                cellSaleReportItem = GetPdfCell(r, defaultFont);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                saleReportTable.AddCell(cellSaleReportItem);
                //Sale Date
                cellSaleReportItem = GetPdfCell(pl.Date.ToString("dd/MM/yyyy"), defaultFont);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                saleReportTable.AddCell(cellSaleReportItem);
                //Payable
                cellSaleReportItem = GetPdfCell(pl.Payable.ToString("#,##0.00"), defaultFont);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);
                //Paid
                cellSaleReportItem = GetPdfCell(pl.Paid.ToString("#,##0.00"), defaultFont);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);
                //Due
                cellSaleReportItem = GetPdfCell(pl.Due.ToString("#,##0.00"), defaultFont);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);
                //Due
                cellSaleReportItem = GetPdfCell(pl.ReturnAmount.ToString("#,##0.00"), defaultFont);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);
                //Due
                cellSaleReportItem = GetPdfCell((pl.Payable - pl.ReturnAmount).ToString("#,##0.00"), defaultFont);
                cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                saleReportTable.AddCell(cellSaleReportItem);
            }
            //SL
            cellSaleReportItem = GetPdfCell("Total", defaultFont);
            cellSaleReportItem.Colspan = 2;
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);
            //TotalPayable
            cellSaleReportItem = GetPdfCell(summaryData.Sum(x => x.Payable).ToString("#,##0.00"), defaultFont);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);
            //TotalPaid
            cellSaleReportItem = GetPdfCell(summaryData.Sum(x => x.Paid).ToString("#,##0.00"), defaultFont);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);
            //TotalDue
            cellSaleReportItem = GetPdfCell(summaryData.Sum(x => x.Due).ToString("#,##0.00"), defaultFont);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);
            //TotalDue
            cellSaleReportItem = GetPdfCell(summaryData.Sum(x => x.ReturnAmount).ToString("#,##0.00"), defaultFont);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);
            //TotalDue
            cellSaleReportItem = GetPdfCell((summaryData.Sum(x => x.Payable) - summaryData.Sum(x => x.ReturnAmount)).ToString("#,##0.00"), defaultFont);
            cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            saleReportTable.AddCell(cellSaleReportItem);

            //if (fromDate.HasValue && toDate.HasValue && branch != null)
            //{
            //    var saleReturnAmount = _commonService.GetSaleReturnTotal(fromDate.Value.Date, toDate.Value.Date, branch.Id);
            //    //..................total sale return..................................
            //    //SL
            //    cellSaleReportItem = GetPdfCell("Sale Return", defaultFont);
            //    cellSaleReportItem.Colspan = 2;
            //    cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            //    saleReportTable.AddCell(cellSaleReportItem);

            //    //Total Sale Return
            //    cellSaleReportItem = GetPdfCell(saleReturnAmount, defaultFont);
            //    cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            //    saleReportTable.AddCell(cellSaleReportItem);
            //    //EmptyColumn
            //    cellSaleReportItem = GetPdfCell("", defaultFont);
            //    cellSaleReportItem.Colspan = 2;
            //    cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            //    saleReportTable.AddCell(cellSaleReportItem);

            //    //..................net sale..................................
            //    //SL
            //    cellSaleReportItem = GetPdfCell("Net Sale", defaultFont);
            //    cellSaleReportItem.Colspan = 2;
            //    cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            //    cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            //    saleReportTable.AddCell(cellSaleReportItem);

            //    //Total Net Sale
            //    cellSaleReportItem = GetPdfCell(summaryData.Sum(x => x.Payable) - saleReturnAmount, defaultFont);
            //    cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            //    cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            //    saleReportTable.AddCell(cellSaleReportItem);
            //    //EmptyColumn
            //    cellSaleReportItem = GetPdfCell("", defaultFont);
            //    cellSaleReportItem.Colspan = 2;
            //    cellSaleReportItem.BackgroundColor = BaseColor.LightGray;
            //    cellSaleReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            //    saleReportTable.AddCell(cellSaleReportItem);
            //}

            doc.Add(saleReportTable);
        }


        //Stock Adjustment
        public void PrintStockAdjustmentReportToPdf(MemoryStream stream, List<StockAdjustmentViewModel> stockAdjustmentViewModels, string reportName, Branch? branch, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (stockAdjustmentViewModels == null)
                throw new ArgumentNullException(nameof(stockAdjustmentViewModels));

            var pageSize = PageSize.A4;
            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //headerTitleFont
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
            PrintReportHeader(pdfWriter, "Stock Adjustment Report", branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Data
            PrintStockAdjustmentReport(reportTitleFont, doc, stockAdjustmentViewModels, defaultFont, attributesFont);

            var stockAdjustmentReportLineNumber = 20;
            var stockAdjustmentReportLineNumberCount = stockAdjustmentViewModels.Count;
            stockAdjustmentReportLineNumber++;

            if (stockAdjustmentReportLineNumber <= stockAdjustmentReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }

        private void PrintStockAdjustmentReport(Font reportTitleFont, Document doc, List<StockAdjustmentViewModel> stockAdjustmentViewModels, Font defaultFont, Font attributesFont)
        {
            var numColumns = 7;
            var stockAdjustmentReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 8,12,27,22,8,10,13 } }
            };

            stockAdjustmentReportTable.SetWidths(widths[numColumns]);

            //SL
            var cellcustomerPaymentReportItem = GetPdfCell("SL", defaultFont);
            cellcustomerPaymentReportItem.BackgroundColor = BaseColor.LightGray;
            cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            stockAdjustmentReportTable.AddCell(cellcustomerPaymentReportItem);
            //Code
            cellcustomerPaymentReportItem = GetPdfCell("Code", defaultFont);
            cellcustomerPaymentReportItem.BackgroundColor = BaseColor.LightGray;
            cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            stockAdjustmentReportTable.AddCell(cellcustomerPaymentReportItem);
            //Product
            cellcustomerPaymentReportItem = GetPdfCell("Product", defaultFont);
            cellcustomerPaymentReportItem.BackgroundColor = BaseColor.LightGray;
            cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            stockAdjustmentReportTable.AddCell(cellcustomerPaymentReportItem);
            //Supplier
            cellcustomerPaymentReportItem = GetPdfCell("Supplier", defaultFont);
            cellcustomerPaymentReportItem.BackgroundColor = BaseColor.LightGray;
            cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            stockAdjustmentReportTable.AddCell(cellcustomerPaymentReportItem);
            //Quantity
            cellcustomerPaymentReportItem = GetPdfCell("Quantity", defaultFont);
            cellcustomerPaymentReportItem.BackgroundColor = BaseColor.LightGray;
            cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            stockAdjustmentReportTable.AddCell(cellcustomerPaymentReportItem);
            //Purchase Rate
            cellcustomerPaymentReportItem = GetPdfCell("Purchase Rate", defaultFont);
            cellcustomerPaymentReportItem.BackgroundColor = BaseColor.LightGray;
            cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            stockAdjustmentReportTable.AddCell(cellcustomerPaymentReportItem);
            //Amount
            cellcustomerPaymentReportItem = GetPdfCell("Amount", defaultFont);
            cellcustomerPaymentReportItem.BackgroundColor = BaseColor.LightGray;
            cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            stockAdjustmentReportTable.AddCell(cellcustomerPaymentReportItem);


            stockAdjustmentReportTable.HeaderRows = 1;
            var r = 0;
            foreach (var sl in stockAdjustmentViewModels)
            {
                r++;

                //SL
                cellcustomerPaymentReportItem = GetPdfCell(r, defaultFont);
                cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                stockAdjustmentReportTable.AddCell(cellcustomerPaymentReportItem);
                //Code
                cellcustomerPaymentReportItem = GetPdfCell(sl.Code, defaultFont);
                cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                stockAdjustmentReportTable.AddCell(cellcustomerPaymentReportItem);

                //Product
                cellcustomerPaymentReportItem = GetPdfCell(sl.Product?.Name + " " + sl.Product?.PackSize + " " + sl.Product?.Category?.Name, defaultFont);
                cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                stockAdjustmentReportTable.AddCell(cellcustomerPaymentReportItem);

                //Supplier
                cellcustomerPaymentReportItem = GetPdfCell(sl.SupplierName, defaultFont);
                cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                stockAdjustmentReportTable.AddCell(cellcustomerPaymentReportItem);

                //Quantity
                cellcustomerPaymentReportItem = GetPdfCell(sl.Quantity, defaultFont);
                cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                stockAdjustmentReportTable.AddCell(cellcustomerPaymentReportItem);

                //Purchase Rate
                cellcustomerPaymentReportItem = GetPdfCell(sl.PurchaseRate.ToString("0.00"), defaultFont);
                cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                stockAdjustmentReportTable.AddCell(cellcustomerPaymentReportItem);

                //Amount
                cellcustomerPaymentReportItem = GetPdfCell((sl.Quantity * sl.PurchaseRate).ToString("0.00"), defaultFont);
                cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                stockAdjustmentReportTable.AddCell(cellcustomerPaymentReportItem);


            }
            //..................total..................................
            //SL
            cellcustomerPaymentReportItem = GetPdfCell("Total", defaultFont);
            cellcustomerPaymentReportItem.Colspan = 4;
            cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            stockAdjustmentReportTable.AddCell(cellcustomerPaymentReportItem);

            //TotalQuantity
            cellcustomerPaymentReportItem = GetPdfCell(stockAdjustmentViewModels.Sum(x => x.Quantity), defaultFont);
            cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            stockAdjustmentReportTable.AddCell(cellcustomerPaymentReportItem);

            //TotalPurchaseRate
            cellcustomerPaymentReportItem = GetPdfCell(" ", defaultFont);
            cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            stockAdjustmentReportTable.AddCell(cellcustomerPaymentReportItem);

            //TotalQuantity
            cellcustomerPaymentReportItem = GetPdfCell(stockAdjustmentViewModels.Sum(x => x.Quantity * x.PurchaseRate).ToString("0.00"), defaultFont);
            cellcustomerPaymentReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            stockAdjustmentReportTable.AddCell(cellcustomerPaymentReportItem);


            doc.Add(stockAdjustmentReportTable);
        }

        // Customer Transaction
        public void PrintCustomerTransactionReportToPdf(MemoryStream stream, Branch? branch, IList<TransactionViewModel> customerTransactionViewModels, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (customerTransactionViewModels == null)
                throw new ArgumentNullException(nameof(customerTransactionViewModels));

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
            defaultFont.Size = 8;

            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Data
            PrintCustomerTransactionReport(reportTitleFont, doc, customerTransactionViewModels, defaultFont);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = customerTransactionViewModels.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }


        protected virtual void PrintCustomerTransactionReport(Font titleFont, Document doc, IList<TransactionViewModel> customerTransactionViewModels, Font font)
        {
            var numColumns = 4;
            var customerTransactionReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 6, 8,25,12 } }
            };

            customerTransactionReportTable.SetWidths(widths[numColumns]);

            //table header
            customerTransactionReportTable.AddCell(new PdfPCell(new Phrase("SL", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            customerTransactionReportTable.AddCell(new PdfPCell(new Phrase("Code", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            customerTransactionReportTable.AddCell(new PdfPCell(new Phrase("Customer Name", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            customerTransactionReportTable.AddCell(new PdfPCell(new Phrase("Balance", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });

            customerTransactionReportTable.HeaderRows = 1;
            var sl = 0;
            foreach (var item in customerTransactionViewModels)
            {
                if (item.Balance == 0M) continue;

                sl++;
                customerTransactionReportTable.AddCell(new PdfPCell(new Phrase(sl.ToString(), font)) { HorizontalAlignment = Element.ALIGN_CENTER });
                customerTransactionReportTable.AddCell(new PdfPCell(new Phrase(item.Code, font)) { HorizontalAlignment = Element.ALIGN_CENTER });
                customerTransactionReportTable.AddCell(new PdfPCell(new Phrase(item.Name, font)) { HorizontalAlignment = Element.ALIGN_LEFT });
                customerTransactionReportTable.AddCell(new PdfPCell(new Phrase(item.Balance.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            //..................total..................................
            customerTransactionReportTable.AddCell(new PdfPCell(new Phrase("Total", font)) { Colspan = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            customerTransactionReportTable.AddCell(new PdfPCell(new Phrase(customerTransactionViewModels.Sum(x => x.Balance).ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });

            doc.Add(customerTransactionReportTable);
        }

        // Supplier Transaction
        public void PrintSupplierTransactionReportToPdf(MemoryStream stream, Branch? branch, IList<TransactionViewModel> customerTransactionViewModels, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (customerTransactionViewModels == null)
                throw new ArgumentNullException(nameof(customerTransactionViewModels));

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
            defaultFont.Size = 8;

            //headers
            PrintReportHeader(pdfWriter, reportName, branch, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Data
            PrintSupplierTransactionReport(reportTitleFont, doc, customerTransactionViewModels, defaultFont);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = customerTransactionViewModels.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }


        protected virtual void PrintSupplierTransactionReport(Font titleFont, Document doc, IList<TransactionViewModel> customerTransactionViewModels, Font font)
        {
            var numColumns = 4;
            var customerTransactionReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 6, 8,25,12 } }
            };

            customerTransactionReportTable.SetWidths(widths[numColumns]);

            //table header
            customerTransactionReportTable.AddCell(new PdfPCell(new Phrase("SL", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            customerTransactionReportTable.AddCell(new PdfPCell(new Phrase("Code", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            customerTransactionReportTable.AddCell(new PdfPCell(new Phrase("Supplier Name", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            customerTransactionReportTable.AddCell(new PdfPCell(new Phrase("Balance", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });

            customerTransactionReportTable.HeaderRows = 1;
            var sl = 0;
            foreach (var item in customerTransactionViewModels)
            {
                if (item.Balance == 0M) continue;

                sl++;
                customerTransactionReportTable.AddCell(new PdfPCell(new Phrase(sl.ToString(), font)) { HorizontalAlignment = Element.ALIGN_CENTER });
                customerTransactionReportTable.AddCell(new PdfPCell(new Phrase(item.Code, font)) { HorizontalAlignment = Element.ALIGN_CENTER });
                customerTransactionReportTable.AddCell(new PdfPCell(new Phrase(item.Name, font)) { HorizontalAlignment = Element.ALIGN_LEFT });
                customerTransactionReportTable.AddCell(new PdfPCell(new Phrase(item.Balance.ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            //..................total..................................
            customerTransactionReportTable.AddCell(new PdfPCell(new Phrase("Total", font)) { Colspan = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            customerTransactionReportTable.AddCell(new PdfPCell(new Phrase(customerTransactionViewModels.Sum(x => x.Balance).ToString("#,##0.00"), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });

            doc.Add(customerTransactionReportTable);
        }
    }

}
