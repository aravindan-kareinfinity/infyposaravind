
var app = angular.module('InfinityLab');

app.controller('PackBarcodeController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', 'localStorageService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, localStorageService) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.BarcodeList = [];
        $scope.ItemList = [];
        $scope.SearchResult = [];
        $scope.SearchCondition = {
            "id": 0,
            "mode": 1,
            "locationid": 0,
            "barcode": ""
        };

        $scope.nsplitqty = 0;
        $scope.npdiscount = 0;

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });

        $scope.Location = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Location = results;
        });

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Clear();
        }

        $scope.SearchHistory = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PackbarcodeService/SearchHistory",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.PackBarcodeConfig();
            }).error(function (result) {
            });
        }
        $scope.PackBarcodeConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('locationname', 'Location', 100);
            igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            igrid.AddSimpleColumn('productname', 'Product Name', 120);
            igrid.AddFormatedColumn('price', 'Price', 80, 'N2', 2);
            igrid.AddFormatedColumn('qty', 'Qty', 60, 'N2', 2);
            igrid.AddSimpleColumn('sourcebarcode', 'Source Barcode', 100);
            igrid.AddSimpleColumn('sourceproductname', 'Source Product', 120);
            igrid.AddFormatedColumn('sourceqty', 'Source Qty', 80, 'N2', 2);
            igrid.AddFormatedColumn('sourcebuyingprice', 'Source Cost', 80, 'N2', 2);
            igrid.AddFormatedColumn('sourcesellingprice', 'Source Selling Price', 80, 'N2', 2);
            igrid.AddFormatedColumn('createdon', 'Created On', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('createdby', 'Created By', 100);

            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Print'/> &nbsp;"

            igrid.AddButtonColumn('action', 'ACTION', 60, commands, function (e, args) {
                $scope.$apply(function () {
                    $scope.PrintSelected(args.item);
                });
            });

            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult, '_id');

        }

        $scope.Export = function () {
            if (typeof ($scope.igrid) == 'undefined') return;
            $scope.exportHref = $scope.igrid.ExportExcel($scope.igrid.grid, 'Pack barcode List');
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
        $scope.PrintSelected = function (item) {
            if ($scope.CurrentBarcodePrinter == null) {
                $rootScope.Handlers.SelectPrinter('BARCODE', function (printdata) {
                    if (printdata == null || printdata.report == null || printdata.report.id <= 0)
                        return;
                    localStorageService.set('PackBarcodePrinterData', printdata);
                    $scope.CurrentBarcodePrinter = printdata;
                });
            }
            var ids = [];
            ids.push({ 'noofcopy': item.qty, 'barcode': item.barcode });

            requestitem = {
                'iswarehouse': false,
                'locationid': item.locationid,
                'barcodes': ids,
                'reportid': 0
            };

            if ($scope.CurrentBarcodePrinter == null) return;

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
                }
            });

        }

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
                        $scope.ItemList.push(result);
                        $scope.SearchCondition.barcode = '';
                        $scope.QtyChanged();
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
                'discount': $scope.SearchCondition.discount, 'displaymrp': $scope.SearchCondition.displaymrp, 'sourcelist': [], 'packlist': []};

            var haslocation = typeof ($scope.SearchCondition.locationid) != 'undefined' && $scope.SearchCondition.locationid != null && $scope.SearchCondition.locationid > 0;
            $.each($scope.ItemList, function (k, v) {
                packcode.sourcelist.push({
                    'barcode': v.barcode, 'productid': v.productid,
                    'qty': v.qty, 'id': haslocation ? v.salablegoodsid : v.stockid,
                    'salablegoodsid': v.salablegoodsid,
                    'stockid': v.stockid
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
                url: "PackbarcodeService/CreatePackcode",
                data: srequest
            }).success(function (result) {
                $scope.BarcodeList = result.barcodes;
            }).error(function (result) {
            });
        }

        $scope.SearchPackCode = function (searchbarcode) {
            var request = {
                "Item": { 'barcode': searchbarcode, sourcesearch :true },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PackbarcodeService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0)
                    $scope.BarcodeList = result[0].barcodes;
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
                if (v.selected) {
                    ids.push({
                        'barcode': v.barcode,
                        'noofcopy': v.qty
                    });
                }
            });

            requestitem = { 'iswarehouse': ($scope.SearchCondition.mode == 1), 'locationid': $scope.SearchCondition.locationid, 'barcodes': ids, 'reportid': 0};
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

        $scope.NewPrintBarcode = function (item, isdirect, lrinvoiceitemid) {

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
            BarcodeList.push({ 'id': item.stockid, 'productid': item.productid, 'qty': item.splitqty, 'code': item.barcodes, 'name': item.productname, 'sno': 0 });

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
                        'productid': v.productid, 'productname': v.productname, 'available': 0 , 'qty' : 1
                    }
                    $scope.PackItemList.push(packitem);
                }
                packitem.available += parseFloat(v.qty);
            });

            var packexpected = Math.round($scope.PackItemList[0].available / $scope.PackItemList[0].qty,0);
            $.each($scope.PackItemList, function (k, v) {
                //if ((v.available / v.qty) != packexpected)
                //    $scope.CanGenerate = false;
            });

            $scope.NoOfPacks = packexpected;
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
                $scope.SearchCondition.discount = final - price;
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
            var printerData = localStorageService.get('PackBarcodePrinterData');
            if (typeof (printerData) != 'undefined' && printerData != null) {
                $scope.CurrentBarcodePrinter = printerData;
                $scope.BillPrint = { 'printer': printerData.printerinfo, 'report': printerData.report.name };
            }
        }, 10);

    }]);