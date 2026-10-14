
var app = angular.module('InfinityLab');

app.controller('LastBitBarcodeController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.BarcodeList = [];
        $scope.ItemList = [];
        $scope.SearchResult = [];
        $scope.SearchCondition = {
            "id": 0,
            "mode": 1,
            "locationid": 0,
            "barcode": "",
            "isbit": true
        };

        $scope.nsplitqty = 0;
        $scope.npdiscount = 0;

        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Location = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Location = results;
        });

        $scope.Search = function () {
            $scope.Item = null;
            if ($scope.SearchCondition.mode == 1)
                $scope.SearchCondition.locationid = 0;

            request = {
                "item": {
                    barcode: $scope.SearchCondition.barcode, mode: 9,
                    locationid: $scope.SearchCondition.locationid
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchBarcode",
                data: srequest
            }).success(function (result) {
                if (result != 'undefined') {
                    if ($scope.SearchCondition.mode == 2) {
                        result.rstock = 0;
                        result.adjstock = 0;
                        if (result.retailstock != null && result.retailstock.length > 0) {
                            result.rstock = result.retailstock[0].qty;
                            result.adjstock = result.retailstock[0].qty * -1;
                            result.qty = 1;
                            result.price = result.retailstock[0].price;
                            result.salablegoodsid = result.retailstock[0].salablegoodsid;
                            result.discount = result.retailstock[0].discount;
                        }
                    }
                    /*if (result.sellingmode == 1) {
                        $rootScope.Toaster.Error("Can't Split Unique barcode");
                    }
                    else if (result.wstock < 0 || result.rstock < 0) {
                        $rootScope.Toaster.Error("Negative Stock, Can't Split the barcode");
                    }
                    else if (result.wstock + result.rstock <= 0) {
                        $rootScope.Toaster.Error("No Stock, Can't Split the barcode");
                    }
                    else*/
                    {
                        result.adjstock = result.rstock * -1;
                        result.qty = 1;
                        $scope.ItemList.push(result);
                        $scope.SearchCondition.barcode = '';
                    }
                }
            }).error(function (result) {
            });
        }
        $scope.Save = function () {

            if ($rootScope.GetNumber($scope.SearchCondition.price) == 0) {
                $rootScope.Toaster.Info("Please Enter Price.");
                return;
            }
            var packcode = {
                'locationid': $scope.SearchCondition.locationid, 'hsncode': $scope.SearchCondition.hsncode,
                'productid': $scope.SearchCondition.productid, 'price': $scope.SearchCondition.price, 'noofpack': $scope.NoOfPacks,
                'discount': $scope.SearchCondition.discount, 'sourcelist': [], 'packlist': []
            };

            var haslocation = typeof ($scope.SearchCondition.locationid) != 'undefined' && $scope.SearchCondition.locationid != null
                && $scope.SearchCondition.locationid > 0;

            $.each($scope.ItemList, function (k, v) {
                packcode.sourcelist.push({
                    'barcode': v.barcode, 'productid': v.productid, 'stockid': v.stockid,
                    'qty': v.qty, 'id': haslocation ? v.salablegoodsid : v.stockid,
                    'salablegoodsid': v.salablegoodsid,
                    'adjstock': v.adjstock, 'companyid': v.companyid, 'itemid': v.itemid
                });
            });

            $.each($scope.PackItemList, function (k, v) {
                packcode.packlist.push({
                    'productid': v.productid,
                    'available': v.available,
                    'qty': v.qty
                });
            });

            var request = {
                "Item": packcode,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PackbarcodeService/CreateLastBits",
                data: srequest
            }).success(function (result) {
                $.each(result.barcodes, function (k, v) {
                    v.selected = true;
                    v.noofcopy = 1;
                    $scope.BarcodeList.push(v);
                });
            }).error(function (result) {
            });
        }

        $scope.SearchPackCode = function (searchbarcode) {
            var request = {
                "Item": { 'barcode': searchbarcode, sourcesearch: true },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PackbarcodeService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0) {
                    $scope.BarcodeList = [];
                    if (result[0].barcode.indexOf('$') == -1)
                        $scope.BarcodeList.push({ 'barcode': result[0].barcode, 'selected': true, 'noofcopy': 1 })
                }
                else
                    $scope.BarcodeList = [];
            }).error(function (result) {
            });
        }

        $scope.fileuploadedcomplete = function (value) {
            var barcodes = [];

            $.each(value.rows, function (k, v) {
                var barcode = v.values[0];
                if (barcode != null && barcode != '' && barcode.length > 0) {
                    var found = barcodes.find(function (item) {
                        return item == barcode;
                    });
                    if (typeof (found) == 'undefinied' || found == null) {
                        barcodes.push(barcode);
                    }
                }
            });

            $scope.Item = null;
            if ($scope.SearchCondition.mode == 1)
                $scope.SearchCondition.locationid = 0;

            request = {
                "item": {
                    barcodes: barcodes, mode: 9,
                    locationid: $scope.SearchCondition.locationid
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/SearchBarcodes",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0) {

                    $.each(result, function (k, v) {
                        if (v.retailstock != null && v.retailstock.length > 0) {
                            v.rstock = v.retailstock[0].qty;
                            v.adjstock = v.retailstock[0].qty * -1;
                            v.qty = 1;
                            v.price = v.retailstock[0].price;
                            v.salablegoodsid = v.retailstock[0].salablegoodsid;
                            v.discount = v.retailstock[0].discount;

                        }
                    })



                    $scope.ItemList = result;
                }
            }).error(function (result) {
            });
        }

        $scope.selectallbarcode = function () {
            $.each($scope.BarcodeList, function (k, v) {
                v.selected = $scope.selectallbarcode;
            });
        }

        $scope.PrintBarcode = function () {
            var ids = [];
            $.each($scope.BarcodeList, function (k, v) {
                if (v.selected)
                    ids.push({ 'barcode': v.barcode, 'stockid': v.stockid, 'itemid': v.itemid, 'noofcopy': v.noofcopy });
            });
            $scope.ChoseReport(ids);
        };

        $scope.ChoseReport = function (ids) {
            requestitem = { 'iswarehouse': ($scope.SearchCondition.mode == 1), 'locationid': $scope.SearchCondition.locationid, 'barcodes': ids, 'reportid': 0, 'companytitle': '' };

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
                        $scope.Sent2Printer(result, printdata);
                    }
                });
            });
        }

        $scope.NoOfPacks = 0;
        $scope.CanGenerate = true;
        $scope.QtyChanged = function () {
            $scope.CanGenerate = true;
            $.each($scope.PackItemList, function (k, v) {
                v.available = 0;
            });

            $.each($scope.ItemList, function (k, v) {
                var packitem = $scope.PackItemList.find(function (item) {
                    return item.productid == v.productid;
                });
                if (typeof (packitem) == 'undefined' || packitem == null) {
                    packitem = {
                        'productid': v.productid, 'productname': v.productname, 'available': 0, 'qty': 1
                    }
                    $scope.PackItemList.push(packitem);
                }
                packitem.available += parseFloat(v.qty);
            });

            var packexpected = Math.round($scope.PackItemList[0].available / $scope.PackItemList[0].qty, 0);
            $.each($scope.PackItemList, function (k, v) {
                if ((v.available / v.qty) != packexpected)
                    $scope.CanGenerate = false;
            });

            $scope.NoOfPacks = packexpected;
        }

        $scope.Sent2Printer = function (printdocument, printerData) {
            $rootScope.Sent2Printer(printdocument.join(';'), printerData);
            return;

            $.each(printdocument, function (k, v) {
                var request = {
                    'id': printerData.printerid,
                    'content': v,
                    'encoded': 'BASE64'
                };
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: printerData.service,
                    data: srequest
                }).success(function (result) {

                }).error(function (error) {

                });
            })
        };

        $scope.PackItemList = [];

        $scope.RemovePackItem = function (item) {
            $scope.PackItemList.splice($scope.PackItemList.indexOf(item), 1);
        }
        $scope.AddPackItem = function () {
            $scope.PackItemList.push({
                'productid': $scope.SearchCondition.productid, 'productname':
                    $scope.SearchRef($scope.SearchCondition.productid, $scope.ProductList, ''), 'qty': 1
            });
        }

        $scope.RemoveItem = function (item) {
            $scope.ItemList.splice($scope.ItemList.indexOf(item), 1);
        };

        $scope.Clear = function () {
            $scope.SearchResult = [];
            $scope.ItemList = [];
            $scope.SearchCondition.locationid = 0;
        }

        $scope.PriceChanged = function (type) {
            if (type == 'price' || type == 'discount') {
                var price = $rootScope.GetNumber($scope.SearchCondition.price);
                var discount = $rootScope.GetNumber($scope.SearchCondition.discount);
                $scope.SearchCondition.final = price - discount;
            }
            if (type == 'final') {
                var price = $rootScope.GetNumber($scope.SearchCondition.price);
                var final = $rootScope.GetNumber($scope.SearchCondition.final);
                $scope.SearchCondition.discount = price - final;
            }
        }

        $scope.system_invcol_pricetag = false;
        $scope.LoadAppsetting = function () {
            if ($rootScope.UserInfo && $rootScope.UserInfo.appsettings) {
                $scope.system_invcol_pricetag = $rootScope.UserInfo.appsettings.system_invcol_pricetag || false;
            }
            else {
                $timeout(function () {
                    $scope.LoadAppsetting();
                }, 30);
            }
        }

        $timeout(function () {
            $scope.LoadAppsetting();
            $scope.setFocusfirst();
        }, 10);

    }]);