using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization.Internal;
using Rotativa.AspNetCore;
using WFHMicrositeMaintenance.Models;

namespace WFHMicrositeMaintenance.Controllers
{
    [Authorize]
    public class ProductionController : Controller
    {
        private readonly WFHMicrositeContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public ProductionController(WFHMicrositeContext context, IWebHostEnvironment env, IConfiguration configuration)
        {
            _context = context;
            _env = env;
            _configuration = configuration;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            SearchData data = new SearchData();
            if (HttpContext.Session.Get<SearchData>("SearchData") != null)
            {
                data = HttpContext.Session.Get<SearchData>("SearchData");
            }
            Production production = new Production() { List = new List<ProductionList>() };
            List<User> users = new List<User>();
            StringBuilder QueryString1 = new StringBuilder("");
            QueryString1.AppendFormat("SELECT * FROM dbo.[User] WHERE ");
            if (!string.IsNullOrEmpty(data.Tracking))
            {
                if (QueryString1.ToString().EndsWith("WHERE "))
                    QueryString1.AppendFormat("TrackingNumber='{0}' ", data.Tracking);
                else
                    QueryString1.AppendFormat("AND TrackingNumber='{0}' ", data.Tracking);
            }
            if (QueryString1.ToString().EndsWith("WHERE "))
            {
                if (data.Completed != default)
                    QueryString1.AppendFormat("(Completed>='{0}' AND Completed<'{1}') ", data.Completed, data.Completed.AddDays(1));
                else
                    QueryString1.AppendFormat("Completed IS NOT NULL ");
            }
            else
            {
                if (data.Completed != default)
                    QueryString1.AppendFormat("AND (Completed>='{0}' AND Completed<'{1}) ", data.Completed, data.Completed.AddDays(1));
                else
                    QueryString1.AppendFormat("AND Completed IS NOT NULL ");
            }
            QueryString1.AppendFormat("AND InProduction IS NULL");
            users = _context.User.FromSqlRaw(QueryString1.ToString()).ToList();
            bool add = false;
            foreach (var user in users)
            {
                user.OrderNumber = user.UserId.ToString().PadLeft(8, '0');
                List<UserSelection> userSelections = await _context.UserSelection.Where(x => x.UserId == user.UserId).ToListAsync();
                add = true;
                if (data.Fabric != 0)
                {
                    if (userSelections.Where(x => x.UserId == user.UserId && x.ProductOptionId == data.Fabric).FirstOrDefault() == null)
                        add = false;
                }
                if (data.Mesh != 0)
                {
                    if (userSelections.Where(x => x.UserId == user.UserId && x.ProductOptionId == data.Mesh).FirstOrDefault() == null)
                        add = false;
                }
                if (data.Frame != 0)
                {
                    if (userSelections.Where(x => x.UserId == user.UserId && x.ProductOptionId == data.Frame).FirstOrDefault() == null)
                        add = false;
                }
                if (add)
                {
                    production.List.Add(new ProductionList()
                    {
                        User = user,
                        Product = await _context.Product.FindAsync(user.ProductId)
                    });
                }
            }
            production.Fabrics = new SelectList(await _context.ProductOption.Where(x => x.Type == "Fabric").ToListAsync(), "ProductOptionId", "Name");
            production.Meshs = new SelectList(await _context.ProductOption.Where(x => x.Type == "Mesh").ToListAsync(), "ProductOptionId", "Name");
            production.Frames = new SelectList(await _context.ProductOption.Where(x => x.Type == "Frame").ToListAsync(), "ProductOptionId", "Name");
            return View(production);
        }

        // POST: SearchPOs/
        [HttpPost]
        public ActionResult SearchUsers(IFormCollection formCollection)
        {
            SearchData data = new SearchData
            {
                Fabric = formCollection["Fabric"] != "All" ? Convert.ToInt32(formCollection["Fabric"]) : 0,
                Mesh = formCollection["Mesh"] != "All" ? Convert.ToInt32(formCollection["Mesh"]) : 0,
                Frame = formCollection["Frame"] != "All" ? Convert.ToInt32(formCollection["Frame"]) : 0,
                Tracking = formCollection["Tracking"].ToString().Trim(),
                Completed = formCollection["Completed"] != "" ? Convert.ToDateTime(formCollection["Completed"]) : default,
                InProduction = formCollection["Production"] != "" ? Convert.ToDateTime(formCollection["Production"]) : default,
                Shipped = formCollection["Shipped"] != "" ? Convert.ToDateTime(formCollection["Shipped"]) : default
            };
            HttpContext.Session.Set<SearchData>("SearchData", data);

            return RedirectToAction("Index");
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            User user = await _context.User.FindAsync(id);
            user.OrderNumber = user.UserId.ToString().PadLeft(8, '0');
            user.AttnName = user.AttnName.ToUpper();
            user.PhoneNumber = String.Format("{0:(###) ###-####}", user.PhoneNumber);
            user.Address1 = user.Address1.ToUpper();
            user.Address2 = !string.IsNullOrEmpty(user.Address2) ? user.Address2 : " ";
            user.City = user.City.ToUpper();
            user.ProvinceState = user.ProvinceState.ToUpper();
            user.PostalZip = user.PostalZip.ToUpper();
            user.Country = user.Country.ToUpper();
            AlternatePonumbers alternatePonumbers = _context.AlternatePonumbers.Where(x => x.UserId == user.UserId).FirstOrDefault();
            if (alternatePonumbers != null)
            {
                user.PoNumber = alternatePonumbers.AlternatePonumber;
                user.WorkOrder = alternatePonumbers.Wo;
            }
            int fabric = 0;
            int mesh = 0;
            int frame = 0;
            List<UserSelection> userSelections = await _context.UserSelection.Where(x => x.UserId == id).ToListAsync();
            ProductOption productOption;
            foreach (var item in userSelections)
            {
                productOption = await _context.ProductOption.Where(x => x.ProductOptionId == item.ProductOptionId).FirstOrDefaultAsync();
                if (item.Type == "Fabric")
                {
                    fabric = item.ProductOptionId;
                    item.Image = productOption.Image;
                    item.Name = productOption.StockCode;
                }
                if (item.Type == "Mesh")
                {
                    mesh = item.ProductOptionId;
                    item.Image = productOption.Image;
                    item.Name = productOption.StockCode;
                }
                if (item.Type == "Frame")
                {
                    frame = item.ProductOptionId;
                    item.Image = productOption.Image;
                    item.Name = productOption.StockCode;
                }
            }
            Production production = new Production
            {
                User = user,
                Product = await _context.Product.FindAsync(user.ProductId),
                UserSelections = userSelections,
                Image = await _context.ProductImage.Where(x => x.ProductId == user.ProductId && x.ProductOption1Id == fabric && x.ProductOption2Id == mesh && x.ProductOption3Id == frame).Select(y => y.Image).FirstOrDefaultAsync()
            };
            if (production.Image == null)
            {
                production.Image = production.Product.Image;
            }

            return View(production);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            User user = await _context.User.FindAsync(id);
            user.OrderNumber = user.UserId.ToString().PadLeft(8, '0');
            user.AttnName = user.AttnName.ToUpper();
            user.PhoneNumber = String.Format("{0:(###) ###-####}", user.PhoneNumber);
            user.Address1 = user.Address1.ToUpper();
            user.Address2 = !string.IsNullOrEmpty(user.Address2) ? user.Address2 : " ";
            user.City = user.City.ToUpper();
            user.ProvinceState = user.ProvinceState.ToUpper();
            user.PostalZip = user.PostalZip.ToUpper();
            user.Country = user.Country.ToUpper();
            AlternatePonumbers alternatePonumbers = _context.AlternatePonumbers.Where(x => x.UserId == user.UserId).FirstOrDefault();
            if (alternatePonumbers != null)
            {
                user.PoNumber = alternatePonumbers.AlternatePonumber;
                user.WorkOrder = alternatePonumbers.Wo;
            }
            int fabric = 0;
            int mesh = 0;
            int frame = 0;
            List<UserSelection> userSelections = await _context.UserSelection.Where(x => x.UserId == id).ToListAsync();
            ProductOption productOption;
            foreach (var item in userSelections)
            {
                productOption = await _context.ProductOption.Where(x => x.ProductOptionId == item.ProductOptionId).FirstOrDefaultAsync();
                if (item.Type == "Fabric")
                {
                    fabric = item.ProductOptionId;
                    item.Image = productOption.Image;
                    item.Name = productOption.StockCode;
                }
                if (item.Type == "Mesh")
                {
                    mesh = item.ProductOptionId;
                    item.Image = productOption.Image;
                    item.Name = productOption.StockCode;
                }
                if (item.Type == "Frame")
                {
                    frame = item.ProductOptionId;
                    item.Image = productOption.Image;
                    item.Name = productOption.StockCode;
                }
            }
            Production production = new Production
            {
                User = user,
                Product = await _context.Product.FindAsync(user.ProductId),
                UserSelections = userSelections,
                Image = await _context.ProductImage.Where(x => x.ProductId == user.ProductId && x.ProductOption1Id == fabric && x.ProductOption2Id == mesh && x.ProductOption3Id == frame).Select(y => y.Image).FirstOrDefaultAsync()
            };
            if (production.Image == null)
            {
                production.Image = production.Product.Image;
            }

            return View(production);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Production production)
        {
            if (id != production.User.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    production.User.InProduction = DateTime.Now;
                    _context.Update(production.User);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(production.User.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(production);
        }

        public async Task<byte[]> GetOrderPdf(int id)
        {
            Production production = new Production();
            int fabric = 0;
            int mesh = 0;
            int frame = 0;
            int code = 0;
            string[] codes = new string[3] { "", "", "" };
            production.User = await _context.User.FindAsync(id);
            AlternatePonumbers alternatePonumbers = _context.AlternatePonumbers.Where(x => x.UserId == production.User.UserId).FirstOrDefault();
            if (alternatePonumbers != null)
            {
                production.User.PoNumber = alternatePonumbers.AlternatePonumber;
                production.User.WorkOrder = alternatePonumbers.Wo;
            }
            List<UserSelection> userSelections = await _context.UserSelection.Where(x => x.UserId == id).ToListAsync();
            ProductOption productOption;
            foreach (var item in userSelections)
            {
                productOption = await _context.ProductOption.Where(x => x.ProductOptionId == item.ProductOptionId).FirstOrDefaultAsync();
                if (item.Type == "Fabric")
                {
                    fabric = item.ProductOptionId;
                    item.Image = productOption.Image;
                    item.Name = productOption.StockCode;
                    codes[code] = "Fabric: " + productOption.StockCode;
                    code++;
                }
                if (item.Type == "Mesh")
                {
                    mesh = item.ProductOptionId;
                    item.Image = productOption.Image;
                    item.Name = productOption.StockCode;
                    codes[code] = "  Mesh: " + productOption.StockCode;
                    code++;
                }
                if (item.Type == "Frame")
                {
                    frame = item.ProductOptionId;
                    item.Image = productOption.Image;
                    item.Name = productOption.StockCode;
                    codes[code] = " Frame: " + productOption.StockCode;
                }
            }
            production.UserSelections = userSelections;
            production.User.PhoneNumber = String.Format("{0:(###) ###-####}", production.User.PhoneNumber);
            production.Image = await _context.ProductImage.Where(x => x.ProductId == production.User.ProductId && x.ProductOption1Id == fabric && x.ProductOption2Id == mesh && x.ProductOption3Id == frame).Select(y => y.Image).FirstOrDefaultAsync();
            if (production.Image == null)
            {
                production.Image = await _context.Product.Where(x => x.ProductId == production.User.ProductId).Select(y => y.Image).FirstOrDefaultAsync();
            }
            production.User.OrderNumber = production.User.UserId.ToString().PadLeft(8, '0');

            string address = production.User.Address1;
            if (!string.IsNullOrEmpty(production.User.Address2)) address += ", " + production.User.Address2;
            List<string> data = new List<string>
            {
                production.User.AttnName,
                address,
                production.User.City + ", " + production.User.ProvinceState + " " + production.User.PostalZip,
                production.User.Country,
                production.User.OrderNumber,
                codes[0],
                codes[1],
                codes[2],
                "0" + production.User.OrderNumber
            };
            ZPLPrint(data);

            var report = new ViewAsPdf("ProdPdf", production)
            {
                PageSize = Rotativa.AspNetCore.Options.Size.Letter,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(10, 0, 0, 10),
                MinimumFontSize = 22
            };
            var byteArray = report.BuildFile(this.ControllerContext);
            return byteArray.Result;
        }

        // Format the label data for printing and send the text to the printer
        private bool ZPLPrint(List<string> data)
        {
            bool bResult = false;
            string file = _env.WebRootPath + "\\labels\\" + _configuration.GetValue<string>("AppSettings:LabelFormat");
            string printer = _configuration.GetValue<string>("AppSettings:LabelPrinter");
            if (!string.IsNullOrEmpty(printer))
            {
                StreamReader sr = new StreamReader(file);
                if (sr != null)
                {
                    string label = sr.ReadToEnd();
                    sr.Close();
                    sr.Dispose();

                    label = label.Replace("%Date%", DateTime.Now.ToString("MM/dd/yy"));
                    label = label.Replace("%Time%", DateTime.Now.ToString("HH:mm"));
                    for (int i = 0; i < data.Count; i++)
                    {
                        label = label.Replace("%P" + i.ToString() + "%", data[i]);
                    }
                    try
                    {
                        bResult = RawPrinterHelper.SendStringToPrinter("Label", printer, label);
                    }
                    catch { }
                }
            }
            return bResult;
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.UserId == id);
        }
    }

    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }

    public class RawPrinterHelper
    {
        // Structure and API declarions:
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        // SendBytesToPrinter()
        // When the function is given a printer name and an unmanaged array
        // of bytes, the function sends those bytes to the print queue.
        // Returns true on success, false on failure.
        public static bool SendBytesToPrinter(string szName, string szPrinterName, IntPtr pBytes, Int32 dwCount)
        {
            _ = new IntPtr(0);
            DOCINFOA di = new DOCINFOA();
            bool bSuccess = false; // Assume failure unless you specifically succeed.

            di.pDocName = szName;
            di.pDataType = "RAW";
            // Open the printer.
            if (OpenPrinter(szPrinterName.Normalize(), out IntPtr hPrinter, IntPtr.Zero))
            {
                // Start a document.
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    // Start a page.
                    if (StartPagePrinter(hPrinter))
                    {
                        // Write your bytes.
                        bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out int dwWritten);
                        _ = dwWritten;
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }
            // If you did not succeed, GetLastError may give more information
            // about why not.
            if (bSuccess == false)
            {
                _ = Marshal.GetLastWin32Error();
            }
            return bSuccess;
        }

        public static bool SendFileToPrinter(string szPrinterName, string szFileName, string sBarcode, string sName, string sCopies)
        {
            string szName = szFileName[0..^4];
            // Open the file.
            FileStream fs = new FileStream(szFileName, FileMode.Open);
            // Create a BinaryReader on the file.
            BinaryReader br = new BinaryReader(fs);
            // Dim an array of bytes big enough to hold the file's contents.
            Byte[] bytes = new Byte[fs.Length * 2];
            Byte[] b1 = new Byte[fs.Length];
            int iCheck = 0;
            int i1 = 0;
            int i2 = 0;
            System.Text.Encoding enc = System.Text.Encoding.ASCII;
            Byte[] bBarcode = enc.GetBytes(sBarcode);
            Byte[] bName = enc.GetBytes(sName);
            Byte[] bCopies = enc.GetBytes(sCopies);
            // Your unmanaged pointer.
            IntPtr pUnmanagedBytes = new IntPtr(0);
            int nLength;

            nLength = Convert.ToInt32(fs.Length);
            // Read the contents of the file into the array.
            b1 = br.ReadBytes(nLength);

            while (i1 < fs.Length)
            {
                if (b1[i1] == '=')
                {
                    bytes[i2++] = b1[i1++];
                    switch (++iCheck)
                    {
                        case 1:
                        case 2:
                            for (int i = 0; i < sBarcode.Length; i++)
                                bytes[i2++] = bBarcode[i];
                            i1 += 11;
                            break;
                        case 3:
                            for (int i = 0; i < sName.Length; i++)
                                bytes[i2++] = bName[i];
                            i1 += 11;
                            break;
                    }
                }
                if (iCheck == 3 && b1[i1] == ',')
                {
                    bytes[i2++] = b1[i1++];
                    for (int i = 0; i < sCopies.Length; i++)
                        bytes[i2++] = bCopies[i];
                    i1 += 4;
                }
                bytes[i2++] = b1[i1++];
            }

            nLength = i2;

            // Allocate some unmanaged memory for those bytes.
            pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
            // Copy the managed byte array into the unmanaged array.
            Marshal.Copy(bytes, 0, pUnmanagedBytes, nLength);
            // Send the unmanaged bytes to the printer.
            bool bSuccess = SendBytesToPrinter(szName, szPrinterName, pUnmanagedBytes, nLength);
            // Free the unmanaged memory that you allocated earlier.
            Marshal.FreeCoTaskMem(pUnmanagedBytes);
            fs.Close();
            fs.Dispose();

            return bSuccess;
        }

        public static bool SendStringToPrinter(string szName, string szPrinterName, string szString)
        {
            IntPtr pBytes;
            Int32 dwCount;
            // How many characters are in the string?
            dwCount = szString.Length;
            // Assume that the printer is expecting ANSI text, and then convert
            // the string to ANSI text.
            pBytes = Marshal.StringToCoTaskMemAnsi(szString);
            // Send the converted ANSI string to the printer.
            SendBytesToPrinter(szName, szPrinterName, pBytes, dwCount);
            Marshal.FreeCoTaskMem(pBytes);
            return true;
        }
    }
}