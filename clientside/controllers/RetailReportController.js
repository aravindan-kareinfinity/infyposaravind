
var app = angular.module('InfinityLab');

app.controller('RetailReportController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'Excel', '$filter','ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, Excel, $filter, ngDialog) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Permission = UserDetails.Info;

        $scope.reporttype = "SALES";
        $scope.ReportType = [];

        $scope.AllReportType = [{ 'code': 'SALES', 'name': 'Sales Report' },
                    { 'code': 'PRODUCTWISESALES', 'name': 'Productwise Sales Report' },
            { 'code': 'SALESRETURN', 'name': 'Sales Return Report' },
                        { 'code': 'ADDITIONALDISC', 'name': 'Additional Discount Report' },
            { 'code': 'SETTLEMENT', 'name': 'Day End Closing(Settlement) Report' },
            { 'code': 'SETTLEMENTDETAIL', 'name': 'Settlement Detail Report' },
            { 'code': 'SALEVSSETTLEMENT', 'name': 'Sale Vs Settlement Report' },
            { 'code': 'CANCELLEDBILL', 'name': 'Cancelled Bill Report' },
            { 'code': 'BULKBILL', 'name': 'Approval Sales Report' },
            { 'code': 'DELIVERY', 'name': 'Delivery Status Report' },
            { 'code': 'SALESTAX', 'name': 'Sales Tax Report' },
            { 'code': 'SALESHSN', 'name': 'Sales HSN Report' },
            { 'code': 'SALESBARCODE', 'name': 'Sales Barcode Report' },
            { 'code': 'SALESMAN', 'name': 'Sales man wise Report' },
        { 'code': 'STOCK', 'name': 'Stock Report' },
        { 'code': 'INWARD', 'name': 'Inward Report' },
        { 'code': 'CUSTOMERPAYMENT', 'name': 'Credit Customer Collection Report' },
            { 'code': 'SALESANALYSIS', 'name': 'Analytical Sales Report' },
              { 'code': 'MISSINGSTOCK', 'name': 'Missing Stock Report' },
            { 'code': 'STOCKMARKER', 'name': 'Stock Marker Report' },
            { 'code': 'INCENT_SECTION', 'name': 'Incentive Report' },
            { 'code': 'INCENT_EMPLOYEE', 'name': 'Incentive Report - Employees' },
            { 'code': 'PRICECHANGE', 'name': 'Price Changer Report' },
            { 'code': 'STOCKAUDIT', 'name': 'Stock Audit Report' }
        ];

        $scope.BillType = [{ 'id': '0', 'name': 'CASH' },
            { 'id': '1', 'name': 'CREDIT' },
            { 'id': '8', 'name': 'RETURN' },
            { 'id': '9', 'name': 'CANCEL' },
            { 'id': '10', 'name': 'APPROVED BILL' },
            { 'id': '11', 'name': 'BILLED' },
            { 'id': '-1', 'name': 'ALL' }];

        $scope.DeliveryStatus = [{ 'id': '0', 'name': 'RECEIVED' },
            { 'id': '1', 'name': 'DELIVERED' },
            { 'id': '2', 'name': 'RETURNED' },
            { 'id': '3', 'name': 'CANCELLED' }];

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
            $rootScope.BindCompany($scope, 'SearchCondition.companyid');
            $rootScope.BindCompany($scope, 'SearchCondition.fromcompanyid');
            $rootScope.BindCompany($scope, 'SearchCondition.tocompanyid');
        });

        $scope.Employees = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });
        $scope.Supplier = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Supplier = results;
        });
        $scope.Section = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Section = results;
        });
        $scope.Location = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Location = results;
            $rootScope.BindLocation($scope, 'SearchCondition.locationid');
            $rootScope.BindLocation($scope, 'SearchCondition.fromlocationid');
            $rootScope.BindLocation($scope, 'SearchCondition.tolocationid');
        });
        $scope.Counter = restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counter = results;
        });
        $scope.BankList = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            $scope.BankList = results;
        });
        $scope.Product = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Product = results;
        });
        $scope.Brand = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brand = results;
        });

        $scope.MarkerType = restService.Get('ReferencelistService/MARKER', true).then(function (results) {
            $scope.MarkerType = results;
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
        $scope.Material = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.Material = results;
        });
        $scope.Pattern = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.Pattern = results;
        });
        $scope.Style = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.Style = results;
        });

        $scope.billcount = 0;
        $scope.totalcash = 0.0;
        $scope.totalcard = 0.0;
        $scope.totalcredit = 0.0;
        $scope.totalsettlementmismatch = 0;
        $scope.totalunsettle = 0;
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
            companyid: 0,
            fromdate: new Date(),
            todate: new Date(),
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
            billerwise: true,
            cashierwise: false,
            setno: "",
            markerid:0
        };

        $scope.ChangeReport = function () {
            $scope.SearchResult = [];
            $scope.SalesResult = [];
            $scope.SalesReturnResult = [];
            $scope.SettlementResult = [];
            $scope.SettlementDetailResult = [];
            $scope.SaleVsSettlementResult = [];
            $scope.InwardResult = [];
            $scope.BarcodeResult = [];
            $scope.StockResult = [];
            $scope.CashcounterResult = [];
            $scope.SalesTaxResult = [];
            $scope.SalesTaxSummaryResult = [];
            $scope.CancelledBillResult = [];
            $scope.CustomerPaymentResult = [];
            $scope.SalesmanResult = [];
            $scope.SalesHSNResult = [];
            $scope.SalesBarcodeResult = [];
            $scope.SalesAnalysisResult = [];
            $scope.BulkBillResult = [];
            $scope.DeliveryResult = [];
            $scope.SalesPerDayList = [];
            $scope.BillwiseDiscountList = [];
            $scope.SettlementwiseDiscountList = [];
            $scope.MissingStockResult = [];
            $scope.StockMarkerList = [];
            $scope.PriceChangerList = [];
            $scope.StockAuditList = [];
            $scope.SearchCondition.locationid = 0;
            $scope.SearchCondition.companyid = 0;
            $scope.SearchCondition.unsettled = false;
            $scope.SearchCondition.showdetail = false;
            $scope.SearchCondition.summary = true;
        }

        $scope.SearchResult = [];
        $scope.SalesResult = [];
        $scope.SalesReturnResult = [];
        $scope.SettlementResult = [];
        $scope.SettlementDetailResult = [];
        $scope.SaleVsSettlementResult = [];
        $scope.InwardResult = [];
        $scope.BarcodeResult = [];
        $scope.StockResult = [];
        $scope.CashcounterResult = [];
        $scope.SalesTaxResult = [];
        $scope.SalesTaxSummaryResult = [];
        $scope.CancelledBillResult = [];
        $scope.CustomerPaymentResult = [];
        $scope.SalesmanResult = [];
        $scope.SalesHSNResult = [];
        $scope.SalesBarcodeResult = [];
        $scope.SalesAnalysisResult = [];
        $scope.DeliveryResult = [];
        $scope.SalesPerDayList = [];
        $scope.BillwiseDiscountList = [];
        $scope.SettlementwiseDiscountList = [];
        $scope.MissingStockResult = [];
        $scope.StockMarkerList = [];
        $scope.PriceChangerList = [];
        $scope.StockAuditList = [];

        $scope.SearchSales = function () {
            if (!$scope.SearchCondition.summary && !$scope.SearchCondition.unsettled && !$scope.SearchCondition.locationwise 
                && ($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
                $rootScope.Toaster.Info("Please select location");
                return;
            }
            if ($scope.SearchCondition.showdetail && ($scope.SearchCondition.billno == "" && $scope.SearchCondition.barcode == "")) {
                $rootScope.Toaster.Info("Please enter billno or barcode");
                return;
            }
            if ($scope.SearchCondition.unsettled && $scope.SearchCondition.summary) {
                $rootScope.Toaster.Info("Please select either summary or un-settled report");
                return;
            }
            $scope.SalesResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            /*Cash = 0,
            Credit = 1,
            Card = 2,
            Redemption = 3,
            ReturnBill = 8,
            Cancel = 9*/

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSales",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }

                $scope.saleqty = 0;
                $scope.saleamount = 0;
                $scope.salecredit = 0;
                $scope.salediscount = 0;
                $scope.saletax = 0;
                $scope.returnamount = 0;
                $scope.cancelledamount = 0;
                $scope.billcount = 0;
                $scope.saleadditionaldiscount = 0;

                $scope.billcount = result.length;
                $.each(result, function (k, v) {
                    if (v.billtype == 8) {
                        $scope.returnamount += v.amount;
                    } else if (v.billtype == 9) {
                        $scope.cancelledamount += v.amount;
                    } else if (v.billtype == 1)
                        $scope.salecredit += v.amount;
                    if (v.billtype != 9) {
                        $scope.saleqty += v.saleqty;
                        $scope.saleamount += v.amount;
                        $scope.salediscount += v.discount;
                        $scope.saletax += v.saletax;
                        $scope.saleadditionaldiscount += v.addlndiscount;
                    }
                });

                result.push({
                    location: 'TOTAL',
                    saleqty: $scope.saleqty,
                    amount: $scope.saleamount,
                    discount: $scope.salediscount,
                    saletax: $scope.saletax,
                    addlndiscount: $scope.saleadditionaldiscount
                });

                $scope.SalesResult = result;

            }).error(function (result) {
            });
        }


        $scope.MissingStock = function () {

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
            }).error(function (result) {
            });

        }

        $scope.StockMarker = function () {

            $scope.totalcount = 0;
            $scope.totalqty = 0;
            $scope.totalamt = 0;
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
                        $scope.totalamt += v.iqty;
                    });
                }
            }).error(function (result) {
            });
        }

        $scope.SearchIncentives = function () {
            $scope.SearchCondition.mode = $scope.reporttype == 'INCENT_SECTION' ? 'section' : 'employee';
            if (typeof ($scope.SearchCondition.employeeid) == 'undefined')
                $scope.SearchCondition.employeeid = 0;

            if ($scope.SearchCondition.showdetail && $scope.SearchCondition.employeeid <= 0) {
                $rootScope.Toaster.Info("Please select employee");
                return;
            }
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
                }
            }).error(function (result) {
            });
        }


        $scope.SearchAdditionalDiscount = function () {
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
                url: "RetailReportService/SearchAdditionalDiscount",
                data: srequest
            }).success(function (result) {
                if (result != null) {
                    $scope.SalesPerDayList = result.lstsalevalue;
                    $scope.BillwiseDiscountList = result.lstbillwisediscount;
                    $scope.SettlementwiseDiscountList = result.lstsettlementwisediscount;

                    if ($scope.SearchCondition.billerwise) {
                        $.each(result.lstbillwisediscount, function (k, v) {
                            $scope.totaldisc += v.additionaldiscount;
                        });
                    }

                    if ($scope.SearchCondition.cashierwise) {
                        $.each(result.lstsettlementwisediscount, function (k, v) {
                            $scope.totaldisc += v.additionaldiscount;
                        });
                    }
                }
            }).error(function (result) {
            });
        }

        $scope.ProductWiseSale = function () {
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
                        $scope.totalamt += v.receivetotal;
                        $scope.totaldisc += v.discount;
                    });

                    $.each(result.lstbillwisediscount, function (k, v) {
                        $scope.totaldisc += v.additionaldiscount;
                    });

                    $.each(result.lstsettlementwisediscount, function (k, v) {
                        $scope.totaldisc += v.additionaldiscount;
                    });
                }
            }).error(function (result) {
            });

        }


        $scope.CancelBills = function () {
            var ids = [];
            $.each($scope.SalesResult, function (k, v) {
                if (v.selected && v.id > 0)
                    ids.push(v.id);
            })

            if (ids.length == 0) {
                $rootScope.Toaster.Notice('Please select the bills to be cancelled');
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
            }).error(function (result) {
            });
        }
        $scope.SearchSettlement = function () {
            //if (($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined')) {
            //    $rootScope.Toaster.Info("Please select location");
            //    return;
            //}

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
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                    return;
                }
                $scope.totalcash = 0;
                $scope.totalcard = 0;
                $scope.totalcredit = 0;
                $scope.totaladjustment = 0;
                $scope.returnamount = 0;
                var totaldiscount = 0;
                var totalvoucher = 0;
                $.each(result, function (k, v) {
                    $scope.totalcash += v.paid_cash;
                    $scope.totalcard += v.paid_card;
                    $scope.totalcredit += v.paid_credit;
                    $scope.totaladjustment += v.adjustment_paid;
                    $scope.returnamount += v.return_cash;
                    totalvoucher += v.paid_voucher;
                    totaldiscount += v.discount;
                });
                
                result.push({
                    location: 'TOTAL',
                    paid_cash: $scope.totalcash,
                    paid_card: $scope.totalcard,
                    paid_credit: $scope.totalcredit,
                    paid_voucher: totalvoucher,
                    adjustment_paid: $scope.totaladjustment,
                    discount: totaldiscount,
                    return_cash: $scope.returnamount
                });
                $scope.SettlementResult = result;

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
                $scope.totalcredit = 0;
                $scope.totaladjustment = 0;
                $scope.returnamount = 0;
                $.each(result, function (k, v) {
                    $scope.totalcash += v.paid_cash;
                    $scope.totalcard += v.paid_card;
                    $scope.totalcredit += v.paid_credit;
                    $scope.totaladjustment += v.adjustment_paid;
                    $scope.returnamount += v.return_cash;
                });
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
            }).error(function (result) {
            });
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

                $scope.billcount = result.length;
                $scope.saleamount = 0;
                $scope.totalamt = 0;
                $scope.totalsettlcount = 0;
                $scope.totalunsettlcount = 0;
                $scope.totalcash = 0;
                $scope.totalcard = 0;
                $scope.totalcredit = 0;
                $scope.totaladjustment = 0;
                $scope.totaldiscount = 0;
                $scope.totalreturn = 0;
                $scope.totalsettlementmismatch = 0;
                $scope.totalunsettle = 0;
                $.each(result, function (k, v) {
                    $scope.saleamount += v.billamount ;
                    $scope.totalamt += v.paid ;
                    $scope.totalcash += v.cash;
                    $scope.totalcard += v.card;
                    $scope.totalcredit += v.paid_credit;
                    $scope.totaladjustment += v.adjustment_paid;
                    $scope.totaldiscount += v.discount;
                    $scope.totalreturn += v.return_cash;
                    
                   
                    if (v.settlementno == null || v.settlementno == "") {
                        $scope.totalunsettlcount += 1;
                        $scope.totalunsettle += v.billamount;
                    }
                    else {
                        $scope.totalsettlcount += 1;
                        if (!v.issameday)
                            $scope.totalsettlementmismatch += v.paid;
                    }
                });

                result.push({
                    billno: 'TOTAL',
                    billamount: $scope.saleamount,
                    paid: $scope.totalamt,
                    cash: $scope.totalcash,
                    card: $scope.totalcard,
                    paid_credit: $scope.totalcredit,
                    adjustment_paid: $scope.totaladjustment,
                    discount: $scope.totaldiscount,
                    return_cash: $scope.totalreturn
                });
                $scope.SaleVsSettlementResult = result;

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

        $scope.SearchInward = function () {
            $scope.totalcount = 0;
            $scope.totalqty = 0;

            $scope.InwardResult = [];
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
            }).error(function (result) {
            });
        }

        $scope.SearchPriceChange = function () {
            $scope.totalcount = 0;
            $scope.totalqty = 0;

            $scope.PriceChangerList = [];
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
                url: "RetailReportService/SearchPriceChange",
                data: srequest
            }).success(function (result) {
                $scope.PriceChangerList = result;
                $scope.totalcount = result.length;
                
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
            }).error(function (result) {
            });
        }

        $scope.SearchStockAudit = function () {
            $scope.totalcount = 0;
            $scope.totalqty = 0;

            $scope.StockAuditList = [];
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
                url: "RetailReportService/SearchStockAudit",
                data: srequest
            }).success(function (result) {
                $scope.StockAuditList = result;
                $scope.totalcount = result.length;

                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
            }).error(function (result) {
            });
        }


        $scope.ViewItem = function (action, item) {
            if (action == "VIEW") {
                $scope.PriceChangeritem = item;
                $scope.printDiv('itemview', 'ITEM VIEW');
            } 
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
            }).error(function (result) {
            });
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
                url: "RetailReportService/SearchCancelledBill",
                data: srequest
            }).success(function (result) {
                $scope.CancelledBillResult = result;
                $scope.totalcash = 0;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $.each(result, function (k, v) {
                    $scope.totalcash += v.amount;
                });
                $scope.billcount = result.length;
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
                        return value.salevalue;
                    else
                        return value.taxvalue;
                }
                return "";
            }
            else {
                if (taxitem.type == 'VALUE')
                    return taxitem.salevalue;
                else
                    return taxitem.taxvalue;
            }
        };

        $scope.SearchSalesTaxSummary = function () {
            $scope.SalesTaxSummaryResult = [];
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
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                    return;
                }
                var vamount = 0.0;
                var vsgst = 0.0;
                var vcgst = 0.0;
                var vigst = 0.0;
                var vtaxamount = 0.0;
                var vtotal = 0.0;
                $.each(result, function (k, v) {
                    vamount += v.amount;
                    vsgst += v.sgst;
                    vcgst += v.cgst;
                    vigst += v.igst;
                    vtaxamount += v.taxamount;
                    vtotal += v.total;
                });              
                
                result.push({
                    taxpercentage: 'TOTAL',
                    amount : vamount,
                    sgst: vsgst,
                    cgst: vcgst,
                    igst: vigst,
                    taxamount: vtaxamount,
                    total: vtotal
                });
                $scope.SalesTaxSummaryResult = result;
            }).error(function (result) {
            });
        }

        $scope.SearchSalesTax = function () {
            $scope.SalesTaxResult = [];
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
            }).error(function (result) {
            });
        }
        $scope.SearchSalesBarcode = function () {
            $scope.SalesBarcodeResult = [];
            $scope.saleqty = 0;
            $scope.saletax = 0;
            $scope.salediscount = 0;
            $scope.saleamount = 0;

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
                $.each(result, function (k, v) {
                    $scope.saleqty += v.qty;
                    $scope.saletax += v.tax;
                    $scope.salediscount += v.taxableamount;
                    $scope.saleamount += v.receivable;
                });
            }).error(function (result) {
            });
        }
        $scope.SearchSalesman = function () {
            $scope.SalesmanResult = [];
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
            }).error(function (result) {
            });
        }
        $scope.SearchSalesAnalysis = function () {
            if (!$scope.SearchCondition.product && !$scope.SearchCondition.brand && !$scope.SearchCondition.material
                && !$scope.SearchCondition.pattern && !$scope.SearchCondition.style && !$scope.SearchCondition.size
                && !$scope.SearchCondition.supplier)
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
            }).error(function (result) {
            });
        }
        $scope.SearchBulkBill = function () {
            if ($scope.SearchCondition.locationid == 0 || $scope.SearchCondition.locationid == 'undefined') {
                $rootScope.Toaster.Info("Please select location");
                return;
            }
            //if ($scope.SearchCondition.showdetail && ($scope.SearchCondition.billno == "" && $scope.SearchCondition.barcode == "")) {
            //    $rootScope.Toaster.Info("Please enter billno");
            //    return;
            //}
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
            }).error(function (result) {
            });
        }

       

        $scope.SearchDeliveryStatus = function () {
            $scope.DeliveryResult = [];
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
                $scope.DeliveryResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.billcount = result.length;
                $.each(result, function (k, v) {
                    $scope.saleamount += v.amount;
                });
            }).error(function (result) {
            });
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

        $scope.ShowCustomerDialog = function () {
            var dialog = ngDialog.open({
                template: 'SearchCustomer',
                data: [$scope],
                width: '800px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {
                    $scope.PopupList = [];
                    $scope.Customer = null;
                    $scope.CloseIfOne = false;

                    $scope.Select = function () {
                        $scope.Customer = null;
                        $.each($scope.PopupList, function (k, v) {
                            if (v.selected)
                                $scope.Customer = v;
                        });
                        if ($scope.Customer != null)
                            $scope.closeThisDialog($scope.Customer);
                    };
                    $scope.Create = function () {
                        var request = {
                            "Item": {
                                'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno,
                                'gstno': $scope.gstno, 'hasgst': $scope.hasgst, 'loyeltyno': $scope.loyeltyno
                            },
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/Save",
                            data: srequest
                        }).success(function (result) {
                            $scope.closeThisDialog(result);
                        });
                    };

                    $scope.hasvalue = function (val) {
                        return typeof (val) != 'undefined' && val != null &&
                            val != '' && val.length > 0;
                    }

                    $scope.Search = function (closeIfOne) {

                        $scope.CloseIfOne = typeof (closeIfOne) != 'undefined' ? closeIfOne : false;
                        if (!$scope.hasvalue($scope.name) &&
                            !$scope.hasvalue($scope.area) &&
                            !$scope.hasvalue($scope.contactno) &&
                            !$scope.hasvalue($scope.gstno) &&
                            !$scope.hasvalue($scope.loyeltyno))
                            return;

                        var request = {
                            "Item": {
                                'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno, 'gstno': $scope.gstno,
                                'hasgst': $scope.hasgst, 'loyeltyno': $scope.loyeltyno
                            },
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/Search",
                            data: srequest
                        }).success(function (result) {
                            if (result.length == 1 && $scope.CloseIfOne) {
                                $scope.closeThisDialog(result[0]);
                            }
                            $scope.PopupList = result;
                        });
                    };

                    $timeout(function () {
                        $scope.setFocuscard_no();
                    }, 10);

                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape' && data.value.id > 0) {
                        $scope.SearchCondition.customerid = data.value.id;
                        $scope.SearchCondition.customername = data.value.name;
                    }
                }
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
                    list.push($rootScope.PrintFormater.Normal("_____________________________________________________"));
                    list.push($rootScope.PrintFormater.Normal("  LOCATION     : " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location)));
                    list.push($rootScope.PrintFormater.Normal("  DATE BETWEEN : " + $filter('date')($scope.SearchCondition.fromdate, 'dd-MM-yyyy') + " AND " + $filter('date')($scope.SearchCondition.todate, 'dd-MM-yyyy')));
                    list.push($rootScope.PrintFormater.Normal("_____________________________________________________"));
                    list.push($rootScope.PrintFormater.Normal("    DATE       TAX%     AMOUNT    IGST   SGST   CGST "));
                    list.push($rootScope.PrintFormater.Normal("_____________________________________________________"));

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

                        var str = $rootScope.PrintFormater.Align($filter('date')(v.billDate, 'dd-MM-yyyy'), 12, true);
                        str += $rootScope.PrintFormater.Align(v.taxpercentage.round(2), 8, true);
                        str += $rootScope.PrintFormater.Align(v.amount.round(2), 10, true);
                        str += $rootScope.PrintFormater.Align(v.igst.round(2), 8, true);
                        str += $rootScope.PrintFormater.Align(v.sgst.round(2), 8, true);
                        str += $rootScope.PrintFormater.Align(v.cgst.round(2), 8, true);
                        list.push($rootScope.PrintFormater.Normal(str));

                    });
                    var str = $rootScope.PrintFormater.Align(amount.round(2), 10, true);
                    str += $rootScope.PrintFormater.Align(sgst.round(2), 10, true);
                    str += $rootScope.PrintFormater.Align(cgst.round(2), 10, true);

                    list.push($rootScope.PrintFormater.Normal("_____________________________________________________"));
                    list.push($rootScope.PrintFormater.Normal("  Total          " + str));
                    list.push($rootScope.PrintFormater.Normal("_____________________________________________________"));
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

        $scope.exporttoExcel = function (tableid, title, removeactioncolumn) {
            if (tableid == "salesreportdet")
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'DETAILED SALES REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == "incentivetbl")
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'INCENTIVE REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'salesreport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'SALES REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company) + "-" + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
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
            if (tableid == 'salestaxreport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'SALES TAX REPORT FOR- ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'salestaxsummaryreport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'SALES TAX FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company) + " ) ");
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
            if (tableid == 'bulkbillreport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'BULK BILL REPORT FOR- ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'deliveryreport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'DELIVERY STATUS REPORT FOR- ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'missingstock')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'DIRECT STOCK ENTRY REPORT FOR- ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'stockmarker')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'STOCK MARKER REPORT FOR- ' + " ( " + $scope.SearchRef($scope.SearchCondition.markerid, $scope.MarkerType) + " ) ");
            if (tableid == 'pricechange')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'STOCK MARKER REPORT FOR- ' + " ( " + $scope.SearchRef($scope.SearchCondition.markerid, $scope.MarkerType) + " ) ");
            if (tableid == 'stockaudit')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'STOCK MARKER REPORT FOR- ' + " ( " + $scope.SearchRef($scope.SearchCondition.markerid, $scope.MarkerType) + " ) ");
            if (tableid == 'salesproductwise')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'PRODUCT WISE SALES REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");

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
            $scope.setFocusfirst();
        }, 10);
    }]);