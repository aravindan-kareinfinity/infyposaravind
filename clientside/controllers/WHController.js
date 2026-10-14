
var app = angular.module('InfinityLab');

app.controller('WHReportController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$filter', 'Excel', '$timeout', '$stateParams','cntxService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, Excel, $timeout, $stateParams, cntxService) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Permission = UserDetails.Info;
        $scope.TransportModes = [{ 'id': 0, 'name': 'Transport' },
        { 'id': 1, 'name': 'Courier' }];
        $scope.reporttype = $stateParams.type;
        $scope.ReportType = [];
        $scope.AllReportType = [{ 'code': 'LRREPORT', 'name': 'LR Report' },
        { 'code': 'LRINVOICE', 'name': 'LR Invoice Report' },
        //{ 'code': 'LRPRODUCT', 'name': 'LR Product Report' },
        { 'code': 'PURCHASE', 'name': 'Purchase Report' },
        { 'code': 'PURCHASEHSN', 'name': 'Purchase HSN Report' },
        { 'code': 'PURCHASERETURN', 'name': 'Purchase Return Report' },
        { 'code': 'PURCHASERETURNCANCEL', 'name': 'Purchase Return(Cancelled) Report' },
        //{ 'code': 'DEBITNOTE', 'name': 'Debit Note Report' },
        { 'code': 'OUTWARD', 'name': 'Outward Report' },
        { 'code': 'OUTWARDPENDING', 'name': 'Pending Outward Report' },
        //{ 'code': 'BARCODE', 'name': 'Barcode Report' },
        { 'code': 'STOCK', 'name': 'Stock Report' },
        { 'code': 'STOCKANALYSIS', 'name': 'Stock Analysis Report' },
        { 'code': 'PRODUCTCREATE', 'name': 'Product Creation Report' },
        { 'code': 'SUPPCREATE', 'name': 'Supplier Creation Report' }

        ];

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        //    $rootScope.BindCompany($scope, 'SearchCondition.companyid');
        //    $rootScope.BindCompany($scope, 'SearchCondition.fromcompanyid');
        //    $rootScope.BindCompany($scope, 'SearchCondition.tocompanyid');
        });

        $scope.Purchase = restService.Get('ReferencelistService/PURCHASETAX', true).then(function (results) {
            $scope.Purchase = results;
        });

        $scope.Sales = restService.Get('ReferencelistService/SALESTAX', true).then(function (results) {
            $scope.Sales = results;
        });

        $scope.SalesCounter = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.SalesCounter = results;
        });
        $scope.BundleRacks = restService.Get('ReferencelistService/BUNDLERACK', true).then(function (results) {
            $scope.BundleRacks = results;
        });

        $scope.Agent = restService.Get('ReferencelistService/AGENT', true).then(function (results) {
            $scope.Agent = results;
        });
        $scope.city = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.city = results;
        });
        $scope.Supplier = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Supplier = results;
        });
        $scope.SupplierCompany = restService.Get('ReferencelistService/SUPPLIERCOMPANY', true).then(function (results) {
            $scope.SupplierCompany = results;
        });
        $scope.WHLocation = restService.Get('ReferencelistService/WHLOCATION', true).then(function (results) {
            $scope.WHLocation = results;
        });
        $scope.Location = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Location = results;
        });
        $scope.RetailLocations = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            $scope.RetailLocations = results;
        });
        $scope.AllLocation = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.AllLocation = results;
        //    $rootScope.BindLocation($scope, 'SearchCondition.locationid');
        //    $rootScope.BindLocation($scope, 'SearchCondition.fromlocationid');
        //    $rootScope.BindLocation($scope, 'SearchCondition.tolocationid');
        });
        $scope.Product = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Product = results;
        });
        $scope.Brand = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brand = results;
        });
        $scope.Fit = restService.Get('ReferencelistService/FIT', true).then(function (results) {
            $scope.Fit = results;
        });
        $scope.Sleeve = restService.Get('ReferencelistService/SLEEVE', true).then(function (results) {
            $scope.Sleeve = results;
        });
        $scope.Type = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.Type = results;
        });
        $scope.Transports = restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $scope.Transports = results;
        });
        $scope.Colour = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.Colour = results;
        });
        $scope.ProductGroups = restService.Get('ReferencelistService/PRODUCTGROUPS', true).then(function (results) {
            $scope.ProductGroups = results;
        });
        $scope.JobWorkers = restService.Get('ReferencelistService/SUPPLIER/1', true).then(function (results) {
            $scope.JobWorkers = results;
        });

        $scope.Sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.Sizes = results;
            var exist = false;
            $.each($scope.Sizes, function (k, v) {
                if (v.id == -2)
                    exist = true;
            });
            if (!exist) {
                $scope.Sizes.push({ id: -2, name: 'Jump' });
                $scope.Sizes.push({ id: -3, name: 'Cut' });
            }

        });

        $scope.RetrunType = [{ 'id': -1, 'name': 'All' }, { 'id': 0, 'name': 'Returns - Debit Note' }, { 'id': 1, 'name': 'DOA Debit Note' },
        { 'id': 2, 'name': 'DOA Credit Note' }, { 'id': 3, 'name': 'Price Drop Credit Note' },
        { 'id': 5, 'name': 'Discount Note' },
        { 'id': 6, 'name': 'Supplier Credit Note' }];

        $scope.PaymentModes = [{ 'id': 0, 'name': 'Inter Bank Transfer' },
        { 'id': 1, 'name': 'Cheque/DD' },
        { 'id': 2, 'name': 'Cash' },
        { 'id': 3, 'name': 'Multiple Cheque/DD' },
        { 'id': 4, 'name': 'Card' }];

        $scope.StockAge = [{ 'id': 1, 'name': '>Holding Period' },
        { 'id': 2, 'name': '<=30 days' },
        { 'id': 30, 'name': '>30 days' },
        { 'id': 45, 'name': '>45 days' },
        { 'id': 60, 'name': '>60 days' },
        { 'id': 90, 'name': '>90 days' },
        { 'id': 120, 'name': '>120 days' },
        { 'id': 180, 'name': '>180 days' },
        { 'id': 270, 'name': '>270 days' },
        { 'id': 360, 'name': '>360 days' }];
        $scope.Status = [{ 'id': 0, 'name': 'Pending' }, { 'id': 1, 'name': 'Completed' }, { 'id': 2, 'name': 'All' }];

        $scope.BundleStatus = [{ 'id': 0, 'name': 'New Bundle' }, { 'id': 1, 'name': 'Inwarded Bundle' },
            { 'id': 2, 'name': 'Inwarded Bundle with changes' }, { 'id': 9, 'name': 'Rejected Bundle' }];

        $scope.igrid = null;

        $scope.ModeofDelivery = [{ 'id': 1, 'name': 'Lorry' }, { 'id': 2, 'name': 'Hand Delivery' }];

        $scope.Material = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.Material = results;
        });
        $scope.Pattern = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.Pattern = results;
        });
        $scope.Style = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.Style = results;
        });
        $scope.Division = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Division = results;
        });
        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });
        $scope.PurchaseManager = restService.Get('ReferencelistService/PURCHASEMANAGER', true).then(function (results) {
            $scope.PurchaseManager = results;
        });

        $scope.lrcount = 0;
        $scope.totalcount = 0;
        $scope.lrbundels = 0;
        $scope.lrtotalvalue = 0;
        $scope.discount = 0;
        $scope.tax = 0;
        $scope.amount = 0;
        $scope.totalqty = 0;
        $scope.ShowStockView = false;
        $scope.SearchCondition = {
            mode: 1,
            withlr: false,
            fromdate: null,
            todate: null,
            fromlrdate: null,
            tolrdate: null,
            companyid: 0,
            supplierid: 0,
            productid: 0,
            brandid: 0,
            materialid: 0,
            patternid: 0,
            styleid: 0,
            sizeid: 0,
            sectionid: 0,
            designid: "",
            showdetail: false,
            summary: true,
            cancelled: false,
            barcode: "",
            packageno: "",
            pendinginward: "",
            modeofdelivery: 0,
            lrno: "",
            lrentryno: '',
            invoiceno: "",
            returnno: "",
            product: false,
            brand: false,
            material: false,
            pattern: false,
            style: false,
            size: false,
            design: false,
            supplier: false,
            section: false,
            productgroup: false,
            productgroupid: 0,
            paymentdate: new Date()

        };

        $scope.ShowFilter = true;
        $scope.ChangeReport = function () {
            $scope.SearchResult = [];
            $scope.LRResult = [];
            $scope.LRInvoiceResult = [];
            $scope.DebitnoteResult = [];
            $scope.PurchaseEntryResult = [];
            $scope.PurchaseListResult = [];
            $scope.SectionwisePurchaseResult = [];
            $scope.SectionwisePurchaseRetResult = [];
            $scope.SupplierPaymentResult = [];
            $scope.PurchaseHSNResult = [];
            $scope.POReturnResult = [];
            $scope.OutwardResult = [];
            $scope.BarcodeResult = [];
            $scope.StockResult = [];
            $scope.AsOnStockResult = [];
            $scope.StockTransactionResult = [];
            $scope.OutwardPendingResult = [];
            $scope.StockAnalysisResult = [];
            $scope.StockMovementResult = [];
        }

        $scope.SearchResult = [];
        $scope.LRResult = [];
        $scope.LRInvoiceResult = [];
        $scope.DebitnoteResult = [];
        $scope.PurchaseEntryResult = [];
        $scope.PurchaseListResult = [];
        $scope.SectionwisePurchaseResult = [];
        $scope.SectionwisePurchaseRetResult = [];
        $scope.SupplierPaymentResult = [];
        $scope.PurchaseHSNResult = [];
        $scope.POReturnResult = [];
        $scope.OutwardResult = [];
        $scope.JobOutwardResult = [];
        $scope.BarcodeResult = [];
        $scope.StockResult = [];
        $scope.OutwardPendingResult = [];
        $scope.StockAnalysisResult = [];
        $scope.AsOnStockResult = [];
        $scope.StockTransactionResult = [];
        $scope.AgentList = [];
        $scope.TaxList = [];
        $scope.StockMovementResult = [];
        $scope.SupplierPayResult = [];
        $scope.PReturnAuditResult = [];

        $scope.Productdetail = [];
        $scope.Barcodedetail = "";

        $scope.SearchLR = function () {
            $scope.LRResult = [];
            $scope.lrcount = 0;
            $scope.lrbundels = 0;
            $scope.lrtotalvalue = 0;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchLR",
                data: srequest
            }).success(function (result) {
                $scope.LRResult = result;
                $scope.lrcount = result.length;
                $.each(result, function (k, v) {
                    $scope.lrtotalvalue += v.totalamount;
                    $scope.lrbundels += v.noofbundels;
                });
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.LRConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchWHEntry = function () {
            $scope.LRResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchWHEntry",
                data: srequest
            }).success(function (result) {
                $scope.LRResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.WHEntryConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchLRInvoice = function () {
            $scope.LRInvoiceResult = [];
            $scope.lrcount = 0;
            $scope.lrtotalvalue = 0;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchLRInvoice",
                data: srequest
            }).success(function (result) {
                $scope.LRInvoiceResult = result;
                $scope.lrcount = result.length;
                $.each(result, function (k, v) {
                    $scope.lrtotalvalue += v.netamount;
                });
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }

                $scope.InvoiceConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchLRInvoiceDetail = function () {
            $scope.LRInvoiceDetailResult = [];

            request = {
                "item": { lrentryno: $scope.SearchCondition.lrentryno, 'showitems': true },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrinvoiceService/Search",
                data: srequest
            }).success(function (result) {
                if (result != undefined) {
                    $scope.Productdetail = result[0];
                }
            }).error(function (result) {
            });
        }


        $scope.taxdisplay = function (taxtype, item) {

            var ditem = item.taxbreakup.find(function (searchitem) {
                return searchitem.taxpercent == taxtype.percentage;
            });

            if (typeof (ditem) == 'undefinied' || ditem == null)
                return null;

            if (taxtype.type == 1)
                return ditem.taxableamount;
            else if (taxtype.type == 2)
                return ditem.taxamount;
            else
                return ditem.qty;
        };

        $scope.SearchTransportPayment = function () {

            $scope.SupplierPaymentResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: $scope.SearchCondition.groupbytransport ? "TransportpaymentService/SearchSupplierPaymentSummary" : "TransportpaymentService/SearchPendingPayment",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.SupplierPaymentResult = result;
                $scope.TransportPaymentConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchTransportPaymentReport = function () {
            $scope.SupplierPayResult = [];
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "TransportpaymentService/SearchAll",
                data: srequest
            }).success(function (result) {

                $scope.SupplierPayResult = result;
                $scope.TransportPaymentReportConfig();
            }).error(function (result) {
            });

        }

        $scope.SearchSupplierPayment = function () {
            $scope.SupplierPaymentResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: ($scope.SearchCondition.groupsupplier) ? "SupplierpaymentService/SearchSupplierPaymentSummary" : "SupplierpaymentService/SearchSupplierPayment",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                    return;
                }

                var newresult = [];
                var isonly = false;
                if ($scope.SearchCondition.onlyinvoice != undefined && $scope.SearchCondition.onlyinvoice != null
                    && $scope.SearchCondition.onlyinvoice) {
                    isonly = true;
                    $.each(result, function (k, v) {
                        if ($rootScope.GetNumber(v.balanceamount) > 0)
                            newresult.push(v);
                    });
                }
                if ($scope.SearchCondition.onlydebit != undefined && $scope.SearchCondition.onlydebit != null
                    && $scope.SearchCondition.onlydebit) {
                    isonly = true;
                    $.each(result, function (k, v) {
                        if ($rootScope.GetNumber(v.balanceamount) < 0)
                            newresult.push(v);
                    });
                }
                if (isonly)
                    $scope.SupplierPaymentResult = newresult;
                else
                    $scope.SupplierPaymentResult = result;
                $scope.SupplierPaymentConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchPurchase = function () {
            $scope.PurchaseEntryResult = [];
            $scope.TaxHeader = [];
            $scope.tax = 0;
            $scope.taxamount = 0;
            $scope.taxableamount = 0;
            $scope.amount = 0;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchPOEntry",
                data: srequest
            }).success(function (result) {
                if (result == null || result.poentry.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.PurchaseEntryResult = result.poentry;
                $scope.TaxHeader = result.taxhead;
                $scope.lrcount = $scope.PurchaseEntryResult.length;
                $.each($scope.PurchaseEntryResult, function (k, v) {
                    $.each(v.taxbreakup, function (k, vv) {
                        $scope.tax += vv.taxamount;
                        $scope.taxableamount += vv.taxableamount;
                    });
                    $scope.amount += v.invoiceamount;
                });
                $scope.PurchaseConfig();
            }).error(function (result) {
            });
        }


        $scope.SearchInvoicePo = function () {
            $scope.PurchaseListResult = [];

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchInoicePOSummary",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.PurchaseListResult = result;

                if ($scope.igrid != null)
                    $scope.igrid.destroy();
                var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
                igrid.options.showHeaderRow = true;
                igrid.EnableSerial($scope.PurchaseListResult);

                igrid.AddReferenceColumn('supplierid', 'Supplier', 100, $scope.Supplier);
                igrid.AddSimpleColumn('lrentryno', 'GRN No', 100);
                igrid.AddSimpleColumn('lrno', 'LR No', 80);
                igrid.AddSimpleColumn('invoiceno', 'Invoice No', 120);
                igrid.AddFormatedColumn('invoicetotal', 'Invoice Amount', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('entrydate', 'Entry Date', 100, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 100, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('pono', 'PO No', 80);
                igrid.AddFormatedColumn('podate', 'PO Date', 100, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('orderedpoqty', 'Ordered', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('invoicebilled', 'Current GRN', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('otherinvoicebilledqty', 'Other GRN', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('balancepoqty', 'Balance', 80, 'N2', 2).total = true;

                igrid.EnableTotal();
                igrid.Initialize();
                $scope.igrid = igrid;
                $scope.igrid.UpdateData($scope.PurchaseListResult);

            }).error(function (result) {
            });
        }

        $scope.SearchPurchaseList = function () {
            $scope.PurchaseListResult = [];
            $scope.taxamount = 0;
            $scope.taxableamount = 0;
            $scope.amount = 0;
            $scope.tax = 0;

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchPOEntryList",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }

                $.each(result, function (k, v) {
                    $scope.tax += v.taxamount;
                    $scope.taxableamount += v.taxableamount;
                    $scope.amount += v.invoiceamount;
                });

                $scope.PurchaseListResult = result;
                $scope.lrcount = $scope.PurchaseListResult.length;

                $scope.PurchaseListConfig();
            }).error(function (result) {
            });
        }


        $scope.SearchPurchaseTaxSummary = function () {
            $scope.PurchaseHSNResult = [];
            $scope.TaxHeaderHSN = [];
            $scope.hsccount = 0;
            $scope.totaltaxbleamount = 0;
            $scope.totaltaxamount = 0;
            $scope.toatlinvoiceamount = 0;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchPurchaseTaxSummary",
                data: srequest
            }).success(function (result) {

                $scope.SalesTaxSummaryResult = result;

                if ($scope.igrid != null)
                    $scope.igrid.destroy();

                var igrid = new InfyGrid('gridContainer', '500px', '100%', true, true, true, 0);
                igrid.options.showHeaderRow = true;

                var taxlist = [];


                $.each($scope.SalesTaxSummaryResult, function (k, v) {
                    var exist = taxlist.find(function (item) {
                        return item.companyid == v.companyid &&
                            item.taxpercentage == v.taxpercentage;
                    });

                    if (exist == undefined) {
                        if (v.rowmode == 'PR') {
                            v.ramount = v.amount;
                            v.rsgst = v.sgst;
                            v.rcgst = v.cgst;
                            v.rigst = v.igst;
                            v.rtaxamount = v.taxamount;
                            v.rtotal = v.total;
                        }
                        else {
                            v.samount = v.amount;
                            v.ssgst = v.sgst;
                            v.scgst = v.cgst;
                            v.sigst = v.igst;
                            v.staxamount = v.taxamount;
                            v.stotal = v.total;
                        }
                        taxlist.push(v);
                    }
                    else {
                        if (v.rowmode == 'PR') {
                            exist.ramount = v.amount;
                            exist.rsgst = v.sgst;
                            exist.rcgst = v.cgst;
                            exist.rigst = v.igst;
                            exist.rtaxamount = v.taxamount;
                            exist.rtotal = v.total;
                        }
                        else {
                            exist.samount = v.amount;
                            exist.ssgst = v.sgst;
                            exist.scgst = v.cgst;
                            exist.sigst = v.igst;
                            exist.staxamount = v.taxamount;
                            exist.stotal = v.total;
                        }                        
                    }
                })

                igrid.EnableSerial(taxlist);

                igrid.AddReferenceColumn('companyid', 'Company', 150, $scope.Company);
                igrid.AddFormatedColumn('taxpercentage', 'TAX %', 100, 'N2', 2);
                var groupColumns = [];
                groupColumns.push(
                    {
                        id: 'name',
                        name: 'Details',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
                igrid.columns = [];

                igrid.AddFormatedColumn('samount', 'TAXABLE', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('ssgst', 'SGST', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('scgst', 'CGST', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('sigst', 'IGST', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('staxamount', 'TAX', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('stotal', 'TOTAL', 100, 'N2', 2).total = true;
                groupColumns.push(
                    {
                        id: 'name',
                        name: 'Purchase',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
                igrid.columns = [];

                igrid.AddFormatedColumn('ramount', 'TAXABLE', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('rsgst', 'SGST', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('rcgst', 'CGST', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('rigst', 'IGST', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('rtaxamount', 'TAX', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('rtotal', 'TOTAL', 100, 'N2', 2).total = true;
                groupColumns.push(
                    {
                        id: 'name',
                        name: 'Purchase Return',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
                igrid.columns = [];

                igrid.AddFormatedColumn('amount', 'TAXABLE', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('sgst', 'SGST', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('cgst', 'CGST', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('igst', 'IGST', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('taxamount', 'TAX', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('total', 'TOTAL', 100, 'N2', 2).total = true;
                groupColumns.push(
                    {
                        id: 'name',
                        name: 'Net Purchase',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });

                $.each(taxlist, function (k, v) {
                    v.amount = v.samount + v.ramount;
                    v.sgst = v.ssgst + v.rsgst;
                    v.cgst = v.scgst + v.rcgst;
                    v.igst = v.sigst + v.rigst;
                    v.taxamount = v.staxamount + v.rtaxamount;
                    v.total = v.stotal + v.rtotal;
                })
                igrid.columns = groupColumns;
                igrid.EnableTotal();
                igrid.Initialize();
                $scope.igrid = igrid;
                $scope.igrid.UpdateData(taxlist);

            }).error(function (result) {
            });
        }

        $scope.SearchPurchaseTax = function () {
            $scope.PurchaseHSNResult = [];
            $scope.TaxHeaderHSN = [];
            $scope.hsccount = 0;
            $scope.totaltaxbleamount = 0;
            $scope.totaltaxamount = 0;
            $scope.toatlinvoiceamount = 0;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchPurchaseTax",
                data: srequest
            }).success(function (result) {
                if (result == null || result.poentry.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.PurchaseHSNResult = result.poentry;
                $scope.hsccount = result.poentry.length;
                $scope.TaxHeaderHSN = result.taxhead;

                $.each($scope.PurchaseHSNResult, function (k, v) {
                    $.each(v.taxbreakup, function (k, v) {
                        if (v.taxpercent != 100) {
                            $scope.totaltaxbleamount += v.taxableamount;
                            $scope.totaltaxamount += v.taxamount;
                            $scope.toatlinvoiceamount = ($scope.totaltaxbleamount + $scope.totaltaxamount);
                        }
                    });
                });


                if ($scope.igrid != null)
                    $scope.igrid.destroy();

                var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
                igrid.options.showHeaderRow = true;
                igrid.EnableSerial($scope.PurchaseHSNResult);

                igrid.AddSimpleColumn('supplier', 'Supplier', 200);
                igrid.AddReferenceColumn('cityid', 'City', 160, $scope.city);
                igrid.AddCustomColumn('isinterstatesale', 'IGST', 100, function (item, col) {
                    return item.isinterstatesale == true ? "YES" : "NO";
                });

                $.each($scope.TaxHeaderHSN, function (k, v) {

                    var column = igrid.AddCustomColumn('tax_col_' + k, v.displaytext, 80, function (item, col) {

                        var taxtype = col.internal;
                        var ditem = item.taxbreakup.find(function (searchitem) {
                            return searchitem.taxpercent == taxtype.percentage;
                        });

                        if (typeof (ditem) == 'undefinied' || ditem == null)
                            return null;

                        if (taxtype.type == 1)
                            return ditem.taxableamount.round($scope.rounding);
                        else if (taxtype.type == 2)
                            return ditem.taxamount.round($scope.rounding);
                        else
                            return ditem.qty.round($scope.rounding);
                    });

                    column.internal = v;
                    column.cssClass = 'rightalign';
                    column.total = true;

                    column.getValue = function (item, col) {

                        var taxtype = col.internal;
                        var ditem = item.taxbreakup.find(function (searchitem) {
                            return searchitem.taxpercent == taxtype.percentage;
                        });

                        if (typeof (ditem) == 'undefinied' || ditem == null)
                            return 0;

                        if (taxtype.type == 1)
                            return ditem.taxableamount.round($scope.rounding);
                        else if (taxtype.type == 2)
                            return ditem.taxamount.round($scope.rounding);
                        else
                            return ditem.qty.round($scope.rounding);

                    }

                });

                igrid.options.autoEdit = true;

                igrid.EnableTotal();
                igrid.Initialize();
                $scope.igrid = igrid;
                $scope.igrid.UpdateData($scope.PurchaseHSNResult);

            }).error(function (result) {
            });
        }

        $scope.SearchPurchaseHSN = function () {
            $scope.PurchaseHSNResult = [];
            $scope.TaxHeaderHSN = [];
            $scope.hsccount = 0;
            $scope.totaltaxbleamount = 0;
            $scope.totaltaxamount = 0;
            $scope.toatlinvoiceamount = 0;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchPurchaseHSN",
                data: srequest
            }).success(function (result) {
                if (result == null || result.poentry.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.PurchaseHSNResult = result.poentry;
                $scope.hsccount = result.poentry.length;
                $scope.TaxHeaderHSN = result.taxhead;

                $.each($scope.PurchaseHSNResult, function (k, v) {
                    $.each(v.taxbreakup, function (k, v) {
                        if (v.taxpercent != 100) {
                            $scope.totaltaxbleamount += v.taxableamount;
                            $scope.totaltaxamount += v.taxamount;
                            $scope.toatlinvoiceamount = ($scope.totaltaxbleamount + $scope.totaltaxamount);
                        }
                    });
                });
                $scope.PurchaseHSNConfig();

            }).error(function (result) {
            });
        }

        $scope.SearchSectionwisePurchase = function () {
            $scope.SectionwisePurchaseResult = [];
            $scope.taxamount = 0;
            $scope.taxableamount = 0;
            $scope.amount = 0;

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchPurchaseSectionwise",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                    return;
                }
                $.each(result, function (k, v) {
                    $scope.taxamount += v.taxamount;
                    $scope.taxableamount += v.taxableamount;
                    $scope.amount += (v.taxableamount + v.taxamount);
                });
                $scope.SectionwisePurchaseResult = result;
                $scope.PurchaseSectionwiseConfig();
            }).error(function (result) {
            });
        }
        $scope.SearchSectionwisePurchaseRet = function () {
            $scope.SectionwisePurchaseRetResult = [];
            $scope.taxamount = 0;
            $scope.taxableamount = 0;
            $scope.amount = 0;

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchPurchaseReturnSectionwise",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                    return;
                }
                $.each(result, function (k, v) {
                    $scope.taxamount += v.taxamount;
                    $scope.taxableamount += v.taxableamount;
                    $scope.amount += (v.taxableamount - v.taxamount);
                });
                $scope.SectionwisePurchaseRetResult = result;
                $scope.PurchaseRetSectionwiseConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchPOReturn = function () {
            $scope.POReturnResult = [];
            $scope.TaxHeaderPurchaseReturn = [];
            $scope.tax = 0;
            $scope.amount = 0;
            $scope.SearchCondition.cancelled = false;

            if ($scope.SearchCondition.returntypeid == undefined || $scope.SearchCondition.returntypeid == null)
                $scope.SearchCondition.returntypeid = -1;

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchPurchaseReturn",
                data: srequest
            }).success(function (result) {
                $scope.POReturnResult = result.purchasereturn;
                $scope.TaxHeaderPurchaseReturn = result.taxhead;
                $scope.lrcount = $scope.POReturnResult.length;
                $scope.tax = 0;
                $scope.amount = 0;
                $scope.taxable = 0;
                $.each($scope.POReturnResult, function (k, v) {
                    $scope.tax += v.taxamount;
                    $scope.taxable += v.returnamount;
                    $scope.amount += v.netamount;
                });

                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.PurchseRetConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchPOReturnCancel = function () {
            $scope.POReturnResult = [];
            $scope.TaxHeaderPurchaseReturn = [];
            $scope.tax = 0;
            $scope.amount = 0;
            $scope.SearchCondition.cancelled = true;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchPurchaseReturn",
                data: srequest
            }).success(function (result) {
                $scope.POReturnResult = result.purchasereturn;
                $scope.TaxHeaderPurchaseReturn = result.taxhead;
                $scope.lrcount = $scope.POReturnResult.length;
                $.each($scope.POReturnResult, function (k, v) {
                    $.each(v.taxbreakup, function (k, vv) {
                        $scope.tax += vv.taxamount;
                        $scope.amount += vv.taxableamount;
                    });
                });
                $scope.PurchseRetCancelConfig();
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
            }).error(function (result) {
            });
        }


        $scope.AddDepreciation = function () {
            if ($scope.DepreciationList == undefined ||
                $scope.DepreciationList == null)
                $scope.DepreciationList = [];
            $scope.DepreciationList.push({ 'fromage': $scope.fromage, 'toage': $scope.toage, 'depreciation': $scope.depreciation });

            $scope.fromage = $scope.toage;
            $scope.toage = '';
            $scope.depreciation = '';

            $scope['setFocustoage']()
        }

        $scope.RemoveDepreciation = function (item) {
            $scope.DepreciationList.splice($scope.DepreciationList.indexOf(item), 1);
        }

        $scope.DepreciationList = [];
        $scope.SearchStockDepreciation = function () {
            $scope.StockDepreciationList = [];
            request = {
                "item": {
                    'companyid': $scope.SearchCondition.companyid,
                    'asondate':$scope.asondate,
                    'config': $scope.DepreciationList
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/StockDepreciation",
                data: srequest
            }).success(function (result) {

                $scope.StockDepreciationList = result;

                if ($scope.igrid != null)
                    $scope.igrid.destroy();
                var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
                igrid.options.showHeaderRow = true;
                igrid.EnableSerial($scope.StockDepreciationList);

                igrid.AddSimpleColumn('display', 'Description', 150);
                igrid.AddFormatedColumn('qty', 'Qty', 60, 'N2', 2).total = true;
                igrid.AddFormatedColumn('value', 'Value', 60, 'N2', 2).total = true;
                igrid.AddFormatedColumn('dpvalue', 'Depreciation', 60, 'N2', 2).total = true;
                igrid.AddFormatedColumn('stvalue', 'Stock', 60, 'N2', 2).total = true;
                igrid.EnableTotal();
                igrid.Initialize();
                $scope.igrid = igrid;
                $scope.igrid.UpdateData($scope.StockDepreciationList);

            }).error(function (result) {
            });
        }


        $scope.SearchStock = function () {
            $scope.totalqty = 0;
            $scope.amount = 0;
            $scope.buyingprice = 0;
            $scope.StockResult = [];

            if ($scope.Appsetting_Company) {
                if (typeof ($scope.SearchCondition.companyid) == 'undefined' || $scope.SearchCondition.companyid == null
                    || $scope.SearchCondition.companyid < 0) {
                    $rootScope.Toaster.Info("Please select the company");
                    return;
                }
            }

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchStock",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                    return;
                }

                $scope.StockResult = result;
                $.each(result, function (k, v) {
                    $scope.totalqty += v.qty;
                    $scope.amount += v.qty * v.price;
                    $scope.buyingprice += v.qty * v.buyingprice;

                });
                $scope.StockConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchPurchaseItems = function () {
            $scope.totalqty = 0;
            $scope.amount = 0;
            $scope.buyingprice = 0;
            $scope.StockResult = [];

            if ($scope.Appsetting_Company) {
                if (typeof ($scope.SearchCondition.companyid) == 'undefined' || $scope.SearchCondition.companyid == null
                    || $scope.SearchCondition.companyid < 0) {
                    $rootScope.Toaster.Info("Please select the company");
                    return;
                }
            }

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchPurchaseItems",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                    return;
                }
                $scope.StockResult = result;
                $scope.PurchaseItemsConfig();
            }).error(function (result) {
            });
        }

        //$scope.SearchBarcodeSummary = function (mode) {
        //    $scope.SearchCondition.mode = mode;
        //    $scope.SearchBarcodeDetails();
        //}

        $scope.fileuploadedcomplete = function (value) {
            $scope.uploadfile = null;
            var condition = {
                barcodes: [],
                locationid: $scope.SearchCondition.locationid,
                companyid: $scope.SearchCondition.companyid
            };

            $scope.StockResult = [];

            $.each(value.rows, function (k, v) {
                var barcode = v.values[0];
                if (barcode != null && barcode != '' && barcode.length > 0) {
                    condition.barcodes.push(barcode)
                }
            });

            if (condition.barcodes.length > 0) {
                request = {
                    "item": condition,
                    "securitytoken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "WarehouseReportService/SearchBarcodeDetails",
                    data: srequest
                }).success(function (result) {
                    $scope.StockResult = result;
                    if (result == null || result.length == 0) {
                        $rootScope.Toaster.Info("No Results Found.");
                    }
                    $scope.BarcodeDetailConfig();
                    $scope.SearchCondition.isbit = false;
                }).error(function (result) {
                });
            }
        }

        $scope.SearchBarcodeDetails = function (mode) {
            $scope.SearchCondition.mode = mode;
            $scope.totalqty = 0;
            $scope.amount = 0;
            $scope.buyingprice = 0;
            $scope.StockResult = [];

            if ($scope.Appsetting_Company) {
                if (typeof ($scope.SearchCondition.companyid) == 'undefined' || $scope.SearchCondition.companyid == null
                    || $scope.SearchCondition.companyid < 0) {
                    $rootScope.Toaster.Info("Please select the company");
                    return;
                }
            }

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchBarcodeDetails",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                    return;
                }
                $scope.StockResult = result;
                if ($scope.SearchCondition.mode == 1)
                    $scope.BarcodeSummaryConfig();
                else if ($scope.SearchCondition.mode == 2)
                    $scope.PurchaseDetailConfig();
                else
                    $scope.BarcodeDetailConfig();
                $scope.SearchCondition.isbit = false;
            }).error(function (result) {
            });
        }

        $scope.SearchPendingTransfer = function () {
            $scope.totalqty = 0;
            $scope.amount = 0;
            $scope.OutwardPendingResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchPendingTransfer",
                data: srequest
            }).success(function (result) {
                $scope.OutwardPendingResult = result;
                $.each(result, function (k, v) {
                    $scope.totalqty += v.qty;
                    $scope.amount += v.qty * (v.price - v.discount);
                });
                $scope.PendingOutwardConfig();
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
            }).error(function (result) {
            });
        }

        $scope.SearchOutward = function () {
            if ($scope.reporttype == "INWARDPENDING")
                $scope.SearchCondition.pendinginward = true;

            $scope.totalcount = 0;
            $scope.totalqty = 0;
            //if ($scope.SearchCondition.showdetail == false && $scope.SearchCondition.withlr == false
            //    && ($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == null)
            //    && ($scope.SearchCondition.supplierid == 0 || $scope.SearchCondition.supplierid == null)) {
            //    $rootScope.Toaster.Info("Please select Location or Supplier");
            //    return;
            //}
            //else if ($scope.SearchCondition.pendinginward && ($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == null)
            //    && ($scope.SearchCondition.supplierid == 0 || $scope.SearchCondition.supplierid == null)) {
            //    $rootScope.Toaster.Info("Please select Location");
            //    return;
            //}

            $scope.OutwardResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchOutwardCost",
                data: srequest
            }).success(function (result) {
                $scope.OutwardResult = result;
                $scope.totalcount = result.length;
                $.each(result, function (k, v) {
                    $scope.totalqty += v.qty;
                });
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.OutwardConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchJobInward = function () {

            if ($scope.SearchCondition.status == undefined || $scope.SearchCondition.status == null)
                $scope.SearchCondition.status = 0;
            $scope.JobOutwardResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "JobworkbillgrnitemsService/SearchJobWorkItems",
                data: srequest
            }).success(function (result) {
                $scope.JobOutwardResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                    return;
                }
                var count = 0;
                $.each($scope.JobOutwardResult, function (k, v) {
                    v.id = count++;
                });
                $scope.JobInwardConfig();
            }).error(function (result) {
            });
        }


        $scope.SearchJobOutward = function () {

            if ($scope.SearchCondition.status == undefined || $scope.SearchCondition.status == null)
                $scope.SearchCondition.status = 0;
            $scope.JobOutwardResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "JobworkbillitemsService/SearchJobWorkItems",
                data: srequest
            }).success(function (result) {
                $scope.JobOutwardResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                    return;
                }
                var count = 0;
                $.each($scope.JobOutwardResult, function (k, v) {
                    v.id = count++;
                });
                $scope.JobOutwardConfig();
            }).error(function (result) {
            });
        }

        $scope.JobInwardConfig = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.JobOutwardResult);

            igrid.AddReferenceColumn('companyid', 'Company', 100, $scope.Company);
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 80);
            igrid.AddSimpleColumn('barcode', 'Barcode', 80);
            igrid.AddSimpleColumn('jobworker', 'Job Worker', 180);
            igrid.AddFormatedColumn('entrydate', 'Entry Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('invoicedate', 'Due Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('sourcesupplier', 'Supplier', 160);
            igrid.AddSimpleColumn('inproduct', 'Product', 120);
            igrid.AddFormatedColumn('oqty', 'Qty', 80, 'N0', 0).total = true;
            igrid.AddFormatedColumn('materialcost', 'Material Cost', 80, 'N0', 0).total = true;
            igrid.AddFormatedColumn('materialsalevalue', 'Material Value', 80, 'N0', 0).total = true;
            igrid.AddFormatedColumn('chargetotal', 'Job Charge', 80, 'N0', 0).total = true;
            igrid.AddSimpleColumn('grnno', 'GRN No', 80);
            igrid.AddSimpleColumn('grndate', 'GRN Date', 80);

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.JobOutwardResult);
        }

        $scope.JobOutwardConfig = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.JobOutwardResult);

            igrid.AddReferenceColumn('companyid', 'Company', 100, $scope.Company);
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 80);
            igrid.AddSimpleColumn('barcode', 'Barcode', 80);
            igrid.AddSimpleColumn('jobworker', 'Job Worker', 180);
            igrid.AddFormatedColumn('entrydate', 'Entry Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('invoicedate', 'Due Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('sourcesupplier', 'Supplier', 160);
            igrid.AddSimpleColumn('inproduct', 'Iss. Product', 120);
            igrid.AddSimpleColumn('outproduct', 'Rec. Product', 120);
            igrid.AddFormatedColumn('oqty', 'Issued Qty', 80, 'N0', 0).total = true;
            igrid.AddFormatedColumn('rqty', 'Received Qty', 80, 'N0', 0).total = true;
            igrid.AddFormatedColumn('pqty', 'Pending Qty', 80, 'N0', 0).total = true;
            igrid.AddFormatedColumn('materialcost', 'Material Cost', 80, 'N0', 0).total = true;
            igrid.AddFormatedColumn('materialsalevalue', 'Material Value', 80, 'N0', 0).total = true;
            igrid.AddFormatedColumn('chargetotal', 'Job Charge', 80, 'N0', 0).total = true;
            igrid.AddSimpleColumn('grnno', 'GRN No', 80);
            igrid.AddSimpleColumn('grndate', 'GRN Date', 80);

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.JobOutwardResult);
        }

        $scope.SearchNewProducts = function () {
            $scope.totalcount = 0;
            $scope.NewProductList = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchNewProducts",
                data: srequest
            }).success(function (result) {
                $scope.NewProductList = result;
                $scope.totalcount = result.length;

                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.NewProductConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchStockMovement = function () {

            if ($scope.Appsetting_Company) {
                if (typeof ($scope.SearchCondition.companyid) == 'undefined' || $scope.SearchCondition.companyid == null
                    || $scope.SearchCondition.companyid < 0) {
                    $rootScope.Toaster.Info("Please select the company");
                    return;
                }
            }

            //if (($scope.SearchCondition.locationid == null || $scope.SearchCondition.locationid == 0)
            //    && ($scope.SearchCondition.supplierid == null || $scope.SearchCondition.supplierid == 0)
            //    && ($scope.SearchCondition.lrentryno == 'undefined' || $scope.SearchCondition.lrentryno == '')
            //    && ($scope.SearchCondition.invoiceno == 'undefined' || $scope.SearchCondition.invoiceno == '')) {
            //    $rootScope.Toaster.Info("Please select Location or Supplier or GRN/Invoice no");
            //    return;
            //}
            $scope.StockMovementResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchStockMovement",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }

                $scope.StockMovementResult = result;
                $scope.StockMovementConfig();
            }).error(function (result) {
            });
        }


        $scope.SearchNewSuppliers = function () {

            $scope.SearchNewSuppliersList = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SupplierService/SearchSupplierDetail",
                data: srequest
            }).success(function (result) {
                $scope.SearchNewSuppliersList = result;
                $scope.totalcount = result.length;

                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.NewSuppliersConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchAgent = function () {

            $scope.AgentList = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "AgentService/Search",
                data: srequest
            }).success(function (result) {
                $scope.AgentList = result;
                $scope.totalcount = result.length;

                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.AgentConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchTax = function () {

            $scope.TaxList = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "TaxService/Search",
                data: srequest
            }).success(function (result) {
                $scope.TaxList = result;
                $scope.totalcount = result.length;

                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.TaxConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchOutwardDetails = function () {

            $scope.totalqty = 0;

            if ($scope.SearchCondition.showdetail == false && $scope.SearchCondition.withlr == false
                && ($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == null)
                && ($scope.SearchCondition.supplierid == 0 || $scope.SearchCondition.supplierid == null)) {
                $rootScope.Toaster.Info("Please select Location or Supplier");
                return;
            }
            else if ($scope.SearchCondition.pendinginward && ($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == null)
                && ($scope.SearchCondition.supplierid == 0 || $scope.SearchCondition.supplierid == null)) {
                $rootScope.Toaster.Info("Please select Location");
                return;
            }

            $scope.OutwardDetailsResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchOutwardCost",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                    return;
                }
                $.each(result, function (k, v) {
                    $scope.totalqty += v.aqty;
                });
                $scope.OutwardDetailsResult = result;

                $scope.OutwardDetailsConfig();
            }).error(function (result) {
            });
        }


        $scope.ManageSearchItem = function (action, item) {
            if (action == "PRODUCTVIEW") {
                $scope.SearchProduct(item.invoiceid);
            } else if (action == "BARCODEVIEW") {
                $scope.SearchBarcode(item.barcode);
            }
            else if (action == "BITUPDATE") {
                $scope.UpdateBitStock(item);
            }
        }
        $scope.UpdateBitStock = function (item) {

            request = {
                "item": { barcode: item.barcode, isbit: !item.isbit },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "StockService/UpdateBitStock",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Barcode updated as non-bit stock.");
            }).error(function (result) {
            });
        }

        $scope.SearchProduct = function (lrid) {

            request = {
                "item": { id: lrid },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrinvoiceService/Search",
                data: srequest
            }).success(function (result) {
                if (result != undefined && result.Items.length > 0) {
                    $scope.Productdetail = result;
                    $scope.printDiv('productviewprintdiv', 'PRODUCT ITEM VIEW');
                }
            }).error(function (result) {
            });

        }
        $scope.SearchBarcode = function (code) {
            $scope.Barcodedetail = "";
            request = {
                "item": { barcode: code },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchBarcode",
                data: srequest
            }).success(function (result) {
                if (result != undefined) {
                    $scope.Barcodedetail = result;
                    $scope.printDiv('stockviewprintdiv', 'BARCODE VIEW');
                }
            }).error(function (result) {
            });

        }

        $scope.SearchStockAnalysis = function (Modes) {
            if (!$scope.SearchCondition.productgroup && !$scope.SearchCondition.product && !$scope.SearchCondition.brand
                && !$scope.SearchCondition.material && !$scope.SearchCondition.pattern && !$scope.SearchCondition.style
                && !$scope.SearchCondition.size && !$scope.SearchCondition.section && !$scope.SearchCondition.sourcesupplier
                && !$scope.SearchCondition.supplier)
                $scope.SearchCondition.product = true;

            $scope.SearchCondition.mode = Modes;

            $scope.totalqty = 0;
            $scope.amount = 0;
            $scope.StockAnalysisResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            if ($scope.Appsetting_Company) {
                if (typeof ($scope.SearchCondition.companyid) == 'undefined' || $scope.SearchCondition.companyid == null
                    || $scope.SearchCondition.companyid < 0) {
                    $rootScope.Toaster.Info("Please select the company");
                    return;
                }
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchStockAnalysis",
                data: srequest
            }).success(function (result) {
                $scope.StockAnalysisResult = result;
                $.each(result, function (k, v) {
                    $scope.totalqty += v.qty;
                    $scope.amount += v.price;
                });
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.StockAnalysisConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchPReturnAudit = function () {

            $scope.PReturnAuditResult = [];
            $scope.SearchCondition.isaudit = true;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrinvoicereturnService/Search",
                data: srequest
            }).success(function (result) {
                $scope.PReturnAuditResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.PReturnAuditConfig();
            }).error(function (result) {
            });
        }
        $scope.PReturnAuditConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.EnableSerial($scope.PReturnAuditResult);

            igrid.AddReferenceColumn('companyid', 'Company', 120, $scope.Company);
            igrid.AddReferenceColumn('locationid', 'Location', 120, $scope.Location);
            igrid.AddSimpleColumn('returnno', 'Return No', 100);
            igrid.AddFormatedColumn('returndate', 'Return Date', 100, 'dd/MM/yyyy');
            igrid.AddReferenceColumn('supplierid', 'Supplier', 160, $scope.Supplier);
            igrid.AddSimpleColumn('createdusername', 'Created By', 120);
            igrid.AddFormatedColumn('createdon', 'Created On', 100, 'dd/MM/yyyy hh:mm');
            igrid.AddSimpleColumn('modifiedusername', 'Modified By', 120);
            igrid.AddFormatedColumn('modifiedon', 'Modified On', 100, 'dd/MM/yyyy hh:mm');

            igrid.AddFormatedColumn('qty', 'Qty', 60, 'N2', 2);
            igrid.AddFormatedColumn('total', 'Total', 80, 'N2', 2);

            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.PReturnAuditResult);
        }

        $scope.SearchAsOnStockOLD = function () {

            $scope.SearchCondition.product = false;
            $scope.SearchCondition.supplier = false;
            $scope.SearchCondition.section = false;

            if ($scope.SearchCondition.mode == "1")
                $scope.SearchCondition.product = true;
            else if ($scope.SearchCondition.mode == "2")
                $scope.SearchCondition.supplier = true;
            else if ($scope.SearchCondition.mode == "3")
                $scope.SearchCondition.section = true;
            else
                $scope.SearchCondition.product = true;

            $scope.AsOnStockResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchAsOnStock",
                data: srequest
            }).success(function (result) {
                $scope.AsOnStockResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.AsOnStockConfig();
            }).error(function (result) {
            });
        }

        $scope.AddCondition = function (key, value, reflist) {
            var dispvalue = value;
            if (reflist && reflist) {
                var refitem = reflist.find(function (item) {
                    return item.id == value;
                });
                if (refitem && refitem != null)
                    dispvalue = refitem.value;
            }
            if ($scope.SearchConditionStkTrans.conditions == undefined ||
                $scope.SearchConditionStkTrans.conditions == null)
                $scope.SearchConditionStkTrans.conditions = [];


            if ($scope.SearchConditionStkTrans.conditiontype == $scope.Options.Barcode)
                $scope.SearchConditionStkTrans.HasBarcodeFilter = true;

            $scope.SearchConditionStkTrans.conditions.push({ 'option': $scope.SearchConditionStkTrans.conditiontype, 'key':key, 'value': value, 'display': value });
        }

        $scope.RemoveFromArray = function (item, list) {
            if (item.option == $scope.Options.Barcode)
                $scope.SearchConditionStkTrans.HasBarcodeFilter = false;
            list.splice(list.indexOf(item), 1);
        }

        $scope.SearchSalesVsPurchaseVsStock = function (barcodehistory) {

            $scope.SearchConditionStkTrans.fields = [];
            $.each($scope.AnalyserOptions, function (k, v) {
                if (v.selected)
                    $scope.SearchConditionStkTrans.fields.push(v.id);
            });
            $scope.StockTransactionResult = [];
            $scope.SearchConditionStkTrans.history = barcodehistory;
            request = {
                "item": $scope.SearchConditionStkTrans,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "AnalyserService/SaleVsPurchaseVsStock",
                data: srequest
            }).success(function (result) {


                $scope.StockTransactionResult = result.items;
                if ($scope.igrid != null)
                    $scope.igrid.destroy();

                var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
                igrid.options.showHeaderRow = true;
                igrid.EnableSerial($scope.StockTransactionResult);

                igrid.AddFormatedColumn('opening', 'Opening', 80, 'N', 2).total = true;
                igrid.AddFormatedColumn('openingcost', 'Op.Taxable', 80, 'N', 2).total = true;
                igrid.AddFormatedColumn('openingicost', 'Op.Net', 80, 'N', 2).total = true;
                igrid.AddFormatedColumn('PU', 'Purchase', 80, 'N', 2).total = true;
                igrid.AddFormatedColumn('MS', 'Direct Stock', 80, 'N', 2).total = true;
                igrid.AddFormatedColumn('PR', 'P.Return', 80, 'N', 2).total = true;
                igrid.AddFormatedColumn('TI', 'Transer In', 80, 'N', 2).total = true;
                igrid.AddFormatedColumn('TO', 'Transfer Out', 80, 'N', 2).total = true;
                igrid.AddFormatedColumn('SL', 'Sales', 80, 'N', 2).total = true;
                igrid.AddFormatedColumn('AU', 'Audit', 80, 'N', 2).total = true;
                igrid.AddFormatedColumn('closing', 'Clsoing', 80, 'N', 2).total = true;
                igrid.AddFormatedColumn('closingcost', 'Cl.Taxable', 80, 'N', 2).total = true;
                igrid.AddFormatedColumn('closingicost', 'Cl.Net', 80, 'N', 2).total = true;

                $.each(result.display, function (k, v) {
                    var option = $scope.AnalyserOptions.find(function (item) {
                        return item.id == v;
                    });

                    var column = igrid.AddCustomColumn('col_' + k,
                        option.name, 80, function (item, col) {
                            return item.values[col.internal];
                        });
                    column.internal = k;
                });

                igrid.EnableTotal();
                igrid.Initialize();
                $scope.igrid = igrid;
                $scope.igrid.UpdateData($scope.StockTransactionResult);


            }).error(function (result) {
            });
        }

        $scope.SearchAsOnStockTransaction = function (barcodehistory) {

            $scope.SearchConditionStkTrans.fields = [];
            $.each($scope.AnalyserOptions, function (k, v) {
                if (v.selected)
                    $scope.SearchConditionStkTrans.fields.push(v.id);
            });
            $scope.StockTransactionResult = [];
            $scope.SearchConditionStkTrans.history = barcodehistory;
            request = {
                "item": $scope.SearchConditionStkTrans,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "AnalyserService/SearchAsOnStock",
                data: srequest
            }).success(function (result) {

                if (barcodehistory) {
                    $scope.StockTransactionResult = result;
                    if ($scope.igrid != null)
                        $scope.igrid.destroy();

                    var modes = [{ 'id': 'OP', 'name': 'Opening' },
                        { 'id': 'PU', 'name': 'Purchase' },
                        { 'id': 'PR', 'name': 'PurchaseReturn' },
                        { 'id': 'TI', 'name': 'TransferIn' },
                        { 'id': 'TO', 'name': 'TransferOut' },
                        { 'id': 'SL', 'name': 'Sales' },
                        { 'id': 'AU', 'name': 'Audit' },
                        { 'id': 'MS', 'name': 'Opening' },
                        { 'id': 'JN', 'name': 'Journal' },
                        { 'id': 'DM', 'name': 'Damage' }
                    ]

                    var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
                    igrid.options.showHeaderRow = true;
                    igrid.EnableSerial($scope.StockTransactionResult);
                    igrid.AddFormatedColumn('ason', 'Date', 100, 'dd-MM-yyyy');
                    igrid.AddSimpleColumn('username', 'User', 80);
                    igrid.AddSimpleColumn('mode', 'Transaction', 80);

                    var column = igrid.AddFormatedColumn('ob', 'Opening', 80, 'N', 2);
                    column.cssClass = 'rightalign';

                    column = igrid.AddFormatedColumn('ti', 'Stock In', 80, 'N', 2);
                    column.cssClass = 'rightalign';
                    column.total = true;

                    column = igrid.AddFormatedColumn('to', 'Stock Out', 80, 'N', 2);
                    column.cssClass = 'rightalign';
                    column.total = true;

                    column = igrid.AddFormatedColumn('cb', 'Clsoing', 80, 'N', 2);
                    column.cssClass = 'rightalign';

                    igrid.EnableTotal();
                    igrid.Initialize();
                    $scope.igrid = igrid;
                    $scope.igrid.UpdateData($scope.StockTransactionResult);
                }
                else {
                    $scope.StockTransactionResult = result.items;
                    if ($scope.igrid != null)
                        $scope.igrid.destroy();

                    var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
                    igrid.options.showHeaderRow = true;
                    igrid.EnableSerial($scope.StockTransactionResult);

                    igrid.AddFormatedColumn('opening', 'Opening', 80, 'N', 2).total = true;
                    igrid.AddFormatedColumn('openingcost', 'Op.Taxable', 80, 'N', 2).total = true;
                    igrid.AddFormatedColumn('openingicost', 'Op.Net', 80, 'N', 2).total = true;
                    igrid.AddFormatedColumn('PU', 'Purchase', 80, 'N', 2).total = true;
                    igrid.AddFormatedColumn('MS', 'Direct Stock', 80, 'N', 2).total = true;
                    igrid.AddFormatedColumn('PR', 'P.Return', 80, 'N', 2).total = true;
                    igrid.AddFormatedColumn('TI', 'Transer In', 80, 'N', 2).total = true;
                    igrid.AddFormatedColumn('TO', 'Transfer Out', 80, 'N', 2).total = true;
                    igrid.AddFormatedColumn('SL', 'Sales', 80, 'N', 2).total = true;
                    igrid.AddFormatedColumn('AU', 'Audit', 80, 'N', 2).total = true;
                    igrid.AddFormatedColumn('closing', 'Clsoing', 80, 'N', 2).total = true;
                    igrid.AddFormatedColumn('closingcost', 'Cl.Taxable', 80, 'N', 2).total = true;
                    igrid.AddFormatedColumn('closingicost', 'Cl.Net', 80, 'N', 2).total = true;

                    $.each(result.display, function (k, v) {
                        var option = $scope.AnalyserOptions.find(function (item) {
                            return item.id == v;
                        });

                        var column = igrid.AddCustomColumn('col_' + k,
                            option.name, 80, function (item, col) {
                                return item.values[col.internal];
                            });
                        column.internal = k;
                    });

                    igrid.EnableTotal();
                    igrid.Initialize();
                    $scope.igrid = igrid;
                    $scope.igrid.UpdateData($scope.StockTransactionResult);
                }

            }).error(function (result) {
            });
        }

        $scope.SearchAsOnStock = function () {

            $scope.StockTransactionResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchAsOnStock",
                data: srequest
            }).success(function (result) {
                $scope.StockTransactionResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.StockTransactionConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchSupplierPaymentReport = function () {
            $scope.SupplierPayResult = [];
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SupplierpaymentService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SupplierPayResult = result;
                $scope.SupplierPaymentReportConfig();
            }).error(function (result) {
            });

        }

        $scope.LoadReportType = function () {

            $.each($scope.AllReportType, function (k, v) {
                if (v.code == "LRREPORT" && $scope.Permission.p_LRREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "LRINVOICE" && $scope.Permission.p_LRINVOICEREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "PURCHASE" && $scope.Permission.p_PURCHASEREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "PURCHASEHSN" && $scope.Permission.p_PURCHASEHSNREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "PURCHASERETURN" && $scope.Permission.p_PURCHASERETURNREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "PURCHASERETURNCANCEL" && $scope.Permission.p_PURCHASERETURNCANCELREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "OUTWARD" && $scope.Permission.p_OUTWARDREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "OUTWARDPENDING" && $scope.Permission.p_OUTWARDPENDINGREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "STOCK" && $scope.Permission.p_WHSTOCKREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "STOCKANALYSIS" && $scope.Permission.p_WHSTOCKANALYSISREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "PRODUCTCREATE" && $scope.Permission.p_WHSTOCKANALYSISREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "SUPPCREATE" && $scope.Permission.p_WHSTOCKREPORT.view) {
                    $scope.ReportType.push(v);
                }
            });
        }

        $scope.printInfyGrid = function (grid) {
            var strWindowFeatures = "menubar=yes,location=yes,resizable=yes,scrollbars=yes,status=yes";
            grid.grid.printPlugin.printToWindow(window.open('/print-grid.html', 'print_window', strWindowFeatures));
        }

        $scope.printDiv = function (div, title) {

            $timeout(function () {

                var otitle = angular.element('#apptitle').context.title;
                var titlele = angular.element('#apptitle').context.title = title;

                var docHead = document.head.outerHTML;
                var printContents = document.getElementById(div).outerHTML;
                var winAttr = "location=yes, statusbar=no, menubar=no, titlebar=no, toolbar=no,dependent=no, width=865, height=600, resizable=yes, screenX=200, screenY=200, personalbar=no, scrollbars=yes";

                var newWin = window.open("", "_blank", winAttr);
                var writeDoc = newWin.document;
                writeDoc.open();
                writeDoc.write('<!doctype html><html>' + docHead + '<body onLoad="window.print()">' + printContents + '</body></html>');
                writeDoc.close();
                newWin.focus();

                angular.element('#apptitle').context.title = otitle;
            }, 100);

        }

        $scope.exportExcel = function (tableid, title, removeactioncolumn) {
            if (tableid == "lrentryreport")
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'LR ENTRY REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'lrinvoicereport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'INVOICE ENTRY REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'purchaseentry')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'PURCHASE ENTRY REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'purchasehsn')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'PURCHASE HSN REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'purchaseretsum')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'PURCHASE RETURN SUMMARY REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'purchaseretsumcancel')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'PURCHASE RETURN CANCEL REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'inwardreport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'OUTWARD REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'outward')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'PENDING OUTWARD REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));

            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = title + ".xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }

        $scope.LRConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.EnableSerial($scope.LRResult);
            igrid.AddReferenceColumn('companyid', 'Company', 120, $scope.Company);
            igrid.AddSimpleColumn('supplier', 'Supplier', 200);
            igrid.AddReferenceColumn('suppliercompanycityid', 'Supplier City', 120, $scope.city);
            igrid.AddReferenceColumn('entrylocationid', 'Entry Location', 120, $scope.AllLocation);
            igrid.AddReferenceColumn('transportid', 'Transport', 150, $scope.Transports);
            igrid.AddSimpleColumn('transportgst', 'Transport GST', 100);
            igrid.AddReferenceColumn('agentid', 'Agent', 150, $scope.Agent);
            igrid.AddSimpleColumn('purchasername', 'Purchase Manager', 120);
            igrid.AddSimpleColumn('slipno', 'Slip No', 80);
            igrid.AddReferenceColumn('bundlerackid', 'Rack ID', 50, $scope.BundleRacks);
            igrid.AddSimpleColumn('lrsectionname', 'LR Section', 80);
            igrid.AddSimpleColumn('lrentryno', 'LR Entry No', 80);
            igrid.AddFormatedColumn('entrydate', 'Entry Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('duedate', 'Due Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('duedays', 'Days', 50, 'N2', 0);
            igrid.AddSimpleColumn('lrno', 'LR No', 100);
            igrid.AddFormatedColumn('lrdate', 'LR Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 100);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 80, 'dd-MM-yyyy');


            if ($scope.SearchCondition.showdetail) {

                igrid.AddCustomColumn('invoicestart', 'Invoice Start', 100, function (item, col) {
                    if (item.trdates == undefined || item.trdates == null)
                        return "";
                    return DateFormatter(item.trdates.invoicestart, 'dd-MM-yyyy');
                });

                igrid.AddCustomColumn('invoiceend', 'Invoice End', 100, function (item, col) {
                    if (item.trdates == undefined || item.trdates == null)
                        return "";
                    return DateFormatter(item.trdates.invoiceend, 'dd-MM-yyyy');
                });

                if ($rootScope.UserInfo.appsettings.system_supportpricingflow) {
                    igrid.AddCustomColumn('pricingstart', 'Price Start', 100, function (item, col) {
                        if (item.trdates == undefined || item.trdates == null)
                            return "";
                        return DateFormatter(item.trdates.pricingstart, 'dd-MM-yyyy');
                    });

                    igrid.AddCustomColumn('pricingend', 'Price End', 100, function (item, col) {
                        if (item.trdates == undefined || item.trdates == null)
                            return "";
                        return DateFormatter(item.trdates.pricingend, 'dd-MM-yyyy');
                    });
                }

                igrid.AddCustomColumn('productstartdate', 'Product Start', 100, function (item, col) {
                    if (item.trdates == undefined || item.trdates == null)
                        return "";
                    return DateFormatter(item.trdates.productstartdate, 'dd-MM-yyyy');
                });

                igrid.AddCustomColumn('productenddate', 'Product End', 100, function (item, col) {
                    if (item.trdates == undefined || item.trdates == null)
                        return "";
                    return DateFormatter(item.trdates.productenddate, 'dd-MM-yyyy');
                });

                igrid.AddCustomColumn('barcodestart', 'Barcode Start', 100, function (item, col) {
                    if (item.trdates == undefined || item.trdates == null)
                        return "";
                    return DateFormatter(item.trdates.barcodestart, 'dd-MM-yyyy');
                });

                igrid.AddCustomColumn('barcodeend', 'Barcode End', 100, function (item, col) {
                    if (item.trdates == undefined || item.trdates == null)
                        return "";
                    return DateFormatter(item.trdates.barcodeend, 'dd-MM-yyyy');
                });
            }


            igrid.AddFormatedColumn('totalamount', 'Total Value', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('noofbundels', 'No Of Bundles', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('noofpieces', 'No Of Pieces', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('noofboxes', 'No Of Boxes', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('freightcharge', 'Freight', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('loadingcharge', 'Loading', 80, 'N2', 2).total = true;
            igrid.AddReferenceColumn('modeofdelivery', 'Delivery Mode', 100, $scope.ModeofDelivery);
            igrid.AddSimpleColumn('statusname', 'Status', 100);

            igrid.options.autoEdit = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.LRResult);
        }

        function DateFormatter(value, format) {
            var result = jQuery.format.date(value, format);
            if (result == '0001-01-01' || result == '01-01-0001') return '';
            return result;
        }

        $scope.InvoiceConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.EnableSerial($scope.LRInvoiceResult);
            igrid.AddSimpleColumn('supplier', 'Supplier', 200);
            igrid.AddSimpleColumn('gstno', 'Supplier GST', 100);

            if (!$scope.SearchCondition.groupsupplier) {
                igrid.AddReferenceColumn('cityid', 'City', 150, $scope.city);
                igrid.AddSimpleColumn('purchaser', 'Purchaser', 120);
                if ($scope.SearchCondition.showdetail) {
                    igrid.AddSimpleColumn('bankname', 'Bank', 150);
                    igrid.AddSimpleColumn('accountno', 'Account No', 120);
                    igrid.AddSimpleColumn('ifsccode', 'IFSC CODE', 100);
                }
                igrid.AddCustomColumn('isinterstatesale', 'IGST', 60, function (item, col) {
                    return item.isinterstatesale == true ? "YES" : "NO";
                });
                igrid.AddReferenceColumn('entrylocationid', 'Entry Location', 150, $scope.Location);
                igrid.AddSimpleColumn('lrentryno', 'GRN No', 120);
                igrid.AddFormatedColumn('lrentrydate', 'GRN Entry Date', 100, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('invoiceno', 'Invoice No', 100);
                igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 100, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('duedate', 'Due Date', 100, 'dd-MM-yyyy');
            }
            else {
                if ($scope.SearchCondition.showdetail) {
                    igrid.AddReferenceColumn('cityid', 'City', 150, $scope.city);
                    igrid.AddSimpleColumn('bankname', 'Bank', 150);
                    igrid.AddSimpleColumn('accountno', 'Account No', 120);
                    igrid.AddSimpleColumn('ifsccode', 'IFSC CODE', 100);
                }
                igrid.AddFormatedColumn('percentage', 'GRN', 80, 'N0', 0).total = true;
            }

            igrid.AddFormatedColumn('marginpercentage', 'Margin', 60, 'N2', 2);

            var hasdealer = $scope.LRInvoiceResult.find(function (item) {
                return item.dealerprice > 0;
            });

            if (hasdealer && hasdealer != null) {
                igrid.AddFormatedColumn('dealermarginpercentage', 'D.Margin', 80, 'N2', 2);
            }
            
            igrid.AddFormatedColumn('qty', 'Inv Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('invoiceamount', 'Inv Value', 120, 'N2', 2).total = true;

            if ($scope.SearchCondition.showbundle) {
                igrid.AddFormatedColumn('noofbundle', 'B.Total', 80, 'N2', 2).total = true;
                var column = igrid.AddCustomColumn('opened', 'B.Opened', 60, function (item, col) {
                    return item.qty > 0 ? item.noofbundle : 0;
                });
                column.cssClass = 'rightalign';
                column.total = true;
                var column = igrid.AddCustomColumn('pending', 'B.Pending', 60, function (item, col) {
                    return item.qty == 0 ? item.noofbundle : 0;
                });
                column.cssClass = 'rightalign';
                column.total = true;
            }
            else {
                igrid.AddFormatedColumn('returnqty', 'Return Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('returnamount', 'Return Value', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('returnfright', 'Return Fright', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('netamount', 'Net Value', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('stocksalevalue', 'Sale Value', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('mrp', 'MRP Value', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('igst', 'Sale Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('closingqty', 'Closing Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('closingstockvalue', 'Closing St.Value', 100, 'N2', 2).total = true;
                if (!$scope.SearchCondition.groupsupplier) {
                    igrid.AddFormatedColumn('stockage', 'Age', 40, 'N0', 0);
                    igrid.AddFormatedColumn('soldpercentage', 'Sold%', 60, 'N2', 2);
                }
            }
            igrid.AddButtonColumn('action', 'Action', 60, 'View', function (e, args) {
                $scope.$apply(function () {
                    $scope.ManageSearchItem('PRODUCTVIEW', args.item);
                });
                return true;
            });

            igrid.options.autoEdit = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.LRInvoiceResult);
        }

        $scope.SupplierPaymentConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SupplierPaymentResult);

            igrid.AddSimpleColumn('supplier', 'Supplier', 200);
            igrid.AddReferenceColumn('cityid', 'City', 100, $scope.city);
            igrid.AddSimpleColumn('gstno', 'GSTNO', 120);
            igrid.AddSimpleColumn('msmeno', 'MSMENO', 100);

            if ($scope.SearchCondition.groupsupplier) {
                igrid.AddSimpleColumn('invoicecount', 'Invoice Count', 120);
                igrid.AddFormatedColumn('totalamount', 'Invoice Amount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('debetnoteamount', 'Debit Note Amount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('paidamount', 'Paid Amount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('balanceamount', 'Balance Amount', 100, 'N2', 2).total = true;
            }
            else {
                igrid.AddSimpleColumn('agentname', 'Agent', 140);
                igrid.AddSimpleColumn('lrentryno', 'Entry No', 140);
                igrid.AddFormatedColumn('invoiceentrydate', 'Entry Date', 80, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('invoiceno', 'Invoice No', 120);
                igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 80, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('duedays', 'Due In Days', 60, 'N2', 0);
                igrid.AddFormatedColumn('totalamount', 'Invoice Amount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('paidamount', 'Paid Amount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('balanceamount', 'Balance Amount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('soldpercent', 'Sold %', 60, 'N2', 2);
            }

            if ($scope.SearchCondition.showdetail) {
                igrid.AddSimpleColumn('bankname', 'Bank Name', 120);
                igrid.AddSimpleColumn('bankbranch', 'Branch Name', 120);
                igrid.AddSimpleColumn('bankifsccode', 'IFSC', 120);
                igrid.AddSimpleColumn('bankacno', 'Acc No', 120);
                igrid.AddSimpleColumn('bankacname', 'Acc Name', 120);
            }

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SupplierPaymentResult);
        }

        $scope.SupplierPaymentReportConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SupplierPayResult);

            igrid.AddReferenceColumn('supplierid', 'Supplier', 180, $scope.Supplier);
            igrid.AddSimpleColumn('supplierbankname', 'Sup.Bank Name', 100);
            igrid.AddSimpleColumn('supplierbankbranch', 'Branch Name', 80);
            igrid.AddSimpleColumn('supplierbankacname', 'Sup.Account Name', 100);
            igrid.AddSimpleColumn('supplierbankacno', 'Sup.Account No', 100);
            igrid.AddSimpleColumn('supplierbankifsccode', 'IFSC Code', 60);
            igrid.AddFormatedColumn('paymentdate', 'Payment Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('receiptno', 'Receipt No', 80);
            igrid.AddReferenceColumn('paymentmode', 'Payment Mode', 120, $scope.PaymentModes);
            igrid.AddSimpleColumn('referencedetail', 'Ref. Nos', 100);
            igrid.AddFormatedColumn('invoicetotal', 'Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('debitnotetotal', 'Cr.Amount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('invoicediscount', 'Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paidamount', 'Paid Amount', 100, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SupplierPayResult);
        }




        $scope.TransportPaymentConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SupplierPaymentResult);

            if ($scope.SearchCondition.groupbytransport) {
                igrid.AddSimpleColumn('transport', 'Transport', 200);
                igrid.AddSimpleColumn('invoicecount', 'Invoice Count', 120);
                igrid.AddFormatedColumn('totalamount', 'Invoice Amount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('paidamount', 'Paid Amount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('balanceamount', 'Balance Amount', 100, 'N2', 2).total = true;
            }
            else {
                igrid.AddSimpleColumn('transportname', 'Transport', 200);
                igrid.AddSimpleColumn('supplier', 'Supplier', 200);
                igrid.AddSimpleColumn('fromlocation', 'Location', 200);
                igrid.AddSimpleColumn('lrentryno', 'Entry No', 120);
                igrid.AddSimpleColumn('invoiceno', 'Invoice No', 120);
                igrid.AddSimpleColumn('lrno', 'LR No', 120);
                igrid.AddFormatedColumn('lrdate', 'LR Date', 80, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('totalamount', 'Invoice Amount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('paidamount', 'Paid Amount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('balanceamount', 'Balance Amount', 100, 'N2', 2).total = true;
            }

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SupplierPaymentResult);
        }

        $scope.TransportPaymentReportConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SupplierPayResult);

            igrid.AddReferenceColumn('transportid', 'Transport', 180, $scope.Transports);
            igrid.AddSimpleColumn('bankname', 'Bank Name', 100);
            igrid.AddSimpleColumn('bankacno', 'Account No', 100);
            igrid.AddSimpleColumn('bankifsccode', 'IFSC Code', 80);
            igrid.AddFormatedColumn('paymentdate', 'Payment Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('receiptno', 'Receipt No', 80);
            igrid.AddReferenceColumn('paymentmode', 'Payment Mode', 120, $scope.PaymentModes);
            igrid.AddSimpleColumn('referencedetail', 'Ref. Nos', 100);
            igrid.AddFormatedColumn('invoicetotal', 'Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('invoicediscount', 'Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paidamount', 'Paid Amount', 100, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SupplierPayResult);
        }

        $scope.PurchaseSectionwiseConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SectionwisePurchaseResult);

            igrid.AddReferenceColumn('sectionid', 'SECTION', 160, $scope.Division);
            if ($scope.SearchCondition.showdetail)
                igrid.AddSimpleColumn('productname', 'PRODUCT', 200);
            igrid.AddFormatedColumn('qty', 'QTY', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('basebuyingprice', 'GROSS VALUE', 120, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'DISCOUNT', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxableamount', 'TAXABLE', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxamount', 'TAX', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('netvalue', 'NET VALUE', 100, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SectionwisePurchaseResult);
        }

        $scope.PurchaseRetSectionwiseConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SectionwisePurchaseRetResult);

            igrid.AddReferenceColumn('sectionid', 'SECTION', 160, $scope.Division);
            if ($scope.SearchCondition.showdetail)
                igrid.AddSimpleColumn('productname', 'PRODUCT', 200);
            igrid.AddFormatedColumn('qty', 'QTY', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('invoiceamount', 'GROSS VALUE', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'DISCOUNT', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxamount', 'TAX', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('netvalue', 'NET VALUE', 100, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SectionwisePurchaseRetResult);
        }

        $scope.PurchaseConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.PurchaseEntryResult);

            igrid.AddSimpleColumn('supplier', 'Supplier', 200);
            igrid.AddReferenceColumn('cityid', 'City', 120, $scope.city);
            igrid.AddReferenceColumn('agentid', 'Agent', 120, $scope.Agent);
            igrid.AddSimpleColumn('gstno', 'GST NO', 100);
            igrid.AddSimpleColumn('msmeno', 'MSME NO', 80);
            igrid.AddCustomColumn('isinterstatesale', 'IGST', 60, function (item, col) {
                return item.isinterstatesale == true ? "YES" : "NO";
            });
            igrid.AddReferenceColumn('entrylocationid', 'Entry Location', 150, $scope.Location);
            igrid.AddReferenceColumn('receivinglocationid', 'Receiving City', 150, $scope.city);
            igrid.AddSimpleColumn('lrentryno', 'GRN No', 100);
            igrid.AddSimpleColumn('lrno', 'LR No', 80);
            igrid.AddFormatedColumn('lrentrydate', 'GRN Entry Date', 100, 'dd-MM-yyyy');

            if ($scope.SearchCondition.showbarcodedon) {
                igrid.AddFormatedColumn('barcodedon', 'Barcoded On', 100, 'dd-MM-yyyy');
            }

            igrid.AddFormatedColumn('duedate', 'Due date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 120);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('qty', 'Inv.Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('invoiceamount', 'Invoice Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('invoicetotalamount', 'Invoice Total', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('otheramount', 'Rounding', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('forwardcharge', 'Other Charges', 100, 'N2', 2).total = true;

            $.each($scope.TaxHeader, function (k, v) {

                var column = igrid.AddCustomColumn('tax_col_' + k, v.displaytext, 80, function (item, col) {

                    var taxtype = col.internal;
                    var ditem = item.taxbreakup.find(function (searchitem) {
                        return searchitem.taxpercent == taxtype.percentage;
                    });

                    if (typeof (ditem) == 'undefinied' || ditem == null)
                        return null;

                    if (taxtype.type == 1)
                        return ditem.taxableamount.toFixed(2);
                    else if (taxtype.type == 2)
                        return ditem.taxamount.toFixed(2);
                    else
                        return ditem.qty
                });

                column.getValue = function (item, col) {

                    var taxtype = col.internal;
                    var ditem = item.taxbreakup.find(function (searchitem) {
                        return searchitem.taxpercent == taxtype.percentage;
                    });

                    if (typeof (ditem) == 'undefinied' || ditem == null)
                        return 0;

                    if (taxtype.type == 1)
                        return ditem.taxableamount.toFixed(2);
                    else if (taxtype.type == 2)
                        return ditem.taxamount.toFixed(2);
                    else
                        return ditem.qty

                }
                column.cssClass = 'rightalign';
                column.internal = v;
                column.total = true;

            });
            igrid.AddFormatedColumn('taxableamount', 'Tot Taxable.Amt', 120, 'N2', 2).total = true;
            igrid.AddFormatedColumn('tdspercentage', 'TDS %', 50, 'N2', 2).total = true;
            igrid.AddFormatedColumn('tdsamount', 'TDS', 50, 'N2', 2).total = true;
            igrid.AddFormatedColumn('tcspercentage', 'TCS %', 50, 'N2', 2).total = true;
            igrid.AddFormatedColumn('tcsamount', 'TCS', 50, 'N2', 2).total = true;

            var column = igrid.AddCustomColumn('tax_SGST', 'SGST', 80, function (item, col) {
                if (item.isinterstatesale)
                    return '';
                return $filter('IndianNumber')(item.taxamount/2, 2);
            });
            column.getValue = function (item, col) {
                if (item.isinterstatesale)
                    return 0;

                return item.taxamount/2;
            }
            column.cssClass = 'rightalign';
            column.total = true;

            var column = igrid.AddCustomColumn('tax_CGST', 'CGST', 80, function (item, col) {
                if (item.isinterstatesale)
                    return '';
                return $filter('IndianNumber')(item.taxamount / 2, 2);
            });
            column.getValue = function (item, col) {
                if (item.isinterstatesale)
                    return 0;

                return item.taxamount / 2;
            }
            column.cssClass = 'rightalign';
            column.total = true;

            var column = igrid.AddCustomColumn('tax_IGST', 'IGST', 80, function (item, col) {
                if (!item.isinterstatesale)
                    return '';
                return $filter('IndianNumber')(item.taxamount, 2);
            });
            column.getValue = function (item, col) {
                if (!item.isinterstatesale)
                    return 0;

                return item.taxamount;
            }
            column.cssClass = 'rightalign';
            column.total = true;
            igrid.AddFormatedColumn('taxamount', 'Tot Tax.Amt', 100, 'N2', 2).total = true;


            igrid.options.autoEdit = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.PurchaseEntryResult);
        }

        $scope.PurchaseListConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.PurchaseListResult);

            igrid.AddSimpleColumn('supplier', 'Supplier', 200);
            igrid.AddReferenceColumn('cityid', 'City', 100, $scope.city);
            igrid.AddReferenceColumn('agentid', 'Agent', 120, $scope.Agent);
            igrid.AddSimpleColumn('gstno', 'GST NO', 100);
            igrid.AddSimpleColumn('msmeno', 'MSME NO', 80);
            igrid.AddCustomColumn('isinterstatesale', 'IGST', 100, function (item, col) {
                return item.isinterstatesale == true ? "YES" : "NO";
            });
            igrid.AddReferenceColumn('entrylocationid', 'Entry Location', 150, $scope.Location);
            igrid.AddReferenceColumn('receivinglocationid', 'Receiving City', 150, $scope.city);
            igrid.AddSimpleColumn('lrentryno', 'GRN No', 100);
            igrid.AddSimpleColumn('lrno', 'LR No', 80);
            igrid.AddFormatedColumn('lrentrydate', 'GRN Entry Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 120);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 100, 'dd-MM-yyyy');
            //igrid.AddFormatedColumn('invoiceamount', 'Invoice Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxableamount', 'Taxable', 120, 'N2', 2).total = true;
            igrid.AddFormatedColumn('percentage', 'Tax %', 60, 'N2', 2);
            igrid.AddFormatedColumn('cgstpercentage', 'CGST %', 60, 'N2', 2);
            igrid.AddFormatedColumn('sgstpercentage', 'SGST %', 60, 'N2', 2);
            igrid.AddFormatedColumn('igstpercentage', 'IGST %', 60, 'N2', 2);
            igrid.AddFormatedColumn('cgst', 'CGST', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('sgst', 'SGST', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('igst', 'IGST', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('tdspercentage', 'TDS %', 50, 'N2', 2).total = true;
            igrid.AddFormatedColumn('tdsamount', 'TDS', 50, 'N2', 2).total = true;
            igrid.AddFormatedColumn('tcspercentage', 'TCS %', 50, 'N2', 2).total = true;
            igrid.AddFormatedColumn('tcsamount', 'TCS', 50, 'N2', 2).total = true;
            igrid.AddFormatedColumn('otheramount', 'Rounding', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('invoiceamount', 'Invoice Amt', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('invoicetotalamount', 'Invoice Total', 100, 'N2', 2).total = true;

            igrid.options.autoEdit = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.PurchaseListResult);
        }

        $scope.WHEntryConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.LRResult);

            igrid.AddSimpleColumn('company', 'Entry Type', 100);
            igrid.AddSimpleColumn('supplier', 'Supplier', 200);
            igrid.AddReferenceColumn('cityid', 'City', 100, $scope.city);
            igrid.AddSimpleColumn('gstno', 'GST NO', 100);
            igrid.AddCustomColumn('isinterstatesale', 'IGST', 100, function (item, col) {
                return item.isinterstatesale == true ? "YES" : "NO";
            });
            igrid.AddReferenceColumn('retaillocationid', 'Location', 150, $scope.Location);
            igrid.AddSimpleColumn('lrentryno', 'GRN No', 100);
            igrid.AddFormatedColumn('lrentrydate', 'GRN Entry Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 120);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('qty', 'Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('invoiceamount', 'Invoice Value', 120, 'N2', 2).total = true;
            igrid.AddFormatedColumn('percentage', 'Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxamount', 'Tax', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('otheramount', 'Rounding', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('invoicetotalamount', 'Invoice Total', 80, 'N2', 2).total = true;

            //igrid.AddFormatedColumn('entryqty', 'Entry Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('entrytaxable', 'Entry Taxable', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('entrytax', 'Entry Tax', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('entrytotalamount', 'Entry Total', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('difference', 'Difference', 60, 'N2', 2).total = true;

            igrid.options.autoEdit = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.LRResult);
        }

        $scope.PurchaseHSNConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.PurchaseHSNResult);

            igrid.AddSimpleColumn('supplier', 'Supplier', 200);
            igrid.AddReferenceColumn('cityid', 'City', 160, $scope.city);
            igrid.AddCustomColumn('isinterstatesale', 'IGST', 100, function (item, col) {
                return item.isinterstatesale == true ? "YES" : "NO";
            });
            igrid.AddSimpleColumn('hsncode', 'HSN Code', 100);
            igrid.AddSimpleColumn('productname', 'Item Name', 150);
            igrid.AddSimpleColumn('measure', 'Measure', 80);

            $.each($scope.TaxHeaderHSN, function (k, v) {

                var column = igrid.AddCustomColumn('tax_col_' + k, v.displaytext, 80, function (item, col) {

                    var taxtype = col.internal;
                    var ditem = item.taxbreakup.find(function (searchitem) {
                        return searchitem.taxpercent == taxtype.percentage;
                    });

                    if (typeof (ditem) == 'undefinied' || ditem == null)
                        return null;

                    if (taxtype.type == 1)
                        return ditem.taxableamount.round($scope.rounding);
                    else if (taxtype.type == 2)
                        return ditem.taxamount.round($scope.rounding);
                    else
                        return ditem.qty.round($scope.rounding);
                });

                column.internal = v;
                column.cssClass = 'rightalign';
                column.total = true;

                column.getValue = function (item, col) {

                    var taxtype = col.internal;
                    var ditem = item.taxbreakup.find(function (searchitem) {
                        return searchitem.taxpercent == taxtype.percentage;
                    });

                    if (typeof (ditem) == 'undefinied' || ditem == null)
                        return 0;

                    if (taxtype.type == 1)
                        return ditem.taxableamount.round($scope.rounding);
                    else if (taxtype.type == 2)
                        return ditem.taxamount.round($scope.rounding);
                    else
                        return ditem.qty.round($scope.rounding);

                }

            });

            igrid.options.autoEdit = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.PurchaseHSNResult);
        }

        $scope.PurchseRetConfig = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.POReturnResult);

            igrid.AddReferenceColumn('locationid', 'Location', 140, $scope.Location);
            igrid.AddSimpleColumn('supplier', 'Supplier', 250, 'N2', 2);
            igrid.AddReferenceColumn('cityid', 'City', 120, $scope.city);
            igrid.AddSimpleColumn('gstno', 'GST NO', 120);
            igrid.AddCustomColumn('isinterstatesale', 'IGST', 60, function (item, col) {
                return item.isinterstatesale == true ? "YES" : "NO";
            });
            igrid.AddSimpleColumn('returnno', 'Debit Note No', 100);
            igrid.AddFormatedColumn('returndate', 'Debit Note Date', 120, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 100);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 120, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('qty', 'Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('returnamount', 'Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('salevalue', 'Sale Value', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxableamount', 'Taxable', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxamount', 'Tax', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('freightamt', 'Fright', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('freighttaxamt', 'Fright Tax', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('packingcharge', 'Packing', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('otheramount', 'Rounding', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('netamount', 'Net Amount', 100, 'N2', 2).total = true;

            var column = igrid.AddCustomColumn('sgst', 'SGST', 50, function (item, col) {
                if (item.isinterstatesale) return '';
                return $filter('IndianNumber')(item.taxamount / 2, 2);
            });
            column.getValue = function (item, col) {
                if (item.isinterstatesale) return 0;
                return item.taxamount / 2;
            }
            column.total = true;
            column.cssClass = 'rightalign';
            var column = igrid.AddCustomColumn('cgst', 'CGST', 50, function (item, col) {
                if (item.isinterstatesale) return '';
                return $filter('IndianNumber')(item.taxamount / 2, 2);
            });
            column.getValue = function (item, col) {
                if (item.isinterstatesale) return 0;
                return item.taxamount/2;
            }
            column.total = true;
            column.cssClass = 'rightalign';
            var column = igrid.AddCustomColumn('igst', 'IGST', 50, function (item, col) {
                if (!item.isinterstatesale) return 0;
                return $filter('IndianNumber')(item.taxamount, 2);
            });
            column.getValue = function (item, col) {
                if (!item.isinterstatesale) return 0;
                   return item.taxamount;
            }
            column.total = true;
            column.cssClass = 'rightalign';

            $.each($scope.TaxHeaderPurchaseReturn, function (k, v) {

                var column = igrid.AddCustomColumn('tax_col_' + k + '_' + v.type, v.displaytext, 80, function (item, col) {

                    var taxtype = col.internal;
                    var ditem = item.taxbreakup.find(function (searchitem) {
                        return searchitem.taxpercent == taxtype.percentage;
                    });

                    if (typeof (ditem) == 'undefinied' || ditem == null)
                        return null;

                    if (taxtype.type == 1)
                        return $filter('IndianNumber')(ditem.taxableamount, 2);
                    else if (taxtype.type == 2)
                        return $filter('IndianNumber')(ditem.taxamount, 2);
                    else
                        return ditem.qty;

                });

                column.internal = v;
                column.total = true;
                column.cssClass = 'rightalign';

                column.getValue = function (item, col) {

                    var taxtype = col.internal;
                    var ditem = item.taxbreakup.find(function (searchitem) {
                        return searchitem.taxpercent == taxtype.percentage;
                    });

                    if (typeof (ditem) == 'undefinied' || ditem == null)
                        return 0;

                    if (taxtype.type == 1)
                        return ditem.taxableamount;
                    else if (taxtype.type == 2)
                        return ditem.taxamount;
                    else
                        return ditem.qty;
                }

            });
            igrid.AddSimpleColumn('createdemployee', 'Created By', 100);
            igrid.AddSimpleColumn('modifiedemployee', 'Modified By', 100);

            igrid.options.autoEdit = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.POReturnResult);


        }

        $scope.PurchseRetCancelConfig = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.POReturnResult);

            igrid.AddSimpleColumn('supplier', 'Supplier', 220);
            igrid.AddReferenceColumn('cityid', 'City', 160, $scope.city);
            igrid.AddCustomColumn('isinterstatesale', 'IGST', 100, function (item, col) {
                return item.isinterstatesale == true ? "YES" : "NO";
            });
            igrid.AddSimpleColumn('returnno', 'Debit Note No', 100);
            igrid.AddFormatedColumn('returndate', 'Debit Note Date', 120, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 100);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 120, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('qty', 'Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('returnamount', 'Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxamount', 'Tax', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('otheramount', 'Rounding', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('netamount', 'Net Amount', 100, 'N2', 2).total = true;

            $.each($scope.TaxHeaderPurchaseReturn, function (k, v) {

                var column = igrid.AddCustomColumn('tax_col_' + k, v.displaytext, 80, function (item, col) {

                    var taxtype = col.internal;
                    var ditem = item.taxbreakup.find(function (searchitem) {
                        return searchitem.taxpercent == taxtype.percentage;
                    });

                    if (typeof (ditem) == 'undefinied' || ditem == null)
                        return null;

                    if (taxtype.type == 1)
                        return $filter('IndianNumber')(ditem.taxableamount, 2);
                    else if (taxtype.type == 2)
                        return $filter('IndianNumber')(ditem.taxamount, 2);
                    else
                        return ditem.qty;

                });

                column.internal = v;
                column.getValue = function (item, col) {

                    var taxtype = col.internal;
                    var ditem = item.taxbreakup.find(function (searchitem) {
                        return searchitem.taxpercent == taxtype.percentage;
                    });

                    if (typeof (ditem) == 'undefinied' || ditem == null)
                        return 0;

                    if (taxtype.type == 1)
                        return $filter('IndianNumber')(ditem.taxableamount, 2);
                    else if (taxtype.type == 2)
                        return $filter('IndianNumber')(ditem.taxamount, 2);
                    else
                        return ditem.qty;


                }

            });

            igrid.options.autoEdit = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.POReturnResult);


        }

        $scope.StockConfig = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.StockResult);

            igrid.AddSimpleColumn('suppliername', 'Supplier Name', 150);
            igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            igrid.AddCustomColumn('isbit', 'BIT', 40, function (item, col) {
                return item.isbit == true ? "YES" : "NO";
            });
            igrid.AddSimpleColumn('glncode', 'GLN Code', 100);
            igrid.AddSimpleColumn('itbuscode', 'Item Code', 100);
            igrid.AddSimpleColumn('productname', 'Product Name', 150);
            igrid.AddReferenceColumn('productgroupid', 'Product Group', 100, $scope.ProductGroups);
            igrid.AddReferenceColumn('sectionid', 'Section', 120, $scope.SalesCounter);
            igrid.AddSimpleColumn('iqty', 'Purchase Qty', 60).total = true;
            igrid.AddSimpleColumn('qty', 'Stock Qty', 60).total = true;
            igrid.AddFormatedColumn('stockage', 'Age(days)', 60, 'N2', 0);
            igrid.AddFormatedColumn('stockholdingperiod', 'Holding Period', 80, 'N2', 0);
            igrid.AddFormatedColumn('taxable', 'Taxable Cost', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('buyingprice', 'Cost Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxvalue', 'Tax Value', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('price', 'MRP', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('dealerprice', 'Dealer Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('mrp', 'Sale Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('margin', 'Margin', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxablevalue', 'Taxable Value', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('stockvalue', 'Stock Value', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('salevalue', 'Sale Value', 100, 'N2', 2).total = true;

            igrid.AddSimpleColumn('brandname', 'Brand', 100);
            igrid.AddSimpleColumn('designid', 'Design', 100);
            igrid.AddSimpleColumn('materialname', 'Material', 90);
            igrid.AddSimpleColumn('patternname', 'Pattern', 90);
            igrid.AddSimpleColumn('stylename', 'Style', 90);
            igrid.AddSimpleColumn('colourname', 'Colour', 90);
            igrid.AddSimpleColumn('sizename', 'Size', 60);
            igrid.AddSimpleColumn('typename', 'Type', 60);
            //igrid.AddButtonColumn('action', 'Action', 50, 'View', function (e, args) {

            //    $scope.$apply(function () {
            //        $scope.ManageSearchItem('BARCODEVIEW', args.item);
            //    });
            //    return true;

            //});
            igrid.AddButtonColumn('action', 'Bit Update', 60, 'Bit Update', function (e, args) {

                $scope.$apply(function () {
                    $scope.ManageSearchItem('BITUPDATE', args.item);
                });
                return true;

            });

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.StockResult);

        }

        $scope.BarcodeSummaryConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.StockResult);

            igrid.AddSimpleColumn('suppliername', 'Supplier Name', 150);
            igrid.AddSimpleColumn('suppliercity', 'Supplier City', 100);
            igrid.AddSimpleColumn('agentname', 'Agent', 120);
            igrid.AddSimpleColumn('brandname', 'Brand', 100);
            igrid.AddReferenceColumn('sectionid', 'Section', 120, $scope.SalesCounter);
            igrid.AddSimpleColumn('productname', 'Product Name', 150);
            igrid.AddSimpleColumn('designid', 'Design', 100);
            igrid.AddSimpleColumn('sizename', 'Size', 60);
            igrid.AddSimpleColumn('colourname', 'Colour', 90);
            igrid.AddFormatedColumn('iqty', 'Purchase Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('returnqty', 'Return Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('oqty', 'Sale Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('jqty', 'Journal Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('whqty', 'WH Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('retailqty', 'Store Qty', 60, 'N2', 2).total = true;
            //igrid.AddFormatedColumn('basebuyingprice', 'Cost Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('buyingprice', 'Last Purchase Rate', 80, 'N2', 2);
            igrid.AddFormatedColumn('price', 'Last Sale Rate', 80, 'N2', 2);
            //igrid.AddFormatedColumn('ibuyingprice', 'Net Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('stockpercentage', 'Stock %', 60, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.StockResult);
        }

        $scope.PurchaseItemsConfig = function (showpo) {

            if (showpo == undefined || showpo == null) showpo = false;

            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.StockResult);

            igrid.AddSimpleColumn('agentname', 'Agent Name', 100);
            igrid.AddSimpleColumn('suppliername', 'Supplier Name', 150);
            igrid.AddSimpleColumn('suppliercity', 'Supplier City', 100);
            igrid.AddSimpleColumn('brandname', 'Brand', 100);
            igrid.AddSimpleColumn('designid', 'Design', 100);
            igrid.AddSimpleColumn('productname', 'Product Name', 150);
            igrid.AddSimpleColumn('sizename', 'Size', 60);
            igrid.AddSimpleColumn('colourname', 'Colour', 90);


            igrid.AddFormatedColumn('qty', 'Purchase Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('iqty', 'Purchase Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('returnqty', 'Return Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('oqty', 'Sale Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('jqty', 'Journal Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('whqty', 'WH Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('retailqty', 'Store Qty', 60, 'N2', 2).total = true;

            if (showpo) {
                var column = igrid.AddColumn({
                    id: "poqty",
                    name: "PO Qty",
                    field: "poqty",
                    width: 60,
                    cssClass: "cell-title",
                    editor: Slick.Editors.Text,
                    'sortable': true
                });
                column.total = true;
            }

            igrid.AddFormatedColumn('basebuyingprice', 'Cost Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('price', 'Sale Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('stockpercentage', 'Stock %', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('rowexpense', 'Exp %', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('dealerprice', 'Dealer Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('stockage', 'Age(days)', 60, 'N2', 0);
            igrid.AddSimpleColumn('lrentryno', 'Entry No', 80);
            igrid.AddFormatedColumn('entrydate', 'Entry Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 80);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('sectionid', 'Section', 120, $scope.SalesCounter);
            igrid.AddReferenceColumn('productgroupid', 'Product Group', 100, $scope.ProductGroups);
            igrid.AddSimpleColumn('materialname', 'Material', 90);
            igrid.AddSimpleColumn('patternname', 'Pattern', 90);
            igrid.AddSimpleColumn('stylename', 'Style', 90);
            igrid.AddSimpleColumn('typename', 'Type', 80);
            igrid.AddSimpleColumn('fitname', 'Fit', 80);
            igrid.AddSimpleColumn('sleevename', 'Sleeve', 90);
            igrid.AddSimpleColumn('agentcity', 'Agent City', 80);
            igrid.AddSimpleColumn('hsncode', 'HSN Code', 60);
            igrid.AddFormatedColumn('pdiscoutpercent', 'P.Discount%', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('pdiscout', 'P.Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('buyingprice', 'Taxable Cost', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('ptaxpercent', 'P.Tax%', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxvalue', 'Tax Value', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('ibuyingprice', 'Net Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('price', 'MRP', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('salestaxpercent', 'S.Tax%', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('salestaxamount', 'S.Tax', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('stockholdingperiod', 'Holding Period', 80, 'N2', 0);
            igrid.AddFormatedColumn('dealermarginpercentage', 'D.Margin %', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('dealermargin', 'D.Margin', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('netmarginpercent', 'Margin %', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('margin', 'Margin', 60, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.options.autoEdit = true;
            igrid.options.showHeaderRow = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.StockResult);
        }

        $scope.ConvertBarcode2PRF = function () {

            if ($scope.SearchCondition.companyid == undefined || $scope.SearchCondition.companyid == null
                || $scope.SearchCondition.companyid == 0) {
                $rootScope.Toaster.Error("Please Select Company");
                return;
            }
            if ($scope.SearchCondition.locationid == undefined || $scope.SearchCondition.locationid == null
                || $scope.SearchCondition.locationid == 0) {
                $rootScope.Toaster.Error("Please Select Location");
                return;
            }

            var items = [];
            $.each($scope.StockResult, function (k, v) {
                if (v.poqty &&
                    v.poqty != null) {
                    var poqty = parseFloat(v.poqty);
                    if (!isNaN(poqty) && poqty >0) {
                        v.poqty = poqty;
                        items.push(v);
                    }
                }
            })

            if (items.length > 0) {
                var request = {
                    "item": {
                        'companyid': $scope.SearchCondition.companyid,
                        'locationid': $scope.SearchCondition.locationid,
                        'employeeid': $scope.SearchCondition.employeeid,
                        'supplierid': $scope.SearchCondition.supplierid,
                        'items': items
                    },
                    "securitytoken": "String content"
                };
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "WarehouseReportService/ConvertBarcode2PRF",
                    data: srequest
                }).success(function (result) {
                    if (result)
                        $rootScope.Toaster.Info("PRF Created successfully.");
                }).error(function (result) {
                });
            }
            else {
                $rootScope.Toaster.Info("No PRF Qty found");
            }
        }

        $scope.exporttoXSLX = function (infygrid, subtitle) {
            var title = $rootScope.UserInfo.CompanyName;
            if ($scope.SearchCondition.companyid != null && $scope.SearchCondition.companyid > 0)
                title = $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company);
            infygrid.ExportXLSX(infygrid.grid, title, subtitle);
        }


        $scope.PurchaseDetailConfig = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.StockResult);

            igrid.AddReferenceColumn('companyid', 'Company', 100, $scope.Company);
            igrid.AddSimpleColumn('entrylocationname', 'Entry Location', 150);
            igrid.AddSimpleColumn('agentname', 'Agent Name', 100);
            igrid.AddSimpleColumn('suppliername', 'Supplier Name', 150);
            igrid.AddSimpleColumn('suppliergstno', 'Supplier GST', 80);
            //igrid.AddSimpleColumn('suppliercity', 'Supplier City', 100);
            igrid.AddSimpleColumn('sourcesuppliername', 'Source Supplier', 150);
            igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            igrid.AddSimpleColumn('brandname', 'Brand', 100);
            igrid.AddSimpleColumn('designid', 'Design', 100);
            igrid.AddSimpleColumn('productname', 'Product Name', 150);
            igrid.AddSimpleColumn('barcodeidname', 'Division Name', 100);
            igrid.AddSimpleColumn('sizename', 'Size', 60);
            igrid.AddReferenceColumn('sectionid', 'Section', 120, $scope.SalesCounter);
            /*igrid.AddReferenceColumn('productgroupid', 'Product Group', 100, $scope.ProductGroups);*/
            igrid.AddSimpleColumn('materialname', 'Material', 90);
            igrid.AddSimpleColumn('patternname', 'Pattern', 90);
            igrid.AddSimpleColumn('stylename', 'Style', 90);
            igrid.AddSimpleColumn('colourname', 'Colour', 90);
            igrid.AddSimpleColumn('typename', 'Type', 80);
            igrid.AddSimpleColumn('fitname', 'Fit', 80);
            igrid.AddSimpleColumn('sleevename', 'Sleeve', 90);
/*            igrid.AddSimpleColumn('pricetagname', 'PriceTag', 80);*/
/*            igrid.AddSimpleColumn('agentcity', 'Agent City', 80);*/
            igrid.AddSimpleColumn('hsncode', 'HSN Code', 60);

            igrid.AddSimpleColumn('lrentryno', 'Entry No', 80);
            igrid.AddFormatedColumn('entrydate', 'Entry Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 80);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 80, 'dd-MM-yyyy');

            igrid.AddFormatedColumn('iqty', 'Purchase Qty', 60, 'N2', 2).total = true;
            //igrid.AddFormatedColumn('qty', 'Barcode Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('basebuyingprice', 'Cost Price', 80, 'N2', 2);;
            igrid.AddFormatedColumn('pdiscoutpercent', 'P.Discount%', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('pdiscout', 'P.Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('buyingprice', 'Taxable Cost', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('ptaxpercent', 'P.Tax%', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxvalue', 'Tax Value', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('ibuyingprice', 'Net Price', 80, 'N2', 2).total = true;

            igrid.AddFormatedColumn('price', 'Sale Price', 80, 'N2', 2).total = true;

            var column = igrid.AddCustomColumn('totaltaxable', 'Total Taxable', 100, function (item, col) {
                return $filter('IndianNumber')(item.iqty * item.buyingprice, 2);
            });
            column.getValue = function (item, col) {
                return item.iqty * item.buyingprice;
            }
            column.total = true;
            column.cssClass = 'rightalign';
            var column = igrid.AddCustomColumn('sgst', 'SGST', 50, function (item, col) {
                if (item.isinterstatesale) return '';
                return $filter('IndianNumber')(item.iqty * item.taxvalue / 2, 2);
            });
            column.getValue = function (item, col) {
                if (item.isinterstatesale) return 0;
                return item.iqty * item.taxvalue / 2;
            }
            column.total = true;
            column.cssClass = 'rightalign';
            var column = igrid.AddCustomColumn('cgst', 'CGST', 50, function (item, col) {
                if (item.isinterstatesale) return '';
                return $filter('IndianNumber')(item.iqty * item.taxvalue / 2, 2);
            });
            column.getValue = function (item, col) {
                if (item.isinterstatesale) return 0;
                return item.iqty * item.taxvalue / 2;
            }
            column.total = true;
            column.cssClass = 'rightalign';
            var column = igrid.AddCustomColumn('igst', 'IGST', 50, function (item, col) {
                if (!item.isinterstatesale) return 0;
                return $filter('IndianNumber')(item.iqty * item.taxvalue, 2);
            });
            column.getValue = function (item, col) {
                return item.iqty * item.taxvalue;
            }
            column.total = true;
            column.cssClass = 'rightalign';
            var column = igrid.AddCustomColumn('netcost', 'Net Cost', 100, function (item, col) {
                return $filter('IndianNumber')(item.iqty * item.ibuyingprice, 2);
            });
            column.getValue = function (item, col) {
                return item.iqty * item.ibuyingprice;
            }
            column.total = true;
            column.cssClass = 'rightalign';
            //igrid.AddFormatedColumn('stockholdingperiod', 'Holding Period', 80, 'N2', 0);
            //igrid.AddFormatedColumn('dealermarginpercentage', 'D.Margin %', 60, 'N2', 2).total = true;
            //igrid.AddFormatedColumn('dealermargin', 'D.Margin', 60, 'N2', 2).total = true;
            //igrid.AddFormatedColumn('netmarginpercent', 'Margin %', 60, 'N2', 2).total = true;
            //igrid.AddFormatedColumn('margin', 'Margin', 60, 'N2', 2).total = true;
            //igrid.AddFormatedColumn('stockpercentage', 'Stock %', 60, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.StockResult);
        }

        $scope.BarcodeDetailConfig = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.StockResult);

            igrid.AddReferenceColumn('companyid', 'Company', 100, $scope.Company);
            igrid.AddSimpleColumn('entrylocationname', 'Entry Location', 150);
            igrid.AddSimpleColumn('agentname', 'Agent Name', 100);
            igrid.AddSimpleColumn('suppliername', 'Supplier Name', 150);
            igrid.AddSimpleColumn('suppliergstno', 'Supplier GST', 80);
            igrid.AddSimpleColumn('suppliercity', 'Supplier City', 100);
            igrid.AddSimpleColumn('sourcesuppliername', 'Source Supplier', 150);
            igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            igrid.AddSimpleColumn('brandname', 'Brand', 100);
            igrid.AddSimpleColumn('designid', 'Design', 100);
            igrid.AddSimpleColumn('productname', 'Product Name', 150);
            igrid.AddSimpleColumn('barcodeidname', 'Division Name', 100);
            igrid.AddSimpleColumn('sizename', 'Size', 60);
            igrid.AddFormatedColumn('iqty', 'Purchase Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('returnqty', 'Return Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('oqty', 'Sale Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('jqty', 'Journal Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('whqty', 'WH Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('retailqty', 'Store Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('basebuyingprice', 'Cost Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('rowexpense', 'Exp %', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('dealerprice', 'Dealer Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('price', 'Sale Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('stockage', 'Age(days)', 60, 'N2', 0);
            igrid.AddSimpleColumn('lrentryno', 'Entry No', 80);
            igrid.AddFormatedColumn('entrydate', 'Entry Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 80);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('stockdate', 'Barcode Date', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('sectionid', 'Section', 120, $scope.SalesCounter);
            igrid.AddReferenceColumn('productgroupid', 'Product Group', 100, $scope.ProductGroups);
            igrid.AddSimpleColumn('materialname', 'Material', 90);
            igrid.AddSimpleColumn('patternname', 'Pattern', 90);
            igrid.AddSimpleColumn('stylename', 'Style', 90);
            igrid.AddSimpleColumn('colourname', 'Colour', 90);
            igrid.AddSimpleColumn('typename', 'Type', 80);
            igrid.AddSimpleColumn('fitname', 'Fit', 80);
            igrid.AddSimpleColumn('sleevename', 'Sleeve', 90);
            igrid.AddSimpleColumn('pricetagname', 'PriceTag', 80);
            igrid.AddSimpleColumn('agentcity', 'Agent City', 80);
            igrid.AddSimpleColumn('hsncode', 'HSN Code', 60);
            igrid.AddFormatedColumn('pdiscoutpercent', 'P.Discount%', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('pdiscout', 'P.Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('buyingprice', 'Taxable Cost', 80, 'N2', 2).total = true;
            igrid.AddCustomColumn('isinterstatesale', 'IGST', 50, function (item, col) {
                return item.isinterstatesale == true ? "YES" : "NO";
            });
            igrid.AddFormatedColumn('ptaxpercent', 'P.Tax%', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxvalue', 'Tax Value', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('ibuyingprice', 'Net Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('price', 'Sale Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('finalprice', 'Net Sale Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('mrp', 'MRP', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('salestaxpercent', 'S.Tax%', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('salestaxamount', 'S.Tax', 60, 'N2', 2).total = true;


            var column = igrid.AddCustomColumn('totaltaxable', 'Total Taxable', 100, function (item, col) {
                return $filter('IndianNumber')(item.iqty * item.buyingprice, 2);
            });
            column.getValue = function (item, col) {
                return item.iqty * item.buyingprice;
            }
            column.total = true;
            column.cssClass = 'rightalign';
            var column = igrid.AddCustomColumn('sgst', 'SGST', 50, function (item, col) {
                if (item.isinterstatesale) return '';
                return $filter('IndianNumber')(item.iqty * item.taxvalue/2, 2);
            });
            column.getValue = function (item, col) {
                if (item.isinterstatesale) return 0;
                return item.iqty * item.taxvalue / 2;
            }
            column.total = true;
            column.cssClass = 'rightalign';
            var column = igrid.AddCustomColumn('cgst', 'CGST', 50, function (item, col) {
                if (item.isinterstatesale) return '';
                return $filter('IndianNumber')(item.iqty * item.taxvalue/2, 2);
            });
            column.getValue = function (item, col) {
                if (item.isinterstatesale) return 0;
                return item.iqty * item.taxvalue / 2;
            }
            column.total = true;
            column.cssClass = 'rightalign';
            var column = igrid.AddCustomColumn('igst', 'IGST', 50, function (item, col) {
                if (!item.isinterstatesale) return 0;
                return $filter('IndianNumber')(item.iqty * item.taxvalue, 2);
            });
            column.getValue = function (item, col) {
                return item.iqty * item.taxvalue;
            }
            column.total = true;
            column.cssClass = 'rightalign';
            var column = igrid.AddCustomColumn('netcost', 'Net Cost', 100, function (item, col) {
                return $filter('IndianNumber')(item.iqty * item.ibuyingprice, 2);
            });
            column.getValue = function (item, col) {
                return item.iqty * item.ibuyingprice;
            }
            column.total = true;
            column.cssClass = 'rightalign';
            igrid.AddFormatedColumn('stockholdingperiod', 'Holding Period', 80, 'N2', 0);
            igrid.AddFormatedColumn('dealermarginpercentage', 'D.Margin %', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('dealermargin', 'D.Margin', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('netmarginpercent', 'Margin %', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('margin', 'Margin', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('stockpercentage', 'Stock %', 60, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.StockResult);
        }

        $scope.StockAnalysisConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.StockAnalysisResult);

            if ($scope.SearchCondition.supplier) {
                igrid.AddReferenceColumn('supplierid', 'Supplier', 200, $scope.Supplier);
            }
            if ($scope.SearchCondition.sourcesupplier) {
                igrid.AddReferenceColumn('sourcesupplierid', 'Source Supplier', 200, $scope.Supplier);
            }
            if ($scope.SearchCondition.productgroup) {
                igrid.AddReferenceColumn('productgroupid', 'Product Group', 150, $scope.ProductGroups);
            }
            if ($scope.SearchCondition.section) {
                igrid.AddReferenceColumn('sectionid', 'Section', 150, $scope.Division);
            }
            if ($scope.SearchCondition.product) {
                igrid.AddSimpleColumn('productname', 'Product Name', 150);
            }
            if ($scope.SearchCondition.brand) {
                igrid.AddSimpleColumn('brandname', 'Brand Name', 100);
            }
            if ($scope.SearchCondition.type) {
                igrid.AddSimpleColumn('typename', 'Type Name', 100);
            }
            if ($scope.SearchCondition.material) {
                igrid.AddSimpleColumn('materialname', 'Material', 90);
            }
            if ($scope.SearchCondition.pattern) {
                igrid.AddSimpleColumn('patternname', 'Pattern', 90);
            }
            if ($scope.SearchCondition.style) {
                igrid.AddSimpleColumn('stylename', 'Style', 90);
            }
            if ($scope.SearchCondition.sleeve) {
                igrid.AddSimpleColumn('sleevename', 'Sleeve', 90);
            }
            if ($scope.SearchCondition.size) {
                igrid.AddSimpleColumn('sizename', 'Size', 60);
            }
            if ($scope.SearchCondition.design) {
                igrid.AddSimpleColumn('designid', 'Design', 90);
            }
            if ($scope.SearchCondition.isbarcode) {
                igrid.AddSimpleColumn('barcode', 'Barcode', 90);
            }

            igrid.AddFormatedColumn('qty', 'Qty', 100, 'N', 2).total = true;
            igrid.AddFormatedColumn('buyingprice', 'Taxable', 110, 'N', 2).total = true;
            igrid.AddFormatedColumn('price', 'Cost Price', 110, 'N', 2).total = true;
            igrid.AddFormatedColumn('salevalue', 'Sale Value', 120, 'N', 2).total = true;

            if ($scope.SearchCondition.isbarcode) {
                igrid.AddSimpleColumn('stockage', 'Days', 80, 'N', 2);
            }
            //igrid.AddButtonColumn('action', 'Action', 50, 'View', function (e, args) {

            //    $scope.$apply(function () {
            //        $scope.ManageSearchItem('BARCODEVIEW', args);
            //    });
            //    return true;
            //});

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.StockAnalysisResult);
        }

        $scope.AsOnStockConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.AsOnStockResult);

            if ($scope.SearchCondition.product)
                igrid.AddSimpleColumn('productname', 'Product Name', 150);
            if ($scope.SearchCondition.supplier)
                igrid.AddSimpleColumn('productname', 'Supplier Name', 150);
            if ($scope.SearchCondition.section)
                igrid.AddSimpleColumn('productname', 'Section Name', 150);


            igrid.AddFormatedColumn('asstockqty', 'As On Date Qty', 100, 'N', 2).total = true;
            if (typeof ($scope.Permission.p_SHOWVALUE) != 'undefined' && !$scope.Permission.p_SHOWVALUE.view) {
                igrid.AddFormatedColumn('asstockvalue', 'As On Date Cost', 120, 'N', 2).total = true;
                igrid.AddFormatedColumn('iasstockvalue', 'As On Date N.Cost', 120, 'N', 2).total = true;
                igrid.AddFormatedColumn('assalevalue', 'As On Date Sale Value', 120, 'N', 2).total = true;
            }

            /*
            igrid.AddFormatedColumn('purchaseqty', 'Purchase Qty', 80, 'N', 2).total = true;

            if (typeof ($scope.Permission.p_SHOWVALUE) != 'undefined' && !$scope.Permission.p_SHOWVALUE.view) {
                igrid.AddFormatedColumn('purchasevalue', 'Purchase Cost', 100, 'N', 2).total = true;
                igrid.AddFormatedColumn('ipurchasevalue', 'N.Purchase Cost', 100, 'N', 2).total = true;
            }

            igrid.AddFormatedColumn('purchasereturnqty', 'Pur.Ret Qty', 80, 'N', 2).total = true;

            if (typeof ($scope.Permission.p_SHOWVALUE) != 'undefined' && !$scope.Permission.p_SHOWVALUE.view) {
                igrid.AddFormatedColumn('purchasereturnvalue', 'Pur.Ret Cost', 100, 'N', 2).total = true;
                igrid.AddFormatedColumn('ipurchasereturnvalue', 'N.Pur.Ret Cost', 100, 'N', 2).total = true;
            }

            igrid.AddFormatedColumn('saleqty', 'Sale Qty', 80, 'N', 2).total = true;
            if (typeof ($scope.Permission.p_SHOWVALUE) != 'undefined' && !$scope.Permission.p_SHOWVALUE.view) {
                igrid.AddFormatedColumn('salevalue', 'Sale(Stock) Cost', 100, 'N', 2).total = true;
                igrid.AddFormatedColumn('isalevalue', 'N.Sale(Stock) Cost', 100, 'N', 2).total = true;
            }
            */

            igrid.AddFormatedColumn('auditqty', 'Audit Qty', 100, 'N', 2).total = true;
            if (typeof ($scope.Permission.p_SHOWVALUE) != 'undefined' && !$scope.Permission.p_SHOWVALUE.view) {
                igrid.AddFormatedColumn('auditvalue', 'Audit Cost', 120, 'N', 2).total = true;
                igrid.AddFormatedColumn('iauditvalue', 'Audit N.Cost', 120, 'N', 2).total = true;
                igrid.AddFormatedColumn('auditsalevalue', 'Audit Sale Value', 120, 'N', 2).total = true;
            }


            igrid.AddFormatedColumn('currentqty', 'Current Qty', 100, 'N', 2).total = true;
            if (typeof ($scope.Permission.p_SHOWVALUE) != 'undefined' && !$scope.Permission.p_SHOWVALUE.view) {
                igrid.AddFormatedColumn('currentvalue', 'Current Cost', 120, 'N', 2).total = true;
                igrid.AddFormatedColumn('icurrentvalue', 'Current Net Cost', 120, 'N', 2).total = true;
                igrid.AddFormatedColumn('currentsalevalue', 'Current Sale Value', 120, 'N', 2).total = true;
            }

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.AsOnStockResult);
        }

        $scope.StockTransactionConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.StockTransactionResult);


            var exist = $scope.StockTransactionResult.find(function (item) { return item.productid && item.productid > 0; });
            if (exist && exist!=null)
                igrid.AddReferenceColumn('productid', 'Product', 150, $scope.Product);

            exist = $scope.StockTransactionResult.find(function (item) { return item.sectionid && item.sectionid > 0; });
            if (exist && exist != null)
                igrid.AddReferenceColumn('sectionid', 'Section', 150, $scope.SalesCounter);

            exist = $scope.StockTransactionResult.find(function (item) { return item.brandid && item.brandid > 0; });
            if (exist && exist != null)
                igrid.AddReferenceColumn('brandid', 'Brand', 150, $scope.Brand);

            exist = $scope.StockTransactionResult.find(function (item) { return item.styleid && item.styleid > 0; });
            if (exist && exist != null)
                igrid.AddReferenceColumn('styleid', 'Style', 150, $scope.Style);


            igrid.AddFormatedColumn('openingqty', 'Opening Qty', 80, 'N', 2).total = true;
            igrid.AddFormatedColumn('openingstockvalue', 'Opening Cost', 100, 'N', 2).total = true;
            igrid.AddFormatedColumn('openingsalevalue', 'Opening SaleValue', 100, 'N', 2).total = true;

            igrid.AddFormatedColumn('purchaseqty', 'Purchase Qty', 80, 'N', 2).total = true;
            igrid.AddFormatedColumn('purchasestockvalue', 'Purchase Cost', 100, 'N', 2).total = true;
            igrid.AddFormatedColumn('purchasesalevalue', 'Purchase SaleValue', 100, 'N', 2).total = true;

            igrid.AddFormatedColumn('purchaseretqty', 'Purchase Ret Qty', 80, 'N', 2).total = true;
            igrid.AddFormatedColumn('purchaseretstockvalue', 'Purchase Ret Cost', 100, 'N', 2).total = true;
            igrid.AddFormatedColumn('purchaseretsalevalue', 'Purchase Ret SaleValue', 100, 'N', 2).total = true;

            igrid.AddFormatedColumn('saleqty', 'Sale Qty', 80, 'N', 2).total = true;
            igrid.AddFormatedColumn('salestockvalue', 'Sale Cost', 100, 'N', 2).total = true;
            igrid.AddFormatedColumn('salesalevalue', 'Sale Value', 100, 'N', 2).total = true;

            //igrid.AddFormatedColumn('jobworkoutward', 'Jobwork Outward', 100, 'N', 2).total = true;
            //igrid.AddFormatedColumn('jobworkinward', 'Jobwork Inward', 100, 'N', 2).total = true;

            igrid.AddFormatedColumn('auditqty', 'Audit Qty', 60, 'N', 2).total = true;
            igrid.AddFormatedColumn('auditstockvalue', 'Audit Cost', 80, 'N', 2).total = true;
            igrid.AddFormatedColumn('auditsalevalue', 'Audit SaleValue', 80, 'N', 2).total = true;

            igrid.AddFormatedColumn('closingqty', 'Closing Qty', 80, 'N', 2).total = true;
            igrid.AddFormatedColumn('closingstockvalue', 'Closing Cost', 100, 'N', 2).total = true;
            igrid.AddFormatedColumn('closingsalevalue', 'Closing SaleValue', 100, 'N', 2).total = true;

            igrid.AddFormatedColumn('currentqty', 'Current Qty', 80, 'N', 2).total = true;
            igrid.AddFormatedColumn('currentstockvalue', 'Current Cost', 100, 'N', 2).total = true;
            igrid.AddFormatedColumn('currentsalevalue', 'Current SaleValue', 100, 'N', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.StockTransactionResult);
        }

        $scope.OutwardConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.OutwardResult);

            igrid.AddSimpleColumn('code', 'Package No', 120);
            igrid.AddReferenceColumn('status', 'Status', 80, $scope.BundleStatus);
            igrid.AddSimpleColumn('fromlocation', 'From Location', 150);
            igrid.AddSimpleColumn('tolocation', 'To Location', 150);
            igrid.AddFormatedColumn('packedon', 'Packed On', 120, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('receivedon', 'Received On', 120, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('receivedby', 'Received By', 150);
            igrid.AddFormatedColumn('qty', 'Transfer Qty', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('aqty', 'Accepted Qty', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('buyingprice', 'Cost Price', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('ibuyingprice', 'Net Cost Price', 100, 'N2', 2).total = true;
            if (!$scope.SearchCondition.showsummary)
                igrid.AddFormatedColumn('taxpercent', 'Tax %', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('price', 'Sale Price', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('saleprice', 'Net Sale Price', 100, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.OutwardResult);
        }

        $scope.NewProductConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.NewProductList);

            igrid.AddSimpleColumn('code', 'Code', 100);
            igrid.AddSimpleColumn('name', 'ProductName', 300);
            igrid.AddSimpleColumn('employeename', 'Created By', 180);
            igrid.AddFormatedColumn('createdon', 'Created On', 170, 'dd/MM/yyyy');
            igrid.AddReferenceColumn('salescounterid', 'Section', 170, $scope.SalesCounter);

            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.NewProductList);
        }

        $scope.StockMovementConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.StockMovementResult);

            if ($scope.SearchCondition.hasallfields) {
                igrid.AddSimpleColumn('locationname', 'Location', 150);
                igrid.AddSimpleColumn('agentname', 'Agent Name', 100);
                igrid.AddSimpleColumn('suppliername', 'Supplier Name', 150);
                igrid.AddSimpleColumn('suppliercity', 'Supplier City', 100);
                igrid.AddSimpleColumn('barcode', 'Barcode', 100);
                igrid.AddSimpleColumn('brandname', 'Brand', 100);
                igrid.AddSimpleColumn('designid', 'Design', 100);
                igrid.AddSimpleColumn('productname', 'Product Name', 150);
                igrid.AddSimpleColumn('sizename', 'Size', 60);
                igrid.AddFormatedColumn('basebuyingprice', 'Cost Price', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('rowexpense', 'Exp %', 60, 'N2', 2).total = true;
                igrid.AddFormatedColumn('dealerprice', 'Dealer Price', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('price', 'Sale Price', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('stockage', 'Age(days)', 60, 'N2', 0);
                igrid.AddSimpleColumn('lrentryno', 'Entry No', 80);
                igrid.AddFormatedColumn('entrydate', 'Entry Date', 80, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('invoiceno', 'Invoice No', 80);
                igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 80, 'dd-MM-yyyy');
                igrid.AddReferenceColumn('sectionid', 'Section', 120, $scope.SalesCounter);
                igrid.AddReferenceColumn('productgroupid', 'Product Group', 100, $scope.ProductGroups);
                igrid.AddSimpleColumn('materialname', 'Material', 90);
                igrid.AddSimpleColumn('patternname', 'Pattern', 90);
                igrid.AddSimpleColumn('stylename', 'Style', 90);
                igrid.AddSimpleColumn('colourname', 'Colour', 90);
                igrid.AddSimpleColumn('typename', 'Type', 80);
                igrid.AddSimpleColumn('fitname', 'Fit', 80);
                igrid.AddSimpleColumn('sleevename', 'Sleeve', 90);
                //igrid.AddSimpleColumn('agentcity', 'Agent City', 80);
                igrid.AddSimpleColumn('hsncode', 'HSN Code', 60);
                igrid.AddFormatedColumn('pdiscoutpercent', 'P.Discount%', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('pdiscout', 'P.Discount', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('buyingprice', 'Taxable Cost', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('ptaxpercent', 'P.Tax%', 60, 'N2', 2).total = true;
                igrid.AddFormatedColumn('taxvalue', 'Tax Value', 60, 'N2', 2).total = true;
                igrid.AddFormatedColumn('ibuyingprice', 'Net Cost Price', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('discount', 'S.Discount', 80, 'N2', 2).total = true;

                var column = igrid.AddCustomColumn('discountpercentage', '%', 50, function (item, col) {
                    var val = (item.discount * 100) / item.price;
                    if (val != 0) return $filter('IndianNumber')(val, 0);
                    return '';
                });
                column.cssClass = 'rightalign';

                igrid.AddFormatedColumn('price', 'MRP', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('finalprice', 'Final Price', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('salestaxpercent', 'S.Tax%', 60, 'N2', 2).total = true;
                igrid.AddFormatedColumn('salestaxamount', 'S.Tax', 60, 'N2', 2).total = true;
                igrid.AddFormatedColumn('stockholdingperiod', 'Holding Period', 80, 'N2', 0);
                igrid.AddFormatedColumn('netmarginpercent', 'Margin %', 60, 'N2', 2).total = true;
                igrid.AddFormatedColumn('margin', 'Margin', 60, 'N2', 2).total = true;
            }
            else {
                if ($scope.SearchCondition.haslocation)
                    igrid.AddSimpleColumn('locationname', 'LOCATION', 200);
                if ($scope.SearchCondition.hassupplier) {
                    igrid.AddReferenceColumn('supplierid', 'SUPPLIER', 200, $scope.Supplier);
                    igrid.AddReferenceColumn('suppliercityid', 'SUPPLIER CITY', 100, $scope.city);
                }
                if ($scope.SearchCondition.haslrentryno)
                    igrid.AddSimpleColumn('lrentryno', 'LR.ENTRY NO', 80);
                if ($scope.SearchCondition.hasinvoiceno)
                    igrid.AddSimpleColumn('invoiceno', 'INVOICE NO', 80);
                if ($scope.SearchCondition.hasinventrydate)
                    igrid.AddFormatedColumn('inventrydate', 'INV.ENTRY DATE', 100, 'dd/MM/yyyy');
                if ($scope.SearchCondition.hasinvoicedate)
                    igrid.AddFormatedColumn('invoicedate', 'INVOICE DATE', 100, 'dd/MM/yyyy');
                if ($scope.SearchCondition.hasproduct)
                    igrid.AddReferenceColumn('productid', 'PRODUCT', 120, $scope.Product);
                if ($scope.SearchCondition.hasbarcode)
                    igrid.AddSimpleColumn('barcode', 'BARCODE', 100);
            }

            igrid.AddFormatedColumn('purchaseqty', 'PUR QTY', 100, 'N', 2).total = true;
            if ($scope.SearchCondition.details)
                igrid.AddFormatedColumn('purchasevalue', 'PUR VALUE', 100, 'N', 2).total = true;

            igrid.AddFormatedColumn('purchaseretqty', 'PUR RETURN QTY', 100, 'N', 2).total = true;
            if ($scope.SearchCondition.details)
                igrid.AddFormatedColumn('purchaseretvalue', 'PUR RETURN VALUE', 100, 'N', 2).total = true;

            igrid.AddFormatedColumn('saleqty', 'SALE QTY', 100, 'N', 2).total = true;
            if ($scope.SearchCondition.details)
                igrid.AddFormatedColumn('salevalue', 'SALE VALUE', 100, 'N', 2).total = true;

            igrid.AddFormatedColumn('inwardqty', 'INWARD QTY', 100, 'N', 2).total = true;
            if ($scope.SearchCondition.details)
                igrid.AddFormatedColumn('inwardvalue', 'INWARD VALUE', 100, 'N', 2).total = true;

            igrid.AddFormatedColumn('outwardqty', 'OUTWARD QTY', 100, 'N', 2).total = true;
            if ($scope.SearchCondition.details)
                igrid.AddFormatedColumn('outwardvalue', 'OUTWARD VALUE', 100, 'N', 2).total = true;

            igrid.AddFormatedColumn('auditqty', 'AUDIT QTY', 80, 'N', 2).total = true;
            igrid.AddFormatedColumn('stockqty', 'CLOSING STOCK', 100, 'N', 2).total = true;
            if ($scope.SearchCondition.details)
                igrid.AddFormatedColumn('stockvalue', 'CLOSING VALUE', 100, 'N', 2).total = true;
            igrid.AddFormatedColumn('outpercentage', 'OUT %', 60, 'N', 2);

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.StockMovementResult);
        }

        $scope.NewSuppliersConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.EnableSerial($scope.SearchNewSuppliersList);

            igrid.AddSimpleColumn('code', 'Code', 120);
            igrid.AddSimpleColumn('name', 'Supplier Name', 300);
            igrid.AddReferenceColumn('cityid', 'City', 100, $scope.city);
            igrid.AddSimpleColumn('address', 'Address', 150);
            igrid.AddSimpleColumn('pincode', 'Pincode', 80);
            igrid.AddSimpleColumn('contactperson', 'Contact Person', 100);
            igrid.AddSimpleColumn('contactnumber', 'Contact No', 100);
            igrid.AddSimpleColumn('emailid', 'Email Id', 100);
            igrid.AddReferenceColumn('agentid', 'Agent', 100, $scope.AgentList);
            igrid.AddReferenceColumn('preferredtransportid', 'Transport', 100, $scope.TransportList);
            igrid.AddCustomColumn('isinterstatesale', 'IGST', 60, function (item, col) {
                return item.isinterstatesale == true ? "YES" : "NO";
            });
            igrid.AddFormatedColumn('creditdays', 'Credit Days', 80, 'N2', 0);
            igrid.AddSimpleColumn('tinanddate', 'GST NO', 120);
            igrid.AddSimpleColumn('msmeno', 'MSME NO', 100);
            igrid.AddCustomColumn('isunregistered', 'URD', 60, function (item, col) {
                return item.isunregistered == true ? "YES" : "NO";
            });
            igrid.AddSimpleColumn('bankname', 'Bank Name', 80);
            igrid.AddSimpleColumn('branchname', 'Branch Name', 100);
            igrid.AddSimpleColumn('bankaccountname', 'Account Name', 100);
            igrid.AddSimpleColumn('bankaccountno', 'Account No', 80);
            igrid.AddSimpleColumn('bankifci', 'IFSC', 80);

            igrid.AddSimpleColumn('createdusername', 'Created By', 120);
            igrid.AddFormatedColumn('createdon', 'Created On', 120, 'dd/MM/yyyy');
            igrid.AddSimpleColumn('modifiedusername', 'Modified By', 120);
            igrid.AddFormatedColumn('modifiedon', 'Modified On', 120, 'dd/MM/yyyy');

            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.SearchNewSuppliersList);
        }

        $scope.AgentConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.EnableSerial($scope.AgentList);

            igrid.AddSimpleColumn('name', 'Agent Name', 300);
            igrid.AddSimpleColumn('contactperson', 'Contact Person', 200);
            igrid.AddSimpleColumn('contactno', 'Contact No', 200);

            igrid.AddSimpleColumn('employeename', 'Created By', 180);
            igrid.AddFormatedColumn('createdon', 'Created On', 170, 'dd/MM/yyyy');

            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.AgentList);
        }

        $scope.TaxConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.EnableSerial($scope.TaxList);

            igrid.AddSimpleColumn('code', 'Tax Code', 200);
            igrid.AddSimpleColumn('name', 'Tax Name', 200);
            igrid.AddSimpleColumn('employeename', 'Created By', 180);
            igrid.AddFormatedColumn('createdon', 'Created On', 170, 'dd/MM/yyyy');

            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.TaxList);
        }


        $scope.PendingOutwardConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.EnableSerial($scope.OutwardPendingResult);

            igrid.AddSimpleColumn('typename', 'Supplier', 120);
            igrid.AddSimpleColumn('itemname', 'Printing Name', 130);
            igrid.AddSimpleColumn('barcode', 'Barcode', 90);
            igrid.AddFormatedColumn('buyingprice', 'Buy.Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('price', 'Price', 90, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('iqty', 'P.Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('qty', 'Qty', 60, 'N2', 2).total = true;
            igrid.AddSimpleColumn('sizename', 'LR.Ref No', 100);
            igrid.AddSimpleColumn('productname', 'Product', 120);
            igrid.AddSimpleColumn('brandname', 'Brand', 120);
            igrid.AddSimpleColumn('materialname', 'Material', 100);

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.OutwardPendingResult);
        }

        $scope.OutwardDetailsConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.OutwardDetailsResult);

            igrid.AddSimpleColumn('code', 'Package No', 80);
            igrid.AddReferenceColumn('status', 'Status', 100, $scope.BundleStatus);
            igrid.AddReferenceColumn('fromcompanyid', 'From Company', 100, $scope.Company);
            igrid.AddReferenceColumn('tocompanyid', 'To Company', 100, $scope.Company);
            igrid.AddSimpleColumn('fromlocation', 'From Location', 120);
            igrid.AddSimpleColumn('tolocation', 'To Location', 120);
            igrid.AddFormatedColumn('packedon', 'Packed On', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('receivedon', 'Received On', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('receivedby', 'Received By', 150);
            igrid.AddSimpleColumn('suppliername', 'Supplier Name', 100);
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 80);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('barcode', 'Barcode', 80);
            igrid.AddSimpleColumn('hsncode', 'HSN Code', 60);
            igrid.AddSimpleColumn('designid', 'Design ID', 60);
            igrid.AddSimpleColumn('productname', 'Product', 120);
            igrid.AddSimpleColumn('section', 'Section', 80);
            igrid.AddSimpleColumn('brandname', 'Brand', 80);
            igrid.AddSimpleColumn('sizename', 'Size', 60);
            igrid.AddFormatedColumn('qty', 'Transfer Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('aqty', 'Accepted Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('itemprice', 'Item Price', 80, 'N2', 2);
            igrid.AddFormatedColumn('itemmrp', 'Item MRP', 80, 'N2', 2);
            igrid.AddFormatedColumn('buyingprice', 'Tot.Cost Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('ibuyingprice', 'Net Cost Price', 90, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxpercent', 'Tax %', 60, 'N2', 2);
            igrid.AddFormatedColumn('taxamount', 'Tax', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('price', 'Total Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Total Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('saleprice', 'Net Sale Price', 80, 'N2', 2).total = true;
            //igrid.AddSimpleColumn('brandname', 'Brand', 80);
            //igrid.AddSimpleColumn('designid', 'Design', 60);
            //igrid.AddSimpleColumn('hsncode', 'HSNCode', 60);
            //igrid.AddSimpleColumn('sizename', 'Size', 60);
            //igrid.AddSimpleColumn('isbit', 'Bit', 40);
            //igrid.AddFormatedColumn('itembuyingprice', 'Buyingprice', 80, 'N2', 2).total = true;
            //igrid.AddFormatedColumn('itemprice', 'Price', 60, 'N2', 2).total = true;
            //igrid.AddFormatedColumn('itemdiscount', 'Discount', 60, 'N2', 2).total = true;
            //igrid.AddFormatedColumn('qty', 'Qty', 60, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.OutwardDetailsResult);
        }


        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = ''
            if ($scope.SearchCondition.companyid != null && $scope.SearchCondition.companyid > 0)
                title = $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company);

            if (tableid == "lrentryreport")
                subtitle = 'GRN ENTRY REPORT FROM : ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');
            if (tableid == 'lrinvoicereport')
                subtitle = 'INVOICE ENTRY REPORT FROM : ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');
            if (tableid == 'purchaseitemlist')
                subtitle = 'PURCHASE ENTRY REPORT FROM : ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');
            if (tableid == 'purchaseentry')
                subtitle = 'PURCHASE ENTRY REPORT FROM : ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');
            if (tableid == 'purchaseentrylist')
                subtitle = 'PURCHASE ENTRY REPORT FROM : ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company) + " ) ";
            if (tableid == 'purchasehsn')
                subtitle = 'PURCHASE HSN REPORT FROM : ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');
            if (tableid == 'purchaseretsum')
                subtitle = 'PURCHASE RETURN SUMMARY REPORT FROM : ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');
            if (tableid == 'purchaseretsumcancel')
                subtitle = 'PURCHASE RETURN CANCEL REPORT FROM : ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');
            if (tableid == 'outwarddetreport')
                subtitle = 'OUTWARD DETAIL REPORT ';
            if (tableid == 'inwardreport')
                subtitle = 'OUTWARD REPORT FROM : ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ";
            if (tableid == 'outward')
                subtitle = 'PENDING OUTWARD REPORT FROM : ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');
            if (tableid == 'supplierbill')
                subtitle = 'SUPPLIER BILLS REPORT FROM : ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');
            if (tableid == 'stock')
                subtitle = 'STOCK AS ON - ' + $filter('date')(new Date(), 'dd-MMM-yyyy');
            if (tableid == 'asonstock')
                subtitle = 'STOCK AS ON - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy');
            if (tableid == 'stockmovement')
                subtitle = 'STOCK MOVEMENT ';
            if (tableid == 'productlist')
                subtitle = 'PRODUCT DETAILS';
            if (tableid == 'supplierlist')
                subtitle = 'SUPPLIER DETAILS';
            if (tableid == 'agentlist')
                subtitle = 'AGENT DETAILS';
            if (tableid == 'joboutwardreport')
                subtitle = 'JOB WORK OUTWARD DETAIL REPORT ';


            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, title, subtitle, true);
            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = filename + ".xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }

        $scope.Entity = {
            "supplierid": 0,
            "locationid": 0,
            Details: {
                "requestedby": 0,
                "fromdate": null,
                "todate": null,
                "reasonid": 0,
                "priorityid": 0,
                "pending": true,
                "printingname": '',
                "productid": 0,
                "brandid": 0,
                "patternid": 0,
                "styleid": 0,
                "sizeid": 0,
                "colourid": 0,
                "materialid": 0,
                "id": 0
            }
        };
        $scope.ProductRequest = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorMessage = "";

        $scope.SearchPendingPO = function () {
            if ($scope.ProductRequest.locationid == null || $scope.ProductRequest.locationid == 0) {
                $rootScope.Toaster.Error("Please select Location !");
                return;
            }
            var request = {
                "Item": (JSON.parse(JSON.stringify($scope.ProductRequest.Details))),
                "securitytoken": "String content"
            }
            request.Item.locationid = $scope.ProductRequest.locationid;
            request.Item.supplierid = $scope.ProductRequest.supplierid;
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ProductrequestformService/SearchOrders",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.PendingPOConfig();
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No PO Request found.");
                }
            });
        };

        $scope.PendingPOConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.EnableSerial($scope.SearchResult);

            igrid.AddFormatedColumn('requestedon', 'Requested On', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('supplierid', 'Supplier', 100, $scope.Supplier);
            igrid.AddSimpleColumn('productname', 'Printing Name', 100);
            igrid.AddSimpleColumn('brandname', 'Brand Name', 60);
            igrid.AddSimpleColumn('materialname', 'Material', 90);
            igrid.AddSimpleColumn('patternname', 'Pattern', 90);
            igrid.AddSimpleColumn('stylename', 'Style', 90);
            igrid.AddSimpleColumn('sizename', 'Size', 60);
            igrid.AddSimpleColumn('designid', 'Design', 90);

            igrid.AddSimpleColumn('qty', 'Qty', 60);

            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.SearchResult);
        }

        $scope.LoadReportType();
        $scope.panelheight = { 'height': '400px' };
        $timeout(function () {

            var available = (typeof ($rootScope.windowSize) != 'undefined' && typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            if (available > 0) {
                $scope.panelheight.height = '' + (available - 50) + 'px';
            }

            if (typeof ($stateParams.Search) != 'undefined' && $stateParams.Search) {
                //if (typeof ($stateParams.fromdate) != 'undefined')
                //    $scope.SearchCondition.fromdate = $stateParams.fromdate;

                //if (typeof ($stateParams.todate) != 'undefined')
                //    $scope.SearchCondition.todate = $stateParams.todate;

                $scope.ShowFilter = false;

            }

        }, 10);

        $scope.rounding = 2;
        $scope.Appsetting_Company = false;
        $scope.LocalContext = cntxService.CreatePageContext('WHReport');
        $scope.LoadApplicationSetting = function () {
            if (typeof ($rootScope.UserInfo) != 'undefined' &&
                typeof ($rootScope.UserInfo.appsettings) != 'undefined' &&
                $rootScope.UserInfo.appsettings != null) {
                $scope.SearchConditionStkTrans = $scope.LocalContext.Get('$scope.SearchConditionStkTrans', false);
                $scope.Appsetting_Company = true;
                if (false || $rootScope.UserInfo.appsettings.system_b2brounding)
                    $scope.rounding = 4;
            }
            else {
                $timeout(function () {
                    $scope.LoadApplicationSetting();
                }, 50);
            }
        }
        $scope.LoadApplicationSetting();
        $scope.$on(
            "$destroy",
            function (event) {
                $scope.LocalContext.Post('$scope.SearchConditionStkTrans', $scope.SearchConditionStkTrans, false);
            }
        );

        var Options =
        {
            Product: 0,
            Brand: 1,
            Size: 2,
            Pattern: 3,
            Material: 4,
            Style: 5,
            Colour: 6,
            Type: 7,
            Section: 8,
            SaleLocation: 9,
            StockLocation: 10,
            Item: 105,
            Design: 11,
            SaleRate: 12,
            PurchaseRate: 13,
            DiscountRate: 14,
            SoldRate: 15,
            SaleRate_Range: 16,
            StockAge_Range: 17,
            PurchaseRate_Range: 18,
            DiscountRate_Range: 19,
            SoldRate_Range: 20,
            LREntryNo: 21,
            InvoiceNo: 22,
            InvDate: 23,
            SaleYear: 24,
            PurchaseYear: 25,
            PurchaseMonth: 26,
            SaleMonth: 27,
            BillNo: 28,
            BillDate: 29,
            Supplier: 30,
            Salesman: 31,
            Supplier_City: 32,
            Barcode: 33,
            HSNCode: 34,
            Tax: 35,
            SaleWeekDay: 36,
            SaleMonthDay: 37,
            SaleMargin_Range: 38,
            RetailMargin_Range: 39,
            Agent: 40,
            Counter: 48,
            Company: 49,
            ProductGroup: 50,
            ReturnReason: 51,
            SaleReason: 52,
            Area: 53,
            Season: 54,
            PurchaseDate: 84,
            Customer_Religion: 93,
            SourceSupplier: 94,
            ProductCode: 98,
            Fit: 99,
            Sleeve: 100,
            TaxPercentage: 102,
            GLNNO: 103,
            Floor: 104,
            StockMarker: 106,
            JobworkSupplier: 110,
            SellingMode: 111,
            BillType: 113,
            PriceTag: 114
        }
        $scope.Options = Options;
        $scope.AnalyserOptions = [
            { 'id': Options.Company, 'name': 'COMPANY' },
            { 'id': Options.Product, 'name': 'PRODUCT' },
            { 'id': Options.Brand, 'name': 'BRAND' },
            { 'id': Options.Size, 'name': 'SIZE' },
            { 'id': Options.Pattern, 'name': 'PATTERN' },
            { 'id': Options.Material, 'name': 'MATERIAL' },
            { 'id': Options.Style, 'name': 'STYLE' },
            { 'id': Options.Colour, 'name': 'COLOUR' },
            { 'id': Options.Type, 'name': 'TYPE' },
            { 'id': Options.Section, 'name': 'SECTION' },
            { 'id': Options.ProductGroup, 'name': 'PRODUCT GROUP' },
            { 'id': Options.ProductCode, 'name': 'PRODUCT CODE' },
            { 'id': Options.Fit, 'name': 'FIT' },
            { 'id': Options.Sleeve, 'name': 'SLEEVE' },
            { 'id': Options.PriceTag, 'name': 'PRICE TAG' },
            { 'id': Options.StockLocation, 'name': 'STOCK LOCATION' },
            { 'id': Options.Design, 'name': 'DESIGN' },
            { 'id': Options.SaleRate, 'name': 'SALE RATE' },
            { 'id': Options.PurchaseRate, 'name': 'PURCHASE RATE' },
            { 'id': Options.DiscountRate, 'name': 'DISCOUNT RATE' },
            { 'id': Options.LREntryNo, 'name': 'LRENTRY NO' },
            { 'id': Options.InvoiceNo, 'name': 'INVOICE NO' },
            { 'id': Options.InvDate, 'name': 'INVOICE DATE' },
            { 'id': Options.PurchaseDate, 'name': 'PURCHASE DATE' },
            { 'id': Options.Agent, 'name': 'AGENT' },
            { 'id': Options.Supplier, 'name': 'SUPPLIER' },
            { 'id': Options.Supplier_City, 'name': 'SUPPLIER CITY' },
            { 'id': Options.Barcode, 'name': 'BARCODE' },
            { 'id': Options.GLNNO, 'name': 'GLN NO' },
            { 'id': Options.HSNCode, 'name': 'HSN CODE' },
            { 'id': Options.TaxPercentage, 'name': 'TAX PERCENTAGE' },
            { 'id': Options.SourceSupplier, 'name': 'SOURCE SUPPLIER' }
        ];

        $scope.AnalyserOptions.sort(function (a, b) {
            var x = a.name.toLowerCase();
            var y = b.name.toLowerCase();
            return x < y ? -1 : x > y ? 1 : 0;
        });

        $scope.AnalyserFilters = [
            { 'id': Options.Company, 'name': 'COMPANY' },
            { 'id': Options.Product, 'name': 'PRODUCT' },
            { 'id': Options.Brand, 'name': 'BRAND' },
            { 'id': Options.Size, 'name': 'SIZE' },
            { 'id': Options.Pattern, 'name': 'PATTERN' },
            { 'id': Options.Material, 'name': 'MATERIAL' },
            { 'id': Options.Style, 'name': 'STYLE' },
            { 'id': Options.Colour, 'name': 'COLOUR' },
            { 'id': Options.Type, 'name': 'TYPE' },
            { 'id': Options.Section, 'name': 'SECTION' },
            { 'id': Options.ProductGroup, 'name': 'PRODUCT GROUP' },
            { 'id': Options.Fit, 'name': 'FIT' },
            { 'id': Options.Sleeve, 'name': 'SLEEVE' },
            { 'id': Options.Design, 'name': 'DESIGN' },
            { 'id': Options.Barcode, 'name': 'BARCODE' },
            { 'id': Options.GLNNO, 'name': 'GLN' },
            { 'id': Options.Supplier, 'name': 'SUPPLIER' }
        ];

        $scope.AnalyserFilters.sort(function (a, b) {
            var x = a.name.toLowerCase();
            var y = b.name.toLowerCase();
            return x < y ? -1 : x > y ? 1 : 0;
        });
    }]);