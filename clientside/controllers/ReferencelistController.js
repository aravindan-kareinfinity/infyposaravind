
var app = angular.module('InfinityLab');

app.controller('ReferencelistController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window',
    'restService', '$timeout', '$stateParams', 'Excel', 'ngDialog', '$compile',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout,
        $stateParams, Excel, ngDialog, $compile) {
        /*$scope.ReferenceTypeList = [{ 'name': 'Country', 'id': 'COUNTRY' }, { 'name': 'Counter', 'id': 'COUNTER' }, { 'name': 'State', 'id': 'STATE' },
            { 'name': 'City', 'id': 'CITY' }, { 'name': 'Area Name', 'id': 'AREA' }, { 'name': 'Material', 'id': 'MATERIAL' },
            { 'name': 'Brand', 'id': 'BRAND' }, { 'name': 'Pattern', 'id': 'PATTERN' }, { 'name': 'Size', 'id': 'SIZE' }, { 'name': 'Style', 'id': 'STYLE' },
            { 'name': 'Location', 'id': 'LOCATION' }, { 'name': 'Designation', 'id': 'DESIGNATION' }, { 'name': 'Fright Charge', 'id': 'FRIGHTCHARGE' },
            { 'name': 'Colour', 'id': 'COLOUR' }, { 'name': 'Courier Type', 'id': 'COURIERTYPE' }];*/
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.IsAttributes = $stateParams.type == 'PRODUCT';
        $scope.ShowType = true;
        $rootScope.AccordionName = $location.path();
        $scope.selectedcount = 0;
        $scope.Entity = {
            "isactive": true,
            "createdby": "",
            "modifiedby": "",
            "version": 0,
            "createdon": "",
            "modifiedon": "",
            "type": "",
            "additionalinfo": {
                "deductsalary": "", "formula": "", "paytype": "", "fixedincentive": "", "dynamicincentive": "", "eligbleday": "", "present": "", "dispressmentmode": "",
                "splpaytypelistid": "", "fromtime": "", "totime": "", "iswarehouse": "", "autosettlement": "", "rewardpoint": "", "billdiscount": "", "address": "",
                "staticip": [], supplierid: "",
                "incentiveconfig": {},
                "mfgconfig": {},
                "reportconfig": {
                    Bata_Minimum_EntryTime: "",
                    Bata_Male_BeforeTime: "",
                    Bata_Male_AfterTime: "",
                    Bata_Male_BefAmount: "",
                    Bata_Male_AftAmount: "",
                    Bata_Female_BeforeTime: "",
                    Bata_Female_AfterTime: "",
                    Bata_Female_BefAmount: "",
                    Bata_Female_AftAmount: "",
                    Bonus_Percentage: "",
                    OT_Male_BeforeTime: "",
                    OT_Male_AfterTime: "",
                    OT_Male_BefAmount: "",
                    OT_Male_AftAmount: "",
                    Dailygracetime: "",
                    Lunchgracetime: "",
                    Monthlygracetime: "",
                    Fine_Male: "",
                    Fine_Female: ""
                },
                "permissionset": [],
                "salesincentive": [],
                "fromdate": null,
                "todate": null
            },

            "sortingindex": "",
            "id": 0,
            "code": "",
            "name": "",
            productgroup: 0,
            referenceid: ""
        };

        if ($stateParams.type == 'PRODUCT') {
            $scope.ReferenceTypeList = [{ 'name': 'SIZE', 'code': 'SIZE' },
            { 'name': 'SIZE GROUP', 'code': 'SIZEGROUP' },
            { 'name': 'PATTERN', 'code': 'PATTERN' },
            { 'name': 'STYLE', 'code': 'STYLE' },
            { 'name': 'TYPE', 'code': 'TYPE' }, { 'name': 'COLOUR', 'code': 'COLOUR' },
            { 'name': 'MATERIAL', 'code': 'MATERIAL' },
            { 'name': 'SLEEVE', 'code': 'SLEEVE' },
            { 'name': 'FIT', 'code': 'FIT' },
            { 'name': 'STOCK MARKER', 'code': 'MARKER' }, { 'name': 'AGESET', 'code': 'AGESET' },
            { 'name': 'UNITS', 'code': 'UNITS' },
            { 'name': 'PRODUCT GROUPS', 'code': 'PRODUCTGROUPS' },
            { 'name': 'PRODUCT HIERARCHY', 'code': 'PRODUCTHIERARCHY' },
            { 'name': 'SUPPLIER GROUP', 'code': 'SUPPLIERGROUP' },
            { 'name': 'CUSTOMER GROUP', 'code': 'CUSTOMERGROUP' },
            { 'name': 'BUYER GROUP', 'code': 'BUYERSGROUP' },
            { 'name': 'BARCODE ID', 'code': 'BARCODEID' },
            { 'name': 'SECTION', 'code': 'DIVISION' },
            { 'name': 'PRODUCT DIVISION', 'code': 'PRODUCTDIVISION' },
            { 'name': 'ITEM GROUPS', 'code': 'ITEMGROUPS' },
            { 'name': 'PRICE TAGS', 'code': 'PRICETAGS' },
            { 'name': 'SELLING NAME TEMPLATE', 'code': 'SELLINGNAMES' },
            { 'name': 'PRODUCT COVERAGE', 'code': 'PRODUCTCOVERAGE' },
            { 'name': 'DAILY PRICE GROUP', 'code': 'DAILYPRICEGROUP' },
            { 'name': 'CORE PRODUCT GROUP', 'code': 'COREPRODUCTGROUP' },
            { 'name': 'DUMPING GROUP', 'code': 'DUMPINGGROUP' },
            { 'name': 'CUSTOM FIELDS', 'code': 'CUSTOMFIELDS' }];

            $scope.BarcodeIDList = [];
            restService.Get('ReferencelistService/BARCODEID', true).then(function (results) {
                $scope.BarcodeIDList = results;
            });
        }
        else if ($stateParams.type == 'PRODUCTGROUP') {
            $scope.ReferenceTypeList = [];
            $scope.ShowType = false;
            if ($stateParams.productgroup != null) {
                $scope.Entity.productgroup = $stateParams.productgroup.id;
                if ($stateParams.productgroup.materialenable)
                    $scope.ReferenceTypeList.push({ 'name': $stateParams.productgroup.materialname, 'code': 'MATERIAL_PG_' + $stateParams.productgroup.id });
                if ($stateParams.productgroup.patternenable)
                    $scope.ReferenceTypeList.push({ 'name': $stateParams.productgroup.patternname, 'code': 'PATTERN_PG_' + $stateParams.productgroup.id });
                if ($stateParams.productgroup.styleenable)
                    $scope.ReferenceTypeList.push({ 'name': $stateParams.productgroup.stylename, 'code': 'STYLE_PG_' + $stateParams.productgroup.id });
                if ($stateParams.productgroup.sizeenable)
                    $scope.ReferenceTypeList.push({ 'name': $stateParams.productgroup.sizename, 'code': 'SIZE_PG_' + $stateParams.productgroup.id });
                if ($stateParams.productgroup.colourenable)
                    $scope.ReferenceTypeList.push({ 'name': $stateParams.productgroup.colourname, 'code': 'COLOUR_PG_' + $stateParams.productgroup.id });
                if ($stateParams.productgroup.typeenable)
                    $scope.ReferenceTypeList.push({ 'name': $stateParams.productgroup.typename, 'code': 'TYPE_PG_' + $stateParams.productgroup.id });
            }
        }
        else if ($stateParams.type == 'POS') {
            $scope.ReferenceTypeList = [{ 'name': 'POSCATEGORY', 'code': 'POSCATEGORY' }, { 'name': 'POSSUBCATEGORY', 'code': 'POSSUBCATEGORY' }, { 'name': 'POSBRAND', 'code': 'POSBRAND' }];
        }
        else if ($stateParams.type == 'MGR') {
            $scope.ReferenceTypeList = [{ 'name': 'BOM CATEGORY', 'code': 'BOMCATEGORY' },
            { 'name': 'UOM', 'code': 'UOM' },
            { 'name': 'SIZE', 'code': 'SIZE' },
            { 'name': 'SIZE GROUP', 'code': 'SIZEGROUP' },
            { 'name': 'PATTERN', 'code': 'PATTERN' },
            { 'name': 'STYLE', 'code': 'STYLE' },
            { 'name': 'TYPE', 'code': 'TYPE' }, { 'name': 'COLOUR', 'code': 'COLOUR' },
            { 'name': 'MATERIAL', 'code': 'MATERIAL' },
            { 'name': 'SLEEVE', 'code': 'SLEEVE' },
            { 'name': 'FIT', 'code': 'FIT' },
            { 'name': 'DELIVERY LOCATION', 'code': 'DELIVERYLOCATION' },
            { 'name': 'JOB WORK CHARGE', 'code': 'JOBCHARGE' },
            { 'name': 'DEPARTMENT', 'code': 'MFRDEPARTMENT' },
            { 'name': 'JOB WORK REASONS', 'code': 'MFRJOBREASON' },
            { 'name': 'FABRIC', 'code': 'FABRIC' },
            { 'name': 'STAGES', 'code': 'MFGSTAGES' },
            { 'name': 'SAMPLE STATUS', 'code': 'MFGSAMPLESTATUS' },
            { 'name': 'SAMPLE QUALITY', 'code': 'MFGSAMPLEQUALITY' },
            { 'name': 'PRODUCT FORM', 'code': 'MFGPRDFORM' },
            { 'name': 'PRODUCTION MACHINE', 'code': 'MFGPRDMACHINE' },
            { 'name': 'SEASON', 'code': 'SEASON' },
            { 'name': 'PRODUCT DIVISION', 'code': 'PRODUCTDIVISION' },
            { 'name': 'CLASSIFICATION', 'code': 'CLASSIFICATION' }];
            $scope.MfgStages = restService.Get('ReferencelistService/MFGSTAGES', true).then(function (results) {
                $scope.MfgStages = results;
            });
        }
        else if ($stateParams.type == 'HR') {
            $scope.ReferenceTypeList = [{ 'name': 'LEAVE TYPE', 'code': 'LEAVETYPE' }, { 'name': 'DESIGNATION', 'code': 'DESIGNATION' }, { 'name': 'SECTION', 'code': 'DIVISION' },
            { 'name': 'FLOOR', 'code': 'FLOOR' }, { 'name': 'EDUCATION TYPE', 'code': 'EDUCATIONTYPE' }, { 'name': 'SETTINGS', 'code': 'SETTINGS' },
            { 'name': 'PAY CATEGORY', 'code': 'PAYCATEGORY' }, { 'name': 'SPECIAL PAYMENT', 'code': 'SPECIALPAYMENT' },
            { 'name': 'ROLE', 'code': 'ROLE' }, { 'name': 'WORKING HOURS', 'code': 'WORKINGHOURS' }, { 'name': 'USER ROLE', 'code': 'USERROLE' }, { 'name': 'DEPARTMENT', 'code': 'DEPARTMENT' },
            { 'name': 'CASTE', 'code': 'CASTE' }, { 'name': 'COMMUNITY', 'code': 'COMMUNITY' }, { 'name': 'BLOOD GROUP', 'code': 'BLOODGROUP' },
                { 'name': 'CITY', 'code': 'CITY' }, { 'name': 'DISTRICT', 'code': 'DISTRICT' },
                { 'name': 'STATE', 'code': 'STATE' }, { 'name': 'COUNTRY', 'code': 'COUNTRY' },
                { 'name': 'PORT', 'code': 'PORT' }, { 'name': 'RELIGION', 'code': 'RELIGION' },
                { 'name': 'PERSONS', 'code': 'PERSONS' }];
        }
        else {
            $scope.ReferenceTypeList = [{ 'name': 'LOCATION', 'code': 'LOCATION' }, { 'name': 'COUNTER', 'code': 'COUNTER' }, { 'name': 'TAILORING UNIT', 'code': 'TAILORINGUNIT' }, { 'name': 'PRF REASON', 'code': 'PRFRESON' },
            { 'name': 'RETURN REASON', 'code': 'RETURNREASON' }, { 'name': 'SALE REASON', 'code': 'BULKSALEREASON' }, { 'name': 'SEASONS', 'code': 'SEASONS' }, { 'name': 'PO TYPE / BUDGET TYPE', 'code': 'POTYPE' },
            { 'name': 'SETTINGS', 'code': 'SETTINGS' }, { 'name': 'BANK', 'code': 'BANK' }, { 'name': 'SWIPPING MACHINE', 'code': 'SWIPPINGMACINE' },
            { 'name': 'CARD TYPES', 'code': 'CARDTYPES' }, { 'name': 'COURIER TYPE', 'code': 'COURIERTYPE' }, { 'name': 'SALE AREA', 'code': 'AREA' },
            { 'name': 'DISCOUNT REMARK', 'code': 'DISCOUNTREMARK' }, { 'name': 'AREA GROUP 1', 'code': 'AREAGROUP1' }, { 'name': 'AREA GROUP 2', 'code': 'AREAGROUP2' },
            { 'name': 'PRIORITY', 'code': 'PRIORITY' }, { 'name': 'CITY', 'code': 'CITY' }, { 'name': 'DISTRICT', 'code': 'DISTRICT' }, { 'name': 'STATE', 'code': 'STATE' }, { 'name': 'EXPENSES TYPE', 'code': 'EXPENSESTYPE' },
            { 'name': 'CUSTOMER CATEGORY', 'code': 'CUSTOMERCATEGORY' }, { 'name': 'VENDOR', 'code': 'VENDOR' }, { 'name': 'OFFLINE CLIENT', 'code': 'OFFLINECLIENT' },
            { 'name': 'AUTO NUMBER', 'code': 'AUTONUMBER' }, { 'name': 'LOAN PROVIDER', 'code': 'LOANPROVIDER' }, { 'name': 'DELIVERYLOCATION', 'code': 'DELIVERYLOCATION' },
            { 'name': 'ADDITIONAL CHARGES', 'code': 'ADDITIONALCHARGES' }, { 'name': 'UPI PROVIDER', 'code': 'UPIPROVIDER' },
            { 'name': 'JOB WORK CHARGE', 'code': 'JOBCHARGE' }, { 'name': 'TAILORING CHARGE', 'code': 'TAILORINGCHARGE' }, { 'name': 'BUNDLE RACK', 'code': 'BUNDLERACK' },
            { 'name': 'DOCUMENT IMPORT', 'code': 'BILLIMPORT' }, { 'name': 'SUPPLIER GROUP', 'code': 'SUPPLIERGROUP' },
            { 'name': 'VOUCHER TYPE', 'code': 'VOUCHERTYPE' }, { 'name': 'BUYER GROUP', 'code': 'BUYERSGROUP' }, { 'name': 'CUSTOMER GROUP', 'code': 'CUSTOMERGROUP' },
            { 'name': 'LEAD EVENT', 'code': 'LEADEVENT' }, { 'name': 'LEAD STATUS', 'code': 'LEADSTATUS' }, { 'name': 'LEAD PRIORITY', 'code': 'LEADPRIORITY' },
            { 'name': 'ASSET TYPES', 'code': 'ASSETTYPE' }, { 'name': 'FOOD COUNTER', 'code': 'FOODCOUNTER' }, { 'name': 'DOCUMENT TYPE', 'code': 'DOCTYPE' },
            { 'name': 'CRM SOURCES', 'code': 'REGSOURCE' }, { 'name': 'CRM VISITING', 'code': 'REGVIST' },
                { 'name': 'PURCHASE TYPE', 'code': 'PURCHASETYPE' }, { 'name': 'ADDRESS TYPE', 'code': 'ADDRESSTYPE' },
                { 'name': 'TDS GROUP', 'code': 'TDSGROUP' }, { 'name': 'PERSONS', 'code': 'PERSONS' }];
        }

        $scope.ImportTargets = [
            { 'id': 0, 'name': 'LR Invoices' },
            { 'id': 1, 'name': 'Sales Order By UAN' },
            { 'id': 2, 'name': 'Invoices By UAN' },
            { 'id': 3, 'name': 'Purchase Order By UAN' },
            { 'id': 4, 'name': 'SKU By UAN' },
            { 'id': 5, 'name': 'Stock Order By UAN' },
            { 'id': 6, 'name': 'Invoices From E-Commerce' },
        ]

        $scope.StandaryRoles = [
            { 'id': 1, 'name': 'Operational Manager' },
            { 'id': 2, 'name': 'Purchase Manager' },
            { 'id': 3, 'name': 'Sales Manager' },
            { 'id': 4, 'name': 'Sales Incharges' },
            { 'id': 5, 'name': 'Salesmen' },
            { 'id': 6, 'name': 'Biller' },
            { 'id': 7, 'name': 'Cashier' },
            { 'id': 8, 'name': 'Sales Support Staff' },
            { 'id': 9, 'name': 'Purchase Support Staff' },
            { 'id': 10, 'name': 'Operational Support Staff' },
            { 'id': 11, 'name': 'Accounts Manager' },
            { 'id': 12, 'name': 'Accounts Support Staff' }
        ];

        $scope.PersonRoles = [
            { 'id': 1, 'name': 'Lorry Pickup' },
            { 'id': 2, 'name': 'Courier Pickup' },
            { 'id': 3, 'name': 'General' },
        ];

        $scope.refvouchertypelist = [
            { 'id': 1, 'name': 'Payment' },
            { 'id': 2, 'name': 'Receipt' },
            { 'id': 3, 'name': 'Purchase' },
            { 'id': 4, 'name': 'Sales' },
            { 'id': 5, 'name': 'Credit Note' },
            { 'id': 6, 'name': 'Debit Note' },
            { 'id': 7, 'name': 'Contra' },
            { 'id': 8, 'name': 'Journal' },
            { 'id': 9, 'name': 'Cash Deposit' },
            { 'id': 10, 'name': 'Cash Widhrawal' },
            { 'id': 11, 'name': 'Main Cash to Petty Cash' },
            { 'id': 12, 'name': 'Petty Cash to Main Cash' },
            { 'id': 13, 'name': 'Cash Counter Expense' },
            { 'id': 14, 'name': 'Advance Payment' }
        ];
        $scope.CustomFieldTypes = [
            { 'id': '1', 'name': 'Text' },
            { 'id': '2', 'name': 'Number' },
            { 'id': '3', 'name': 'Date' },
            { 'id': '4', 'name': 'Reference List' },
            { 'id': '5', 'name': 'Colours' },
            { 'id': '6', 'name': 'Size' },
            { 'id': '7', 'name': 'Fit' },
            { 'id': '8', 'name': 'Type' },
            { 'id': '9', 'name': 'Material' },
            { 'id': '10', 'name': 'Style' },
            { 'id': '11', 'name': 'Pattern' },
            { 'id': '12', 'name': 'Season' },
            { 'id': '13', 'name': 'Classification' },
            { 'id': '14', 'name': 'Product Division' }
        ];

        $scope.PaymentIntegrationModes = [
            { 'id': 0, 'name': 'None', 'keys': [] },
            { 'id': 1, 'name': 'innoviti', 'keys': [] },
            { 'id': 2, 'name': 'razorpay', 'keys': ['Device ID', 'User Name', 'Password', 'App Key'] },
            { 'id': 3, 'name': 'pinelab', 'keys': ['POS Code', 'IMEI', 'Merchant ID', 'Security Token'] },
            { 'id': 4, 'name': 'paytm', 'keys': ['Channel', 'Version', 'Merchant ID', 'Device ID', 'Access Key'] },
        ];


        $scope.MfgSampleStatusList = [
            { 'id': '0', 'name': 'None' },
            { 'id': '1', 'name': 'Requested' },
            { 'id': '2', 'name': 'Rejected' },
            { 'id': '3', 'name': 'Completed' },
            { 'id': '4', 'name': 'Processing' },
            { 'id': '5', 'name': 'Client Validation' },
            { 'id': '7', 'name': 'Client Change Requested' },
            { 'id': '7', 'name': 'Client Approved' },
            { 'id': '10', 'name': 'Ready for Production' },
        ];

        $scope.MarginModes = [
            { 'id': 'buyingprice', 'name': 'Buying Price - Markup' },
            { 'id': 'mrp', 'name': 'MRP - Mark down' },
            { 'id': 'price', 'name': 'Price - Mark down' }
        ];

        $scope.SizeRatioTypes = [
            { 'id': 0, 'name': 'None' },
            { 'id': 1, 'name': 'Even Qty' },
            { 'id': 2, 'name': 'Hierarchy' }
        ];


        $scope.MarginTaxModes = [
            { 'id': 0, 'name': 'Normal' },
            { 'id': 1, 'name': 'Net Of Tax' },
            { 'id': 2, 'name': 'Exclude Tax On MRP' },
            { 'id': 3, 'name': 'Extra Margin On Higher Tax Slab' },
            { 'id': 4, 'name': 'Include Tax On MRP' }, //Toniq,
            { 'id': 5, 'name': 'Additional Tax On MRP' } //Yesto
        ];

        $scope.BOMCoreTypes = [
            { 'id': 0, 'name': 'None' },
            { 'id': 1, 'name': 'Fabric' },
            { 'id': 2, 'name': 'Accessories' },
            { 'id': 3, 'name': 'Yarn' },
            { 'id': 31, 'name': 'Beam' },
            { 'id': 32, 'name': 'Trim' },
            { 'id': 6, 'name': 'Button' },
            { 'id': 7, 'name': 'Tag' },
            { 'id': 8, 'name': 'Carton' },
            { 'id': 9, 'name': 'Label' },
            { 'id': 10, 'name': 'Zip' },
            { 'id': 11, 'name': 'Tab' },
            { 'id': 12, 'name': 'Bobbin' },
            { 'id': 4, 'name': 'Others' }
        ];

        $scope.BarcodeIDList = restService.Get('ReferencelistService/BARCODEID', true).then(function (results) {
            $scope.BarcodeIDList = results;
        });

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.Customers = restService.Get('ReferencelistService/RETAILCUSTOMERLEDGER', true).then(function (results) {
            $scope.Customers = results;
        });
        restService.Get('ReferencelistService/LIST-PRODUCT', true).then(function (results) {
            $scope.ProductList = results;
        });
        $scope.PriceTags = restService.Get('ReferencelistService/PRICETAGS', true).then(function (results) {
            $scope.PriceTags = results;
        });
        $scope.RetailLocations = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            $scope.RetailLocations = results;
        });
        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });

        $scope.CourierCompanies = restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $scope.CourierCompanies = results;
        });

        $scope.Additionalcharges = restService.Get('ReferencelistService/ADDITIONALCHARGES', true).then(function (results) {
            $scope.Additionalcharges = results;
        });

        $scope.AreaGroup1 = restService.Get('ReferencelistService/AREAGROUP1', true).then(function (results) {
            $scope.AreaGroup1 = results;
        });

        $scope.AreaGroup2 = restService.Get('ReferencelistService/AREAGROUP2', true).then(function (results) {
            $scope.AreaGroup2 = results;
        });

        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
        });
        $scope.SizeList = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
        });
        $scope.StateList = restService.Get('ReferencelistService/STATE', true).then(function (results) {
            $scope.StateList = results;
        });
        $scope.CityList = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.CityList = results;
        });
        $scope.DistrictList = restService.Get('ReferencelistService/DISTRICT', true).then(function (results) {
            $scope.DistrictList = results;
        });
        $scope.UOMList = restService.Get('ReferencelistService/UOM', true).then(function (results) {
            $scope.UOMList = results;
        });
        $scope.SalesTax = restService.Get('ReferencelistService/SALESTAX', true).then(function (results) {
            $scope.SalesTax = results;
        });
        $scope.Floors = restService.Get('ReferencelistService/FLOOR', true).then(function (results) {
            $scope.Floors = results;
        });
        $scope.Banks = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            $scope.Banks = results;
        });
        $scope.PermissionList = restService.Get('ReferencelistService/PERMISSION', true).then(function (results) {
            $scope.PermissionList = results;
        });
        $scope.ReportList = restService.Get('ConfigurationService/ReportList', true).then(function (results) {
            $scope.ReportList = results;
        });

        $scope.AgeSet = restService.Get('ReferencelistService/AGESET', true).then(function (results) {
            $scope.AgeSet = results;
        });

        $scope.ReportList = restService.Get('ConfigurationService/ReportList?id=CHEQUEPRINT', true).then(function (results) {
            $scope.ReportList = results;
        });

        $scope.BOMCategoryList = restService.Get('ReferencelistService/BOMCATEGORY', true).then(function (results) {
            $scope.BOMCategoryList = results;
        });

        $scope.PrintTemplates = restService.Get('ConfigurationService/ReportList?id=SALERECEIPTPDF,SALERETURN,SALEINVOICE,SALERECEIPTCUSTOM', true).then(function (results) {
            $scope.PrintTemplates = results;
        });

        $scope.ProductGroups = restService.Get('ReferencelistService/FULL-PRODUCTGROUPS', true).then(function (results) {
            $scope.ProductGroups = results;
        });

        $scope.ParentHierarchyName = function (list, parentid) {
            var parent = $scope.ProductHierarchy.find(function (item) {
                return item.id == parentid;
            });
            if (parent && parent != null) {
                list.push(parent.name);
                $scope.ParentHierarchyName(list, parent.referenceid);
            }
        }

        $scope.ProductHierarchy = restService.Get('ReferencelistService/FULL-PRODUCTHIERARCHY', true).then(function (results) {
            $scope.ProductHierarchy = results;
            var ProductGroupFirst = [];
            var ProductGroupParents = [];
            $.each(results, function (k, v) {
                if (v.additionalinfo &&
                    v.additionalinfo.hierarchyconfig &&
                    v.additionalinfo.hierarchyconfig.islast) {
                    var list = [v.name];
                    $scope.ParentHierarchyName(list, v.referenceid);
                    v.parentdisplay = list.join(' <- ');
                    ProductGroupParents.push(v);
                }
                if (v.additionalinfo &&
                    v.additionalinfo.hierarchyconfig &&
                    v.additionalinfo.hierarchyconfig.isfirst) {
                    ProductGroupFirst.push(v);
                }
            })
            $scope.BuildProductHierarchy(ProductGroupFirst, results);
            $scope.ProductGroupParents = ProductGroupParents;
        });

        $scope.UOM = [{ 'id': 0, 'name': 'Piece' }, { 'id': 1, 'name': 'Meter' },
        { 'id': 2, 'name': 'Gram' },
        { 'id': 3, 'name': 'KG' },
        { 'id': 4, 'name': 'Ml' },
        { 'id': 5, 'name': 'Litter' }];

        $scope.MonthList = [{ 'id': '1', 'name': 'JANUARY' }, { 'id': '2', 'name': 'FEBRUARY' },
        { 'id': '3', 'name': 'MARCH' }, { 'id': '4', 'name': 'APRIL' },
        { 'id': '5', 'name': 'MAY' }, { 'id': '6', 'name': 'JUNE' },
        { 'id': '7', 'name': 'JULY' }, { 'id': '8', 'name': 'AUGUST' },
        { 'id': '9', 'name': 'SEPTEMBER' }, { 'id': '10', 'name': 'OCTOBER' },
        { 'id': '11', 'name': 'NOVEMBER' }, { 'id': '12', 'name': 'DECEMBER' }];

        $scope.incentivemodes = [{ 'id': 'Fixed', 'name': 'Fixed' }, { 'id': 'Sales', 'name': 'Sales' }];

        $scope.refpaytypelist = [{ 'id': 'BasicPay', 'name': 'Basic Pay' }, { 'id': 'Earnings', 'name': 'Earnings' },
        { 'id': 'ProvidentFund', 'name': 'Provident Fund' }, { 'id': 'VPF', 'name': 'VPF' },
        { 'id': 'ESI', 'name': 'ESI' }, { 'id': 'VPF', 'name': 'VPF' },
        { 'id': 'IncomeTax', 'name': 'Income Tax' }, { 'id': 'HouseRentAllowance', 'name': 'House Rent Allowance' },
        { 'id': 'DearnessAllowance', 'name': 'Dearness Allowance' }, { 'id': 'Allowance', 'name': 'Allowance' },
        { 'id': 'Deduction', 'name': 'Deduction' }, { 'id': 'MedicalInsurance', 'name': 'MedicalInsurance' },
        { 'id': 'Overtime', 'name': 'Overtime' }, { 'id': 'Rounding', 'name': 'Rounding' },
        { 'id': 'Advance', 'name': 'Advance' }, { 'id': 'LoanDeduction', 'name': 'LoanDeduction' },
        { 'id': 'Bonus', 'name': 'Bonus' }, { 'id': 'Increment', 'name': 'Increment' },
        { 'id': 'LeaveEncashment', 'name': 'Leave Encashment' }, { 'id': 'Conveyance', 'name': 'Conveyance' },
        { 'id': 'SpecialDeduction', 'name': 'Special Deduction' },
        { 'id': 'OtherAllowance', 'name': 'Other Allowances' },
        { 'id': 'SpecialAllowance', 'name': 'Special Allowances' },
        { 'id': 'ESI_Employee', 'name': 'ESI Employer' },
        { 'id': 'PF_Employee', 'name': 'PF Employer' },
        { 'id': 'FixedAllowance', 'name': 'Fixed Allowance' },
        { 'id': 'Incentives', 'name': 'Incentive' },
        { 'id': 'FixedDeduction', 'name': 'Fixed Deduction' },
        { 'id': 'HolidaySalary', 'name': 'Holiday Salary' },
        { 'id': 'FineDeduction', 'name': 'Fine Deduction' }];

        $scope.splpaytypelist = [{ 'id': 'Bonus', 'name': 'Bonus' }, { 'id': 'Beta', 'name': 'Beta' },
        { 'id': 'OverTime', 'name': 'OverTime' }, { 'id': 'Incentive', 'name': 'Incentive' }, { 'id': 'Fine', 'name': 'Fine' }
        ];

        $scope.AdditionalChargeTypes = [{ 'id': 'FRIGHT', 'name': 'Fright' },
        { 'id': 'COURIER', 'name': 'Courier' },
        { 'id': 'PACKING', 'name': 'Packing' },
        { 'id': 'BOX', 'name': 'Box' },
        { 'id': 'TCS', 'name': 'TCS' },
        { 'id': 'SERVICE', 'name': 'SERVICE' },
        { 'id': 'EXPORTGST', 'name': 'EXPORT GST' },
        { 'id': 'ROUND', 'name': 'Rounding' }];

        $scope.ChangeReport = function () {
            if ($scope.Referencelist.additionalinfo.splpaytypelistid == 'Bonus') {
                $scope.Bonus = true;
                $scope.Beta = false;
                $scope.OverTime = false;
                $scope.Fine = false;

            }
            else if ($scope.Referencelist.additionalinfo.splpaytypelistid == 'Beta' || $scope.Referencelist.additionalinfo.splpaytypelistid == 'Incentive') {
                $scope.Bonus = false;
                $scope.Beta = true;
                $scope.OverTime = false;
                $scope.Fine = false;

            }
            else if ($scope.Referencelist.additionalinfo.splpaytypelistid == 'Fine') {
                $scope.Bonus = false;
                $scope.Beta = false;
                $scope.OverTime = false;
                $scope.Fine = true;

            }
            else {
                $scope.Bonus = false;
                $scope.Beta = false;
                $scope.OverTime = true;
                $scope.Fine = false;

            }
        }

        $scope.refleavetypelist = [{ 'id': 'monthly', 'name': 'Monthly' }, { 'id': 'yearly', 'name': 'Yearly' }];
        $scope.SearchRef = restService.SearchReference;
        $scope.domaintype = "";
        $scope.ShowNew = false;
        $scope.ShowLeaveType = false;
        $scope.ShowSizeGroup = false;
        $scope.ShowPersons = false;
        $scope.ShowArea = false;
        $scope.ShowProductHierarchy = false;
        $scope.ShowCustomFields = false;
        $scope.ShowProductGroup = false;
        $scope.ShowMfgMachine = false;
        $scope.ShowAttribute = false;
        $scope.ShowPaycategory = false;
        $scope.ShowDepartmentType = false;
        $scope.ShowDesignation = false;
        $scope.ShowSpecialPayment = false;
        $scope.ShowBillImport = false;
        $scope.ShowWorkingHour = false;
        $scope.ShowLocation = false;
        $scope.ShowDeliveryLocation = false;
        $scope.ShowCounter = false;
        $scope.ShowPOType = false;
        $scope.ShowFloor = false;
        $scope.ShowBank = false;
        $scope.ShowCard = false;
        $scope.ShowUPI = false;
        $scope.ShowSetting = false;
        $scope.ShowUserRole = false;
        $scope.ShowSeasons = false;
        $scope.ShowAgeSet = false;
        $scope.ShowSize = false;
        $scope.ShowSection = false;
        $scope.ShowCustomerCategory = false;
        $scope.ShowOfflineClient = false;
        $scope.ShowAdditionalCharges = false;
        $scope.ShowBOMCATEGORY = false;
        $scope.ShowJOBCHARGE = false;
        $scope.ShowSTAGE = false;
        $scope.ShowMFGSAMPLESTATUS = false;
        $scope.ShowAssetTypes = false;
        $scope.ShowVoucherTypes = false;
        $scope.ShowProductDivision = false;
        $scope.ShowPurchaseTypes = false;
        $scope.ShowProductCoverage = false;
        $scope.ShowMARKER = false;
        $scope.ShowCourierType = false;
        $scope.ShowItemPriceTag = false;
        $scope.ShowFoodCounter = false;
        $scope.ShowSupplierGroup = false;
        $scope.ShowCustomerGroup = false;
        $scope.SearchResult = [];

        $scope.ErrorStyle = "success";
        $scope.ErrorMessage = "";




        $scope.Referencelist = (JSON.parse(JSON.stringify($scope.Entity)));


        $scope.SearchCondition = {
            type: "",
            code: "",
            name: "",
            additionalinfo: ""
        }

        $scope.SwitchPanel = function () {
            $scope.ShowNew = false;
            $scope.ShowLeaveType = false;
            $scope.ShowSizeGroup = false;
            $scope.ShowPersons = false;
            $scope.ShowArea = false;
            $scope.ShowProductHierarchy = false;
            $scope.ShowCustomFields = false;
            $scope.ShowProductGroup = false;
            $scope.ShowMfgMachine = false;
            $scope.ShowAttribute = false;
            $scope.ShowPaycategory = false;
            $scope.ShowDepartmentType = false;
            $scope.ShowDesignation = false;
            $scope.ShowSpecialPayment = false;
            $scope.ShowBillImport = false;
            $scope.ShowWorkingHour = false;
            $scope.ShowLocation = false;
            $scope.ShowDeliveryLocation = false;
            $scope.ShowCounter = false;
            $scope.ShowPOType = false;
            $scope.ShowFloor = false;
            $scope.ShowBank = false;
            $scope.ShowCard = false;
            $scope.ShowUPI = false;
            $scope.ShowSetting = false;
            $scope.ShowUserRole = false;
            $scope.ShowSeasons = false;
            $scope.ShowAgeSet = false;
            $scope.ShowSize = false;
            $scope.ShowSection = false;
            $scope.ShowCustomerCategory = false;
            $scope.ShowOfflineClient = false;
            $scope.ShowAdditionalCharges = false;
            $scope.ShowBOMCATEGORY = false;
            $scope.ShowJOBCHARGE = false;
            $scope.ShowSTAGE = false;
            $scope.ShowMFGSAMPLESTATUS = false;
            $scope.ShowAssetTypes = false;
            $scope.ShowVoucherTypes = false;
            $scope.ShowProductDivision = false;
            $scope.ShowPurchaseTypes = false;
            $scope.ShowProductCoverage = false
            $scope.ShowMARKER = false;
            $scope.ShowCourierType = false;
            $scope.ShowItemPriceTag = false;
            $scope.ShowFoodCounter = false;
            $scope.ShowSupplierGroup = false;
            $scope.ShowCustomerGroup = false;
            $scope.Search();
        }
        $scope.GetDomainValues = function () {
            //$scope.domaintype = code;
            $scope.SearchCondition.type = $scope.domaintype;
            $scope.ShowNew = false;
            $scope.ShowLeaveType = false;
            $scope.ShowSizeGroup = false;
            $scope.ShowPersons = false;
            $scope.ShowArea = false;
            $scope.ShowProductHierarchy = false;
            $scope.ShowCustomFields = false;
            $scope.ShowProductGroup = false;
            $scope.ShowMfgMachine = false;
            $scope.ShowAttribute = false;
            $scope.ShowPaycategory = false;
            $scope.ShowDepartmentType = false;
            $scope.ShowDesignation = false;
            $scope.ShowSpecialPayment = false;
            $scope.ShowBillImport = false;
            $scope.ShowWorkingHour = false;
            $scope.ShowLocation = false;
            $scope.ShowDeliveryLocation = false;
            $scope.ShowCounter = false;
            $scope.ShowPOType = false;
            $scope.ShowFloor = false;
            $scope.ShowBank = false;
            $scope.ShowCard = false;
            $scope.ShowUPI = false;
            $scope.ShowSetting = false;
            $scope.ShowUserRole = false;
            $scope.ShowSeasons = false;
            $scope.ShowAgeSet = false;
            $scope.ShowSize = false;
            $scope.ShowSection = false;
            $scope.ShowCustomerCategory = false;
            $scope.ShowOfflineClient = false;
            $scope.ShowAdditionalCharges = false;
            $scope.ShowJOBCHARGE = false;
            $scope.ShowSTAGE = false;
            $scope.ShowMFGSAMPLESTATUS = false;
            $scope.ShowAssetTypes = false;
            $scope.ShowVoucherTypes = false;
            $scope.ShowProductDivision = false;
            $scope.ShowPurchaseTypes = false;
            $scope.ShowProductCoverage = false
            $scope.ShowMARKER = false;
            $scope.ShowCourierType = false;
            $scope.ShowItemPriceTag = false;
            $scope.ShowFoodCounter = false;
            $scope.ShowSupplierGroup = false;
            $scope.ShowCustomerGroup = false;
        }
        $scope.allpermission = false;
        $scope.SelectAll = function () {
            $.each($scope.PermissionList, function (k, v) {
                v.include = $scope.allpermission
            });
        }

        $scope.SalesInc = {
            from: "",
            to: "",
            incentive: ""
        }

        $scope.fileuploadedcomplete = function (value) {

            $scope.fileuploadedcomplete = null;
            if (value.rows.length < 2) {
                $rootScope.Toaster.Error("Uploaded file don't have rows.");
                return;
            }

            if (value.columns.length != 2 && ($scope.domaintype != "SWIPPINGMACINE")) {
                $rootScope.Toaster.Error("Uploaded file should have only 2 columns.");
                return;
            }
            var listofcodes = [];

            var findValue = function (list, name) {
                var value = list.find(function (item) {
                    return item.name == name;
                });
                if (value && value != null)
                    return value;
                return { 'name': name, 'index': -1 };
            };

            if ($scope.domaintype == "SWIPPINGMACINE") {



                var fields = [];
                var cfields = [];

                $.each(value.columns, function (k, v) {
                    fields.push({ 'name': v, 'index': k });
                    if (!(v == 'Location' ||
                        v == 'Bank' ||
                        v == 'Name' ||
                        v == 'Integration')) {
                        cfields.push({ 'name': v, 'index': k });
                    }
                })

                $.each(value.rows, function (k, v) {
                    if (k > 0) {
                        var row = {
                            'type': "SWIPPINGMACINE", 'isactive': true, 'additionalinfo': {
                                'payintegration': { 'list': [] }
                            }
                        };

                        if (findValue(fields, 'Name').index >= 0) {
                            row['Name'] = v.values[findValue(fields, 'Name').index];
                        }

                        if (findValue(fields, 'Location').index >= 0) {
                            var location = v.values[findValue(fields, 'Location').index];
                            var locationinfo = $scope.Locations.find(function (item) {
                                return item.code == location || item.name == location;
                            });
                            if (locationinfo && locationinfo != null)
                                row.additionalinfo.locationid = locationinfo.id;
                        }

                        if (findValue(fields, 'Bank').index >= 0) {
                            var bank = v.values[findValue(fields, 'Bank').index];
                            var bankinfo = $scope.Banks.find(function (item) {
                                return item.code == bank || item.name == bank;
                            });
                            if (bankinfo && bankinfo != null)
                                row.additionalinfo.parentid = bankinfo.id;
                        }

                        if (findValue(fields, 'Integration').index >= 0) {
                            var integration = v.values[findValue(fields, 'Integration').index];
                            var integrationinfo = $scope.PaymentIntegrationModes.find(function (item) {
                                return item.name == integration;
                            });
                            if (integrationinfo && integrationinfo != null)
                                row.additionalinfo.payintegration.type = integrationinfo.id;
                        }

                        $.each(cfields, function (kk, vv) {
                            row.additionalinfo.payintegration.list.push({ 'key': vv.name, 'value': v.values[vv.index] });
                        })

                        listofcodes.push(row);
                    }
                });


            }
            else {
                $.each(value.rows, function (k, v) {
                    if (k > 0) {
                        listofcodes.push({
                            'code': v.values[0], 'name': v.values[1], 'type': $scope.domaintype, 'isactive': true
                        });
                    }
                });
            }

            var request = {
                "Items": listofcodes,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ReferencelistService/InsertAll",
                data: srequest
            }).success(function (result) {

                if (result != null && result.length > 0) {
                    $rootScope.Toaster.Info(result.name + " Saved Successfully.!");
                    $.each(result, function (k, v) {
                        $scope.SearchResult.splice(0, 0, v);
                    });
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });
        }

        $scope.ManageArray = function (action, array, item) {
            array.splice(array.indexOf(item), 1);
        }

        $scope.AddBankOpening = function () {
            if ($scope.Referencelist.additionalinfo == undefined || $scope.Referencelist.additionalinfo == null)
                $scope.Referencelist.additionalinfo = {};

            if ($scope.Referencelist.additionalinfo.bankinfo == undefined || $scope.Referencelist.additionalinfo.bankinfo == null)
                $scope.Referencelist.additionalinfo.bankinfo = {};

            if ($scope.Referencelist.additionalinfo.bankinfo.ob == undefined || $scope.Referencelist.additionalinfo.bankinfo.ob == null)
                $scope.Referencelist.additionalinfo.bankinfo.ob = [];

            $scope.Referencelist.additionalinfo.bankinfo.ob.push({ 'date': $scope.openingdate, 'value': $scope.openingvalue });
        }

        $scope.AddBankInterest = function () {
            if ($scope.Referencelist.additionalinfo == undefined || $scope.Referencelist.additionalinfo == null)
                $scope.Referencelist.additionalinfo = {};

            if ($scope.Referencelist.additionalinfo.bankinfo == undefined || $scope.Referencelist.additionalinfo.bankinfo == null)
                $scope.Referencelist.additionalinfo.bankinfo = {};

            if ($scope.Referencelist.additionalinfo.bankinfo.ir == undefined || $scope.Referencelist.additionalinfo.bankinfo.ir == null)
                $scope.Referencelist.additionalinfo.bankinfo.ir = [];

            $scope.Referencelist.additionalinfo.bankinfo.ir.push({ 'date': $scope.iropeningdate, 'value': $scope.iropeningvalue });
        }

        $scope.AddDepartmentIncentive = function () {
            if (typeof ($scope.Referencelist.additionalinfo) == 'undefined' || $scope.Referencelist.additionalinfo == null)
                $scope.Referencelist.additionalinfo = {};

            if (typeof ($scope.Referencelist.additionalinfo.salesincentive) == 'undefined' || $scope.Referencelist.additionalinfo.salesincentive == null)
                $scope.Referencelist.additionalinfo.salesincentive = [];

            var from = parseFloat($scope.SalesInc.from);
            var step = parseFloat($scope.SalesInc.step);
            var to = parseFloat($scope.SalesInc.to);
            for (; from < to; from = from + step) {

                var incentiveitem = $scope.Referencelist.additionalinfo.salesincentive.find(function (item) {
                    return item.from == from + 1;
                });

                if (typeof (incentiveitem) == 'undefined' || incentiveitem == null) {
                    $scope.Referencelist.additionalinfo.salesincentive.push(
                        {
                            'from': from + 1,
                            'to': from + step,
                            'incentive': 0
                        });
                }
            }
        }

        $scope.AddIncentive = function () {
            if (typeof ($scope.Referencelist.additionalinfo) == 'undefined' || $scope.Referencelist.additionalinfo == null)
                $scope.Referencelist.additionalinfo = {};
            if (typeof ($scope.Referencelist.additionalinfo.salesincentive) == 'undefined' || $scope.Referencelist.additionalinfo.salesincentive == null)
                $scope.Referencelist.additionalinfo.salesincentive = [];

            var from = parseFloat($scope.SalesInc.from);
            var step = parseFloat($scope.SalesInc.step);
            var to = parseFloat($scope.SalesInc.to);



            for (; from < to; from = from + step) {

                var incentiveitem = $scope.Referencelist.additionalinfo.salesincentive.find(function (item) {
                    return item.from == from + 1;
                });

                if (typeof (incentiveitem) == 'undefined' || incentiveitem == null) {
                    $scope.Referencelist.additionalinfo.salesincentive.push(
                        {
                            'from': from + 1,
                            'to': from + step,
                            'incentive': 0
                        });
                }
            }
        }

        $scope.ManageInc = function (action, item) {
            if (action == "DeleteInc") {
                $scope.Referencelist.additionalinfo.salesincentive.splice($scope.Referencelist.additionalinfo.salesincentive.indexOf(item), 1);
            }
            else if (action == "EditInc") {
                $scope.SalesInc = item;
            }
        }


        $scope.CollectIncentiveInformation = function () {
            var request = {
                "item": $scope.Referencelist,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "AnalyserService/SectionWiseSalesIncentive",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0) {

                    /*if (typeof ($scope.Referencelist.additionalinfo.salesincentive) != 'undefined' &&
                        $scope.Referencelist.additionalinfo.salesincentive != null &&
                        $scope.Referencelist.additionalinfo.salesincentive.length > 0) {
                        $.each(result, function (item) {
                            item.newincentive = item.incentive;
                            var existingitem = $scope.Referencelist.additionalinfo.salesincentive.find(function (sitem) {
                                return sitem.from == item.from && sitem.to == item.to;
                            });
                            if (existingitem != null)
                                item.incentive = existingitem.incentive; 
                        });

                    }*/
                    $scope.Referencelist.additionalinfo.salesincentive = result;
                    $scope.Referencelist.additionalinfo.showincentiveconfig = true;
                    $scope.Referencelist.additionalinfo.calculatedincentive = 0;
                    $scope.Referencelist.additionalinfo.calculatednewincentive = 0;
                    $scope.calculatedincentive();
                }
            }).error(function (result) {
            });
        }

        $scope.calculatedincentive = function () {
            if (typeof ($scope.Referencelist.additionalinfo.showincentiveconfig) != 'undefined' &&
                $scope.Referencelist.additionalinfo.showincentiveconfig) {
                $scope.Referencelist.additionalinfo.calculatedincentive = 0;
                $scope.Referencelist.additionalinfo.calculatednewincentive = 0;
                $.each($scope.Referencelist.additionalinfo.salesincentive, function (index, item) {
                    $scope.Referencelist.additionalinfo.calculatedincentive += (parseFloat(item.incentive) * item.qty);
                    $scope.Referencelist.additionalinfo.calculatednewincentive += (item.newincentive * item.qty);
                });
            }
        }

        $scope.Save = function () {

            if ($scope.domaintype == "USERROLE") {
                $scope.Referencelist.additionalinfo.permissionset = [];
                $.each($scope.PermissionList, function (k, v) {
                    if (v.include != 'undefined' && v.include == true) {
                        $scope.Referencelist.additionalinfo.permissionset.push({
                            'code': v.code,
                            'include': v.include
                        });
                    }
                });
            }

            if ($scope.domaintype == "PRODUCTHIERARCHY" &&
                $scope.Referencelist.referenceid &&
                $scope.Referencelist.referenceid > 0) {
                $scope.HierarchyChanged(true);
            }

            if ($scope.domaintype == "PRODUCTGROUPS" &&
                $scope.Referencelist.referenceid &&
                $scope.Referencelist.referenceid > 0) {
                $scope.UpdateProductHierarchy();
            }

            if ($scope.domaintype == null || $scope.domaintype == 0) {
                $rootScope.Toaster.Error("Please Select Type");
                return;
            }

            if ($scope.Referencelist.code == null || $scope.Referencelist.code == '') {
                $scope['setFocuscode']();
                $rootScope.Toaster.Error("Please fill out code ");
                return;
            }
            if ($scope.Referencelist.name == null || $scope.Referencelist.name == '') {
                $scope['setFocusname']();
                $rootScope.Toaster.Error("Please fill out name");
                return;
            }

            request = {
                "item": $scope.Referencelist,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ReferencelistService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0 &&
                    (result.ErrorMessage == 'undefined' || result.ErrorMessage == null || result.ErrorMessage == '')) {
                    $rootScope.Toaster.Info(result.name + " Saved Successfully.!");

                    $scope.AddNew();
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });
        }

        $scope.ConvertLocationToVendors = function () {

            request = {
                "item": $scope.Referencelist,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ReferencelistService/ConvertLocationToVendors",
                data: srequest
            }).success(function (result) {
                if (result != null && result.id > 0 &&
                    (result.ExceptionMessage == 'undefined' || result.ExceptionMessage == null || result.ExceptionMessage == '')) {
                    $rootScope.Toaster.Info(result.name + " Saved Successfully.!");
                    $scope.Referencelist.version = result.version;
                    $scope.Referencelist.additionalinfo.supplierid = result.additionalinfo.supplierid;
                    $scope.Referencelist.additionalinfo.customerid = result.additionalinfo.customerid;
                }
                else {
                    $rootScope.Toaster.Error(result.ExceptionMessage);
                }
            }).error(function (result) {
            });
        }


        $scope.SyncSizeGroup = function () {
            $scope.Referencelist.additionalinfo.sizegroups = $scope.Referencelist.additionalinfo.sizegroups || [];
            $scope.Referencelist.additionalinfo.mfgconfig.SizeRatioList = $scope.Referencelist.additionalinfo.mfgconfig.SizeRatioList || [];

            $.each($scope.Referencelist.additionalinfo.sizegroups, function (k, v) {
                var sizegroupsize = $scope.Referencelist.additionalinfo.mfgconfig.SizeRatioList.find(function (item) {
                    return item.sizeid == v;
                });
                if (sizegroupsize == undefined || sizegroupsize == null) {
                    var size = $scope.SizeList.find(function (item) {
                        return item.id == v;
                    });
                    if (size && size != null)
                        $scope.Referencelist.additionalinfo.mfgconfig.SizeRatioList.push({ 'sizeid': size.id, 'name': size.name, 'index': '' });
                }
            })
        }

        $scope.CreateSizeRatioGroup = function (value) {
            $scope.Referencelist.additionalinfo.sizegroups = $scope.Referencelist.additionalinfo.sizegroups || [];
            $scope.Referencelist.additionalinfo.mfgconfig.SizeRatioList = $scope.Referencelist.additionalinfo.mfgconfig.SizeRatioList || [];

            var allsizelist = value.split(',');
            $.each(allsizelist, function (k, v) {
                var size = $scope.SizeList.find(function (item) {
                    return item.name == v;
                });
                if (size && size != null) {

                    var exist = $scope.Referencelist.additionalinfo.mfgconfig.SizeRatioList.find(function (sitem) {
                        return sitem.sizeid == size.id;
                    })
                    if (exist == undefined || exist == null) {
                        $scope.Referencelist.additionalinfo.mfgconfig.SizeRatioList.push({ 'sizeid': size.id, 'name': size.name, 'index': 0 });
                        $scope.Referencelist.additionalinfo.sizegroups.push(size.id);
                    }
                }
            });
        }

        $scope.ManageSizeRatioGroup = function (command, value) {
            if (command == 'ADD') {
                $scope.Referencelist.additionalinfo.mfgconfig.SizeRatioList = $scope.Referencelist.additionalinfo.mfgconfig.SizeRatioList || [];
                $scope.Referencelist.additionalinfo.sizegroups = $scope.Referencelist.additionalinfo.sizegroups || [];
                var size = $scope.SizeList.find(function (item) {
                    return item.id == $scope.selectedsizeid;
                });
                $scope.Referencelist.additionalinfo.mfgconfig.SizeRatioList.push({ 'sizeid': size.id, 'name': size.name, 'index': $scope.selectedsizeindex });
                $scope.Referencelist.additionalinfo.sizegroups.push(size.id);
                $scope.selectedsizeid = '';
            }
            else if (command == 'DELETE') {
                $scope.Referencelist.additionalinfo.mfgconfig.SizeRatioList.splice($scope.Referencelist.additionalinfo.mfgconfig.SizeRatioList.indexOf(value), 1);
                $scope.Referencelist.additionalinfo.sizegroups.splice($scope.Referencelist.additionalinfo.sizegroups.indexOf(value.id), 1);
            }
        }

        $scope.CreateSizeGroup = function (value) {
            var allsizelist = value.split(',');
            $.each(allsizelist, function (k, v) {
                var size = $scope.SizeList.find(function (item) {
                    return item.name == v;
                });

                if (size && size != null) {
                    $scope.Referencelist.additionalinfo.sizegroups = $scope.Referencelist.additionalinfo.sizegroups || [];
                    $scope.Referencelist.additionalinfo.sizegroups.push(size.id);
                }
            });
        }

        $scope.ManageSizeGroup = function (command, value) {
            if (command == 'ADD') {
                $scope.Referencelist.additionalinfo.sizegroups = $scope.Referencelist.additionalinfo.sizegroups || [];
                $scope.Referencelist.additionalinfo.sizegroups.push($scope.selectedsizeid);
                $scope.selectedsizeid = '';
            }
            else if (command == 'DELETE') {
                $scope.Referencelist.additionalinfo.sizegroups.splice($scope.Referencelist.additionalinfo.sizegroups.indexOf(value), 1);
            }
        }

        $scope.ManageIPAddres = function (command, value) {
            if (command == 'ADD') {
                if ($scope.StaticIP && $scope.StaticIP != null && $scope.StaticIP != '') {
                    if (!$scope.Referencelist.additionalinfo.staticip || $scope.Referencelist.additionalinfo.staticip == null)
                        $scope.Referencelist.additionalinfo.staticip = [];

                    $scope.Referencelist.staticipchanged = true;
                    $scope.Referencelist.additionalinfo.staticip.push({ 'ip': $scope.StaticIP });
                    $scope.StaticIP = '';
                }
            }
            else if (command == 'DELETE') {
                $scope.Referencelist.staticipchanged = true;
                $scope.Referencelist.additionalinfo.staticip.splice($scope.Referencelist.additionalinfo.staticip.indexOf(value), 1);
            }
        }

        $scope.ApplyIncentive2Products = function (productwise) {

            if ($scope.Referencelist.locationid == null || $scope.Referencelist.locationid == 0) {
                $rootScope.Toaster.Error("Please fill out location id");
                return;
            }

            if ($scope.Referencelist.id == null || $scope.Referencelist.id == 0) {
                $rootScope.Toaster.Error("Please save the section before apply to product level");
                return;
            }

            $scope.Referencelist.productwise = productwise;
            $scope.Referencelist.additionalinfo.fixedincentive = $scope.Referencelist.incentivepercentage;
            var request = {
                "item": $scope.Referencelist,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ReferencelistService/SaveIncentive",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info(result.name + "Saved Successfully.!");
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });
        }

        $scope.PreapreCustomField = function () {
            request = {
                "Item": $scope.Referencelist,
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ReferencelistService/PreapreCustomField",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Generated Successfully.!");

            }).error(function (result) {

            });
        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {


                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.name + "?",
                    confirmButton: 'Yes',
                    confirmButtonClass: 'btn-info',
                    icon: 'fa fa-question-circle',
                    animation: 'scale',
                    animationClose: 'top',
                    opacity: 0.5,
                    buttons: {
                        formSubmit: {
                            text: 'Submit',
                            btnClass: 'btn-blue',
                            action: function () {
                                request = {
                                    "Item": item,
                                    "SecurityToken": "String content"
                                }
                                var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: "ReferencelistService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                        if (item.selected) $scope.selectedcount--;
                                    }

                                }).error(function (result) {

                                });
                            }
                        },
                        cancel: function () {
                            //close
                        },
                    },

                    onContentReady: function () {
                        // bind to events
                        var jc = this;
                        jc.$$formSubmit.trigger('focus');
                        this.$content.find('form').on('submit', function (e) {
                            // if the user submits the form by pressing enter in the field.
                            e.preventDefault();
                            jc.$$formSubmit.trigger('click'); // reference the button and click it
                        });
                    }
                });


            }
            else if (action == "Edit") {
                $scope.Referencelist = item;
                if (typeof (item.additionalinfo) == 'undefined' || item.additionalinfo == null)
                    item.additionalinfo = (JSON.parse(JSON.stringify($scope.Entity.additionalinfo)));

                if ($scope.domaintype == "PAYCATEGORY")
                    $scope.ShowPaycategory = true;
                else if ($scope.domaintype == "DEPARTMENT")
                    $scope.ShowDepartmentType = true;
                else if ($scope.domaintype == "DESIGNATION")
                    $scope.ShowDesignation = true;
                else if ($scope.domaintype == "WORKINGHOURS")
                    $scope.ShowWorkingHour = true;
                else if ($scope.domaintype == "OFFLINECLIENT")
                    $scope.ShowOfflineClient = true;
                else if ($scope.domaintype == "LEAVETYPE")
                    $scope.ShowLeaveType = true;
                else if ($scope.domaintype == "SIZEGROUP")
                    $scope.ShowSizeGroup = true;
                else if ($scope.domaintype == "PERSONS")
                    $scope.ShowPersons = true;
                else if ($scope.domaintype == "AREA")
                    $scope.ShowArea = true;
                else if ($scope.domaintype == "PRODUCTHIERARCHY") {
                    $scope.ShowProductHierarchy = true;
                    $scope.HierarchyChanged();
                }
                else if ($scope.domaintype == "CUSTOMFIELDS") {
                    $scope.ShowCustomFields = true;
                    $scope.HierarchyChanged();
                }
                else if ($scope.domaintype == "PRODUCTGROUPS")
                    $scope.ShowProductGroup = true;
                else if ($scope.domaintype == "MFGPRDMACHINE")
                    $scope.ShowMfgMachine = true;
                else if ($scope.domaintype == "MATERIAL")
                    $scope.ShowAttribute = true;
                else if ($scope.domaintype == "LOCATION")
                    $scope.ShowLocation = true;
                else if ($scope.domaintype == "DELIVERYLOCATION")
                    $scope.ShowDeliveryLocation = true;
                else if ($scope.domaintype == "COUNTER")
                    $scope.ShowCounter = true;
                else if ($scope.domaintype == "POTYPE")
                    $scope.ShowPOType = true;
                else if ($scope.domaintype == "FLOOR")
                    $scope.ShowFloor = true;
                else if ($scope.domaintype == "BANK")
                    $scope.ShowBank = true;
                else if ($scope.domaintype == "SWIPPINGMACINE")
                    $scope.ShowCard = true;
                else if ($scope.domaintype == "UPIPROVIDER")
                    $scope.ShowUPI = true;
                else if ($scope.domaintype == "SETTINGS")
                    $scope.ShowSetting = true;
                else if ($scope.domaintype == "SEASONS")
                    $scope.ShowSeasons = true;
                else if ($scope.domaintype == "AGESET")
                    $scope.ShowAgeSet = true;
                else if ($scope.domaintype == "SIZE")
                    $scope.ShowSize = true;
                else if ($scope.domaintype == "DIVISION") {
                    $scope.ShowSection = true;
                    if ($scope.Referencelist.additionalinfo.isonline) {
                        $timeout(function () {
                            $scope.ShowSectionCosting($scope.Referencelist.additionalinfo.isonline);
                        }, 50);
                    }
                    else {
                        $scope.ShowSectionCosting(false);
                    }
                }
                else if ($scope.domaintype == "JOBCHARGE" || $scope.domaintype == "TAILORINGCHARGE") {
                    $scope.ShowJOBCHARGE = true;
                    $scope.Showadditionalchargeamount($scope.Referencelist.additionalinfo.mfgconfig.additionalchargeid,
                        $scope.Additionalcharges);
                }
                else if ($scope.domaintype == "DIVISION") {
                    $scope.ShowSection = true;
                    if ($scope.Referencelist.additionalinfo.isonline) {
                        $timeout(function () {
                            $scope.ShowSectionCosting($scope.Referencelist.additionalinfo.isonline);
                        }, 50);
                    }
                    else {
                        $scope.ShowSectionCosting(false);
                    }
                }
                else if ($scope.domaintype == "BOMCATEGORY")
                    $scope.ShowBOMCATEGORY = true;
                else if ($scope.domaintype == "USERROLE") {
                    $scope.ShowUserRole = true;
                    if ($scope.Referencelist.additionalinfo.permissionset != null && $scope.Referencelist.additionalinfo.permissionset != 'undefined') {
                        $.each($scope.Referencelist.additionalinfo.permissionset, function (k, v) {
                            var sc = $scope.PermissionList.find(function (item) {
                                return item.code == v.code;
                            });

                            if (sc != null) {
                                sc.include = true;
                            }
                        });
                    }

                }
                else if ($scope.domaintype == "SPECIALPAYMENT") {
                    $scope.ShowSpecialPayment = true;

                    $scope.ChangeReport()
                }
                else if ($scope.domaintype == "ADDITIONALCHARGES") {
                    $scope.ShowAdditionalCharges = true;
                }
                else if ($scope.domaintype == "BILLIMPORT") {
                    $scope.ShowBillImport = true;
                }
                else if ($scope.domaintype == "ASSETTYPE") {
                    $scope.ShowAssetTypes = true;
                }
                else if ($scope.domaintype == "VOUCHERTYPE") {
                    $scope.ShowVoucherTypes = true;
                }
                else if ($scope.domaintype == "PRODUCTDIVISION") {
                    $scope.ShowProductDivision = true;
                }
                else if ($scope.domaintype == "PURCHASETYPE") {
                    $scope.ShowPurchaseTypes = true;
                }
                else if ($scope.domaintype == "PRODUCTCOVERAGE") {
                    $scope.ShowProductCoverage = true;
                }
                else if ($scope.domaintype == "MFGSTAGES") {
                    $scope.ShowSTAGE = true;
                }
                else if ($scope.domaintype == "MFGSAMPLESTATUS") {
                    $scope.ShowMFGSAMPLESTATUS = true;
                }
                else if ($scope.domaintype == "MARKER") {
                    $scope.ShowMARKER = true;
                }
                else if ($scope.domaintype == "PRICETAGS") {
                    $scope.ShowItemPriceTag = true;
                }
                else if ($scope.domaintype == "FOODCOUNTER") {
                    $scope.ShowFoodCounter = true;
                }
                else if ($scope.domaintype == "SUPPLIERGROUP") {
                    $scope.ShowSupplierGroup = true;
                    $scope.LoadConfiguration();
                }
                else if ($scope.domaintype == "CUSTOMERGROUP") {
                    $scope.ShowCustomerGroup = true;
                    $scope.LoadConfiguration();
                }
                else if ($scope.domaintype == "COURIERTYPE") {
                    $scope.ShowCourierType = true;
                }
                else if ($scope.domaintype == "CUSTOMERCATEGORY") {
                    $scope.ShowCustomerCategory = true;
                    if (typeof ($scope.Referencelist.additionalinfo.billdiscountconfig) == 'undefined' || $scope.Referencelist.additionalinfo.billdiscountconfig == null)
                        $scope.Referencelist.additionalinfo.billdiscountconfig = [];

                    if ($scope.Referencelist.additionalinfo.ccmarginmode &&
                        $scope.Referencelist.additionalinfo.ccmargin > 0)
                        $scope.Referencelist.additionalinfo.marginmode = 'buyingprice';
                    else if ($scope.Referencelist.additionalinfo.ccmarginmode &&
                        $scope.Referencelist.additionalinfo.ccmarginmrp > 0)
                        $scope.Referencelist.additionalinfo.marginmode = 'mrp';
                    else if ($scope.Referencelist.additionalinfo.ccmarginmode &&
                        $scope.Referencelist.additionalinfo.ccmarginprice > 0)
                        $scope.Referencelist.additionalinfo.marginmode = 'price';

                    $.each($scope.RetailLocations, function (k, v) {

                        var billdiscount = $scope.Referencelist.additionalinfo.billdiscountconfig.find(function (item) {
                            return item.id == v.id;
                        });

                        if (typeof (billdiscount) == 'undefined' || billdiscount == null) {
                            $scope.Referencelist.additionalinfo.billdiscountconfig.push({
                                'id': v.id, 'name': v.name, 'billdiscount': 0, 'billdiscountinclude': false, 'rewardpoint': 0, 'upgrade': false
                            })
                        }
                        else {
                            billdiscount.name = v.name;
                        }
                    });
                }
                else {
                    $scope.ShowNew = true;
                }
            }
        }

        $scope.AddNew = function () {
            $scope.Referencelist = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.Referencelist.type = $scope.domaintype;
            $.each($scope.PermissionList, function (k, v) {
                v.include = false
            });
            if ($scope.domaintype == "PAYCATEGORY")
                $scope.ShowPaycategory = true;
            else if ($scope.domaintype == "DEPARTMENT")
                $scope.ShowDepartmentType = true;
            else if ($scope.domaintype == "DESIGNATION")
                $scope.ShowDesignation = true;
            else if ($scope.domaintype == "WORKINGHOURS")
                $scope.ShowWorkingHour = true;
            else if ($scope.domaintype == "OFFLINECLIENT")
                $scope.ShowOfflineClient = true;
            else if ($scope.domaintype == "SIZEGROUP")
                $scope.ShowSizeGroup = true;
            else if ($scope.domaintype == "PERSONS")
                $scope.ShowPersons = true;
            else if ($scope.domaintype == "AREA")
                $scope.ShowArea = true;
            else if ($scope.domaintype == "CUSTOMFIELDS")
                $scope.ShowCustomFields = true;
            else if ($scope.domaintype == "PRODUCTHIERARCHY")
                $scope.ShowProductHierarchy = true;
            else if ($scope.domaintype == "PRODUCTGROUPS")
                $scope.ShowProductGroup = true;
            else if ($scope.domaintype == "MFGPRDMACHINE")
                $scope.ShowMfgMachine = true;
            else if ($scope.domaintype == "MATERIAL")
                $scope.ShowAttribute = true;
            else if ($scope.domaintype == "LEAVETYPE")
                $scope.ShowLeaveType = true;
            else if ($scope.domaintype == "SPECIALPAYMENT")
                $scope.ShowSpecialPayment = true;
            else if ($scope.domaintype == "BILLIMPORT")
                $scope.ShowBillImport = true;
            else if ($scope.domaintype == "LOCATION")
                $scope.ShowLocation = true;
            else if ($scope.domaintype == "DELIVERYLOCATION")
                $scope.ShowDeliveryLocation = true;
            else if ($scope.domaintype == "COUNTER")
                $scope.ShowCounter = true;
            else if ($scope.domaintype == "POTYPE")
                $scope.ShowPOType = true;
            else if ($scope.domaintype == "FLOOR")
                $scope.ShowFloor = true;
            else if ($scope.domaintype == "USERROLE")
                $scope.ShowUserRole = true;
            else if ($scope.domaintype == "SETTINGS")
                $scope.ShowSetting = true;
            else if ($scope.domaintype == "SEASONS")
                $scope.ShowSeasons = true;
            else if ($scope.domaintype == "AGESET")
                $scope.ShowAgeSet = true;
            else if ($scope.domaintype == "SIZE")
                $scope.ShowSize = true;
            else if ($scope.domaintype == "DIVISION") {
                $scope.ShowSection = true;
                $scope.ShowSectionCosting(false);
            }
            else if ($scope.domaintype == "BANK")
                $scope.ShowBank = true;
            else if ($scope.domaintype == "SWIPPINGMACINE")
                $scope.ShowCard = true;
            else if ($scope.domaintype == "UPIPROVIDER")
                $scope.ShowUPI = true;
            else if ($scope.domaintype == "ADDITIONALCHARGES")
                $scope.ShowAdditionalCharges = true;
            else if ($scope.domaintype == "BOMCATEGORY")
                $scope.ShowBOMCATEGORY = true;
            else if ($scope.domaintype == "ASSETTYPE")
                $scope.ShowAssetTypes = true;
            else if ($scope.domaintype == "VOUCHERTYPE")
                $scope.ShowVoucherTypes = true;
            else if ($scope.domaintype == "PRODUCTDIVISION")
                $scope.ShowProductDivision = true;
            else if ($scope.domaintype == "PURCHASETYPE")
                $scope.ShowPurchaseTypes = true;
            else if ($scope.domaintype == "PRODUCTCOVERAGE")
                $scope.ShowProductCoverage = true;
            else if ($scope.domaintype == "JOBCHARGE" || $scope.domaintype == "TAILORINGCHARGE")
                $scope.ShowJOBCHARGE = true;
            else if ($scope.domaintype == "MFGSTAGES")
                $scope.ShowSTAGE = true;
            else if ($scope.domaintype == "MFGSAMPLESTATUS")
                $scope.ShowMFGSAMPLESTATUS = true;
            else if ($scope.domaintype == "MARKER" || $scope.domaintype == "MARKER")
                $scope.ShowMARKER = true;
            else if ($scope.domaintype == "COURIERTYPE")
                $scope.ShowCourierType = true;
            else if ($scope.domaintype == "SUPPLIERGROUP") {
                $scope.ShowSupplierGroup = true;
                $scope.LoadConfiguration();
            }
            else if ($scope.domaintype == "CUSTOMERGROUP") {
                $scope.ShowCustomerGroup = true;
                $scope.LoadConfiguration();
            }
            else if ($scope.domaintype == "PRICETAGS")
                $scope.ShowItemPriceTag = true;
            else if ($scope.domaintype == "FOODCOUNTER")
                $scope.ShowFoodCounter = true;
            else if ($scope.domaintype == "CUSTOMERCATEGORY") {
                $scope.ShowCustomerCategory = true;
                $scope.Referencelist.additionalinfo.billdiscountconfig = [];
                $.each($scope.RetailLocations, function (k, v) {
                    $scope.Referencelist.additionalinfo.billdiscountconfig.push({
                        'id': v.id, 'name': v.name, 'billdiscount': 0, 'billdiscountinclude': false, 'rewardpoint': 0, 'upgrade': false
                    })
                });
            }
            else
                $scope.ShowNew = true;
        }

        $scope.SupplierGroupTreeLoaded = false;
        $scope.LoadConfiguration = function () {
            if ($scope.SupplierGroupTreeLoaded) return;
            $scope.SupplierGroupTreeLoaded = true;

            $scope.allmenuOptions = [
                ['Create', function ($itemScope) {
                    $scope.OnContextMenuSelected($itemScope, 'Create', 'room');
                }],
                ['Delete', function ($itemScope) {
                    $scope.OnContextMenuSelected($itemScope, 'Delete', 'delete');
                }],
                ['Edit', function ($itemScope) {
                    $scope.OnContextMenuSelected($itemScope, 'Edit', 'edit');
                }]
            ];

            $scope.SetParentSelection = function (treedata, levels, level) {
                if (typeof (levels) == 'undefined' || levels == null) return treedata[0];
                var selectednode = null;
                for (var i = 0; i < treedata.length; i++) {
                    var v = treedata[i];
                    if (v.name == levels[level]) {
                        v.collapsed = false;
                        selectednode = v;
                        if (v.children && v.children != null && v.children.length > 0) {
                            selectednode = $scope.SetParentSelection(v.children, levels, level + 1);
                        }
                    }
                    else {
                        v.collapsed = true;
                    }
                }
                return selectednode;
            }

            $scope.SaveLedgerHierarchy = function () {

                var r = $scope.bintreedata[0];
                var request = {
                    "name": r.name, "id": r.id,
                    "typeid": r.typeid,
                    "stylecollapsed": 'location_collapsed',
                    "styleexpanded": 'location_expanded',
                    "stylenormal": 'location_normal',
                    "selected": r.selected,
                    "supportchild": true,
                    "supportallocation": false,
                    "modified": false,
                    "level": 0,
                    "isactive": true,
                    "version": r.version,
                    "nodetype": "location",
                    "children": $scope.GetChildrens(r.children, 1)
                };

                var srequest = JSON.stringify({ 'Item': request });

                $http({
                    method: 'POST',
                    url: "ReferencelistService/SaveLedgerHierarchy",
                    data: srequest
                }).success(function (result) {
                    r.id = result.id;
                });
            }

            $scope.GetChildrens = function (childrens, level) {
                if (typeof (childrens) == 'undefined' || childrens == null) return null;
                var nchildrens = [];
                $.each(childrens, function (k, v) {
                    var data = {
                        "name": v.name,
                        "stylecollapsed": 'location_collapsed',
                        "styleexpanded": 'location_expanded',
                        "stylenormal": 'location_normal',
                        "selected": v.selected,
                        "supportchild": true,
                        "supportallocation": false,
                        "level": level,
                        "nodetype": v.nodetype,
                        "children": $scope.GetChildrens(v.children, level + 1)
                    }
                    nchildrens.push(data);
                });
                return nchildrens;
            }

            $scope.UpdateParent = function () {
                var currentnode = $scope.CurrentNode;
                var level = 0;
                var levels = [currentnode.name];
                while (currentnode.parentnode && currentnode.parentnode != null) {
                    currentnode = currentnode.parentnode;
                    level++;
                    levels.splice(0, 0, currentnode.name);
                }
                if ($scope.Referencelist.additionalinfo == undefined ||
                    $scope.Referencelist.additionalinfo == null) {
                    $scope.Referencelist.additionalinfo = {};
                }
                $scope.Referencelist.additionalinfo.tallyledger = levels.join('/');
            }

            $scope.AssignParent = function (node, parentnode) {
                node.parentnode = parentnode;
                if (typeof (node.children) != 'undefined') {
                    $.each(node.children, function (k, v) {
                        $scope.AssignParent(v, node);
                    })
                }
            }

            $scope.RemoveParent = function (node) {
                node.parentnode = null;
                if (typeof (node.children) != 'undefined') {
                    $.each(node.children, function (k, v) {
                        $scope.RemoveParent(v)
                    })
                }
            }

            $scope.CurrentNode = { 'binlist': [], supportallocation: false, selected: "" };
            $scope.Clipboard = {};
            $scope['bintree'] = $scope['bintree'] || {};
            $scope['bintree'].onCurrentNodeChange = function (node) {
                if ($scope.CurrentNode != null && $scope.CurrentNode.selected)
                    $scope.CurrentNode.selected = "";
                $scope.CurrentNode = node;
                $scope.CurrentNode.selected = "selected";
                $scope.UpdateParent();
            }

            restService.Get('ReferencelistService/LEDGERHIERARCHY/12', true).then(function (results) {
                var currentNode = null;
                if (results.length == 0) {
                    var name = 'Sundry Creditors';
                    $scope.bintreedata =
                        [
                            {
                                "name": name, "id": 0,
                                "typeid": 12,
                                "referencelocationid": 12,
                                "stylecollapsed": 'location_collapsed',
                                "styleexpanded": 'location_expanded',
                                "stylenormal": 'location_normal',
                                "selected": "selected",
                                "supportchild": true,
                                "supportallocation": false,
                                "modified": false,
                                "level": 0,
                                "nodetype": "location",
                                "children": [],
                                "isactive": true,
                                "version": 1
                            }
                        ];

                    currentNode = $scope.bintreedata[0];
                }
                else {
                    var name = 'Sundry Creditors';
                    $scope.bintreedata =
                        [
                            {
                                "name": name, "id": results[0].id,
                                "typeid": 12,
                                "referencelocationid": 12,
                                "stylecollapsed": 'location_collapsed',
                                "styleexpanded": 'location_expanded',
                                "stylenormal": 'location_normal',
                                "selected": "",
                                "supportchild": true,
                                "supportallocation": false,
                                "modified": false,
                                "level": 0,
                                "isactive": true,
                                "version": results[0].version,
                                "nodetype": "location",
                                "children": results[0].children
                            }
                        ];

                    if ($scope.Referencelist.additionalinfo &&
                        $scope.Referencelist.additionalinfo.tallyledger &&
                        $scope.Referencelist.additionalinfo.tallyledger != null &&
                        $scope.Referencelist.additionalinfo.tallyledger != '') {
                        currentNode = $scope.SetParentSelection($scope.bintreedata, $scope.Referencelist.additionalinfo.tallyledger.split('/'), 0);
                    }
                    else {
                        currentNode = $scope.bintreedata[0];
                    }
                }

                $scope.AssignParent($scope.bintreedata[0], null);
                //if ($scope.CurrentNode != null && $scope.CurrentNode.selected)
                //    $scope.CurrentNode.selected = "";
                $timeout(function () {
                    if (currentNode && currentNode != null) {
                        $scope['bintree'].currentNode = currentNode;
                        $scope['bintree'].currentNode.selected = "selected";
                        $scope.CurrentNode = $scope['bintree'].currentNode;
                    }
                    $scope.UpdateParent();
                }, 30);
            });
        }

        $scope.DeleteSelected = function () {
            var ids = [];
            var items = [];
            $.each(this.igrid.GetSelectedRows(), function (k, v) {
                ids.push(v.id);
                items.push(v);
            });
            if (ids.length == 0) return;

            var request = {
                'Item': { 'ids': ids, 'type': $scope.domaintype }
            };
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ReferencelistService/DeleteByID",
                data: srequest
            }).success(function (result) {
                $.each(items, function (k, item) {
                    $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                })
                $scope.CalculatedSelected();
            }).error(function (result) {
            });

        }

        $scope.CalculatedSelected = function () {
            $scope.igrid.UpdateData($scope.SearchResult);
        }

        $scope.Showadditionalchargeamount = function (additionalchargeid, Additionalcharges) {
            if (additionalchargeid && additionalchargeid != null) {
                var addcharge = Additionalcharges.find(function (item) {
                    return item.id == additionalchargeid;
                });
                if (addcharge && addcharge != null) {
                    $scope.Referencelist.additionalinfo.mfgconfig.additionalchargeamount = addcharge.additionalinfo.billdiscount;
                }
                else {
                    $scope.Referencelist.additionalinfo.mfgconfig.additionalchargeamount = 0;
                }
            }
        }

        $scope.Search = function () {

            if (typeof ($scope.domaintype) == 'undefined' || $scope.domaintype == null || $scope.domaintype == '') {
                $rootScope.Toaster.Error("Please select the type name");
                return;
            }

            $scope.SearchCondition.searchall = true;
            request = {
                "Item": $scope.SearchCondition,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ReferencelistService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.ConfigGrid();
                $scope.selectedcount = 0;
            }).error(function (result) {
            });

        }
        $("#timepicker1").timepicker({
            showInputs: false
        });
        $("#timepicker2").timepicker({
            showInputs: false
        });

        $timeout(function () {

            if (typeof ($location.search().id) != 'undefined' &&
                $location.search().id.length > 0) {
                request = {
                    "Item": { 'id': $location.search().id },
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "ReferencelistService/Search",
                    data: srequest
                }).success(function (result) {
                    if (angular.isArray(result)) {
                        $scope.domaintype = result[0].type;
                        $scope.ManageSearchItem('Edit', result[0]);
                    }

                }).error(function (result) {
                });
            }


        }, 10);

        $scope.ConfigGrid = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 30);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('code', 'Code', 250);
            igrid.AddSimpleColumn('name', 'Name', 400);
            if ($scope.domaintype == 'PAYCATEGORY') {
                igrid.AddCustomColumn('formula', 'Formula', 60, function (item) {
                    if (item == null || typeof (item) == 'undefined' || item.additionalinfo == null ||
                        typeof (item.additionalinfo) == 'undefined' || typeof (item.additionalinfo.formula) == 'undefined')
                        return '';
                    return item.additionalinfo.formula;
                });
                igrid.AddCustomColumn('deductsalary', 'Ded.Salary', 60, function (item) {
                    if (item == null || typeof (item) == 'undefined' || item.additionalinfo == null ||
                        typeof (item.additionalinfo) == 'undefined' || typeof (item.additionalinfo.deductsalary) == 'undefined')
                        return '';
                    return item.additionalinfo.deductsalary ? 'Yes' : 'No';
                });
                igrid.AddCustomColumn('includeinesi', 'Inc.ESI', 60, function (item) {
                    if (item == null || typeof (item) == 'undefined' || item.additionalinfo == null ||
                        typeof (item.additionalinfo) == 'undefined' || typeof (item.additionalinfo.includeinesi) == 'undefined')
                        return '';
                    return item.additionalinfo.includeinesi ? 'Yes' : 'No';
                });
                igrid.AddCustomColumn('includeinpf', 'Inc.PF', 60, function (item) {
                    if (item == null || typeof (item) == 'undefined' || item.additionalinfo == null ||
                        typeof (item.additionalinfo) == 'undefined' || typeof (item.additionalinfo.includeinpf) == 'undefined')
                        return '';
                    return item.additionalinfo.includeinpf ? 'Yes' : 'No';
                });
            }
            if ($scope.domaintype == 'COUNTER') {
                igrid.AddCustomColumn('locationid', 'Location', 80, function (item) {
                    if (item == null || typeof (item) == 'undefined' ||
                        item.additionalinfo == null || typeof (item.additionalinfo) == 'undefined' ||
                        typeof (item.additionalinfo.locationid) == 'undefined')
                        return '';
                    return $scope.SearchRef(item.additionalinfo.locationid, $scope.Locations);
                });
            }
            if ($scope.domaintype == 'LOCATION') {
                igrid.AddCustomColumn('iswarehouse', 'IsWarehouse', 100, function (item) {
                    if (item == null || typeof (item) == 'undefined' ||
                        item.additionalinfo == null || typeof (item.additionalinfo) == 'undefined' ||
                        typeof (item.additionalinfo.iswarehouse) == 'undefined')
                        return '';
                    return item.additionalinfo.iswarehouse ? 'Yes' : 'No';
                });
            }

            if ($scope.domaintype == 'LOCATION' || $scope.domaintype == 'COUNTER') {
                igrid.AddCustomColumn('autosettlement', 'Autosettlement', 80, function (item) {
                    if (item == null || typeof (item) == 'undefined' ||
                        item.additionalinfo == null || typeof (item.additionalinfo) == 'undefined' ||
                        typeof (item.additionalinfo.autosettlement) == 'undefined')
                        return '';
                    return item.additionalinfo.autosettlement ? 'Yes' : 'No';
                });
            }

            if ($scope.domaintype == 'LOCATION') {
                igrid.AddCustomColumn('address', 'Address', 120, function (item) {
                    if (item == null || typeof (item) == 'undefined' ||
                        item.additionalinfo == null || typeof (item.additionalinfo) == 'undefined' ||
                        typeof (item.additionalinfo.address) == 'undefined')
                        return '';
                    return item.additionalinfo.address;
                });
            }

            igrid.AddCustomColumn('isactive', 'Active', 50, function (item) {
                if (item == null || typeof (item) == 'undefined')
                    return '';
                return item.isactive ? 'Yes' : 'No';
            });

            var commands = "";
            if ($scope.UserInfo.p_REFERENCEMASTER.modify)
                commands = "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"

            if ($scope.UserInfo.p_REFERENCEMASTER.delete)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Delete'/> &nbsp;";

            if (commands != "") {
                igrid.AddButtonColumn('action', 'ACTION', 125, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                    });
                });
            }

            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }

        $scope.MergeSelected = function () {
            var dialog = ngDialog.open({
                template: 'ChooseMerge',
                data: {
                    'rows': $scope.igrid.GetSelectedRows(),
                    'scope': $scope
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.SelectedRows = (JSON.parse(JSON.stringify($scope.ngDialogData.rows)));
                    $scope.SelectedItem = null;
                    $timeout(function () {
                        $scope.id = 0;
                    }, 10);
                    $scope.rowselected = function () {
                        $scope.SelectedRows = (JSON.parse(JSON.stringify($scope.ngDialogData.rows)));
                        if (typeof ($scope.id) != 'undefined' && $scope.id > 0) {
                            var selected = $scope.SelectedRows.find(function (item) {
                                return item.id == $scope.id;
                            });
                            $scope.SelectedItem = selected;
                        }
                    }
                    $scope.Selected = function () {
                        $scope.closeThisDialog({ 'selectedid': $scope.id });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {
                        request = {
                            "item": {
                                'id': data.value.selectedid,
                                'deletelist': $scope.igrid.GetSelectedRows()
                            },
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "ReferencelistService/Merge",
                            data: srequest
                        }).success(function (result) {
                            if (result) {
                                var rows = $scope.igrid.GetSelectedRows();
                                $.each(rows, function (k, v) {
                                    if (v.id != data.value.id) {
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(v), 1);
                                    }
                                });
                                $scope.igrid.UpdateData($scope.SearchResult);
                            }
                        });
                    }
                }
            });
        }
        $scope.AddedCreteria = [];
        $scope.TagCondition = {
            productgroupid: 0,
            from: 0,
            to: 0,
            type: 1
        }

        $scope.ManageFoodItem = function (action, item) {
            if (action == 'Delete') {
                $scope.Referencelist.additionalinfo.prdgroups.splice($scope.Referencelist.additionalinfo.prdgroups.indexOf(item), 1);
            }
        }

        $scope.AddFoodItem = function () {
            if ($scope.foodPgid &&
                $scope.foodPgid > 0) {
                if (typeof ($scope.Referencelist.additionalinfo.prdgroups) == 'undefined')
                    $scope.Referencelist.additionalinfo.prdgroups = [];
                $scope.Referencelist.additionalinfo.prdgroups.push($scope.foodPgid);
                $scope.foodPgid = 0;
            }
        }


        $scope.ManageTagItem = function (action, item) {
            if (action == 'Delete') {
                $scope.Referencelist.additionalinfo.pricetags.splice($scope.Referencelist.additionalinfo.pricetags.indexOf(item), 1);
            }
        }

        $scope.AddPriceTag = function () {
            var addedObject = {
                'pgid': $scope.TagCondition.pgid, 'from': $scope.TagCondition.from,
                'to': $scope.TagCondition.to, 'type': 1
            };
            if (typeof ($scope.Referencelist.additionalinfo.pricetags) == 'undefined')
                $scope.Referencelist.additionalinfo.pricetags = [];
            $scope.Referencelist.additionalinfo.pricetags.push(addedObject);
            $scope.TagCondition = {};
        }

        $scope.exporttoExcel = function (tableid, title, removeactioncolumn) {
            $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, title);
            $timeout(function () {
                var link = document.createElement('a');
                link.download = title + ".xls";
                link.href = $scope.exportHref;
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }

        $scope.OnContextMenuSelected = function (item, menu, config) {


            if (menu == 'Delete') {
                var request = {
                    "item": {
                        'type': 'DeleteProductHierarchy',
                        'id': item.node.id
                    },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "ReferencelistService/ChangeProductHierarchy",
                    data: srequest
                }).success(function (result) {
                    if (item.node.islast) {
                        $scope.CurrentNode = null;
                        if ($scope.gridPdgroupContainer != null)
                            $scope.gridPdgroupContainer.destroy();
                        $scope.gridPdgroupResults = [];
                    }
                    if ($scope.bintree.currentNode.parent &&
                        $scope.bintree.currentNode.parent.children &&
                        $scope.bintree.currentNode.parent.children.length > 0) {
                        $scope.bintree.currentNode.parent.children.splice($scope.bintree.currentNode.parent.children.indexOf($scope.bintree.currentNode), 1);
                    }
                    else {
                        $scope.bintreedata.splice($scope.bintreedata.indexOf($scope.bintree.currentNode), 1);
                    }
                });
                return;
            }
            //data.value.node.parentnode.children.push(data.value.node);


            if (menu == 'Edit' ||
                $scope.bintree.currentNode.nodetype == config.id ||
                $scope.bintree.currentNode.nodetype + 1 == config.id) {
                var productgroup = 0;
                var referenceid = 0;
                var parentgroup = null;
                if (menu != 'Edit') {
                    if (config.id == item.node.nodetype &&
                        $scope.bintree.currentNode.parent &&
                        $scope.bintree.currentNode.parent != null) {
                        referenceid = $scope.bintree.currentNode.parent.id;
                        productgroup = item.node.nodetype;
                        parentgroup = $scope.bintree.currentNode.parent;
                    }
                    else if (config.id > item.node.nodetype) {
                        referenceid = $scope.bintree.currentNode.id;
                        productgroup = config.id;
                        parentgroup = $scope.bintree.currentNode;
                    }
                    else {
                        productgroup = $scope.bintree.currentNode.nodetype;
                    }
                }
                $timeout(function () {

                    if (typeof ($scope.bintree.currentNode) == 'undefined')
                        return;
                    var node = $scope.bintree.currentNode;
                    var dialog = ngDialog.open({
                        template: 'HierarchyConfig',
                        data: [$scope, item, menu, node],
                        width: '700px',
                        controller: ['$scope', '$timeout', function ($scope, $timeout) {
                            $scope.parentScope = $scope.ngDialogData[0];
                            $scope.item = $scope.ngDialogData[1];
                            $scope.menu = $scope.ngDialogData[2];
                            $scope.node = $scope.ngDialogData[3];
                            if ($scope.menu == 'Edit') {
                                $scope.node.name = $scope.node.display;
                            }
                            $scope.Selected = function (item) {
                                $scope.closeThisDialog({
                                    'node': $scope.node,
                                    'menu': $scope.menu,
                                    'item': $scope.item,
                                    'code': $scope.code,
                                    'name': $scope.name,
                                });
                            };
                        }],
                        className: 'ngdialog-theme-plain'
                    });
                    dialog.closePromise.then(function (data) {
                        {
                            if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                                && data.value != '$document') {
                                if (data.value.menu == 'Edit') {
                                    data.value.node.display = data.value.node.name;

                                    var request = {
                                        "item": {
                                            'type': 'EditProductHierarchy',
                                            'code': data.value.code,
                                            'name': data.value.name,
                                            'id': node.id
                                        },
                                        "securitytoken": "String content"
                                    }
                                    var srequest = JSON.stringify(request);
                                    $http({
                                        method: 'POST',
                                        url: "ReferencelistService/ChangeProductHierarchy",
                                        data: srequest
                                    }).success(function (result) {

                                    });
                                    return;
                                }

                                var request = {
                                    "item": {
                                        'type': 'PRODUCTHIERARCHY',
                                        'code': data.value.code,
                                        'name': data.value.name,
                                        'productgroup': productgroup,
                                        "referenceid": referenceid
                                    },
                                    "securitytoken": "String content"
                                }

                                var srequest = JSON.stringify(request);

                                $http({
                                    method: 'POST',
                                    url: "ReferencelistService/Save",
                                    data: srequest
                                }).success(function (result) {
                                    if (result.id && result.id > 0) {
                                        var item = {
                                            "display": result.name, "id": result.id,
                                            "stylecollapsed": 'collapsed',
                                            "styleexpanded": 'expanded',
                                            "stylenormal": 'normal',
                                            "supportchild": true,
                                            "level": result.productgroup,
                                            "isactive": true,
                                            "parent": null,
                                            "nodetype": result.productgroup
                                        };
                                        item.children = [];
                                        if (parentgroup == null) {
                                            $scope.bintreedata.push(item);
                                        }
                                        else {
                                            parentgroup.children.push(item);
                                        }
                                    }
                                });


                            }
                        }
                    });

                }, 10);

            } else {
                $rootScope.Toaster.Error("Plesae select correct parent");
            }
        };

        $scope.BuildProductGroupValue = function (productgroupvalue, node) {
            productgroupvalue.push(node.id);
            if (node.parent && node.parent != null)
                $scope.BuildProductGroupValue(productgroupvalue, node.parent);
        }

        $scope.Add2ProductHierarchy = function (type) {
            if (type == "Add2Hierarchy") {

                var existmap = $scope.gridPdgroupResults.find(function (item) {
                    return item.id == $scope.CurrentProductGroupid;
                });
                if (existmap && existmap != null) {
                    $rootScope.Toaster.Error("Already this product available");
                    return;
                }
                var hierarchyitem = $scope.ProductHierarchyList.find(function (item) {
                    return item.id == $scope.CurrentNode.nodetype;
                });
                //"hierarchyconfig":{"islast":true,"isfirst":false,"p1":0,"p2":0,"p3":0,"p4":0,"p5":0}
                var request = {
                    "item": {
                        'type': 'Add2ProductHierarchy',
                        'id': $scope.CurrentProductGroupid,
                        'referenceid': $scope.CurrentNode.id,
                        'additionalinfo': {
                            "hierarchyconfig": {
                                "islast": hierarchyitem.islast,
                                "isfirst": hierarchyitem.isfirst,
                                "p1": 0, "p2": 0, "p3": 0, "p4": 0, "p5": 0
                            }
                        }
                    },
                    "securitytoken": "String content"
                }

                var productgroupvalue = [];
                $scope.BuildProductGroupValue(productgroupvalue, $scope.CurrentNode);
                productgroupvalue.reverse();
                $.each(productgroupvalue, function (k, v) {
                    request.item.additionalinfo.hierarchyconfig["p" + (k + 1)] = v;
                });
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "ReferencelistService/ChangeProductHierarchy",
                    data: srequest
                }).success(function (result) {
                    if (result.id && result.id > 0) {
                        var existproductgroup = $scope.ProductGroups.find(function (item) {
                            return item.id == result.id
                        });
                        if (existproductgroup == undefined || existproductgroup == null) {
                            $scope.ProductGroups.push(result);
                        }
                        else {
                            existproductgroup.referenceid = result.referenceid;
                            existproductgroup.additionalinfo = result.additionalinfo;
                        }
                        $scope.gridPdgroupResults.push(result);
                        $scope.gridPdgroupContainer.UpdateData($scope.gridPdgroupResults);
                    }
                });
            }
            else if (type == 'CreateProductGroup') {
                var dialog = ngDialog.open({
                    template: 'HierarchyConfig',
                    data: [$scope, type],
                    width: '700px',
                    controller: ['$scope', '$timeout', function ($scope, $timeout) {
                        $scope.parentScope = $scope.ngDialogData[0];
                        $scope.item = $scope.ngDialogData[1];
                        $scope.Selected = function (item) {
                            $scope.closeThisDialog({
                                'node': $scope.node,
                                'menu': $scope.item,
                                'code': $scope.code,
                                'name': $scope.name
                            });
                        };
                    }],
                    className: 'ngdialog-theme-plain'
                });
                dialog.closePromise.then(function (data) {
                    {
                        if (typeof (data.value) != 'undefined' && data.value != null
                            && data.value != '$escape'
                            && data.value != '$document') {
                            var request = {
                                "item": {
                                    'type': 'PRODUCTGROUPS',
                                    'code': data.value.code,
                                    'name': data.value.name,
                                    'isactive': true,
                                    'version': 1
                                },
                                "securitytoken": "String content"
                            }
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "ReferencelistService/Save",
                                data: srequest
                            }).success(function (result) {
                                if (result.id && result.id > 0) {
                                    $scope.ProductGroups.push(result);
                                }
                            });
                        }
                    }
                });
            }
        }

        $scope.BuildProductHierarchyList = function () {
            if ($rootScope.UserInfo.appsettings.prd_hierarchy &&
                $rootScope.UserInfo.appsettings.prd_hierarchy != null &&
                $rootScope.UserInfo.appsettings.prd_hierarchy != '') {
                var list = $rootScope.UserInfo.appsettings.prd_hierarchy.split(',');
                var hlist = [];
                $scope.allmenuOptions = [];
                for (var i = 0; i < list.length; i++) {
                    var hitem = {
                        'id': i + 1, 'name': list[i],
                        'islast': i == list.length - 1,
                        'isfirst': i == 0
                    };
                    hlist.push(hitem);
                    var menu = list[i];
                    if (hitem.isfirst)
                        menu = 'Create New ' + menu;
                    else
                        menu = 'Add ' + menu;
                    $scope.AssociateHieraryMenu(menu, hitem);
                }

                $scope.AssociateHieraryMenu('Edit', { 'name': 'Edit', 'islast': false, 'isfirst': false });
                $scope.AssociateHieraryMenu('Delete', { 'name': 'Delete', 'islast': false, 'isfirst': false });

                var available = (typeof ($rootScope.windowSize) != 'undefined' && typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
                if (available > 0) {
                    $scope.panelheight.height = '' + (available - 150) + 'px';
                }
                return hlist;
            }
            return [];
        }
        $scope.AssociateHieraryMenu = function (menu, hitem) {
            $scope.allmenuOptions.push([menu, function ($itemScope) {
                $scope.OnContextMenuSelected($itemScope, menu, hitem);
            }]);
        }
        $scope.ShowProductTree = true;
        $scope['bintree'] = $scope['bintree'] || {};
        $scope['bintree'].onCurrentNodeChange = function (node) {
            $scope.CurrentNode = node;
            var hierarchyitem = $scope.ProductHierarchyList.find(function (item) {
                return item.id == node.nodetype;
            });
            if (hierarchyitem.islast) {
                $scope.CurrentNode.islast = true;
                $timeout(function () {
                    $scope.ShowProductGroupGrid(node.id);
                }, 20);
            }
        }

        $scope.gridPdgroupContainer = null;
        $scope.gridPdgroupResults = [];


        $scope.ShowProductGroupGrid = function (parentid) {
            if ($scope.gridPdgroupContainer != null)
                $scope.gridPdgroupContainer.destroy();
            $scope.gridPdgroupResults = [];
            $.each($scope.ProductGroups, function (k, v) {
                if (v.referenceid == parentid) {
                    $scope.gridPdgroupResults.push(v);
                    var parentlist = [];
                    $scope.ParentHierarchyName(parentlist, v.referenceid);
                    v.parentdisplay = parentlist.join(' <- ');
                }
            });
            $scope.ShowHieraryProducts = false;
            var igrid = new InfyGrid('gridPdgroupContainer', '400px', '100%', true, true, true, 100);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('code', 'Code', 150);
            igrid.AddSimpleColumn('name', 'Name', 200);
            igrid.AddSimpleColumn('parentdisplay', 'Parent', 250);
            igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-warning btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            igrid.Initialize();
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;

                if (command == 'Edit') {

                    if (!$scope.ShowHieraryProducts) {
                        $timeout(function () {
                            $scope.ShowHieraryProducts = true;
                            if ($scope.ProductList == null) {
                                restService.Get('ReferencelistService/LIST-PRODUCT', true).then(function (results) {
                                    $scope.ProductList = results;
                                    $scope.ShowProductGroupProducts(item);
                                });
                            }
                            else {
                                $scope.ShowProductGroupProducts(item);
                            }
                        }, 50)
                    }
                    else {
                        if ($scope.ProductList == null) {
                            restService.Get('ReferencelistService/LIST-PRODUCT', true).then(function (results) {
                                $scope.ProductList = results;
                                $scope.ShowProductGroupProducts(item);
                            });
                        }
                        else {
                            $scope.$apply(function () {
                                $scope.ShowProductGroupProducts(item);
                            });
                        }
                    }

                }
            });
            $scope.gridPdgroupContainer = igrid;
            $scope.gridPdgroupContainer.UpdateData($scope.gridPdgroupResults);
        }

        $scope.ManageHierarchy = function (command) {
            if (command == 'Add') {
                var rows = $scope.gridproductlist.GetSelectedRows();
                $.each(rows, function (k, v) {
                    var exist = $scope.ProductSelectedList.find(function (item) {
                        return item.id == v.id;
                    });
                    if (exist == undefined || exist == null) {
                        $scope.ProductSelectedList.push(v);
                    }
                })
                $scope.gridselectedproductlist.UpdateData($scope.ProductSelectedList);
                $scope.gridproductlist.SetSelectedRows([]);
            }
            else if (command == 'Delete') {
                var rows = $scope.gridselectedproductlist.GetSelectedRows();
                $.each(rows, function (k, v) {
                    $scope.ProductSelectedList.splice($scope.ProductSelectedList.indexOf(v), 1);
                })
                $scope.gridselectedproductlist.UpdateData($scope.ProductSelectedList);
            } else if (command == 'Save') {

                var request = {
                    "Item": {
                        'removeexisting': true,
                        'productids': [],
                        'productgroup': $scope.CurrentProductGroup.id
                    },
                    "SecurityToken": "String content"
                }

                $.each($scope.ProductSelectedList, function (k, v) {
                    request.Item.productids.push(v.id);
                })
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "ProductsService/ChangeProductGroup",
                    data: srequest
                }).success(function (result) {
                    $.each($scope.ProductSelectedList, function (k, v) {
                        v.productgroup = $scope.CurrentProductGroup.id;
                    })
                    $.each($scope.ProductList, function (k, v) {
                        if (v.productgroup == $scope.CurrentProductGroup.id)
                            v.productgroup = 0;

                        var exist = $scope.ProductSelectedList.find(function (item) {
                            return item.id == v.id;
                        });
                        if (exist && exist != null) {
                            exist.productgroup = $scope.CurrentProductGroup.id;
                        }
                    })
                    $scope.gridproductlist.UpdateData($scope.ProductList);
                    $scope.gridselectedproductlist.UpdateData($scope.ProductSelectedList);
                    $rootScope.Toaster.Info("Updated successfully.");
                });

            }
        }


        $scope.gridSectionCosting = null;
        $scope.ShowSectionCosting = function (supportconfig) {
            if ($scope.gridSectionCosting != null)
                $scope.gridSectionCosting.destroy();

            if (supportconfig) {
                var igrid = new InfyGrid('gridSectionCosting', '200px', '100%', true, true, true, 400);
                igrid.options.showHeaderRow = true;
                igrid.options.autoEdit = true;
                igrid.AddReferenceColumn('id', 'Location', 250, $scope.RetailLocations);
                var column = igrid.AddColumn(
                    {
                        id: "sqft", name: "STORE SQFT", field: "sqft",
                        width: 150, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );
                column = igrid.AddColumn(
                    {
                        id: "avginv", name: "STORE AVG INVENTORY", field: "avginv",
                        width: 150, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );
                igrid.Initialize();
                if ($scope.Referencelist.additionalinfo == undefined ||
                    $scope.Referencelist.additionalinfo == null)
                    $scope.Referencelist.additionalinfo = {};

                if ($scope.Referencelist.additionalinfo.locationconfig == undefined ||
                    $scope.Referencelist.additionalinfo.locationconfig == null)
                    $scope.Referencelist.additionalinfo.locationconfig = {};

                if ($scope.Referencelist.additionalinfo.locationconfig.retinv == undefined ||
                    $scope.Referencelist.additionalinfo.locationconfig.retinv == null)
                    $scope.Referencelist.additionalinfo.locationconfig.retinv = [];

                $.each($scope.RetailLocations, function (k, v) {
                    var exist = $scope.Referencelist.additionalinfo.locationconfig.retinv.find(function (item) {
                        return item.id == v.id;
                    });
                    if (exist == undefined || exist == null) {
                        $scope.Referencelist.additionalinfo.locationconfig.retinv.push({ 'id': v.id, 'avginv': '', 'sqft': '' });
                    }
                })
                $scope.gridSectionCosting = igrid;
                $scope.gridSectionCosting.UpdateData($scope.Referencelist.additionalinfo.locationconfig.retinv);
            }
        }


        $scope.CurrentProductGroup = null;
        $scope.ProductList = null;
        $scope.ProductSelectedList = null;
        $scope.gridselectedproductlist = null;
        $scope.gridproductlist = null;
        $scope.ShowHieraryProducts = false;
        $scope.ShowProductGroupProducts = function (item) {
            $scope.CurrentProductGroup = item;
            if ($scope.gridselectedproductlist != null)
                $scope.gridselectedproductlist.destroy();
            if ($scope.gridproductlist != null)
                $scope.gridproductlist.destroy();
            var igrid = new InfyGrid('gridproductlistContainer', '200px', '100%', true, true, true, 400);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('name', 'Name', 200);
            igrid.AddReferenceColumn('productgroup', 'Group', 200, $scope.ProductGroups);
            igrid.Initialize();
            $scope.gridproductlist = igrid;
            $scope.gridproductlist.UpdateData($scope.ProductList);

            var igrid = new InfyGrid('gridselectedproductlistContainer', '270px', '100%', true, true);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('name', 'Name', 200);
            igrid.AddReferenceColumn('productgroup', 'Group', 200, $scope.ProductGroups);
            igrid.Initialize();
            $scope.gridselectedproductlist = igrid;
            restService.Get('ReferencelistService/LIST-PRODUCT/' + item.id, true).then(function (results) {
                $scope.ProductSelectedList = results;
                $scope.gridselectedproductlist.UpdateData(results);
            });
        }

        $scope.ProductGroupHierarchy = null;
        $scope.ProductHierarchyList = null;
        $scope.ProductGroupParents = null;
        $scope.UpdateProductHierarchy = function () {
            var list = [];
            var currentparentid = $scope.Referencelist.referenceid;
            while (true) {
                list.push(currentparentid);
                var hparent = $scope.ProductHierarchy.find(function (item) {
                    return item.id == currentparentid;
                });
                if (hparent == undefined || hparent == null || hparent.referenceid == 0)
                    break;
                currentparentid = hparent.referenceid;
            }
            if ($scope.Referencelist.additionalinfo == undefined ||
                $scope.Referencelist.additionalinfo == null)
                $scope.Referencelist.additionalinfo = {};

            if ($scope.Referencelist.additionalinfo.hierarchyconfig == undefined ||
                $scope.Referencelist.additionalinfo.hierarchyconfig == null)
                $scope.Referencelist.additionalinfo.hierarchyconfig = {};

            list.reverse();
            $.each(list, function (k, v) {
                $scope.Referencelist.additionalinfo.hierarchyconfig['p' + (k + 1)] = v;
            })
        }


        $scope.ProductTreeChanged = function () {
            $scope.ShowProductTree = !$scope.ShowProductTree;
            if (!$scope.ShowProductTree) {
                var template = '<div class="col-md-12" style="padding-top:10px;padding-left:0px">'
                    + '<div class="col-md-12" style="overflow-y:scroll;padding-left:0px;" ng-style="panelheight">'
                    + '         <div data-angular-treeview="true"'
                    + '              data-tree-id="bintree"'
                    + '              data-tree-model="bintreedata"'
                    + '              data-node-id="id"'
                    + '              data-node-label="display"'
                    + '              data-node-children="children">'
                    + '         </div>'
                    + '     </div>'
                    + ' </div>';
                angular.element($("#produthierarchy")).empty();
                angular.element($("#produthierarchy")).append($compile(template)($scope));
            }
        }

        $scope.BuildProductHierarchy_GetChild = function (parent, parentid, alldata) {
            var children = [];
            $.each(alldata, function (k, v) {
                if (v.referenceid == parentid) {
                    var item = {
                        "display": v.name, "id": v.id,
                        "code": v.code,
                        "stylecollapsed": 'collapsed',
                        "styleexpanded": 'expanded',
                        "stylenormal": 'normal',
                        "supportchild": true,
                        "level": v.productgroup,
                        "isactive": true,
                        "nodetype": v.productgroup,
                        "parent": parent
                    };
                    item.children = $scope.BuildProductHierarchy_GetChild(item, v.id, alldata);
                    children.push(item);
                }
            });
            return children;
        }

        $scope.BuildProductHierarchy = function (firstlevel, alldata) {
            $scope.bintreedata = [];

            $.each(firstlevel, function (k, v) {
                var item = {
                    "display": v.name, "id": v.id,
                    "code": v.code,
                    "stylecollapsed": 'collapsed',
                    "styleexpanded": 'expanded',
                    "stylenormal": 'normal',
                    "supportchild": true,
                    "level": v.productgroup,
                    "isactive": true,
                    "parent": null,
                    "nodetype": v.productgroup
                };
                item.children = $scope.BuildProductHierarchy_GetChild(item, v.id, alldata);
                $scope.bintreedata.push(item);
            })
        }

        $scope.ListPaymentIntegrationKeyValues = function () {
            if ($scope.Referencelist.additionalinfo.payintegration.type) {
                var mode = $scope.PaymentIntegrationModes.find(function (item) {
                    return item.id == $scope.Referencelist.additionalinfo.payintegration.type;
                });
                var paymentlist = [];
                $.each(mode.keys, function (k, v) {
                    paymentlist.push({ 'key': v, 'value': '' });
                });
                $scope.Referencelist.additionalinfo.payintegration.list = paymentlist;
            }
        }

        $scope.HierarchyChanged = function (updatemodelonly) {
            var selected = $scope.ProductHierarchyList.find(function (item) {
                return item.id == $scope.Referencelist.productgroup;
            });
            if (selected && selected != null) {
                if ($scope.Referencelist.additionalinfo == undefined ||
                    $scope.Referencelist.additionalinfo == null)
                    $scope.Referencelist.additionalinfo = {};

                if ($scope.Referencelist.additionalinfo.hierarchyconfig == undefined ||
                    $scope.Referencelist.additionalinfo.hierarchyconfig == null)
                    $scope.Referencelist.additionalinfo.hierarchyconfig = {};

                $scope.Referencelist.additionalinfo.hierarchyconfig.isfirst = selected.isfirst;
                $scope.Referencelist.additionalinfo.hierarchyconfig.islast = selected.islast;

                if (updatemodelonly == undefined || updatemodelonly == false) {
                    if (selected.isfirst) {
                        $scope.ProductGroupHierarchy = null;
                    }
                    else {
                        var list = [];
                        var parentproductgroup = $scope.Referencelist.productgroup - 1;
                        $.each($scope.ProductHierarchy, function (k, v) {
                            if (v.productgroup == parentproductgroup) {
                                list.push(v);
                            }
                        })
                        $scope.ProductGroupHierarchy = list;
                    }
                }
            }
        }

        $scope.system_invcol_pricetype = false;
        $scope.LoadApplicationSetting = function () {
            if (typeof ($rootScope.UserInfo) != 'undefined' &&
                typeof ($rootScope.UserInfo.appsettings) != 'undefined' &&
                $rootScope.UserInfo.appsettings != null) {
                if ($scope.IsAttributes) {
                    $scope.ProductHierarchyList = $scope.BuildProductHierarchyList();
                    if ($rootScope.UserInfo.appsettings.system_customfields) {
                        restService.Get('ReferencelistService/FULL-CUSTOMFIELDS', true).then(function (results) {
                            $.each(results, function (k, v) {
                                if (v.additionalinfo && v.additionalinfo.customfield && v.additionalinfo.customfield.cfbuilt &&
                                    v.referenceid == 4) {
                                    $scope.ReferenceTypeList.push({ 'name': v.name, 'code': v.code });
                                }
                            })
                        });
                    }
                }
                $scope.mfg_mode = $rootScope.UserInfo.appsettings.mfg_mode;
                $scope.appmode = $rootScope.UserInfo.appsettings.appmode;
                $scope.system_invcol_pricetype = $rootScope.UserInfo.appsettings.system_invcol_pricetype;
            }
            else {
                $timeout(function () {
                    $scope.LoadApplicationSetting();
                }, 50);
            }
        }
        $scope.panelheight = { 'height': '400px' };
        $scope.LoadApplicationSetting();

    }]);