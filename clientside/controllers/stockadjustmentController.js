
var app = angular.module('InfinityLab');

app.controller('StockAdjustmentController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', '$filter', 'restService', 'ngDialog', 'localStorageService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, $filter, restService, ngDialog, localStorageService, $timeout) {
        $scope.SearchRef = restService.SearchReference;
        $scope.status = '';
        $scope.system_invcol_pricetag = false;

        $scope.Supplier = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Supplier = results;
        });

        $scope.Type = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.Type = results;
        });

        //$scope.BarcodeFiles = restService.Get('ReferencelistService/BARCODEFILE', true).then(function (results) {
        //    $scope.BarcodeFiles = results;
        //});

        $scope.Color = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.Color = results;
        });

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
            $rootScope.BindCompany($scope, 'SearchCondition.companyid');
            $rootScope.BindCompany($scope, 'FullStockInfo.companyid');
        });

        $scope.Location = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Location = results;
            $rootScope.BindLocation($scope, 'SearchCondition.locationid');
            $rootScope.BindLocation($scope, 'FullStockInfo.locationid');
        });
        $scope.Product = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Product = results;
        });

        $scope.Sections = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Sections = results;
        });

        $scope.Brand = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brand = results;
        });
        $scope.SearchSources = [{ id: '1', name: 'Warehouse' },
        { id: '2', name: 'Retail Store' },
        { id: '3', name: 'Full' },
        { id: '4', name: 'HSN' },
        { id: '5', name: 'File' }];

        $scope.BarcodeType = [{ id: '1', name: 'All' },
        { id: '2', name: 'Quanto Barcode' },
        { id: '3', name: 'UAN Barcode' },
        { id: '4', name: 'Designs' },
        { id: '5', name: 'DCode/Size' }];

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
        $scope.Sleeve = restService.Get('ReferencelistService/SLEEVE', true).then(function (results) {
            $scope.Sleeve = results;
        });
        $scope.Fit = restService.Get('ReferencelistService/FIT', true).then(function (results) {
            $scope.Fit = results;
        });
        $scope.PriceTags = restService.Get('ReferencelistService/PRICETAGS', true).then(function (results) {
            $scope.PriceTags = results;
        });

        $scope.Entity = {
            "id": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": "",
            "stockitems": [],
        };

        $scope.Journal = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.selectall = function () {
            $.each($scope.StockResult, function (k, v) {
                v.selected = $scope.isselectall;
            });
        }

        $scope.FullStockInfo = {};
        $scope.uploadfullstockfilecomplete = function (value) {
            value.companyid = $scope.FullStockInfo.companyid;
            value.locationid = $scope.FullStockInfo.locationid;
            $scope.FullStockInfo = value;
            $scope.uploadfullstockfile = null;
            //localStorageService.set('StockAdjust.FullStock', value);
        }

        $scope.Clear = function () {
            $scope.status = '';
            $scope.AvailableStock = 0;
            $scope.SyncStock = 0;
            $scope.UploadedStock = 0;
            $scope.SyncMissingStock = 0;
            $scope.StockResult = [];
            $scope.igrid.UpdateData($scope.StockResult);
        }

        $scope.SynchronizeStockComformation = function (fullstock) {
            $.confirm({
                title: 'SYNCHRONIZATION',
                content: 'Do you want to perform the stock synchronization? The changes will be applied immediately!!',
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
                            if (typeof (fullstock) != 'undefined' && fullstock)
                                $scope.SynchronizeFullStock();
                            else
                                $scope.SynchronizeStock();
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

        $scope.SynchronizeStock = function () {


            $scope.status = "Sync Started.";
            var request = {
                'mode': $scope.SearchCondition.mode,
                'companyid': $scope.SearchCondition.companyid,
                'locationid': $scope.SearchCondition.locationid,
                'reason': $scope.SearchCondition.reason,
                'items': [],
                'uanitems': []
            };

            var seletedRows = this.igrid.GetSelectedRows();
            if ($scope.addremovestock) {
                $.each(seletedRows, function (k, v) {
                    if ((v.salablegoodsid > 0 || v.stockid > 0)) {
                        var astock = $scope.GetNumber(v.astock);
                        var rstock = $scope.GetNumber(v.rstock);
                        if (astock > 0) {
                            if (v.uanstk && v.uanstk != null && v.uanstk.length > 0) {
                                $.each(v.uanstk, function (uk, uv) {
                                    if (astock > 0) {
                                        if (uv.iqty - uv.oqty > 0) {
                                            if (uv.iqty - uv.oqty >= astock) {
                                                request.items.push({
                                                    'stockid': uv.stockid, 'salablegoodsid': uv.salablegoodsid,
                                                    'stock': astock+uv.qty
                                                });
                                                astock = 0;
                                            }
                                            else {
                                                request.items.push({
                                                    'stockid': uv.stockid, 'salablegoodsid': uv.salablegoodsid,
                                                    'stock': (uv.iqty - uv.oqty)+uv.qty
                                                });
                                                astock -= (uv.iqty - uv.oqty);
                                            }
                                        }
                                    }
                                })
                                if (astock > 0) {
                                    request.uanitems.push({ 'stockid': v.stockid, 'barcode': v.serialno, 'salablegoodsid': v.salablegoodsid, 'stock': astock });
                                }
                            }
                            else {
                                v.stock = $scope.GetNumber(v.qty) + astock;
                                request.items.push({ 'stockid': v.stockid, 'salablegoodsid': v.salablegoodsid, 'stock': v.stock });
                            }
                        }
                        else if (rstock > 0) {
                            if (v.uanstk && v.uanstk != null && v.uanstk.length > 0) {
                                $.each(v.uanstk, function (uk, uv) {
                                    if (rstock > 0) {
                                        if ($scope.GetNumber(uv.qty) > 0) {
                                            if ($scope.GetNumber(uv.qty) >= rstock) {
                                                uv.qty = $scope.GetNumber(uv.qty) - rstock;
                                                request.items.push({ 'stockid': uv.stockid, 'salablegoodsid': uv.salablegoodsid, 'stock': uv.qty });
                                                rstock = 0;
                                            }
                                            else {
                                                request.items.push({ 'stockid': uv.stockid, 'salablegoodsid': uv.salablegoodsid, 'stock': 0 });
                                                rstock -= (uv.qty);
                                            }
                                        }
                                    }
                                })
                                if (rstock > 0) {
                                    request.uanitems.push({
                                        'stockid': v.stockid, 'barcode': v.serialno, 'salablegoodsid': v.salablegoodsid,
                                        'stock': rstock * -1
                                    });
                                }
                            }
                            else {
                                v.stock = $scope.GetNumber(v.qty) - rstock;
                                request.items.push({ 'stockid': v.stockid, 'salablegoodsid': v.salablegoodsid, 'stock': v.stock });
                            }
                        }
                    }
                });
            }
            else {
                $.each(seletedRows, function (k, v) {
                    if ((v.salablegoodsid > 0 || v.stockid > 0) && $scope.GetNumber(v.qty) != $scope.GetNumber(v.stock)) {
                        if (v.uanstk && v.uanstk != null && v.uanstk.length > 1) {
                            var newstock = $scope.GetNumber(v.stock);
                            var oldstock = $scope.GetNumber(v.qty);
                            if (newstock > oldstock) {
                                var astock = newstock - oldstock;
                                if (v.uanstk && v.uanstk != null && v.uanstk.length > 0) {
                                    $.each(v.uanstk, function (uk, uv) {
                                        if (astock > 0) {
                                            if (uv.iqty - uv.oqty > 0) {
                                                if (uv.iqty - uv.oqty >= astock) {
                                                    request.items.push({ 'stockid': uv.stockid, 'salablegoodsid': uv.salablegoodsid, 'stock': uv.qty+astock });
                                                    astock = 0;
                                                }
                                                else {
                                                    request.items.push({ 'stockid': uv.stockid, 'salablegoodsid': uv.salablegoodsid, 'stock': (uv.iqty - uv.oqty)+uv.qty });
                                                    astock -= (uv.iqty - uv.oqty);
                                                }
                                            }
                                        }
                                    })
                                    if (astock > 0) {
                                        request.uanitems.push({ 'stockid': v.stockid, 'barcode':v.serialno, 'salablegoodsid': v.salablegoodsid, 'stock': astock });
                                    }
                                }
                                else {
                                    v.stock = $scope.GetNumber(v.qty) + astock;
                                    request.items.push({ 'stockid': v.stockid, 'salablegoodsid': v.salablegoodsid, 'stock': v.stock });
                                }
                            } else if (newstock < oldstock) {
                                var rstock = oldstock - newstock;
                                if (v.uanstk && v.uanstk != null && v.uanstk.length > 0) {
                                    $.each(v.uanstk, function (uk, uv) {
                                        if (rstock > 0) {
                                            if ($scope.GetNumber(uv.qty) > 0) {
                                                if ($scope.GetNumber(uv.qty) >= rstock) {
                                                    uv.stock = $scope.GetNumber(uv.qty) - rstock;
                                                    request.items.push({ 'stockid': uv.stockid, 'salablegoodsid': uv.salablegoodsid, 'stock': uv.stock });
                                                    rstock = 0;
                                                }
                                                else {
                                                    request.items.push({ 'stockid': uv.stockid, 'salablegoodsid': uv.salablegoodsid, 'stock': 0 });
                                                    rstock -= (uv.qty);
                                                }
                                            }
                                        }
                                    })
                                    if (rstock > 0) {
                                        request.uanitems.push({ 'stockid': v.stockid, 'barcode': v.serialno, 'salablegoodsid': v.salablegoodsid, 'stock': rstock*-1 });
                                    }
                                }
                                else {
                                    v.stock = $scope.GetNumber(v.qty) - rstock;
                                    request.items.push({ 'stockid': v.stockid, 'salablegoodsid': v.salablegoodsid, 'stock': v.stock });
                                }
                            }
                        }
                        else {
                            request.items.push({ 'stockid': v.stockid, 'salablegoodsid': v.salablegoodsid, 'stock': v.stock });
                        }
                    }
                });
            }
            var webrequest = {
                "item": request,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(webrequest);
            $http({
                method: 'POST',
                url: "StockService/SyncStock",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Sync Completed.");
                $scope.status = "Sync Completed.";
            }).error(function (result) {
            });
        }

        $scope.SearchforSync = function () {

            if (typeof ($scope.SearchCondition.barcode) != 'undefined' && $scope.SearchCondition.barcode != null && $scope.SearchCondition.barcode.length > 0) {
                $scope.SearchBarcode();
                return;
            }
            $scope.StockResult = [];
            $scope.status = 'Searching stock for synchronizing.'
            $scope.SearchStock($scope.SearchCondition, function (result) {
                if (typeof ($scope.StockResult) == 'undefined' || $scope.StockResult == null) {
                    $scope.StockResult = result;
                }
                else {
                    $.each(result, function (k, v) {
                        $scope.StockResult.push(v);
                    })
                }
                $scope.status = 'Please upload excel file to sync';
                $scope.AvailableStock = 0;
                $scope.SyncStock = 0;
                $scope.UploadedStock = 0;
                $scope.SyncMissingStock = 0;
                $.each($scope.StockResult, function (k, v) {
                    $scope.AvailableStock += v.qty;
                    if (v.sellingmode == 3)
                        v.nos = 1;
                    else
                        v.nos = v.qty;
                })
                $scope.igrid.UpdateData($scope.StockResult);
            });
        }

        $scope.FileBarcodes = [];

        $scope.CheckBarcode4FileExist = function (barcode) {

            var exist = $scope.FileBarcodes.find(function (item) {
                return item.barcode == barcode || item.uan == barcode;
            });

            if (exist) {
                var product = $scope.Product.find(function (item) {
                    return item.id == exist.productid;
                });

                if (product && product.sellingmode != 3) {
                    $scope.FileBarcodes[0].style = '';
                    exist.qty = exist.qty + 1;
                    $scope.FileBarcodes.splice($scope.FileBarcodes.indexOf(exist), 1);
                    $scope.FileBarcodes.splice(0, 0, exist);
                    if ($scope.FileBarcodes.length == 1) {
                        $scope.ConfigFileGrid();
                    }
                    $scope.igrid.UpdateData($scope.FileBarcodes);
                    $scope.SearchCondition.barcode = '';
                    return true;
                }
            }
            return false;
        }

        $scope.ShowFile = function (allfile) {
            restService.Get('File/ListBarcodeFile/' + $scope.UserInfo.OrganizationID + '/' + allfile + '/' + $scope.SearchCondition.locationid, true).then(function (results) {

                if ($scope.bgrid != null)
                    $scope.bgrid.destroy();

                $scope.BarcodeFiles = results;
                var igrid = new InfyGrid('barcodeContainer', '420px', '100%', true, true, true, 350);
                igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-warning btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                igrid.AddSimpleColumn('name', 'File', 100);
                igrid.AddSimpleColumn('data', 'Created', 100);
                igrid.Initialize();

                igrid.grid.onClick.subscribe(function (e) {
                    var cell = igrid.grid.getCellFromEvent(e);
                    var item = igrid.grid.getDataItem(cell.row);
                    var command = igrid.grid.getColumns()[cell.cell].id;
                    if (command == 'Delete') {
                        $http({
                            method: 'POST',
                            url: "File/DeleteBarcodeFile/" + item.id,
                        }).success(function (result) {
                            $rootScope.Toaster.Info("Deletion Completed.");
                        }).error(function (result) {
                        });
                    }
                    if (command == 'Edit') {
                        $scope.LoadBarcodeFile(item.id);
                    }
                });
                igrid.onSelectedRangesChanged = function (range, rowmodel) {
                    $timeout(function () {
                        $scope.selectedcount = range.length;
                    }, 10);
                };
                $scope.bgrid = igrid;
                $scope.bgrid.UpdateData($scope.BarcodeFiles);
            });
        }

        $scope.LoadBarcodeFile = function (id) {
            restService.Get('File/BarcodeFileData/' + id, true).then(function (results) {
                $scope.CurrentFileID = results.id;
                $scope.FileBarcodes = JSON.parse(results.content);
                $scope.ConfigFileGrid();
            });
        }

        $scope.CurrentFileID = '';
        $scope.Save2File = function () {

            if ($scope.CurrentFileID && $scope.CurrentFileID != null &&
                $scope.CurrentFileID > 0) {
                var file = { 'content': JSON.stringify($scope.FileBarcodes), 'id': $scope.CurrentFileID };
                var srequest = JSON.stringify(file);
                $http({
                    method: 'POST',
                    url: "File/SaveBarcodeFile",
                    data: srequest
                }).success(function (result) {
                    $scope.CurrentFileID = result;
                    $rootScope.Toaster.Info("Barcode updated on server");
                });
            }
            else {
                $rootScope.Prompt('Hold', 'Filename', function (code) {
                    code = code.toString();
                    if (typeof (code) != 'undefined' && code.length > 0) {
                        var file = { 'filename': code, 'content': JSON.stringify($scope.FileBarcodes), 'id': $scope.CurrentFileID };
                        var srequest = JSON.stringify(file);
                        $http({
                            method: 'POST',
                            url: "File/SaveBarcodeFile",
                            data: srequest
                        }).success(function (result) {
                            $scope.CurrentFileID = result;
                            $rootScope.Toaster.Info("Barcode saved on server");
                        });
                    }
                })
            }
        }

        $scope.SearchBarcode4File = function () {
            if ($scope.CheckBarcode4FileExist($scope.SearchCondition.barcode)) {

                return;
            }

            var condition = {
                'mode': $scope.SearchCondition.mode,
                'locationid': $scope.SearchCondition.locationid,
                'companyid': $scope.SearchCondition.companyid,
                'barcode': $scope.SearchCondition.barcode,
                'barcodetype': $scope.SearchCondition.barcodetype
            };

            var request = {
                "item": condition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchStock4File",
                data: srequest
            }).success(function (result) {
                if (result && result != null && result.length > 0) {
                    $scope.SearchCondition.barcode = '';
                    var product = $scope.Product.find(function (item) {
                        return item.id == result[0].productid;
                    });
                    if (product) {
                        result[0].secid = product.salescounterid;
                        if (product.sellingmode == 3) {
                            $rootScope.Prompt('Qty', 'Meter', function (code) {
                                code = code.toString();
                                if (typeof (code) != 'undefined' && code.length > 0) {
                                    result[0].mqty = parseFloat(code);
                                    if (isNaN(result[0].mqty)) result[0].mqty = 0;
                                    $timeout(function () {
                                        $scope.FileBarcodes.splice(0, 0, result[0]);
                                        if ($scope.FileBarcodes.length == 1) {
                                            $scope.ConfigFileGrid();
                                        }
                                        $scope.igrid.UpdateData($scope.FileBarcodes);
                                    }, 20);
                                }
                            })
                        }
                        else {
                            result[0].qty = 1;
                            if ($scope.FileBarcodes.length > 0)
                                $scope.FileBarcodes[0].style = '';
                            $scope.FileBarcodes.splice(0, 0, result[0]);
                            if ($scope.FileBarcodes.length == 1) {
                                $scope.ConfigFileGrid();
                            }
                            $scope.igrid.UpdateData($scope.FileBarcodes);
                        }
                    }
                }
                else {
                    $scope.setFocuscompany5();
                    $rootScope.Toaster.Info("Barcode missing in the system,please enter the product details");
                }
            });
        }

        $scope.CreateBarcode = function () {
            $scope.SearchCondition.locationid = 0;
            var request = {
                "Items": [$scope.SearchCondition],
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "StockService/MissingStock",
                data: srequest
            }).success(function (result) {
                $scope.SearchBarcode4File();
            });
        }

        $scope.SearchBarcode = function () {
            $scope.SearchStock({
                'mode': $scope.SearchCondition.mode,
                'locationid': $scope.SearchCondition.locationid,
                'companyid': $scope.SearchCondition.companyid,
                'barcode': $scope.SearchCondition.barcode
            }, function (result) {
                if (typeof ($scope.StockResult) == 'undefined' || $scope.StockResult == null) {
                    $scope.StockResult = result;
                }
                else {
                    $.each(result, function (k, v) {
                        var qty = 0;
                        var eitem = $scope.StockResult.find(function (item) {
                            return item.barcode == v.barcode;
                        });
                        if (typeof (eitem) != 'undefined' && eitem != null) {
                            qty = $rootScope.GetNumber(eitem.stock);
                            $scope.StockResult.splice($scope.StockResult.indexOf(eitem), 1);
                        }
                        v.stock = qty + 1;
                        $scope.StockResult.splice(0, 0, v);
                    });
                }
                $scope.SearchCondition.barcode = '';

                $.each($scope.StockResult, function (k, v) {
                    if (v.sellingmode == 3)
                        v.nos = 1;
                    else
                        v.nos = v.qty;
                })

                $scope.igrid.UpdateData($scope.StockResult);
            });
        }
        $scope.GetNumber = function (value) {
            if (typeof (value) == 'number')
                return value;

            if (typeof (value) == 'undefined' || value == '')
                return -9;

            nvalue = parseFloat(value);
            if (isNaN(nvalue))
                return -9;

            return nvalue;
        };

        $scope.SynchronizeFullStock = function () {

            var serviceurl = "FullstocksyncService/SyncFullStock";
            request = {
                "item": $scope.FullStockInfo,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: serviceurl,
                data: srequest
            }).success(function (result) {
                if (result)
                    $rootScope.Toaster.Info("Full stock sync completed successfully.");
                else
                    $rootScope.Toaster.Error("Full stock sync failed, check the trace.");

            }).error(function (result) {
            });

        }


        $scope.SearchFullStockConflict = function () {

            var serviceurl = "FullstocksyncService/SearchFullStockConflict";
            request = {
                "item": $scope.FullStockInfo,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: serviceurl,
                data: srequest
            }).success(function (result) {
                if ($scope.igrid != null)
                    $scope.igrid.destroy();
                if ($scope.mgrid != null)
                    $scope.mgrid.destroy();

                var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 30);
                igrid.options.showHeaderRow = true;
                igrid.AddSimpleColumn('product', 'Product', 100);
                igrid.AddSimpleColumn('barcode', 'Barcode', 100);
                igrid.AddFormatedColumn('currentstock', 'Current Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('physicalstock', 'Physical Qty', 80, 'N2', 2).total = true;

                igrid.AddFormatedColumn('mergeqty', 'Merge Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('removeqty', 'Remove Qty', 80, 'N2', 2).total = true;

                igrid.EnableTotal();
                igrid.Initialize();
                $scope.igrid = igrid;
                $scope.igrid.UpdateData(result);
            });

        }

        $scope.ShowConflict = false;

        $scope.SearchFullStock = function () {
            $scope.ShowConflict = false;
            var serviceurl = "FullstocksyncService/SearchFullStock";
            request = {
                "item": $scope.FullStockInfo,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: serviceurl,
                data: srequest
            }).success(function (result) {

                if ($scope.igrid != null)
                    $scope.igrid.destroy();
                if ($scope.mgrid != null)
                    $scope.mgrid.destroy();

                if (result.errorb2b != null && result.errorb2b.length > 0) {
                    var rows = [];
                    $.each(result.errorb2b, function (k, v) {
                        rows.push({ 'key': v, 'stock': 0 });
                    })
                    if (result.missingb2b != null) {
                        $.each(result.missingb2b, function (k, v) {
                            rows.push(v);
                        })
                    }
                    var mgrid = new InfyGrid('missinggrid', '150px', '100%', true, true, true, 370);
                    mgrid.options.showHeaderRow = true;
                    mgrid.AddSimpleColumn('key', 'Product', 100);
                    mgrid.AddSimpleColumn('stock', 'stock', 100).total = true;
                    mgrid.EnableTotal();
                    mgrid.Initialize();
                    $scope.mgrid = mgrid;
                    $scope.mgrid.UpdateData(rows);
                }
                else {
                    var mgrid = new InfyGrid('missinggrid', '150px', '100%', true, true, true, 370);
                    mgrid.options.showHeaderRow = true;
                    mgrid.AddSimpleColumn('barcode', 'Barcode', 100);
                    mgrid.AddSimpleColumn('qty', 'Qty', 100).total = true;
                    mgrid.EnableTotal();
                    mgrid.Initialize();
                    $scope.mgrid = mgrid;
                    $scope.mgrid.UpdateData(result.missing);
                }

                var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 30);
                igrid.options.showHeaderRow = true;
                igrid.AddSimpleColumn('product', 'Product', 100);
                igrid.AddFormatedColumn('currentstock', 'Current Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('newstock', 'New Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('currentcost', 'Current Cost', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('newcost', 'New Cost', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('currentvalue', 'Current Value', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('newvalue', 'New Value', 80, 'N2', 2).total = true;

                if ($scope.FullStockInfo.remove) {
                    igrid.AddFormatedColumn('remove', 'Remove Qty', 80, 'N2', 2).total = true;
                }

                if ($scope.FullStockInfo.merge) {
                    igrid.AddFormatedColumn('merge', 'Merge Qty', 80, 'N2', 2).total = true;
                }

                if ($scope.FullStockInfo.remove || $scope.FullStockInfo.merge) {
                    var column = igrid.AddCustomColumn('possible', 'Possible', 70, function (item, col) {
                        var possible = item.newstock;
                        if (col.remove)
                            possible -= item.remove;
                        if (col.merge)
                            possible -= item.merge;
                        return $filter('IndianNumber')(possible, 2);
                    });
                    column.getValue = function (item, col) {
                        var possible = item.newstock;
                        if (col.remove)
                            possible -= item.remove;
                        if (col.merge)
                            possible -= item.merge;
                        return possible;
                    }
                    column.total = true;
                    column.remove = $scope.FullStockInfo.remove;
                    column.merge = $scope.FullStockInfo.merge;
                    column.cssClass = 'rightalign';
                }
                igrid.EnableTotal();
                igrid.Initialize();
                $scope.igrid = igrid;
                $scope.igrid.UpdateData(result.summary);

                $scope.ShowConflict = true;

            }).error(function (result) {
            });

        }

        $timeout(function () {
            //var value = localStorageService.get('StockAdjust.FullStock');
            //if (typeof (value) != 'undefined' && value != null) {
            //    $scope.uploadfullstockfilecomplete(value);
            //}

            //var value = localStorageService.get('StockAdjust.Stock');
            //if (typeof (value) != 'undefined' && value != null) {
            //    $scope.fileuploadedcomplete(value);
            //}
        }, 100);


        $scope.HSNList = null;
        $scope.SynchronizeHSN = function () {

            var webrequest = {
                "Items": $scope.HSNList,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(webrequest);
            $http({
                method: 'POST',
                url: "StockService/ChangeHSN",
                data: srequest
            }).success(function (result) {
                if (result && result > 0)
                    $rootScope.Toaster.Info("HSN Code Updations completed successfully.(" + result + ")");
                else
                    $rootScope.Toaster.Info("HSN Code Updations failed.");
            }).error(function (result) {
            });
        }
        $scope.filehsnuploadedcomplete = function (value) {


            $scope.uploadfile = null;
            $scope.status = 'excel file importing.';
            $scope.HSNList = [];

            $.each(value.rows, function (k, v) {
                var barcode = v.values[0];
                var hsn = v.values[1];
                $scope.HSNList.push({ 'barcode': barcode, 'hsn': hsn });
            });


            if ($scope.igrid != null) $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '420px', '100%', true, true, true, 40);

            igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            igrid.AddSimpleColumn('hsn', 'HSN Code', 100);
            igrid.options.showHeaderRow = true;
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.HSNList);

        };

        $scope.fileuploadedcomplete = function (value) {

            $scope.ConfigGrid(false);
            var qtycolumninexcel = false;

            //localStorageService.set('StockAdjust.Stock', value);
            $scope.uploadfile = null;
            $scope.status = 'excel file importing.';
            $scope.SearchCondition.sellingname = $scope.SearchCondition.printingname = $scope.SearchRef($scope.SearchCondition.productid, $scope.Product);
            var condition = {
                mode: $scope.SearchCondition.mode,
                barcodes: [],
                locationid: $scope.SearchCondition.locationid,
                companyid: $scope.SearchCondition.companyid
            };

            $.each($scope.StockResult, function (k, v) {
                v.stock = 0;
                v.reason = 'Uploaded';
            });

            $scope.UploadedStock = value.rows.length;
            $scope.SyncStock = 0;
            $scope.SyncMissingStock = 0;
            var uploadstocklist = [];
            $.each(value.rows, function (k, v) {
                var barcode = v.values[0];
                if (barcode.toLowerCase() == 'barcode' || barcode.toLowerCase() == 'uid'
                    || barcode.toLowerCase() == 'uan' || barcode.toLowerCase() == 'serial'
                    || barcode.toLowerCase() == 'ean')
                    return;
                var sqty = 1;
                if (v.values.length > 1) {
                    sqty = $scope.GetNumber(v.values[1]);
                    qtycolumninexcel = true;
                }
                var exist = uploadstocklist.find(function (item) {
                    return item.barcode == v.values[0];
                });

                if (exist && exist != null)
                    exist.stock += sqty;
                else
                    uploadstocklist.push({ 'barcode': v.values[0], 'stock': sqty });
            });

            $.each(uploadstocklist, function (k, v) {
                var barcode = v.barcode;
                var sqty = v.stock;
                if (barcode != null && barcode != '' && barcode.length > 0) {
                    var stock = $scope.StockResult.find(function (item) {
                        return item.barcode == barcode;
                    });
                    if (typeof (stock) != 'undefinied' && stock != null) {
                        if (sqty != -9)
                            stock.stock = stock.stock + sqty;
                        else
                            stock.stock++;
                        $scope.SyncStock++;
                    }
                    else {

                        stock = $scope.StockResult.find(function (item) {
                            return item.serialno == barcode && item.qty > item.stock;
                        });
                        if (typeof (stock) == 'undefined' || stock == null) {
                            stock = $scope.StockResult.find(function (item) {
                                return item.serialno == barcode;
                            });
                        }
                        if (typeof (stock) != 'undefinied' && stock != null) {
                            if (sqty != -9)
                                stock.stock = stock.stock + sqty;
                            else
                                stock.stock++;
                            $scope.SyncStock++;
                        }
                        else {
                            condition.barcodes.push(barcode)
                        }
                    }
                }
            });


            if (condition.barcodes.length > 0) {
                $scope.status = 'Searching some stock barcode in Sold items';
                $scope.MissingBarodes = condition.barcodes;
                $scope.SearchStock(condition, function (result, condition) {

                    $.each(result, function (k, v) {
                        v.stock = 0;
                        if (v.qty == 0 && v.oqty > 0)
                            v.reason = 'Already Sold';
                        else if (v.qty < 0) {
                            v.reason = 'Negetive Stock';
                        }
                        else if ($scope.SearchCondition.productid != 'undefined'
                            && $scope.SearchCondition.productid != null
                            && $scope.SearchCondition.productid > 0
                            && v.productid != $scope.SearchCondition.productid)
                            v.reason = 'Product Mismatch';
                        else {
                            v.reason = 'Normal';
                        }
                    });

                    $scope.selectedRows = [];
                    var missingbarcode = [];
                    $.each($scope.MissingBarodes, function (k, v) {
                        var barcode = v;
                        var fileresponseitem = uploadstocklist.find(function (item) {
                            return item.barcode == v;
                        });

                        var estock = $scope.StockResult.find(function (item) {
                            return item.barcode == barcode;
                        });

                        if (estock == undefined || estock == null) {
                            estock = $scope.StockResult.find(function (item) {
                                return item.serialno == barcode;
                            });
                        }

                        if (estock && estock != null) {
                            if (qtycolumninexcel) {
                                if ($scope.addremovestock) {
                                    var stval = fileresponseitem.stock;
                                    if (stval > 0)
                                        estock.astock = stval;
                                    else
                                        estock.rstock = Math.abs(stval);
                                }
                                else {
                                    estock.stock = fileresponseitem.stock;
                                }
                            }
                            else if (estock.stock == undefined)
                                estock.stock = 1;
                            else
                                estock.stock++;
                            return;
                        }

                        var stock = result.find(function (item) {
                            return item.barcode == barcode;
                        });

                        if (typeof (stock) == 'undefined' || stock == null) {
                            var stock = result.find(function (item) {
                                return item.serialno == barcode;
                            });
                        }

                        if (typeof (stock) != 'undefinied' && stock != null) {
                            if (qtycolumninexcel) {
                                if (typeof (fileresponseitem) != 'undefined' && fileresponseitem != null) {
                                    if ($scope.addremovestock) {
                                        var stval = fileresponseitem.stock;
                                        if (stval > 0)
                                            stock.astock = stval;
                                        else
                                            stock.rstock = Math.abs(stval);
                                    }
                                    else {
                                        stock.stock = fileresponseitem.stock;
                                    }
                                }
                                else {
                                    stock.stock = 1;
                                }
                                $scope.selectedRows.push($scope.StockResult.length);
                                $scope.StockResult.push(stock);
                            }
                            else {
                                if ($scope.addremovestock) {
                                    stock.astock = 1;
                                }
                                else if (typeof (stock.stock) == 'undefined') {
                                    stock.stock = 1;
                                }
                                else if ($rootScope.GetNumber(fileresponseitem.stock) > 0)
                                    stock.stock = fileresponseitem.stock;
                                else
                                    stock.stock++;
                                $scope.selectedRows.push($scope.StockResult.length);
                                $scope.StockResult.push(stock);
                            }
                        }
                        //else if (condition.mode == "1") {
                        //    result.push({
                        //        'barcode': barcode, 'stock': 1, 'stockid': 0, 'nos': 0, 'qty': 0, 'reason': 'missing'
                        //    });
                        //    $scope.SyncMissingStock++;
                        //    $scope.MissingBarcode4Sync++;
                        //}
                        else {
                            missingbarcode.push(barcode);
                        }
                    });


                    //$scope.selectedRows = [];
                    //$.each(result, function (k, v) {
                    //    v.id = $scope.StockResult.length + 1;
                    //    if ($scope.SearchCondition.sellingname != 'None' && ($scope.SearchCondition.sellingname != v.productname || v.stockid == 0)) {
                    //        $scope.selectedRows.push($scope.StockResult.length);
                    //        v.reason = 'Product Mismatch';
                    //    }
                    //    else {
                    //        v.reason = 'Sold';
                    //    }
                    //    $scope.selectedRows.push($scope.StockResult.length);
                    //    $scope.StockResult.push(v);
                    //});

                    $.each($scope.StockResult, function (k, v) {
                        v.printcopy = v.stock;
                        if (v.sellingmode == 3)
                            v.nos = 1;
                        else
                            v.nos = v.qty;
                    })
                    $scope.igrid.UpdateData($scope.StockResult);
                    $scope.igrid.SetSelectedRows($scope.selectedRows);

                    if (missingbarcode.length > 0 && condition.mode == 2) {

                        var condition = {};
                        condition.barcodes = missingbarcode;
                        condition.mode = "1";
                        condition.loationid = $scope.SearchCondition.locationid;
                        $scope.MissingBarodes = missingbarcode;
                        $scope.status = 'Searching some stock barcode from WH';
                        $scope.SearchStock(condition, function (wresult, condition) {

                            $.each(wresult, function (k, v) {
                                v.stock = 0;
                            });

                            var missingbarcode = [];
                            $.each($scope.MissingBarodes, function (k, v) {
                                var barcode = v;

                                var fileresponseitem = uploadstocklist.find(function (item) {
                                    return item.barcode == v;
                                });

                                var wstock = wresult.find(function (item) {
                                    return item.barcode == barcode;
                                });

                                if (typeof (wstock) == 'undefined' || wstock == null) {
                                    wstock = wresult.find(function (item) {
                                        return item.serialno == barcode;
                                    });
                                }

                                if (typeof (wstock) != 'undefinied' && wstock != null) {
                                    wstock.iswarehouse = true;
                                    if (fileresponseitem && fileresponseitem != null) {
                                        if ($scope.addremovestock) {
                                            var stval = fileresponseitem.stock;
                                            if (stval > 0)
                                                wstock.astock = stval;
                                            else
                                                wstock.rstock = Math.abs(stval);
                                        }
                                        else {
                                            wstock.stock = fileresponseitem.stock;
                                        }
                                    }

                                    if (wstock.sellingmode == 3)
                                        wstock.nos = 1;
                                    else
                                        wstock.nos = wstock.stock;
                                    $scope.SyncMissingStock++;
                                    if (wstock.stockid == 0 && wstock.salablegoodsid == 0)
                                        $scope.MissingBarcode4Sync++;
                                }
                                else {
                                    wresult.push({
                                        'barcode': barcode, 'stock': 1, 'nos': 0, stockid: 0,
                                        salablegoodsid: 0, 'qty': 0
                                    });
                                    $scope.SyncMissingStock++;
                                    $scope.MissingBarcode4Sync++;
                                }
                            });


                            $.each(wresult, function (k, v) {
                                //v.id = $scope.StockResult.length + 1;
                                //if ($scope.SearchCondition.sellingname != v.productname || (v.stockid == 0))
                                v.reason = v.stockid == 0 ? 'missing' : 'warehouse stock';

                                var uanitem = $scope.StockResult.find(function (item) {
                                    return item.serialno == v.barcode;
                                });

                                if (typeof (uanitem) == 'undefined' || uanitem == null) {
                                    if ($scope.addremovestock) {
                                        if (v.astock && v.astock > 0)
                                            $scope.selectedRows.push($scope.StockResult.length);
                                        else if (v.rstock && v.rstock > 0)
                                            $scope.selectedRows.push($scope.StockResult.length);
                                    }
                                    else {
                                        if (v.stock && v.stock > 0)
                                            $scope.selectedRows.push($scope.StockResult.length);
                                    }
                                    $scope.StockResult.push(v);
                                }
                            });


                            //$scope.StockResult.sort(function (a, b) {
                            //    return a.productname + a.barcode == b.productname + b.barcode ? 0 : (a.productname + a.barcode < b.productname + b.barcode ? -1 : 1);
                            //})
                            $scope.status = 'Sync Ready, check and click Sync button.';
                            $scope.igrid.UpdateData($scope.StockResult, 'id');
                            $scope.igrid.SetSelectedRows($scope.selectedRows);

                        });
                    } else {
                        //$scope.StockResult.sort(function (a, b) {
                        //    return a.productname + a.barcode == b.productname + b.barcode ? 0 : (a.productname + a.barcode < b.productname + b.barcode ? -1 : 1);
                        //})
                        $scope.igrid.UpdateData($scope.StockResult);
                        $scope.status = 'Sync Ready, check and click Sync button.';
                    }
                });
            }
            else {
                $scope.StockResult.sort(function (a, b) {
                    return a.productname + a.barcode == b.productname + b.barcode ? 0 : (a.productname + a.barcode < b.productname + b.barcode ? -1 : 1);
                })
                $scope.status = 'Sync Ready, check and click Sync button.';
                $scope.igrid.UpdateData($scope.StockResult);
            }

            return true;
        };

        $scope.SearchCondition = {
            mode: 1,
            fromdate: null,
            todate: null,
            locationid: 0,
            supplierid: 0,
            productid: 0,
            brandid: 0,
            materialid: 0,
            patternid: 0,
            styleid: 0,
            sizeid: 0,
            designid: "",
            reason: "",
            searchmode: '2',
            barcodetype: 2
        };
        $scope.StockResult = [];
        $scope.AvailableStock = 0;
        $scope.UploadedStock = 0;
        $scope.SyncStock = 0;
        $scope.MissingBarcode4Sync = 0;
        $scope.SyncMissingStock = 0;


        $scope.addremovechanged = function () {
            $scope.StockResult = [];
            $scope.ConfigGrid();
            $scope.igrid.UpdateData($scope.StockResult);
        }

        $scope.ShowChangeDialog = function () {

            var dialog = ngDialog.open({
                template: 'ChangeAttributeDialog',
                data: [$scope],
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.parent = $scope.ngDialogData[0];
                    $scope.ChangeStock = function () {
                        $scope.closeThisDialog($scope.parent);
                    };
                    $scope.ChangeCompany = function () {
                        $scope.parent.ChangeCompany();
                        $scope.closeThisDialog(null);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $scope.ChangeStock();
                    }
                }
            });
        }

        $scope.ChangeStock = function () {
            $scope.SearchCondition.sellingname = $scope.SearchCondition.printingname = $scope.SearchRef($scope.SearchCondition.productid, $scope.Product);
            var request = {
                'mode': $scope.SearchCondition.mode,
                'locationid': $scope.SearchCondition.locationid,
                'reason': $scope.SearchCondition.reason,
                'items': [],
                'detail': $scope.SearchCondition
            };

            var seletedRows = this.igrid.GetSelectedRows();
            $.each(seletedRows, function (k, v) {
                request.items.push({ 'stockid': v.stockid, 'salablegoodsid': v.salablegoodsid, 'stock': v.stock, 'barcode': v.barcode });
            });
            var webrequest = {
                "item": request,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(webrequest);
            $http({
                method: 'POST',
                url: "StockService/ChangeStock",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Attribute change completed successfully.");
            }).error(function (result) {
            });
        }

        $scope.ChangeCompany = function () {
            $scope.SearchCondition.sellingname = $scope.SearchCondition.printingname = $scope.SearchRef($scope.SearchCondition.productid, $scope.Product);
            var request = { 'mode': $scope.SearchCondition.mode, 'companyid': $scope.SearchCondition.companyid, 'items': [], 'detail': $scope.SearchCondition };

            var seletedRows = this.igrid.GetSelectedRows();
            $.each(seletedRows, function (k, v) {
                request.items.push({ 'stockid': v.stockid, 'salablegoodsid': v.salablegoodsid, 'stock': v.stock, 'barcode': v.barcode });
            });
            var webrequest = {
                "item": request,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(webrequest);
            $http({
                method: 'POST',
                url: "StockService/ChangeCompany",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Company change completed successfully.");
            }).error(function (result) {
            });
        }

        $scope.RemoveSales = function () {
            $.each($scope.StockResult, function (k, v) {
                if (typeof (v.sold) != 'undefined') {
                    var cst = parseFloat(v.stock);
                    if (isNaN(cst)) cst = 0;
                    cst += v.sold;
                    v.stock = cst;
                    v.sold = 0;
                }
            });

            $scope.ConfigGrid(false);
            $scope.igrid.UpdateData($scope.StockResult);
        }

        $scope.ReduceSales = function () {

            var request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSoldBarcodes",
                data: srequest
            }).success(function (result) {

                $.each($scope.StockResult, function (k, v) {
                    if (typeof (v.sold) == 'undefined')
                        v.sold = 0;
                });
                var missing = 0;
                $.each(result, function (k, v) {
                    var cstock = $scope.StockResult.find(function (item) {
                        return item.barcode == v.barcode;
                    });
                    if (cstock != 'undefined' && cstock != null) {
                        var cst = parseFloat(cstock.stock);
                        if (isNaN(cst)) cst = 0;

                        if (typeof (cstock.sold) != 'undefined')
                            cst += cstock.sold;
                        cst -= v.qty;
                        cstock.stock = cst;
                        cstock.sold = v.qty;
                    }
                    else
                        missing += v.qty
                });
                $rootScope.Toaster.Info("Qty which are sold in-between and are zero stock hence which will not be shown in the list : " + missing)
                $scope.ConfigGrid(true);
                $scope.igrid.UpdateData($scope.StockResult);
            });

        }

        $scope.PrintBarcode = function () {
            var ids = [];
            var _selectindex = 1;
            var seletedRows = this.igrid.GetSelectedRows();
            $.each(seletedRows, function (k, v) {
                if (typeof (v.printcopy) == 'undefined' || v.printcopy == null || v.printcopy == 0) {
                    ids.push({
                        'barcode': v.barcode,
                        'noofcopy': 1,
                        'selectindex': _selectindex++
                    });
                }
                else {
                    ids.push({
                        'barcode': v.barcode,
                        'noofcopy': v.printcopy,
                        'selectindex': _selectindex++
                    });
                }
            });

            if (ids.length == 0) {
                $rootScope.Toaster.Info("Please select barcode to be print.");
            }
            $scope.ChoseReport({ 'iswarehouse': ($scope.SearchCondition.mode == 1), 'locationid': $scope.SearchCondition.locationid, 'barcodes': ids, 'reportid': 0 });
        };

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
        }

        $scope.SearchStock = function (condition, callback) {

            //if ($scope.Appsetting_Company) {
            //    if (typeof ($scope.SearchCondition.companyid) == 'undefined' || $scope.SearchCondition.companyid == null || $scope.SearchCondition.companyid < 0) {
            //        $rootScope.Toaster.Info("Please select the company");
            //        return;
            //    }
            //}

            if (typeof (condition) == 'undefined')
                condition = $scope.SearchCondition;
            $scope.totalqty = 0;
            $scope.amount = 0;
            condition.fromstockaudit = true;
            var serviceurl = "WarehouseReportService/SearchStock";
            if (condition.mode == 2) {
                if ((condition.locationid == 0 || condition.locationid == 'undefined')) {
                    $rootScope.Toaster.Info("Please select location");
                    return;
                }
                //if ((condition.companyid == 0 || typeof (condition.companyid) == 'undefined')) {
                //    $rootScope.Toaster.Info("Please select company");
                //    return;
                //}
                serviceurl = "RetailReportService/SearchStock";
            }
            condition.barcodetype = $scope.SearchCondition.barcodetype;
            request = {
                "item": condition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: serviceurl,
                data: srequest
            }).success(function (result) {
                if (typeof (callback) != 'undefined') {
                    callback(result, condition);
                }
                else {
                    $scope.StockResult = result;
                    $scope.AvailableStock = 0;
                    $scope.MissingBarcode4Sync = 0;
                    $scope.SyncStock = 0;
                    $scope.UploadedStock = 0;
                    $scope.SyncMissingStock = 0;
                    if (result == null || result.length == 0) {
                        $rootScope.Toaster.Info("No Results Found.");
                    }
                    else {
                        $.each(result, function (k, v) {
                            $scope.AvailableStock += v.qty;
                        })
                    }
                    $.each($scope.StockResult, function (k, v) {
                        if (v.sellingmode == 3)
                            v.nos = 1;
                        else
                            v.nos = v.qty;
                    })
                    $scope.igrid.UpdateData($scope.StockResult);
                }
            }).error(function (result) {
            });
        }

        $scope.exporttoExcel = function (infygrid, filename) {
            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, filename);
            //if ($scope.exportHref != null) {
            //    $timeout(function () {
            //        var link = document.createElement('a');
            //        link.download = filename + ".xls";
            //        link.href = $scope.exportHref;
            //        document.body.appendChild(link);
            //        link.click();
            //        link.remove();
            //    }, 100);
            //}
        }

        $scope.Appsetting_Company = false;
        $scope.LoadApplicationSetting = function () {
            if (typeof ($rootScope.UserInfo) != 'undefined' &&
                $rootScope.UserInfo != null &&
                typeof ($rootScope.UserInfo.appsettings) != 'undefined' &&
                $rootScope.UserInfo.appsettings != null) {
                $scope.Appsetting_Company = true;
                $scope.system_invcol_pricetag = $rootScope.UserInfo.appsettings.system_invcol_pricetag || false;
            }
            else {
                $timeout(function () {
                    $scope.LoadApplicationSetting();
                }, 50);
            }
        }
        $scope.LoadApplicationSetting();

        $scope.RemoveItem = function (item) {
            $scope.StockResult.splice($scope.StockResult.indexOf(item), 1);
            $scope.igrid.UpdateData($scope.StockResult);
        }

        $scope.DownloadExcelFile = function (log) {

            var ids = [];
            $.each($scope.bgrid.GetSelectedRows(), function (k, v) {
                ids.push(v.id);
            });

            $rootScope.PostDownload('File/DownloadBarcodeFile', { 'idlist': ids.join('_'), 'log': log },
                'barcode.xlsx', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet');
        }

        $scope.igrid = null;
        $scope.mgrid = null;
        $scope.selectedcount = 0;
        $scope.ConfigFileGrid = function () {

            if ($scope.igrid != null) {
                $scope.igrid.destroy();
                $scope.igrid = null;
            }

            var igrid = new InfyGrid('gridContainer', '420px', '100%', true, true, true, 40);
            igrid.AddButtonColumn('action', 'Action', 70, 'Remove', function (e, args) {
                $scope.$apply(function () {
                    $scope.FileBarcodes.splice($scope.FileBarcodes.indexOf(args.item), 1);
                    $scope.igrid.UpdateData($scope.FileBarcodes);
                });
                return true;
            });

            igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            igrid.AddSimpleColumn('serialno', 'UAN', 100);
            igrid.AddFormatedColumn('qty', 'Qty', 80, 'N2', 2).total = true;
            var stockcolumn = igrid.AddFormatedColumn('mqty', 'Meter', 80, 'N2', 2);
            stockcolumn.editor = Slick.Editors.NumberEditor;
            stockcolumn.total = true;

            igrid.AddReferenceColumn('productid', 'Product', 100, $scope.Product);
            igrid.AddReferenceColumn('secid', 'Section', 100, $scope.Sections);

            igrid.options.showHeaderRow = true;
            igrid.options.autoEdit = true;
            igrid.EnableTotal();
            igrid.Initialize();
            igrid.dataView.getItemMetadata = function (row) {
                if (row == 0) {
                    var rowdata = igrid.grid.getData().getItem(row);
                    rowdata.style = 'igridbigrowswithmark';
                }
                return { 'cssClasses': '' };
            };


            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.FileBarcodes);
        }

        $scope.ConfigGrid = function (soldqty) {

            if ($scope.igrid != null) {
                $scope.igrid.destroy();
                $scope.igrid = null;
            }

            if ($scope.SearchCondition.mode == '5') {
                $scope.ConfigFileGrid();
                return;
            }

            var igrid = new InfyGrid('gridContainer', '420px', '100%', true, true, true, 40);
            igrid.AddButtonColumn('action', 'Action', 70, 'Remove', function (e, args) {
                $scope.$apply(function () {
                    $scope.RemoveItem(args.item);
                });
                return true;
            });

            igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            igrid.AddSimpleColumn('serialno', 'UAN', 100);
            igrid.AddSimpleColumn('productname', 'Product', 100);
            igrid.AddReferenceColumn('sectionid', 'Section', 100, $scope.Sections);
            igrid.AddSimpleColumn('brandname', 'Brand', 100);
            igrid.AddSimpleColumn('sizename', 'Size', 100);
            igrid.AddSimpleColumn('designid', 'Design', 100);
            var mode = $scope.SearchCondition.mode ? $scope.SearchCondition.mode : 1;
            if ($scope.addremovestock && mode <= 2) {
                igrid.AddFormatedColumn('qty', 'Current', 80, 'N2', 2).total = true;
                var stockcolumn = igrid.AddSimpleColumn('astock', 'Add', 80);
                stockcolumn.editor = Slick.Editors.NumberEditor;
                stockcolumn.total = true;
                var stockcolumn = igrid.AddSimpleColumn('rstock', 'Remove', 80);
                stockcolumn.editor = Slick.Editors.NumberEditor;
                stockcolumn.total = true;
            }
            else {
                igrid.AddFormatedColumn('nos', 'Count', 60, 'N2', 2).total = true;
                igrid.AddFormatedColumn('qty', 'Qty', 80, 'N2', 2).total = true;
                var stockcolumn = igrid.AddSimpleColumn('stock', 'Stock', 80);
                stockcolumn.editor = Slick.Editors.NumberEditor;
                stockcolumn.total = true;
                stockcolumn.cssClass = 'rightalign';
            }
            if (soldqty)
                igrid.AddFormatedColumn('sold', 'Sold', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('buyingprice', 'Cost Price', 80, 'N2', 2);
            igrid.AddFormatedColumn('ibuyingprice', 'Net Price', 80, 'N2', 2);
            igrid.AddFormatedColumn('price', 'Price', 80, 'N2', 2);
            igrid.AddFormatedColumn('discount', 'Discount', 80, 'N2', 2);

            var column = igrid.AddCustomColumn('saleprice', 'Sale Price', 80, function (item, col) {
                return $filter('IndianNumber')(item.price - item.discount, 2);
            });

            igrid.AddColumn(
                {
                    id: "printcopy", name: "Print Copy", field: "printcopy",
                    width: 70, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );

            igrid.AddSimpleColumn('suppliername', 'Supplier', 120);
            igrid.AddSimpleColumn('materialname', 'Material', 100);
            igrid.AddSimpleColumn('patternname', 'Pattern', 100);
            igrid.AddSimpleColumn('stylename', 'Style', 100);
            igrid.AddSimpleColumn('colourname', 'Colour', 100);
            igrid.AddSimpleColumn('typename', 'Type', 100);
            igrid.AddSimpleColumn('sleevename', 'Sleeve', 100);
            igrid.AddSimpleColumn('fitname', 'Fit', 100);
            igrid.AddSimpleColumn('pricetagname', 'Price Tag', 100);
            igrid.AddSimpleColumn('designid', 'Design No', 100);
            igrid.AddSimpleColumn('hsncode', 'HSN Code', 100);
            igrid.AddSimpleColumn('reason', 'Reason', 100);
            igrid.AddReferenceColumn('companyid', 'Company', 100, $scope.Company);

            igrid.options.showHeaderRow = true;
            igrid.options.autoEdit = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
        }

        $timeout(function () {
            $scope.ConfigGrid(false);
        }, 50);
    }]);

