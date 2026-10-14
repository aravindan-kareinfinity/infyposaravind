
var app = angular.module('InfinityLab');

app.controller('ReportBuilderController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$filter', 'Excel', '$timeout', '$stateParams', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, Excel, $timeout, $stateParams, ngDialog) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Permission = UserDetails.Info;
        $scope.CreteriaList = [];
        $scope.ReportTypes = [{ 'code': 'SALES', 'name': 'SALES' },
        { 'code': 'STOCK', 'name': 'STOCK' },
        { 'code': 'PURCHASE', 'name': 'PURCHASE' },
        { 'code': 'DEBIT NOTE', 'name': 'DEBIT NOTE' }];

        $scope.DateRanges = [{ 'code': 'TODAY', 'name': 'TODAY' },
        { 'code': 'YESTERDAY', 'name': 'YESTERDAY' },
        { 'code': 'LAST2DAYS', 'name': 'LAST 2 DAYS' },
        { 'code': 'LAST7DAYS', 'name': 'LAST 7 DAYS' },
        { 'code': 'WEEK', 'name': 'THIS WEEK' },
        { 'code': 'LASTWEEK', 'name': 'LAST WEEK' },
        { 'code': 'LAST2WEEK', 'name': 'LAST 2 WEEK' },
        { 'code': 'MONTH', 'name': 'THIS MONTH' },
        { 'code': 'LASTMONTH', 'name': 'LAST MONTH' },
        { 'code': 'LAST2MONTH', 'name': 'LAST 2 MONTH' },
        { 'code': 'LASTYEARTODAY', 'name': 'LASTYEAR TODAY' },
        { 'code': 'LASTYEARYESTERDAY', 'name': 'LASTYEAR YESTERDAY' },
        { 'code': 'LASTYEARTOMORROW', 'name': 'LASTYEAR TOMORROW' },
        { 'code': 'LASTYEARTHISMONTH', 'name': 'LASTYEAR THIS MONTH' },
        { 'code': 'LASTYEARPREVIOUSMONTH', 'name': 'LASTYEAR PREVIOUS MONTH' },
        { 'code': 'LASTYEARNEXTMONTH', 'name': 'LASTYEAR NEXT MONTH' },
        { 'code': 'YEAR', 'name': 'THIS YEAR' },
        { 'code': 'LASTYEAR', 'name': 'LAST YEAR' },
        { 'code': 'LAST2YEAR', 'name': 'LAST 2 YEAR' },
        { 'code': 'FINYEAR', 'name': 'THIS FIN-YEAR' },
        { 'code': 'LASTFINYEAR', 'name': 'LAST FIN-YEAR' },
        { 'code': 'LAST2FINYEAR', 'name': 'LAST 2 FIN-YEAR' }];

        $scope.ReportList = restService.Get('ReferencelistService/DIRECT-REPORTBUILDER', true).then(function (results) {
            results.push({ 'id': -1, 'name': 'New Report' });
            $scope.ReportList = results;
        });

        $scope.exporttoExcel = function (infygrid, filename) {
            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, filename, 'REPORT FOR');
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

        $scope.ShowSearchResult = false;
        var CondtionType =
        {
            Range: 0,
            String: 1,
            Long: 2,
            Date: 3,
            Decmal: 4,
            DayRange: 5
        }

        var Roles =
        {
            All: 0,
            Sales: 1,
            BothStock: 2,
            WHStock: 3,
            RStock: 4
        }

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
            SalesmanName: 31,
            Supplier_City: 32,
            Barcode: 33,
            HSNCode: 34,
            Tax: 35,
            SaleWeekDay: 36,
            SaleMonthDay: 37,
            SaleMargin_Range: 38,
            RetailMargin_Range: 39,
            Agent: 40,
            PurchaseQty: 41,
            SaleQty: 42,
            PurchaseValue: 43,
            SaleValue: 44,
            AddDiscountRate: 45,
            ZeroQty: 46,
            NegativeQty: 47,
            Counter: 48,
            Company: 49,
            ProductGroup: 50,
            ReturnReason: 51,
            SaleReason: 52,
            Area: 53,
            Season: 54,
            Customer_Name: 55,
            Customer_AgeSet: 56,
            Customer_Gender: 57,
            Customer_DOB: 58,
            Customer_Anniversary: 59,
            Customer_LoyeltyNo: 60,
            Customer_Mobile: 61,
            Product_Owner: 62,
            StockQty: 63,
            StockValue: 64,
            AvgStockDays: 65,
            AvgSaleDays: 66,
            InwardQty: 67,
            OutwardQty: 68,
            ReturnQty: 69,
            ReturnValue: 70,
            Margin: 71,
            EntryLocation: 72,
            LRNo: 73,
            LRDate: 74,
            LREntryDate: 75,
            DebitNoteNo: 76,
            DebitNoteDate: 77,
            SalesmanCode: 78,
            SchemeDiscount: 79,
            SaleTax: 80,
            PurchaseDiscount: 81,
            PurchaseTax: 82,
            Promotion: 83,
            PurchaseDate: 84,
            DebitNoteQty: 85,
            DebitNoteValue: 86,
            JournalQty: 87,
            JournalValue: 88,
            B2BSales: 89,
            CREDITSALES: 90,
            SALESRETURN: 91,
            APPROVEDSALES: 92,
            Customer_Religion: 93,
            Source_Supplier: 94,
            Customer_City: 95,
            IStockValue: 96,
            StockSaleValue: 97,
            Fit: 99,
            Sleeve: 100,
            GLNNO: 103,
            WithZeroandNagativeStock: 109,
            JobworkSupplier: 110,
            SellingMode: 111,
            BillType: 113,
            PriceTag: 114,
            CustomerCategory: 115,
            Customer_Agent: 116,
            BarcodeGroup: 117,
            TodayYesterDay: 120,
            WeekLastWeek: 121,
            MonthLastMonth: 122,
            YearLastYear: 123,
            FinYearLastFinYear: 124,
            Custom_ProductGroups: 200,
            Custom_Fields: 201,
            ValueType:250
        }

        $scope.Options = [
            { 'id': Options.Product, 'name': 'PRODUCT', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Brand, 'name': 'BRAND', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Size, 'name': 'SIZE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Pattern, 'name': 'PATTERN', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Material, 'name': 'MATERIAL', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Style, 'name': 'STYLE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Colour, 'name': 'COLOUR', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Fit, 'name': 'FIT', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Sleeve, 'name': 'SLEEVE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Type, 'name': 'TYPE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Item, 'name': 'ITEM', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Section, 'name': 'SECTION', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.SaleLocation, 'name': 'SALE LOCATION', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sales },
            { 'id': Options.StockLocation, 'name': 'STOCK LOCATION', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Design, 'name': 'DESIGN', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.SaleRate, 'name': 'SALE RATE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All },
            { 'id': Options.PurchaseRate, 'name': 'PURCHASE RATE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All },
            { 'id': Options.DiscountRate, 'name': 'DISCOUNT RATE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All },
            { 'id': Options.SoldRate, 'name': 'SOLD RATE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.Sales },
            { 'id': Options.LREntryNo, 'name': 'LRENTRY NO', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.InvoiceNo, 'name': 'INVOICE NO', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.InvDate, 'name': 'INVOICE DATE', 'style': $scope.style_unselected, 'type': CondtionType.Date, 'role': Roles.All },
            { 'id': Options.PurchaseDate, 'name': 'PURCHASE DATE', 'style': $scope.style_unselected, 'type': CondtionType.Date, 'role': Roles.All },
            { 'id': Options.SaleYear, 'name': 'SALE YEAR', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.Sales },
            { 'id': Options.PurchaseYear, 'name': 'PURCHASE YEAR', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.PurchaseMonth, 'name': 'PURCHASE MONTH', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.SaleMonth, 'name': 'SALE MONTH', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sales },
            { 'id': Options.BillNo, 'name': 'BILL NO', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.Sale },
            { 'id': Options.BillDate, 'name': 'BILL DATE', 'style': $scope.style_unselected, 'type': CondtionType.Date, 'role': Roles.Sale },
            { 'id': Options.Supplier, 'name': 'SUPPLIER', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.SalesmanCode, 'name': 'SALES MAN', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sale },
            { 'id': Options.Supplier_City, 'name': 'SUPPLIER CITY', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Barcode, 'name': 'BARCODE', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.GLNNO, 'name': 'GLN NO', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.HSNCode, 'name': 'HSN CODE', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.Tax, 'name': 'TAX', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Agent, 'name': 'AGENT', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Counter, 'name': 'COUNTER', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Company, 'name': 'COMPANY', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.ProductGroup, 'name': 'PRODUCT GROUP', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.BarcodeGroup, 'name': 'BARCODE GROUP', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.ReturnReason, 'name': 'RETURN REASON', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.SaleReason, 'name': 'SALE REASON', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Area, 'name': 'AREA', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Season, 'name': 'SEASON', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.SaleRate_Range, 'name': 'Sale Rate Range', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.Sales },
            { 'id': Options.StockAge_Range, 'name': 'Stock Age Range', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.Sales },
            { 'id': Options.PurchaseRate_Range, 'name': 'Purchase Rate Range', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.Sales },
            { 'id': Options.DiscountRate_Range, 'name': 'Discount Rate Range', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.Sales },
            { 'id': Options.SaleMargin_Range, 'name': 'Sale Margin Range', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.Sales },
            { 'id': Options.RetailMargin_Range, 'name': 'Retail Margin Range', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.Sales }
        ];

        $scope.Options.sort(function (a, b) {
            var x = a.name.toLowerCase();
            var y = b.name.toLowerCase();
            return x < y ? -1 : x > y ? 1 : 0;
        });

        $scope.refattributes = [
            { 'id': Options.ProductGroup, 'name': 'PRODUCT GROUP', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_stock: true, support_sales: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.BarcodeGroup, 'name': 'BARCODE GROUP', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_stock: true, support_sales: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.Product, 'name': 'PRODUCT', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_stock: true, support_sales: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.Section, 'name': 'SECTION', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_stock: true, support_sales: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.Brand, 'name': 'BRAND', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_stock: true, support_sales: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.Material, 'name': 'MATERIAL', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_stock: true, support_sales: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.Pattern, 'name': 'PATTERN', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_stock: true, support_sales: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.Style, 'name': 'STYLE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_stock: true, support_sales: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.Type, 'name': 'TYPE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_stock: true, support_sales: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.Colour, 'name': 'COLOUR', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_stock: true, support_sales: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.Size, 'name': 'SIZE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_stock: true, support_sales: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.Fit, 'name': 'FIT', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_stock: true, support_sales: true, support_purchase: true, support_debitnote: true  },
            { 'id': Options.Sleeve, 'name': 'SLEEVE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_stock: true, support_sales: true, support_purchase: true, support_debitnote: true  },
            { 'id': Options.Design, 'name': 'DESIGN', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All, support_stock: true, support_sales: true, support_purchase: true, support_debitnote: true }];

        $scope.refconditions = [
            { 'id': Options.ZeroQty, 'name': 'ZERO QTY', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_stock: true },
            { 'id': Options.NegativeQty, 'name': 'NEGATIVE QTY', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_stock: true },
            { 'id': Options.WithZeroandNagativeStock, 'name': 'WITH ZERO AND NEGATIVE QTY', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_stock: true },
            { 'id': Options.B2BSales, 'name': 'B2B SALES', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_sales: true },
            { 'id': Options.CREDITSALES, 'name': 'CREDIT SALES', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_sales: true },
            { 'id': Options.SALESRETURN, 'name': 'SALES RETURN', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_sales: true }
        ];

        $scope.refbusiness = [
            { 'id': Options.Company, 'name': 'COMPANY', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_stock: true, support_sales: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.EntryLocation, 'name': 'ENTRY LOCATION', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sales, support_stock: true, support_sales: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.Supplier, 'name': 'SUPPLIER', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_stock: true, support_sales: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.Supplier_City, 'name': 'SUPPLIER CITY', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_stock: true, support_sales: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.Agent, 'name': 'AGENT', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_purchase: true },
            { 'id': Options.LREntryNo, 'name': 'LR ENTRY NO', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All, support_stock: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.LREntryDate, 'name': 'LR ENTRY DATE', 'style': $scope.style_unselected, 'type': CondtionType.Date, 'role': Roles.Sale, support_stock: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.LRNo, 'name': 'LR NO', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All, support_stock: true, support_sales: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.LRDate, 'name': 'LR DATE', 'style': $scope.style_unselected, 'type': CondtionType.Date, 'role': Roles.Sale, support_stock: true, support_sales: true, support_purchase: true, support_debitnote: true },

            { 'id': Options.InvoiceNo, 'name': 'INVOICE NO', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All, support_stock: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.InvDate, 'name': 'INVOICE DATE', 'style': $scope.style_unselected, 'type': CondtionType.Date, 'role': Roles.All, support_stock: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.PurchaseDate, 'name': 'PURCHASE DATE', 'style': $scope.style_unselected, 'type': CondtionType.Date, 'role': Roles.All, support_stock: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.PurchaseYear, 'name': 'PURCHASE YEAR', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.All, support_stock: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.PurchaseMonth, 'name': 'PURCHASE MONTH', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_stock: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.PurchaseRate, 'name': 'PURCHASE RATE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_stock: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.PurchaseDiscount, 'name': 'PURCHASE DISCOUNT', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_stock: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.PurchaseTax, 'name': 'PURCHASE TAX', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_stock: true, support_purchase: true, support_debitnote: true },
            { 'id': Options.Margin, 'name': 'MARGIN RATE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_stock: true, support_purchase: true, support_sales: true },

            { 'id': Options.StockLocation, 'name': 'STOCK LOCATION', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_stock: true },
            { 'id': Options.DebitNoteNo, 'name': 'DEBIT NOTE NO', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All, support_debitnote: true },
            { 'id': Options.Barcode, 'name': 'BARCODE', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All, support_stock: true, support_sales: true, support_debitnote: true },
            { 'id': Options.GLNNO, 'name': 'GLN NO', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All, support_stock: true, support_sales: true, support_debitnote: true },
            { 'id': Options.SaleLocation, 'name': 'SALE LOCATION', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sales, support_sales: true },

            { 'id': Options.BillNo, 'name': 'BILL NO', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.Sale, support_sales: true },
            { 'id': Options.BillDate, 'name': 'BILL DATE', 'style': $scope.style_unselected, 'type': CondtionType.Date, 'role': Roles.Sale, support_sales: true },
            { 'id': Options.Counter, 'name': 'COUNTER', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sale, support_sales: true },
            { 'id': Options.SaleMonth, 'name': 'SALE MONTH', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sales, support_sales: true },
            { 'id': Options.SaleYear, 'name': 'SALE YEAR', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.Sales, support_sales: true },
            { 'id': Options.SalesmanCode, 'name': 'SALAESMAN CODE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sales, support_sales: true },
            { 'id': Options.SalesmanName, 'name': 'SALES MAN', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sale, support_sales: true },
            { 'id': Options.SoldRate, 'name': 'SALE RATE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.Sales, support_sales: true },
            { 'id': Options.SchemeDiscount, 'name': 'SALES DISCOUNT', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.Sales, support_sales: true },
            { 'id': Options.SaleTax, 'name': 'SALES TAX', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.Sales, support_sales: true },
            { 'id': Options.Promotion, 'name': 'PROMOTION', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sales, support_sales: true },
            { 'id': Options.ReturnReason, 'name': 'SALES RETURN REASON', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_sales: true },
            { 'id': Options.SaleReason, 'name': 'SALE REASON', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_sales: true },
            { 'id': Options.Area, 'name': 'AREA', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_sales: true },
            { 'id': Options.Season, 'name': 'SEASON', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_sales: true },
            { 'id': Options.Customer_Name, 'name': 'CUSTOMER', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_sales: true },
            { 'id': Options.AvgStockDays, 'name': 'STOCK DAYS', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_sales: true },
            { 'id': Options.AvgSaleDays, 'name': 'SALE DAYS', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_sales: true },
            { 'id': Options.TodayYesterDay, 'name': 'TODAY/YESTERDAY', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_sales: true, support_purchase: true },
            { 'id': Options.WeekLastWeek, 'name': 'WEEK/LAST WEEK', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_sales: true, support_purchase: true },
            { 'id': Options.MonthLastMonth, 'name': 'MONTH/LAST MONTH', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_sales: true, support_purchase: true },
            { 'id': Options.YearLastYear, 'name': 'YEAR/LAST YEAR', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_sales: true, support_purchase: true },
            { 'id': Options.FinYearLastFinYear, 'name': 'FIN YEAR/LAST YEAR', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_sales: true, support_purchase: true },
        ];

        $scope.refaggregate = [
            { 'id': Options.PurchaseQty, 'name': 'PURCHASE QTY', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_purchase: true },
            { 'id': Options.PurchaseValue, 'name': 'PURCHASE VALUE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_purchase: true },
            { 'id': Options.StockQty, 'name': 'STOCK QTY', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_stock: true },
            { 'id': Options.StockValue, 'name': 'TAXABLE STOCK VALUE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_stock: true },
            { 'id': Options.IStockValue, 'name': 'STOCK VALUE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_stock: true },
            { 'id': Options.StockSaleValue, 'name': 'STOCK(SALE) VALUE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_stock: true },
            { 'id': Options.SaleQty, 'name': 'SALE QTY', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_sales: true },
            { 'id': Options.SaleValue, 'name': 'SALE VALUE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_sales: true },
            { 'id': Options.ValueType, 'name': 'VALUE TYPE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_sales: true },
            { 'id': Options.InwardQty, 'name': 'INWARD QTY', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_stock: true },
            { 'id': Options.OutwardQty, 'name': 'OUTWARD QTY', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_stock: true },
            { 'id': Options.ReturnQty, 'name': 'RETURN QTY', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_stock: true, support_debitnote: true },
            { 'id': Options.ReturnValue, 'name': 'RETURN VALUE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_stock: true, support_debitnote: true },
            { 'id': Options.SaleTax, 'name': 'SALES TAX', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.Sales, support_sales: true },
            { 'id': Options.DiscountRate, 'name': 'DISCOUNT RATE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_sales: true },
            { 'id': Options.AddDiscountRate, 'name': 'ADD DISCOUNT RATE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_sales: true },
            { 'id': Options.AvgStockDays, 'name': 'STOCK AVG DAYS', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_stock: true },
            { 'id': Options.AvgSaleDays, 'name': 'SALE AVG DAYS', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All, support_sales: true },
            { 'id': Options.DebitNoteQty, 'name': 'DebitNote QTY', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_debitnote: true },
            { 'id': Options.DebitNoteValue, 'name': 'DebitNote VALUE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_debitnote: true },
            { 'id': Options.JournalQty, 'name': 'Journal QTY', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_stock: true },
            { 'id': Options.JournalValue, 'name': 'Journal VALUE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All, support_stock: true },
        ];

        $scope.MonthList = [{ 'id': 1, 'name': 'JANUARY' }, { 'id': 2, 'name': 'FEBRUARY' },
        { 'id': 3, 'name': 'MARCH' }, { 'id': 4, 'name': 'APRIL' },
        { 'id': 5, 'name': 'MAY' }, { 'id': 6, 'name': 'JUNE' },
        { 'id': 7, 'name': 'JULY' }, { 'id': 8, 'name': 'AUGUST' },
        { 'id': 9, 'name': 'SEPTEMBER' }, { 'id': 10, 'name': 'OCTOBER' },
        { 'id': 11, 'name': 'NOVEMBER' }, { 'id': 12, 'name': 'DECEMBER' }];

        $scope.WeekList = [{ 'id': 1, 'name': 'SUNDAY' }, { 'id': 2, 'name': 'MONDAY' },
        { 'id': 3, 'name': 'TUESDAY' }, { 'id': 4, 'name': 'WEDNESDAY' },
        { 'id': 5, 'name': 'THURSDAY' }, { 'id': 6, 'name': 'FRIDAY' },
        { 'id': 7, 'name': 'SATURDAY' }];

        $scope.Product = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Product = results;
        });
        $scope.Location = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Location = results;
        });
        $scope.AddedCreteria = [];
        $scope.CurrentOption = { 'defaultdate': true };
        $scope.selectedboxid = 0;
        $scope.textbox = "";
        $scope.daterange = new Date();


        var creteria = { 'option': $scope.CurrentOption, 'selectedvalue': $scope.selectedboxid, 'entervalue': $scope.textbox, 'fromdate': $scope.daterange.start, 'todate': $scope.daterange.end, 'display': 'PRODUCT - ' + $scope.CurrentOption }


        $scope.Add = function () {

            var addedObject = null;

            if ($scope.CurrentOption.type == 2) {
                addedObject = {
                    'option': $scope.CurrentOption, 'selectedvalue': $scope.creteriaid, 'entervalue': null, 'fromdate': null, 'todate': null,
                    'display': $scope.CurrentOption.name + '-' + $scope.SearchRef($scope.creteriaid, $scope.CreteriaList)
                };
            }

            if ($scope.CurrentOption.type == 1) {
                addedObject = {
                    'option': $scope.CurrentOption, 'selectedvalue': null, 'entervalue': $scope.creteriatext, 'fromdate': null, 'todate': null,
                    'display': $scope.CurrentOption.name + '-' + $scope.creteriatext
                };
            }

            if ($scope.CurrentOption.type == 4) {
                addedObject = {
                    'option': $scope.CurrentOption, 'selectedvalue': null, 'fromvalue': $scope.creterianumberfrom,
                    'tovalue': $scope.creterianumberto, 'fromdate': null, 'todate': null,
                    'display': $scope.CurrentOption.name + '- ' + $scope.creterianumberfrom + ' to ' + $scope.creterianumberto, 'showcolumn': true
                };
            }

            if ($scope.CurrentOption.type == 0 || $scope.CurrentOption.type == 3) {
                var exist = $scope.AddedCreteria.find(function (item) {
                    return item.option.id == $scope.CurrentOption.id;
                });

                if (typeof (exist) != 'undefined' || exist != null) {
                    $rootScope.Toaster.Info($scope.CurrentOption.name + ' Already added');
                    return;
                }
            }

            if ($scope.CurrentOption.type == 0) {


                var range = { 'start': 0, 'incrementor': 0, 'end': 0 };
                var ranges = $scope.creteriatext.split('-');
                if (ranges.length == 3) {
                    range = { 'start': ranges[0], 'incrementor': ranges[1], 'end': ranges[2] };
                }

                addedObject = {
                    'option': $scope.CurrentOption, 'selectedvalue': null, 'range': range,
                    'fromvalue': $scope.creterianumberfrom, 'tovalue': $scope.creterianumberto, 'fromdate': null, 'todate': null,
                    'display': $scope.CurrentOption.name + ' Range - ' + $scope.creteriatext +
                        ' (' + $scope.creterianumberfrom + '-' + $scope.creterianumberto + ')', 'showcolumn': true
                };
            }

            if ($scope.CurrentOption.type == 3) {
                if ($scope.CurrentOption.defaultdate) {
                    addedObject = {
                        'option': $scope.CurrentOption, 'selectedvalue': null, 'entervalue': null,
                        'fromdate': $scope.creteriafromdate,
                        'todate': $scope.creteriatodate,
                        'display': $scope.CurrentOption.name + '-' + $filter('date')($scope.creteriadate, 'dd/MM/yyyy')
                    };
                }
                else {
                    addedObject = {
                        'option': $scope.CurrentOption, 'selectedvalue': null, 'entervalue': null,
                        'daterange': $scope.CurrentOption.defaultdatrange,
                        'display': $scope.CurrentOption.name + '-' + $scope.CurrentOption.defaultdatrange
                    };
                }
            }

            if (addedObject != null)
                $scope.AddedCreteria.push(addedObject);
        };


        $scope.ManageItem = function (action, item) {
            if (action == 'Delete') {
                $scope.AddedCreteria.splice($scope.AddedCreteria.indexOf(item), 1);
            }
        }

        $scope.GenerateReport = function () {

            if ($scope.reportmode == null || $scope.reportmode == '') {
                $rootScope.Toaster.Info("Please Select Report mode");
                return;
            }

            $scope.ColumnList = [];
            $scope.Condition = [];
            $scope.Aggregation = [];
            $.each($scope.refconditions, function (k, v) {
                if (typeof (v.selected) != 'undefined' && v.selected) {
                    $scope.Condition.push(v);
                }
            })

            $.each($scope.refattributes, function (k, v) {
                if (typeof (v.selected) != 'undefined' && v.selected) {
                    $scope.ColumnList.push(v);
                }
            })
            $.each($scope.refbusiness, function (k, v) {
                if (typeof (v.selected) != 'undefined' && v.selected) {
                    $scope.ColumnList.push(v);
                }
            })
            $.each($scope.refaggregate, function (k, v) {
                if (v.id == Options.ValueType) {
                    $scope.SupportValueType = true;
                }
                else if (typeof (v.selected) != 'undefined' && v.selected) {
                    $scope.Aggregation.push(v);
                }
            })

            request = {
                "item": {
                    'creterialist': $scope.AddedCreteria,
                    'columnlist': $scope.ColumnList,
                    'aggregatelist': $scope.Aggregation,
                    'conditionlist': $scope.Condition,
                    'reportmode': $scope.reportmode,
                    'locationid': $scope.locationid,
                    'datemode': $scope.defaultdate,
                    'daterange': $scope.defaultdatrange,
                    'fromdate': $scope.fromdate,
                    'todate': $scope.todate
                },
                "securitytoken": "String content"
            }
            $scope.Request = request.item;
            var srequest = JSON.stringify(request);
            $scope.currentresult = null;

            $http({
                method: 'POST',
                url: "AnalyserService/GenerateReportBuilder",
                data: srequest
            }).success(function (result) {
                $scope.Result = result;
                $scope.Request.aggregatelist = result.aggregatelist;
                $scope.Request.columnlist = result.columnlist;
                $scope.Request.rangelist = result.rangelist;
                $scope.ShowSearchResult = true;
                $scope.ShowPivot = false;
                $timeout(function () {
                    $scope.ShowResult(result);
                }, 100);

            }).error(function (result) {
            });
        }

        $scope.LoadReport = function () {
            var report = $scope.ReportList.find(function (item) {
                return item.id == $scope.reportid;
            });
            if (typeof (report) == 'undefined')
                return null;

            var report = JSON.parse(report.data);
            $scope.reportmode = report.reportmode;
            $scope.reportmodechanged();

            $scope.AddedCreteria = report.creterialist;
            $scope.ColumnList = report.columnlist;
            $scope.Aggregation = report.aggregatelist;
            $scope.Condition = report.conditionlist;
            $scope.locationid = report.locationid;
            $scope.fromdate = report.fromdate;
            $scope.todate = report.todate;

            $.each($scope.refconditions, function (k, v) {

                var avl = $scope.Condition.find(function (item) {
                    return item.id == v.id; ``
                });

                if (typeof (avl) != 'undefined' && avl != null)
                    v.selected = true;
                else
                    v.selected = false;
            })

            $.each($scope.refattributes, function (k, v) {
                var avl = $scope.ColumnList.find(function (item) {
                    return item.id == v.id;
                });

                if (typeof (avl) != 'undefined' && avl != null)
                    v.selected = true;
                else
                    v.selected = false;
            });

            $.each($scope.refbusiness, function (k, v) {
                var avl = $scope.ColumnList.find(function (item) {
                    return item.id == v.id;
                });

                if (typeof (avl) != 'undefined' && avl != null)
                    v.selected = true;
                else
                    v.selected = false;
            });

            $.each($scope.refaggregate, function (k, v) {
                var avl = $scope.Aggregation.find(function (item) {
                    return item.id == v.id;
                });

                if (typeof (avl) != 'undefined' && avl != null)
                    v.selected = true;
                else
                    v.selected = false;
            });

        }

        $scope.DeleteReport = function () {
            var exist = $scope.ReportList.find(function (item) {
                return item.id == $scope.reportid;
            })
            if (typeof (exist) != 'undefined' || exist != null) {
                $scope.ReportList.splice($scope.ReportList.indexOf(exist), 1);

                var request = {
                    "Item": exist,
                    "securitytoken": "String content"
                }

                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "ReferencelistService/Delete",
                    data: srequest
                }).success(function (result) {

                    if (result != null && result) {
                        $rootScope.Toaster.Info("Deleted Successfully.!");
                        $scope.reportid = result.id;
                    }

                }).error(function (result) {
                });
            }
        }

        $scope.SupportValueType = false;
        $scope.SaveReport = function (id, name) {
            $scope.SupportValueType = false;
            var data = {
                'id': id, 'name': name, 'type': 'REPORTBUILDER', 'code': 'REPORTBUILDER', data: {}
            };

            $scope.ColumnList = [];
            $scope.Condition = [];
            $scope.Aggregation = [];
            $.each($scope.refconditions, function (k, v) {
                if (typeof (v.selected) != 'undefined' && v.selected) {
                    $scope.Condition.push(v);
                }
            })

            $.each($scope.refattributes, function (k, v) {
                if (typeof (v.selected) != 'undefined' && v.selected) {
                    $scope.ColumnList.push(v);
                }
            })
            $.each($scope.refbusiness, function (k, v) {
                if (typeof (v.selected) != 'undefined' && v.selected) {
                    $scope.ColumnList.push(v);
                }
            })

            $.each($scope.refaggregate, function (k, v) {
                //if (v.id != Options.ValueType)
                //{
                //    $scope.SupportValueType = true;
                //}
                //else
                if (typeof (v.selected) != 'undefined' && v.selected) {
                    $scope.Aggregation.push(v);
                }
            })

            data.data = JSON.stringify({
                'creterialist': $scope.AddedCreteria,
                'columnlist': $scope.ColumnList,
                'aggregatelist': $scope.Aggregation,
                'conditionlist': $scope.Condition,
                'reportmode': $scope.reportmode,
                'locationid': $scope.locationid,
                'fromdate': $scope.fromdate,
                'todate': $scope.todate
            });

            var request = {
                "Item": data,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ReferencelistService/SaveDirect",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Saved Successfully.!");
                    var exist = $scope.ReportList.find(function (item) {
                        return item.id == result.id;
                    })
                    if (typeof (exist) == 'undefined' || exist == null)
                        $scope.ReportList.push(result)
                    $scope.reportid = result.id;
                }

            }).error(function (result) {
            });
        }


        $scope.ShowSaveDialog = function (reportid) {
            var reportname = null;
            if (typeof (reportid) == 'undefined' || reportid <= 0) {
                $rootScope.Prompt('Report Builder', 'Name', function (code) {
                    if (code && code != null) {
                        reportname = code.toString();
                        $scope.SaveReport(reportid, code);
                    }
                });
            }
        }

        $scope.SelectAll = function (arraylist, value) {

            $.each(arraylist, function (k, v) {
                v.selected = value;
            })

        }

        $scope.igrid = null;

        $scope.ModeofDelivery = [{ 'id': 1, 'name': 'Lorry' }, { 'id': 2, 'name': 'Hand Delivery' }];


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
            lrentryno: "",
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
            text: ""
        };

        $scope.ShowFilter = true;

        $scope.SearchResult = [];
        $scope.RulesList = [];
        $scope.StockAnalysisResult = [];
        $scope.CurrentOption = { 'defaultdate': true };
        $scope.ChangeCreteriaList = function (optionid) {
            $scope.CreteriaList = [];

            $scope.CurrentOption = $scope.Options.find(function (item) {
                return item.id == optionid;
            })

            if (typeof ($scope.CurrentOption) == 'undefined' || $scope.CurrentOption == null)
                return;

            $scope.CurrentOption.defaultdate = true;
            if (optionid == Options.Product) {
                restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
                    $scope.CreteriaList = results;
                });
            }
            if (optionid == Options.Brand) {
                restService.Get('ReferencelistService/BRAND', true).then(function (results) {
                    $scope.CreteriaList = results;
                });
            }
            if (optionid == Options.Size) {
                restService.Get('ReferencelistService/SIZE', true).then(function (results) {
                    $scope.CreteriaList = results;
                });
            }
            if (optionid == Options.Style) {
                restService.Get('ReferencelistService/STYLE', true).then(function (results) {
                    $scope.CreteriaList = results;
                });
            }
            if (optionid == Options.Pattern) {
                restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
                    $scope.CreteriaList = results;
                });
            }
            if (optionid == Options.Material) {
                restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
                    $scope.CreteriaList = results;
                });
            }
            if (optionid == Options.Colour) {
                restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
                    $scope.CreteriaList = results;
                });
            }
            if (optionid == Options.Type) {
                restService.Get('ReferencelistService/TYPE', true).then(function (results) {
                    $scope.CreteriaList = results;
                });
            }
            if (optionid == Options.Agent) {
                restService.Get('ReferencelistService/AGENT', true).then(function (results) {
                    $scope.CreteriaList = results;
                });
            }
            if (optionid == Options.Section) {
                restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
                    $scope.CreteriaList = results;
                });
            }
            if (optionid == Options.SaleLocation) {
                restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
                    $scope.CreteriaList = results;
                });
            }
            if (optionid == Options.StockLocation) {
                restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
                    $scope.CreteriaList = results;
                });
            }
            if (optionid == Options.SaleMonth) {
                $scope.CreteriaList = $scope.MonthList;

            }
            if (optionid == Options.PurchaseMonth) {
                $scope.CreteriaList = $scope.MonthList;

            }
            if (optionid == Options.Supplier) {
                restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
                    $scope.CreteriaList = results;
                });
            }
            if (optionid == Options.Supplier_City) {
                restService.Get('ReferencelistService/CITY', true).then(function (results) {
                    $scope.CreteriaList = results;
                });
            }
            if (optionid == Options.Tax) {
                restService.Get('ReferencelistService/SALESTAX', true).then(function (results) {
                    $scope.CreteriaList = results;
                });
            }
            if (optionid == Options.Fit) {
                restService.Get('ReferencelistService/FIT', true).then(function (results) {
                    $scope.CreteriaList = results;
                });
            }
            if (optionid == Options.Sleeve) {
                restService.Get('ReferencelistService/SLEEVE', true).then(function (results) {
                    $scope.CreteriaList = results;
                });
            }

            if (optionid == Options.SaleWeekDay) {
                $scope.CreteriaList = $scope.WeekList;
            }
        }


        $scope.DynamicPivot = function () {

            var columns = [];
            var rows = [];
            var cols = [];
            if ($scope.currentresult.columnlist &&
                $scope.currentresult.columnlist != null &&
                $scope.currentresult.columnlist.length > 0) {
                rows.push($scope.currentresult.columnlist[0].name);
            }
            //if ($scope.currentresult.aggregatelist &&
            //    $scope.currentresult.aggregatelist != null &&
            //    $scope.currentresult.aggregatelist.length > 0) {
            //    rows.push($scope.currentresult.aggregatelist[0].name);
            //}


            $.each($scope.currentresult.columnlist, function (k, v) {
                columns.push({ 'type': 'columns', 'id': 'col_' + v.index, 'value': v, 'name': v.name });
            });

            $.each($scope.currentresult.rangelist, function (k, v) {
                columns.push({ 'type': 'range', 'id': 'rng_' + v.index, 'value': v, 'name': v.name });
            });

            var list = [];
            if ($scope.SupportValueType) {
                $.each($scope.currentresult.result, function (k, v) {
                    var row = {};
                    $.each($scope.currentresult.columnlist, function (kk, vv) {
                        row[vv.name] = v.String[vv.index];
                    });
                    $.each($scope.currentresult.rangelist, function (kk, vv) {
                        row[vv.name] = v.RangeResult[vv.index];
                    });
                    $.each($scope.currentresult.aggregatelist, function (kk, vv) {
                        var newrow = JSON.parse(JSON.stringify(row));
                        newrow['valuetype'] = vv.name;
                        newrow['value'] = v.Amount[vv.index];
                        list.push(newrow);
                    });
                });
            }
            else {
                $.each($scope.currentresult.result, function (k, v) {
                    var row = {};
                    $.each($scope.currentresult.columnlist, function (kk, vv) {
                        row[vv.name] = v.String[vv.index];
                    });
                    $.each($scope.currentresult.rangelist, function (kk, vv) {
                        row[vv.name] = v.RangeResult[vv.index];
                    });
                    $.each($scope.currentresult.aggregatelist, function (kk, vv) {
                        row[vv.name] = v.Amount[vv.index];
                    });
                    list.push(row);
                });
            }

            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }

            var derivers = $.pivotUtilities.derivers;
            var renderers = $.extend(
                $.pivotUtilities.renderers,
                $.pivotUtilities.plotly_renderers,
                $.pivotUtilities.d3_renderers
            );

            var availableheight = (typeof ($rootScope.windowSize) != 'undefined' &&
                typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            var availablewidth = (typeof ($rootScope.windowSize) != 'undefined' &&
                typeof ($rootScope.windowSize.width) != 'undefined') ? $rootScope.windowSize.width : 0;

            if (availableheight > 0) {
                $scope.panelheight.height = '' + (availableheight - 50) + 'px';
            }

            if (availablewidth > 0) {
                $scope.panelheight.width = '' + (availablewidth - 50) + 'px';
            }

            $('#pivotContainer').empty();
            $scope.ShowPivot = true;
            $scope.PivotList = list;
            $timeout(function () {
                $("#pivotContainer").pivotUI(list, {
                    renderers: renderers,
                    rows: rows, cols: cols,
                    grouping: true,
                    rendererName: "Table Barchart",
                    rowOrder: "value_a_to_z", colOrder: "value_z_to_a",
                });
            }, 10);
        }
        $scope.ShowPivot = false;
        $scope.panelheight = { "height": '0px', "width": '0px' };

        $scope.Pivot = function () {

            var columns = [];
            $.each($scope.currentresult.columnlist, function (k, v) {
                columns.push({ 'type': 'columns', 'id': 'col_' + v.index, 'value': v, 'name': v.name });
            });

            $.each($scope.currentresult.rangelist, function (k, v) {
                columns.push({ 'type': 'range', 'id': 'rng_' + v.index, 'value': v, 'name': v.name });
            });

            var dialog = ngDialog.open({
                template: 'SaveReport',
                data: {
                    'columns': columns
                },
                width: '650px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {

                    $scope.GroupColumns = $scope.ngDialogData.columns;
                    $scope.Save = function () {
                        var column = $scope.GroupColumns.find(function (item) {
                            return item.id == $scope.selectedcolumn;
                        });
                        $scope.closeThisDialog(column);
                    }
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                        var groupingcolumn = data.value;

                        var groupcolumnvalues = [];
                        var helper = {};
                        var groupresult = $scope.currentresult.result.reduce(function (r, o) {

                            var keys = [];
                            var columnvalue = '';

                            $.each($scope.currentresult.columnlist, function (k, v) {
                                if (!(groupingcolumn.type == 'columns' && v.name == groupingcolumn.name)) {
                                    keys.push(o.String[v.index]);
                                }
                                else {
                                    columnvalue = o.String[v.index];
                                }
                            });

                            $.each($scope.currentresult.rangelist, function (k, v) {
                                if (!(groupingcolumn.type == 'range' && k == groupingcolumn.name)) {
                                    keys.push(o.RangeResult[v.index]);
                                } else {
                                    columnvalue = o.RangeResult[v.index];
                                }
                            });

                            if (groupcolumnvalues.indexOf(columnvalue) < 0)
                                groupcolumnvalues.push(columnvalue);

                            var key = keys.join('-');
                            if (!helper[key]) {
                                var newobj = { 'key': key, 'value': Object.assign({ 'group': [] }, o), };
                                newobj.value.group.push({ 'code': columnvalue, 'Amount': Object.assign({}, o.Amount) });
                                helper[key] = newobj;
                                r.push(helper[key]);
                            } else {
                                helper[key].value.group.push({ 'code': columnvalue, 'Amount': Object.assign({}, o.Amount) });
                            }

                            return r;
                        }, []);
                        $scope.ShowPivot = false;
                        $timeout(function () {
                            $scope.ShowGroupResult(groupingcolumn, groupcolumnvalues, groupresult);
                        }, 10);
                    }
                }
            });
        }

        $scope.igrid = null;
        $scope.currentresult = null;

        $scope.ShowGroupResult = function (groupingcolumn, groupcolumnvalues, groupresult) {

            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }
            var igrid = new InfyGrid('gridContainer', '575px', '100%', true, false, true, 0);

            var groupColumns = [];
            var dpg = $.fn.datepicker.DPGlobal;
            var dpgformat = dpg.parseFormat(datePicker.controlFormat);
            var freecolumns = ($scope.currentresult.columnlist.length + $scope.currentresult.rangelist);
            $.each($scope.currentresult.columnlist, function (k, v) {

                if (!(groupingcolumn.type == 'columns' && v.name == groupingcolumn.name)) {

                    var column = igrid.AddCustomColumn('comn_' + v.index, v.name, 150, function (item, col) {
                        var cellresult = item.value.String[col.internal.index];
                        if (cellresult != null) return cellresult;
                        return '';
                    });

                    if (v.type == 4)
                        column.cssClass = 'rightalign';

                    column.dpgformat = dpgformat;
                    column.dpg = dpg;
                    column.internal = v;
                    column.sortable = true;
                    column.getValue = function (item, col) {
                        var cellresult = item.value.String[col.internal.index];
                        if (cellresult != null) return cellresult;
                        return '';
                    }
                }

            });

            $.each($scope.currentresult.rangelist, function (k, v) {

                if (!(groupingcolumn.type == 'range' && k == groupingcolumn.name)) {

                    var column = igrid.AddCustomColumn('cond_' + v.index, v.name, 100, function (item, col) {
                        var cellresult = item.value.RangeResult[col.internal.index];
                        if (cellresult != null) return cellresult;
                        return '';
                    });
                    column.internal = v;
                    column.sortable = true;
                    column.getValue = function (item, col) {
                        var cellresult = item.value.RangeResult[col.internal.index];
                        if (cellresult != null) return cellresult;
                        return '';
                    }

                }

            });
            groupColumns.push(
                {
                    id: 'Filter',
                    name: 'Filter',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            igrid.columns = [];

            groupcolumnvalues.sort();

            $.each(groupcolumnvalues, function (gk, gv) {

                igrid.columns = [];

                $.each($scope.currentresult.aggregatelist, function (k, v) {

                    var column = igrid.AddCustomColumn(gk.toString() + '_amt_' + v.index, v.name, 80, function (item, col) {

                        var aggValue = item.value.group.find(function (grpfind) {
                            return grpfind.code == col.internal.groupkey;
                        });

                        if (typeof (aggValue) != 'undefined' && aggValue != null) {
                            var cellresult = aggValue.Amount[col.internal.index];
                            if (cellresult != null) return $filter('IndianNumber')(cellresult, 2);
                        }
                        return '';
                    });
                    column.sortable = true;
                    column.internal = Object.assign({ 'groupkey': gv }, v);
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.getValue = function (item, col) {
                        var aggValue = item.value.group.find(function (grpfind) {
                            return grpfind.code == col.internal.groupkey;
                        });

                        if (typeof (aggValue) != 'undefined' && aggValue != null) {
                            var cellresult = aggValue.Amount[col.internal.index];
                            if (cellresult != null) return $filter('IndianNumber')(cellresult, 2);
                        }
                        return 0;
                    }

                });

                if (igrid.columns.length > 0) {
                    groupColumns.push(
                        {
                            id: gv,
                            name: gv,
                            headerCssClass: 'text-center',
                            columns: igrid.columns
                        });
                }
            });
            igrid.columns = groupColumns;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            if (freecolumns - 2 >= 0) {
                igrid.grid.setOptions({ 'frozenColumn': freecolumns - 2 });
            }
            $scope.igrid.UpdateData(groupresult);
        }

        $scope.ShowResult = function (result) {

            $scope.currentresult = result;
            var dpg = $.fn.datepicker.DPGlobal;
            var dpgformat = dpg.parseFormat(datePicker.controlFormat);
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }

            var igrid = new InfyGrid('gridContainer', '450px', '100%', false, false, true, 0);

            $.each(result.columnlist, function (k, v) {
                var column = igrid.AddCustomColumn('comn_' + v.index, v.name, 100, function (item, col) {
                    var cellresult = item.String[col.internal.index];
                    if (cellresult != null) return cellresult;
                    return '';
                });

                if (v.type == 4)
                    column.cssClass = 'rightalign';
                column.dpgformat = dpgformat;
                column.dpg = dpg;
                column.internal = v;
                column.sortable = true;
                column.getValue = function (item, col) {
                    var cellresult = item.String[col.internal.index];
                    if (cellresult != null) return cellresult;
                    return '';
                }
            });

            $.each(result.rangelist, function (k, v) {
                var column = igrid.AddCustomColumn('cond_' + v.index, v.name, 100, function (item, col) {
                    var cellresult = item.RangeResult[col.internal.index];
                    if (cellresult != null) return cellresult;
                    return '';
                });
                column.internal = v;
                column.sortable = true;
                column.getValue = function (item, col) {
                    var cellresult = item.RangeResult[col.internal.index];
                    if (cellresult != null) return cellresult;
                    return '';
                }
            });

            $.each(result.aggregatelist, function (k, v) {

                var column = igrid.AddCustomColumn('amt_' + v.index, v.name, 100, function (item, col) {
                    var cellresult = item.Amount[col.internal.index];
                    if (cellresult != null) return $filter('IndianNumber')(cellresult, 2);
                    return '';
                });
                column.internal = v;
                column.cssClass = 'rightalign';
                column.total = true;
                column.sortable = true;
                column.getValue = function (item, col) {
                    var cellresult = item.Amount[col.internal.index];
                    if (cellresult != null) return cellresult;
                    return 0;
                }
            });

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.Result.result);
        }

        $scope.LoadAppSetting = function () {
            if (typeof ($rootScope.UserInfo) != 'undefined' &&
                typeof ($rootScope.UserInfo.appsettings) != 'undefined' &&
                $rootScope.UserInfo.appsettings != null) {
                if ($rootScope.UserInfo.appsettings.prd_hierarchy &&
                    $rootScope.UserInfo.appsettings.prd_hierarchy != null &&
                    $rootScope.UserInfo.appsettings.prd_hierarchy != '') {
                    var list = $rootScope.UserInfo.appsettings.prd_hierarchy.split(',');
                    for (var i = 0; i < list.length; i++) {
                        $scope.refattributes.push({
                            'id': Options.Custom_ProductGroups,
                            'name': 'PRD GRP - ' + list[i],
                            'customoption': i + 1,
                            'style': $scope.style_unselected,
                            'type': CondtionType.Long,
                            'role': Roles.All,
                            'support_stock': true,
                            'support_sales': true,
                            'support_purchase': true,
                            'support_debitnote': true
                        })
                    }
                }

                if ($rootScope.UserInfo.appsettings.system_customfields) {
                    restService.Get('ReferencelistService/FULL-CUSTOMFIELDS', true).then(function (results) {
                        $.each(results, function (k, v) {
                            if (v.additionalinfo &&
                                v.additionalinfo.customfield &&
                                v.additionalinfo.customfield.cfbuilt) {
                                var coption = v.additionalinfo.customfield.cfname + ',' +
                                    v.referenceid + ',' +
                                    v.additionalinfo.customfield.cftype + ',' +
                                    v.additionalinfo.customfield.cfref;
                                var type = CondtionType.Long;
                                if (v.referenceid == 1)
                                    type = CondtionType.String;
                                else if (v.referenceid == 2)
                                    type = CondtionType.Decmal;
                                else if (v.referenceid == 3)
                                    type = CondtionType.Date;

                                $scope.refbusiness.push({
                                    'id': Options.Custom_Fields,
                                    'name': 'CFIELDS - ' + v.name,
                                    'customoption': coption,
                                    'style': $scope.style_unselected,
                                    'type': type,
                                    'role': Roles.All,
                                    'support_stock': true,
                                    'support_sales': true,
                                    'support_purchase': true,
                                    'support_debitnote': true
                                })
                            }
                        })
                        $scope.reportmodechanged();
                    });
                }
                else {
                    $scope.reportmodechanged();
                }

            }
            else {
                $timeout(function () {
                    $scope.LoadAppSetting();
                }, 50);
            }
        }

        $scope.reportmode = 'SALES';
        $scope.defaultdate = true;
        $scope.reportmodechanged = function () {
            $scope.frefattributes = $scope.Filter($scope.refattributes);
            $scope.frefbusiness = $scope.Filter($scope.refbusiness);
            $scope.frefaggregate = $scope.Filter($scope.refaggregate);
            $scope.frefconditions = $scope.Filter($scope.refconditions);
        }

        $scope.SaveWidget = function () {

            var dialog = ngDialog.open({
                template: 'SaveWidget',
                data: {
                    'parent': $scope,
                    'restService': restService
                },
                width: '650px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.StorageModes = [{ 'code': 'STATIC', 'name': 'STATIC' },
                    { 'code': 'MANUAL', 'name': 'MANUAL' },
                    { 'code': 'REALTIME', 'name': 'REALTIME' },
                    { 'code': 'PERIODIC', 'name': 'PERIODIC' }];

                    $scope.DisplayModes = [{ 'code': 'LIST', 'name': 'LIST' },
                    { 'code': 'CHART', 'name': 'CHART' },
                    { 'code': 'DUAL', 'name': 'DUAL' },
                    { 'code': 'SINGLE', 'name': 'SINGLE' }];

                    $scope.SIZES = [{ 'code': 'FULL', 'name': 'FULL' },
                    { 'code': 'HALF', 'name': 'HALF' },
                    { 'code': 'QUARTER', 'name': 'QUARTER' }];

                    $scope.SINGLESIZES = [{ 'code': 'FULL', 'name': 'FULL' },
                    { 'code': 'HALF', 'name': 'HALF' },
                    { 'code': 'QUARTER', 'name': 'QUARTER' },
                    { 'code': 'TWO', 'name': 'TWO' },
                    { 'code': 'SINGLE', 'name': 'SINGLE' }
                    ];

                    $scope.parent = $scope.ngDialogData.parent;
                    $scope.aggregatelist = JSON.parse(JSON.stringify($scope.parent.currentresult.aggregatelist));
                    $scope.ngDialogData.restService.Get('ReferencelistService/USERLIST', true).then(function (results) {
                        $scope.UserList = results;
                    });
                    $scope.Select = function () {
                        var response = {
                            'name': $scope.name,
                            'title': $scope.title,
                            'datastorage': $scope.datastorage,
                            'displaymode': $scope.displaymode,
                            'accessall': $scope.accessall,
                            'width': $scope.width,
                            'topcount': $scope.topcount,
                            'topfield': $scope.topfield,
                            'firstvalue': $scope.firstvalue,
                            'secondvalue': $scope.secondvalue,
                            'height': $scope.height,
                            'userids': []
                        };
                        $.each($scope.UserList, function (k, v) {
                            if (v.access)
                                response.userids.push(v.id);
                        })
                        $scope.closeThisDialog(response);
                    }
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                        var config = $("#pivotContainer").data("pivotUIOptions");
                        var config_copy = JSON.parse(JSON.stringify(config));
                        delete config_copy["aggregators"];
                        delete config_copy["renderers"];
                        var hastop = data.value.topcount && data.value.topcount != null &&
                            data.value.topfield && data.value.topfield != null && data.value.topcount > 0;
                        data.value.datastorage = 'MANUAL';
                        var wgconfig = {
                            'title': data.value.title,
                            'name': data.value.name,
                            'datastorage': data.value.datastorage,
                            'displaymode': data.value.displaymode,
                            'userids': data.value.userids,
                            'width': data.value.width,
                            'firstvalue': data.value.firstvalue,
                            'secondvalue': data.value.secondvalue,
                            'topcount': data.value.topcount,
                            'topfield': data.value.topfield,
                            'height': data.value.height,
                            'accessall': data.value.accessall,
                            'pivotconfig': JSON.stringify(config_copy),
                            'rptrequest': $scope.Request,
                            'data': data.value.datastorage == 'STATIC' ? JSON.stringify($scope.PivotList) : null
                        };

                        var configuration = {
                            'name': data.value.name,
                            'key': 'WGCONFIG',
                            'reportformat': { 'format': JSON.stringify(wgconfig) }
                        };
                        request = {
                            "Item": configuration,
                            "securitytoken": "String content"
                        }

                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "ConfigurationService/Save",
                            data: srequest
                        }).success(function (result) {

                        });
                    }
                }
            });
        }

        $scope.ShowTool = function () {
            //var config = $("#pivotContainer").data("pivotUIOptions");
            //var config_copy = JSON.parse(JSON.stringify(config));
            ////delete some values which will not serialize to JSON
            //delete config_copy["aggregators"];
            //delete config_copy["renderers"];
            //$.cookie("pivotConfig", JSON.stringify(config_copy));

            var rows = $("#pivotContainer tbody").children();
            if (rows.length >= 3) {
                $(rows[0]).show()
                $(rows[1]).show()
                $($(rows[2]).children()[0]).show()
            }
        }

        $scope.HideTool = function () {
            var rows = $("#pivotContainer tbody").children();
            if (rows.length >= 3) {
                $(rows[0]).hide()
                $(rows[1]).hide()
                $($(rows[2]).children()[0]).hide()
            }
        }

        $scope.Filter = function (list) {
            var flist = [];
            $.each(list, function (k, v) {
                if ($scope.reportmode == 'SALES' && v.support_sales)
                    flist.push(v);
                if ($scope.reportmode == 'STOCK' && v.support_stock)
                    flist.push(v);
                if ($scope.reportmode == 'PURCHASE' && v.support_purchase)
                    flist.push(v);
                if ($scope.reportmode == 'DEBIT NOTE' && v.support_debitnote)
                    flist.push(v);
            });
            return flist;
        }

        $scope.DownloadDumpStatus = 'Dump';
        $scope.DownloadDump = function () {

            if ($scope.reportmode == null || $scope.reportmode == '') {
                $rootScope.Toaster.Info("Please Select Report mode");
                return;
            }

            $scope.ColumnList = [];
            $scope.Condition = [];
            $scope.Aggregation = [];
            $.each($scope.refconditions, function (k, v) {
                if (typeof (v.selected) != 'undefined' && v.selected) {
                    $scope.Condition.push(v);
                }
            })

            $.each($scope.refattributes, function (k, v) {
                if (typeof (v.selected) != 'undefined' && v.selected) {
                    $scope.ColumnList.push(v);
                }
            })
            $.each($scope.refbusiness, function (k, v) {
                if (typeof (v.selected) != 'undefined' && v.selected) {
                    $scope.ColumnList.push(v);
                }
            })
            $.each($scope.refaggregate, function (k, v) {
                if (v.id != Options.ValueType) {
                    $scope.SupportValueType = true;
                }
                else if (typeof (v.selected) != 'undefined' && v.selected) {
                    $scope.Aggregation.push(v);
                }
            })
            $scope.DownloadDumpStatus = 'Downloading...';
            request = {
                "item": {
                    'creterialist': $scope.AddedCreteria,
                    'columnlist': $scope.ColumnList,
                    'aggregatelist': $scope.Aggregation,
                    'conditionlist': $scope.Condition,
                    'reportmode': $scope.reportmode,
                    'locationid': $scope.locationid,
                    'datemode': $scope.defaultdate,
                    'daterange': $scope.defaultdatrange,
                    'fromdate': $scope.fromdate,
                    'todate': $scope.todate
                },
                "securitytoken": "String content"
            }
            $scope.Request = request.item;
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "AnalyserService/ProcessReportBuilder",
                data: srequest
            }).success(function (result) {
                $scope.UploadedingStatus = result;
                $scope.ProgressValue = 0;
                $scope.migrationtimer = $timeout(function () {
                    $scope.CheckStatus(result.identifier);
                }, 2000);
            }).error(function (result) {
            });
        }

        $scope.CheckStatus = function (identifier) {
            restService.Get('File/ProcessStatus/' + $scope.UserInfo.OrganizationID + '/' + identifier).then(function (result) {
                $scope.UploadedingStatus = result;
                $scope.ProgressValue = result.percentage;
                $scope.DownloadDumpStatus = 'Download ' + result.percentage + '%';
                if (!result.hascompleted) {
                    $scope.migrationtimer = $timeout(function () {
                        $scope.CheckStatus(identifier);
                    }, 2000);
                }
                else {
                    $scope.DownloadDumpStatus = 'Downloading';
                    $rootScope.Download('File/ProcessDownloadXLSX/' + $scope.UserInfo.OrganizationID + '/' + identifier,
                        'datadump',
                        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', function () {
                            $scope.DownloadDumpStatus = 'Dump';
                        });
                }
            });
        }


        $timeout(function () {
            $scope.fromdate = $filter('date')(new Date(), datePicker.shortDisplay);
            $scope.todate = $filter('date')(new Date(), datePicker.shortDisplay);
            $scope.LoadAppSetting();
        }, 50);
    }]);