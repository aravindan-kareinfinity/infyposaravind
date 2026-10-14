
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using InfyPOS.Models;
using InfyPOS.Common;
using InfyPOS.Data;

namespace InfyPOS.Controllers
{
    [RoutePrefix("CouponService")]
    public class GiftVoucherServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<GiftVoucher>();
            result.Item = new GiftVoucher();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<GiftVoucherCreteria> request)
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            List<GiftVoucher> result = await Task<List<GiftVoucher>>.Run(() => CouponData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchVoucher")]
        public async Task<IHttpActionResult> SearchVoucher(ActionRequest<GiftVoucherCreteria> request)
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            List<GiftVoucher> result = await Task<List<GiftVoucher>>.Run(() => CouponData.SearchVoucher(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchCustomerCreditNote")]
        public async Task<IHttpActionResult> SearchCustomerCreditNote(ActionRequest<GiftVoucherCreteria> request)
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            List<SearchCouponResult> result = await Task<List<SearchCouponResult>>.Run(() => CouponData.SearchCustomerCreditNote(request.Item));
            return this.Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintGiftVoucherPDF/{organizationid}/{id}/{code=null}")]
        public async Task<IHttpActionResult> PrintGiftVoucherPDF(string organizationid, string id, string code)
        {
            try
            {
                if (long.Parse(id) == 0) return null;
                GiftCouponDataService CouponData = new GiftCouponDataService();
                CouponData.Context = new UserClaim() { OrganizationID = long.Parse(organizationid) };
                List<GiftVoucher> lst = CouponData.SearchVoucher(new GiftVoucherCreteria() { id = long.Parse(id) });

                if (lst == null || lst.Count == 0 || lst[0].coupons == null || lst[0].coupons.Count == 0)
                    return null;

                if (string.IsNullOrEmpty(code)) return null;
                
                Customer cust = null;
                if (lst[0].customerid > 0)
                {
                    CustomerDataService customerDataService = new CustomerDataService();
                    cust = customerDataService.Search(new CustomerCreteria()
                    {
                        id = lst[0].customerid
                    }).First();
                }

                List<SearchVoucherResult> toprint = new List<SearchVoucherResult>();
                foreach (var item in lst[0].coupons)
                {
                    if (code.IndexOf('~' + item.code + '~') > -1 &&
                        item.amount > 0)
                    {
                        toprint.Add(new SearchVoucherResult()
                        {
                            name = lst[0].name,
                            mobileno = lst[0].mobileno,
                            givento = lst[0].givento,
                            seqcode = lst[0].seqcode,
                            vouchercode = item.code,
                            vouchervalue = item.amount,
                            issuedon = item.issuedon,
                            validfrom = item.issuedon,
                            validto = item.validupto,
                            totalvouchervalue = lst[0].receivedamt,
                            customername = (cust != null ? cust.name : ""),
                            customeraddress = (cust != null ? cust.address : "")
                        });
                    }
                }
                //if (!string.IsNullOrEmpty(code) && code != "null")
                //{
                //    toprint.RemoveAll(e => e.vouchercode != code);
                //}

                InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService { Context = new Common.UserClaim() { OrganizationID = 0 } };
                var lstconfig = config.Search(new ConfigurationCreteria
                {
                    key = "GIFTVOUCHERPDF"
                });

                if (lstconfig == null || lstconfig.Count == 0)
                    return null;

                ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(0);
                labreport.format = ReportLibrary.DosFormatParser.ParseXML(0, lstconfig[0].reportformat.format);
                labreport.Mode = ReportLibrary.DotMatrixReporter.PrintingMode.Single;
                labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
                labreport.Additional = new SortedDictionary<string, object>();
                ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
                printer.Report = labreport;
                printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
                labreport.DataSourceList = toprint;
                labreport.DataSource = toprint[0];

                using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
                {
                    var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                    fci.FileName = "supplierlabel.pdf";
                    fci.Extension = "application/pdf";
                    return new FileActionResult(true, "giftvoucher.pdf", fci.Extension, fci.Content);
                }
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }

        [HttpPost]
        [Route("PrintBarcode")]
        public async Task<IHttpActionResult> PrintBarcode(ActionRequest<SalesmanPrintCreteria> request)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                id = request.Item.reportid
            });

            if (reportformats.Count > 0)
            {
                InfyPOS.Report.CitizenBarcode barcodeprinter = new Report.CitizenBarcode(request.Item.companytitle,
                             reportformats[0].reportformat.format, reportformats[0].reportformat.mode, reportformats[0].nooflines);

                List<Salesman2Print> salesman2print = new List<Salesman2Print>();
                var barcodelist = request.Item.items.ToList();
                for (int i = 0; i < request.Item.copy; i++)
                {
                    salesman2print.AddRange(barcodelist);
                }
                var barcodeList = salesman2print.OrderBy(e => e.id).ToList().ConvertAll(e => (Report.IBarcodeFormat)e);
                var response = barcodeprinter.Render(barcodeList);
                List<string> base64Response = new List<string>();
                foreach (var item in response)
                {
                    var bytes = System.Text.Encoding.ASCII.GetBytes(item);
                    base64Response.Add(Convert.ToBase64String(bytes));
                }
                return this.Ok(base64Response);
            }

            return this.NotFound();
        }



        [HttpPost]
        [Route("PrintCouponReceipt")]
        public async Task<IHttpActionResult> PrintCouponReceipt(ActionRequest<VoucherPrintCreteria> request)
        {
            var organizationid = 0;
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                id = request.Item.reportid
            });

            if (reportformats.Count > 0)
            {
                List<Voucher2Print> salesman2print = new List<Voucher2Print>();
                GiftCouponDataService giftCouponDataService = new GiftCouponDataService();
                var barcodelist = giftCouponDataService.SearchCodes(request.Item.items);
                if (request.Item.couponid > 0)
                {
                    var coupon = giftCouponDataService.SearchCoupon(new GiftVoucherCreteria()
                    {
                        id = request.Item.couponid
                    });

                    if (coupon.Count > 0 && coupon[0].rules.Count > 0)
                    {
                        barcodelist.ForEach(e =>
                        {
                            e.name = coupon[0].name;
                            e.validfrom = coupon[0].validfrom;
                            e.validto = coupon[0].validto;
                            e.value = coupon[0].rules[0].amountaspercentage ? coupon[0].rules[0].amount.ToString("F0") + "%" : coupon[0].rules[0].amount.ToString("F0");
                        });
                    }
                }
                for (int i = 0; i < request.Item.copy; i++)
                {
                    salesman2print.AddRange(barcodelist);
                }

                var barcodeList = salesman2print.OrderBy(e => e.id).ToList().ConvertAll(e => (Report.IBarcodeFormat)e);

                ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
                ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid, new ReportLibrary.DotMatrixReporter.PrinterConfig());
                labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, reportformats[0].reportformat.format);
                labreport.Additional = new SortedDictionary<string, object>();
                var additional = new SortedDictionary<string, object>();

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (var pitem in barcodeList)
                {
                    labreport.DataSource = pitem;
                    labreport.DataItems = null;
                    var document = labreport.format.RenderBytes(new ImageData(), pitem, new List<Report.IBarcodeFormat>(new Report.IBarcodeFormat[]{pitem}), additional, true);
                    sb.Append(Convert.ToBase64String(document));
                    sb.Append(";");
                }
                request.Item.printdocument = sb.ToString();

                return this.Ok(request);
            }

            return this.NotFound();
        }


        [HttpPost]
        [Route("PrintCoupon")]
        public async Task<IHttpActionResult> PrintCoupon(ActionRequest<VoucherPrintCreteria> request)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                id = request.Item.reportid
            });

            if (reportformats.Count > 0)
            {
                InfyPOS.Report.CitizenBarcode barcodeprinter = new Report.CitizenBarcode(request.Item.companytitle,
                             reportformats[0].reportformat.format, reportformats[0].reportformat.mode, reportformats[0].nooflines);
                List<Voucher2Print> salesman2print = new List<Voucher2Print>();
                GiftCouponDataService giftCouponDataService = new GiftCouponDataService();
                List<Voucher2Print> barcodelist = null;
                if(request.Item.billsettlementmasterid>0)
                {
                    BillsettlementmasterDataService billsettlementmasterDataService = new BillsettlementmasterDataService();
                    var settlements = billsettlementmasterDataService.Search(new BillsettlementmasterCreteria() { id = request.Item.billsettlementmasterid });
                    barcodelist = giftCouponDataService.SearchCodes(settlements[0].paymentinfo.coupons.ConvertAll(x=>x.id));
                }
                else
                {
                    barcodelist = giftCouponDataService.SearchCodes(request.Item.items);
                }
                if(request.Item.couponid>0)
                {
                    var coupon = giftCouponDataService.SearchCoupon(new GiftVoucherCreteria()
                    {
                        id = request.Item.couponid
                    });

                    if(coupon.Count>0 && coupon[0].rules.Count>0)
                    {
                        barcodelist.ForEach(e =>
                        {
                            e.name = coupon[0].name;
                            e.validfrom = coupon[0].validfrom;
                            e.validto = coupon[0].validto;
                            e.value = coupon[0].rules[0].amountaspercentage? coupon[0].rules[0].amount.ToString("F0") +"%" : coupon[0].rules[0].amount.ToString("F0");
                        });
                    }
                }
                for (int i = 0; i < request.Item.copy; i++)
                {
                    salesman2print.AddRange(barcodelist);
                }

                var barcodeList = salesman2print.OrderBy(e => e.id).ToList().ConvertAll(e => (Report.IBarcodeFormat)e);
                var response = barcodeprinter.Render(barcodeList);
                List<string> base64Response = new List<string>();
                foreach (var item in response)
                {
                    var bytes = System.Text.Encoding.ASCII.GetBytes(item);
                    base64Response.Add(Convert.ToBase64String(bytes));
                }
                return this.Ok(base64Response);
            }

            return this.NotFound();
        }

        [HttpGet]
        [Route("SearchVoucherList")]
        public async Task<IHttpActionResult> SearchVoucherList()
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            List<SchemeCouponList> result = await Task<List<SchemeCouponList>>.Run(() => CouponData.SearchVoucherList());
            return this.Ok(result);
        }


        [HttpGet]
        [Route("SearchCouponList")]
        public async Task<IHttpActionResult> SearchCouponList()
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            List<SchemeCouponList> result = await Task<List<SchemeCouponList>>.Run(() => CouponData.SearchCouponList());
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchCouponIssue")]
        public async Task<IHttpActionResult> SearchCouponIssue(ActionRequest<GiftVoucherCreteria> request)
        {
            if (request.Item.iscoupononly)
            {
                GiftCouponDataService CouponData = new GiftCouponDataService();
                List<SearchCouponResult> result = await Task<List<SearchCouponResult>>.Run(() => CouponData.SearchCouponIssue(request.Item));
                return this.Ok(result);
            }
            else
            {
                GiftCouponDataService CouponData = new GiftCouponDataService();
                List<SearchCouponResult> result = await Task<List<SearchCouponResult>>.Run(() => CouponData.SearchSettlementCouponIssue(request.Item));
                return this.Ok(result);
            }
        }
        [HttpPost]
        [Route("SearchCoupons")]
        public async Task<IHttpActionResult> SearchCoupons(ActionRequest<GiftVoucherCreteria> request)
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            List<SchemeCoupon> result = await Task<List<SchemeCoupon>>.Run(() => CouponData.SearchCoupon(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchCouponConsumption")]
        public async Task<IHttpActionResult> SearchCouponConsumption(ActionRequest<GiftVoucherCreteria> request)
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            List<SearchCouponResult> result = await Task<List<SearchCouponResult>>.Run(() => CouponData.SearchCouponConsumption(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("CouponHistory/{mode}/{id}")]
        public async Task<IHttpActionResult> CouponHistory(string mode,string id)
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            List<SearchCouponResult> result = await Task<List<SearchCouponResult>>.Run(() => CouponData.CouponHistory(2,mode,id));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("VoucherHistory/{mode}/{id}")]
        public async Task<IHttpActionResult> VoucherHistory(string mode, string id)
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            List<SearchVoucherResult> result = await Task<List<SearchVoucherResult>>.Run(() => CouponData.VoucherHistory(1, mode, id));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchVoucherConsumption")]
        public async Task<IHttpActionResult> SearchVoucherConsumption(ActionRequest<GiftVoucherCreteria> request)
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            List<SearchVoucherResult> result = await Task<List<SearchVoucherResult>>.Run(() => CouponData.SearchVoucherConsumption(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchSettlementCoupon")]
        public async Task<IHttpActionResult> SearchSettlementCoupon(ActionRequest<GiftVoucherCreteria> request)
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            SearchVoucherResult result = await Task<SearchVoucherResult>.Run(() => CouponData.SearchSettlementCoupon(request.Item));
            return this.Ok(result);
        }

        //[HttpPost]
        //[Route("SearchVouchersList")]
        //public async Task<IHttpActionResult> SearchVouchersList(ActionRequest<GiftVoucherCreteria> request)
        //{
        //    GiftCouponDataService CouponData = new GiftCouponDataService();
        //    List<GiftVoucher> result = await Task<List<GiftVoucher>>.Run(() => CouponData.SearchVouchersList(request.Item));
        //    return this.Ok(result);
        //}

        [HttpPost]
        [Route("SaveCoupon")]
        public async Task<IHttpActionResult> SaveCoupon(ActionRequest<SchemeCoupon> request)
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            if (request.Item.id <= 0)
            {
                SchemeCoupon result = await Task<SchemeCoupon>.Run(() => CouponData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                SchemeCoupon result = await Task<SchemeCoupon>.Run(() => CouponData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("RemoveIssue")]
        public async Task<IHttpActionResult> RemoveIssue(ActionRequest<GiftVoucherIssue> request)
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            GiftVoucherIssue result = await Task<GiftVoucherIssue>.Run(() => CouponData.RemoveIssue(request.Item));
            return this.Ok(result);

        }


        [HttpPost]
        [Route("IssueVoucher")]
        public async Task<IHttpActionResult> IssueVoucher(ActionRequest<GiftVoucherIssue> request)
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            GiftVoucherIssue result = await Task<GiftVoucherIssue>.Run(() => CouponData.IssueVoucher(request.Item));
            return this.Ok(result);

        }

        [HttpPost]
        [Route("SaveVoucher")]
        public async Task<IHttpActionResult> SaveVoucher(ActionRequest<GiftVoucher> request)
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            if (request.Item.id <= 0)
            {
                GiftVoucher result = await Task<GiftVoucher>.Run(() => CouponData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                GiftVoucher result = await Task<GiftVoucher>.Run(() => CouponData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<GiftVoucher> request)
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            GiftVoucher result = await Task<GiftVoucher>.Run(() => CouponData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<GiftVoucher> request)
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            GiftVoucher result = await Task<GiftVoucher>.Run(() => CouponData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<GiftVoucher> request)
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            GiftVoucher result = await Task<GiftVoucher>.Run(() => CouponData.Delete(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("DeleteVoucher")]
        public async Task<IHttpActionResult> DeleteVoucher(ActionRequest<GiftVoucher> request)
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            GiftVoucher result = await Task<GiftVoucher>.Run(() => CouponData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("DeleteVoucherCode")]
        public async Task<IHttpActionResult> DeleteVoucherCode(ActionRequest<GiftVoucher> request)
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            GiftVoucher result = await Task<GiftVoucher>.Run(() => CouponData.DeleteVoucherCode(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("UpdateVoucherValidDate")]
        public async Task<IHttpActionResult> UpdateVoucherValidDate(ActionRequest<GiftVoucher> request)
        {
            GiftCouponDataService CouponData = new GiftCouponDataService();
            GiftVoucher result = await Task<GiftVoucher>.Run(() => CouponData.UpdateVoucherValidDate(request.Item));
            return this.Ok(result);
        }

    }
}