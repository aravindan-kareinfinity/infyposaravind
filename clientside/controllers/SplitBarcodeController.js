
var app = angular.module('InfinityLab');

app.controller('SplitBarcodeController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', '$filter', 'localStorageService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, $filter, localStorageService) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.SupportRounding = true;
        $scope.ItemList = [];
        $scope.SearchResult = [];
        $scope.SplitBarcodeList = [];
        $scope.SearchCondition = {
            "id": 0,
            "mode": 1,
            "locationid": 0,
            "barcode": ""
        };
        $scope.SMOptions = [
            { 'id': '1', 'name': 'Add Only' },
            { 'id': '2', 'name': 'Remove Only' },
            { 'id': '3', 'name': 'Add & Print' },
            { 'id': '4', 'name': 'Remove & Print' }];

        $scope.SearchModes = [
            { 'id': '1', 'name': 'Warehouse' },
            { 'id': '2', 'name': 'Retail' },
            { 'id': '3', 'name': 'Billing Barcode' },
            { 'id': '4', 'name': 'Price Change' },
            { 'id': '5', 'name': 'Stock Marking' },
            { 'id': '6', 'name': 'Barcode Swap' },
            { 'id': '7', 'name': 'Split 2 Product' },
            { 'id': '8', 'name': 'Bulk Barcode Split' }
        ];

        $scope.nsplitqty = 0;
        $scope.npdiscount = 0;
        $scope.system_splitbitprice = false;
        $scope.bitprice = false;
        $scope.Location = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Location = results;

        });

        $scope.StockMarkers = restService.Get('ReferencelistService/MARKER', true).then(function (results) {
            $scope.StockMarkers = results;
        });

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });

        $scope.SearchByBarcode = function (barcode) {
            if ($scope.Price.mode == 2 && ($scope.Price.locationid == null || $scope.Price.locationid == 0)) {
                $rootScope.Toaster.Info("Please select retail location.");
                return;
            }
            $scope.Price.barcode = barcode;
            if ($scope.Price.mode == 2) {
                restService.Get('SalablegoodsService/ReturnBarcode/' + $scope.Price.locationid + '/' + $scope.Price.barcode).then(function (results) {
                    $scope.ItemList.push({
                        'printingname': results.sellingname, 'splitqty': (results.iqty - (results.dqty + results.oqty)), 'splitpiece': 1,
                        'barcode': results.barcode, 'oprice': results.price, 'price': results.price, 'discount': results.discount,
                        'nprice': results.price, 'odiscount': results.discount, 'pdiscount': (results.discount / results.price * 100).round(0),
                        'ndiscount': results.discount, 'stockqty': (results.iqty - (results.dqty + results.oqty)),
                        'salablegoodsid': results.id, 'edit': false, 'npdiscount': (results.discount / results.price * 100).round(0)
                    });
                });
            } else {
                restService.Get('StockService/Barcode/' + $scope.qEncodeURIComponent($scope.Price.barcode)).then(function (results) {
                    $scope.ItemList.push({
                        'printingname': results.printingname, 'splitqty': (results.qty - (results.damagedqty + results.transfered + results.returnqty)),
                        'barcode': results.barcode, 'oprice': results.price, 'price': results.price, 'discount': results.discount, 'splitpiece': 1,
                        'nprice': results.price, 'odiscount': results.discount, 'pdiscount': (results.discount / results.price * 100).round(0),
                        'ndiscount': results.discount, 'stockqty': (results.qty - (results.damagedqty + results.transfered + results.returnqty)),
                        'stockid': results.id, 'edit': false, 'npdiscount': (results.discount / results.price * 100).round(0)
                    });
                });
            }
        };


        $scope.qEncodeURIComponent = function (value) {
            if (value.indexOf('.') >= 0)
                value = value.replace('.', '~dot~');
            return encodeURIComponent(value);
        }

        $scope.SMSave = function () {
            if ($scope.billitem && $scope.billitem != null && $scope.billitem.stockid > 0) {
                $scope.billitem.actionmode = $scope.billitem.actionmode;

                if ($scope.CurrentBarcodePrinter == null) {
                    if ($scope.billitem.actionmode == '3' || $scope.billitem.actionmode == '4') {
                        $rootScope.ShowAlert('Printer', 'Please select printer & format');
                        return;
                    }
                }

                $scope.ItemList = [];
                $scope.ItemList.push($scope.billitem);
                $scope.Save(function (splitBarcode) {
                    $scope.LastPrintedBarcode = [];
                    $.each(splitBarcode[0].newbarcodelist.split(','), function (k, v) {
                        if (splitBarcode[0].barcode != v)
                            $scope.LastPrintedBarcode.push(v);
                    });
                    if ($scope.billitem.actionmode == '3' || $scope.billitem.actionmode == '4') {
                        $scope.PrintLastBarcode($scope.billitem.splitqty);
                    }
                });
            }
        }
        $scope.SMSearch = function () {
            $scope.Search(function (result) {
                result.actionmode = $scope.billitem.actionmode;
                result.stockmarkerid = $scope.billitem.stockmarkerid;
                $scope.billitem = result;
                $scope.billitem.splitpiece = 1;
                $scope.billitem.rstock = result.retailstock[0].qty;
                $scope.billitem.sellingprice = result.retailstock[0].price;
                $scope.billitem.salediscount = result.retailstock[0].discount;
                $scope.billitem.salablegoodsid = result.retailstock[0].salablegoodsid;
                $scope.billitem.salediscountpercentage = 0;
                if (result.salediscount > 0)
                    result.salediscountpercentage = ($rootScope.GetNumber($scope.billitem.salediscount) / $rootScope.GetNumber($scope.billitem.sellingprice) * 100).round(0);
                $scope.billitem.nprice = $scope.billitem.sellingprice;
                $scope.billitem.ndiscount = $scope.billitem.salediscount;
                $scope.billitem.npdiscount = $scope.billitem.salediscountpercentage;
                $scope.billitem.nfprice = $scope.billitem.nprice - $scope.billitem.ndiscount;
                $scope.billitem.splitpiece = 1;
                $scope.billitem.splitqty = '';
            });
        }

        $scope.ShowOption = false;
        $scope.smoption = '';
        $scope.optionchanged = function () {
            $scope.ShowOption = false;
            $scope.smoption = '';
            var selectedoption = $scope.SMOptions.find(function (item) {
                return item.id == $scope.billitem.actionmode;
            });
            if (selectedoption && selectedoption != null) {
                $scope.ShowOption = true;
                $scope.smoption = selectedoption.name;
            }
        }

        $scope.Swap = function () {
            var request = {
                "Item": {
                    'frombarcode': $scope.billitem,
                    'tobarcode': $scope.tbillitem,
                    'locationid': $scope.SearchCondition.locationid
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrinvoiceService/SwapBarcode",
                data: srequest
            }).success(function (result) {
                if (result) {
                    $rootScope.Toaster.Info("Done.");
                }
            }).error(function (result) {
            });
        }

        $scope.filehsnuploadedcomplete = function (value) {
            $scope.uploadfile = null;
            $scope.status = 'excel file importing.';
            $scope.HSNList = [];
            var supportedList = ["BARCODE", "QTY", "COLOUR", "SLEEVE", "PRODUCT", "BRAND", "SIZE", "PATTERN", "STYLE", "MATERIAL"]
            var supportedIndex = [-1, -1, -1, -1, -1, -1, -1]
            if (value.rows.length > 0) {
                var msg = [];
                $.each(value.rows[0].values, function (k, v) {
                    if (supportedList.indexOf(v.toUpperCase()) < 0) {
                        msg.push(v + ' not supported');
                    }
                    else {
                        supportedIndex[supportedList.indexOf(v.toUpperCase())] = k;
                    }
                })
                if (msg.length > 0) {
                    $rootScope.Toaster.Info(msg.join('\r\n'));
                    return;
                }
                if (supportedIndex[0] == -1) {
                    $rootScope.Toaster.Info('Barcode column missing');
                    return;
                }
                if (supportedIndex[1] == -1) {
                    $rootScope.Toaster.Info('Qty column missing');
                    return;
                }
            }

            $scope.BulkSplitRows = [];
            var uniquebarcodes = [];
            $.each(value.rows, function (k, v) {
                if (k == 0) return;
                var row = {};
                row.barcode = v.values[supportedIndex[0]];
                row.qty = parseFloat(v.values[supportedIndex[1]]);
                var supportedList = ["BARCODE", "QTY", "COLOUR", "SLEEVE", "PRODUCT", "BRAND", "SIZE", "PATTERN", "STYLE", "MATERIAL"]
                row.colour = supportedIndex[2] == -1 ? '' : v.values[supportedIndex[2]];
                row.sleeve = supportedIndex[3] == -1 ? '' : v.values[supportedIndex[3]];
                row.product = supportedIndex[4] == -1 ? '' : v.values[supportedIndex[4]];
                row.brand = supportedIndex[5] == -1 ? '' : v.values[supportedIndex[5]];
                row.size = supportedIndex[6] == -1 ? '' : v.values[supportedIndex[6]];
                row.pattern = supportedIndex[7] == -1 ? '' : v.values[supportedIndex[7]];
                row.style = supportedIndex[8] == -1 ? '' : v.values[supportedIndex[8]];
                row.material = supportedIndex[9] == -1 ? '' : v.values[supportedIndex[9]];
                var eitem = uniquebarcodes.find(function (item) {
                    return item.barcode == row.barcode;
                });
                if (eitem == undefined || eitem == null) {
                    eitem = { 'barcode': row.barcode, 'id': 0 };
                    uniquebarcodes.push(eitem);
                }
                eitem.id += 1;
                row.nbarcode = row.barcode + '-' + eitem.id;
                $scope.BulkSplitRows.push(row);
            })

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '300px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.BulkSplitRows);
            igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            igrid.AddSimpleColumn('nbarcode', 'New Barcode', 100);

            igrid.AddFormatedColumn('qty', 'Qty', 80, 'N2', 2);
            if (supportedIndex[4] != -1)
                igrid.AddSimpleColumn('product', 'Product', 150);
            if (supportedIndex[5] != -1)
                igrid.AddSimpleColumn('brand', 'Brand', 150);
            if (supportedIndex[2] != -1)
                igrid.AddSimpleColumn('colour', 'Colour', 150);
            if (supportedIndex[6] != -1)
                igrid.AddSimpleColumn('size', 'Size', 150);
            if (supportedIndex[8] != -1)
                igrid.AddSimpleColumn('style', 'Style', 150);
            if (supportedIndex[7] != -1)
                igrid.AddSimpleColumn('pattern', 'Pattern', 150);
            if (supportedIndex[9] != -1)
                igrid.AddSimpleColumn('material', 'Material', 150);
            //var stockcolumn = igrid.AddSimpleColumn('copy', 'Copy', 60);
            //stockcolumn.editor = Slick.Editors.NumberEditor;
            //igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.BulkSplitRows);
        };

        $scope.BulkSplitProduct = function () {
            $scope.CanPrintBulk = false;
            var request = {
                "Item": {
                    'companyid': $scope.SearchCondition.companyid, 'locationid': $scope.SearchCondition.locationid,
                    'items': $scope.BulkSplitRows
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrinvoiceService/BulkSplit",
                data: srequest
            }).success(function (result) {
                if (result && result.length > 0) {
                    $scope.CanPrintBulk = true;
                    $rootScope.Toaster.Info("Done.");

                    if ($scope.igrid != null)
                        $scope.igrid.destroy();

                    var igrid = new InfyGrid('gridContainer', '300px', '100%', true, true, true, 0);
                    igrid.options.showHeaderRow = true;
                    $scope.BulkSplitRows = result
                    $.each(result, function (k, v) {
                        v.oldbarcode = v.barcode;
                        v.barcode = v.nbarcode;
                    })
                    igrid.EnableSerial($scope.BulkSplitRows);
                    igrid.AddSimpleColumn('barcode', 'Barcode', 100);
                    igrid.AddSimpleColumn('oldbarcode', 'OLD Barcode', 100);
                    igrid.AddFormatedColumn('qty', 'Qty', 80, 'N2', 2);
                    var stockcolumn = igrid.AddSimpleColumn('copy', 'Copy', 60);
                    stockcolumn.editor = Slick.Editors.NumberEditor;
                    igrid.AddSimpleColumn('product', 'Product', 150);
                    igrid.AddSimpleColumn('size', 'Size', 100);
                    igrid.AddSimpleColumn('brand', 'Brand', 150);
                    igrid.AddSimpleColumn('colour', 'Colour', 150);
                    igrid.AddSimpleColumn('style', 'Style', 150);
                    igrid.AddSimpleColumn('pattern', 'Pattern', 150);
                    igrid.AddSimpleColumn('material', 'Material', 150);
                    igrid.options.autoEdit = true;
                    igrid.Initialize();
                    $scope.igrid = igrid;
                    $scope.igrid.UpdateData($scope.BulkSplitRows);
                }
            }).error(function (result) {
            });
        }

        $scope.Search = function (callback, barcode) {

            if (typeof ($scope.SearchCondition.barcode) == 'undefined' ||
                $scope.SearchCondition.barcode == null ||
                $scope.SearchCondition.barcode == '') return;

            $scope.Item = null;
            if ($scope.SearchCondition.mode == 1)
                $scope.SearchCondition.locationid = 0;

            request = {
                "item": {
                    barcode: typeof (barcode) == 'undefined' || barcode == null || barcode == '' ? $scope.SearchCondition.barcode : barcode,
                    mode: 9,
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
                if (typeof (callback) != 'undefined' && callback != null) {
                    callback(result);
                    return;
                }
                if (result != 'undefined') {
                    result.rstock = 0;
                    result.salablegoodsid = 0;
                    if (result.sellingmode == 1 && $rootScope.GetNumber(result.retailstock[0].qty) <= 1) {
                        $rootScope.Toaster.Error("Can't Split Unique barcode");
                        return;
                    }
                    if ($scope.SearchCondition.mode == 3) {
                        $scope.billitem = result;
                        $scope.billitem.isbit = true;
                        $scope.billitem.splitpiece = 1;

                        $scope.billitem.rstock = result.retailstock[0].qty;
                        $scope.billitem.sellingprice = result.retailstock[0].price;
                        $scope.billitem.salediscount = result.retailstock[0].discount;
                        $scope.billitem.salablegoodsid = result.retailstock[0].salablegoodsid;
                        $scope.billitem.salediscountpercentage = 0;
                        if (result.salediscount > 0)
                            result.salediscountpercentage = ($rootScope.GetNumber($scope.billitem.salediscount) / $rootScope.GetNumber($scope.billitem.sellingprice) * 100).round(0);
                        $scope.billitem.nprice = $scope.billitem.sellingprice;
                        $scope.billitem.ndiscount = $scope.billitem.salediscount;
                        $scope.billitem.npdiscount = $scope.billitem.salediscountpercentage;
                        $scope.billitem.nfprice = $scope.billitem.nprice - $scope.billitem.ndiscount;
                        $scope.billitem.splitpiece = 1;
                        $scope.billitem.splitqty = '';
                        $scope.setFocusbillsplitqty();
                        return;
                    } else if ($scope.SearchCondition.mode == 4 || $scope.SearchCondition.mode == 5) {
                        $scope.billitem = result;
                        $scope.billitem.splitpiece = 1;
                        $scope.billitem.rstock = result.retailstock[0].qty;
                        $scope.billitem.locationid = result.retailstock[0].locationid;
                        $scope.billitem.companyid = result.retailstock[0].companyid;
                        $scope.billitem.sellingprice = result.retailstock[0].price;
                        $scope.billitem.salediscount = result.retailstock[0].discount;
                        $scope.billitem.salablegoodsid = result.retailstock[0].salablegoodsid;
                        $scope.billitem.entrydate = result.entrydate;
                        $scope.billitem.salediscountpercentage = 0;
                        if (result.salediscount > 0)
                            result.salediscountpercentage = ($rootScope.GetNumber($scope.billitem.salediscount) / $rootScope.GetNumber($scope.billitem.sellingprice) * 100).round(0);
                        $scope.billitem.nprice = $scope.billitem.sellingprice;
                        $scope.billitem.ndiscount = $scope.billitem.salediscount;
                        $scope.billitem.npdiscount = $scope.billitem.salediscountpercentage;
                        $scope.billitem.nfprice = $scope.billitem.nprice - $scope.billitem.ndiscount;
                        $scope.billitem.splitpiece = 1;
                        $scope.billitem.splitqty = '';
                        $scope.setFocusnmargin();
                        return;
                    }
                    else if ($scope.SearchCondition.mode == 2 || $scope.SearchCondition.mode == 7) {
                        if (result.retailstock != null && result.retailstock.length > 0) {
                            result.rstock = result.retailstock[0].qty;
                            result.locationid = result.retailstock[0].locationid;
                            result.sellingprice = result.retailstock[0].price;
                            result.salediscount = result.retailstock[0].discount;
                            result.salablegoodsid = result.retailstock[0].salablegoodsid;
                            result.salediscountpercentage = 0;
                            if (result.salediscount > 0)
                                result.salediscountpercentage = ($rootScope.GetNumber(result.salediscount) / $rootScope.GetNumber(result.sellingprice) * 100).round(0);

                            result.nprice = result.sellingprice;
                            result.ndiscount = result.salediscount;
                            result.npdiscount = result.salediscountpercentage;
                            result.nfprice = result.nprice - result.ndiscount;
                            result.splitpiece = 1;
                        }
                    }


                    if ($scope.SearchCondition.mode == 1 && result.wstock < 0) {
                        $rootScope.Toaster.Error("Negative Stock, Can't Split the barcode");
                    }
                    else if ($scope.SearchCondition.mode == 2 && result.rstock < 0) {
                        $rootScope.Toaster.Error("Negative Stock, Can't Split the barcode");
                    }
                    else {
                        if ($scope.SearchCondition.mode == 1) {
                            result.nprice = result.sellingprice;
                            result.ndiscount = result.salediscount;
                            result.npdiscount = result.salediscountpercentage;
                            result.nfprice = result.nprice - result.ndiscount;
                            result.splitpiece = 1;
                        }
                        result.isbit = result.sellingmode == 3;
                        if (result.isbit && $scope.system_splitbitprice)
                            $scope.bitprice = true;
                        else
                            $scope.bitprice = false;

                        $scope.ItemList = [];
                        $scope.ItemList.push(result);
                        $scope.SearchCondition.barcode = '';
                    }
                }
            }).error(function (result) {
            });
        }

        $scope.discountchanged = function () {
            var npdiscount = parseFloat($scope.billitem.npdiscount);
            if (isNaN(npdiscount)) npdiscount = 0;
            $scope.billitem.ndiscount = Math.round(($scope.billitem.nprice * npdiscount / 100));
        }

        $scope.marginchanged = function () {
            var nmargin = parseFloat($scope.billitem.nmargin);
            if (isNaN(nmargin)) nmargin = 0;
            $scope.billitem.nprice = $scope.RoundPrice(Math.round($scope.billitem.sellingprice + ($scope.billitem.sellingprice * nmargin / 100)));
        }

        $scope.LastPrintedBarcode = null;

        $scope.PrintSwapBarcodes = function () {
            $scope.LastPrintedBarcode = [$scope.billitem.barcode, $scope.tbillitem.barcode];
            $scope.PrintLastBarcode(1);
        }

        $scope.PrintLastBarcode = function (noofcopy) {
            if (isNaN(noofcopy)) noofcopy = 1;
            var ids = [];
            $.each($scope.LastPrintedBarcode, function (k, v) {
                ids.push({ 'noofcopy': noofcopy, 'barcode': v });
            });

            requestitem = {
                'iswarehouse': false,
                'locationid': $scope.SearchCondition.locationid,
                'barcodes': ids,
                'reportid': 0
            };

            requestitem.reportid = $scope.CurrentBarcodePrinter.report.id;

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
                    $scope.Sent2Printer(result, $scope.CurrentBarcodePrinter);
                    $scope.SearchCondition.barcode = "";
                    $scope.billitem = {};
                    $scope.setFocustxtbarcode();
                }
            });
        }
        $scope.SaveAndPrint = function (isprint) {
            if ($scope.CurrentBarcodePrinter == null) {
                $rootScope.ShowAlert('Printer', 'Please select printer & format');
                return;
            }

            if ($scope.billitem == 'undefined' || $scope.billitem == null ||
                $scope.billitem.barcode == '' || $rootScope.GetNumber($scope.billitem.sellingprice) == 0) {
                $rootScope.Toaster.Error('Please enter valid barcode and selling price');
                return;
            }

            if ($scope.SearchCondition.mode != '5') {
                if ($scope.billitem.splitqty.toString().indexOf('-') > 0) {
                    var noofpieces = $scope.billitem.splitqty.toString().split('-');
                    $scope.billitem.splitpiece = parseInt(noofpieces[1]);
                    $scope.billitem.splitqty = parseFloat(noofpieces[0]);
                }
                else {
                    $scope.billitem.splitqty = parseFloat($scope.billitem.splitqty);
                }
                if (isNaN($scope.billitem.splitqty) || isNaN($scope.billitem.splitpiece)) {
                    $scope.billitem.splitqty = '';
                    $scope.setFocusbillsplitqty();
                    return;
                }
                if ($scope.SearchCondition.mode == '3') {
                    $scope.billitem.sellingprice = $scope.billitem.nprice;
                    $scope.billitem.salediscount = $scope.billitem.ndiscount;
                    $scope.billitem.salediscountpercentage = $scope.billitem.npdiscount;
                }
                else if ($scope.SearchCondition.mode != '4') {
                    $scope.billitem.nprice = $scope.billitem.sellingprice;
                    $scope.billitem.ndiscount = $scope.billitem.salediscount;
                    $scope.billitem.npdiscount = $scope.billitem.salediscountpercentage;
                    $scope.billitem.nfprice = $scope.billitem.nprice - $scope.billitem.ndiscount;
                }
            }

            $scope.ItemList = [];
            $scope.ItemList.push($scope.billitem);
            $scope.Save(function (splitBarcode) {
                $scope.LastPrintedBarcode = [];
                $.each(splitBarcode[0].newbarcodelist.split(','), function (k, v) {
                    if (splitBarcode[0].barcode != v)
                        $scope.LastPrintedBarcode.push(v);
                });
                if (isprint) {
                    if ($scope.SearchCondition.mode == '4' &&
                        $scope.billitem.sellingmode == 2) {
                        $scope.PrintLastBarcode($scope.billitem.splitqty);
                    }
                    else {
                        $scope.PrintLastBarcode();
                    }
                }
                else {
                    $scope.billitem = {};
                    $scope.SearchCondition.barcode = '';
                    $scope.setFocustxtbarcode();
                }
            });
        }

        $scope.RoundPrice = function (amount) {

            if (!$scope.SupportRounding) return Math.round(amount, 0).toFixed(0);

            if (amount < 100) {
                amount = Math.round(amount, 0);
            }

            if (amount > 100 && amount < 1000) {
                amount = Math.round(amount, 0);
                var diff = amount - (Math.floor(amount / 10) * 10);
                if (diff > 5) {
                    if (diff - 5 >= 2.5) {
                        amount = (Math.floor(amount / 10) * 10) + 10;
                    } else {
                        amount = (Math.floor(amount / 10) * 10) + 5;
                    }
                } else {
                    if (diff >= 2.5) {
                        amount = (Math.floor(amount / 10) * 10) + 5;
                    } else {
                        amount = (Math.floor(amount / 10) * 10);
                    }
                }

            } else if (amount > 1000) {
                amount = Math.round(amount, 0);
                var diff = amount - (Math.floor(amount / 10) * 10);
                if (diff > 5) {
                    amount = (Math.floor(amount / 10) * 10) + 10;
                } else {
                    amount = (Math.floor(amount / 10) * 10);
                }
            }
            return amount;
        }

        $scope.SearchStock = function (isto) {
            $scope.Search(function (result) {
                if (isto) {
                    $scope.tbillitem = result;
                    $scope.tbillitem.rstock = result.retailstock[0].qty;
                    $scope.tbillitem.sellingprice = result.retailstock[0].price;
                    $scope.tbillitem.salediscount = result.retailstock[0].discount;
                    $scope.tbillitem.salablegoodsid = result.retailstock[0].salablegoodsid;
                    $scope.tbillitem.salediscountpercentage = 0;
                    if (result.salediscount > 0)
                        result.salediscountpercentage = ($rootScope.GetNumber($scope.tbillitem.salediscount) / $rootScope.GetNumber($scope.tbillitem.sellingprice) * 100).round(0);
                    $scope.tbillitem.nprice = $scope.tbillitem.sellingprice;
                    $scope.tbillitem.ndiscount = $scope.tbillitem.salediscount;
                    $scope.tbillitem.npdiscount = $scope.tbillitem.salediscountpercentage;
                    $scope.tbillitem.nfprice = $scope.tbillitem.nprice - $scope.tbillitem.ndiscount;
                }
                else {
                    $scope.billitem = result;
                    $scope.billitem.rstock = result.retailstock[0].qty;
                    $scope.billitem.sellingprice = result.retailstock[0].price;
                    $scope.billitem.salediscount = result.retailstock[0].discount;
                    $scope.billitem.salablegoodsid = result.retailstock[0].salablegoodsid;
                    $scope.billitem.salediscountpercentage = 0;
                    if (result.salediscount > 0)
                        result.salediscountpercentage = ($rootScope.GetNumber($scope.billitem.salediscount) / $rootScope.GetNumber($scope.billitem.sellingprice) * 100).round(0);
                    $scope.billitem.nprice = $scope.billitem.sellingprice;
                    $scope.billitem.ndiscount = $scope.billitem.salediscount;
                    $scope.billitem.npdiscount = $scope.billitem.salediscountpercentage;
                    $scope.billitem.nfprice = $scope.billitem.nprice - $scope.billitem.ndiscount;
                }
            }, isto ? $scope.SearchCondition.tbarcode : null);
        }

        $scope.bitpricechanged = function (value) {
            $scope.bitprice = value;
        }
        $scope.Split2Product = function () {
            var sbarcode = '';
            var product = $scope.ProductList.find(function (item) {
                return item.id == $scope.ItemList[0].changeproductid;
            });

            $.each($scope.ItemList, function (k, v) {
                v.mode = $scope.SearchCondition.mode;
                v.locationid = $scope.SearchCondition.locationid;
                if (product != null)
                    v.productname = product.name;
                sbarcode = v.barcode;
            });

            var requestitems = (JSON.parse(JSON.stringify($scope.ItemList)));
            $.each(requestitems, function (k, v) {
                v.nprice = (v.nprice / v.splitqty * 1).round(2);
                v.ndiscount = (v.ndiscount / v.splitqty * 1).round(2);
                v.nfprice = (v.nfprice / v.splitqty * 1).round(2);
            });
            var requestitems = (JSON.parse(JSON.stringify($scope.ItemList)));
            var request = {
                "Items": requestitems,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrinvoiceService/ConvertItemList",
                data: srequest
            }).success(function (result) {
                if (result.Success) {
                    $rootScope.Toaster.Info("Done.");
                }
                if (typeof (callback) != 'undefined') {
                    callback(result.Items);
                }
                else {
                    $scope.ItemList = result.Items;
                    $scope.LoadSplitBarcodes(sbarcode);
                }
            }).error(function (result) {
            });
        }
        $scope.Save = function (callback) {
            var error = false;
            var sbarcode = "";
            if ($scope.SearchCondition.mode != 5) {
                $.each($scope.ItemList, function (k, v) {
                    v.mode = $scope.SearchCondition.mode;
                    v.actionmode = 5;
                    if ($scope.SearchCondition.mode == 1) {
                        v.locationid = 0;
                        if (typeof (v.splitqty) == 'undefined' || v.splitqty == 0) {
                            $rootScope.Toaster.Info("Please enter split qty.");
                            error = true;
                        }
                        else if (v.wstock < v.splitqty) {
                            $rootScope.Toaster.Info("Split qty must be less then the warehouse stock");
                            error = true;
                        }
                    }
                    else {
                        v.locationid = $scope.SearchCondition.locationid;
                        if (typeof (v.splitqty) == 'undefined' || v.splitqty == 0) {
                            $rootScope.Toaster.Info("Please enter split qty.");
                            error = true;
                        }
                        else if (v.rstock < v.splitqty) {
                            $rootScope.Toaster.Info("Split qty must be less then the selected location stock");
                            error = true;
                        }
                    }
                    v.changeproductid = 0;
                    if (typeof ($scope.ChangeProduct) != 'undefined' && typeof ($scope.productid) != null &&
                        $scope.ChangeProduct && $scope.productid > 0) {
                        v.changeproductid = $scope.productid;
                        var product = $scope.ProductList.find(function (item) {
                            return item.id == v.changeproductid;
                        });
                        if (product != null)
                            v.productname = product.name;
                    }
                    sbarcode = v.barcode;
                });
                if (error) return;
            }
            else {
                $.each($scope.ItemList, function (k, v) {
                    v.mode = $scope.SearchCondition.mode;
                    v.actionmode = $scope.billitem.actionmode;
                });
            }

            var requestitems = (JSON.parse(JSON.stringify($scope.ItemList)));
            if ($scope.bitprice && $scope.system_splitbitprice) {
                $.each(requestitems, function (k, v) {
                    v.nprice = (v.nprice / v.splitqty * 1).round(2);
                    v.ndiscount = (v.ndiscount / v.splitqty * 1).round(2);
                    v.nfprice = (v.nfprice / v.splitqty * 1).round(2);
                });
            }

            var request = {
                "Items": requestitems,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrinvoiceService/ConvertItemList",
                data: srequest
            }).success(function (result) {
                if (result.Success) {
                    $rootScope.Toaster.Info("Done.");
                }
                if (typeof (callback) != 'undefined') {
                    callback(result.Items);
                }
                else {
                    $scope.ItemList = result.Items;
                    $scope.LoadSplitBarcodes(sbarcode);
                }
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
                if (result != null && result.length > 0)
                    $scope.ItemList = result;
            }).error(function (result) {
            });
        }

        $scope.ChangePrice = function (item, action) {
            var nprice = $rootScope.GetNumber(item.nprice);
            var ndiscount = $rootScope.GetNumber(item.ndiscount);
            var npdiscount = $rootScope.GetNumber(item.npdiscount);
            var nfprice = $rootScope.GetNumber(item.nfprice);
            if (action == 4) {
                item.ndiscount = nprice - nfprice;
                item.npdiscount = ($rootScope.GetNumber(item.ndiscount) / $rootScope.GetNumber(item.nprice) * 100).round(0);
            }
            else {
                if (action == 1 && npdiscount > 0) {
                    item.ndiscount = ($rootScope.GetNumber(item.nprice) * $rootScope.GetNumber(item.npdiscount) / 100).round(0);
                } else if (action == 2) {
                    item.npdiscount = ($rootScope.GetNumber(item.ndiscount) / $rootScope.GetNumber(item.nprice) * 100).round(0);
                }
                else if (action == "3") {
                    item.ndiscount = ($rootScope.GetNumber(item.nprice) * $rootScope.GetNumber(item.npdiscount) / 100).round(0);
                }
                item.nfprice = nprice - item.ndiscount;
            }
        }
        $scope.ChangeBillingPrice = function (item, action) {
            var nprice = $rootScope.GetNumber(item.nprice) * $rootScope.GetNumber(item.splitqty);
            var ndiscount = $rootScope.GetNumber(item.ndiscount);
            var npdiscount = $rootScope.GetNumber(item.npdiscount);
            var nfprice = $rootScope.GetNumber(item.nfprice);
            if (action == 4) {
                item.ndiscount = nprice - nfprice;
                item.npdiscount = ($rootScope.GetNumber(item.ndiscount) / $rootScope.GetNumber(item.nprice) * 100).round(0);
            }
            else {
                if (action == 1 && npdiscount > 0) {
                    item.ndiscount = ($rootScope.GetNumber(item.nprice) * $rootScope.GetNumber(item.npdiscount) / 100).round(0);
                } else if (action == 2) {
                    item.npdiscount = ($rootScope.GetNumber(item.ndiscount) / $rootScope.GetNumber(item.nprice) * 100).round(0);
                }
                else if (action == "3") {
                    item.ndiscount = ($rootScope.GetNumber(item.nprice) * $rootScope.GetNumber(item.npdiscount) / 100).round(0);
                }
            }
        }

        $scope.compute = function (item, type) {
            if (type == 'discountpercent' || type == 'price')
                item.ndiscount = ($rootScope.GetNumber(item.nprice) * $rootScope.GetNumber(item.npdiscount) / 100).round(0);
            else if (type == 'discount')
                item.npdiscount = ($rootScope.GetNumber(item.ndiscount) / $rootScope.GetNumber(item.nprice) * 100).round(0);
        };

        $scope.LoadSplitBarcodes = function (_barcode) {
            var BarcodeList = [];
            $.each($scope.ItemList, function (k, v) {
                BarcodeList.push({ 'id': v.lrinvoiceitemid, 'productid': v.productid, 'qty': v.splitqty, 'code': v.barcodes, 'name': v.productname, 'sno': 0 });
            });
            request = {
                "Items": BarcodeList,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrinvoiceService/SearchBarcode",
                data: srequest
            }).success(function (result) {
                $.each(result, function (k, v) {
                    if (v.code != _barcode) {
                        $scope.SplitBarcodeList.push({
                            'barcode': v.code,
                            'id': v.id,
                            'name': v.name,
                            'qty': v.qty,
                            'copy': v.sellingmode == 3 ? 1 : v.qty,
                            'sellingmode': v.sellingmode
                        });
                    }
                });
                $scope.UpdateSplitList();
            }).error(function (result) {
            });

        }

        $scope.UpdateSplitList = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '300px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SplitBarcodeList);

            igrid.AddSimpleColumn('name', 'Product Name', 150);
            igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            igrid.AddFormatedColumn('qty', 'Qty', 80, 'N2', 2);
            var stockcolumn = igrid.AddSimpleColumn('copy', 'Copy', 60);
            stockcolumn.editor = Slick.Editors.NumberEditor;

            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SplitBarcodeList);
        }

        $scope.SearchHistory = function () {
            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "SplitbarcodehistoryService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchGrid(result);
            }).error(function (result) {
            });
        }

        $scope.SearchGrid = function (splitbarcodehistory) {
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();

            var igrid = new InfyGrid('sgridContainer', '300px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial(splitbarcodehistory);
            igrid.AddSimpleColumn('createdemployee', 'Created By', 100);
            igrid.AddFormatedColumn('createdon', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('sourceproductid', 'Product', 100, $scope.ProductList);
            igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            igrid.AddSimpleColumn('newbarcode', 'New Barcode(Last)', 100);
            igrid.AddFormatedColumn('splitqty', 'Split Qty', 80, 'N2', 2);
            igrid.AddFormatedColumn('noofqtyeachbarcode', 'New Qty in each barcode', 100, 'N2', 2);
            igrid.AddFormatedColumn('noofnewbarcode', 'No of new barcode', 100, 'N2', 2);
            igrid.AddFormatedColumn('newbarcodecount', 'Count', 50, 'N2', 0);

            igrid.AddCustomColumn('price', 'Price', 80, function (item) {
                return item.newprice > 0 && item.price != item.newprice ? ($filter('IndianNumber')(item.price, 2) + '=>' + $filter('IndianNumber')(item.newprice, 2)) : ($filter('IndianNumber')(item.price, 2));
            });
            igrid.AddCustomColumn('discount', 'Discount', 80, function (item) {
                return item.newdiscount > 0 && item.discount != item.newdiscount ? ($filter('IndianNumber')(item.discount, 2) + '=>' + $filter('IndianNumber')(item.newdiscount, 2)) : ($filter('IndianNumber')(item.discount, 2));
            });
            igrid.AddSimpleColumn('barcodelist', 'New barcodes', 160);
            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Print'/> &nbsp;"

            igrid.AddButtonColumn('action', 'ACTION', 85, commands, function (e, args) {
                $scope.$apply(function () {
                    $scope.PrintSelected(args.item);
                });
            });

            igrid.Initialize();
            $scope.sgrid = igrid;
            $scope.sgrid.UpdateData(splitbarcodehistory);
        }

        $scope.ClearPrinterSelection = function () {
            $scope.CurrentBarcodePrinter = null;
        }

        $scope.PrintSelected = function (item) {
            if ($scope.CurrentBarcodePrinter == null) {
                $scope.SelectPrinter();
            }

            $scope.LastPrintedBarcode = [];
            $.each(item.barcodelist, function (k, v) {
                if (k > 0)
                    $scope.LastPrintedBarcode.push(v);
            })
            if (item.isbit)
                $scope.PrintLastBarcode(item.noofnewbarcode);
            else
                $scope.PrintLastBarcode(item.noofqtyeachbarcode);
        }

        $scope.PrintPriceChangeBarcode = function () {
            var ids = [];
            var _selectindex = 1;
            var seletedRows = $scope.sgrid.GetSelectedRows();
            $.each(seletedRows, function (k, v) {
                $scope.LastPrintedBarcode = [];

                $.each(v.barcodelist, function (kk, vv) {
                    if (v.barcode != vv)
                        $scope.LastPrintedBarcode.push(vv);
                });

                $.each($scope.LastPrintedBarcode, function (kk, vv) {
                    ids.push({
                        'noofcopy': (v.isbit ? v.noofnewbarcode : v.noofqtyeachbarcode), 'barcode': vv, 'selectindex': _selectindex++
                    });
                });

                //    ids.push({
                //        'barcode': v.newbarcode,
                //        'noofcopy': (v.splitqty == 'undefined' || v.splitqty == null) ? 1 : v.splitqty,
                //        'selectindex': _selectindex++
                //    });
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
                        $scope.Sent2Printer(result, printdata);
                    }
                });
            });
        }

        $scope.PrintAllBarcode = function () {
            var ids = [];

            var seletedRows = this.igrid.GetSelectedRows();
            $.each(seletedRows, function (k, v) {
                ids.push({
                    'barcode': v.barcode,
                    'noofcopy': (v.copy == 'undefined' || v.copy == null) ? 1 : v.copy
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
                        $scope.Sent2Printer(result, printdata);
                    }
                });
            });
        };

        $scope.PrintBarcode = function (item, isdirect, lrinvoiceitemid) {

            var requestitem = {
                'reportid': 0, 'companyid': item.companyid,
                'companytitle': item.company, 'lrid': item.lrid,
                'lrno': item.lrno, 'lrentryno': item.lrentryno,
                'items': []
            };
            var BarcodeList = [];

            requestitem.items.push(item.lrinvoiceitemid);
            if (item.productname == null)
                item.productname = $scope.SearchRef(item.productid, $scope.ProductList)
            BarcodeList.push({ 'id': item.lrinvoiceitemid, 'productid': item.productid, 'qty': item.splitqty, 'code': item.barcodes, 'name': item.productname, 'sno': 0 });

            if (requestitem.items.length == 0) {
                $rootScope.ShowAlert('Print', 'Please select items to Print');
                return;
            }

            request = {
                "Items": BarcodeList,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrinvoiceService/SearchBarcode",
                data: srequest
            }).success(function (result) {
                $scope.ShowBarcodeList(requestitem, result);
            }).error(function (result) {
            });
        }
        $scope.ShowBarcodeList = function (requestitem, BarcodeList) {
            var dialog = ngDialog.open({
                template: 'ChooseBarecodes',
                data: {
                    'requestitem': requestitem,
                    'BarcodeList': BarcodeList
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
                            v.copy = 2;
                        else if (v.sellingmode == 2)
                            v.copy = v.qty;
                        else
                            v.copy = 1;
                    });

                    $scope.SetCopy = function () {
                        $.each($scope.ngDialogData.BarcodeList, function (k, v) {
                            if (v.selected == true) {
                                v.copy = $scope.fixcopy;
                            }
                        });
                    };

                    $scope.SetSelection = function () {
                        var range = $scope.fixno.split('-');
                        if (range.length != 2) return;
                        var from = parseInt(range[0]);
                        var to = parseInt(range[1]);
                        if (to < from)
                            to = from + to;

                        $.each($scope.ngDialogData.BarcodeList, function (k, v) {
                            v.selected = v.no >= from && v.no <= to;
                        });
                    }

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

        $scope.Apply = function (action) {
            var nsplitqty = $rootScope.GetNumber($scope.nsplitqty);
            var npdiscount = $rootScope.GetNumber($scope.npdiscount);

            $.each($scope.ItemList, function (k, v) {
                if (action == "1") {
                    v.splitqty = nsplitqty;
                }
                else if (action == "2") {
                    v.npdiscount = npdiscount;
                    $scope.ChangePrice(v);
                }
            });
        };


        $scope.ChoseReport = function (requestitem) {
            var ids = [];
            $.each(requestitem.filterbarcode, function (k, v) {
                ids.push({
                    'barcode': v.code,
                    'stockid': v.id,
                    'noofcopy': v.copy
                });
            });
            requestitem = { 'iswarehouse': ($scope.SearchCondition.mode == 1), 'locationid': $scope.SearchCondition.locationid, 'barcodes': ids, 'reportid': 0, 'companytitle': requestitem.companytitle };

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

        $scope.Sent2Printer = function (printdocument, printerData) {
            $rootScope.Sent2Printer(printdocument.join(';'), printerData);
            return;

            $.each(printdocument, function (k, v) {

                $rootScope.Sent2Printer(v, printerData);
                //var request = {
                //    'id': printerData.printerid,
                //    'content': v,
                //    'encoded': 'BASE64'
                //};
                //var srequest = JSON.stringify(request);
                //$http({
                //    method: 'POST',
                //    url: printerData.service,
                //    data: srequest
                //}).success(function (result) {

                //}).error(function (error) {

                //});
            })
        };

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'SPLIT BARCODES HISTORY'
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

        $scope.CurrentBarcodePrinter = null;
        $scope.SelectPrinter = function () {
            $rootScope.Handlers.SelectPrinter('BARCODE', function (printdata) {
                if (printdata == null || printdata.report == null || printdata.report.id <= 0)
                    return;
                localStorageService.set('BillPrinterData', printdata);
                $scope.CurrentBarcodePrinter = printdata;
                $scope.BillPrint = { 'printer': printdata.printerinfo, 'report': printdata.report.name };
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

        $scope.LoadSetting = function () {
            if ($rootScope.UserInfo && $rootScope.UserInfo.appsettings) {
                $scope.system_splitbitprice = $rootScope.UserInfo.appsettings.system_splitbitprice || false;
                $scope.bitprice = $scope.system_splitbitprice;
                if ($rootScope.LocationInfo && $rootScope.LocationInfo.location) {
                    $scope.SearchCondition.locationid = $rootScope.LocationInfo.location.id;
                    $scope.SupportRounding = $rootScope.UserInfo.appsettings.system_supportrounding || false;
                }
            }
            else {
                $timeout($scope.LoadSetting, 20);
            }
        }

        $timeout(function () {
            $scope.LoadSetting();
            $scope.setFocustxtbarcode();
            var printerData = localStorageService.get('BillPrinterData');
            if (typeof (printerData) != 'undefined' && printerData != null) {
                $scope.CurrentBarcodePrinter = printerData;
                $scope.BillPrint = { 'printer': printerData.printerinfo, 'report': printerData.report.name };
            }
        }, 10);

    }]);