
var app = angular.module('InfinityLab');

app.controller('WareHouseReportController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$filter', 'Excel', '$timeout', '$stateParams',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, Excel, $timeout, $stateParams) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Permission = UserDetails.Info;

        $scope.reporttype = "LRREPORT";
        $scope.ReportType = [];
        $scope.AllReportType = [{ 'code': 'LRREPORT', 'name': 'Transport Report' },
            { 'code': 'LRINVOICE', 'name': 'Invoice Report' },
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
            { 'code': 'STOCKANALYSIS', 'name': 'Analytical Stock Report' }
        ];

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
            $rootScope.BindCompany($scope, 'SearchCondition.companyid');
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


        $scope.city = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.city = results;
        });
        $scope.Supplier = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Supplier = results;
        });

        $scope.Location = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Location = results;
            $rootScope.BindLocation($scope, 'SearchCondition.locationid');
            $rootScope.BindLocation($scope, 'SearchCondition.fromlocationid');
            $rootScope.BindLocation($scope, 'SearchCondition.tolocationid');
        });
        $scope.Product = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Product = results;
        });
        $scope.Brand = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brand = results;
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
            section: false
        };

        $scope.ShowFilter = true;
        $scope.ChangeReport = function () {
            $scope.SearchResult = [];
            $scope.LRResult = [];
            $scope.LRInvoiceResult = [];
            $scope.DebitnoteResult = [];
            $scope.PurchaseEntryResult = [];
            $scope.PurchaseItemListResult = [];
            $scope.PurchaseListResult = [];
            $scope.PurchaseHSNResult = [];
            $scope.POReturnResult = [];
            $scope.OutwardResult = [];
            $scope.BarcodeResult = [];
            $scope.StockResult = [];
            $scope.OutwardPendingResult = [];
            $scope.StockAnalysisResult = [];
        }

        $scope.SearchResult = [];
        $scope.LRResult = [];
        $scope.LRInvoiceResult = [];
        $scope.DebitnoteResult = [];
        $scope.PurchaseEntryResult = [];
        $scope.PurchaseHSNResult = [];
        $scope.POReturnResult = [];
        $scope.OutwardResult = [];
        $scope.BarcodeResult = [];
        $scope.StockResult = [];
        $scope.OutwardPendingResult = [];
        $scope.StockAnalysisResult = [];
        $scope.PurchaseItemListResult = [];
        $scope.PurchaseListResult = [];

        $scope.Productdetail = "";
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

        $scope.SearchPurchaseList = function () {
            $scope.PurchaseListResult = [];
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
                url: "WarehouseReportService/SearchPOEntryList",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                var vtaxableamount = 0.0;
                var vsgst = 0.0;
                var vcgst = 0.0;
                var vigst = 0.0;
                var votheramount = 0.0;
                var vinvoiceamount = 0.0;
                $.each(result, function (k, v) {
                    $scope.tax += v.taxamount;
                    $scope.taxableamount += v.taxableamount;
                    $scope.amount += v.invoiceamount;

                    vtaxableamount += v.taxableamount;
                    vcgst += v.cgst;
                    vsgst += v.sgst;
                    vigst += v.igst;
                    votheramount += v.otheramount;
                    vinvoiceamount += v.invoiceamount;
                });

                result.push({
                    supplier: 'TOTAL',
                    taxableamount: vtaxableamount,
                    sgst: vsgst,
                    cgst: vcgst,
                    igst: vigst,
                    otheramount: votheramount,
                    invoiceamount: vinvoiceamount
                });

                $scope.PurchaseListResult = result;
                $scope.lrcount = $scope.PurchaseListResult.length;

                //$scope.PurchaseConfig();
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


        $scope.SearchPurchaseItemList = function () {
            $scope.PurchaseItemListResult = [];
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
                url: "WarehouseReportService/SearchPurchaseItemDetails",
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

                $scope.PurchaseItemListResult = result;
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

                        $scope.totaltaxbleamount += v.taxableamount;
                        $scope.totaltaxamount += v.taxamount;
                        $scope.toatlinvoiceamount = ($scope.totaltaxbleamount + $scope.totaltaxamount);
                    });
                });
                $scope.PurchaseHSNConfig();

            }).error(function (result) {
            });
        }

        $scope.SearchPOReturn = function () {
            $scope.POReturnResult = [];
            $scope.TaxHeaderPurchaseReturn = [];
            $scope.tax = 0;
            $scope.amount = 0;
            $scope.SearchCondition.cancelled = false;
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


        $scope.SearchStock = function () {
            $scope.totalqty = 0;
            $scope.amount = 0;
            $scope.StockResult = [];
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
                $scope.StockResult = result;
                $.each(result, function (k, v) {
                    $scope.totalqty += v.qty;
                    $scope.amount += v.qty * v.buyingprice;
                });
                $scope.StockConfig();
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
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

            $scope.totalcount = 0;
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

            $scope.OutwardResult = [];
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

        $scope.SearchNewProducts = function () {

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
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.NewProductConfig();
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
                url: "WarehouseReportService/SearchNewSuppliers",
                data: srequest
            }).success(function (result) {
                $scope.SearchNewSuppliersList = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.NewSuppliersConfig();
            }).error(function (result) {
            });
        }


        $scope.SearchOutwardDetails = function () {
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
                url: "WarehouseReportService/SearchOutward",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                else {
                    $scope.OutwardDetailsResult = result[0].items;
                }
                $scope.OutwardDetailsConfig();
            }).error(function (result) {
            });
        }


        $scope.ManageSearchItem = function (action, item) {
            if (action == "PRODUCTVIEW") {
                $scope.SearchProduct(item.lrid);
            } else if (action == "BARCODEVIEW") {
                $scope.SearchBarcode(item.barcode);
            }
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
                if (result != 'undefined' && result.Items.length > 0) {
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
                if (result != 'undefined') {
                    $scope.Barcodedetail = result;
                    $scope.printDiv('stockviewprintdiv', 'BARCODE VIEW');
                }
            }).error(function (result) {
            });

        }
        $scope.SearchStockAnalysis = function () {
            if (!$scope.SearchCondition.product && !$scope.SearchCondition.brand && !$scope.SearchCondition.material && !$scope.SearchCondition.pattern && !$scope.SearchCondition.style && !$scope.SearchCondition.size && !$scope.SearchCondition.section)
                $scope.SearchCondition.product = true;

            $scope.totalqty = 0;
            $scope.amount = 0;
            $scope.StockAnalysisResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
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

        $scope.LRConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true,0);

            igrid.AddSimpleColumn('supplier', 'Supplier', 200);
            igrid.AddSimpleColumn('lrentryno', 'LR Entry No', 120);
            igrid.AddFormatedColumn('entrydate', 'Entry Date', 150, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('lrno', 'LR No', 120);
            igrid.AddFormatedColumn('lrdate', 'LR Date', 150, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('totalamount', 'LR Value', 150);
            igrid.AddSimpleColumn('noofbundels', 'No Of Bundles', 100);
            igrid.AddReferenceColumn('modeofdelivery', 'Delivery Mode', 120, $scope.ModeofDelivery);

            igrid.options.autoEdit = true;

            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.LRResult);
        }

        $scope.InvoiceConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true,0);

            igrid.AddSimpleColumn('supplier', 'Supplier', 300);
            igrid.AddSimpleColumn('lrentryno', 'LR Entry No', 120);
            igrid.AddFormatedColumn('invoiceno', 'Invoice No', 150, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('entrydate', 'Inv.Ent Date', 150, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 150, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('totalamount', 'Value', 150);

            igrid.options.autoEdit = true;

            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.LRInvoiceResult);
        }

        $scope.PurchaseConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true,0);

            igrid.AddSimpleColumn('supplier', 'Supplier', 200);
            igrid.AddReferenceColumn('cityid', 'City', 160, $scope.city);
            igrid.AddSimpleColumn('isinterstatesale', 'IGST', 120);
            igrid.AddSimpleColumn('lrentryno', 'EntryNo', 100);
            igrid.AddFormatedColumn('lrentrydate', 'Entry Date', 150, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('lrentryno', 'Invoice No', 120);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 150, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('invoiceamount', 'Invoice Amount', 120);

            igrid.options.autoEdit = true;

            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.PurchaseEntryResult);
        }
        $scope.PurchaseListConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true,0);

            igrid.AddSimpleColumn('supplier', 'Supplier', 200);
            igrid.AddReferenceColumn('cityid', 'City', 160, $scope.city);
            igrid.AddSimpleColumn('isinterstatesale', 'IGST', 120);
            igrid.AddSimpleColumn('lrentryno', 'EntryNo', 100);
            igrid.AddFormatedColumn('lrentrydate', 'Entry Date', 150, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('lrentryno', 'Invoice No', 120);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 150, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('invoiceamount', 'Invoice Amount', 120);

            igrid.options.autoEdit = true;

            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.PurchaseEntryResult);
        }
        $scope.PurchaseHSNConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '350px', '100%', true, true, true,0);

            igrid.AddSimpleColumn('supplier', 'Supplier', 320);
            igrid.AddReferenceColumn('cityid', 'City', 260, $scope.city);
            igrid.AddSimpleColumn('isinterstatesale', 'IGST', 120);
            igrid.AddSimpleColumn('hsncode', 'HSN Code', 170);
            igrid.AddSimpleColumn('productname', 'Item Name', 220);

            igrid.options.autoEdit = true;

            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.PurchaseHSNResult);
        }

        $scope.PurchseRetConfig = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '350px', '100%', true, true, true,0);

            igrid.AddSimpleColumn('supplier', 'Supplier', 250);
            igrid.AddReferenceColumn('cityid', 'City', 170, $scope.city);
            igrid.AddSimpleColumn('isinterstatesale', 'IGST', 120);
            igrid.AddSimpleColumn('returnno', 'RTN No', 120);
            igrid.AddFormatedColumn('returndate', 'RTN Date', 150, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('invoiceno', 'Inv.No', 120);
            igrid.AddFormatedColumn('invoicedate', 'Inv.Date', 150, 'dd-MM-yyyy');
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.POReturnResult);


        }

        $scope.PurchseRetCancelConfig = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '370px', '100%', true, true, true,0);

            igrid.AddSimpleColumn('supplier', 'Supplier', 220);
            igrid.AddReferenceColumn('cityid', 'City', 160, $scope.city);
            igrid.AddSimpleColumn('isinterstatesale', 'IGST', 120);
            igrid.AddSimpleColumn('returnno', 'RTN No', 120);
            igrid.AddFormatedColumn('returndate', 'RTN Date', 150, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('invoiceno', 'Inv.No', 120);
            igrid.AddFormatedColumn('invoicedate', 'Inv.Date', 150, 'dd-MM-yyyy');
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.POReturnResult);


        }

        $scope.StockConfig = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '350px', '100%', true, true, true,0);

            igrid.AddSimpleColumn('itemname', 'Printing Name', 100);
            igrid.AddSimpleColumn('barcode', 'Barcode', 60);
            igrid.AddSimpleColumn('buyingprice', 'Buy.Price', 60);
            igrid.AddSimpleColumn('price', 'Price', 60);
            igrid.AddSimpleColumn('discount', 'Discount', 60);
            igrid.AddSimpleColumn('iqty', 'P.Qty', 60);
            igrid.AddSimpleColumn('qty', 'Qty', 60);
            igrid.AddSimpleColumn('productname', 'Product', 100);
            igrid.AddSimpleColumn('brandname', 'Brand', 90);
            igrid.AddSimpleColumn('designid', 'Design', 90);
            igrid.AddSimpleColumn('materialname', 'Material', 90);
            igrid.AddSimpleColumn('patternname', 'Pattern', 90);
            igrid.AddSimpleColumn('stylename', 'Style', 90);
            igrid.AddSimpleColumn('sizename', 'Size', 60);
            igrid.AddButtonColumn('action', 'Action', 50, 'View', function (e, args) {

                $scope.$apply(function () {
                    $scope.ManageSearchItem('BARCODEVIEW', args.item);
                });
                return true;

            });
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.StockResult);


        }

        $scope.StockAnalysisConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '350px', '100%', true, true, true,0);
            igrid.AddReferenceColumn('supplierid', 'Supplier', 100, $scope.Supplier);
            igrid.AddSimpleColumn('productname', 'Product Name', 100);
            igrid.AddSimpleColumn('brandname', 'Brand Name', 60);
            igrid.AddSimpleColumn('materialname', 'Material', 90);
            igrid.AddSimpleColumn('patternname', 'Pattern', 90);
            igrid.AddSimpleColumn('stylename', 'Style', 90);
            igrid.AddSimpleColumn('sizename', 'Size', 60);
            igrid.AddSimpleColumn('designid', 'Design', 90);
            igrid.AddSimpleColumn('sectionid', 'Section', 90);
            igrid.AddSimpleColumn('qty', 'Qty', 60);
            igrid.AddSimpleColumn('price', 'Price', 60);

            igrid.AddButtonColumn('action', 'Action', 50, 'View', function (e, args) {

                $scope.$apply(function () {
                    $scope.ManageSearchItem('BARCODEVIEW', args);
                });
                return true;
            });
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.StockAnalysisResult);


        }

        $scope.OutwardConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true,0);

            igrid.AddSimpleColumn('code', 'Package No', 120);
            igrid.AddSimpleColumn('fromlocation', 'From Location', 150);
            igrid.AddSimpleColumn('tolocation', 'To Location', 120);
            igrid.AddFormatedColumn('packedon', 'Packed On', 120, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('receivedon', 'Received On', 120, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('receivedby', 'Received By', 150);
            igrid.AddSimpleColumn('qty', 'Qty', 100);
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.OutwardResult);


        }

        $scope.NewProductConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '360px', '100%', true, true, true,0);

            igrid.AddReferenceColumn('name', 'ProductName', 300, $scope.Product);
            igrid.AddSimpleColumn('code', 'Code', 250);
            igrid.AddSimpleColumn('createdby', 'Created By', 180);
            igrid.AddSimpleColumn('created', 'Created On', 170);
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.NewProductList);


        }

        $scope.PendingOutwardConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '360px', '100%', true, true, true,0);

            igrid.AddSimpleColumn('typename', 'Supplier', 120);
            igrid.AddSimpleColumn('itemname', 'Printing Name', 130);
            igrid.AddSimpleColumn('barcode', 'Barcode', 90);
            igrid.AddFormatedColumn('buyingprice', 'Buy.Price', 80);
            igrid.AddFormatedColumn('price', 'Price', 90);
            igrid.AddFormatedColumn('discount', 'Discount', 80);
            igrid.AddSimpleColumn('iqty', 'P.Qty', 60);
            igrid.AddSimpleColumn('qty', 'Qty', 60);
            igrid.AddSimpleColumn('sizename', 'LR.Ref No', 100);
            igrid.AddSimpleColumn('productname', 'Product', 120);
            igrid.AddSimpleColumn('brandname', 'Brand', 120);
            igrid.AddSimpleColumn('materialname', 'Material', 100);

            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.OutwardPendingResult);


        }
        
        $scope.OutwardDetailsConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '360px', '100%', true, true, true,0);

            igrid.AddReferenceColumn('supplierid', 'Supplier', 300, $scope.Supplier);
            igrid.AddSimpleColumn('lrreference', 'LR Reference No', 180);
            igrid.AddSimpleColumn('barcode', 'Barcode', 170);
            igrid.AddSimpleColumn('printingname', 'Printing Name', 250);
            igrid.AddSimpleColumn('iqty', 'Qty', 130);
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.OutwardDetailsResult);


        }


        $scope.PendingOutwardConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '360px', '100%', true, true, true,0);

            igrid.AddSimpleColumn('typename', 'Supplier', 120);
            igrid.AddSimpleColumn('itemname', 'Printing Name', 130);
            igrid.AddSimpleColumn('barcode', 'Barcode', 90);
            igrid.AddFormatedColumn('buyingprice', 'Buy.Price', 80);
            igrid.AddFormatedColumn('price', 'Price', 90);
            igrid.AddFormatedColumn('discount', 'Discount', 80);
            igrid.AddSimpleColumn('iqty', 'P.Qty', 60);
            igrid.AddSimpleColumn('qty', 'Qty', 60);
            igrid.AddSimpleColumn('sizename', 'LR.Ref No', 100);
            igrid.AddSimpleColumn('productname', 'Product', 120);
            igrid.AddSimpleColumn('brandname', 'Brand', 120);
            igrid.AddSimpleColumn('materialname', 'Material', 100);

            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.OutwardPendingResult);


        }

        //$scope.exporttoExcel = function (infygrid, filename) {
        //    $scope.exportHref = infygrid.ExportExcel(infygrid.grid, filename);
        //    if ($scope.exportHref != null) {
        //        $timeout(function () {
        //            var link = document.createElement('a');
        //            link.download = filename + ".xls";
        //            link.href = $scope.exportHref;
        //            document.body.appendChild(link);
        //            link.click();
        //            link.remove();
        //        }, 100);
        //    }
        //}


        $scope.exporttoExcel = function (tableid, title, removeactioncolumn) {
            if (tableid == "lrentryreport")
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'LR ENTRY REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'lrinvoicereport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'INVOICE ENTRY REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'purchaseitemlist')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'PURCHASE ENTRY REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'purchaseentry')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'PURCHASE ENTRY REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'purchaseentrylist')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'PURCHASE ENTRY REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company) + " ) ");
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

            $timeout(function () {
                var link = document.createElement('a');
                link.download = title + ".xls";
                link.href = $scope.exportHref;
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }


        $scope.LoadReportType();
        $timeout(function () {


            if (typeof ($stateParams.Search) != 'undefined' && $stateParams.Search) {

                if (typeof ($stateParams.fromdate) != 'undefined')
                    $scope.SearchCondition.fromdate = $stateParams.fromdate;

                if (typeof ($stateParams.todate) != 'undefined')
                    $scope.SearchCondition.todate = $stateParams.todate;

                //if (typeof ($stateParams.Command == 'OneMonthLR'))

                //    var onemonthago = moment().subtract(1, 'month');
                //   console.log(onemonthago.format());
                //   $scope.SearchCondition.fromdate = onemonthago.format();
                //   $scope.SearchCondition.todate = new Date();
                //   $scope.SearchLR();

                //   if (typeof ($stateParams.Command == 'OneMonthInvoice'))

                //       var onemonthago = moment().subtract(1, 'month');
                //   console.log(onemonthago.format());
                //   $scope.SearchCondition.fromdate = onemonthago.format();
                //   $scope.SearchCondition.todate = new Date();
                //   $scope.SearchLRInvoice();

                //   if (typeof ($stateParams.Command == 'OneMonthPurchaseReturn'))
                //       var onemonthago = moment().subtract(1, 'month');
                //   console.log(onemonthago.format());
                //   $scope.SearchCondition.fromdate = onemonthago.format();
                //   $scope.SearchCondition.todate = new Date();
                //   $scope.SearchPOReturn();

                   if ($stateParams.Command == 'SearchLR')
                       $scope.SearchLR();
                   $scope.ShowFilter = false;

                if ($stateParams.Command == 'SearchInvoice')
                    $scope.SearchLRInvoice();
                if ($stateParams.Command == 'PurchaseReturn')
                    $scope.SearchPOReturn();

                if ($stateParams.Command == 'OneMonthLR') {
                    var onemonthago = moment().subtract(1, 'month');
                    console.log(onemonthago.format());
                    $scope.SearchCondition.fromdate = onemonthago.format();
                    $scope.SearchCondition.todate = new Date();
                    $scope.SearchLR();
                }
                if ($stateParams.Command == 'OneMonthInvoice') {
                    var onemonthago = moment().subtract(1, 'month');
                    console.log(onemonthago.format());
                    $scope.SearchCondition.fromdate = onemonthago.format();
                    $scope.SearchCondition.todate = new Date();
                    $scope.SearchLRInvoice();
                }
                   
               if ($stateParams.Command == 'OneMonthPurchaseReturn')
                {
                var onemonthago = moment().subtract(1, 'month');
                console.log(onemonthago.format());
                $scope.SearchCondition.fromdate = onemonthago.format();
                $scope.SearchCondition.todate = new Date();
                $scope.SearchPOReturn();
                return;
                }
                 

            }


        }, 10);
    }]);