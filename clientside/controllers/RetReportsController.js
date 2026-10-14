var app = angular.module('InfinityLab');

app.controller('RetReportController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'Excel', '$filter', '$stateParams', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, Excel, $filter, $stateParams, ngDialog) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Permission = UserDetails.Info;
        $scope.isb2b = $stateParams.type == 'B2B';
        $scope.reporttype = $stateParams.type;
        $scope.ReportType = [];
        $scope.SupportDealerPrice = false;
        $scope.AllReportType = [{ 'code': 'SALES', 'name': 'Sales Report' },
        { 'code': 'PRODUCTWISESALES', 'name': 'Productwise Sales Report' },
        { 'code': 'SALESRETURN', 'name': 'Sales Return Report' },
        { 'code': 'ADDITIONALDISC', 'name': 'Additional Discount Report' },
        { 'code': 'SETTLEMENT', 'name': 'Settlement Report' },
        { 'code': 'SETTLEMENTDETAIL', 'name': 'Settlement Detail Report' },
        { 'code': 'SALEVSSETTLEMENT', 'name': 'Sale Vs Settlement Report' },
        { 'code': 'CANCELLEDBILL', 'name': 'Cancelled Bill Report' },
        { 'code': 'BULKBILL', 'name': 'Bulk Bill Report' },
        { 'code': 'DELIVERY', 'name': 'Delivery Status Report' },
        { 'code': 'SALESTAX', 'name': 'Sales Tax Report' },
        { 'code': 'SALESHSN', 'name': 'Sales HSN Report' },
        { 'code': 'SALESBARCODE', 'name': 'Sales Barcode Report' },
        { 'code': 'SALESMAN', 'name': 'Sales man wise Report' },
        { 'code': 'STOCK', 'name': 'Stock Report' },
        { 'code': 'INWARD', 'name': 'Inward Report' },
        { 'code': 'CUSTOMERPAYMENT', 'name': 'Customer Payment Report' },
        { 'code': 'SALESANALYSIS', 'name': 'Sales Analysis Report' },
        { 'code': 'MISSINGSTOCK', 'name': 'Missing Stock Report' },
        { 'code': 'STOCKMARKER', 'name': 'Stock Marker Report' },
        { 'code': 'INCENT_SECTION', 'name': 'Incentive Report - Section' },
        { 'code': 'INCENT_EMPLOYEE', 'name': 'Incentive Report - Employees' },
        { 'code': 'PRICECHANGE', 'name': 'Price Changer Report' },
        { 'code': 'STOCKAUDIT', 'name': 'Stock Audit Report' }
        ];
        $scope.refpaytypelist = restService.Get('ReferencelistService/PAYCATEGORY', true).then(function (results) {
            $scope.refpaytypelist = results;

            $timeout(function () {
                $scope.refpaytypelist.find(function (item) {
                    if (item.name == 'ADVANCE' || item.code == 'ADVANCE' || item.name == 'ADV') {
                        if ($scope.Advance != null) $scope.Advance.salaryheaderid = item.id;
                        $scope.$digest();
                    }
                });
            }, 20);
        });

        $scope.GiftReports = [{ 'id': '0', 'name': 'ALL' },
        { 'id': '1', 'name': 'ISSUED' },
        { 'id': '2', 'name': 'NON-ISSUE' },
        { 'id': '3', 'name': 'CATEGORY SUMMARY' },
        { 'id': '4', 'name': 'GIFT SUMMARY' }];

        $scope.Modes = [{ 'code': 'All', 'name': 'All' },
        { 'code': 'local', 'name': 'Local' },
        { 'code': 'credit', 'name': 'Credit' },
        { 'code': 'interstate', 'name': 'Interstate' },
        { 'code': 'cash', 'name': 'Cash Sales' }];

        $scope.HSNSource = [{ 'id': 0, 'name': 'Bill Item' },
        { 'id': 1, 'name': 'Stock Item' },
        { 'id': 2, 'name': 'Product Master' }];

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

        $scope.BillType = [{ 'id': '0', 'name': 'PAID INVOICE' },
        { 'id': '1', 'name': 'CREDIT' },
        { 'id': '8', 'name': 'RETURN' },
        { 'id': '9', 'name': 'CANCEL' },
        { 'id': '10', 'name': 'APPROVED BILL' },
        { 'id': '20', 'name': 'B2B' },
        { 'id': '30', 'name': 'B2C' },
        { 'id': '-1', 'name': 'ALL' }];

        $scope.BillBarcodeSalesMode = [{ 'id': '0', 'name': 'ALL' },
        { 'id': '1', 'name': 'ONLY SALES' },
        { 'id': '2', 'name': 'ONLY SALES RETURN' },
        { 'id': '3', 'name': 'SALES RETURN VALIDATION' },
        { 'id': '4', 'name': 'VOID TRANSACTION' }];

        $scope.BillDisplayType = [{ 'id': '0', 'name': 'CASH' },
        { 'id': '1', 'name': 'CREDIT' },
        { 'id': '8', 'name': 'RETURN' },
        { 'id': '9', 'name': 'CANCEL' },
        { 'id': '10', 'name': 'APPROVED BILL' },
        { 'id': '11', 'name': 'COMPLETED APPROVED BILL' },
        { 'id': '12', 'name': 'RETURN BILL' },
        { 'id': '13', 'name': 'POS BILLINING' },
        { 'id': '14', 'name': 'POS BILLINING/RETURN' },
        { 'id': '15', 'name': 'POS BILLINING/REMAING' },
        { 'id': '16', 'name': 'CANCELLED APPROVED BILL' },
        { 'id': '20', 'name': 'ESTIMATED BILL' },
        { 'id': '21', 'name': 'COMPLETED ESTIMATED BILL' },
        { 'id': '-1', 'name': 'ALL' }];

        $scope.DeliveryStatus = [{ 'id': '0', 'name': 'RECEIVED' },
        { 'id': '1', 'name': 'VERIFIED' },
        { 'id': '2', 'name': 'DELIVERED' },
        { 'id': '3', 'name': 'CANCELLED' },
        { 'id': '-9', 'name': 'BILLED' }];

        $scope.PaymentModeList = [{ 'id': 0, 'name': 'ALL' },
        { 'id': 1, 'name': 'CASH' },
        { 'id': 2, 'name': 'CARD' },
        { 'id': 3, 'name': 'CREDIT' },
        { 'id': 4, 'name': 'VOUCHER' },
        { 'id': 5, 'name': 'RETURN' },
        { 'id': 6, 'name': 'ADJUSTMENT' },
        { 'id': 7, 'name': 'DISCOUNT' },
        { 'id': 8, 'name': 'UPI' },
        { 'id': 10, 'name': 'CHEQUE' },
        { 'id': 9, 'name': 'MISMATCH ONLY' }];

        $scope.VoucherTypeList = [{ 'id': 0, 'name': 'ALL' },
        { 'id': 1, 'name': 'GIFT VOUCHER' },
        { 'id': 7, 'name': 'CREDIT NOTE' },
        { 'id': 8, 'name': 'CUSTOMER ADVANCE' }];

        //$scope.CreditPaymentModes = [{ 'id': 0, 'name': 'Inter Bank Transfer' },
        //{ 'id': 1, 'name': 'Cheque/DD' },
        //{ 'id': 2, 'name': 'Cash' },
        //{ 'id': 3, 'name': 'Multiple Cheque/DD' },
        //    { 'id': 4, 'name': 'Card' }];

        $scope.PaymentModes = [{ 'id': 1, 'name': 'Inter Bank Transfer' },
        { 'id': 2, 'name': 'Cheque/DD' },
        { 'id': 3, 'name': 'Cash' },
        { 'id': 4, 'name': 'Card' },
        { 'id': 5, 'name': 'UPI' }];

        $scope.CreditPaymentModes = [{ 'id': 0, 'name': 'Card' },
        { 'id': 1, 'name': 'Voucher' },
        { 'id': 2, 'name': 'Cash' },
        { 'id': 3, 'name': 'UPI' },
        { 'id': 4, 'name': 'Cheque/DD' }
        ];

        $scope.selectedcount = 0;
        $scope.UpdateBillItem = {
            hsncode: ""
        }

        $scope.UpdateSalesmanCode = {
            salesmanid: 0
        }

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.CardTypeList = restService.Get('ReferencelistService/CARDTYPES', true).then(function (results) {
            $scope.CardTypeList = results;
        });
        $scope.ReturnReason = restService.Get('ReferencelistService/RETURNREASON', true).then(function (results) {
            $scope.ReturnReason = results;
        });
        $scope.States = restService.Get('ReferencelistService/STATE', true).then(function (results) {
            $scope.States = results;
        });
        $scope.Floors = restService.Get('ReferencelistService/FLOOR', true).then(function (results) {
            $scope.Floors = results;
        });
        $scope.Bloodgroup = restService.Get('ReferencelistService/BLOODGROUP', true).then(function (results) {
            $scope.Bloodgroup = results;
        });
        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
        });
        $scope.RetailLocations = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            $scope.RetailLocations = results;
        });
        $scope.AllLocation = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.AllLocation = results;
        });
        $scope.Location = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Location = results;
        });
        $scope.Designations = restService.Get('ReferencelistService/DESIGNATION', true).then(function (results) {
            $scope.Designations = results;
        });
        $scope.Promotions = restService.Get('ReferencelistService/PROMOTIONS', true).then(function (results) {
            $scope.Promotions = results;
        });

        $scope.Employees = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });

        $scope.Supplier = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Supplier = results;
        });
        //$scope.Customer = restService.Get('ReferencelistService/CUSTOMER', true).then(function (results) {
        //    $scope.Customer = results;
        //});
        $scope.Section = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Section = results;
        });
        $scope.Counter = restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counter = results;
        });
        $scope.BankList = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            $scope.BankList = results;
            $scope.UPIProviderList = restService.Get('ReferencelistService/UPIPROVIDER', true).then(function (results) {
                if (results.length == 0) {
                    results.push({ 'id': -1, 'name': '-' });
                }
                $.each(results, function (k, v) {
                    $scope.BankList.push(v);
                });
            });
        });
        $scope.Product = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Product = results;
        });
        $scope.Brand = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brand = results;
        });
        $scope.ProductGroups = restService.Get('ReferencelistService/PRODUCTGROUPS', true).then(function (results) {
            $scope.ProductGroups = results;
        });
        $scope.MarkerType = restService.Get('ReferencelistService/MARKER', true).then(function (results) {
            $scope.MarkerType = results;
        });
        $scope.SalesCounter = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.SalesCounter = results;
        });
        $scope.Companies = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Companies = results;
        });
        $scope.CustomerCategory = restService.Get('ReferencelistService/CUSTOMERCATEGORY', true).then(function (results) {
            $scope.CustomerCategory = results;
        });
        $scope.Agent = restService.Get('ReferencelistService/AGENT', true).then(function (results) {
            $scope.Agent = results;
        });
        $scope.City = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.City = results;
        });
        $scope.SizeGroup = restService.Get('ReferencelistService/SIZEGROUP', true).then(function (results) {
            $scope.SizeGroup = results;
        });

        $scope.CustomerType = [{ 'id': 1, 'name': 'Employee' },
        { 'id': 2, 'name': 'Retail' },
        { 'id': 3, 'name': 'Provider' },
        { 'id': 4, 'name': 'Customer' },
        { 'id': 5, 'name': 'Tailoring' }];

        $scope.PurchaseType = [{ 'id': 1, 'name': 'Local' },
        { 'id': 2, 'name': 'Interstate' }];

        $scope.RangeType = [{ 'id': 1, 'name': 'Stock Age' },
        { 'id': 2, 'name': 'Sale Price' },
        { 'id': 3, 'name': 'MRP' }];

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

        $scope.ModesList = [{ 'id': 1, 'name': 'BARCODE' },
        { 'id': 2, 'name': 'PRODUCT' },
        { 'id': 3, 'name': 'DATEWISE' }];

        $scope.Material = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.Material = results;
        });
        $scope.Pattern = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.Pattern = results;
        });
        $scope.Style = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.Style = results;
        });
        $scope.Type = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.Type = results;
        });
        $scope.Sleeve = restService.Get('ReferencelistService/SLEEVE', true).then(function (results) {
            $scope.Sleeve = results;
        });
        $scope.Colour = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.Colour = results;
        });
        $scope.PriceTag = restService.Get('ReferencelistService/PRICETAGS', true).then(function (results) {
            $scope.PriceTag = results;
        });
        $scope.SellingMode = [{ 'id': 1, 'name': 'Nos' }, { 'id': 2, 'name': 'Meters' }];

        $scope.billcount = 0;
        $scope.totalcash = 0.0;
        $scope.totalcard = 0.0;
        $scope.totalupi = 0.0;
        $scope.totalvoucher = 0.0;
        $scope.totalcredit = 0.0;
        $scope.totaladjustment = 0.0;
        $scope.saleqty = 0.0;
        $scope.saleamount = 0.0;
        $scope.saletax = 0.0;
        $scope.salediscount = 0.0;
        $scope.salecredit = 0;
        $scope.saletax = 0;
        $scope.returnamount = 0;
        $scope.cancelledamount = 0;
        $scope.saleadditionaldiscount = 0;
        $scope.totalcount = 0;
        $scope.totalsettlcount = 0;
        $scope.totalunsettlcount = 0;
        $scope.totalamt = 0;
        $scope.totalqty = 0;
        $scope.totaldisc = 0;

        $scope.SearchCondition = {
            fromdate: null,
            todate: null,
            companyid: 0,
            supplierid: 0,
            locationid: 0,
            counterid: 0,
            customerid: 0,
            unsettled: false,
            showdetail: false,
            showsalesummary: false,
            summary: true,
            barcode: "",
            productid: 0,
            brandid: 0,
            materialid: 0,
            patternid: 0,
            styleid: 0,
            sizeid: 0,
            designid: "",
            billno: "",
            packageno: "",
            customername: "",
            billerwise: "",
            cashierwise: "",
            setno: "",
            markerid: 0,
            mode: 1,
            schemeid: "",
            workingmode: "",
            schedulemonth: ''
        };

        $scope.ChangeReport = function () {
            $scope.SearchResult = [];
            $scope.SalesResult = [];
            $scope.SalesMarginResult = [];
            $scope.SalesReturnResult = [];
            $scope.SettlementResult = [];
            $scope.SettlementSummaryResult = [];
            $scope.AdvanceCollectionResult = [];
            $scope.CreditSaleCollectionResult = [];
            $scope.SettlementDetailResult = [];
            $scope.SaleVsSettlementResult = [];
            $scope.DeliveryStatusResult = [];
            $scope.DeliveryResult = [];
            $scope.InwardResult = [];
            $scope.BarcodeResult = [];
            $scope.StockResult = [];
            $scope.CashcounterResult = [];
            $scope.SalesTaxResult = [];
            $scope.SalesTaxSummaryResult = [];
            $scope.InvoicewiseSalesResult = [];
            $scope.CancelledBillResult = [];
            $scope.CustomerPaymentResult = [];
            $scope.CustomerCreditNoteResult = [];
            $scope.SalesmanResult = [];
            $scope.SalesmanDetailResult = [];
            $scope.SalesHSNResult = [];
            $scope.SalesBarcodeResult = [];
            $scope.SalesAnalysisResult = [];
            $scope.BulkBillResult = [];
            $scope.EstimateBillResult = [];
            $scope.SalesPerDayList = [];
            $scope.BillwiseDiscountList = [];
            $scope.SettlementwiseDiscountList = [];
            $scope.MissingStockResult = [];
            $scope.StockMarkerList = [];
            $scope.PriceChangerList = [];
            $scope.PriceChangerItemList = [];
            $scope.InwardDetailsResult = [];
            $scope.StockAuditList = [];
            $scope.DaySummary = [];
            $scope.RewardConsumptionResult = [];
            $scope.SearchCondition.unsettled = false;
            $scope.SearchCondition.showdetail = false;
            $scope.SearchCondition.summary = true;
        }

        $scope.SettlementReconcilationResult = [];
        $scope.DaySummary = [];
        $scope.SearchResult = [];
        $scope.SalesResult = [];
        $scope.SalesMarginResult = [];
        $scope.SalesReturnResult = [];
        $scope.SettlementResult = [];
        $scope.SettlementSummaryResult = [];
        $scope.AdvanceCollectionResult = [];
        $scope.CreditSaleCollectionResult = [];
        $scope.SettlementDetailResult = [];
        $scope.SaleVsSettlementResult = [];
        $scope.DeliveryStatusResult = [];
        $scope.DeliveryResult = [];
        $scope.InwardResult = [];
        $scope.BarcodeResult = [];
        $scope.StockResult = [];
        $scope.CashcounterResult = [];
        $scope.SalesTaxResult = [];
        $scope.SalesTaxSummaryResult = [];
        $scope.InvoicewiseSalesResult = [];
        $scope.CancelledBillResult = [];
        $scope.CustomerPaymentResult = [];
        $scope.CustomerCreditNoteResult = [];
        $scope.SalesmanResult = [];
        $scope.SalesmanDetailResult = [];
        $scope.SalesHSNResult = [];
        $scope.SalesBarcodeResult = [];
        $scope.SalesAnalysisResult = [];
        $scope.SalesPerDayList = [];
        $scope.BillwiseDiscountList = [];
        $scope.SettlementwiseDiscountList = [];
        $scope.MissingStockResult = [];
        $scope.StockMarkerList = [];
        $scope.PriceChangerList = [];
        $scope.PriceChangerItemList = [];
        $scope.InwardDetailsResult = [];
        $scope.Incentives = [];
        $scope.StockAuditList = [];
        $scope.SearchResult = [];
        $scope.EstimateBillResult = [];
        $scope.RewardConsumptionResult = [];

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;

            var subtitle = ''
            if ($scope.SearchCondition.companyid != null && $scope.SearchCondition.companyid > 0)
                title = $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company);

            if (tableid == 'salesreport') {

                if ($scope.SearchCondition.mode == '66') {
                    var loc = $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location);

                    var fromdate = $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy');
                    var todate = $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');
                    var subtitle = loc + " Day Summary for " + (fromdate == todate ? fromdate : (fromdate + " to " + todate));
                }
                else {
                    subtitle = 'SALES REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');
                }
            }
            if (tableid == 'unsettledbill')
                subtitle = 'UN-SETTLED BILLS FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');
            if (tableid == 'salestaxreport' || tableid == 'salesbytaxreport' || tableid == 'salesbytaxpaymentreport')
                subtitle = 'SALES TAX REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ";
            if (tableid == 'salesmanwise')
                subtitle = 'SALES MAN WISE SALE REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ";
            if (tableid == 'salesmargin')
                subtitle = 'SALES MARGIN REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');
            if (tableid == 'salesinvoicereport')
                subtitle = 'SALES REPORT - INVOICE WISE FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');
            if (tableid == 'salesbarcodereport')
                subtitle = 'BARCODE WISE SALE REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ";
            if (tableid == 'cancelledsalereport')
                subtitle = 'CANCELLED SALES BILL REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ";
            if (tableid == 'settlementreport')
                subtitle = 'SETTLEMENT REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ";
            if (tableid == 'customerpaymentreport')
                subtitle = 'CUSTOMER PAYMENT REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');
            if (tableid == 'saleshsnreport')
                subtitle = 'SALE TAX REPORT - HSN WISE FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ";
            if (tableid == 'salesanalysis')
                subtitle = 'SALES ANALYSIS MATRIX REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ";
            if (tableid == 'salevssettlementreport')
                subtitle = 'SALE VS SETTLEMENT REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ";
            if (tableid == 'stockreport')
                subtitle = 'RETAIL STOCK REPORT ' + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ";
            if (tableid == 'missingstock')
                subtitle = 'DIRECT STOCK ENTRY REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ";
            if (tableid == 'stockmarker')
                subtitle = 'STOCK MARKER REPORT FROM ' + " ( " + $scope.SearchRef($scope.SearchCondition.markerid, $scope.MarkerType) + " ) ";
            if (tableid == 'pricechange')
                subtitle = 'PRICE CHANGE HISTORY REPORT FROM ' + " ( " + $scope.SearchRef($scope.SearchCondition.markerid, $scope.MarkerType) + " ) ";
            if (tableid == 'stockaudit')
                subtitle = 'STOCK AUDIT REPORT FROM ' + " ( " + $scope.SearchRef($scope.SearchCondition.markerid, $scope.MarkerType) + " ) ";
            if (tableid == 'addisc')
                subtitle = 'ADDITIONAL DISCOUNT REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ";
            if (tableid == "incentivetbl")
                subtitle = 'INCENTIVE REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');

            if (tableid == "salesreportdet")
                subtitle = 'DETAILED SALES REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');
            if (tableid == 'salesreturn')
                subtitle = 'SALES RETURN REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ";
            if (tableid == 'inwardreportsum')
                subtitle = 'INWARD SUMMARY REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');
            if (tableid == 'inwardreportdet')
                subtitle = 'DETAILED INWARD REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');
            if (tableid == 'cancelledbillreport')
                subtitle = 'CANCELLED BILL REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ";
            if (tableid == 'productwisesale')
                subtitle = 'PRODUCT WISE SALES REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ";
            if (tableid == 'bulkbillreport')
                subtitle = 'BULK BILL REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ";
            if (tableid == 'deliveryreport')
                subtitle = 'DELIVERY STATUS REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ";
            if (tableid == 'creditsalereport')
                subtitle = 'CREDIT SALE COLLECTION REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ";
            if (tableid == 'advancereport')
                subtitle = 'CUSTOMER ADVANCE COLLECTION REPORT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ";
            if (tableid == 'emistatement')
                subtitle = 'EMI STATEMENT FROM ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ";
            if (tableid == "employeedetail")
                subtitle = 'EMPLOYEE DETAILS REPORT';

            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, title, subtitle);
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
        $scope.WorkingModes = [{ 'id': 1, 'name': 'Temporary' }, { 'id': 2, 'name': 'Probation' }, { 'id': 3, 'name': 'Permanant' }];
        $scope.SalaryModes = [{ 'id': 0, 'name': 'Monthly' }, { 'id': 1, 'name': 'Dailywages' }];

        $scope.exportExcel = function (tableid, title, removeactioncolumn) {
            if (tableid == "salesreportdet")
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'DETAILED SALES REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == "incentivetbl")
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'INCENTIVE REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'salesreport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'SALES REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'salesreturn')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'SALES RETURN REPORT FOR- ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'settlementreport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'SETTLEMENT REPORT FOR- ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'salevssettlementreport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'SALE VS SETTLEMENT REPORT FOR- ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'inwardreportsum')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'INWARD SUMMARY REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'inwardreportdet')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'DETAILED INWARD REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'stockreport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'STOCK REPORT ' + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'salestaxreport' || tableid == 'salesbytaxreport' || tableid == 'salesbytaxpaymentreport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'SALES TAX REPORT FOR- ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'cancelledbillreport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'CANCELLED BILL REPORT FOR- ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'customerpaymentreport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'CUSTOMER PAYMENT REPORT FOR- ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'salesmanwise')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'SALES MAN WISE SALE REPORT FOR- ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'saleshsnreport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'HSN CODE WISE SALE REPORT FOR- ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'salesbarcodereport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'BARCODE WISE SALE REPORT FOR- ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'salesanalysis')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'SALES ANALYSIS MATRIX REPORT FOR- ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'productwisesale')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'PRODUCT WISE SALES REPORT FOR- ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'addisc')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'ADDITIONAL DISCOUNT REPORT FOR- ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'bulkbillreport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'BULK BILL REPORT FOR- ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'deliveryreport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'DELIVERY STATUS REPORT FOR- ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'missingstock')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'DIRECT STOCK ENTRY REPORT FOR- ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'stockmarker')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'STOCK MARKER REPORT FOR- ' + " ( " + $scope.SearchRef($scope.SearchCondition.markerid, $scope.MarkerType) + " ) ");
            if (tableid == 'daysummary') {
                var title = 'DAY SUMMARY for ' + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + ' - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');
                if ($filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') == $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'))
                    title = 'DAY SUMMARY for ' + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + ' As On ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy');
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, title);
            }

            $timeout(function () {
                var link = document.createElement('a');
                link.download = title + ".xls";
                link.href = $scope.exportHref;
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }
        $scope.SearchDaySales = function () {
            if ($scope.SearchCondition.mode == "66") {

                if ($scope.SearchCondition.locationid > 0) {

                    var request = {
                        "item": $scope.SearchCondition,
                        "securitytoken": "String content"
                    }

                    var srequest = JSON.stringify(request);

                    $http({
                        method: 'POST',
                        url: "RetailReportService/SearchDaySummaryAutoSettlement",
                        data: srequest
                    }).success(function (result) {
                        $scope.DaySummary = result;

                        if ($scope.igrid != null)
                            $scope.igrid.destroy();

                        var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
                        igrid.options.showHeaderRow = true;

                        igrid.AddSimpleColumn('name', 'Detail', 250);
                        igrid.AddSimpleColumn('value', 'Value', 100).cssClass = 'rightalign';;

                        igrid.Initialize();
                        $scope.igrid = igrid;

                        var data = result.Items;
                        $scope.igrid.UpdateData(data);
                    }).error(function (result) {
                    });
                }
                else {
                    $rootScope.Toaster.Info("Please Select Location");
                }

            }
            else if ($scope.SearchCondition.mode == "20")
                $scope.SearchSalesTax();
            else if ($scope.SearchCondition.mode == "21")
                $scope.SearchSalesTaxSummary();
            else if ($scope.SearchCondition.mode == "22")
                $scope.SearchSettlement();
            else if ($scope.SearchCondition.mode == "23") {
                $scope.SearchCondition.product = true;
                $scope.SearchCondition.section = true;
                $scope.SearchSalesAnalysis();
            }
            else if ($scope.SearchCondition.mode == "24") {
                if ($scope.SearchCondition.locationid > 0) {

                    var request = {
                        "item": $scope.SearchCondition,
                        "securitytoken": "String content"
                    }

                    var srequest = JSON.stringify(request);

                    $http({
                        method: 'POST',
                        url: "RetailReportService/SearchDaySalesSummary",
                        data: srequest
                    }).success(function (result) {
                        $scope.SearchResult = result;
                        $scope.DaySalesSummaryConfig();
                    }).error(function (result) {
                    });
                }
            }
            else
                $rootScope.Toaster.Info("Please Select Report Options.");
        }
        $scope.SeachUnSettled = function () {
            $scope.SearchCondition.mode = "6";
            $scope.SearchCondition.unsettled = true;
            $scope.SearchCondition.summary = false;
            $scope.SearchSales();
        }
        $scope.SearchSummaryMobile = function () {
            $scope.SearchCondition.mode = "9";
            $scope.SearchSales();
        }
        $scope.SearchSales = function () {
            $scope.SearchCondition.isb2b = $scope.isb2b;

            if ($scope.Appsetting_Company &&
                (typeof ($scope.SearchCondition.companyid) == 'undefined' || $scope.SearchCondition.companyid < 0)) {
                $rootScope.Toaster.Info(" Please select company");
                return;
            }

            if ($scope.SearchCondition.mode == "66") {
                $scope.SearchDaySummary($scope.igrid);
                return;
            }

            if ($scope.SearchCondition.mode == "5" && $scope.SearchCondition.billno == "") {
                $rootScope.Toaster.Info("Please enter billno");
                return;
            }

            //if ($scope.SearchCondition.mode == "6" && ($scope.SearchCondition.locationid == 'undefined' || $scope.SearchCondition.locationid == null ||
            //    $scope.SearchCondition.locationid == 0)) {
            //    $rootScope.Toaster.Error('Please Select Location');
            //    return;
            //}

            $scope.SalesResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: $scope.SearchCondition.mode == "6" ? "RetailReportService/SearchUnsettledSales" : "RetailReportService/SearchSales",
                data: srequest
            }).success(function (result) {

                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.SalesResult = result;
                if ($scope.SearchCondition.mode == '7') {
                    $scope.SalesBillNoConfig();
                    return;
                }
                if ($scope.SearchCondition.mode == '8') {
                    $scope.SalesBillTaxValidation();
                    return;
                }
                if ($scope.SearchCondition.mode == '12') {
                    $scope.SalesBillWiseTaxSplit();
                    return;
                }
                if ($scope.SearchCondition.mode == '9') {
                    $scope.showlocationsummary = true;
                    $scope.LocationSummary = result;
                    return;
                }
                else {
                    $scope.LocationSummary = [];
                    $scope.showlocationsummary = false;
                }
                $scope.billcount = result.length;
                //$.each(result, function (k, v) {
                //    if (v.billtype == 9 && $scope.SearchCondition.billtype == -1) {
                //        v.saleqty = 0;
                //        v.saletax = 0;
                //        v.discount = 0;
                //        v.addlndiscount = 0;
                //        v.amount = 0;
                //    }
                //});
                $scope.SalesConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchSalesSnapshot = function () {

            $scope.SalesResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSalesSnapshot",
                data: srequest
            }).success(function (result) {

                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.SalesResult = result;
                $scope.SalesSnapshotConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchCreditSales = function () {
            $scope.CreditSalesList = [];
            $scope.SearchCondition.mode = 'credit';

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchCreditSalesTaxSummary",
                data: srequest
            }).success(function (result) {
                $scope.CreditSalesList = result;
                $scope.CreditSalesTaxConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchEMIStatement = function () {
            $scope.SalesResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MobileReportService/SearchEMIStatement",
                data: srequest
            }).success(function (result) {
                $scope.SalesResult = result;
                $scope.EMIStatementConfig();
            }).error(function (result) {
            });
        }
        $scope.EMIStatementConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SalesResult);

            igrid.AddFormatedColumn('settlementdate', 'Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('locationid', 'Branch Name', 120, $scope.Location);
            igrid.AddSimpleColumn('customername', 'Customer Name', 100);
            igrid.AddSimpleColumn('customergst', 'Customer GST', 100);
            igrid.AddSimpleColumn('customeraddress', 'Customer Address', 140);
            igrid.AddSimpleColumn('billno', 'Bill No', 100);
            igrid.AddSimpleColumn('barcode', 'IMEI', 140);
            igrid.AddSimpleColumn('productname', 'Product', 140);
            igrid.AddSimpleColumn('loanprovider', 'Ledger', 120);
            igrid.AddSimpleColumn('loanreferenceno', 'Ledger Ref.No', 120);
            igrid.AddFormatedColumn('qty', 'Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('receivable', 'Bill Amount', 120, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_cash', 'Cash Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_card', 'Card Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_upi', 'UPI Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('loanamount', 'Loan Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_credit', 'Credit Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_voucher', 'Voucher Amount', 100, 'N2', 2).total = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.SalesResult);
        }
        $scope.SearchECStatement = function () {
            $scope.SalesResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MobileReportService/SearchECStatement",
                data: srequest
            }).success(function (result) {
                $scope.SalesResult = result;
                $scope.ECStatementConfig();
            }).error(function (result) {
            });
        }
        $scope.ECStatementConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SalesResult);

            igrid.AddReferenceColumn('locationid', 'Branch Name', 120, $scope.Location);
            igrid.AddFormatedColumn('transferin', 'Transfer In', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('transferout', 'Transfer Out', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('topup', 'TopUp', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('recharged', 'Recharged', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('balance', 'Balance', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('rate', 'Sale Value', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_cash', 'Cash Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_card', 'Card Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_upi', 'UPI Amount', 100, 'N2', 2).total = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.SalesResult);
        }

        $scope.SearchBillItemDetails = function () {
            $scope.SalesResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MobileReportService/SearchBillItemDetails",
                data: srequest
            }).success(function (result) {
                $scope.SalesResult = result;
                $scope.BillItemDetailsConfig();
            }).error(function (result) {
            });
        }
        $scope.BillItemDetailsConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SalesResult);

            igrid.AddFormatedColumn('billdate', 'Bill Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('locationid', 'Branch Name', 120, $scope.Location);
            igrid.AddSimpleColumn('customername', 'Customer Name', 140);
            igrid.AddSimpleColumn('customergstno', 'Customer GST', 100);
            igrid.AddSimpleColumn('customeraddress', 'Customer Address', 140);
            igrid.AddSimpleColumn('billno', 'Bill No', 120);
            igrid.AddSimpleColumn('barcode', 'IMEI', 140);
            igrid.AddSimpleColumn('productname', 'Product', 140);
            igrid.AddFormatedColumn('qty', 'Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('price', 'Sale Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('buyingprice', 'Cost Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('additionaldiscount', 'Addln.Discount', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('receivable', 'Receivable', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxable', 'Taxable', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxpercentage', 'Tax %', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('cgst', 'CGST', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('sgst', 'SGST', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('igst', 'IGST', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('tax', 'Total Tax', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('billvalue', 'Bill Value', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('settlementdate', 'Paid On', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('settlementcode', 'Settlement Code', 120);
            igrid.AddSimpleColumn('loanprovider', 'Ledger', 120);
            igrid.AddSimpleColumn('loanreferenceno', 'Ledger Ref.No', 120);
            igrid.AddFormatedColumn('receivable', 'Bill Amount', 120, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_cash', 'Cash Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_card', 'Card Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_upi', 'UPI Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('loanamount', 'Loan Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_credit', 'Credit Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_voucher', 'Voucher Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('marginvalue', 'Margin', 80, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.SalesResult);
        }

        $scope.SearchSchemeDetails = function () {
            $scope.SchemeDetailList = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MobileReportService/SearchSchemeDetails",
                data: srequest
            }).success(function (result) {
                $scope.SchemeDetailList = result;
                $scope.SchemeDetailConfig();
            }).error(function (result) {
            });
        }

        $scope.SchemeDetailConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SchemeDetailList);

            if ($scope.SearchCondition.mode == "1") {
                igrid.AddReferenceColumn('locationid', 'Location Name', 100, $scope.Location);
                igrid.AddFormatedColumn('billdate', 'Bill Date', 100, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('billno', 'Bill No', 100);
                igrid.AddSimpleColumn('brandname', 'Brand Name', 150);
                igrid.AddSimpleColumn('productname', 'Product Name', 150);                
                igrid.AddSimpleColumn('designid', 'Design', 150);
                igrid.AddSimpleColumn('barcode', 'Barcode', 100);
                igrid.AddSimpleColumn('serialno', 'EAN Code', 100);
                igrid.AddSimpleColumn('schemename', 'Scheme Name', 150);
                igrid.AddFormatedColumn('qty', 'Qty', 60, 'N2', 2).total = true;
                igrid.AddFormatedColumn('mrp', 'MRP', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('sellingprice', 'Selling Price', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('schemediscount', 'Scheme Discount', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('schemediscountpercent', 'Scheme Discount %', 80, 'N2', 2);
                igrid.AddFormatedColumn('netamount', 'Net Amount', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('nsvtax', 'NSV Tax', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('nsvmargin', 'NSV Margin', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('mrptax', 'MRP Tax', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('mrpmargin', 'MRP Margin', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('buyingprice', 'Cost', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('taxpercentage', 'Tax %', 60, 'N2', 2);
                igrid.AddFormatedColumn('tax', 'Tax Amount', 60, 'N2', 2).total = true;
                igrid.AddFormatedColumn('billvalue', 'Bill Value', 60, 'N2', 2).total = true;
                igrid.AddFormatedColumn('payble', 'Payble', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('cn', 'CN', 60, 'N2', 2).total = true;
            }
            if ($scope.SearchCondition.mode == "2") {
                igrid.AddSimpleColumn('productname', 'Model Name', 150);
                igrid.AddFormatedColumn('schemediscount', 'Scheme Discount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('billvalue', 'Bill Value', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('qty', 'Billed Qty', 100, 'N2', 2).total = true;
            }

            if ($scope.SearchCondition.mode == "3") {
                if ($scope.SearchCondition.schemeid > 0)
                    igrid.AddFormatedColumn('billdate', 'Bill Date', 100, 'dd-MM-yyyy');
                else
                    igrid.AddSimpleColumn('schemename', 'Scheme Name', 150);
                igrid.AddFormatedColumn('schemediscount', 'Scheme Discount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('billvalue', 'Bill Value', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('qty', 'Billed Qty', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('rounding', 'Round', 100, 'N2', 2).total = true;
            }

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.SchemeDetailList);
        }

        $scope.SearchSalesMargin = function () {

            $scope.SalesMarginResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSalesMargin",
                data: srequest
            }).success(function (result) {
                $scope.SalesMarginResult = result;
                $scope.SalesMarginConfig();
            }).error(function (result) {
            });
        }


        $scope.MissingStock = function () {
            //if ($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined'){
            //    $rootScope.Toaster.Info("Please select location");
            //    return;
            //}
            $scope.totalcount = 0;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchMissingStock",
                data: srequest
            }).success(function (result) {
                if (result != null) {
                    $scope.MissingStockResult = result;
                    $scope.totalcount = result.length;

                }
                $scope.MissingStockConfig();
            }).error(function (result) {
            });

        }

        $scope.StockMarker = function () {

            $scope.totalcount = 0;
            $scope.totalqty = 0;

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/StockMarker",
                data: srequest
            }).success(function (result) {
                if (result != null) {
                    $scope.StockMarkerList = result;
                    $scope.totalcount = result.length;
                    $.each(result, function (k, v) {
                        $scope.totalqty += v.qty;
                    });
                }
                $scope.StockMarkerConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchIncentives = function () {
            $scope.SearchCondition.showdetail = true;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchIncentive",
                data: srequest
            }).success(function (result) {
                if (result != null) {
                    $scope.Incentives = result;
                    $scope.totalcount = result.length;

                }
                $scope.IncentiveConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchEmployeeDetail = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeeService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.EmployeeConfig();
            }).error(function (result) {
            });

        }


        $scope.ProductWiseSale = function () {

            if ($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined') {
                $rootScope.Toaster.Info("Please select location");
                return;
            }

            $scope.totalcount = 0;
            $scope.totalqty = 0;
            $scope.totalamt = 0;
            $scope.totaldisc = 0;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchAdditionalDashboard",
                data: srequest
            }).success(function (result) {
                if (result != null) {
                    $scope.SalesPerDayList = result.lstsalevalue;
                    $scope.BillwiseDiscountList = result.lstbillwisediscount;
                    $scope.SettlementwiseDiscountList = result.lstsettlementwisediscount;
                    $scope.totalcount = result.lstsalevalue.length;
                    $.each(result.lstsalevalue, function (k, v) {
                        $scope.totalqty += v.qtytotal;
                    });

                    $.each(result.lstsalevalue, function (k, v) {
                        $scope.totalamt += v.receivetotal;
                    });

                    $.each(result.lstbillwisediscount, function (k, v) {
                        $scope.totaldisc += v.additionaldiscount;
                    });

                    $.each(result.lstsettlementwisediscount, function (k, v) {
                        $scope.totaldisc += v.additionaldiscount;
                    });
                }
                $scope.ProductConfig();
            }).error(function (result) {
            });

        }

        $scope.SearchAdditionalDiscount = function () {
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchAdditionalDiscount",
                data: srequest
            }).success(function (result) {
                if (result != null) {
                    $scope.BillwiseDiscountList = result.lstbillwisediscount;
                    $scope.SettlementwiseDiscountList = result.lstsettlementwisediscount;
                }
                $scope.AdDiscConfig();
            }).error(function (result) {
            });
        }

        $scope.CancelBills = function () {

            if ($scope.SearchCondition.showsalesummary != 'undefined'
                && $scope.SearchCondition.showsalesummary != null
                && $scope.SearchCondition.showsalesummary) {
                $rootScope.Toaster.Error('Please use Bill wise cancel option.');
                return;
            }

            var ids = [];

            var seletedRows = $scope.igrid.GetSelectedRows();

            $.each(seletedRows, function (k, v) {
                ids.push(v.id);
            });

            //$.each($scope.SalesResult, function (k, v) {
            //    if (v.selected && v.id > 0)
            //        ids.push(v.id);
            //})

            if (ids.length == 0) {
                $rootScope.Toaster.Error('Please select the bills to be cancelled');
                return;
            }
            if ($scope.SearchCondition.locationid == 'undefined' || $scope.SearchCondition.locationid == null ||
                $scope.SearchCondition.locationid == 0) {
                $rootScope.Toaster.Error('Please Select the Location and Search Again. Cannot Cancel bills from multiple Locations.');
                return;
            }

            request = {
                "item": { 'ids': ids, 'locationid': $scope.SearchCondition.locationid },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillService/Cancellation",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Notice('Cancellation completed, total bills ' + result.length);
            }).error(function (result) {
            });

        };

        $scope.CancelEstimateBills = function () {

            var ids = [];
            var seletedRows = $scope.igrid.GetSelectedRows();

            $.each(seletedRows, function (k, v) {
                if (v.billtype == 9 || v.billtype == 21) {
                    $rootScope.Toaster.Error('Some of the selected bills are either completed or cancelled.');
                    return;
                }
                ids.push(v.id);
            });

            if (ids.length == 0) {
                $rootScope.Toaster.Error('Please select the bills to be cancelled');
                return;
            }
            if ($scope.SearchCondition.locationid == 'undefined' || $scope.SearchCondition.locationid == null ||
                $scope.SearchCondition.locationid == 0) {
                $rootScope.Toaster.Error('Please Select the Location and Search Again. Cannot Cancel bills from multiple Locations.');
                return;
            }

            request = {
                "item": {
                    'ids': ids, 'locationid': $scope.SearchCondition.locationid, 'isbulksale': true
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillService/CancelEstimate",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Notice('Cancellation completed, total bills ' + result.length);
            }).error(function (result) {
            });

        };

        $scope.SelectAll = function (selected) {
            $.each($scope.SalesResult, function (k, v) {
                v.selected = selected;
            })
        };

        $scope.SearchSalesReturn = function () {
            $scope.SalesReturnResult = [];
            if (($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
                $rootScope.Toaster.Info("Please select location");
                return;
            }
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSalesReturn",
                data: srequest
            }).success(function (result) {
                $scope.SalesReturnResult = result;
                $scope.returnamount = 0;
                $.each(result, function (k, v) {
                    $scope.returnamount += v.amount;
                });
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.billcount = result.length;
                $scope.SalesRetConfig();
            }).error(function (result) {
            });
        }


        $scope.SearchSettlementDaySummary = function () {

            if (($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')
                && $scope.Location.length != $scope.Locations.length) {
                $rootScope.Toaster.Info("Please select location");
                return;
            }

            $scope.SettlementDaySummaryResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SettlementSummaryByDay",
                data: srequest
            }).success(function (result) {
                $scope.SettlementDaySummaryResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.SettlementDaySummaryConfig();

            }).error(function (result) {
            });
        }



        $scope.SearchSettlementSummary = function () {

            $scope.SettlementSummaryResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SettlementSummaryByCompany",
                data: srequest
            }).success(function (result) {
                $scope.SettlementSummaryResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.SettlementSummaryConfig();

            }).error(function (result) {
            });
        }

        $scope.SearchSettlementReconcilation = function () {

            $scope.SettlementReconcilationResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSettlementReconcilation",
                data: srequest
            }).success(function (result) {
                $scope.SettlementReconcilationResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.SettlementReconcilationConfig();

            }).error(function (result) {
            });
        }



        $scope.SearchSettlement = function () {

            $scope.SettlementResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSettlement",
                data: srequest
            }).success(function (result) {
                $scope.SettlementResult = result;
                $scope.totalcash = 0;
                $scope.totalcard = 0;
                $scope.totalupi = 0;
                $scope.totalcredit = 0;
                $scope.totaladjustment = 0;
                $scope.returnamount = 0;
                $scope.totalamt = 0;
                $scope.totalsettlcount = 0;
                $.each(result, function (k, v) {
                    $scope.totalcash += v.paid_cash;
                    $scope.totalcard += v.paid_card;
                    $scope.totalupi += v.paid_upi;
                    $scope.totalcredit += v.paid_credit;
                    $scope.totaladjustment += v.adjustment_paid;
                    $scope.returnamount += v.return_cash;
                    $scope.totalamt = $scope.totalcash + $scope.totalcard + $scope.totalcredit;
                    $scope.totalcount = $scope.totalamt - $scope.returnamount;
                });
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.SettlementConfig();

            }).error(function (result) {
            });
        }

        $scope.SearchSettlementDetail = function () {
            if (($scope.SearchCondition.setno == null || $scope.SearchCondition.setno == "")
                && ($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
                $rootScope.Toaster.Info("Please select location or setno");
                return;
            }

            $scope.SettlementDetailResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSettlementDetail",
                data: srequest
            }).success(function (result) {
                $scope.SettlementDetailResult = result;
                $scope.totalcash = 0;
                $scope.totalcard = 0;
                $scope.totalupi = 0;
                $scope.totalcredit = 0;
                $scope.totaladjustment = 0;
                $scope.returnamount = 0;
                $.each(result, function (k, v) {
                    $scope.totalcash += v.paid_cash;
                    $scope.totalcard += v.paid_card;
                    $scope.totalupi += v.paid_upi;
                    $scope.totalcredit += v.paid_credit;
                    $scope.totaladjustment += v.adjustment_paid;
                    $scope.returnamount += v.return_cash;
                });
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.SettlementDetailConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchCashierSettlementDetail = function () {
            if (($scope.SearchCondition.setno == null || $scope.SearchCondition.setno == "")
                && ($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
                $rootScope.Toaster.Info("Please select location or setno");
                return;
            }

            $scope.SettlementDetailResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSettlementDetail",
                data: srequest
            }).success(function (result) {
                $scope.SettlementDetailResult = result;
                $scope.totalcash = 0;
                $scope.totalcard = 0;
                $scope.totalupi = 0;
                $scope.totalcredit = 0;
                $scope.totaladjustment = 0;
                $scope.returnamount = 0;
                $.each(result, function (k, v) {
                    $scope.totalcash += v.paid_cash;
                    $scope.totalcard += v.paid_card;
                    $scope.totalupi += v.paid_upi;
                    $scope.totalcredit += v.paid_credit;
                    $scope.totaladjustment += v.adjustment_paid;
                    $scope.returnamount += v.return_cash;
                });
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.SettlementDetailConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchSettlementSMachine = function () {

            $scope.SettlementResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MobileReportService/SearchSettlementSMachine",
                data: srequest
            }).success(function (result) {
                $scope.SettlementResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.SettlementSMachineConfig();

            }).error(function (result) {
            });
        }
        $scope.SettlementSMachineConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SettlementResult);

            igrid.AddReferenceColumn('bankid', 'Swiping Machine', 100, $scope.BankList);
            igrid.AddSimpleColumn('employeename', 'TID', 100);
            igrid.AddSimpleColumn('counter', 'Bill No', 100);
            if ($scope.SearchCondition.showdetail == true) {
                igrid.AddSimpleColumn('customername', 'Customer Name', 100);
                igrid.AddSimpleColumn('operatorname', 'Operator Name', 100);
            }
            igrid.AddFormatedColumn('receiptdate', 'Bill Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('receivable', 'Bill Amount', 120, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_card', 'Card Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_upi', 'UPI Amount', 100, 'N2', 2).total = true;




            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.SettlementResult);
        }

        $scope.SearchSaleVsSettlement = function () {
            if (($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
                $rootScope.Toaster.Info("Please select location");
                return;
            }

            $scope.SaleVsSettlementResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSaleVsSettlement",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                    return;
                }

                $scope.SaleVsSettlementResult = result;
                $scope.SalesSettlementConfig();

            }).error(function (result) {
            });
        }


        $scope.SearchSalesVsPurchaseVsStock = function () {

            $scope.SearchSaleVsStockResult = [];
            var hascondition = ($scope.SearchConditionStkTrans.productid && $scope.SearchConditionStkTrans.productid > 0);
            hascondition = hascondition || ($scope.SearchConditionStkTrans.supplierid && $scope.SearchConditionStkTrans.supplierid > 0);
            hascondition = hascondition || ($scope.SearchConditionStkTrans.sectionid && $scope.SearchConditionStkTrans.sectionid > 0);
            hascondition = hascondition || ($scope.SearchConditionStkTrans.brandid && $scope.SearchConditionStkTrans.brandid > 0);
            hascondition = hascondition || ($scope.SearchConditionStkTrans.productgroupid && $scope.SearchConditionStkTrans.productgroupid > 0);
            
            if (!hascondition)
            {
                $rootScope.Toaster.Error("Please select minimum one condition");
                return;
            }

            var request = {
                "item": $scope.SearchConditionStkTrans,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSaleVsPurchaseVsStock",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                    return;
                }
                $scope.SearchSaleVsStockResult = result.items;
                $scope.SalesStokPurchaseConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchSaleVsStock = function () {

            $scope.SearchSaleVsStockResult = [];

            var request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            request.item.showsupplier = true;
            request.item.showproduct = true;
            request.item.showdesign = true;
            request.item.showcost = true;

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSaleVsStock",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                    return;
                }
                $scope.SearchSaleVsStockResult = result.items;
                $scope.SalesStokConfig();

            }).error(function (result) {
            });
        }

        $scope.ManageSettlement = function (action, item) {
            request = {
                "Item": { id: item.id },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillsettlementmasterService/Cancel",
                data: srequest
            }).success(function (result) {

                if (result) {
                    $scope.Toaster.Info("Successfully deleted");
                }

            }).error(function (result) {

            });
        }
        $scope.printInfyGrid = function (grid) {
            var strWindowFeatures = "menubar=yes,location=yes,resizable=yes,scrollbars=yes,status=yes";
            grid.grid.printPlugin.printToWindow(window.open('/print-grid.html', 'print_window', strWindowFeatures));
        }
        $scope.printSalesmanSalesReceipt = function () {
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSalesmanReceipt",
                data: srequest
            }).success(function (result) {
                if (result == null || result.lstsalesman == null || result.lstsalesman.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                else {
                    var list = [];
                    var total = 0;
                    var billsummary = result.lstbillsummary;
                    var salesmansummary = result.lstsalesman;
                    var countersummary = result.lstcountersummary;

                    $.each(result.lstcountersummary, function (k, v) {
                        total += v.amount;
                    });

                    if ($scope.SearchCondition.companyid == 0)
                        list.push($rootScope.PrintFormater.Bold($rootScope.UserInfo.CompanyName));
                    else
                        list.push($rootScope.PrintFormater.Bold($scope.SearchRef($scope.SearchCondition.companyid, $scope.Company)));
                    list.push($rootScope.PrintFormater.Bold("SALES REPORT"));
                    list.push($rootScope.PrintFormater.Normal("______________________________"));
                    list.push($rootScope.PrintFormater.Normal("  LOCATION     : " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location)));
                    list.push($rootScope.PrintFormater.Normal("  DATE  : " + $filter('date')($scope.SearchCondition.fromdate, 'dd-MM-yyyy') + "AND" + $filter('date')($scope.SearchCondition.todate, 'dd-MM-yyyy')));
                    list.push($rootScope.PrintFormater.Normal("______________________________"));
                    if (countersummary != null && countersummary.length > 0) {
                        list.push($rootScope.PrintFormater.Normal(" S.No  Counter  Amount "));
                        list.push($rootScope.PrintFormater.Normal("______________________________"));
                        $.each(countersummary, function (k, v) {
                            list.push($rootScope.PrintFormater.Normal($rootScope.PrintFormater.Align(v.sno, 4, false) + '  ' + $rootScope.PrintFormater.Align(v.empno, 5, false) + ' ' + $rootScope.PrintFormater.Align(v.amount, 10, true)));
                        })
                        list.push($rootScope.PrintFormater.Normal("______________________________"));
                        list.push($rootScope.PrintFormater.Normal("  Total : " + $rootScope.PrintFormater.Align(total, 10, true)));
                    }
                    list.push("\r\n");
                    list.push($rootScope.PrintFormater.Cutter());
                    list.push("\r\n");

                    if ($scope.SearchCondition.companyid == 0)
                        list.push($rootScope.PrintFormater.Bold($rootScope.UserInfo.CompanyName));
                    else
                        list.push($rootScope.PrintFormater.Bold($scope.SearchRef($scope.SearchCondition.companyid, $scope.Company)));
                    list.push($rootScope.PrintFormater.Bold("SALES REPORT"));
                    list.push($rootScope.PrintFormater.Normal("______________________________"));
                    list.push($rootScope.PrintFormater.Normal("  LOCATION     : " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location)));
                    list.push($rootScope.PrintFormater.Normal("  DATE  : " + $filter('date')($scope.SearchCondition.fromdate, 'dd-MM-yyyy') + "AND" + $filter('date')($scope.SearchCondition.todate, 'dd-MM-yyyy')));
                    list.push($rootScope.PrintFormater.Normal("______________________________"));
                    list.push($rootScope.PrintFormater.Normal(""));
                    if (billsummary != null && billsummary.length > 0) {
                        list.push($rootScope.PrintFormater.Normal(" S.No  Salesman  BillNo  Amount "));
                        list.push($rootScope.PrintFormater.Normal("______________________________"));
                        $.each(billsummary, function (k, v) {
                            list.push($rootScope.PrintFormater.Normal($rootScope.PrintFormater.Align(v.sno, 3, false) + '  ' + $rootScope.PrintFormater.Align(v.empno, 5, false) + ' ' + $rootScope.PrintFormater.Align(v.billno, 9, false) + ' ' + $rootScope.PrintFormater.Align(v.amount, 8, true)));
                        })
                        list.push($rootScope.PrintFormater.Normal("______________________________"));
                        list.push($rootScope.PrintFormater.Normal("  Total : " + $rootScope.PrintFormater.Align(total, 10, true)));

                    }
                    list.push($rootScope.PrintFormater.Normal(""));
                    list.push("\r\n");
                    list.push($rootScope.PrintFormater.Cutter());

                    if ($scope.SearchCondition.companyid == 0)
                        list.push($rootScope.PrintFormater.Bold($rootScope.UserInfo.CompanyName));
                    else
                        list.push($rootScope.PrintFormater.Bold($scope.SearchRef($scope.SearchCondition.companyid, $scope.Company)));
                    list.push($rootScope.PrintFormater.Bold("SALES REPORT"));
                    list.push($rootScope.PrintFormater.Normal("______________________________"));
                    list.push($rootScope.PrintFormater.Normal("  LOCATION     : " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location)));
                    list.push($rootScope.PrintFormater.Normal("  DATE  : " + $filter('date')($scope.SearchCondition.fromdate, 'dd-MM-yyyy') + "AND" + $filter('date')($scope.SearchCondition.todate, 'dd-MM-yyyy')));
                    list.push($rootScope.PrintFormater.Normal("______________________________"));
                    if (salesmansummary != null && salesmansummary.length > 0) {
                        list.push($rootScope.PrintFormater.Normal(" S.No  Salesman  Amount "));
                        list.push($rootScope.PrintFormater.Normal("______________________________"));
                        $.each(salesmansummary, function (k, v) {
                            list.push($rootScope.PrintFormater.Normal($rootScope.PrintFormater.Align(v.sno, 4, false) + '  ' + $rootScope.PrintFormater.Align(v.empno, 5, false) + ' ' + $rootScope.PrintFormater.Align(v.amount, 10, true)));
                        })
                        list.push($rootScope.PrintFormater.Normal("______________________________"));
                        list.push($rootScope.PrintFormater.Normal("  Total : " + $rootScope.PrintFormater.Align(total, 10, true)));
                    }
                    list.push("\r\n");
                    list.push($rootScope.PrintFormater.Cutter());
                    list.push("\r\n");

                    var printdocument = window.btoa(list.join('\r\n'));
                    $rootScope.Sent2Printer(printdocument);
                }
            }).error(function (result) {
            });
        }

        $scope.SearchInward = function () {
            $scope.totalcount = 0;
            $scope.totalqty = 0;

            $scope.InwardResult = [];
            if ($scope.SearchCondition.showdetail == false && $scope.SearchCondition.withlr == false
                && ($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == null)
                && ($scope.SearchCondition.supplierid == 0 || $scope.SearchCondition.supplierid == null)) {
                $rootScope.Toaster.Info("Please select Location or Supplier");
                return;
            }
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchInward",
                data: srequest
            }).success(function (result) {
                $scope.InwardResult = result;
                $scope.totalcount = result.length;
                $.each(result, function (k, v) {
                    $scope.totalqty += v.qty;
                });
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.InwardConfig();
            }).error(function (result) {
            });
        }
        $scope.SearchPriceChange = function () {
            $scope.totalcount = 0;
            $scope.totalqty = 0;

            $scope.PriceChangerList = [];
            //if (($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
            //    $rootScope.Toaster.Info("Please select location");
            //    return;
            //}
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchPriceChange",
                data: srequest
            }).success(function (result) {
                $scope.PriceChangerList = result;
                $scope.totalcount = result.length;

                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.PriceChangeConfig();

            }).error(function (result) {
            });
        }

        $scope.SearchStockAudit = function () {
            $scope.totalcount = 0;
            $scope.totalqty = 0;

            $scope.StockAuditList = [];
            if ($scope.reporttype == 'STOCKAUDITREP') {
                if (($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
                    $rootScope.Toaster.Info("Please select location");
                    return;
                }
            }
            $scope.SearchCondition.mode = $scope.reporttype == 'STOCKAUDITREP' ? 0 : 1;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchStockAudit",
                data: srequest
            }).success(function (result) {
                $scope.StockAuditList = result;
                $scope.totalcount = result.length;

                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.StockAuditConfig();

            }).error(function (result) {
            });
        }

        $scope.SearchSplitDetails = function () {
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSplitDetails",
                data: srequest
            }).success(function (result) {
                $scope.StockAuditList = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.StockSplitConfig();

            }).error(function (result) {
            });
        }


        $scope.ViewItem = function (action, item) {
            if (action == "VIEW") {
                $scope.PriceChangerItemList = item.itemlist;
                $scope.printDiv('itemview', 'ITEM VIEW');
            }
        }

        $scope.SearchInwardDetails = function () {

            if ($scope.SearchCondition.showdetail == false && $scope.SearchCondition.withlr == false
                && ($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == null)
                && ($scope.SearchCondition.supplierid == 0 || $scope.SearchCondition.supplierid == null)) {
                $rootScope.Toaster.Info("Please select Location or Supplier");
                return;
            }

            $scope.totalqty = 0;
            $scope.InwardDetailsResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchOutward",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                else {
                    $scope.InwardDetailsResult = result[0].items;
                }
                $scope.totalcount = result.length;
                $.each(result[0].items, function (k, v) {
                    $scope.totalqty += v.qty;
                });
                $scope.InwardDetailsConfig();
            }).error(function (result) {
            });
        }


        $scope.SearchBarcode = function () {
            $scope.BarcodeResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchBarcode",
                data: srequest
            }).success(function (result) {
                $scope.BarcodeResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
            }).error(function (result) {
            });
        }

        $scope.SearchStock = function () {
            $scope.StockResult = [];
            $scope.saleqty = 0;
            $scope.amount = 0;
            if (($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
                $rootScope.Toaster.Info("Please select location");
                return;
            }
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchStock",
                data: srequest
            }).success(function (result) {
                $scope.StockResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $.each(result, function (k, v) {
                    $scope.saleqty += v.qty;
                    $scope.amount += v.buyingprice * v.qty;
                });
                $scope.StockConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchShelfStockDetail = function () {
            $scope.SearchCondition.isbit = false;
            $scope.SearchShelfStock();
        }
        $scope.SearchShelfStockSummary = function () {
            $scope.SearchCondition.isbit = true;
            $scope.SearchShelfStock();
        }

        $scope.SearchShelfStock = function () {
            $scope.StockResult = [];
            if (($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
                $rootScope.Toaster.Info("Please select location");
                return;
            }
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchShelfStock",
                data: srequest
            }).success(function (result) {
                $scope.StockResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                if ($scope.SearchCondition.isbit == true)
                    $scope.ShelfStockSummaryConfig();
                else
                    $scope.ShelfStockDetailConfig();
                $scope.SearchCondition.isbit = false;

            }).error(function (result) {
            });
        }

        $scope.ShelfStockSummaryConfig = function () {
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
            igrid.AddFormatedColumn('whqty', 'WH Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('retailqty', 'Store Qty', 60, 'N2', 2).total = true;
            //igrid.AddFormatedColumn('basebuyingprice', 'Cost Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('buyingprice', 'Last Purchase Rate', 80, 'N2', 2);
            igrid.AddFormatedColumn('price', 'Last Sale Rate', 80, 'N2', 2);
            //igrid.AddFormatedColumn('ibuyingprice', 'Net Price', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('stockpercentage', 'Stock %', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('salespercentage', 'Sales %', 60, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.StockResult);
        }

        $scope.ShelfStockDetailConfig = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.StockResult);

            igrid.AddSimpleColumn('agentname', 'Agent Name', 100);
            igrid.AddSimpleColumn('suppliername', 'Supplier Name', 150);
            igrid.AddSimpleColumn('suppliercity', 'Supplier City', 100);
            igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            igrid.AddSimpleColumn('brandname', 'Brand', 100);
            igrid.AddSimpleColumn('designid', 'Design', 100);
            igrid.AddSimpleColumn('productname', 'Product Name', 150);
            igrid.AddSimpleColumn('sizename', 'Size', 60);
            igrid.AddFormatedColumn('iqty', 'Purchase Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('returnqty', 'Return Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('oqty', 'Sale Qty', 60, 'N2', 2).total = true;
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
            igrid.AddReferenceColumn('sectionid', 'Section', 120, $scope.SalesCounter);
            igrid.AddReferenceColumn('productgroupid', 'Product Group', 100, $scope.ProductGroups);
            igrid.AddSimpleColumn('materialname', 'Material', 90);
            igrid.AddSimpleColumn('patternname', 'Pattern', 90);
            igrid.AddSimpleColumn('stylename', 'Style', 90);
            igrid.AddSimpleColumn('colourname', 'Colour', 90);
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
            igrid.AddFormatedColumn('stockpercentage', 'Stock %', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('salespercentage', 'Sales %', 60, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.StockResult);
        }

        $scope.SearchAgeStock = function () {
            $scope.StockResult = [];
            if (($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
                $rootScope.Toaster.Info("Please select location");
                return;
            }
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchAgeStock",
                data: srequest
            }).success(function (result) {
                $scope.StockResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.AgeStockConfig();
                $scope.SearchCondition.isbit = false;

            }).error(function (result) {
            });
        }

        $scope.AgeStockConfig = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.StockResult);

            igrid.AddReferenceColumn('productid', 'Product', 150, $scope.Product);
            igrid.AddReferenceColumn('sectionid', 'Section', 150, $scope.SalesCounter);

            igrid.AddFormatedColumn('age1qty', 'Age1', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('age2qty', 'Age2', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('age3qty', 'Age3', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('age4qty', 'Age4', 100, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.StockResult);
        }

        $scope.SearchCashCounter = function () {
            $scope.CashcounterResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchCashCounter",
                data: srequest
            }).success(function (result) {
                $scope.CashcounterResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
            }).error(function (result) {
            });
        }

        $scope.SearchCancelledBill = function () {
            $scope.CancelledBillResult = [];
            //if (($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
            //    $rootScope.Toaster.Info("Please select location");
            //    return;
            //}

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchCancelledBill",
                data: srequest
            }).success(function (result) {
                $scope.CancelledBillResult = result;
                $scope.CancelledBillConfig();

            }).error(function (result) {
            });
        }

        $scope.SearchCustomerPayment = function () {
            $scope.totalcredit = 0;
            $scope.totalcash = 0;
            $scope.totaladjustment = 0;
            $scope.CustomerPaymentResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchCustomerPayment",
                data: srequest
            }).success(function (result) {
                $scope.CustomerPaymentResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }

                $.each(result, function (k, v) {
                    $scope.totalcredit += v.credit;
                    $scope.totalcash += v.paid;
                    $scope.totaladjustment += v.balance;
                });
                $scope.CustomerPaymentConfig();

            }).error(function (result) {
            });
        }

        $scope.SearchCustomerCreditNote = function () {
            $scope.CustomerCreditNoteResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CouponService/SearchCustomerCreditNote",
                data: srequest
            }).success(function (result) {
                $scope.CustomerCreditNoteResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.CustomerCreditNoteConfig();

            }).error(function (result) {
            });
        }

        $scope.getTaxSummary = function (item, taxitem) {
            if (item != null) {
                var value = item.items.find(function (fitem) {
                    return fitem.taxpercent == taxitem.taxpercent;
                });

                if (typeof (value) != 'undefined' && value != null) {
                    if (taxitem.type == 'VALUE')
                        return value.salevalue.round($scope.rounding);
                    else
                        return value.taxvalue.round($scope.rounding);
                }
                return "";
            }
            else {
                if (taxitem.type == 'VALUE')
                    return taxitem.salevalue.round($scope.rounding);
                else
                    return taxitem.taxvalue.round($scope.rounding);
            }
        };

        $scope.SearchSalesTaxFullSummary = function () {
            $scope.SalesTaxSummaryResult = [];


            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSalesTaxFullSummary",
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
                        exist = (JSON.parse(JSON.stringify(v)));
                        if (exist.taxamount > 0) {
                            exist.ramount = 0;
                            exist.rsgst = 0;
                            exist.rcgst = 0;
                            exist.rigst = 0;
                            exist.rtaxamount = 0;
                            exist.rtotal = 0;
                        }
                        else {
                            exist.ramount = exist.amount;
                            exist.rsgst = exist.sgst;
                            exist.rcgst = exist.cgst;
                            exist.rigst = exist.igst;
                            exist.rtaxamount = exist.taxamount;
                            exist.rtotal = exist.total;
                            exist.amount = 0;
                            exist.sgst = 0;
                            exist.cgst = 0;
                            exist.igst = 0;
                            exist.taxamount = 0;
                            exist.total = 0;
                        }
                        taxlist.push(exist);
                    }
                    else {
                        if (v.taxamount > 0) {
                            exist.amount = v.amount;
                            exist.sgst = v.sgst;
                            exist.cgst = v.cgst;
                            exist.igst = v.igst;
                            exist.taxamount = v.taxamount;
                            exist.total = v.total;
                        }
                        else {
                            exist.ramount = v.amount;
                            exist.rsgst = v.sgst;
                            exist.rcgst = v.cgst;
                            exist.rigst = v.igst;
                            exist.rtaxamount = v.taxamount;
                            exist.rtotal = v.total;
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
                        name: 'Sales',
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
                        name: 'Sales Return',
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
                        name: 'Net Sales',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });

                $.each(taxlist, function (k, v) {
                    v.samount = v.amount - v.ramount;
                    v.ssgst = v.sgst - v.rsgst;
                    v.scgst = v.cgst - v.rcgst;
                    v.sigst = v.igst - v.rigst;
                    v.staxamount = v.taxamount - v.rtaxamount;
                    v.stotal = v.total - v.rtotal;
                })
                igrid.columns = groupColumns;
                igrid.EnableTotal();
                igrid.Initialize();
                $scope.igrid = igrid;
                $scope.igrid.UpdateData(taxlist);


            }).error(function (result) {
            });
        }

        $scope.SearchSalesTaxSummary = function () {
            $scope.SalesTaxSummaryResult = [];
            $scope.SearchCondition.isb2b = $scope.isb2b;
            if ($scope.SearchCondition.mode == null || $scope.SearchCondition.mode == 0
                || $scope.SearchCondition.mode == 1 || $scope.SearchCondition.mode == '')
                $scope.SearchCondition.mode = "All";

            $scope.SearchCondition.showmarker = $scope.SearchCondition.markerid == 1;
            $scope.SearchCondition.showsalesummary = $scope.SearchCondition.markerid == 2;

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSalesTaxSummary",
                data: srequest
            }).success(function (result) {
                $scope.SalesTaxSummaryResult = result;
                if ($scope.SearchCondition.showsalesummary) {
                    $scope.SalesTaxSummaryConfig();
                }
                else {
                    if ($scope.SearchCondition.showmarker) {
                        var summary = [];
                        $.each($scope.SalesTaxSummaryResult, function (k, v) {

                            var exist = summary.find(function (item) {
                                return item.billdate == v.billdate &&
                                    item.companyid == v.companyid &&
                                    item.taxpercentage == v.taxpercentage &&
                                    item.locationid == v.locationid &&
                                    item.interstate == v.interstate;
                            });

                            if (exist && exist != null) {
                                exist.sgst += v.sgst;
                                exist.cgst += v.cgst;
                                exist.igst += v.igst;
                                exist.amount += v.amount;
                                exist.taxamount += v.taxamount;
                                exist.total += v.total;
                            }
                            else {
                                exist = (JSON.parse(JSON.stringify(v)));
                                summary.push(exist);
                            }
                        });
                        $scope.SalesTaxSummaryResult = summary;
                    }
                    $scope.SalesTaxDetailConfig();
                }
            }).error(function (result) {
            });
        }

        $scope.SearchSalesTaxHSNSummary = function () {
            $scope.SalesTaxSummaryResult = [];
            $scope.SearchCondition.isb2b = $scope.isb2b;
            if ($scope.SearchCondition.mode == null || $scope.SearchCondition.mode == 0
                || $scope.SearchCondition.mode == 1 || $scope.SearchCondition.mode == '')
                $scope.SearchCondition.mode = "All";

            $scope.SearchCondition.showmarker = $scope.SearchCondition.markerid == 1;
            $scope.SearchCondition.showsalesummary = $scope.SearchCondition.markerid == 2;

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSalesTaxHSNSummary",
                data: srequest
            }).success(function (result) {
                $scope.SalesTaxSummaryResult = result;
                $scope.SearchSalesTaxHSNSummaryConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchInvoicewiseSales = function () {
            $scope.InvoicewiseSalesResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchInvoicewiseSales",
                data: srequest
            }).success(function (result) {
                $scope.InvoicewiseSalesResult = result;
                $scope.InvoicewiseSalesConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchSalesTax = function () {
            $scope.SalesTaxResult = [];
            $scope.SearchCondition.isb2b = $scope.isb2b;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSalesTax",
                data: srequest
            }).success(function (result) {
                $scope.SalesTaxResult = result;
                $scope.SalesTaxConfig()
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
            }).error(function (result) {
            });
        }

        $scope.SearchSalesHSN = function () {
            $scope.SalesHSNResult = [];
            $scope.TaxHeaderHSN = [];
            $scope.hsccount = 0;
            $scope.totaltaxbleamount = 0;
            $scope.totaltaxamount = 0;
            $scope.toatlinvoiceamount = 0;

            $scope.SearchCondition.isb2b = $scope.isb2b;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSalesHSN",
                data: srequest
            }).success(function (result) {
                if (result == null) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                if ($scope.SearchCondition.showsalesummary) {
                    $scope.SalesHSNResult = result;
                    $scope.SalesHSNSummaryConfig();
                }
                else {
                    $scope.SalesHSNResult = result.poentry;
                    $scope.hsccount = result.poentry.length;
                    $scope.TaxHeaderHSN = result.taxhead;
                    $.each($scope.SalesHSNResult, function (k, v) {
                        $.each(v.taxbreakup, function (k, v) {
                            if (v.taxpercent != 100) {
                                $scope.totaltaxbleamount += v.taxableamount;
                                $scope.totaltaxamount += v.taxamount;
                                $scope.toatlinvoiceamount = ($scope.totaltaxbleamount + $scope.totaltaxamount);
                            }
                        });
                    });
                    $scope.SalesHSNConfig();
                }

            }).error(function (result) {
            });
        }


        $scope.SearchSalesSplitUp = function () {
            $scope.SalesTaxResult = [];
            $scope.SearchCondition.isb2b = $scope.isb2b;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            if ($scope.SearchCondition.showdetail) {

                $http({
                    method: 'POST',
                    url: "RetailReportService/SearchSettlementByTax",
                    data: srequest
                }).success(function (result) {
                    if ($scope.SearchCondition.showdetail &&
                        $scope.SearchCondition.paymentmodes) {
                        var SalesTaxModeResult = [];

                        var Convert2TaxLine = function (companyid, taxpercentage, settlementby, value) {
                            var taxable = taxpercentage > 0 ? (value / (100 + taxpercentage) * 100) : value;
                            var tax = value - taxable;
                            return {
                                'companyid': companyid, 'taxpercentage': taxpercentage, 'tax': tax,
                                'taxable': taxable, 'total': value, 'settlementby': settlementby
                            };
                        }

                        $.each(result, function (k, v) {
                            if (v.paid_cash > 0)
                                SalesTaxModeResult.push(Convert2TaxLine(v.companyid, v.taxpercentage, 'CASH', v.paid_cash));
                            if (v.paid_card > 0)
                                SalesTaxModeResult.push(Convert2TaxLine(v.companyid, v.taxpercentage, 'CARD', v.paid_card));
                            if (v.paid_credit > 0)
                                SalesTaxModeResult.push(Convert2TaxLine(v.companyid, v.taxpercentage, 'CREDIT', v.paid_credit));
                            if (v.paid_voucher > 0)
                                SalesTaxModeResult.push(Convert2TaxLine(v.companyid, v.taxpercentage, 'VOUCHER', v.paid_voucher));
                            if (v.paid_upi > 0)
                                SalesTaxModeResult.push(Convert2TaxLine(v.companyid, v.taxpercentage, 'UPI', v.paid_upi));
                            if (v.adjustment_paid > 0)
                                SalesTaxModeResult.push(Convert2TaxLine(v.companyid, v.taxpercentage, 'OTHERS', v.adjustment_paid));
                        });
                        $scope.itemtotaltax = 0;
                        $scope.itemtotaltaxable = 0;
                        $scope.itemtotal = 0;
                        $.each(SalesTaxModeResult, function (k, v) {
                            $scope.itemtotaltax += v.tax;
                            $scope.itemtotaltaxable += v.taxable;
                            $scope.itemtotal += v.total;
                        });
                        $scope.SalesTaxModeResult = SalesTaxModeResult;
                    }
                    else {
                        $scope.SalesTaxResult = result;
                    }
                    if (result == null || result.length == 0) {
                        $rootScope.Toaster.Info("No Results Found.");
                        return;
                    }
                }).error(function (result) {
                });
            }
            else {
                $http({
                    method: 'POST',
                    url: "RetailReportService/SearchSalesTax",
                    data: srequest
                }).success(function (result) {
                    $scope.SalesTaxResult = result;
                    //$scope.SalesTaxConfig()
                    if (result == null || result.length == 0) {
                        $rootScope.Toaster.Info("No Results Found.");
                        return;
                    }
                }).error(function (result) {
                });
            }

            // To get the collection information 
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "RetailReportService/SearchSettlement",
                data: srequest
            }).success(function (result) {
                $scope.totalcash = 0;
                $scope.totalcard = 0;
                $scope.totalupi = 0;
                $scope.totalcredit = 0;
                $scope.totalvoucher = 0;
                $scope.totaladjustment = 0;
                $scope.returnamount = 0;
                $scope.totalamt = 0;
                $scope.totalsettlcount = 0;
                $.each(result, function (k, v) {
                    $scope.totalcash += v.paid_cash;
                    $scope.totalcard += v.paid_card;
                    $scope.totalupi += v.paid_upi;
                    $scope.totalvoucher += v.paid_voucher;
                    $scope.totalcredit += v.paid_credit;
                    $scope.totaladjustment += v.adjustment_paid;
                    $scope.returnamount += v.return_cash;
                    $scope.totalamt = $scope.totalcash + $scope.totalupi + $scope.totalcard + $scope.totalcredit + $scope.totalvoucher;
                    $scope.totalcount = $scope.totalamt - $scope.returnamount;
                });
            }).error(function (result) {
            });
        }

        $scope.DownloadItemWiseSalesXslx = function () {
            var dtfrom = Date.parse($scope.SearchCondition.fromdate);
            var dtto = Date.parse($scope.SearchCondition.todate);
            $rootScope.Download("RetailReportService/DownloadSalesBarcode/"
                + $scope.UserInfo.OrganizationID + '/'
                + $filter('date')(dtfrom, 'dd-MM-yyyy') + '/'
                + $filter('date')(dtto, 'dd-MM-yyyy') + '/'
                + $scope.SearchCondition.companyid + '/'
                + $scope.SearchCondition.locationid + '/'
                + ($scope.SearchCondition.isb2b == 'undefined' || $scope.SearchCondition.isb2b == null ? 'false' : $scope.SearchCondition.isb2b)
                , 'dt' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MM-yyyy') + '.xlsx', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet');
        }

        $scope.SearchSalesBarcode = function () {
            $scope.SalesBarcodeResult = [];

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSalesBarcode",
                data: srequest
            }).success(function (result) {
                if (result == null) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.SalesBarcodeResult = result;
                if ($scope.SearchCondition.reporttype == "3") {
                    $scope.SalesValidateBarcodeConfig();
                }
                else {
                    $scope.SalesBarcodeConfig();
                }
            }).error(function (result) {
            });
        }

        $scope.SearchVoidBarcode = function () {
            $scope.SalesBarcodeResult = [];

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSalesVoid",
                data: srequest
            }).success(function (result) {
                if (result == null) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.SalesBarcodeResult = result;
                $scope.SalesVoidConfig();
            }).error(function (result) {
            });
        }
        $scope.SearchOpeningClosing = function () {

            $scope.OpeningClosing = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchOpeningClosing",
                data: srequest
            }).success(function (result) {
                $scope.OpeningClosing = result;

                $scope.OpeningClosingConfig();
            }).error(function (result) {
            });
        }
        $scope.OpeningClosingConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.OpeningClosing);

            igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.Location);
            igrid.AddReferenceColumn('counterid', 'Counter', 90, $scope.Counter);
            igrid.AddReferenceColumn('cashierid', 'Cashier', 100, $scope.Employees);
            igrid.AddReferenceColumn('paidbyemployeeid', 'Paid By', 100, $scope.Employees);
            igrid.AddReferenceColumn('receivedbyemployeid', 'Received By', 100, $scope.Employees);

            igrid.AddFormatedColumn('paidon', 'Paid On', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('returnedon', 'Returned On', 90, 'dd-MM-yyyy');

            igrid.AddFormatedColumn('openingamount', 'Opening Amt', 100, 'N2', 2).total = true;
            if ($scope.system_cashclosewithtaken)
                igrid.AddFormatedColumn('takenamount', 'Taken Amount', 100, 'N2', 2).total = true;

            igrid.AddFormatedColumn('closingamount', 'Cash Entered', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('closingcollected', 'Cash Collected', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('closingdifference', 'Cash Difference', 100, 'N2', 2).total = true;

            igrid.AddFormatedColumn('cardamount', 'Card Entered', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('cardcollected', 'Card Collected', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('carddifference', 'Card Difference', 100, 'N2', 2).total = true;

            igrid.AddFormatedColumn('upiamount', 'UPI Entered', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('upicollected', 'UPI Collected', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('upidifference', 'UPI Difference', 100, 'N2', 2).total = true;

            var column = igrid.AddCustomColumn('TEntered', 'T.Entered', 100, function (item, col) {
                return $filter('IndianNumber')(item.closingamount + item.cardamount + item.upiamount, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.closingamount + item.cardamount + item.upiamount;
            };

            var column = igrid.AddCustomColumn('TCollected', 'T.Collected', 100, function (item, col) {
                return $filter('IndianNumber')(item.closingcollected + item.cardcollected + item.upicollected, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.closingcollected + item.cardcollected + item.upicollected;
            };

            var column = igrid.AddCustomColumn('TDifference', 'T.Difference', 100, function (item, col) {
                return $filter('IndianNumber')(item.closingdifference + item.carddifference + item.upidifference, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.closingdifference + item.carddifference + item.upidifference;
            };

            igrid.AddFormatedColumn('creditnotereceived', 'Credit Note Rec', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('creditnoteissued', 'Credit Note Issued', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('expenses', 'Expenses Amt', 80, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.OpeningClosing);
        }


        $scope.SearchAdvanceCollection = function () {

            $scope.AdvanceCollectionResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerorderService/SearchAdvanceCollection",
                data: srequest
            }).success(function (result) {
                $scope.AdvanceCollectionResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.AdvanceCollectionConfig();

            }).error(function (result) {
            });
        }
        $scope.AdvanceCollectionConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.AdvanceCollectionResult);

            igrid.AddReferenceColumn('companyid', 'Company', 180, $scope.Company);
            igrid.AddSimpleColumn('location', 'Location', 120);
            igrid.AddSimpleColumn('employeename', 'Cashier', 120);
            igrid.AddSimpleColumn('customername', 'Customer', 120);
            igrid.AddReferenceColumn('mode', 'Payment Mode', 80, $scope.PaymentModes);
            igrid.AddFormatedColumn('paid_cash', 'Cash', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_card', 'Bank Transfer', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_upi', 'UPI', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_voucher', 'Cheque/DD', 100, 'N2', 2).total = true;;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.AdvanceCollectionResult);
        }

        $scope.SearchCreditSaleCollection = function () {

            $scope.CreditSaleCollectionResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerpaymentService/SearchCreditSaleCollection",
                data: srequest
            }).success(function (result) {
                $scope.CreditSaleCollectionResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.CreditSaleCollectionConfig();

            }).error(function (result) {
            });
        }
        $scope.CreditSaleCollectionConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.CreditSaleCollectionResult);

            igrid.AddReferenceColumn('companyid', 'Company', 180, $scope.Company);
            igrid.AddSimpleColumn('location', 'Location', 120);
            igrid.AddSimpleColumn('counter', 'Counter', 80);
            igrid.AddSimpleColumn('employeename', 'Cashier', 120);
            igrid.AddSimpleColumn('customername', 'Customer', 120);
            igrid.AddSimpleColumn('customerphoneno', 'Mobile No', 100);
            igrid.AddFormatedColumn('receiptdate', 'Payment On', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('count', 'Count', 80, 'N0', 0).total = true;
            igrid.AddFormatedColumn('receivable', 'Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 100, 'N2', 2).total = true;
            igrid.AddReferenceColumn('mode', 'Payment Mode', 80, $scope.CreditPaymentModes);
            igrid.AddSimpleColumn('billno', 'BIll Nos', 120);
            igrid.AddFormatedColumn('paid_cash', 'Cash', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_card', 'Bank Transfer', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_upi', 'UPI', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_voucher', 'Cheque/DD', 100, 'N2', 2).total = true;;
            igrid.AddFormatedColumn('adjustment_paid', 'Others', 100, 'N2', 2).total = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.CreditSaleCollectionResult);
        }

        $scope.SearchSalesman = function () {
            $scope.SalesmanResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSalesmanSales",
                data: srequest
            }).success(function (result) {
                $scope.SalesmanResult = result;
                $scope.saleqty = 0;
                $scope.saleamount = 0;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $.each(result, function (k, v) {
                    $scope.saleqty += v.qty;
                    $scope.saleamount += v.amount;
                });
                $scope.SalesManConfig();
            }).error(function (result) {
            });
        }
        $scope.SearchSalesmanDetail = function () {
            $scope.SalesmanDetailResult = [];
            if (($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
                $rootScope.Toaster.Info("Please select location");
                return;
            }
            if (($scope.SearchCondition.employeeid == 0 || $scope.SearchCondition.employeeid == 'undefined')
                && ($scope.SearchCondition.barcode == '' || $scope.SearchCondition.barcode == null)) {
                $rootScope.Toaster.Info("Please enter barcode or employee");
                return;
            }

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSalesmanDetailSales",
                data: srequest
            }).success(function (result) {
                $scope.SalesmanDetailResult = result;
                $scope.saleqty = 0;
                $scope.saleamount = 0;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $.each(result, function (k, v) {
                    $scope.saleqty += v.qty;
                    $scope.saleamount += v.amount;
                });
                $scope.SalesManDetailConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchSalesAnalysis = function () {
            if (!$scope.SearchCondition.product && !$scope.SearchCondition.brand && !$scope.SearchCondition.material
                && !$scope.SearchCondition.pattern && !$scope.SearchCondition.style && !$scope.SearchCondition.size
                && !$scope.SearchCondition.supplier && !$scope.SearchCondition.section && !$scope.SearchCondition.sourcesupplier)
                $scope.SearchCondition.product = true;

            $scope.totalqty = 0;
            $scope.amount = 0;
            $scope.SalesAnalysisResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSalesAnalysis",
                data: srequest
            }).success(function (result) {
                $scope.SalesAnalysisResult = result;
                $.each(result, function (k, v) {
                    $scope.totalqty += v.qty;
                    $scope.amount += v.price;
                });
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.SalesAnalysisCnfig();
            }).error(function (result) {
            });
        }

        $scope.SearchEstimateBill = function () {
            if (($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
                $rootScope.Toaster.Info("Please select location");
                $scope.SearchCondition.unsettled = false;
                return;
            }

            $scope.EstimateBillResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchEstimateBill",
                data: srequest
            }).success(function (result) {
                $scope.EstimateBillResult = result;
                $scope.SearchCondition.unsettled = false;

                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.EstimateBillConfig();

            }).error(function (result) {
            });
        }

        $scope.PrintPendingEstimateBill = function () {
            if (($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
                $rootScope.Toaster.Info("Please select location");
                $scope.SearchCondition.unsettled = false;
                return;
            }

            $scope.EstimateBillResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchEstimateBill",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                else {
                    var list = [];
                    if ($scope.SearchCondition.companyid == 0)
                        list.push($rootScope.PrintFormater.Bold($rootScope.UserInfo.CompanyName));
                    else
                        list.push($rootScope.PrintFormater.Bold($scope.SearchRef($scope.SearchCondition.companyid, $scope.Company)));
                    list.push($rootScope.PrintFormater.Bold(" ESTIMATE BILLS "));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    list.push($rootScope.PrintFormater.Normal(" LOCATION     : " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location)));
                    list.push($rootScope.PrintFormater.Normal(" DATE  : " + $filter('date')($scope.SearchCondition.fromdate, 'dd-MM-yyyy') + "AND" + $filter('date')($scope.SearchCondition.todate, 'dd-MM-yyyy')));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    if (result != null && result.length > 0) {
                        list.push($rootScope.PrintFormater.Normal(" COUNTER   BILL NO     QTY    AMOUNT  "));
                        list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                        $.each(result, function (k, v) {
                            list.push($rootScope.PrintFormater.Normal($rootScope.PrintFormater.Align(v.counter, 10, false)
                                + '  ' + $rootScope.PrintFormater.Align(v.estimatebillno, 10, false)
                                + ' ' + $rootScope.PrintFormater.Align(v.saleqty, 6, true)
                                + ' ' + $rootScope.PrintFormater.Align(v.amount, 6, true)));
                        })
                        list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    }
                    list.push("\r\n");
                    list.push($rootScope.PrintFormater.Cutter());
                    list.push("\r\n");

                    var printdocument = window.btoa(list.join('\r\n'));
                    $rootScope.Sent2Printer(printdocument);

                }

            }).error(function (result) {
            });
        }

        $scope.SearchBulkBill = function () {
            if (($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
                $rootScope.Toaster.Info("Please select location");
                $scope.SearchCondition.unsettled = false;
                return;
            }

            $scope.BulkBillResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchBulkBill",
                data: srequest
            }).success(function (result) {
                $scope.BulkBillResult = result;
                //$scope.SearchCondition.unsettled = false;
                $scope.saleqty = 0;
                $scope.saleamount = 0;
                $scope.billcount = 0;

                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.billcount = result.length;
                $.each(result, function (k, v) {
                    $scope.saleqty += v.saleqty;
                    $scope.saleamount += v.amount;
                });
                $scope.BulkBillConfig();

            }).error(function (result) {
            });
        }

        $scope.SearchRewardConsumption = function () {
            if (($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
                $rootScope.Toaster.Info("Please select location");
                $scope.SearchCondition.unsettled = false;
                return;
            }

            $scope.RewardConsumptionResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchRewardConsumption",
                data: srequest
            }).success(function (result) {
                $scope.RewardConsumptionResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.RewardConsumptionConfig();

            }).error(function (result) {
            });
        }

        $scope.RewardConsumptionConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.RewardConsumptionResult);

            igrid.AddSimpleColumn('location', 'Location', 120);
            igrid.AddSimpleColumn('customer', 'Customer Name', 100);
            igrid.AddSimpleColumn('mobileno', 'Mobile', 100);
            igrid.AddSimpleColumn('remarks', 'Loyalty No', 80);            
            igrid.AddSimpleColumn('billno', 'Bill No', 120);
            igrid.AddFormatedColumn('billdate', 'Bill Date', 120, 'dd-MM-yyyy hh:mm a');
            igrid.AddFormatedColumn('saleqty', 'Sale Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('saletax', 'Sale Tax', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('addlndiscount', 'Addln. Discount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('amount', 'Net Amount', 120, 'N2', 2).total = true;
            if ($scope.SearchCondition.showdetail)
                igrid.AddFormatedColumn('taxable', 'New Rewards', 120, 'N2', 2).total = true;
            else
                igrid.AddFormatedColumn('taxable', 'Claimed', 120, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.RewardConsumptionResult);
        }

        $scope.DownloadApprovalImagePDF = function () {

            var seletedRows = $scope.igrid.GetSelectedRows();
            var barcodes = [];
            $.each(seletedRows, function (k, v) {
                barcodes.push(v.barcode);
            });
            $scope.SearchCondition.barcodes = barcodes;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "RetailReportService/BulkBillPDF",
                data: srequest
            }).success(function (result) {
                if (result && result != null && result.length == 16) {
                    $rootScope.Download("RetailReportService/ImagePDFGeneration/" + result, 'images.pdf', 'application/pdf');
                    return;
                }
            });
        }

        $scope.TextDaySummary = '-';

        $scope.downloadText = function () {
            var blob = new Blob([$scope.TextDaySummary],
                { type: "text/plain;charset=utf-8" });
            saveAs(blob, "Report.txt");
        }

        $scope.printText = function () {

            var css = '<style type="text/css" media="print">  @page {      size: auto; margin-top: 20mm; margin-bottom: 20mm; }</style >';

            var divstyle = "<div style='position:absolute;background-color:red;top:0px;left:0px;width:100%;height:10px'>&nbsp;</div>";

            var style = 'style="' + "font-family: 'Courier New';" + '"';
            var w = window.open();
            w.document.write('<html>' + css + '<body ' + style + '>' + divstyle + '<pre>' + $scope.TextDaySummary + '</pre></body></html>');
            w.print();
            w.close();
        }

        $scope.SearchTextAllTransaction = function () {

            var textarea = $('.main-container');
            var height = textarea.height();
            $('#txtdaysummary').css("height", (height - 60))

            if (($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
                $rootScope.Toaster.Info("Please select location");
                return;
            }

            $scope.DeliveryResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchTextAllTransaction",
                data: srequest
            }).success(function (result) {
                $scope.TextDaySummary = result;
                $('#txtdaysummary').val(result);
            }).error(function (result) {
            });
        }

        $scope.SearchTextDaySummary = function () {

            var textarea = $('.main-container');
            var height = textarea.height();
            $('#txtdaysummary').css("height", (height - 60))

            if (($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
                $rootScope.Toaster.Info("Please select location");
                return;
            }

            $scope.DeliveryResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/TextDaySummary",
                data: srequest
            }).success(function (result) {
                $scope.TextDaySummary = result;
                $('#txtdaysummary').val(result);
            }).error(function (result) {
            });
        }

        $scope.SearchDeliveryReport = function () {
            if (($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
                $rootScope.Toaster.Info("Please select location");
                return;
            }

            $scope.DeliveryResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchDelivery",
                data: srequest
            }).success(function (result) {
                $scope.DeliveryResult = result;
                $scope.DeliveryConfig();

            }).error(function (result) {
            });
        }

        $scope.SearchAlterationReport = function () {
            if (($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
                $rootScope.Toaster.Info("Please select location");
                return;
            }

            $scope.DeliveryResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchAlteration",
                data: srequest
            }).success(function (result) {
                $scope.DeliveryResult = result;
                $scope.AlterationConfig();

            }).error(function (result) {
            });
        }

        $scope.SearchPendingAdvanceSummary = function () {

            $scope.AdvanceSummaryList = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            if ($scope.SearchCondition.employeeid > 0 && $scope.SearchCondition.summary) {
                $http({
                    method: 'POST',
                    url: "EmployeesalaryscheduleService/SearchAdvanceDetails",
                    data: srequest
                }).success(function (result) {
                    $scope.AdvanceSummaryList = result;
                }).error(function (result) {
                });
            }
            else {

                $http({
                    method: 'POST',
                    url: "EmployeesalaryscheduleService/SearchAdvanceSummary",
                    data: srequest
                }).success(function (result) {
                    $scope.AdvanceSummaryConfig(result);
                }).error(function (result) {
                });
            }
        }

        $scope.AdvanceSummaryConfig = function (result) {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '500px', '100%', false, false, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('employeecode', 'EMPLOYEE CODE', 120);
            igrid.AddSimpleColumn('name', 'EMPLOYEE NAME', 230);
            igrid.AddSimpleColumn('description', 'DESCRIPTION', 230);
            igrid.AddFormatedColumn('openingamt', 'OPENING', 180, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid', 'EMPLOYEE RECEIVED', 180, 'N2', 2).total = true;
            igrid.AddFormatedColumn('receivable', 'EMPLOYEE PAID', 180, 'N2', 2).total = true;
            igrid.AddFormatedColumn('pendingamt', 'BALANCE', 180, 'N2', 2).total = true;
            igrid.EnableTotal();

            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result);
        }

        $scope.SearchDeliveryStatus = function () {
            $scope.DeliveryStatusResult = [];
            if (($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
                $rootScope.Toaster.Info("Please select location");
                return;
            }

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchDeliveryStatus",
                data: srequest
            }).success(function (result) {
                $scope.DeliveryStatusResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.billcount = result.length;
                $.each(result, function (k, v) {
                    $scope.saleamount += v.amount;
                });
                $scope.DeliveryStatusConfig();
            }).error(function (result) {
            });
        }
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
                    url: "RetailReportService/SearchStock",
                    data: srequest
                }).success(function (result) {
                    $scope.StockResult = result;
                    if (result == null || result.length == 0) {
                        $rootScope.Toaster.Info("No Results Found.");
                    }
                    $scope.StockConfig();
                }).error(function (result) {
                });
            }
        }

        $scope.LoadReportType = function () {

            $.each($scope.AllReportType, function (k, v) {
                if (v.code == "SALES" && $scope.Permission.p_SALESREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "PRODUCTWISESALES" && $scope.Permission.p_SALESREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "ADDITIONALDISC" && $scope.Permission.p_SALESREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "SALESRETURN" && $scope.Permission.p_SALESRETURNREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "SETTLEMENT" && $scope.Permission.p_SETTLEMENTREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "SETTLEMENTDETAIL" && $scope.Permission.p_SETTLEMENTREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "SALEVSSETTLEMENT" && $scope.Permission.p_SETTLEMENTREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "CANCELLEDBILL" && $scope.Permission.p_CANCELLEDBILLREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "BULKBILL" && $scope.Permission.p_BULKBILLREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "DELIVERY" && $scope.Permission.p_DELIVERYREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "SALESTAX" && $scope.Permission.p_SALESTAXREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "SALESHSN" && $scope.Permission.p_SALESHSNREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "SALESBARCODE" && $scope.Permission.p_SALESHSNREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "SALESMAN" && $scope.Permission.p_SALESMANREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "STOCK" && $scope.Permission.p_RETAILSTOCKREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "INWARD" && $scope.Permission.p_INWARDREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "CUSTOMERPAYMENT" && $scope.Permission.p_CUSTOMERPAYMENTREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "SALESANALYSIS" && $scope.Permission.p_SALESANALYSISREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "MISSINGSTOCK" && $scope.Permission.p_RETAILSTOCKREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "STOCKMARKER" && $scope.Permission.p_RETAILSTOCKREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "PRICECHANGE" && $scope.Permission.p_RETAILSTOCKREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else if (v.code == "STOCKAUDIT" && $scope.Permission.p_RETAILSTOCKREPORT.view) {
                    $scope.ReportType.push(v);
                }
                else {
                    $scope.ReportType.push(v);
                }
            });
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

        $scope.printunsettled = function () {
            $scope.SalesResult.sort(function (a, b) {
                return (a.counter + a.employeename) > (b.counter + b.employeename) ? -1 : (a.counter + a.employeename) < (b.counter + b.employeename) ? 1 : 0;
            });

            var list = [];
            list.push($rootScope.PrintFormater.Bold("UNSETTLED REPORT - " + $filter('date')($scope.SalesResult[0].billdate, 'dd-MM-yyyy')))
            var lastcounter = null;
            $.each($scope.SalesResult, function (k, v) {
                if (lastcounter != v.counter) {
                    lastcounter = v.counter;
                    list.push($rootScope.PrintFormater.Bold(v.counter + "(" + v.employeename + ")"));
                }
                var row = [];
                row.push($rootScope.PrintFormater.Normal($filter('date')(v.billdate, 'hh:mm a')));
                row.push(v.billno.padEnd(14, ' '));
                row.push($filter('number')(v.amount, 0).padStart(12, ' '));
                list.push(row.join(' '));
            });
            list.join($rootScope.PrintFormater.Cutter());
            var printdocument = window.btoa(list.join('\r\n'));
            $rootScope.Sent2Printer(printdocument);
        }

        $scope.SalesBillTaxValidation = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '500px', '100%', false, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('description', 'Tax %', 80);
            igrid.AddFormatedColumn('billincludetax', 'Bill Value', 100, 'N2', 2).total = true;
            //igrid.AddFormatedColumn('billexcludetax', 'bill exclude tax', 120);
            igrid.AddFormatedColumn('taxableamount', 'Taxable Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('tax', 'Tax Value', 90, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxdiff', 'Tax Difference', 100, 'N2', 4).total = true;

            igrid.EnableTotal();

            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SalesResult);
        }

        $scope.SalesBillWiseTaxSplit = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '500px', '100%', false, true, true, 0);
            igrid.options.showHeaderRow = true;

            var taxunique = []
            $.each($scope.SalesResult, function (k, v) {
                v.serial = k + 1;
                $.each(v.taxsplits, function (kk, vv) {
                    if (taxunique.indexOf(vv.percentage) < 0) {
                        taxunique.push(vv.percentage);
                    }
                });
            });
            taxunique.sort();

            if (typeof (addcolumn) == 'undefined' || addcolumn)
                igrid.AddFormatedColumn('serial', 'S.No', 40, 'N0', 0);

            igrid.AddSimpleColumn('location', 'Location', 150);
            igrid.AddSimpleColumn('customer', 'Name', 100);
            igrid.AddSimpleColumn('productname', 'GST NO', 120);
            igrid.AddSimpleColumn('mobileno', 'Mobile', 100);
            igrid.AddSimpleColumn('sizename', 'City', 120);
            var groupColumns = [];
            groupColumns.push(
                {
                    id: 'name',
                    name: 'Customer Details',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });
            igrid.columns = [];
            igrid.AddSimpleColumn('billno', 'Bill No', 120);
            igrid.AddFormatedColumn('billdate', 'Bill Date', 120, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('saleqty', 'Sale Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('saletax', 'Sale Tax', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('addlndiscount', 'Addln. Discount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxable', 'Taxable Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('rounding', 'Rounding', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('amount', 'Net Amount', 120, 'N2', 2).total = true;
            groupColumns.push(
                {
                    id: 'name',
                    name: 'Bill Details',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });


            $.each(taxunique, function (k, v) {

                igrid.columns = [];

                var column = igrid.AddCustomColumn('Taxable' + v, 'Taxable', 100, function (item, col) {
                    var cellresult = item.taxsplits.find(function (item) { return item.percentage == col.internal; });
                    if (cellresult && cellresult != null) return $filter('IndianNumber')(cellresult.taxable, 2);
                    return '';
                });
                column.internal = v;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var cellresult = item.taxsplits.find(function (item) { return item.percentage == col.internal; });
                    if (cellresult && cellresult != null) return cellresult.taxable;
                    return 0;
                }

                column = igrid.AddCustomColumn('SGST' + v, 'SGST', 100, function (item, col) {
                    var cellresult = item.taxsplits.find(function (item) { return item.percentage == col.internal; });
                    if (cellresult && cellresult != null && !cellresult.igst) {
                        return $filter('IndianNumber')(cellresult.tax / 2, 2);
                    }
                    return '';
                });
                column.internal = v;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var cellresult = item.taxsplits.find(function (item) { return item.percentage == col.internal; });
                    if (cellresult && cellresult != null && !cellresult.igst) {
                        return cellresult.tax / 2;
                    }
                    return 0;
                }

                column = igrid.AddCustomColumn('CGST' + v, 'CGST', 100, function (item, col) {
                    var cellresult = item.taxsplits.find(function (item) { return item.percentage == col.internal; });
                    if (cellresult && cellresult != null && !cellresult.igst) {
                        return $filter('IndianNumber')(cellresult.tax / 2, 2);
                    }
                    return '';
                });
                column.internal = v;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var cellresult = item.taxsplits.find(function (item) { return item.percentage == col.internal; });
                    if (cellresult && cellresult != null && !cellresult.igst) {
                        return cellresult.tax / 2;
                    }
                    return 0;
                }

                column = igrid.AddCustomColumn('IGST' + v, 'IGST', 100, function (item, col) {
                    var cellresult = item.taxsplits.find(function (item) { return item.percentage == col.internal; });
                    if (cellresult && cellresult != null && cellresult.igst) {
                        return $filter('IndianNumber')(cellresult.tax, 2);
                    }
                    return '';
                });
                column.internal = v;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var cellresult = item.taxsplits.find(function (item) { return item.percentage == col.internal; });
                    if (cellresult && cellresult != null && cellresult.igst) {
                        return cellresult.tax;
                    }
                    return 0;
                }

                groupColumns.push(
                    {
                        id: 'name',
                        name: v + '%',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            });

            igrid.columns = groupColumns;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.SalesResult);
        }

        $scope.SalesBillNoConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '500px', '100%', false, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('prefix', 'Prefix', 150);
            igrid.AddSimpleColumn('start', 'Start', 120);
            igrid.AddSimpleColumn('end', 'End', 150);
            igrid.AddSimpleColumn('cancelledbills', 'Cancelled', 120);
            igrid.AddSimpleColumn('normalbill', 'Sales', 120);
            igrid.AddSimpleColumn('billnosummary', 'Total', 120);
            igrid.EnableTotal();

            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SalesResult);
        }


        $scope.SalesConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '500px', '100%', false, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SalesResult);

            if ($scope.SearchCondition.mode == "1") {
                igrid.AddSimpleColumn('location', 'Location', 150);
            }
            else if ($scope.SearchCondition.mode == "10") {
                igrid.AddSimpleColumn('location', 'Company', 150);
            }
            else if ($scope.SearchCondition.mode == "2") {
                igrid.AddFormatedColumn('billdate', 'Bill Date', 120, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('serialno', 'Day', 80);
                igrid.AddFormatedColumn('billcount', 'Bills', 80, 'N0', 0).total = true;
            }
            else if ($scope.SearchCondition.mode == "3") {
                igrid.AddSimpleColumn('location', 'Location', 150);
                igrid.AddSimpleColumn('counter', 'Counter', 100);
                igrid.AddSimpleColumn('employeename', 'Biller', 120);
                igrid.AddFormatedColumn('billcount', 'Bills', 80, 'N0', 0).total = true;
            }
            else if ($scope.SearchCondition.mode == "4" || $scope.SearchCondition.mode == "6"
                || $scope.SearchCondition.mode == "11" || $scope.SearchCondition.mode == "13") {
                if (!$scope.SearchCondition.isb2b) {
                    igrid.AddSimpleColumn('location', 'Location', 150);
                    igrid.AddSimpleColumn('counter', 'Counter', 120);
                    igrid.AddSimpleColumn('employeename', 'Biller', 150);
                    if ($scope.SearchCondition.mode == "11")
                        igrid.AddSimpleColumn('modifiedemployeename', 'Cashier Alt.', 100);
                }
                if ($scope.SearchCondition.isb2b || $scope.SearchCondition.mode == "4") {
                    igrid.AddSimpleColumn('modifiedemployeename', 'Modified By.', 100);
                    igrid.AddSimpleColumn('salerefname', 'Sales Rep. By', 100);
                }

                igrid.AddSimpleColumn('customer', 'Customer Name', 100);
                igrid.AddSimpleColumn('productname', 'GST NO', 120);
                igrid.AddSimpleColumn('mobileno', 'Mobile', 100);
                igrid.AddSimpleColumn('sizename', 'Customer City', 80);
                igrid.AddSimpleColumn('placeofsupply', 'Place Of Supply', 120);
                if ($scope.SearchCondition.showsalesummary) {
                    igrid.AddSimpleColumn('serialno', 'Delivery No', 120);
                }
                igrid.AddSimpleColumn('billno', 'Bill No', 120);
                igrid.AddFormatedColumn('billdate', 'Bill Date', 120, 'dd-MM-yyyy hh:mm a');

                if ($scope.SearchCondition.isb2b) {
                    igrid.AddFormatedColumn('journalqty', 'CGST', 80, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('balanceqty', 'SGST', 80, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('billedqty', 'IGST', 80, 'N2', 2).total = true;
                }
                else {
                    igrid.AddCustomColumn('isinterstatesale', 'IGST', 60, function (item, col) {
                        return item.isinterstatesale == true ? "YES" : "NO";
                    });
                    igrid.AddCustomColumn('isexport', 'Export', 60, function (item, col) {
                        return item.isexport == true ? "YES" : "NO";
                    });
                    igrid.AddCustomColumn('istransfer', 'Transfer', 60, function (item, col) {
                        return item.istransfer == true ? "YES" : "NO";
                    });

                    igrid.AddReferenceColumn('billtype', 'Bill Type', 80, $scope.BillType);
                    igrid.AddSimpleColumn('remarks', 'Bill Remarks', 100);
                    igrid.AddSimpleColumn('returnreason', 'Return Reason', 100);
                }
            }
            else if ($scope.SearchCondition.mode == "5") {
                igrid.AddSimpleColumn('counter', 'Counter', 120);
                igrid.AddSimpleColumn('employeename', 'Biller', 150);
                igrid.AddSimpleColumn('billno', 'Bill No', 120);
                igrid.AddFormatedColumn('billdate', 'Bill Date', 120, 'dd-MM-yyyy hh:mm a');
                igrid.AddReferenceColumn('billtype', 'Bill Type', 120, $scope.BillType);
                igrid.AddSimpleColumn('barcode', 'Barcode', 100);
                igrid.AddSimpleColumn('serialno', 'UAN No', 120);
                igrid.AddFormatedColumn('rate', 'Rate', 100, 'N2', 2).total = true;
                igrid.AddSimpleColumn('suppliername', 'Supplier', 120);
                igrid.AddSimpleColumn('itemname', 'Product', 120);
                igrid.AddReferenceColumn('brandid', 'Brand', 120, $scope.Brand);
                igrid.AddReferenceColumn('materialid', 'Material', 120, $scope.Material);
                igrid.AddReferenceColumn('patternid', 'Pattern', 120, $scope.Pattern);
                igrid.AddReferenceColumn('styleid', 'Style', 120, $scope.Style);
                igrid.AddReferenceColumn('sizeid', 'Size', 120, $scope.Sizes);
            }

            igrid.AddFormatedColumn('saleqty', 'Sale Qty', 80, 'N2', 2).total = true;
            if ($scope.SearchCondition.mode == "4")
                igrid.AddFormatedColumn('total', 'Gross Value', 80, 'N2', 2).total = true;

            igrid.AddFormatedColumn('discount', 'Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('addlndiscount', 'Addln. Discount', 80, 'N2', 2).total = true;

            if ($scope.SearchCondition.mode == "4" || $scope.SearchCondition.mode == "11") {
                igrid.AddFormatedColumn('schemediscount', 'Scheme Discount', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('loyaltydiscount', 'Loyalty Discount', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('discountpercentage', 'Discount %', 60, 'N2', 2).average = true;
                igrid.AddFormatedColumn('addlndiscountpercentage', 'Addln.Discount %', 60, 'N2', 2).average = true;
                igrid.AddFormatedColumn('schemediscountpercentage', 'Scheme Discount %', 60, 'N2', 2).average = true;
                igrid.AddFormatedColumn('loyaltydiscountpercentage', 'Loyalty Discount %', 60, 'N2', 2).average = true;
            }
                
            igrid.AddFormatedColumn('taxable', 'Taxable Amount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('saletax', 'Sale Tax', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('rounding', 'Rounding', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('amount', 'Net Amount', 100, 'N2', 2).total = true;

            if ($scope.SearchCondition.mode == "2") {
                var column = igrid.AddCustomColumn('avgbillvalue', 'Avg Value', 100, function (item, col) {
                    return $filter('IndianNumber')(item.amount / item.billcount, 2);
                });
                column.getValue = function (item, col) {
                    return item.amount / item.billcount;
                }
                column.cssClass = 'rightalign';
                column.total = true;
            }

            if ($scope.SearchCondition.mode == "2" &&
                $scope.SearchCondition.locationid &&
                $scope.SearchCondition.locationid > 0 &&
                $scope.SearchCondition.companyid &&
                $scope.SearchCondition.companyid > 0) {
                igrid.AddButtonColumn('action', 'Download', 60, 'Xlsx', function (e, args) {
                    $scope.$apply(function () {
                        $scope.DownloadDayXslx(args.item);
                    });
                    return true;
                });
            }
            igrid.EnableTotal();
            igrid.Initialize();

            igrid.dataView.getItemMetadata = function (row) {
                var item = igrid.dataView.getItem(row);
                if (item.isverified) {
                    item.style = 'igridbigrowswithdarkgreen';
                }
                return { 'cssClasses': 'igridbigrows' };
            };

            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SalesResult, '_id');
        }


        $scope.DownloadDayXslx = function (item) {
            $rootScope.Download("RetailReportService/DayTransaction/"
                + $scope.UserInfo.OrganizationID + '/'
                + $filter('date')(item.billdate, 'dd-MM-yyyy') + '/'
                + $scope.SearchCondition.companyid + '/'
                + $scope.SearchCondition.locationid + '/'
                + $scope.isb2b
                , 'dt' + $filter('date')(item.billdate, 'dd-MM-yyyy') + '.xlsx', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet');
        }


        $scope.SalesSnapshotConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', false, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.EnableSerial($scope.SalesResult);
            igrid.AddSimpleColumn('reportname', 'Report Name', 180);
            igrid.AddFormatedColumn('qty', 'Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('additionaldiscount', 'Addln. Discount', 100, 'N2', 2).total = true;
            var column = igrid.AddCustomColumn('discount', 'Total Discount', 100, function (item, col) {
                return $filter('IndianNumber')(item.discount + item.additionaldiscount, 2);
            });
            column.cssClass = 'rightalign';

            igrid.AddFormatedColumn('tax', 'Tax', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxableamount', 'Taxable', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('rounding', 'Rounding', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('netsales', 'Net Sales', 120, 'N2', 2).total = true;

            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SalesResult);
        }

        $scope.CreditSalesTaxConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', false, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.CreditSalesList);

            igrid.AddFormatedColumn('billdate', 'Bill Date', 120, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('billno', 'Bill No', 120);
            igrid.AddSimpleColumn('customername', 'Customer Name', 120);
            igrid.AddSimpleColumn('mobileno', 'Mobile No', 120);
            igrid.AddSimpleColumn('area', 'Area', 120);
            igrid.AddFormatedColumn('taxpercentage', 'Tax Percentage', 100, 'N2', 2);
            igrid.AddFormatedColumn('cgst', 'CGST', 120, 'N2', 2).total = true;
            igrid.AddFormatedColumn('sgst', 'SGST', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('amount', 'Amount', 100, 'N2', 2).total = true;
            igrid.EnableTotal();

            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.CreditSalesList);
        }


        $scope.SalesMarginConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '500px', '100%', false, true, true, 0, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SalesMarginResult);

            igrid.AddFormatedColumn('billdate', 'Sale Date', 120, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('location', 'Location', 120);
            igrid.AddFormatedColumn('billcount', 'Bill Count', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('rate', 'Cost Price', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('rounding', 'Gr.Sale Value', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxable', 'Taxable Value', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('saletax', 'Tax Value', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('amount', 'Net Sale Value', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('total', 'Margin Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('addlndiscount', 'Margin %', 100, 'N2', 2);

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SalesMarginResult);
        }

        $scope.SearchSalesTaxHSNSummaryConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '500px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SalesTaxSummaryResult);

            igrid.AddReferenceColumn('companyid', 'Company', 150, $scope.Company);
            igrid.AddReferenceColumn('locationid', 'Location', 150, $scope.Location);


            igrid.AddSimpleColumn('customername', 'Customer Name', 150);
            igrid.AddSimpleColumn('gstno', 'GST NO', 140);
            igrid.AddSimpleColumn('placeofsupply', 'Place Of Supply', 120);
            igrid.AddSimpleColumn('billno', 'Bill No', 100);

            igrid.AddFormatedColumn('billdate', 'Sale Date', 120, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('printingname', 'Product', 150);
            igrid.AddSimpleColumn('hsncode', 'HSN', 100);


            igrid.AddFormatedColumn('taxpercentage', 'TAX %', 100, 'N2', 2);
            igrid.AddFormatedColumn('amount', 'SALE VALUE', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('qty', 'Qty', 100, 'N2', 2).total = true;
            if ($scope.SearchCondition.mode != "interstate") {
                igrid.AddFormatedColumn('sgst', 'SGST', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('cgst', 'CGST', 100, 'N2', 2).total = true;
            }

            if ($scope.SearchCondition.mode == "interstate" || $scope.SearchCondition.mode == "All" || $scope.SearchCondition.mode == "credit") {
                igrid.AddFormatedColumn('igst', 'IGST', 100, 'N2', 2).total = true;
            }


            igrid.AddFormatedColumn('taxamount', 'TOTAL TAX', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('total', 'GRAND TOTAL', 100, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SalesTaxSummaryResult);
        }

        $scope.SalesTaxDetailConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '500px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SalesTaxSummaryResult);

            igrid.AddReferenceColumn('companyid', 'Company', 150, $scope.Company);
            igrid.AddReferenceColumn('locationid', 'Location', 150, $scope.Location);

            if (!$scope.SearchCondition.showmarker) {
                igrid.AddSimpleColumn('customername', 'Customer Name', 150);
                igrid.AddSimpleColumn('gstno', 'GST NO', 140);
                igrid.AddSimpleColumn('placeofsupply', 'Place Of Supply', 120);
                igrid.AddSimpleColumn('billno', 'Bill No', 100);
            }

            igrid.AddFormatedColumn('billdate', 'Sale Date', 120, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('taxpercentage', 'TAX %', 100, 'N2', 2);
            igrid.AddFormatedColumn('amount', 'SALE VALUE', 100, 'N2', 2).total = true;
            if ($scope.SearchCondition.mode != "interstate") {
                igrid.AddFormatedColumn('sgst', 'SGST', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('cgst', 'CGST', 100, 'N2', 2).total = true;
            }

            if ($scope.SearchCondition.mode == "interstate" || $scope.SearchCondition.mode == "All" || $scope.SearchCondition.mode == "credit") {
                igrid.AddFormatedColumn('igst', 'IGST', 100, 'N2', 2).total = true;
            }

            igrid.AddFormatedColumn('taxamount', 'TOTAL TAX', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('total', 'GRAND TOTAL', 100, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SalesTaxSummaryResult);

        }

        $scope.SalesTaxSummaryConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var summary = [];
            var taxpercentages = [];
            var hasigst = false;
            var sno = 1;
            $.each($scope.SalesTaxSummaryResult, function (k, v) {
                if (v.igst && v.igst != 0)
                    hasigst = true;

                var exist = summary.find(function (item) {
                    return item.billdate == v.billdate &&
                        item.companyid == v.companyid &&
                        item.locationid == v.locationid &&
                        item.interstate == v.interstate;
                });

                if (taxpercentages.indexOf(v.taxpercentage) < 0)
                    taxpercentages.push(v.taxpercentage);

                if (exist && exist != null) {
                    var etax = exist.taxs.find(function (item) {
                        return item.taxpercentage == v.taxpercentage;
                    });
                    if (etax && etax != null) {
                        etax.sgst += v.sgst;
                        etax.cgst += v.cgst;
                        etax.igst += v.igst;
                    }
                    else {
                        exist.taxs.push(v);
                    }
                    exist.amount += v.amount;
                    exist.taxamount += v.taxamount;
                    exist.total += v.total;
                }
                else {
                    exist = (JSON.parse(JSON.stringify(v)));
                    exist.taxs = [v];
                    summary.push(exist);
                }
            });

            var igrid = new InfyGrid('gridContainer', '500px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial(summary);

            igrid.AddFormatedColumn('billdate', 'SALE DATE', 150, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('companyid', 'Company', 150, $scope.Company);
            igrid.AddReferenceColumn('locationid', 'Location', 150, $scope.Location);
            igrid.AddFormatedColumn('amount', 'SALE VALUE', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxamount', 'TOTAL TAX', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('total', 'GRAND TOTAL', 100, 'N2', 2).total = true;
            var groupColumns = [];
            groupColumns.push(
                {
                    id: 'name',
                    name: 'Details',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });


            $.each(taxpercentages, function (k, v) {

                igrid.columns = [];

                var column = igrid.AddCustomColumn('SGST' + v, 'SGST', 60, function (item, col) {
                    var cellresult = item.taxs.find(function (item) { return item.taxpercentage == col.internal; });
                    if (cellresult && cellresult != null) return $filter('IndianNumber')(cellresult.sgst, 2);
                    return '';
                });
                column.internal = v;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var cellresult = item.taxs.find(function (item) { return item.taxpercentage == col.internal; });
                    if (cellresult && cellresult != null) return cellresult.sgst;
                    return 0;
                }

                column = igrid.AddCustomColumn('CGST' + v, 'CGST', 60, function (item, col) {
                    var cellresult = item.taxs.find(function (item) { return item.taxpercentage == col.internal; });
                    if (cellresult && cellresult != null) return $filter('IndianNumber')(cellresult.cgst, 2);
                    return '';
                });
                column.internal = v;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var cellresult = item.taxs.find(function (item) { return item.taxpercentage == col.internal; });
                    if (cellresult && cellresult != null) return cellresult.cgst;
                    return 0;
                }

                if (hasigst) {
                    column = igrid.AddCustomColumn('IGST' + v, 'IGST', 60, function (item, col) {
                        var cellresult = item.taxs.find(function (item) { return item.taxpercentage == col.internal; });
                        if (cellresult && cellresult != null) return $filter('IndianNumber')(cellresult.igst, 2);
                        return '';
                    });
                    column.internal = v;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.getValue = function (item, col) {
                        var cellresult = item.taxs.find(function (item) { return item.taxpercentage == col.internal; });
                        if (cellresult && cellresult != null) return cellresult.igst;
                        return 0;
                    }
                }
                groupColumns.push(
                    {
                        id: 'name',
                        name: v + '%',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            });

            igrid.columns = groupColumns;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(summary);

        }
        $scope.InvoicewiseSalesConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.InvoicewiseSalesResult);

            igrid.AddSimpleColumn('billno', 'SALE BILL', 100);
            igrid.AddFormatedColumn('billdate', 'SALE DATE', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('customername', 'PRODUCT', 150);
            igrid.AddSimpleColumn('area', 'MATERIAL', 120);
            igrid.AddSimpleColumn('mobileno', 'HSNCODE', 80);
            igrid.AddFormatedColumn('qty', 'QTY', 80, 'N2', 2);
            igrid.AddFormatedColumn('rate', 'RATE', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('amount', 'TAXABLE AMT', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxpercentage', 'TAX %', 100, 'N2', 2);

            if ($scope.SearchCondition.mode != "interstate") {
                igrid.AddFormatedColumn('sgst', 'SGST', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('cgst', 'CGST', 100, 'N2', 2).total = true;
            }

            if ($scope.SearchCondition.mode == "interstate" || $scope.SearchCondition.mode == "All" || $scope.SearchCondition.mode == "credit") {
                igrid.AddFormatedColumn('igst', 'IGST', 100, 'N2', 2).total = true;
            }

            igrid.AddFormatedColumn('total', 'SALE VALUE', 100, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.InvoicewiseSalesResult);

        }
        $scope.ProductConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SalesPerDayList);

            igrid.AddReferenceColumn('companyid', 'Company', 150, $scope.Company);
            igrid.AddSimpleColumn('location', 'Location', 120);
            igrid.AddSimpleColumn('printingname', 'Product', 180);
            igrid.AddFormatedColumn('qtytotal', 'Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('receivetotal', 'Amount', 120, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.SalesPerDayList);
        }
        $scope.SalesRetConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SalesReturnResult);

            igrid.AddSimpleColumn('billno', 'Bill No', 180);
            igrid.AddSimpleColumn('employeename', 'Biller', 150);
            igrid.AddSimpleColumn('counter', 'Counter', 150);
            igrid.AddFormatedColumn('billdate', 'Return Date', 150, 'dd-MM-yyyy');
            if ($scope.SearchCondition.showdetail == true) {
                igrid.AddSimpleColumn('barcode', 'Barcode', 120);
            }
            if ($scope.SearchCondition.showdetail == true) {
                igrid.AddSimpleColumn('itemname', 'Detail', 120);
            }
            igrid.AddSimpleColumn('rate', 'Rate', 150).total = true;
            igrid.AddSimpleColumn('saleqty', 'Qty', 150).total = true;
            igrid.AddSimpleColumn('saletax', 'Tax', 150).total = true;
            igrid.AddSimpleColumn('discount', 'Discount', 150).total = true;
            igrid.AddSimpleColumn('amount', 'Amount', 150).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.SalesReturnResult);
        }

        $scope.AdDiscConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            if ($scope.SearchCondition.billerwise == true)
                igrid.EnableSerial($scope.BillwiseDiscountList);
            else
                igrid.EnableSerial($scope.SettlementwiseDiscountList);

            igrid.AddSimpleColumn('location', 'Location', 180);
            igrid.AddSimpleColumn('counter', 'Counter', 150);
            if ($scope.SearchCondition.billerwise == true) {
                igrid.AddSimpleColumn('employeename', 'Biller', 120);
            }
            if ($scope.SearchCondition.cashierwise == true) {
                igrid.AddSimpleColumn('employeename', 'Cashier', 120);
            }
            igrid.AddFormatedColumn('additionaldiscount', 'Discount', 150, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            if ($scope.SearchCondition.billerwise == true) {
                $scope.igrid.UpdateData($scope.BillwiseDiscountList);
            }
            else {
                $scope.igrid.UpdateData($scope.SettlementwiseDiscountList);

            }

        }

        $scope.SettlementDaySummaryConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SettlementDaySummaryResult);

            igrid.AddSimpleColumn('companyname', 'Company', 180);
            igrid.AddSimpleColumn('locationname', 'Location', 180);
            igrid.AddFormatedColumn('settlementon', 'Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('paid_cash', 'Cash', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_card', 'Card', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_upi', 'UPI', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_credit', 'Credit', 100, 'N2', 2).total = true;;
            igrid.AddFormatedColumn('paid_voucher', 'Vouchar', 100, 'N2', 2).total = true;;
            igrid.AddFormatedColumn('paid_adjustment', 'Adjustment', 100, 'N2', 2).total = true;;
            igrid.AddFormatedColumn('paid_discount', 'Discount', 100, 'N2', 2).total = true;;
            igrid.AddFormatedColumn('paid_refund', 'Refund', 100, 'N2', 2).total = true;;
            igrid.AddFormatedColumn('total', 'Total', 100, 'N2', 2).total = true;;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SettlementDaySummaryResult);
        }



        $scope.SettlementReconcilationConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SettlementReconcilationResult);

            igrid.AddSimpleColumn('code', 'Code', 130);
            igrid.AddSimpleColumn('employ', 'Emplyoee', 180);
            igrid.AddFormatedColumn('createdon', 'Paid On', 120, 'dd-MM-yyyy hh:mm a');
            igrid.AddSimpleColumn('bank', 'Bank', 180);
            if ($scope.SearchCondition.integration) {
                igrid.AddSimpleColumn('bankinvoiceno', 'Invoice', 180);
                igrid.AddSimpleColumn('banktxntype', 'Txn Type', 180);
                igrid.AddSimpleColumn('rrnno', 'RRNo', 180);
            }
            igrid.AddFormatedColumn('paid_card', 'Card', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_upi', 'UPI', 100, 'N2', 2).total = true;
            igrid.AddCustomColumn('edit', '<input type="checkbox" style="z-index: auto;margin-left:5px">', 40, function (item, col) {
                if (item.recon)
                    return '<input type="checkbox" style="z-index: auto;margin-left:10px" checked>';
                else
                    return '<input type="checkbox" style="z-index: auto;margin-left:10px">';
            });
            igrid.AddFormatedColumn('paidamount', 'Amount', 100, 'N2', 2).total = true;

            var column = igrid.AddCustomColumn('received', 'Receied', 100, function (item, col) {
                if (item.recon)
                    return item.paidamount;
                else
                    return 0;
            });
            column.getValue = function (item, col) {
                if (item.recon)
                    return item.paidamount;
                else
                    return 0;
            };
            column.total = true;
            column.cssClass = 'rightalign';
            igrid.EnableTotal();
            igrid.Initialize();

            igrid.grid.onClick.subscribe(function (e, args) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'edit') {
                    item = args.grid.getDataItem(args.row);
                    item.recon = !item.recon && $(e.target).val() == 'on';
                    $timeout(function () {
                        igrid.UpdateAllTotals(args.grid);
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
            });

            igrid.grid.onHeaderClick.subscribe(function (e, args) {

                if (args.column.id == 'edit' && $(e.target).is(":checkbox")) {
                    if (igrid.grid.getEditorLock().isActive() && !igrid.grid.getEditorLock().commitCurrentEdit()) {
                        e.preventDefault();
                        e.stopImmediatePropagation();
                        return;
                    }
                    if ($(e.target).is(":checked")) {
                        for (var i = 0; i < igrid.grid.getData().getLength(); i++) {
                            var item = igrid.grid.getDataItem(i);
                            item.recon = true;
                        }

                    } else {
                        for (var i = 0; i < igrid.grid.getData().getLength(); i++) {
                            var item = igrid.grid.getDataItem(i);
                            item.recon = false;
                        }
                    }
                    igrid.grid.invalidate();
                    igrid.grid.render();
                    e.stopPropagation();
                    e.stopImmediatePropagation();
                }

            });

            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SettlementReconcilationResult);
        }

        $scope.SaveReconcilation = function () {
            var correctids = [];
            var missingids = [];
            $.each($scope.SettlementReconcilationResult, function (k, v) {
                if (v.recon)
                    correctids.push(v.id);
                else
                    missingids.push(v.id);
            })
            request = {
                "item": { 'reconids': correctids, 'reconmids': missingids },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BillsettlementmasterService/Reconilation",
                data: srequest
            }).success(function (result) {
                if (result) {
                    $rootScope.Toaster.Info("Completed");
                }
            }).error(function (result) {
            });
        }

        $scope.SettlementSummaryConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SettlementSummaryResult);

            igrid.AddSimpleColumn('companyname', 'Company', 180);
            igrid.AddFormatedColumn('paid_cash', 'Cash', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_card', 'Card', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_upi', 'UPI', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_credit', 'Credit', 100, 'N2', 2).total = true;;
            igrid.AddFormatedColumn('paid_voucher', 'Vouchar', 100, 'N2', 2).total = true;;
            igrid.AddFormatedColumn('paid_adjustment', 'Adjustment', 100, 'N2', 2).total = true;;
            igrid.AddFormatedColumn('paid_discount', 'Discount', 100, 'N2', 2).total = true;;
            igrid.AddFormatedColumn('paid_refund', 'Refund', 100, 'N2', 2).total = true;;
            igrid.AddFormatedColumn('total', 'Total', 100, 'N2', 2).total = true;;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SettlementSummaryResult);
        }

        $scope.DaySalesSummaryConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);

            igrid.AddSimpleColumn('companyname', 'COMPANY', 180);
            igrid.AddFormatedColumn('billcount', 'BILL COUNT', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('totalsales', 'TOTAL SALES', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('billerdiscount', 'BILLER DISCOUNT', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('returnbills', 'EXCHANGE BILL', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('returnvalue', 'EXCHANGE VALUE', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('cancelbills', 'CANCEL BILL', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('cancelvalue', 'CANCEL VALUE', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('approvedbills', 'APP. BILL', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('approvedvalue', 'APP. VALUE', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('cashsales', 'CASH', 90, 'N2', 2).total = true;
            igrid.AddFormatedColumn('cardsales', 'CARD', 90, 'N2', 2).total = true;
            igrid.AddFormatedColumn('upisales', 'UPI', 90, 'N2', 2).total = true;
            igrid.AddFormatedColumn('creditsales', 'CREDIT', 80, 'N2', 2).total = true;;
            igrid.AddFormatedColumn('cashierdiscount', 'CASHIER DISCOUNT', 100, 'N2', 2).total = true;;
            igrid.AddFormatedColumn('cashieradjustment', 'ADJUSTMENT', 80, 'N2', 2).total = true;;
            igrid.AddFormatedColumn('cashrefund', 'REFUND', 60, 'N2', 2).total = true;;
            igrid.AddFormatedColumn('totalcollection', 'TOTAL COLLECTION', 100, 'N2', 2).total = true;;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }

        $scope.SettlementConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '500px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SettlementResult);

            if ($scope.SearchCondition.showdetail || $scope.SearchCondition.showsalesummary) {
                igrid.AddFormatedColumn('receiptdate', 'Date', 100, 'dd-MM-yyyy');
            }

            igrid.AddSimpleColumn('location', 'LOCATION', 150);

            if (!$scope.SearchCondition.showdetail && !$scope.SearchCondition.showsalesummary && !$scope.SearchCondition.showmarker && 
                $scope.SearchCondition.locationid > 0) {
                igrid.AddSimpleColumn('counter', 'COUNTER', 100);
                igrid.AddSimpleColumn('employeename', 'CASHIER', 150);
                igrid.AddFormatedColumn('openingcash', 'OPENING', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('closingcash', 'CLOSING', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('cashdifference', 'CASH DIFF', 80, 'N2', 2).total = true;
            }
            if ($scope.SearchCondition.showmarker) {
                igrid.AddSimpleColumn('employeename', 'CASHIER', 150);
            }
            if ($scope.SearchCondition.showdetail) {
                igrid.AddSimpleColumn('counter', 'COUNTER', 100);
                igrid.AddSimpleColumn('employeename', 'CASHIER', 150);
                igrid.AddReferenceColumn('bankid', 'BANK', 120, $scope.BankList);
            }

            igrid.AddFormatedColumn('paid_cash', 'CASH', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_card', 'CARD', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_upi', 'UPI', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_credit', 'CREDIT', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_voucher', 'VOUCHAR', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('adjustment_paid', 'ADJUSTMENT', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'DISCOUNT', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('return_cash', 'RETURN', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('creditnote_refund', 'CREDIT NOTE', 80, 'N2', 2).total = true;

            var column = igrid.AddCustomColumn('cashinhand', 'CASH IN HAND', 100, function (item, col) {
                return $filter('IndianNumber')(item.paid_cash - item.return_cash, 2);
            });
            column.getValue = function (item, col) {
                return item.paid_cash - item.return_cash;
            }
            column.cssClass = 'rightalign';
            column.total = true;

            var column2 = igrid.AddCustomColumn('netsale', 'NET SALE(CASH+CARD+UPI)', 100, function (item, col) {
                return $filter('IndianNumber')((item.paid_cash + item.paid_upi + item.paid_card), 2);
            });
            column2.getValue = function (item, col) {
                return (item.paid_cash + item.paid_upi + item.paid_card);
            }
            column2.cssClass = 'rightalign';
            column2.total = true;

            igrid.AddFormatedColumn('totalsale', 'TOTAL SALE', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('totalcollection', 'COLLECTION', 100, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SettlementResult);

        }

        $scope.SettlementDetailConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '500px', '100%', false, true, true, 0);
            igrid.options.showHeaderRow = true;

            if (typeof ($scope.SearchCondition.payment_typeid) != 'undefined' && $scope.SearchCondition.payment_typeid > 0) {
                var result = [];
                $.each($scope.SettlementDetailResult, function (k, v) {

                    switch ($scope.SearchCondition.payment_typeid) {
                        case 1:
                            if (v.paid_cash != 0)
                                result.push(v);
                            break;
                        case 2:
                            if (v.paid_card != 0)
                                result.push(v);
                            break;
                        case 3:
                            if (v.paid_credit != 0)
                                result.push(v);
                            break;
                        case 4:
                            if (v.paid_voucher != 0)
                                result.push(v);
                            break;
                        case 5:
                            if (v.return_cash != 0)
                                result.push(v);
                            break;
                        case 6:
                            if (v.adjustment_paid != 0)
                                result.push(v);
                            break;
                        case 7:
                            if (v.discount != 0)
                                result.push(v);
                            break;
                        case 8:
                            if (v.paid_upi != 0)
                                result.push(v);
                            break;
                        case 9:
                            if (v.receivable != (v.paid_cash + v.paid_card + v.paid_upi + v.paid_cheque + v.paid_credit + v.paid_voucher + v.adjustment_paid + v.discount))
                                result.push(v);
                            break;
                        case 10:
                            if (v.paid_cheque != 0)
                                result.push(v);
                            break;
                    }

                });

                $scope.SettlementDetailResult = result;
            }

            igrid.EnableSerial($scope.SettlementDetailResult);
            igrid.AddSimpleColumn('location', 'Location', 120);
            igrid.AddSimpleColumn('counter', 'Counter', 100);
            igrid.AddSimpleColumn('employeename', 'Cashier Name', 120);
            igrid.AddFormatedColumn('receiptdate', 'Settlement On', 100, 'dd-MM-yyyy hh:mm a');
            igrid.AddSimpleColumn('receiptno', 'Set No', 100);
            igrid.AddSimpleColumn('billno', 'Bill No', 100);
            igrid.AddFormatedColumn('receivable', 'Receivable', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paidamount', 'Paid', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_cash', 'Cash', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_card', 'Card', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_upi', 'UPI', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_cheque', 'Cheque', 80, 'N2', 2).total = true;

            if ($scope.SearchCondition.showcardno)
                igrid.AddSimpleColumn('card_no', 'Card No', 120);
            if ($scope.SearchCondition.showcardtype)
                igrid.AddReferenceColumn('bankid', 'Bank', 120, $scope.BankList);

            igrid.AddFormatedColumn('paid_credit', 'Credit', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_voucher', 'Vouchar', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('adjustment_paid', 'Adjustment', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discountpercent', 'Discount %', 80, 'N2', 2).average = true;
            igrid.AddFormatedColumn('discount_adjusted', 'Cashier Discount Adjusted', 120, 'N2', 2).total = true;
            igrid.AddFormatedColumn('cashierdiscountpercent', 'Cashier Discount %', 80, 'N2', 2).average = true;
            igrid.AddSimpleColumn('discountapprovedby', 'Cashier Discount Approvedby', 120);
            igrid.AddSimpleColumn('discountremark', 'Discount Remark', 100);
            igrid.AddFormatedColumn('return_cash', 'Return', 100, 'N2', 2).total = true;

            if (typeof ($scope.UserInfo.p_SETTLEMENTREPORT.delete) != 'undefined' && $scope.UserInfo.p_SETTLEMENTREPORT.delete) {
                igrid.AddButtonColumn('action', 'Action', 80, 'Cancel', function (e, args) {
                    $scope.$apply(function () {
                        $scope.ManageSettlement('Cancel', args.item);
                    });
                    return true;
                });
            }

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SettlementDetailResult);
        }


        $scope.CancelledBillConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.CancelledBillResult);

            igrid.AddSimpleColumn('location', 'Location', 150);
            igrid.AddSimpleColumn('counter', 'Counter', 100);
            igrid.AddSimpleColumn('employeename', 'Biller', 120);
            igrid.AddSimpleColumn('billno', 'Bill No', 100);
            igrid.AddFormatedColumn('billdate', 'Bill Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('customer', 'Customer Name', 120);
            igrid.AddSimpleColumn('mobileno', 'Mobile No', 100);
            igrid.AddSimpleColumn('itemname', 'Bill Type', 120);
            igrid.AddFormatedColumn('saleqty', 'Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('saletax', 'Tax', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('amount', 'Amount', 100, 'N2', 2).total = true;
            igrid.AddSimpleColumn('displaytext', 'Bill Status', 120);
            igrid.AddSimpleColumn('returnbillno', 'Adj.Bill No', 100);
            igrid.AddSimpleColumn('returnreason', 'Return Reason', 100);
            igrid.AddSimpleColumn('remarks', 'Remarks', 100);
            igrid.AddSimpleColumn('modifiedemployeename', 'Updated By', 100);

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.CancelledBillResult);
        }

        $scope.BulkBillConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '500px', '100%', false, $scope.SearchCondition.showdetail == true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.EnableSerial($scope.BulkBillResult);

            if (!$scope.SearchCondition.showdetail) {
                igrid.AddSimpleColumn('location', 'Location', 120);
                igrid.AddSimpleColumn('counter', 'Counter', 100);
                igrid.AddSimpleColumn('employeename', 'Biller', 100);
                igrid.AddSimpleColumn('customer', 'Customer', 120);
                igrid.AddSimpleColumn('mobileno', 'Mobile No', 80);
                igrid.AddSimpleColumn('billno', 'Appd.Bill No', 100);
                igrid.AddSimpleColumn('approvedby', 'Approved by', 120);
                igrid.AddSimpleColumn('returnbillno', 'Sale Bill No', 100);
                igrid.AddFormatedColumn('billdate', 'Bill Date', 100, 'dd-MM-yyyy');
                igrid.AddReferenceColumn('billtype', 'Bill Type', 100, $scope.BillDisplayType);
                igrid.AddFormatedColumn('saleqty', 'Approved Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('billedqty', 'Billed Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('journalqty', 'Return Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('balanceqty', 'Balance Qty', 80, 'N2', 2).total = true;

                igrid.AddFormatedColumn('saletax', 'Tax', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('discount', 'Discount', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('amount', 'Net Amount', 100, 'N2', 2).total = true;
            }
            else {
                igrid.AddSimpleColumn('counter', 'Counter', 120);
                igrid.AddSimpleColumn('employeename', 'Biller', 150);
                igrid.AddSimpleColumn('approvedby', 'Approved by', 120);
                igrid.AddSimpleColumn('customer', 'Customer', 120);
                igrid.AddSimpleColumn('mobileno', 'Mobile No', 80);
                igrid.AddSimpleColumn('billno', 'Approve Bill No', 120);
                igrid.AddSimpleColumn('returnbillno', 'Sale Bill No', 100);
                igrid.AddFormatedColumn('billdate', 'Bill Date', 120, 'dd-MM-yyyy hh:mm a');
                igrid.AddReferenceColumn('billtype', 'Bill Type', 120, $scope.BillDisplayType);
                igrid.AddSimpleColumn('barcode', 'Barcode', 100);
                igrid.AddFormatedColumn('rate', 'Rate', 80, 'N2', 2).total = true;
                igrid.AddSimpleColumn('suppliername', 'Supplier', 140);
                igrid.AddSimpleColumn('itemname', 'Product', 120);
                igrid.AddSimpleColumn('sectionname', 'Section', 120);
                igrid.AddReferenceColumn('brandid', 'Brand', 100, $scope.Brand);
                igrid.AddReferenceColumn('materialid', 'Material', 100, $scope.Material);
                igrid.AddReferenceColumn('patternid', 'Pattern', 100, $scope.Pattern);
                igrid.AddReferenceColumn('styleid', 'Style', 100, $scope.Style);
                igrid.AddReferenceColumn('sizeid', 'Size', 100, $scope.Sizes);
                igrid.AddFormatedColumn('saleqty', 'Approved Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('billedqty', 'Billed Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('journalqty', 'Return Qty', 80, 'N2', 2).total = true;
                //igrid.AddFormatedColumn('balanceqty', 'Balance Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('saletax', 'Sale Tax', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('discount', 'Discount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('addlndiscount', 'Addln. Discount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('taxable', 'Taxable Amount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('rounding', 'Rounding', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('amount', 'Net Amount', 100, 'N2', 2).total = true;
            }
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.ShowImage = false;
            if ($scope.SearchCondition.showdetail) {
                $scope.igrid.grid.onDblClick.subscribe(function (e, args) {
                    if (args.grid.getColumns()[args.cell].id == 'barcode') {
                        item = args.grid.getDataItem(args.row);
                        $timeout(function () {

                            $scope.ShowImage = false;
                            if (item.employeecode != '') {
                                $scope.currentimageid = item.employeecode;
                                $scope.ShowImage = true;
                            }

                        }, 10);
                    }
                });
            }

            $scope.igrid.UpdateData($scope.BulkBillResult);
        }

        $scope.EstimateBillConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '500px', '100%', false, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.EstimateBillResult);

            if ($scope.SearchCondition.showdetail) {
                igrid.AddSimpleColumn('counter', 'Counter', 120);
                igrid.AddSimpleColumn('employeename', 'Biller', 150);
                igrid.AddSimpleColumn('customer', 'Customer', 120);
                igrid.AddSimpleColumn('mobileno', 'Mobile No', 80);
                igrid.AddSimpleColumn('billno', 'Estimate No', 120);
                igrid.AddSimpleColumn('returnbillno', 'Bill No', 100);
                igrid.AddFormatedColumn('billdate', 'Bill Date', 120, 'dd-MM-yyyy hh:mm a');
                igrid.AddReferenceColumn('billtype', 'Bill Type', 120, $scope.BillDisplayType);
                igrid.AddSimpleColumn('barcode', 'Barcode', 100);
                igrid.AddFormatedColumn('rate', 'Rate', 80, 'N2', 2).total = true;
                igrid.AddSimpleColumn('itemname', 'Product', 120);
                igrid.AddSimpleColumn('sectionname', 'Section', 120);
                igrid.AddReferenceColumn('brandid', 'Brand', 100, $scope.Brand);
                igrid.AddReferenceColumn('sizeid', 'Size', 100, $scope.Sizes);
                igrid.AddFormatedColumn('saleqty', 'Estimate Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('billedqty', 'Billed Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('discount', 'Discount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('addlndiscount', 'Addln. Discount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('taxable', 'Taxable Amount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('rounding', 'Rounding', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('amount', 'Net Amount', 100, 'N2', 2).total = true;
            }
            else {
                igrid.AddSimpleColumn('location', 'Location', 150);
                igrid.AddSimpleColumn('counter', 'Counter', 120);
                igrid.AddSimpleColumn('employeename', 'Biller', 100);
                igrid.AddSimpleColumn('modifiedemployeename', 'Updatedby', 100);
                igrid.AddSimpleColumn('customer', 'Customer Name', 120);
                igrid.AddSimpleColumn('mobileno', 'Mobile', 100);
                igrid.AddReferenceColumn('billtype', 'Bill Type', 100, $scope.BillDisplayType);
                igrid.AddSimpleColumn('estimatebillno', 'Estimate Bill No', 120);
                igrid.AddSimpleColumn('billno', 'Bill No', 120);
                igrid.AddFormatedColumn('billdate', 'Bill Date', 120, 'dd-MM-yyyy hh:mm a');
                igrid.AddFormatedColumn('saleqty', 'Sale Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('saletax', 'Sale Tax', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('discount', 'Discount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('addlndiscount', 'Addln. Discount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('taxable', 'Taxable Amount', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('rounding', 'Rounding', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('amount', 'Net Amount', 120, 'N2', 2).total = true;
            }
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.EstimateBillResult);
        }


        $scope.DeliveryConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '500px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.DeliveryResult);

            igrid.AddSimpleColumn('location', 'Location', 150);
            igrid.AddSimpleColumn('counter', 'Bill Counter', 120);
            igrid.AddSimpleColumn('billno', 'Bill No', 120);
            igrid.AddFormatedColumn('billdate', 'Bill Date', 120, 'dd/MM/yyyy hh:mm:ss');
            igrid.AddFormatedColumn('qty', 'Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('amount', 'Net Amount', 80, 'N2', 2).total = true;
            igrid.AddSimpleColumn('deliverycounter', 'Delivery Counter', 120);
            igrid.AddReferenceColumn('status', 'Status', 100, $scope.DeliveryStatus);
            igrid.AddSimpleColumn('receivedby', 'Received By', 120);
            igrid.AddFormatedColumn('receivedon', 'Received on', 120, 'dd/MM/yyyy hh:mm:ss');
            igrid.AddSimpleColumn('verifiedby', 'Verified By', 120);
            igrid.AddFormatedColumn('verifiedon', 'Verified On', 120, 'dd/MM/yyyy hh:mm:ss');
            igrid.AddSimpleColumn('deliveredby', 'Delivered By', 120);
            igrid.AddFormatedColumn('deliveredon', 'Delivered On', 120, 'dd/MM/yyyy hh:mm:ss');

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.DeliveryResult);
        }

        $scope.DeliveryStatusConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '500px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.DeliveryStatusResult);

            igrid.AddSimpleColumn('location', 'Location', 180);
            igrid.AddSimpleColumn('billno', 'Bill No', 150);

            if ($scope.SearchCondition.counterid == 0) {
                igrid.AddFormatedColumn('receivedon', 'Received Date', 100, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('employeename', 'Received By', 120);
            }
            if ($scope.SearchCondition.counterid == 1) {
                igrid.AddFormatedColumn('deliveredon', 'Delivered Date', 100, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('deliveredby', 'Delivered By', 120);
            }
            if ($scope.SearchCondition.counterid == 2) {
                igrid.AddFormatedColumn('receivedon', 'Return Date', 100, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('returnedby', 'Returned By', 120);
                igrid.AddSimpleColumn('returnedto', 'Returned To', 120);
            }

            igrid.AddSimpleColumn('qty', 'Qty', 100).total = true;
            igrid.AddSimpleColumn('amount', 'Amount', 100).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.DeliveryStatusResult);
        }

        $scope.AlterationConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '500px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.DeliveryResult);

            igrid.AddSimpleColumn('location', 'Location', 150);
            igrid.AddSimpleColumn('billno', 'Bill No', 120);
            igrid.AddFormatedColumn('qty', 'Qty', 60, 'N2', 2).total = true;
            igrid.AddSimpleColumn('deliverycounter', 'Delivery Counter', 120);
            igrid.AddSimpleColumn('statusstring', 'Status', 100);
            igrid.AddSimpleColumn('receivedby', 'Received By', 120);
            igrid.AddFormatedColumn('receivedon', 'Received on', 120, 'dd/MM/yyyy hh:mm:ss');
            //igrid.AddSimpleColumn('verifiedby', 'Verified By', 120);
            //igrid.AddFormatedColumn('verifiedon', 'Verified On', 120, 'dd/MM/yyyy hh:mm:ss');
            igrid.AddSimpleColumn('deliveredby', 'Delivered By', 120);
            igrid.AddFormatedColumn('deliveredon', 'Delivered On', 120, 'dd/MM/yyyy hh:mm:ss');

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.DeliveryResult);
        }

        $scope.SalesVoidConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SalesBarcodeResult);

            igrid.AddSimpleColumn('counter', 'Counter', 80);
            igrid.AddSimpleColumn('biller', 'Biller', 80);
            igrid.AddSimpleColumn('glncode', 'EAN No', 100);
            igrid.AddSimpleColumn('barcode', 'Barcode', 80);
            igrid.AddFormatedColumn('mrp', 'MRP', 80, 'N2', 2);
            igrid.AddFormatedColumn('qty', 'Qty', 80, 'N2', 2).total = true;
            igrid.AddSimpleColumn('productname', 'Product', 150);
            igrid.AddSimpleColumn('hsncode', 'HSNCode', 80);
            igrid.AddSimpleColumn('suppliername', 'SupplierName', 150);
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 100);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 120, 'dd/MM/yyyy');
            igrid.AddSimpleColumn('measure', 'Measure', 80);
            igrid.AddSimpleColumn('brandname', 'Brand Name', 80);
            igrid.AddSimpleColumn('stylename', 'Style Name', 80);
            igrid.AddSimpleColumn('patternname', 'Pattern', 80);
            igrid.AddSimpleColumn('designid', 'Design', 80);
            igrid.AddSimpleColumn('sizename', 'Size Name', 80);
            igrid.AddSimpleColumn('colourname', 'Colour Name', 80);
            igrid.AddSimpleColumn('materialname', 'Material Name', 80);
            igrid.AddSimpleColumn('typename', 'Type Name', 80);
            igrid.AddSimpleColumn('billno', 'Bill No', 80);
            igrid.AddFormatedColumn('billdate', 'Bill Date', 100, 'dd/MM/yyyy');
            igrid.AddFormatedColumn('mrpvalue', 'MRP Value', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('buyingprice', 'Taxable Value(CP)', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxoncostprice', 'Tax Value(CP)', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('ibuyingprice', 'Cost Value', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxableamount', 'Taxable (SP)', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('tax', 'Tax (SP)', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('receivable', 'Sale Value', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 80, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SalesBarcodeResult);
        }

        $scope.SalesBarcodeConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SalesBarcodeResult);
            igrid.AddSimpleColumn('companyname', 'Company', 100);
            igrid.AddSimpleColumn('locationname', 'Location', 100);

            var itbscode = $scope.SalesBarcodeResult.find(function (item) {
                return item.itbuscode && item.itbuscode != '';
            })

            if (itbscode && itbscode != null) {
                igrid.AddSimpleColumn('itbuscode', 'Item Code', 100);
            }
            
            igrid.AddSimpleColumn('glncode', 'EAN No', 100);
            igrid.AddSimpleColumn('barcode', 'Barcode', 80);
            igrid.AddSimpleColumn('productname', 'Product', 150);
            igrid.AddSimpleColumn('sectionname', 'Section', 100);
            igrid.AddSimpleColumn('hsncode', 'HSNCode', 80);
            igrid.AddSimpleColumn('suppliername', 'SupplierName', 150);
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 100);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 120, 'dd/MM/yyyy');
            igrid.AddSimpleColumn('measure', 'Measure', 80);
            igrid.AddSimpleColumn('brandname', 'Brand Name', 80);
            igrid.AddSimpleColumn('stylename', 'Style Name', 80);
            igrid.AddSimpleColumn('patternname', 'Pattern', 80);
            igrid.AddSimpleColumn('designid', 'Design', 80);
            igrid.AddSimpleColumn('sizename', 'Size Name', 80);
            igrid.AddSimpleColumn('colourname', 'Colour Name', 80);
            igrid.AddSimpleColumn('materialname', 'Material Name', 80);
            igrid.AddSimpleColumn('typename', 'Type Name', 80);
            igrid.AddSimpleColumn('billno', 'Bill No', 80);
            igrid.AddFormatedColumn('billdate', 'Bill Date', 100, 'dd/MM/yyyy');
            igrid.AddSimpleColumn('customername', 'Customer', 100);
            igrid.AddSimpleColumn('gstno', 'GST No', 100);
            igrid.AddReferenceColumn('posid', 'POS', 100, $scope.States);
            igrid.AddFormatedColumn('mrp', 'MRP', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('qty', 'Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('mrpvalue', 'MRP Value', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('buyingprice', 'Taxable Value(CP)', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxoncostprice', 'Tax Value(CP)', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('ibuyingprice', 'Cost Value', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxpercentage', 'Tax %', 80, 'N2', 2);
            igrid.AddFormatedColumn('igsttaxpercentage', 'IGST %', 80, 'N2', 2);
            igrid.AddFormatedColumn('cgsttaxpercentage', 'CGST %', 80, 'N2', 2);
            igrid.AddFormatedColumn('sgsttaxpercentage', 'SGST %', 80, 'N2', 2);
            igrid.AddFormatedColumn('taxableamount', 'Taxable (SP)', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('igst', 'IGST', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('cgst', 'CGST', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('sgst', 'SGST', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('tax', 'Tax (SP)', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('grossvalue', 'Gross Value', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discountpercent', 'Discount %', 80, 'N2', 2).average = true;
            igrid.AddFormatedColumn('addlndiscount', 'Addln Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('addlndiscountpercent', 'Addln Discount %', 80, 'N2', 2).average = true;
            igrid.AddFormatedColumn('schemediscount', 'Scheme Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('schemediscountpercent', 'Scheme Discount %', 80, 'N2', 2).average = true;
            igrid.AddFormatedColumn('totaldiscount', 'Tot.Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('totaldiscountpercent', 'Tot.Discount %', 80, 'N2', 2).average = true;
            igrid.AddFormatedColumn('receivable', 'Sale Value', 100, 'N2', 2).total = true;

            if ($scope.SearchCondition.reporttype == "2")
                igrid.AddSimpleColumn('returnreason', 'Return Reason', 100);
            //igrid.AddFormatedColumn('taxpercentage', 'Tax%', 60, 'N2', 2);

            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                    $scope.$apply();
                }, 10);
            };

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SalesBarcodeResult);
        }

        $scope.ShowUpdateDialog = function () {

            var dialog = ngDialog.open({
                template: 'UpdateHSN',
                data: [$scope],
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.parent = $scope.ngDialogData[0];
                    $scope.UpdateHSN = function () {
                        $scope.closeThisDialog($scope.parent);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $scope.UpdateHSN();
                    }
                }
            });
        }
        $scope.UpdateHSN = function () {
            if ($scope.UpdateBillItem.hsncode == 'undefined' || $scope.UpdateBillItem.hsncode == null
                || $scope.UpdateBillItem.hsncode == "") {
                $rootScope.Toaster.Error("Please enter new hsncode");
                return;
            }

            var seletedRows = this.igrid.GetSelectedRows();
            var barcodes = [];
            $.each(seletedRows, function (k, v) {
                barcodes.push(v.barcode);
            });
            var webrequest = {
                "item": {
                    'hsncode': $scope.UpdateBillItem.hsncode, 'barcodes': barcodes
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(webrequest);
            $http({
                method: 'POST',
                url: "BillService/UpdateHSN",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("HSN code updated successfully.");
            }).error(function (result) {
            });
        }

        $scope.SalesHSNSummaryConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SalesHSNResult);

            igrid.AddSimpleColumn('hsncode', 'HSN CODE', 100);
            if (!$scope.SearchCondition.showdetail)
                igrid.AddSimpleColumn('productname', 'PRODUCT NAME', 120);
            igrid.AddSimpleColumn('unit', 'UNIT', 100);
            igrid.AddFormatedColumn('taxable', 'TAXABLE VALUE', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxpercentage', 'Tax%', 60, 'N2', 2);
            igrid.AddFormatedColumn('tax', 'TAX', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('igst', 'IGST', 80, 'N2', 2).total = true;;
            igrid.AddFormatedColumn('cgst', 'CGST', 80, 'N2', 2).total = true;;
            igrid.AddFormatedColumn('sgst', 'SGST', 80, 'N2', 2).total = true;;
            igrid.AddFormatedColumn('qty', 'TOTAL QTY', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('value', 'TOTAL VALUE', 100, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SalesHSNResult);
        }

        $scope.MissingStockConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.MissingStockResult);

            igrid.AddSimpleColumn('location', 'LOCATION', 150);
            igrid.AddSimpleColumn('barcode', 'BARCODE', 100);
            igrid.AddSimpleColumn('glncode', 'GLN CODE', 100);
            igrid.AddSimpleColumn('suppliername', 'SUPPLIER NAME', 150);
            igrid.AddSimpleColumn('invoiceno', 'INVOICE NO', 80);
            igrid.AddFormatedColumn('invoicedate', 'INVOICE DATE', 100, 'dd/MM/yyyy');
            igrid.AddSimpleColumn('itemname', 'PRODUCT NAME', 120);
            igrid.AddFormatedColumn('buyingprice', 'BUYING PRICE', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('price', 'SELLING PRICE', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('iqty', 'QTY', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('whqty', 'STOCK', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'DISCOUNT', 80, 'N2', 2).total = true;;
            igrid.AddFormatedColumn('employeename', 'CREATED BY', 100);
            igrid.AddFormatedColumn('createdon', 'CREATED ON', 120, 'dd/MM/yyyy');

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.MissingStockResult);
        }

        $scope.StockMarkerConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.StockMarkerList);

            igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            igrid.AddSimpleColumn('suppliername', 'Supplier', 120);
            igrid.AddSimpleColumn('productname', 'Product', 120);
            igrid.AddSimpleColumn('brandname', 'Brand', 100);
            igrid.AddSimpleColumn('sizename', 'Size', 80);
            igrid.AddFormatedColumn('markedqty', 'Stock When', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('qty', 'Stock Current', 80, 'N2', 2).total = true;
            igrid.AddSimpleColumn('materialname', 'Material', 100);
            igrid.AddSimpleColumn('patternname', 'Pattern', 100);
            igrid.AddSimpleColumn('stylename', 'Style', 100);
            igrid.AddSimpleColumn('colourname', 'Colour', 100);
            igrid.AddSimpleColumn('typename', 'Type', 100);
            igrid.AddSimpleColumn('hsncode', 'HSN Code', 100);

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.StockMarkerList);

        }

        $scope.DeleteCustomerPayment = function () {
            var seletedRows = $scope.igrid.GetSelectedRows();
            var ids = [];
            $.each(seletedRows, function (k, v) {
                ids.push(v.id);
            });
            var request = {
                "item": {
                    'ids': ids
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CustomerinvoiceService/Delete",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Notice('Customer Credit Item deleted - ' + ids.length);
            }).error(function (result) {
            });
        }

        $scope.CustomerPaymentConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            $scope.candeletependinginvoice = false;
            if ($scope.CustomerPaymentResult.length > 0 && $scope.CustomerPaymentResult[0].id > 0) {
                if (typeof ($scope.UserInfo.p_CUSTOMERPAYMENT.delete) != 'undefined' && $scope.UserInfo.p_CUSTOMERPAYMENT.delete) {
                    $scope.candeletependinginvoice = true;
                }
            }

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.CustomerPaymentResult);

            igrid.AddSimpleColumn('customername', 'CUSTOMER NAME', 150);
            igrid.AddSimpleColumn('mobileno', 'MOBILE NO', 80);
            igrid.AddSimpleColumn('gstno', 'GST NO', 80);
            igrid.AddSimpleColumn('area', 'AREA', 120);
            igrid.AddFormatedColumn('credit', 'BILL AMT', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'DISCOUNT', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid', 'PAID', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('balance', 'BALANCE', 100, 'N2', 2).total = true;
            if ($scope.SearchCondition.showdetail == true) {
                igrid.AddSimpleColumn('billno', 'BILL NO', 100);
                igrid.AddFormatedColumn('transactiondate', 'BILL DATE', 120, 'dd/MM/yyyy');
                igrid.AddReferenceColumn('locationid', 'LOCATION', 100, $scope.Location);
            }
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.CustomerPaymentResult);
        }

        $scope.CustomerCreditNoteConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.CustomerCreditNoteResult);

            igrid.AddSimpleColumn('vouchertype', 'TYPE', 100);
            igrid.AddSimpleColumn('issuedbyname', 'ISSUED BY', 100);
            igrid.AddSimpleColumn('countername', 'COUNTER NAME', 100);
            igrid.AddSimpleColumn('issuetocustomername', 'ISSUE TO', 150);
            igrid.AddSimpleColumn('issuetomobileno', 'MOBILE NO', 80);
            igrid.AddFormatedColumn('vouchervalue', 'CREDIT AMOUNT', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('validfrom', 'ISSUED ON', 80, 'dd/MM/yyyy');
            igrid.AddFormatedColumn('validto', 'VALID UPTO', 80, 'dd/MM/yyyy');
            igrid.AddSimpleColumn('vouchercode', 'CODE', 120);
            igrid.AddSimpleColumn('billno', 'CLAIMED BILL NO', 80);
            igrid.AddFormatedColumn('billdate', 'C.BILL DATE', 80, 'dd/MM/yyyy');
            igrid.AddSimpleColumn('customername', 'C.BILLED TO', 150);
            igrid.AddSimpleColumn('mobileno', 'C.MOBILE NO', 80);

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.CustomerCreditNoteResult);
        }

        $scope.SMMatrixIncentive = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            var markers = [];
            var SalesmanResult = [];
            $.each($scope.SalesmanResult, function (k, v) {

                if (markers.indexOf(v.markername) < 0)
                    markers.push(v.markername);

                var exist = SalesmanResult.find(function (item) {
                    return item.employeecode == v.employeecode &&
                        item.employeename == v.employeename;
                });

                if (exist == undefined || exist == null) {
                    exist = {
                        'employeecode': v.employeecode,
                        'employeename': v.employeename,
                        'floor': v.floor,
                        'section': v.section,
                        'items': []
                    };
                    SalesmanResult.push(exist);
                }

                exist.items.push({
                    'markername': v.markername,
                    'qty': v.qty,
                    'amount': v.amount,
                    'incentive': v.incentive,
                });
            })

            igrid.EnableSerial(SalesmanResult);
            igrid.AddSimpleColumn('employeecode', 'EMPLOYEE CODE', 120);
            igrid.AddSimpleColumn('employeename', 'EMPLOYEE NAME', 180);
            igrid.AddSimpleColumn('floor', 'FLOOR', 100);
            igrid.AddSimpleColumn('section', 'SECTION', 120);
            var groupColumns = [];
            groupColumns.push(
                {
                    id: 'name',
                    name: 'Details',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            $.each(markers, function (k, v) {
                igrid.columns = [];

                var column = igrid.AddCustomColumn('qty' + k, 'SALE QTY', 60, function (item, col) {
                    var markvalue = item.items.find(function (sitem) {
                        return sitem.markername == col.internal;
                    });
                    if (markvalue && markvalue != null)
                        return markvalue.qty;
                    return '';
                });
                column.internal = v;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var markvalue = item.items.find(function (sitem) {
                        return sitem.markername == col.internal;
                    });
                    if (markvalue && markvalue != null)
                        return markvalue.qty;
                    return 0;
                }

                var column = igrid.AddCustomColumn('amount' + k, 'AMOUNT', 60, function (item, col) {
                    var markvalue = item.items.find(function (sitem) {
                        return sitem.markername == col.internal;
                    });
                    if (markvalue && markvalue != null)
                        return markvalue.amount;
                    return '';
                });
                column.internal = v;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var markvalue = item.items.find(function (sitem) {
                        return sitem.markername == col.internal;
                    });
                    if (markvalue && markvalue != null)
                        return markvalue.amount;
                    return 0;
                }

                var column = igrid.AddCustomColumn('incentive' + k, 'INCENTIVE', 60, function (item, col) {
                    var markvalue = item.items.find(function (sitem) {
                        return sitem.markername == col.internal;
                    });
                    if (markvalue && markvalue != null)
                        return markvalue.incentive;
                    return '';
                });
                column.internal = v;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var markvalue = item.items.find(function (sitem) {
                        return sitem.markername == col.internal;
                    });
                    if (markvalue && markvalue != null)
                        return markvalue.incentive;
                    return 0;
                }
                groupColumns.push(
                    {
                        id: 'mname' + k,
                        name: v,
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            })

            igrid.columns = [];
            var column = igrid.AddCustomColumn('qty', 'QTY', 60, function (item, col) {
                var qty = 0;
                $.each(item.items, function (k, v) {
                    qty += v.qty;
                });
                return qty;
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                var qty = 0;
                $.each(item.items, function (k, v) {
                    qty += v.qty;
                });
                return qty;
            }

            var column = igrid.AddCustomColumn('amount', 'AMOUNT', 60, function (item, col) {
                var amount = 0;
                $.each(item.items, function (k, v) {
                    amount += v.amount;
                });
                return Math.round(amount);
            });

            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                var amount = 0;
                $.each(item.items, function (k, v) {
                    amount += v.amount;
                });
                return Math.round(amount);
            }

            var column = igrid.AddCustomColumn('incentive', 'INCENTIVE', 60, function (item, col) {
                var incentive = 0;
                $.each(item.items, function (k, v) {
                    incentive += v.incentive;
                });
                return Math.round(incentive);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                var incentive = 0;
                $.each(item.items, function (k, v) {
                    incentive += v.incentive;
                });
                return incentive;
            }
            groupColumns.push(
                {
                    id: 'Total',
                    name: 'TOTAL',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            igrid.columns = groupColumns;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(SalesmanResult);
        }

        $scope.SalesManConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SalesmanResult);

            if ($scope.SearchCondition.hidebusinesstrend)
                igrid.AddFormatedColumn('billdate', 'BILL DATE', 100, 'dd/MM/yyyy');
            if ($scope.SearchCondition.integration) {
                igrid.AddFormatedColumn('billdate', 'BILL DATE', 100, 'dd/MM/yyyy');
                igrid.AddSimpleColumn('billno', 'BILL NO', 100);
            }

            if ($scope.SearchCondition.showdetail == false) {
                igrid.AddSimpleColumn('employeecode', 'EMPLOYEE CODE', 120);
                igrid.AddSimpleColumn('employeename', 'EMPLOYEE NAME', 180);
                igrid.AddSimpleColumn('floor', 'FLOOR', 100);
            }
            igrid.AddSimpleColumn('section', 'SECTION', 120);

            if ($scope.SearchCondition.showmarker) {

                if (!$scope.SearchCondition.showmarkerincentive) {
                    igrid.AddSimpleColumn('billno', 'BILL NO', 80);
                    igrid.AddSimpleColumn('barcode', 'BARCODE', 80);
                }
                igrid.AddSimpleColumn('markername', 'MARKER', 100);
                igrid.AddFormatedColumn('qty', 'SALE QTY', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('costprice', 'COST PRICE', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('icostprice', 'NET COST PRICE', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('amount', 'SALE VALUE', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('incentive', 'INCENTIVE', 100, 'N2', 2).total = true;
            }
            else {
                igrid.AddFormatedColumn('billcount', 'BILL COUNT', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('qty', 'SALE QTY', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('tax', 'SALE TAX', 100, 'N2', 2).total = true;
                if ($scope.SearchCondition.showdetail) {
                    igrid.AddFormatedColumn('costprice', 'COST PRICE', 100, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('icostprice', 'NET COST PRICE', 100, 'N2', 2).total = true;
                }
                igrid.AddFormatedColumn('taxableamount', 'TAXABLE VALUE', 120, 'N2', 2).total = true;
                igrid.AddFormatedColumn('amount', 'SALE VALUE', 100, 'N2', 2).total = true;
            }
            if ($scope.SearchCondition.integration) 
                igrid.AddFormatedColumn('discount', 'DISCOUNT', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('additionaldiscount', 'GROSS VALUE', 100, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SalesmanResult);
        }

        $scope.ShowUpdateSalesmanDialog = function () {

            var dialog = ngDialog.open({
                template: 'UpdateSalesman',
                data: [$scope],
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.parent = $scope.ngDialogData[0];
                    $scope.UpdateSalesman = function () {
                        $scope.closeThisDialog($scope.parent);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $scope.UpdateSalesMan();
                    }
                }
            });
        }

        $scope.UpdateSalesMan = function () {
            var seletedRows = this.igrid.GetSelectedRows();
            var ids = [];
            $.each(seletedRows, function (k, v) {
                ids.push(v.billitemid);
            });

            if (ids.length == 0) {
                $rootScope.Toaster.Error("Please select bill.");
                return;
            }
            var webrequest = {
                "item": {
                    'customerid': $scope.UpdateSalesmanCode.salesmanid, 'ids': ids
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(webrequest);
            $http({
                method: 'POST',
                url: "BillService/UpdateSalesman",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Salesman code updation completed successfully.");
            }).error(function (result) {
            });
        }


        $scope.SalesManDetailConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '500px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SalesmanDetailResult);

            igrid.AddSimpleColumn('employeecode', 'EMP CODE', 80);
            igrid.AddSimpleColumn('employeename', 'EMPLOYEE NAME', 160);
            igrid.AddSimpleColumn('floor', 'EMP.FLOOR', 120);
            igrid.AddSimpleColumn('section', 'EMP.SECTION', 120);
            igrid.AddSimpleColumn('billno', 'BILL NO', 100);
            igrid.AddFormatedColumn('billdate', 'BILL DATE', 100, 'dd/MM/yyyy');
            igrid.AddSimpleColumn('barcode', 'BARCODE', 100);
            igrid.AddSimpleColumn('countername', 'SALE COUNTER', 100);
            igrid.AddSimpleColumn('productname', 'PRODUCT NAME', 120);
            igrid.AddFormatedColumn('invoicedate', 'INVOICE DATE', 100, 'dd/MM/yyyy');
            igrid.AddReferenceColumn('salescounterid', 'SECTION', 120, $scope.SalesCounter);
            igrid.AddSimpleColumn('brandname', 'BRAND', 100);
            igrid.AddSimpleColumn('stylename', 'STYLE', 100);
            igrid.AddSimpleColumn('sizename', 'SIZE', 100);
            igrid.AddCustomColumn('islastbit', 'LAST BIT', 60, function (item, col) {
                return item.islastbit == true ? "YES" : "NO";
            });
            igrid.AddFormatedColumn('lastbitqty', 'LAST BIT QTY', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('qty', 'SALE QTY', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('price', 'PRICE', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'DISCOUNT', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('additionaldiscount', 'ADDLN.DISCOUNT', 100, 'N2', 2).total = true;
            igrid.AddSimpleColumn('schemename', 'SCHEME NAME', 80);
            igrid.AddFormatedColumn('schemediscount', 'SCH.DISCOUNT', 80, 'N2', 2).total = true;
            igrid.AddSimpleColumn('markername', 'MARKER NAME', 80);
            igrid.AddFormatedColumn('taxableamount', 'TAXABLE AMT', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('tax', 'TAX AMT', 80, 'N2', 2).total = true;

            var column = igrid.AddCustomColumn('grossamount', 'G.SALE VALUE', 80, function (item, col) {
                return (item.amount + item.discount + item.additionaldiscount + item.schemediscount).toFixed(2);
            });

            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return (item.amount + item.discount + item.additionaldiscount + item.schemediscount).toFixed(2);
            }

            igrid.AddFormatedColumn('amount', 'SALE VALUE', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('incentive', '* Incentive', 100, 'N2', 2).total = true;

            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                    $scope.$apply();
                }, 10);
            };

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SalesmanDetailResult);

        }
        $scope.StockConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '500px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.StockResult);

            if ($scope.SearchCondition.locationid == null || $scope.SearchCondition.locationid == 0)
                igrid.AddReferenceColumn('locationid', 'Location', 120, $scope.Location);
            igrid.AddSimpleColumn('suppliername', 'Supplier Name', 150);
            igrid.AddFormatedColumn('createdon', 'Stock Date', 100, 'dd/MM/yyyy');
            igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            igrid.AddSimpleColumn('glncode', 'GLN Code', 100);
            igrid.AddSimpleColumn('itbuscode', 'Item Code', 100);
            igrid.AddSimpleColumn('itemname', 'Item Name', 120);
            igrid.AddSimpleColumn('hsncode', 'HSN Code', 80);
            igrid.AddReferenceColumn('productgroupid', 'Product Group', 100, $scope.ProductGroups);
            igrid.AddSimpleColumn('productname', 'Product', 100);
            igrid.AddFormatedColumn('iqty', 'In.Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('oqty', 'Sold Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('dqty', 'DM. Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('qty', 'Stock', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('stockage', 'Age(days)', 60, 'N2', 0);
            igrid.AddFormatedColumn('stockholdingperiod', 'Holding Period', 80, 'N2', 2);
            igrid.AddFormatedColumn('buyingprice', 'Taxable Cost', 80, 'N2', 2);
            igrid.AddFormatedColumn('ibuyingprice', 'Cost Price', 80, 'N2', 2);
            igrid.AddFormatedColumn('taxvalue', 'Tax Price', 80, 'N2', 2);
            igrid.AddFormatedColumn('mrp', 'MRP', 80, 'N2', 2);
            igrid.AddFormatedColumn('price', 'Price', 80, 'N2', 2);
            igrid.AddFormatedColumn('discount', 'Discount', 80, 'N2', 2);
            igrid.AddFormatedColumn('discountpercentage', 'Discount %', 80, 'N2', 2).average = true;
            igrid.AddFormatedColumn('dealerprice', 'Dealer Price', 80, 'N2', 2);

            var column = igrid.AddCustomColumn('price', 'Sale Price', 80, function (item, col) {
                if (typeof (item.price) != 'undefined' && typeof (item.discount) != 'undefined') {
                    return (item.price - item.discount).toFixed(2);
                }
            });

            column.cssClass = 'rightalign';
            //column.total = true;
            column.getValue = function (item, col) {
                if (typeof (item.price) != 'undefined' && typeof (item.discount) != 'undefined') {
                    return (item.price - item.discount);
                }
                return 0;
            }

            //igrid.AddFormatedColumn('mrp', 'Sale Price', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('margin', 'Margin', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxablevalue', 'Taxable Value', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('stockvalue', 'Cost Value', 80, 'N2', 2).total = true;

            var column1 = igrid.AddCustomColumn('taxvalue', 'Tax Value', 80, function (item, col) {
                if (typeof (item.taxvalue) != 'undefined' && typeof (item.qty) != 'undefined') {
                    return (item.taxvalue * item.qty).toFixed(2);
                }
            });

            column1.cssClass = 'rightalign';
            column1.total = true;
            column1.getValue = function (item, col) {
                if (typeof (item.taxvalue) != 'undefined' && typeof (item.qty) != 'undefined') {
                    return (item.taxvalue * item.qty);
                }
                return 0;
            }

            igrid.AddFormatedColumn('salevalue', 'Sale Value', 80, 'N2', 2).total = true;
            igrid.AddReferenceColumn('sectionid', 'Section', 120, $scope.SalesCounter);
            igrid.AddReferenceColumn('brandid', 'Brand', 90, $scope.Brand);
            igrid.AddSimpleColumn('designid', 'Design', 90);
            igrid.AddReferenceColumn('materialid', 'Material', 90, $scope.Material);
            igrid.AddReferenceColumn('patternid', 'Pattern', 90, $scope.Pattern);
            igrid.AddReferenceColumn('styleid', 'Style', 90, $scope.Style);
            igrid.AddReferenceColumn('colourid', 'Colour', 90, $scope.Colour);
            igrid.AddReferenceColumn('sizeid', 'Size', 60, $scope.Sizes);
            igrid.AddReferenceColumn('typeid', 'Type', 60, $scope.Type);
            igrid.AddSimpleColumn('pricetagname', 'PriceTag', 60);
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.StockResult);

        }

        $scope.InwardConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.InwardResult);

            if ($scope.SearchCondition.showdetail == false) {
                igrid.AddSimpleColumn('code', 'Package No', 120);
                igrid.AddSimpleColumn('fromlocation', 'From Location', 150);
                igrid.AddSimpleColumn('tolocation', 'To Location', 120);
                igrid.AddFormatedColumn('packedon', 'Packed On', 120, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('receivedon', 'Received On', 120, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('receivedby', 'Received By', 150);
                igrid.AddSimpleColumn('qty', 'Qty', 100).total = true;
            }

            if ($scope.SearchCondition.showdetail == true) {
                igrid.AddReferenceColumn('supplierid', 'Supplier', 300, $scope.Supplier);
                igrid.AddSimpleColumn('lrreference', 'LR Reference No', 180);
                igrid.AddSimpleColumn('barcode', 'Barcode', 170);
                igrid.AddSimpleColumn('printingname', 'Printing Name', 250);
                igrid.AddSimpleColumn('iqty', 'Qty', 130).total = true;
            }

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.InwardResult);


        }

        $scope.InwardDetailsConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.InwardDetailsResult);

            igrid.AddReferenceColumn('supplierid', 'Supplier', 300, $scope.Supplier);
            igrid.AddSimpleColumn('lrreference', 'LR Reference No', 180);
            igrid.AddSimpleColumn('barcode', 'Barcode', 170);
            igrid.AddSimpleColumn('printingname', 'Printing Name', 250);
            igrid.AddSimpleColumn('iqty', 'Qty', 130).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.InwardDetailsResult);
        }

        $scope.SalesSettlementConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SaleVsSettlementResult);

            igrid.AddReferenceColumn('locationid', 'Location', 180, $scope.AllLocation);
            igrid.AddSimpleColumn('billno', 'Bill No', 100);
            igrid.AddFormatedColumn('billdate', 'Bill Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('mobileno', 'Customer No', 100);
            igrid.AddSimpleColumn('customername', 'Customer Name', 100);
            igrid.AddFormatedColumn('grossvalue', 'Bill Gross', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('billdiscount', 'Bill Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('billamount', 'Bill Amount', 100, 'N2', 2).total = true;
            igrid.AddSimpleColumn('settlementno', 'Set No', 100);
            igrid.AddFormatedColumn('settlementdate', 'Set Date', 120, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('paid', 'Paid', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('cash', 'Cash', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('card', 'Card', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('upi', 'UPI', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid_credit', 'Credit', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('adjustment_paid', 'Adjustment', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('return_cash', 'Return', 100, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.SaleVsSettlementResult);
        }

        $scope.SalesStokConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchSaleVsStockResult);

            igrid.AddSimpleColumn('suppliername', 'Supplier', 100);
            igrid.AddSimpleColumn('productname', 'Product', 100);
            igrid.AddSimpleColumn('design', 'Design', 100);
            igrid.AddFormatedColumn('cost', 'Cost', 120, 'N2', 2).total = true;

            igrid.AddFormatedColumn('stockqty', 'Stock Qty', 120, 'N2', 2).total = true;
            igrid.AddFormatedColumn('stockvalue', 'Stock Value', 120, 'N2', 2).total = true;
            igrid.AddFormatedColumn('salesqty', 'Sales Qty', 120, 'N2', 2).total = true;
            igrid.AddFormatedColumn('salesvalue', 'Sales Value', 120, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.SearchSaleVsStockResult);
        }

        $scope.SalesStokPurchaseConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchSaleVsStockResult);
            if ($scope.SearchConditionStkTrans.showlocation)
                igrid.AddSimpleColumn('locationname', 'Location', 100);

            if ($scope.SearchConditionStkTrans.showsupplier)
                igrid.AddSimpleColumn('suppliername', 'Supplier', 100);
            if ($scope.SearchConditionStkTrans.showbrand)
                igrid.AddSimpleColumn('brandname', 'Brand', 100);
            if ($scope.SearchConditionStkTrans.showproduct)
                igrid.AddSimpleColumn('productname', 'Product', 100);

            if ($scope.SearchConditionStkTrans.showdesign)
                igrid.AddSimpleColumn('design', 'Design', 100);


            igrid.AddFormatedColumn('cost', 'Cost', 120, 'N2', 2).total = true;

            igrid.AddFormatedColumn('purchaseqty', 'Purchase Qty', 120, 'N2', 2).total = true;
            igrid.AddFormatedColumn('purchasevalue', 'Purchase Value', 120, 'N2', 2).total = true;

            igrid.AddFormatedColumn('salesqty', 'Sales Qty', 120, 'N2', 2).total = true;
            igrid.AddFormatedColumn('salesvalue', 'Sales Value', 120, 'N2', 2).total = true;

            igrid.AddFormatedColumn('stockqty', 'Stock Qty', 120, 'N2', 2).total = true;
            igrid.AddFormatedColumn('stockvalue', 'Stock Value', 120, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.SearchSaleVsStockResult);
        }

        $scope.SalesAnalysisCnfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SalesAnalysisResult);

            if ($scope.SearchCondition.supplier) {
                igrid.AddReferenceColumn('supplierid', 'Supplier', 150, $scope.Supplier);
            }
            if ($scope.SearchCondition.sourcesupplier) {
                igrid.AddReferenceColumn('sourcesupplierid', 'Source Supplier', 150, $scope.Supplier);
            }
            if ($scope.SearchCondition.issuppliercity) {
                igrid.AddReferenceColumn('suppliercityid', 'Supplier City', 150, $scope.City);
            }
            if ($scope.SearchCondition.section) {
                igrid.AddReferenceColumn('sectionid', 'Section', 120, $scope.Section);
            }
            if ($scope.SearchCondition.agent) {
                igrid.AddReferenceColumn('agentid', 'Agent', 150, $scope.Agent);
            }
            if ($scope.SearchCondition.isbarcode)
                igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            if ($scope.SearchCondition.isgln)
                igrid.AddSimpleColumn('glncode', 'GLN Code', 100);
            if ($scope.SearchCondition.product)
                igrid.AddSimpleColumn('productname', 'Product Name', 150);
            if ($scope.SearchCondition.brand) {
                igrid.AddSimpleColumn('brandname', 'Brand', 100);
            }
            if ($scope.SearchCondition.material) {
                igrid.AddSimpleColumn('materialname', 'Material', 100);
            }
            if ($scope.SearchCondition.pattern) {
                igrid.AddSimpleColumn('patternname', 'Pattern', 100);
            }
            if ($scope.SearchCondition.style) {
                igrid.AddSimpleColumn('stylename', 'Style', 150);
            }
            if ($scope.SearchCondition.size) {
                igrid.AddSimpleColumn('sizename', 'Size', 100);
            }
            if ($scope.SearchCondition.design) {
                igrid.AddSimpleColumn('designid', 'Design', 100);
            }

            igrid.AddFormatedColumn('qty', 'Qty', 80, 'N', 2).total = true;
            igrid.AddFormatedColumn('taxvalue', 'Tax', 80, 'N', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 100, 'N', 2).total = true;
            igrid.AddFormatedColumn('addlndiscount', 'Addin.Discount', 100, 'N', 2).total = true;
            igrid.AddFormatedColumn('schemediscount', 'Sch.Discount', 100, 'N', 2).total = true;
            igrid.AddFormatedColumn('grosssalevalue', 'Sale Value', 100, 'N', 2).total = true;
            igrid.AddFormatedColumn('netsalevalue', 'Taxable SP', 100, 'N', 2).total = true;
            igrid.AddFormatedColumn('taxvalue', 'Tax SP', 100, 'N', 2).total = true;
            igrid.AddFormatedColumn('price', 'Act. Total SP', 100, 'N', 2).total = true;
            igrid.AddFormatedColumn('stockvalue', 'Taxable CP', 100, 'N', 2).total = true;
            igrid.AddFormatedColumn('ibuyingprice', 'Tax CP', 100, 'N', 2).total = true;
            igrid.AddFormatedColumn('istockvalue', 'Total CP', 100, 'N', 2).total = true;
            igrid.AddFormatedColumn('marginpercentage', 'Markup', 50, 'N', 2);
            igrid.AddFormatedColumn('markdown', 'Markdown', 70, 'N', 2);

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.SalesAnalysisResult);
        }

        $scope.StockAuditConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.StockAuditList);

            igrid.AddFormatedColumn('modifiedon', 'Audited On', 150, 'dd-MM-yyyy hh:mm a');
            igrid.AddSimpleColumn('empname', 'Modified By', 160);
            igrid.AddSimpleColumn('productname', 'Product Name', 100);
            igrid.AddSimpleColumn('barcode', 'Barcode', 80);
            igrid.AddFormatedColumn('price', 'Price*', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount*', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('currentqty', 'Current QTY', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('whenqty', 'Old QTY', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('auditqty', 'Audit QTY', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('auditvalue', 'Audit Value', 100, 'N2', 2).total = true;
            igrid.AddSimpleColumn('reason', 'Reason', 120);

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.StockAuditList);

        }

        $scope.StockSplitConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.StockAuditList);

            igrid.AddFormatedColumn('createdon', 'Created On', 150, 'dd-MM-yyyy hh:mm a');
            igrid.AddSimpleColumn('createdemployee', 'Created by', 100);
            igrid.AddSimpleColumn('barcode', 'Source Barcode', 100);
            igrid.AddSimpleColumn('newbarcode', 'New barcode', 100);
            igrid.AddFormatedColumn('noofqtyeachbarcode', 'Split Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('stockqty', 'Stock Qty', 80, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.StockAuditList);

        }

        $scope.PriceChangeConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.PriceChangerList);

            igrid.AddSimpleColumn('locationname', 'Location', 100);
            igrid.AddFormatedColumn('createdon', 'Created On', 150, 'dd-MM-yyyy hh:mm a');
            igrid.AddSimpleColumn('empname', 'Created By', 150);
            igrid.AddSimpleColumn('reason', 'Reason', 150);
            igrid.AddSimpleColumn('printingname', 'Product Name', 120);
            igrid.AddSimpleColumn('sectionname', 'Section', 80);
            igrid.AddSimpleColumn('barcode', 'Barcode', 80);
            igrid.AddFormatedColumn('stockqty', 'Stock', 80, 'N2', 2);
            igrid.AddFormatedColumn('buyingprice', 'Cost Price', 80, 'N2', 2);
            igrid.AddFormatedColumn('totalcost', 'Total Cost', 80, 'N2', 2).total = true;
            if ($scope.SupportDealerPrice) {
                igrid.AddFormatedColumn('odealerprice', 'O.Dealer Price', 90, 'N2', 2).total = true;
                igrid.AddFormatedColumn('ndealerprice', 'N.Dealer Price', 90, 'N2', 2).total = true;
            }
            igrid.AddFormatedColumn('oprice', 'O.Price', 90, 'N2', 2).total = true;
            igrid.AddFormatedColumn('odiscount', 'O.Discount', 90, 'N2', 2).total = true;
            igrid.AddFormatedColumn('nprice', 'N.Price', 90, 'N2', 2).total = true;
            igrid.AddFormatedColumn('ndiscount', 'N.Discount', 90, 'N2', 2).total = true;
            igrid.AddFormatedColumn('changevalue', 'Difference Amt', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('totalchangevalue', 'Margin Diff Amt', 80, 'N2', 2).total = true;
            //igrid.AddButtonColumn('action', 'Action', 50, 'View', function (e, args) {

            //    $scope.$apply(function () {
            //        $scope.ViewItem('VIEW', args.item);
            //    });
            //    return true;
            //});
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.PriceChangerList);
        }

        $scope.IncentiveConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.Incentives);

            igrid.AddSimpleColumn('sectionname', 'SECTION', 100);
            igrid.AddSimpleColumn('qty', 'SALE QTY', 80).total = true;
            igrid.AddSimpleColumn('amount', 'INCENTIVE AMOUNT', 150).total = true;

            if ($scope.SearchCondition.employeeid >= 1) {
                igrid.AddSimpleColumn('employeename', 'EMPLOYEE NAME', 150);
                igrid.AddSimpleColumn('billno', 'BILL NO', 100);
                igrid.AddFormatedColumn('billdate', 'BILL DATE', 120, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('price', 'BILL AMOUNT', 100).total = true;
                igrid.AddSimpleColumn('from', 'INC.RANGE FROM', 100);
                igrid.AddSimpleColumn('to', 'INC.RANGE FROM', 100);
            }

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.Incentives);

        }

        $scope.EmployeeConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, false, true, 0);

            igrid.AddSimpleColumn('employeecode', 'EMP ID', 100);
            igrid.AddSimpleColumn('name', 'Name', 250);
            igrid.AddSimpleColumn('gender', 'Gender', 50);
            igrid.AddFormatedColumn('dateofbirth', 'Date Of Birth', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('grosspay', 'Gross Salary', 100, 'N2', 2);
            igrid.AddFormatedColumn('dateofjoining', 'Date Of Joining', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('bloodgroup', 'Blood Group', 100);
            igrid.AddSimpleColumn('caste', 'Caste', 100);
            igrid.AddSimpleColumn('religion', 'Religion', 100);
            igrid.AddSimpleColumn('emailid', 'EmailId', 100);
            igrid.AddSimpleColumn('contactno', 'Contact No', 100);
            igrid.AddSimpleColumn('personal.aadharno ', 'Aadhar No', 100);
            igrid.AddSimpleColumn('pfandesi.pfno ', 'PF No', 100);
            igrid.AddSimpleColumn('pfandesi.pfuan ', 'PF UAN', 100);
            igrid.AddSimpleColumn('pfandesi.esino', 'ESI No', 100);
            igrid.AddReferenceColumn('floorid', 'Floor', 100, $scope.Floors);
            igrid.AddReferenceColumn('sectionid', 'Section', 100, $scope.Section);
            igrid.AddReferenceColumn('desingnationid', 'Desingnation', 100, $scope.Designations);
            igrid.AddReferenceColumn('locationid', 'Locations', 100, $scope.Locations);
            igrid.AddSimpleColumn('emppresentaddress', 'Present Address', 100);
            igrid.AddSimpleColumn('emppermanentaddress', 'Permanent Address', 100);

            //var commands = "<input type='button' class='btn btn-primary btn-xs' value='Edit'/>"
            //commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"
            //commands += "<input type='button' class='btn btn-danger btn-xs' value='Print'/>"
            //igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
            //    $scope.$apply(function () {
            //        if ($(e.target).val() == 'Edit')
            //            $scope.ManageSearchItem('Edit', args.item);
            //        else if ($(e.target).val() == 'Delete')
            //            $scope.ManageSearchItem('Delete', args.item);
            //        else if ($(e.target).val() == 'Print')
            //            $scope.ManageSearchItem('Print', args.item);
            //    });
            //});


            //igrid.AddButtonColumn('Edit', 'Action', 110, 'Edit', function (e, args) {

            //    $scope.$apply(function () {
            //        $scope.ManageSearchItem('Edit', args.item);
            //    });
            //    return true;
            //});

            //igrid.AddButtonColumn('Delete', '', 110, 'Delete', function (e, args) {

            //    $scope.$apply(function () {
            //        $scope.ManageSearchItem('Delete', args.item);
            //    });
            //    return true;
            //});

            //igrid.AddButtonColumn('id', '', 100, 'Print', function (e, args) {

            //    $scope.$apply(function () {
            //        $scope.ManageSearchItem('id', args.item);
            //    });
            //    return true;
            //});


            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);

        }

        $scope.PrintAllBarcode = function () {
            var ids = [];
            var _selectindex = 1;
            var seletedRows = this.igrid.GetSelectedRows();
            $.each(seletedRows, function (k, v) {
                ids.push({
                    'barcode': v.barcode,
                    'selectindex': _selectindex++,
                    'noofcopy': (v.iqty == 'undefined' || v.iqty == null) ? 1 : v.iqty
                });
            });

            requestitem = {
                'iswarehouse': ($scope.SearchCondition.mode == 1),
                'locationid': $scope.SearchCondition.locationid,
                'barcodes': ids, 'reportid': 0
            };
            $rootScope.Handlers.SelectPrinter('BARCODE', function (printdata) {
                if (printdata == null || printdata.report == null || printdata.report.id <= 0)
                    return;

                requestitem.reportid = printdata.report.id;

                var request = {
                    "Item": requestitem,
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "PricechangeService/PrintBarcode",
                    data: srequest
                }).success(function (result) {

                    if (result != null && result.length > 0) {
                        $rootScope.Sent2Printer(result.join(';'), printdata);
                    }
                });
            });
        };


        $scope.SalesTaxConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SalesTaxResult.taxbreakup);

            igrid.AddFormatedColumn('billdate', 'Bill Date', 120, 'dd-MM-yyyy');
            if ($scope.SearchCondition.showdetail) {
                igrid.AddSimpleColumn('billno', 'Bill No', 100);
                igrid.AddSimpleColumn('statecode', 'ST.Code', 100);
                var column = igrid.AddCustomColumn('isinterstate', 'IGST', 100, function (item, col) {
                    return item.isinterstate;
                });
                column.getValue = function (item, col) {
                    return item.isinterstate;
                }
            }
            $.each($scope.SalesTaxResult.taxList, function (k, v) {

                var column = igrid.AddCustomColumn('tax_col_' + k, v.title, 100, function (item, col) {
                    var taxitem = col.internal;
                    var value = col.getTaxSummary(item, taxitem);
                    if(value && value != null)
                        return $filter('IndianNumber')(col.getTaxSummary(item, taxitem), 2);
                    return '';
                });

                column.internal = v;
                column.getTaxSummary = $scope.getTaxSummary;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var taxitem = col.internal;
                    var value = col.getTaxSummary(item, taxitem);
                    if (value && value != null)
                        return value;
                    return 0;
                }
            });

            if (!$scope.SearchCondition.showdetail)
                igrid.AddFormatedColumn('rounding', 'Rounding', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('totalvalue', 'Total', 100, 'N2', 2).total = true;

            igrid.options.autoEdit = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SalesTaxResult.taxbreakup);
        }

        $scope.printDaySummary = function () {

            var list = [];
            list.push($rootScope.PrintFormater.Bold($rootScope.UserInfo.CompanyName));
            list.push($rootScope.PrintFormater.Bold("         DAY SUMMARY"));
            list.push($rootScope.PrintFormater.Normal("__________________________________________"));
            list.push($rootScope.PrintFormater.Normal("  LOCATION : " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location)));

            var from = $filter('date')($scope.SearchCondition.fromdate, 'dd-MM-yyyy');
            var to = $filter('date')($scope.SearchCondition.todate, 'dd-MM-yyyy');
            if (from == to) {
                list.push($rootScope.PrintFormater.Normal("  DATE     : " + from));
            }
            else {
                list.push($rootScope.PrintFormater.Normal("  DATE     : " + from + " - " + to));
            }

            list.push($rootScope.PrintFormater.Normal("__________________________________________"));
            $.each($scope.DaySummary.Items, function (k, v) {
                var tline = [];
                tline.push($rootScope.PrintFormater.Align(v.name, 30, false))
                tline.push($rootScope.PrintFormater.Align(v.value, 10, true))
                list.push(tline.join(''));
            });

            list.push($rootScope.PrintFormater.Normal("__________________________________________"));

            list.push($rootScope.PrintFormater.Normal(""));
            list.push("\r\n");
            list.push("\r\n");
            list.push($rootScope.PrintFormater.Cutter());
            list.push("\r\n");

            var printdocument = window.btoa(list.join('\r\n'));
            $rootScope.Sent2Printer(printdocument);
        }

        $scope.printCounterSummary = function () {
            request = {
                "item": {
                    'fromdate': $scope.SearchCondition.fromdate, 'todate': $scope.SearchCondition.todate,
                    'companyid': $scope.SearchCondition.companyid, 'locationid': $scope.SearchCondition.locationid,
                    'counterid': $scope.SearchCondition.counterid
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchBillSummary",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                else {
                    var list = [];

                    if ($scope.SearchCondition.companyid == 0)
                        list.push($rootScope.PrintFormater.Bold($rootScope.UserInfo.CompanyName));
                    else
                        list.push($rootScope.PrintFormater.Bold($scope.SearchRef($scope.SearchCondition.companyid, $scope.Company)));

                    list.push($rootScope.PrintFormater.Bold("SALES SUMMARY REPORT"));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    list.push($rootScope.PrintFormater.Normal("  LOCATION     : " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location)));
                    list.push($rootScope.PrintFormater.Normal("  COUNTER     : " + $scope.SearchRef($scope.SearchCondition.counterid, $scope.Counter)));
                    list.push($rootScope.PrintFormater.Normal("  DATE BETWEEN : " + $filter('date')($scope.SearchCondition.fromdate, 'dd-MM-yyyy') + " AND " + $filter('date')($scope.SearchCondition.todate, 'dd-MM-yyyy')));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    list.push($rootScope.PrintFormater.Normal("  Sales Amount            (A) : " + $rootScope.PrintFormater.Align(result.totalsale, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("  Exchange Amount         (B) : " + $rootScope.PrintFormater.Align(result.returnvalue, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("  Cancel Amount           (C) : " + $rootScope.PrintFormater.Align(result.cancelvalue, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    list.push($rootScope.PrintFormater.Normal("  Total             (A-B-C) : " + $rootScope.PrintFormater.Align(result.netsale, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    list.push($rootScope.PrintFormater.Normal(""));
                    list.push($rootScope.PrintFormater.Normal("  BILL COUNT                  : " + $rootScope.PrintFormater.Align(result.billcount, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("  STARTING BILL NO            : " + $rootScope.PrintFormater.Align(result.startingbillno, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("  ENDING BILL NO              : " + $rootScope.PrintFormater.Align(result.endingbillno, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("  CANCELLED BILLS"));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    list.push($rootScope.PrintFormater.Normal("  BILL NO       AMOUNT"));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));

                    var amount = 0;
                    if (result.cancelledbills != 'undefined' && result.cancelledbills != null) {
                        $.each(result.cancelledbills, function (k, v) {
                            amount += v.amount;

                            var str = $rootScope.PrintFormater.Align(v.billno, 12, true);
                            str += $rootScope.PrintFormater.Align(v.amount, 10, true);
                            list.push($rootScope.PrintFormater.Normal(str));

                        });
                        list.push($rootScope.PrintFormater.Normal("___________________________________________"));
                        list.push($rootScope.PrintFormater.Normal("  Total     " + $rootScope.PrintFormater.Align(amount, 12, true)));
                        list.push($rootScope.PrintFormater.Normal("___________________________________________"));
                        list.push($rootScope.PrintFormater.Normal(""));
                    }
                    else {
                        list.push($rootScope.PrintFormater.Normal("  -                      0.00  "));
                    }
                    list.push("\r\n");
                    list.push("\r\n");
                    list.push($rootScope.PrintFormater.Cutter());
                    list.push("\r\n");

                    var printdocument = window.btoa(list.join('\r\n'));
                    $rootScope.Sent2Printer(printdocument);

                    //$scope.printsalestaxslip();
                }
            }).error(function (result) {
            });
        }

        $scope.printsettlementslip = function () {

            request = {
                "item": {
                    'fromdate': $scope.SearchCondition.fromdate, 'todate': $scope.SearchCondition.todate,
                    'companyid': $scope.SearchCondition.companyid, 'locationid': $scope.SearchCondition.locationid
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSettlement",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                else {
                    var list = [];
                    var totalcash = 0;
                    var totalcard = 0;
                    var totalcredit = 0;
                    var totaladjustment = 0;
                    var returnamount = 0;
                    var addlndiscount = 0;
                    $.each(result, function (k, v) {
                        totalcash += v.paid_cash;
                        totalcard += v.paid_card;
                        totalcredit += v.paid_credit;
                        totaladjustment += v.adjustment_paid;
                        returnamount += v.return_cash;
                        addlndiscount += v.discount;
                    });

                    if ($scope.SearchCondition.companyid == 0)
                        list.push($rootScope.PrintFormater.Bold($rootScope.UserInfo.CompanyName));
                    else
                        list.push($rootScope.PrintFormater.Bold($scope.SearchRef($scope.SearchCondition.companyid, $scope.Company)));

                    list.push($rootScope.PrintFormater.Bold("SALES RECONCILATION REPORT"));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    list.push($rootScope.PrintFormater.Normal("  LOCATION     : " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location)));
                    list.push($rootScope.PrintFormater.Normal("  DATE BETWEEN : " + $filter('date')($scope.SearchCondition.fromdate, 'dd-MM-yyyy') + " AND " + $filter('date')($scope.SearchCondition.todate, 'dd-MM-yyyy')));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));

                    list.push($rootScope.PrintFormater.Normal("  Cash Payment            (A) : " + $rootScope.PrintFormater.Align(totalcash, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("  Card Payment            (B) : " + $rootScope.PrintFormater.Align(totalcard, 10, true)));
                    //list.push($rootScope.PrintFormater.Normal("  Credit Sale             (C) : " + $rootScope.PrintFormater.Align(totalcard, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("  Return Amount           (C) : " + $rootScope.PrintFormater.Align(returnamount, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("  Discount Amount         (D) : " + $rootScope.PrintFormater.Align(addlndiscount, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("  Adjustment Amount       (E) : " + $rootScope.PrintFormater.Align(totaladjustment, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    list.push($rootScope.PrintFormater.Normal("  Total             (A+B-C) : " + $rootScope.PrintFormater.Align((parseFloat(totalcash) + parseFloat(totalcard) - parseFloat(returnamount)).toString(), 10, true)));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    list.push($rootScope.PrintFormater.Normal(""));
                    list.push("\r\n");
                    list.push("\r\n");
                    list.push($rootScope.PrintFormater.Cutter());
                    list.push("\r\n");

                    var printdocument = window.btoa(list.join('\r\n'));
                    $rootScope.Sent2Printer(printdocument);

                }
            }).error(function (result) {
            });
        }
        $scope.printsalessummary = function () {
            request = {
                "item": {
                    'fromdate': $scope.SearchCondition.fromdate, 'todate': $scope.SearchCondition.todate,
                    'companyid': $scope.SearchCondition.companyid, 'locationid': $scope.SearchCondition.locationid
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSaleSummarySlip",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                else {
                    var list = [];

                    if ($scope.SearchCondition.companyid == 0)
                        list.push($rootScope.PrintFormater.Bold($rootScope.UserInfo.CompanyName));
                    else
                        list.push($rootScope.PrintFormater.Bold($scope.SearchRef($scope.SearchCondition.companyid, $scope.Company)));

                    list.push($rootScope.PrintFormater.Bold("SALES SUMMARY REPORT - BILL WISE"));
                    list.push($rootScope.PrintFormater.Normal("_________________________________________________"));
                    list.push($rootScope.PrintFormater.Normal("  LOCATION     : " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location)));
                    list.push($rootScope.PrintFormater.Normal("  DATE BETWEEN : " + $filter('date')($scope.SearchCondition.fromdate, 'dd-MM-yyyy') + " AND " + $filter('date')($scope.SearchCondition.todate, 'dd-MM-yyyy')));
                    list.push($rootScope.PrintFormater.Normal("_________________________________________________"));
                    list.push($rootScope.PrintFormater.Normal("  BILL NO       QTY       DISC     TAX       AMOUNT"));
                    list.push($rootScope.PrintFormater.Normal("_________________________________________________"));

                    var qty = 0;
                    var disc = 0;
                    var tax = 0;
                    var amount = 0;

                    $.each(result, function (k, v) {
                        qty += v.saleqty;
                        disc += v.discount + v.addlndiscount;
                        tax += v.tax;
                        amount += v.amount;

                        var str = $rootScope.PrintFormater.Align(v.billno, 12, true);
                        str += $rootScope.PrintFormater.Align(v.saleqty, 10, true);
                        str += $rootScope.PrintFormater.Align(v.discount + v.addlndiscount, 10, true);
                        str += $rootScope.PrintFormater.Align(v.saletax, 10, true);
                        str += $rootScope.PrintFormater.Align(v.amount, 10, true);
                        list.push($rootScope.PrintFormater.Normal(str));

                    });
                    var str = $rootScope.PrintFormater.Align(qty.round(2), 10, true);
                    str += $rootScope.PrintFormater.Align(disc.round(2), 10, true);
                    str += $rootScope.PrintFormater.Align(tax.round(2), 10, true);
                    str += $rootScope.PrintFormater.Align(amount.round(2), 10, true);

                    list.push($rootScope.PrintFormater.Normal("________________________________________________"));
                    list.push($rootScope.PrintFormater.Normal("  Total          " + str));
                    list.push($rootScope.PrintFormater.Normal("________________________________________________"));
                    list.push($rootScope.PrintFormater.Normal(""));
                    list.push("\r\n");
                    list.push("\r\n");
                    list.push($rootScope.PrintFormater.Cutter());
                    list.push("\r\n");

                    var printdocument = window.btoa(list.join('\r\n'));
                    $rootScope.Sent2Printer(printdocument);

                }
            }).error(function (result) {
            });
        }

        $scope.printsalestaxslip = function () {
            request = {
                "item": {
                    'fromdate': $scope.SearchCondition.fromdate,
                    'todate': $scope.SearchCondition.todate,
                    'companyid': $scope.SearchCondition.companyid,
                    'counterid': $scope.SearchCondition.counterid,
                    'locationid': $scope.SearchCondition.locationid
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSalesTaxSummary",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                else {
                    var list = [];

                    if ($scope.SearchCondition.companyid == 0)
                        list.push($rootScope.PrintFormater.Bold($rootScope.UserInfo.CompanyName));
                    else
                        list.push($rootScope.PrintFormater.Bold($scope.SearchRef($scope.SearchCondition.companyid, $scope.Company)));

                    list.push($rootScope.PrintFormater.Bold("       SALES TAX REPORT"));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________________________"));
                    list.push($rootScope.PrintFormater.Normal("  LOCATION     : " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location)));
                    list.push($rootScope.PrintFormater.Normal("  DATE BETWEEN : " + $filter('date')($scope.SearchCondition.fromdate, 'dd-MM-yyyy') + " AND " + $filter('date')($scope.SearchCondition.todate, 'dd-MM-yyyy')));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________________________"));
                    list.push($rootScope.PrintFormater.Normal("    DATE       TAX%     AMOUNT    IGST     SGST     CGST "));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________________________"));

                    var amount = 0;
                    var sgst = 0;
                    var cgst = 0;
                    var igst = 0;
                    var total = 0;

                    $.each(result, function (k, v) {
                        sgst += v.sgst;
                        cgst += v.cgst;
                        igst += v.igst;
                        amount += v.amount;

                        var str = $rootScope.PrintFormater.Align($filter('date')(v.billdate, 'dd-MM-yyyy'), 12, true);
                        str += $rootScope.PrintFormater.Align(v.taxpercentage, 8, true);
                        str += $rootScope.PrintFormater.Align(v.amount.round(2), 10, true);
                        str += $rootScope.PrintFormater.Align(v.igst.round(2), 8, true);
                        str += $rootScope.PrintFormater.Align(v.sgst.round(2), 10, true);
                        str += $rootScope.PrintFormater.Align(v.cgst.round(2), 10, true);
                        list.push($rootScope.PrintFormater.Normal(str));

                    });
                    var str = $rootScope.PrintFormater.Align(amount.round(2), 10, true);
                    str += $rootScope.PrintFormater.Align(igst.round(2), 10, true);
                    str += $rootScope.PrintFormater.Align(sgst.round(2), 10, true);
                    str += $rootScope.PrintFormater.Align(cgst.round(2), 10, true);

                    list.push($rootScope.PrintFormater.Normal("__________________________________________________________"));
                    list.push($rootScope.PrintFormater.Normal("  Total          " + str));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________________________"));
                    list.push($rootScope.PrintFormater.Normal(""));
                    list.push($rootScope.PrintFormater.Normal(""));
                    list.push("\r\n");
                    list.push($rootScope.PrintFormater.Cutter());
                    list.push("\r\n");

                    var printdocument = window.btoa(list.join('\r\n'));
                    $rootScope.Sent2Printer(printdocument);

                }
            }).error(function (result) {
            });

        }


        $scope.SearchDaySummary = function (grid) {

            if ($scope.SearchCondition.locationid > 0) {

                var request = {
                    "item": $scope.SearchCondition,
                    "securitytoken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "RetailReportService/SearchDaySummary",
                    data: srequest
                }).success(function (result) {
                    $scope.DaySummary = result;

                    if ($scope.igrid != null)
                        $scope.igrid.destroy();

                    var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
                    igrid.options.showHeaderRow = true;

                    igrid.AddSimpleColumn('name', 'Detail', 250);
                    igrid.AddSimpleColumn('value', 'Value', 100).cssClass = 'rightalign';;

                    igrid.Initialize();
                    $scope.igrid = igrid;

                    var data = result.Items;
                    $scope.igrid.UpdateData(data);

                    //$scope.igrid.getData().getItemMetadata = function (row) {
                    //    alert(row);
                    //    if (this.getData()[row].istitle) {
                    //        return {
                    //            "columns": {
                    //                "name": {
                    //                    "colspan": 2
                    //                }
                    //            }
                    //        };
                    //    } else {
                    //        return {
                    //            "columns": {
                    //                0: {
                    //                    "colspan": "*"
                    //                }
                    //            }
                    //        };
                    //    }
                    //};

                }).error(function (result) {
                });
            }
            else {
                $rootScope.Toaster.Info("Please Select Location");
            }
        }


        $scope.salereturnbarcodeexcess = function () {
            var faildList = [];
            $.each($scope.SalesBarcodeResult, function (k, v) {
                if (v.vqty < v.qty)
                    faildList.push(v);
            })
            $scope.SalesBarcodeResult = faildList;
            $scope.igrid.UpdateData($scope.SalesBarcodeResult);
        }

        $scope.Unsettledpackagevalidation = function (barcode) {
            if (barcode == 'undefined' || barcode == null || barcode == '') return;

            if ($scope.SalesResult == null || $scope.SalesResult.length == 0) {
                $rootScope.Toaster.Info("Please load the unsettled bill masters before start the scanning..");
                return;
            }
            var sritem = $scope.SalesResult.find(function (item) {
                return item.billno == barcode;
            });

            if (sritem && sritem != null) {
                sritem.isverified = true;
                $scope.SearchCondition.barcode = '';
            }
            else {
                $rootScope.Toaster.Error(barcode + " doesn't have any bill master code");
                return;
            }

            $scope.igrid.UpdateData($scope.SalesResult);
        }

        $scope.salereturnbarcodevalidation = function (barcode) {

            if (barcode == 'undefined' || barcode == null || barcode == '') return;

            if ($scope.SalesBarcodeResult == null || $scope.SalesBarcodeResult.length == 0) {
                $rootScope.Toaster.Info("Please load the return bills before start the scanning..");
                return;
            }

            var sritem = $scope.SalesBarcodeResult.find(function (item) {
                return item.barcode == barcode && (item.vqty == undefined || item.vqty < (item.qty * -1));
            });
            if (typeof (sritem) == 'undefined' || sritem == null) {
                sritem = $scope.SalesBarcodeResult.find(function (item) {
                    return item.glncode == barcode && (item.vqty == undefined || item.vqty < (item.qty * -1));
                });
            }
            if (sritem && sritem != null) {
                if (sritem.vqty && sritem.qty == sritem.vqty) {
                    $rootScope.Toaster.Error(barcode + " excess barcode found");
                    return;
                }
                if (typeof (sritem.vqty) == 'undefined') sritem.vqty = 0;
                if (typeof (sritem.measure) != 'undefined' && sritem.measure != null && sritem.measure == 'METER')
                    sritem.vqty = (sritem.qty * -1);
                else
                    sritem.vqty++;
                $scope.TotalValidateBarcode++;
                $scope.SearchCondition.barcode = '';
            }
            else {
                $rootScope.Toaster.Error(barcode + " doesn't have any sales return");
                return;
            }
            $scope.igrid.UpdateData($scope.SalesBarcodeResult);
        }
        $scope.TotalSRBarcode = 0;
        $scope.TotalValidateBarcode = 0;
        $scope.salereturnbarcodevalidationgrid = function (value) {
            if (!value) {
                $scope.SalesBarcodeConfig();
            }
            else {
                $scope.TotalSRBarcode = 0;
                $scope.TotalValidateBarcode = 0;
                if ($scope.SalesBarcodeResult != null) {
                    $.each($scope.SalesBarcodeResult, function (k, v) {
                        v.vqty = 0;
                        v.qty = Math.abs(v.qty);
                        $scope.TotalSRBarcode += v.qty;
                    })
                }
                $scope.SalesValidateBarcodeConfig();
            }
        }

        $scope.PrintBarcodes = function () {

            var seletedRows = $scope.igrid.GetSelectedRows();
            var requestitem = {};
            var ids = [];
            var BarcodeList = [];
            var sno = 1;
            $.each(seletedRows, function (k, v) {
                ids.push({
                    'barcode': v.barcode,
                    'stockid': v.stockid,
                    'sno': sno++,
                    'printingname': v.printingname,
                    'sellingmode': v.sellingmode,
                    'noofcopy': v.stockqty
                });
            });


            requestitem = {
                'iswarehouse': seletedRows[0].locationname.endsWith('(WAREHOUSE)'),
                'locationid': $scope.SearchCondition.locationid,
                'barcodes': ids, 'reportid': 0,
                'companytitle': $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company)
            };

            var dialog = ngDialog.open({
                template: 'ChooseBarecodes',
                data: {
                    'requestitem': requestitem,
                    'BarcodeList': ids
                },
                width: '650px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.BarcodeList = $scope.ngDialogData.BarcodeList
                    $scope.requestitem = $scope.ngDialogData.requestitem;

                    $scope.SelectAllChanged = function (items) {
                        $.each(items, function (k, v) {
                            v.selected = $scope.selectall;
                        });
                    };

                    $.each($scope.ngDialogData.BarcodeList, function (k, v) {
                        v.selected = true;
                        if (v.sellingmode == 3)
                            v.noofcopy = 2;
                        else if (v.sellingmode == 2)
                            v.noofcopy = v.noofcopy;
                        else
                            v.noofcopy = 1;
                    });

                    $scope.SetCopy = function () {
                        $.each($scope.ngDialogData.BarcodeList, function (k, v) {
                            if (v.selected == true) {
                                v.noofcopy = $scope.fixcopy;
                            }
                        });
                    };

                    $scope.Selected = function () {
                        var blist = [];
                        $.each($scope.ngDialogData.BarcodeList, function (k, v) {
                            if (v.selected)
                                blist.push(v);
                        });
                        $scope.requestitem.filterbarcode = blist;
                        $scope.requestitem.packagesize = $scope.packagesize;
                        $scope.closeThisDialog($scope.requestitem);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $scope.ChoseReport(data.value);
                    }
                }
            });
        }


        $scope.ChoseReport = function (requestitem) {

            $rootScope.Handlers.SelectPrinter('BARCODE', function (printdata) {
                if (printdata == null || printdata.report == null || printdata.report.id <= 0)
                    return;

                requestitem.reportid = printdata.report.id;

                var request = {
                    "Item": requestitem,
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "PricechangeService/PrintBarcode",
                    data: srequest
                }).success(function (printdocument) {

                    if (printdocument != null && printdocument.length > 0) {
                        $rootScope.Sent2Printer(printdocument.join(';'), printdata);
                    }
                });
            });
        }

        $scope.SaveReturnBarcodes = function () {
            var items = [];
            $.each($scope.SalesBarcodeResult, function (k, v) {
                items.push({ 'barcode': v.barcode, 'qty': v.qty, 'vqty': v.vqty });
            })

            var request = {
                "item": {
                    'locationid': $scope.SearchCondition.locationid, 'isactive': true,
                    'actiondate': $scope.SearchCondition.fromdate, type: 'SaleReturn',
                    'SalesReturnAtribute': { 'date': $scope.SearchCondition.fromdate, 'items': items }
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "StoretransactionService/StoreReturnBarcode",
                data: srequest
            }).success(function (result) {
                if (result != 'undefined' && result != null)
                    $rootScope.Toaster.Info("Saved Successfully.");
            });
        }

        $scope.SalesValidateBarcodeConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SalesBarcodeResult);

            igrid.AddFormatedColumn('qty', 'Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('vqty', 'Scanned', 80, 'N2', 2).total = true;
            igrid.AddSimpleColumn('glncode', 'EAN No', 100);
            igrid.AddSimpleColumn('barcode', 'Barcode', 80);
            igrid.AddSimpleColumn('productname', 'Product', 150);
            igrid.AddSimpleColumn('sectionname', 'Section', 150);
            igrid.AddSimpleColumn('suppliername', 'SupplierName', 150);
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 100);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 120, 'dd/MM/yyyy');
            igrid.AddSimpleColumn('measure', 'Measure', 80);
            igrid.AddSimpleColumn('brandname', 'Brand Name', 80);
            igrid.AddSimpleColumn('stylename', 'Style Name', 80);
            igrid.AddSimpleColumn('patternname', 'Pattern', 80);
            igrid.AddSimpleColumn('designid', 'Design', 80);
            igrid.AddSimpleColumn('sizename', 'Size Name', 80);
            igrid.AddSimpleColumn('colourname', 'Colour Name', 80);
            igrid.AddSimpleColumn('materialname', 'Material Name', 80);
            igrid.AddSimpleColumn('typename', 'Type Name', 80);
            igrid.AddSimpleColumn('billno', 'Bill No', 80);
            igrid.AddFormatedColumn('billdate', 'Bill Date', 100, 'dd/MM/yyyy');
            igrid.AddFormatedColumn('mrp', 'MRP', 80, 'N2', 2).total = true;

            igrid.AddFormatedColumn('mrpvalue', 'MRP Value', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('receivable', 'Sale Value', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 80, 'N2', 2).total = true;
            igrid.EnableTotal();
            igrid.Initialize();

            igrid.dataView.getItemMetadata = function (row) {
                var item = igrid.dataView.getItem(row);
                var qty = item.qty * -1;
                if (item.vqty == qty) {
                    item.style = 'igridbigrowswithdarkgreen';
                }
                else if (item.vqty > qty) {
                    item.style = 'igridbigrowswithred';
                }
                return { 'cssClasses': 'igridbigrows' };
            };

            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SalesBarcodeResult);
        }

        $scope.SearchGiftIssue = function () {

            var request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DeliveryService/Report",
                data: srequest
            }).success(function (result) {

                $timeout(function () {
                    if ($scope.igrid != null)
                        $scope.igrid.destroy();

                    var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
                    igrid.options.showHeaderRow = true;
                    igrid.EnableSerial(result);
                    if ($scope.SearchCondition.mode == '3') {

                        igrid.AddFormatedColumn('receivedon', 'Voucher On', 80, 'dd-MM-yyyy');
                        igrid.AddSimpleColumn('categoryname', 'CATEGORY', 100);
                        igrid.AddFormatedColumn('voucherqty', 'Voucher Qty', 80, 'N2', 2).total = true;
                        igrid.AddFormatedColumn('issueqty', 'Gift Qty', 80, 'N2', 2).total = true;

                    } else if ($scope.SearchCondition.mode == '4') {
                        igrid.AddFormatedColumn('receivedon', 'Voucher On', 80, 'dd-MM-yyyy');
                        igrid.AddSimpleColumn('categoryname', 'CATEGORY', 100);
                        igrid.AddSimpleColumn('barcode', 'BARCODE', 100);
                        igrid.AddSimpleColumn('giftproduct', 'GIFT', 100);
                        igrid.AddFormatedColumn('voucherqty', 'Issued Qty', 80, 'N2', 2).total = true;

                    } else {
                        igrid.AddFormatedColumn('receivedon', 'Voucher On', 80, 'dd-MM-yyyy');
                        igrid.AddSimpleColumn('voucherno', 'VOUCHER NO', 80);
                        igrid.AddSimpleColumn('voucheredby', 'V. EMP', 80);
                        igrid.AddFormatedColumn('settlementon', 'SET. Date', 80, 'dd-MM-yyyy');
                        igrid.AddSimpleColumn('settlementcode', 'SET. CODE', 80);
                        if ($scope.SearchCondition.mode == '0') {
                            igrid.AddSimpleColumn('billno', 'BILL NO', 80);
                        }
                        igrid.AddSimpleColumn('counter', 'COUNTER', 100);
                        igrid.AddSimpleColumn('categoryname', 'CATEGORY', 100);

                        if ($scope.SearchCondition.mode == '2') {
                            igrid.AddFormatedColumn('giftqty', 'Gift Qty', 80, 'N2', 2);
                        }
                        else {
                            igrid.AddSimpleColumn('barcode', 'BARCODE', 100);
                            igrid.AddSimpleColumn('giftproduct', 'GIFT', 100);
                            if ($scope.SearchCondition.mode == '0') {
                                igrid.AddFormatedColumn('billamount', 'Bill Value', 80, 'N2', 2);
                            }
                            igrid.AddFormatedColumn('giftvalue', 'Gift Value', 80, 'N2', 2);
                            igrid.AddFormatedColumn('giftqty', 'Gift Qty', 80, 'N2', 2);
                            if ($scope.SearchCondition.mode != '1')
                                var column = igrid.AddCustomColumn('Issued', 'Issued', 80, function (item, col) {
                                    return item.issued ? 'Yes' : 'No'
                                });
                            igrid.AddSimpleColumn('issuedby', 'I. EMP', 80);
                            var column = igrid.AddCustomColumn('DeliveredOn', 'Delivered On', 80, function (item, col) {
                                return item.issued ? $filter('date')(item.deliveredon, 'dd-MMM-yyyy') : '';
                            });
                        }
                    }
                    igrid.EnableTotal();
                    igrid.Initialize();
                    $scope.igrid = igrid;
                    $scope.igrid.UpdateData(result);
                }, 50);
            });
        }

        $scope.Months = [{ 'id': '01', 'name': 'Jan' },
        { 'id': '02', 'name': 'Feb' }, { 'id': '03', 'name': 'Mar' }, { 'id': '04', 'name': 'Apr' },
        { 'id': '05', 'name': 'May' }, { 'id': '06', 'name': 'June' }, { 'id': '07', 'name': 'July' },
        { 'id': '08', 'name': 'Augest' }, { 'id': '09', 'name': 'September' }, { 'id': '10', 'name': 'October' },
        { 'id': '11', 'name': 'November' }, { 'id': '12', 'name': 'December' }];

        $scope.SaveGST1AXLSX = function () {
            $rootScope.Download('Gst_InboxService/DownloadGSTR1/' + $scope.UserInfo.OrganizationID + '/' + $scope.GSTInbox.companyid
                + '/' + $scope.GSTInbox.periodyear + '/' + $scope.GSTInbox.periodmonth,
                'gstr1' + $scope.GSTInbox.periodyear + '_' + $scope.GSTInbox.periodmonth + '.xlsx', 'application/pdf');
        }

        $scope.SalesHSNConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SalesHSNResult);
            if ($scope.SearchCondition.gstbill) {
                igrid.AddSimpleColumn('customer', 'CUSTOMER NAME', 180);
                igrid.AddSimpleColumn('gstno', 'GST NO', 100);
                igrid.AddSimpleColumn('billno', 'BILL NO', 100);
            }
            igrid.AddSimpleColumn('hsncode', 'HSN CODE', 100);
            if (!$scope.SearchCondition.showdetail)
                igrid.AddSimpleColumn('productname', 'PRODUCT NAME', 180);
            igrid.AddSimpleColumn('measure', 'MEASURE', 80);

            $.each($scope.TaxHeaderHSN, function (k, v) {

                var column = igrid.AddCustomColumn('tax_col_' + k, v.displaytext, 80, function (item, col) {

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

                });

                column.internal = v;
                column.cssClass = 'rightalign';
                column.total = true;
                column.decimal = 2;
                column.format = 'N2';
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
            $scope.igrid.UpdateData($scope.SalesHSNResult);
        }

        $scope.rounding = 2;
        $scope.Appsetting_Company = false;
        $scope.system_cashclosewithtaken = false;
        $scope.LoadApplicationSetting = function () {
            if (typeof ($rootScope.UserInfo) != 'undefined' &&
                typeof ($rootScope.UserInfo.appsettings) != 'undefined' &&
                $rootScope.UserInfo.appsettings != null) {
                $scope.Appsetting_Company = true;
                $scope.system_cashclosewithtaken = $rootScope.UserInfo.appsettings.system_cashclosewithtaken || false;
                $scope.SupportDealerPrice = $rootScope.UserInfo.appsettings.system_dealerprice || false;
                if (false || $rootScope.UserInfo.appsettings.system_b2brounding)
                    $scope.rounding = 4;
            }
            else {
                $timeout(function () {
                    $scope.LoadApplicationSetting();
                }, 50);
            }
            if ($scope.reporttype == "TEXTDAYSUMMARY") {
                $scope.SearchCondition.showsalesummary = true;
                $scope.SearchCondition.showdetail = true;
                $scope.SearchCondition.summary = true;
                $scope.SearchCondition.includeb2b = true;
                $scope.SearchCondition.taxwise = true;
                $scope.SearchCondition.gstbill = true;
            }
        }
        $scope.LoadApplicationSetting();
        //$timeout(function () {
        //    $scope.setFocusfirst();
        //}, 10);

    }]);